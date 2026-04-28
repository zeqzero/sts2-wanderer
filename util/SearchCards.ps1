param(
    [string]$Filter = "",
    [string]$Title = "",
    [string]$Cost = "",
    [string]$Type = "",
    [string]$Rarity = "",
    [string]$Target = "",
    [string]$Tags = "",
    [string]$Desc = ""
)

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$locFile = "$scriptRoot/../Wanderer/localization/eng/cards.json"
$loc = @{}
if (Test-Path $locFile) {
    $json = Get-Content $locFile -Raw -Encoding UTF8 | ConvertFrom-Json
    foreach ($prop in $json.PSObject.Properties) {
        $loc[$prop.Name] = $prop.Value
    }
}

# Files to exclude from card search (non-card .cs files in the Cards directory)
$blacklist = @(
    "WandererCard.cs",
    "IEnterStance.cs",
    "Jodan.cs",
    "Waki.cs"
)

# Convert PascalCase to UPPER_SNAKE_CASE
function ConvertTo-UpperSnake($name) {
    ($name -creplace '([a-z])([A-Z])', '$1_$2').ToUpper()
}

$cards = Get-ChildItem -Path "$scriptRoot/../WandererCode/Cards" -Filter "*.cs" -Recurse |
    Where-Object { $_.Name -notin $blacklist } |
    ForEach-Object {
        $content = Get-Content $_.FullName -Raw
        if ($content -match 'base\((\d+),\s*CardType\.(\w+),\s*CardRarity\.(\w+),\s*TargetType\.(\w+)[^)]*\)') {
            $cardCost = $Matches[1]; $cardType = $Matches[2]; $cardRarity = $Matches[3]; $cardTarget = $Matches[4]
            $className = $_.BaseName
            $locKey = "WANDERER-" + (ConvertTo-UpperSnake $className)
            $cardTitle = $loc["$locKey.title"]
            $cardDesc = $loc["$locKey.description"]
            if ($cardDesc) {
                $cardDesc = $cardDesc -replace '\{[^}]+\}', '#' -replace '\[/?[a-z]+\]', '' -replace '\r?\n', ' '
            }
            $cardTags = ""
            if ($content -match '/// <tags>(.+?)</tags>') {
                $cardTags = $Matches[1].Trim()
            }
            $cardKeywords = ""
            if ($content -match 'CanonicalKeywords\s*=>\s*\[([^\]]*)\]') {
                $kwRaw = $Matches[1].Trim()
                if ($kwRaw) {
                    $cardKeywords = ($kwRaw -replace 'CardKeyword\.', '' -replace 'WandererKeywords\.', '' -replace '\s+', '' -split ',') -join ', '
                }
            }
            [PSCustomObject]@{
                Title    = if ($cardTitle) { $cardTitle } else { $className }
                Cost     = $cardCost
                Type     = $cardType
                Rarity   = $cardRarity
                Target   = $cardTarget
                Keywords = $cardKeywords
                Tags     = $cardTags
                Desc     = $cardDesc
                Source   = $content
            }
        }
    } |
    Where-Object { $_ -ne $null }

# Apply broad filter (searches everything)
if ($Filter) {
    $cards = $cards | Where-Object {
        $_.Title -match $Filter -or
        $_.Type -match $Filter -or
        $_.Rarity -match $Filter -or
        $_.Target -match $Filter -or
        $_.Keywords -match $Filter -or
        $_.Desc -match $Filter -or
        $_.Tags -match $Filter -or
        $_.Source -match $Filter
    }
}

# Apply field-specific filters (all must match)
if ($Title)   { $cards = $cards | Where-Object { $_.Title -match $Title } }
if ($Cost)    { $cards = $cards | Where-Object { $_.Cost -eq $Cost } }
if ($Type)    { $cards = $cards | Where-Object { $_.Type -eq $Type } }
if ($Rarity)  { $cards = $cards | Where-Object { $_.Rarity -eq $Rarity } }
if ($Target)  { $cards = $cards | Where-Object { $_.Target -match $Target } }
if ($Tags)    { $cards = $cards | Where-Object { $_.Tags -match $Tags } }
if ($Desc)    { $cards = $cards | Where-Object { $_.Desc -match $Desc } }

$sorted = @($cards | Sort-Object Rarity, Type, Title)
$sorted | Format-Table Title, Cost, Type, Rarity, Target, Keywords, Tags, Desc -AutoSize -Wrap
Write-Host "$($sorted.Count) cards"
