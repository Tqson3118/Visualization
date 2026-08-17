# build-algorithms.ps1 — Sinh backend/seed-data/grokking-algorithms.json từ source riêng.
# (manifest.json + lessons/*.md + quizzes/*.json + assignments/*.json)
# LÝ DO viết thủ công: PowerShell 5.1 ConvertTo-Json có bug — property là mảng Object[]
# (từ ConvertFrom-Json) bị serialize thành {"value":[...],"Count":N} thay vì [...].
# → Build bằng StringBuilder + escape chuỗi qua ConvertTo-Json (chuỗi đơn luôn đúng).
param(
    [string]$SourceDir = "C:\Users\Tam Phuc\Downloads\Visualization-dev\Visualization-dev\backend\seed-data\algorithms",
    [string]$OutJson = "C:\Users\Tam Phuc\Downloads\Visualization-dev\Visualization-dev\backend\seed-data\grokking-algorithms.json"
)

$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

function Read-Utf8([string]$path) {
    [System.IO.File]::ReadAllText($path, [System.Text.Encoding]::UTF8)
}

# Escape chuỗi JSON an toàn (dùng ConvertTo-Json cho 1 string — luôn đúng trên PS5.1)
function Js([string]$s) { ($s | ConvertTo-Json) }

# ── Helper: serial 1 task codelab thành JSON chuỗi (ghép testCases/hints thủ công) ──
function Build-TaskJson($task, $fallbackTitle) {
    $id = [string]$task.id
    $title = [string]$task.title
    $desc = [string]$task.description
    $init = [string]$task.initialCode
    $entry = [string]$task.entryFunction

    $tcArr = @()
    foreach ($tc in $task.testCases) {
        $tcArr += ("        { ""name"": $(Js $tc.name), ""input"": $(Js $tc.input), ""expectedOutput"": $(Js $tc.expectedOutput), ""isHidden"": $(if ([bool]$tc.isHidden) { "true" } else { "false" }) }")
    }
    $hintArr = @()
    foreach ($h in $task.hints) {
        $hintArr += ("          $(Js $h)")
    }

    $json = @"
  {
    "id": $(Js $id),
    "title": $(Js $title),
    "description": $(Js $desc),
    "initialCode": $(Js $init),
    "solution": "",
    "entryFunction": $(Js $entry),
    "hints": [
$($hintArr -join ",`n")
    ],
    "testCases": [
$($tcArr -join ",`n")
    ]
  }
"@
    return (Js $json.Trim())
}

$manifest = Read-Utf8 (Join-Path $SourceDir "manifest.json") | ConvertFrom-Json

# ── Modules ──
$moduleIdByOrder = @{}
$sb = New-Object System.Text.StringBuilder
[void]$sb.AppendLine('{')
[void]$sb.AppendLine('  "id": "grokking-algorithms",')
[void]$sb.AppendLine('  "modules": [')
for ($i = 0; $i -lt $manifest.modules.Count; $i++) {
    $m = $manifest.modules[$i]
    $mid = [guid]::NewGuid().ToString("D").ToUpper()
    $moduleIdByOrder[[int]$m.order] = $mid
    $comma = if ($i -lt $manifest.modules.Count - 1) { "," } else { "" }
    [void]$sb.AppendLine("    { ""id"": $(Js $mid), ""title"": $(Js $m.name), ""description"": $(Js $m.description), ""orderIndex"": $($m.order) }$comma")
}
[void]$sb.AppendLine('  ],')
[void]$sb.AppendLine('  "lessons": [')

# ── Lessons ──
for ($i = 0; $i -lt $manifest.lessons.Count; $i++) {
    $l = $manifest.lessons[$i]
    $contentMd = ""
    if ($l.file) {
        $path = Join-Path $SourceDir ($l.file -replace "/", "\")
        if (Test-Path -LiteralPath $path) {
            $contentMd = Read-Utf8 $path
        } else {
            Write-Warning "Thiếu file nội dung: $($l.file)"
        }
    }

    $sandboxConfig = "null"
    if ($l.sandboxType -eq "quiz") {
        $sandboxConfig = (Js (ConvertTo-Json -InputObject @{ quizId = $l.quizId } -Compress))
        # contentMd mô tả ngắn — file lessons/{quizId}.md (vd q1-concept.md)
        $contentMd = Read-Utf8 (Join-Path $SourceDir ("lessons\" + $l.quizId + ".md"))
    } elseif ($l.sandboxType -eq "codelab") {
        # contentMd = mô tả ngắn gọn (KHÔNG nhét JSON thô vào nội dung bài học)
        $nl = [char]10
        $contentMd = "# " + $l.title + $nl + $nl + "Bài tập lập trình thực hành (Assignment) — mở bài tập để đọc đề, viết code bằng JavaScript và nộp để được chấm tự động theo test ẩn."
        $path = Join-Path $SourceDir ($l.file -replace "/", "\")
        if (Test-Path -LiteralPath $path) {
            $raw = Read-Utf8 $path
            $parsed = $raw | ConvertFrom-Json
            # Serialize task đơn thành chuỗi JSON (ghép testCases/hints thủ công)
            $sandboxConfig = Build-TaskJson $parsed $l.title
        }
    }

    $lessonId = [guid]::NewGuid().ToString("D").ToUpper()
    $orderIndex = ([int]($l.module / 1000) * 1000) + [int]$l.order
    $comma = if ($i -lt $manifest.lessons.Count - 1) { "," } else { "" }
    [void]$sb.AppendLine("    { ""id"": $(Js $lessonId), ""title"": $(Js $l.title), ""xpReward"": $($l.xpReward), ""sandboxType"": $(Js $l.sandboxType), ""sandboxConfig"": $sandboxConfig, ""contentMd"": $(Js $contentMd), ""moduleId"": $(Js $moduleIdByOrder[[int]$l.module]), ""orderIndex"": $orderIndex }$comma")
}
[void]$sb.AppendLine('  ],')
[void]$sb.AppendLine('  "quizzes": [')

# ── Quizzes (build thủ công — tránh bug mảng) ──
$quizMeta = @{}
foreach ($l in $manifest.lessons) {
    if ($l.sandboxType -eq "quiz") { $quizMeta[$l.quizId] = $l.title }
}
$quizFiles = Get-ChildItem (Join-Path $SourceDir "quizzes") -Filter "*.json" | Sort-Object Name
for ($i = 0; $i -lt $quizFiles.Count; $i++) {
    $f = $quizFiles[$i]
    $qid = [System.IO.Path]::GetFileNameWithoutExtension($f.Name)
    $title = if ($quizMeta.ContainsKey($qid)) { $quizMeta[$qid] } else { $qid }
    $questions = Read-Utf8 $f.FullName | ConvertFrom-Json

    # Mỗi câu hỏi: object thuần (options là string) → ConvertTo-Json an toàn
    $qJson = @()
    foreach ($q in $questions) {
        $qJson += ("      { ""id"": $(Js $q.id), ""question"": $(Js $q.question), ""options"": $(Js $q.options), ""correctIndex"": $($q.correctIndex), ""explanation"": $(Js $q.explanation) }")
    }
    $comma = if ($i -lt $quizFiles.Count - 1) { "," } else { "" }
    [void]$sb.AppendLine("    { ""id"": $(Js $qid), ""title"": $(Js $title), ""questions"": [")
    [void]$sb.AppendLine(($qJson -join ",`n"))
    [void]$sb.AppendLine("    ] }$comma")
}
[void]$sb.AppendLine('  ]')
[void]$sb.AppendLine('}')

$json = $sb.ToString()
[System.IO.File]::WriteAllText($OutJson, $json, [System.Text.Encoding]::UTF8)
Write-Host "OK: sinh $OutJson ($($json.Length) ký tự)"
Write-Host "  modules=$($manifest.modules.Count) lessons=$($manifest.lessons.Count) quizzes=$($quizFiles.Count)"