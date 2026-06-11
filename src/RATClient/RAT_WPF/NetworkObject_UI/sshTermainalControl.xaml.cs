using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace RAT_WPF.NetworkObject_UI
{
    /// <summary>
    /// Interaktionslogik für sshTermainalControl.xaml
    /// </summary>
    public partial class sshTermainalControl : UserControl
    {
        private RAT_Logic.NetworkObject networkObject;

        private int shellId;

        private List<string> commandHistory = new List<string>();

        private int historyIndex = -1;

        public sshTermainalControl(RAT_Logic.NetworkObject networkObject_, int shellID_)
        {
            InitializeComponent();
            networkObject = networkObject_;
            shellId = shellID_;
            TitleText.Text = $"ssh — shell #{shellId}";
            AttachShell(shellId);
        }

        //KI
        private void AttachShell(int id)
        {
            _ = networkObject.StartReadingAsync((text) =>
            {
                Dispatcher.Invoke(() =>
                {
                    AppendTerminalText(text);
                });

            }, shellId);
        }

        //KI start (Claude Opus 4.8, prompt 1/13): tokenizing syntax highlighter for the shell stream.
        // Resolves theme brushes so colors follow the active theme; tokenizes each line into
        // prompt / command / flags / strings / numbers / IPs / env-vars / file-types / log levels.

        private Brush B(string key) => (Brush)Application.Current.Resources[key];

        // a token kind -> brush, resolved lazily per-append so theme switches are honoured
        private Brush Term(string token) => B("Term." + token);

        // matches a bash-style prompt:  user@host:/path$   (or # for root)
        private static readonly Regex PromptRegex =
            new(@"^(?<user>[\w.\-]+)@(?<host>[\w.\-]+)(:(?<path>[^\$#]*))?(?<sym>[\$#])\s?(?<rest>.*)$");

        private static readonly Regex Ipv4Regex =
            new(@"\b\d{1,3}(\.\d{1,3}){3}(:\d+)?\b");

        private static readonly Regex PermsRegex =
            new(@"^[\-dlbcps][rwxsStT\-]{9}[+@.]?$");

        private void AppendTerminalText(string text)
        {
            text = CleanTerminalText(text);

            foreach (string rawLine in text.Split('\n'))
            {
                string line = rawLine.Replace("\r", "");
                Paragraph paragraph = new Paragraph { Margin = new Thickness(0) };

                Match prompt = PromptRegex.Match(line);
                string lower = line.ToLowerInvariant();

                if (prompt.Success)
                {
                    HighlightPromptLine(paragraph, prompt);
                }
                else if (LooksLikeError(lower))
                {
                    paragraph.Inlines.Add(Styled(line, Term("Error")));
                }
                else if (LooksLikeWarning(lower))
                {
                    paragraph.Inlines.Add(Styled(line, Term("Warn")));
                }
                else if (LooksLikeSuccess(lower))
                {
                    paragraph.Inlines.Add(Styled(line, Term("Success")));
                }
                else
                {
                    HighlightOutputLine(paragraph, line);
                }

                TerminalBox.Document.Blocks.Add(paragraph);
            }

            TerminalBox.ScrollToEnd();
        }

        private void HighlightPromptLine(Paragraph p, Match prompt)
        {
            p.Inlines.Add(Styled(prompt.Groups["user"].Value, Term("User"), bold: true));
            p.Inlines.Add(Styled("@", Term("Muted")));
            p.Inlines.Add(Styled(prompt.Groups["host"].Value, Term("Host"), bold: true));

            if (prompt.Groups["path"].Success && prompt.Groups["path"].Value.Length > 0)
            {
                p.Inlines.Add(Styled(":", Term("Muted")));
                p.Inlines.Add(Styled(prompt.Groups["path"].Value, Term("Path")));
            }

            p.Inlines.Add(Styled(prompt.Groups["sym"].Value + " ", Term("Prompt"), bold: true));

            // the rest of the prompt line is the typed command -> highlight it as a command
            string rest = prompt.Groups["rest"].Value;
            if (!string.IsNullOrEmpty(rest))
            {
                HighlightCommand(p, rest);
            }
        }

        // command line: first word = command, then flags / strings / numbers / paths / env vars
        private void HighlightCommand(Paragraph p, string command)
        {
            bool first = true;
            foreach (string tok in SplitKeepingSpaces(command))
            {
                if (tok.Trim().Length == 0) { p.Inlines.Add(new Run(tok)); continue; }

                if (first)
                {
                    p.Inlines.Add(Styled(tok, Term("Exec"), bold: true)); // the command itself
                    first = false;
                }
                else
                {
                    p.Inlines.Add(TokenRun(tok));
                }
            }
        }

        // generic output line: color file-like tokens, perms, IPs, numbers, strings
        private void HighlightOutputLine(Paragraph p, string line)
        {
            foreach (string tok in SplitKeepingSpaces(line))
            {
                if (tok.Trim().Length == 0) { p.Inlines.Add(new Run(tok)); continue; }
                p.Inlines.Add(TokenRun(tok));
            }
        }

        // classify a single token and return a colored Run
        private Run TokenRun(string token)
        {
            string t = token.Trim();

            // quoted string
            if ((t.StartsWith("\"") && t.EndsWith("\"") && t.Length > 1) ||
                (t.StartsWith("'") && t.EndsWith("'") && t.Length > 1))
            {
                return Styled(token, Term("String"));
            }

            // long / short flags:  --verbose  -la
            if (t.StartsWith("--") || (t.StartsWith("-") && t.Length > 1 && char.IsLetter(t[1])))
            {
                return Styled(token, Term("Flag"));
            }

            // env var / variable reference
            if (t.StartsWith("$"))
            {
                return Styled(token, Term("Number"));
            }

            // unix permission string e.g. drwxr-xr-x
            if (PermsRegex.IsMatch(t))
            {
                return Styled(token, Term("Flag"));
            }

            // IPv4 (optionally with port)
            if (Ipv4Regex.IsMatch(t))
            {
                return Styled(token, Term("Host"));
            }

            // pure number / size
            if (Regex.IsMatch(t, @"^\d[\d.,]*[kKmMgGtT]?[bB]?$"))
            {
                return Styled(token, Term("Number"));
            }

            // file-type by extension / shape
            Brush? byKind = ClassifyFile(t);
            if (byKind != null)
            {
                return Styled(token, byKind);
            }

            return Styled(token, Term("Text"));
        }

        private Brush? ClassifyFile(string t)
        {
            // directory: ends with '/' or has no dot and isn't all-caps (heuristic from the old code, refined)
            if (t.EndsWith("/")) { return Term("Dir"); }

            string ext = GetExt(t);

            if (ext is "sh" or "bash" or "zsh" or "bin" or "run" || t.StartsWith("./"))
            {
                return Term("Exec");
            }
            if (ext is "zip" or "tar" or "gz" or "tgz" or "bz2" or "xz" or "7z" or "rar")
            {
                return Term("Archive");
            }
            if (ext is "png" or "jpg" or "jpeg" or "gif" or "bmp" or "svg" or "ico" or "webp")
            {
                return Term("Image");
            }
            if (ext is "py" or "js" or "ts" or "c" or "cpp" or "h" or "cs" or "java" or "go" or "rs"
                    or "rb" or "php" or "json" or "xml" or "yml" or "yaml" or "html" or "css" or "md")
            {
                return Term("Code");
            }
            return null;
        }

        private static string GetExt(string t)
        {
            int slash = Math.Max(t.LastIndexOf('/'), t.LastIndexOf('\\'));
            string name = slash >= 0 ? t.Substring(slash + 1) : t;
            int dot = name.LastIndexOf('.');
            return dot > 0 ? name.Substring(dot + 1).ToLowerInvariant() : "";
        }

        // split a line into tokens but keep the whitespace runs so spacing is preserved
        private static IEnumerable<string> SplitKeepingSpaces(string line)
        {
            return Regex.Matches(line, @"\s+|[^\s]+").Select(m => m.Value);
        }

        private Run Styled(string text, Brush brush, bool bold = false)
        {
            Run run = new Run(text) { Foreground = brush };
            if (bold) { run.FontWeight = FontWeights.Bold; }
            return run;
        }

        private static bool LooksLikeError(string lower) =>
            Regex.IsMatch(lower, @"\b(error|errno|not found|no such|permission denied|failed|fatal|cannot|denied|segfault|traceback|exception)\b");

        private static bool LooksLikeWarning(string lower) =>
            Regex.IsMatch(lower, @"\b(warning|warn|deprecated|notice)\b");

        private static bool LooksLikeSuccess(string lower) =>
            Regex.IsMatch(lower, @"\b(success|successful|succeeded|done|ok|complete|completed|installed|up to date|active \(running\))\b");

        private static string CleanTerminalText(string text)
        {
            return Regex.Replace(
                text,
                @"\x1B(?:[@-Z\\-_]|\[[0-?]*[ -/]*[@-~])",
                "");
        }
        //KI end

        private void CommandInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string cmd = CommandInput.Text;

                commandHistory.Add(cmd);

                historyIndex = commandHistory.Count;

                networkObject.SendCommand(cmd, shellId);

                CommandInput.Clear();

                e.Handled = true;
            }

            // UP

            else if (e.Key == Key.Up)
            {
                if (commandHistory.Count == 0)
                    return;

                historyIndex--;

                if (historyIndex < 0)
                    historyIndex = 0;

                CommandInput.Text = commandHistory[historyIndex];

                CommandInput.CaretIndex =
                    CommandInput.Text.Length;
            }

            // DOWN

            else if (e.Key == Key.Down)
            {
                if (commandHistory.Count == 0)
                    return;

                historyIndex++;

                if (historyIndex >= commandHistory.Count)
                {
                    historyIndex = commandHistory.Count;

                    CommandInput.Clear();

                    return;
                }

                CommandInput.Text = commandHistory[historyIndex];

                CommandInput.CaretIndex =
                    CommandInput.Text.Length;
            }
        }
    }
}
