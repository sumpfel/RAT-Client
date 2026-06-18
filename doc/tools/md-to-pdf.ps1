<#
.SYNOPSIS
  Converts a Markdown file to a styled, self-contained HTML file and then to PDF via headless Edge.
  No external packages required (uses Microsoft Edge's --headless --print-to-pdf).

.USAGE
  powershell -ExecutionPolicy Bypass -File doc/tools/md-to-pdf.ps1 -InputMd doc/markdown/Dokumentation.md -OutPdf doc/pdf/Dokumentation.pdf

  Supports: ATX headings (#..######), fenced code blocks (``` ), tables (| .. |),
  blockquotes (>), ordered/unordered lists, **bold**, *italic*, `inline code`,
  [links](url), horizontal rules (---), and raw <div ...>...</div> passthrough.
#>
param(
    [Parameter(Mandatory=$true)][string]$InputMd,
    [Parameter(Mandatory=$true)][string]$OutPdf
)

$ErrorActionPreference = 'Stop'

function HtmlEncode([string]$s) {
    $s = $s -replace '&','&amp;' -replace '<','&lt;' -replace '>','&gt;'
    return $s
}

# inline markdown -> html (operates on already HTML-encoded text, so code/links are safe)
function Inline([string]$t) {
    $t = HtmlEncode $t
    # inline code first (so * inside code isn't treated as italic)
    $t = [regex]::Replace($t, '`([^`]+)`', { param($m) '<code>' + $m.Groups[1].Value + '</code>' })
    # images ![alt](path) — MUST run before links (an image literally contains a [..](..)).
    # relative paths are resolved against the markdown file's folder and turned into file:// URIs.
    $t = [regex]::Replace($t, '!\[([^\]]*)\]\(([^)]+)\)', {
        param($m)
        $src = $m.Groups[2].Value.Trim()
        if ($src -notmatch '^(https?:|file:|data:)') {
            $full = [System.IO.Path]::GetFullPath((Join-Path $script:baseDir $src))
            if (Test-Path $full) { $src = ([System.Uri]$full).AbsoluteUri }
        }
        '<img src="' + $src + '" alt="' + $m.Groups[1].Value + '"/>'
    })
    # links [text](url)
    $t = [regex]::Replace($t, '\[([^\]]+)\]\(([^)]+)\)', { param($m) '<a href="' + $m.Groups[2].Value + '">' + $m.Groups[1].Value + '</a>' })
    # bold then italic
    $t = [regex]::Replace($t, '\*\*([^*]+)\*\*', '<strong>$1</strong>')
    $t = [regex]::Replace($t, '\*([^*]+)\*', '<em>$1</em>')
    return $t
}

$lines = Get-Content -LiteralPath $InputMd -Encoding UTF8
$script:baseDir = Split-Path -Parent ([System.IO.Path]::GetFullPath($InputMd))
$sb = [System.Text.StringBuilder]::new()
$inCode = $false
$inMermaid = $false
$inList = $false; $listTag = $null
$tableBuf = @()

function Flush-List {
    if ($script:inList) { [void]$script:sb.AppendLine("</$script:listTag>"); $script:inList = $false; $script:listTag = $null }
}
function Flush-Table {
    if ($script:tableBuf.Count -eq 0) { return }
    $rows = $script:tableBuf
    [void]$script:sb.AppendLine('<table>')
    $sep = if ($rows.Count -ge 2) { $rows[1] } else { '' }
    $hasHeader = $sep -match '^\s*\|?\s*:?-{1,}'
    for ($i = 0; $i -lt $rows.Count; $i++) {
        if ($i -eq 1 -and $hasHeader) { continue }
        $cells = ($rows[$i].Trim() -replace '^\|','' -replace '\|\s*$','') -split '\|'
        $cellTag = if ($i -eq 0 -and $hasHeader) { 'th' } else { 'td' }
        [void]$script:sb.Append('<tr>')
        foreach ($c in $cells) { [void]$script:sb.Append("<$cellTag>" + (Inline $c.Trim()) + "</$cellTag>") }
        [void]$script:sb.AppendLine('</tr>')
    }
    [void]$script:sb.AppendLine('</table>')
    $script:tableBuf = @()
}

foreach ($raw in $lines) {
    $line = $raw

    # mermaid fenced block -> <pre class="mermaid"> (rendered client-side by mermaid.js in headless Edge)
    if ($line -match '^\s*```\s*mermaid\s*$' -and -not $inCode -and -not $inMermaid) {
        Flush-List; Flush-Table
        [void]$sb.AppendLine('<pre class="mermaid">')
        $inMermaid = $true
        continue
    }
    if ($inMermaid) {
        if ($line -match '^\s*```\s*$') { [void]$sb.AppendLine('</pre>'); $inMermaid = $false }
        else { [void]$sb.AppendLine((HtmlEncode $line)) }   # raw mermaid source, just HTML-escaped
        continue
    }

    # fenced code blocks
    if ($line -match '^\s*```') {
        Flush-List; Flush-Table
        if (-not $inCode) { [void]$sb.AppendLine('<pre><code>'); $inCode = $true }
        else { [void]$sb.AppendLine('</code></pre>'); $inCode = $false }
        continue
    }
    if ($inCode) { [void]$sb.AppendLine((HtmlEncode $line)); continue }

    # raw HTML passthrough (e.g. <div align="center">)
    if ($line -match '^\s*</?(div|center|br|img|p)\b') { Flush-List; Flush-Table; [void]$sb.AppendLine($line); continue }

    # table rows
    if ($line -match '^\s*\|.*\|\s*$') { Flush-List; $tableBuf += $line; continue }
    else { Flush-Table }

    # horizontal rule
    if ($line -match '^\s*---+\s*$') { Flush-List; [void]$sb.AppendLine('<hr/>'); continue }

    # headings
    if ($line -match '^(#{1,6})\s+(.*)$') {
        Flush-List
        $level = $Matches[1].Length
        [void]$sb.AppendLine("<h$level>" + (Inline $Matches[2].Trim()) + "</h$level>")
        continue
    }

    # blockquote
    if ($line -match '^\s*>\s?(.*)$') {
        Flush-List
        [void]$sb.AppendLine('<blockquote>' + (Inline $Matches[1]) + '</blockquote>')
        continue
    }

    # ordered list
    if ($line -match '^\s*\d+\.\s+(.*)$') {
        if (-not $inList -or $listTag -ne 'ol') { Flush-List; [void]$sb.AppendLine('<ol>'); $inList = $true; $listTag = 'ol' }
        [void]$sb.AppendLine('<li>' + (Inline $Matches[1]) + '</li>')
        continue
    }
    # unordered list (supports nested via leading spaces -> just flat here)
    if ($line -match '^\s*[-*+]\s+(.*)$') {
        if (-not $inList -or $listTag -ne 'ul') { Flush-List; [void]$sb.AppendLine('<ul>'); $inList = $true; $listTag = 'ul' }
        [void]$sb.AppendLine('<li>' + (Inline $Matches[1]) + '</li>')
        continue
    }

    # blank line
    if ($line -match '^\s*$') { Flush-List; [void]$sb.AppendLine(''); continue }

    # paragraph
    Flush-List
    [void]$sb.AppendLine('<p>' + (Inline $line) + '</p>')
}
Flush-List; Flush-Table
if ($inCode) { [void]$sb.AppendLine('</code></pre>') }
if ($inMermaid) { [void]$sb.AppendLine('</pre>') }

$css = @'
<style>
  @page { size: A4; margin: 18mm 16mm; }
  body { font-family: "Segoe UI", Arial, sans-serif; color: #2A1A10; line-height: 1.5; font-size: 11pt; }
  h1 { color: #5A3A28; font-size: 24pt; border-bottom: 3px solid #C97A4A; padding-bottom: 6px; }
  h2 { color: #5A3A28; font-size: 17pt; border-bottom: 1px solid #D9B58A; padding-bottom: 4px; margin-top: 28px; }
  h3 { color: #6B4A3A; font-size: 13pt; margin-top: 20px; }
  h4 { color: #6B4A3A; font-size: 11.5pt; }
  a { color: #B85C2A; text-decoration: none; }
  code { background: #F3E9DC; color: #5A3A28; padding: 1px 4px; border-radius: 3px; font-family: Consolas, monospace; font-size: 9.5pt; }
  pre { background: #2A1A10; color: #F6EFE4; padding: 12px 14px; border-radius: 6px; overflow-x: auto; }
  pre code { background: transparent; color: #F6EFE4; padding: 0; font-size: 9pt; line-height: 1.35; }
  pre.mermaid { background: #FBF4EA; color: #2A1A10; border: 1px solid #D9B58A; text-align: center; page-break-inside: avoid; }
  table { border-collapse: collapse; width: 100%; margin: 12px 0; font-size: 10pt; }
  th, td { border: 1px solid #D9B58A; padding: 6px 9px; text-align: left; vertical-align: top; }
  th { background: #5A3A28; color: #F6EFE4; }
  tr:nth-child(even) td { background: #FBF4EA; }
  blockquote { border-left: 4px solid #C97A4A; margin: 10px 0; padding: 4px 14px; background: #FBF4EA; color: #6B4A3A; }
  hr { border: 0; border-top: 1px solid #D9B58A; margin: 22px 0; }
  ul, ol { margin: 8px 0 8px 4px; }
  li { margin: 3px 0; }
  div[align="center"] { text-align: center; }
  img { max-width: 100%; height: auto; border: 1px solid #D9B58A; border-radius: 6px; margin: 8px 0; display: block; }
</style>
'@

# Mermaid: reference the locally-downloaded library (works offline in headless Edge) and render
# every <pre class="mermaid"> block. PSScriptRoot is doc/tools where mermaid.min.js lives.
$mermaidScript = ""
$mermaidPath = Join-Path $PSScriptRoot "mermaid.min.js"
if (Test-Path $mermaidPath) {
    $mermaidUri = ([System.Uri]((Resolve-Path $mermaidPath).Path)).AbsoluteUri
    $mermaidScript = @"
<script src="$mermaidUri"></script>
<script>
  mermaid.initialize({ startOnLoad: true, theme: 'neutral', securityLevel: 'loose' });
</script>
"@
}

$html = "<!DOCTYPE html><html lang='de'><head><meta charset='utf-8'>$css</head><body>" + $sb.ToString() + $mermaidScript + "</body></html>"

$htmlPath = [System.IO.Path]::ChangeExtension($OutPdf, '.html')
Set-Content -LiteralPath $htmlPath -Value $html -Encoding UTF8
Write-Host "Wrote HTML: $htmlPath"

$edge = @(
    "$env:ProgramFiles\Microsoft\Edge\Application\msedge.exe",
    "${env:ProgramFiles(x86)}\Microsoft\Edge\Application\msedge.exe"
) | Where-Object { Test-Path $_ } | Select-Object -First 1

if (-not $edge) { throw "Microsoft Edge not found; cannot render PDF." }

$uri = ([System.Uri](Resolve-Path $htmlPath).Path).AbsoluteUri
$pdfFull = [System.IO.Path]::GetFullPath($OutPdf)
& $edge --headless=new --disable-gpu --no-pdf-header-footer "--virtual-time-budget=20000" "--print-to-pdf=$pdfFull" $uri | Out-Null
Start-Sleep -Milliseconds 2500
if (Test-Path $pdfFull) { Write-Host "Wrote PDF:  $pdfFull ($([math]::Round((Get-Item $pdfFull).Length/1kb)) KB)" }
else { throw "PDF was not produced." }
