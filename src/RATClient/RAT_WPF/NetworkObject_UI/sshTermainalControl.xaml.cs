using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        //KI prompt: mach syntaxhilighting so krass wie möglich mit der richtextbox
        private void AppendTerminalText(string text)
        {
            text = CleanTerminalText(text);

            string[] lines = text.Split('\n');

            foreach (string rawLine in lines)
            {
                string line = rawLine.Replace("\r", "");

                Paragraph paragraph = new Paragraph()
                {
                    Margin = new Thickness(0)
                };

                // LINUX PROMPT

                if (line.Contains("@") && line.Contains("$"))
                {
                    int dollarIndex = line.IndexOf('$');

                    if (dollarIndex > 0)
                    {
                        string prompt = line.Substring(0, dollarIndex + 1);
                        string command = "";

                        if (line.Length > dollarIndex + 1)
                        {
                            command = line.Substring(dollarIndex + 1);
                        }

                        Run promptRun = new Run(prompt)
                        {
                            Foreground = Brushes.LightGreen,
                            FontWeight = FontWeights.Bold
                        };

                        Run commandRun = new Run(command)
                        {
                            Foreground = Brushes.White
                        };

                        paragraph.Inlines.Add(promptRun);
                        paragraph.Inlines.Add(commandRun);
                    }
                }

                // ERROR

                else if (
                    line.Contains("not found") ||
                    line.Contains("error") ||
                    line.Contains("failed"))
                {
                    paragraph.Inlines.Add(new Run(line)
                    {
                        Foreground = Brushes.IndianRed
                    });
                }

                // LS OUTPUT STYLE

                else
                {
                    string[] parts = line.Split(' ');

                    foreach (string part in parts)
                    {
                        Run run = new Run(part + " ");

                        // folders

                        if (!part.Contains(".") &&
                            part.ToLower() != part.ToUpper())
                        {
                            run.Foreground = Brushes.DeepSkyBlue;
                        }

                        // shell scripts / executables

                        if (
                            part.EndsWith(".sh") ||
                            part.StartsWith("./"))
                        {
                            run.Foreground = Brushes.LightGreen;
                        }

                        // python

                        if (part.EndsWith(".py"))
                        {
                            run.Foreground = Brushes.Gold;
                        }

                        paragraph.Inlines.Add(run);
                    }
                }

                TerminalBox.Document.Blocks.Add(paragraph);
            }

            TerminalBox.ScrollToEnd();
        }

        private static string CleanTerminalText(string text)
        {
            return Regex.Replace(
                text,
                @"\x1B(?:[@-Z\\-_]|\[[0-?]*[ -/]*[@-~])",
                "");
        }

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
        //KI END
    }
}
