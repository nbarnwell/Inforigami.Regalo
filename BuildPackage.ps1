$scriptDir = $MyInvocation.MyCommand.Path | split-path
$outputDir = "$scriptDir\BuildOutput"

. (Join-Path $scriptDir 'Get-Version.ps1')

$v = Get-GitVersion
$semanticVersion = "$($v.Major).$($v.Minor).$($v.Build)-$($v.Revision)"

md $outputDir -f | out-null
del $outputDir\*

gci $scriptDir -include Inforigami.Regalo.Core.csproj,Inforigami.Regalo.RavenDB.csproj,Inforigami.Regalo.Testing.csproj,Inforigami.Regalo.ObjectCompare.csproj -recurse | %{ 
    Write-Host -ForegroundColor Green "$_"
    nuget.exe pack $_ -Build -Symbols -outputdirectory $outputDir -Properties Configuration=Release -version $semanticVersion
}
