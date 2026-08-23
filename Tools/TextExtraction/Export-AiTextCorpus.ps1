param(
    [string]$OutputPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$projectRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$modRoot = Join-Path $projectRoot 'MihoAdfqs'

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $modRoot 'Docs\AI文本语料库.md'
}

$textElementNames = [System.Collections.Generic.HashSet[string]]::new(
    [string[]]@(
        'label',
        'description',
        'fixedName',
        'leaderTitle',
        'title',
        'titleShort',
        'baseDesc',
        'reportString',
        'deathMessage',
        'kindName',
        'kindDesName',
        'customLabel',
        'customDesc',
        'letterLabel',
        'letterText',
        'message',
        'gerund',
        'jobString',
        'successMessage',
        'rejectedMessage',
        'pawnSingular',
        'pawnsPlural'
    ),
    [System.StringComparer]::OrdinalIgnoreCase
)

$categoryOrder = @(
    '模组信息',
    '派系与世界',
    '角色、背景与血统',
    '服装',
    '武器与弹药',
    '物品与配方',
    '能力与状态',
    '事件与工作',
    '心情与记忆',
    '命名词库',
    '运行时界面文本',
    '其他文本'
)

# 作用：把绝对路径转换为以模组根目录为基准、稳定使用正斜杠的来源路径。
function Get-SourcePath {
    param(
        [Parameter(Mandatory)]
        [string]$FullName
    )

    return $FullName.Substring($modRoot.Length + 1).Replace('\', '/')
}

# 作用：根据 XML 来源目录确定语料分类，便于 AI 按领域检索。
function Get-TextCategory {
    param(
        [Parameter(Mandatory)]
        [string]$SourcePath
    )

    switch -Wildcard ($SourcePath) {
        'About/*' { return '模组信息' }
        '1.6/Defs/FactionDefs/*' { return '派系与世界' }
        '1.6/Defs/WorldObjectDefs/*' { return '派系与世界' }
        '1.6/Defs/TraderKindDefs/*' { return '派系与世界' }
        '1.6/Defs/PawnKindDefs/*' { return '角色、背景与血统' }
        '1.6/Defs/BackstoryDefs/*' { return '角色、背景与血统' }
        '1.6/Defs/GeneDefs/*' { return '角色、背景与血统' }
        '1.6/Defs/Apparel/*' { return '服装' }
        '1.6/Defs/Weapons/*' { return '武器与弹药' }
        '1.6/Defs/ThingDefs/*' { return '物品与配方' }
        '1.6/Defs/AbilityDefs/*' { return '能力与状态' }
        '1.6/Defs/HediffDefs/*' { return '能力与状态' }
        '1.6/Defs/IncidentDef/*' { return '事件与工作' }
        '1.6/Defs/JobDef/*' { return '事件与工作' }
        '1.6/Defs/ThoughtDefs/*' { return '心情与记忆' }
        '1.6/Defs/RulePackDefs/*' { return '命名词库' }
        default { return '其他文本' }
    }
}

# 作用：生成文本节点相对当前 Def 的 XML 路径，并为同名兄弟节点加上序号。
function Get-XmlFieldPath {
    param(
        [Parameter(Mandatory)]
        [System.Xml.XmlNode]$RecordNode,

        [Parameter(Mandatory)]
        [System.Xml.XmlNode]$TextNode
    )

    $segments = [System.Collections.Generic.List[string]]::new()
    $current = $TextNode

    while ($null -ne $current -and -not [object]::ReferenceEquals($current, $RecordNode)) {
        $segment = $current.LocalName
        $parent = $current.ParentNode

        if ($null -ne $parent) {
            $sameNameNodes = @(
                $parent.ChildNodes | Where-Object {
                    $_.NodeType -eq [System.Xml.XmlNodeType]::Element -and $_.LocalName -eq $current.LocalName
                }
            )

            if ($sameNameNodes.Count -gt 1) {
                $index = 1
                for ($i = 0; $i -lt $sameNameNodes.Count; $i++) {
                    if ([object]::ReferenceEquals($sameNameNodes[$i], $current)) {
                        $index = $i + 1
                        break
                    }
                }
                $segment = '{0}[{1}]' -f $segment, $index
            }
        }

        $segments.Insert(0, $segment)
        $current = $parent
    }

    return $segments -join '/'
}

# 作用：判断 XML 叶节点是否属于玩家可见、可本地化或叙事相关文本。
function Test-IsTextNode {
    param(
        [Parameter(Mandatory)]
        [System.Xml.XmlNode]$Node,

        [Parameter(Mandatory)]
        [bool]$IsAboutRecord
    )

    if ($Node.NodeType -ne [System.Xml.XmlNodeType]::Element -or $Node.HasChildNodes -and $Node.ChildNodes.Count -gt 1) {
        return $false
    }

    if ($IsAboutRecord) {
        if ($Node.LocalName -in @('name', 'author', 'description', 'displayName')) {
            return $true
        }

        return $Node.LocalName -eq 'li' -and $Node.ParentNode.LocalName -eq 'supportedVersions'
    }

    if ($textElementNames.Contains($Node.LocalName)) {
        return $true
    }

    return $Node.LocalName -eq 'li' -and $null -ne $Node.ParentNode -and $Node.ParentNode.LocalName -eq 'rulesStrings'
}

# 作用：清理 XML 缩进造成的首尾空白，同时保留文本内部段落与换行。
function Get-NormalizedText {
    param(
        [Parameter(Mandatory)]
        [AllowEmptyString()]
        [string]$Value
    )

    $normalized = $Value.Replace("`r`n", "`n").Replace("`r", "`n").Trim()
    $lines = $normalized.Split([char]10)
    return ($lines | ForEach-Object { $_.TrimEnd() }) -join "`n"
}

# 作用：从一个 XML 记录中收集文本字段，并保留字段路径和原文。
function Get-XmlTextFields {
    param(
        [Parameter(Mandatory)]
        [System.Xml.XmlNode]$RecordNode,

        [Parameter(Mandatory)]
        [bool]$IsAboutRecord
    )

    $fields = [System.Collections.Generic.List[object]]::new()

    foreach ($node in $RecordNode.SelectNodes('.//*[not(*)]')) {
        if (-not (Test-IsTextNode -Node $node -IsAboutRecord $IsAboutRecord)) {
            continue
        }

        $value = Get-NormalizedText -Value $node.InnerText
        if ([string]::IsNullOrWhiteSpace($value)) {
            continue
        }

        $fields.Add([PSCustomObject]@{
            Path = Get-XmlFieldPath -RecordNode $RecordNode -TextNode $node
            Value = $value
        })
    }

    return $fields.ToArray()
}

# 作用：选取记录中最适合作为 Markdown 小节标题的显示文本。
function Get-RecordDisplayName {
    param(
        [Parameter(Mandatory)]
        [object[]]$Fields,

        [Parameter(Mandatory)]
        [string]$Fallback
    )

    foreach ($preferredPath in @('name', 'label', 'fixedName', 'title', 'kindName')) {
        $match = $Fields | Where-Object { $_.Path -eq $preferredPath } | Select-Object -First 1
        if ($null -ne $match) {
            return ($match.Value -split "`n", 2)[0]
        }
    }

    return $Fallback
}

# 作用：把文本值转换为单行字段或多行引用块，保证 Markdown 清晰可读。
function Get-TextFieldMarkdown {
    param(
        [Parameter(Mandatory)]
        [string]$Path,

        [Parameter(Mandatory)]
        [string]$Value
    )

    if (-not $Value.Contains("`n") -and $Value.Length -le 180) {
        return ,('- `{0}`：{1}' -f $Path, $Value)
    }

    $fieldLines = [System.Collections.Generic.List[string]]::new()
    [void]$fieldLines.Add(('- `{0}`：' -f $Path))
    [void]$fieldLines.Add('')
    foreach ($line in $Value.Split([char]10)) {
        [void]$fieldLines.Add(('  > {0}' -f $line))
    }

    return $fieldLines.ToArray()
}

# 作用：从源码中提取中文界面字面量，以及需要翻译的英文界面片段。
function Get-CodeTextRecords {
    param(
        [Parameter(Mandatory)]
        [string]$SourceRoot
    )

    $records = [System.Collections.Generic.List[object]]::new()
    $englishUiFragments = [System.Collections.Generic.HashSet[string]]::new(
        [string[]]@('Fuel: ', ' (Needs Reloading)', 'Reload ', ' with '),
        [System.StringComparer]::Ordinal
    )
    $stringPattern = '(?<!\\)"(?:\\.|[^"\\])*"'

    foreach ($file in Get-ChildItem -LiteralPath $SourceRoot -Recurse -Filter '*.cs' -File | Sort-Object FullName) {
        if ($file.FullName -match '[\\/](obj|bin)[\\/]') {
            continue
        }

        $lineNumber = 0
        foreach ($line in Get-Content -LiteralPath $file.FullName -Encoding UTF8) {
            $lineNumber++
            if ($line.TrimStart().StartsWith('//')) {
                continue
            }

            foreach ($match in [regex]::Matches($line, $stringPattern)) {
                $literal = $match.Value.Substring(1, $match.Value.Length - 2)
                $isChinese = $literal -match '[\p{IsCJKUnifiedIdeographs}]'
                if (-not $isChinese -and -not $englishUiFragments.Contains($literal)) {
                    continue
                }

                $displayText = $literal.Replace('\n', "`n").Replace('\r', '').Replace('\"', '"').Replace('\\', '\')
                $records.Add([PSCustomObject]@{
                    Source = Get-SourcePath -FullName $file.FullName
                    Line = $lineNumber
                    Text = $displayText
                    CodeContext = $line.Trim()
                })
            }
        }
    }

    return $records.ToArray()
}

$xmlRecords = [System.Collections.Generic.List[object]]::new()

foreach ($file in Get-ChildItem -LiteralPath $modRoot -Recurse -Filter '*.xml' -File | Sort-Object FullName) {
    [xml]$document = Get-Content -LiteralPath $file.FullName -Raw -Encoding UTF8
    $sourcePath = Get-SourcePath -FullName $file.FullName

    if ($document.DocumentElement.LocalName -eq 'ModMetaData') {
        $recordNode = $document.DocumentElement
        $fields = @(Get-XmlTextFields -RecordNode $recordNode -IsAboutRecord $true)
        $identifier = $recordNode.packageId

        if ($fields.Count -gt 0) {
            $xmlRecords.Add([PSCustomObject]@{
                Category = '模组信息'
                Source = $sourcePath
                XmlType = $recordNode.LocalName
                DefName = [string]$identifier
                DisplayName = Get-RecordDisplayName -Fields $fields -Fallback ([string]$identifier)
                Fields = $fields
            })
        }
        continue
    }

    if ($document.DocumentElement.LocalName -ne 'Defs') {
        continue
    }

    foreach ($recordNode in $document.DocumentElement.ChildNodes) {
        if ($recordNode.NodeType -ne [System.Xml.XmlNodeType]::Element) {
            continue
        }

        $fields = @(Get-XmlTextFields -RecordNode $recordNode -IsAboutRecord $false)
        if ($fields.Count -eq 0) {
            continue
        }

        $identifierNode = $recordNode.SelectSingleNode('./defName')
        $identifier = if ($null -ne $identifierNode) { $identifierNode.InnerText.Trim() } else { '(无 defName)' }

        $xmlRecords.Add([PSCustomObject]@{
            Category = Get-TextCategory -SourcePath $sourcePath
            Source = $sourcePath
            XmlType = $recordNode.LocalName
            DefName = $identifier
            DisplayName = Get-RecordDisplayName -Fields $fields -Fallback $identifier
            Fields = $fields
        })
    }
}

$sourceRoot = Join-Path $modRoot '1.6\Source\MihoAdfqs'
$codeRecords = @(Get-CodeTextRecords -SourceRoot $sourceRoot)
$lines = [System.Collections.Generic.List[string]]::new()
$xmlFieldCount = ($xmlRecords | ForEach-Object { $_.Fields.Count } | Measure-Object -Sum).Sum
$sourceFileCount = @(
    @($xmlRecords.Source) + @($codeRecords.Source) | Sort-Object -Unique
).Count

[void]$lines.Add('# MihoAdfqs AI 文本语料库')
[void]$lines.Add('')
[void]$lines.Add('这份文档汇总模组中玩家可见、可本地化、叙事相关的文本，供 AI 阅读、检索、续写、校对或翻译使用。')
[void]$lines.Add('')
[void]$lines.Add('## 语料说明')
[void]$lines.Add('')
[void]$lines.Add(('- 生成日期：{0}' -f (Get-Date -Format 'yyyy-MM-dd')))
[void]$lines.Add('- 文本保持原文，不主动纠错或润色；只清理 XML 缩进产生的首尾空白。')
[void]$lines.Add('- RimWorld 占位符、动态插值和规则表达式均保留，翻译或改写时不要删除。')
[void]$lines.Add('- 收录 XML 名称、描述、背景、提示、报告文本、命名规则，以及 C# 运行时显示字符串。')
[void]$lines.Add('- 不收录纯数值配置、贴图路径、程序集标识、代码注释和第三方框架内部名称。')
[void]$lines.Add(('- 当前收录：{0} 个 XML 记录、{1} 个 XML 文本字段、{2} 个代码文本片段，来自 {3} 个源文件。' -f $xmlRecords.Count, $xmlFieldCount, $codeRecords.Count, $sourceFileCount))
[void]$lines.Add('')
[void]$lines.Add('## 快速索引')
[void]$lines.Add('')

foreach ($category in $categoryOrder) {
    $xmlCount = @($xmlRecords | Where-Object { $_.Category -eq $category }).Count
    $codeCount = if ($category -eq '运行时界面文本') { $codeRecords.Count } else { 0 }
    $count = $xmlCount + $codeCount
    if ($count -gt 0) {
        [void]$lines.Add(('- {0}：{1} 条记录' -f $category, $count))
    }
}

foreach ($category in $categoryOrder) {
    if ($category -eq '运行时界面文本') {
        if ($codeRecords.Count -eq 0) {
            continue
        }

        [void]$lines.Add('')
        [void]$lines.Add('## 运行时界面文本')
        [void]$lines.Add('')
        [void]$lines.Add('以下内容来自 C# 字符串。连续行上的多个片段通常会在游戏中拼接成完整句子。')

        foreach ($record in $codeRecords) {
            [void]$lines.Add('')
            [void]$lines.Add(('### `{0}:{1}`' -f $record.Source, $record.Line))
            foreach ($fieldLine in @(Get-TextFieldMarkdown -Path 'text' -Value $record.Text)) {
                [void]$lines.Add($fieldLine)
            }
            [void]$lines.Add(('- `codeContext`：`{0}`' -f $record.CodeContext.Replace('`', '``')))
        }
        continue
    }

    $categoryRecords = @($xmlRecords | Where-Object { $_.Category -eq $category } | Sort-Object Source, DefName)
    if ($categoryRecords.Count -eq 0) {
        continue
    }

    [void]$lines.Add('')
    [void]$lines.Add(('## {0}' -f $category))

    foreach ($record in $categoryRecords) {
        [void]$lines.Add('')
        [void]$lines.Add(('### {0}' -f $record.DisplayName))
        [void]$lines.Add('')
        [void]$lines.Add(('- 来源：`{0}`' -f $record.Source))
        [void]$lines.Add(('- XML 类型：`{0}`' -f $record.XmlType))
        [void]$lines.Add(('- 检索键：`{0}`' -f $record.DefName))

        foreach ($field in $record.Fields) {
            foreach ($fieldLine in @(Get-TextFieldMarkdown -Path $field.Path -Value $field.Value)) {
                [void]$lines.Add($fieldLine)
            }
        }
    }
}

$outputDirectory = Split-Path -Parent $OutputPath
if (-not (Test-Path -LiteralPath $outputDirectory)) {
    New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
}

$utf8WithoutBom = [System.Text.UTF8Encoding]::new($false)
[System.IO.File]::WriteAllText($OutputPath, (($lines -join "`n") + "`n"), $utf8WithoutBom)

Write-Output ('已生成：{0}' -f $OutputPath)
Write-Output ('XML 记录：{0}' -f $xmlRecords.Count)
Write-Output ('XML 文本字段：{0}' -f $xmlFieldCount)
Write-Output ('代码文本片段：{0}' -f $codeRecords.Count)
