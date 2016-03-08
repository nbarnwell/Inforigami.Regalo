[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [string] $Source,
    [switch] $IncludeSymbols)

write-verbose "Preparing to push packages..."

$scriptDir = $MyInvocation.MyCommand.Path | split-path
$outputDir = "$scriptDir\BuildOutput"
write-debug "outputDir = $outputDir"

md $outputDir -f | out-null

$exclude = $null;

if ($IncludeSymbols -eq $false) {
    write-verbose "Excluding symbols"
    $exclude = "*.symbols.*";
}

gci "$outputDir\*" -filter *.nupkg -exc $exclude | %{ 
    Write-Host -ForegroundColor Green "$_"

    if ($Source) {
        if ($PSCmdLet.ShouldProcess("Push $($_.Fullname) to $Source")) {
            nuget.exe push $_.FullName -Source $Source
        }
    } else {
        if ($PSCmdLet.ShouldProcess("Push $($_.Fullname) to default source")) {
            nuget.exe push $_.FullName
        }
    }
}
