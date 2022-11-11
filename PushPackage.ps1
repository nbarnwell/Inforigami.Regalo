[CmdletBinding(SupportsShouldProcess = $true)]
param()

write-verbose "Preparing to push packages..."

$scriptDir = $MyInvocation.MyCommand.Path | split-path
$outputDir = "$scriptDir\nugets"
write-debug "outputDir = $outputDir"

md $outputDir -f | out-null

gci "$outputDir\*" -filter *.nupkg | 
    Foreach-Object { 
        Write-Host -ForegroundColor Green "$_"

        if ($PSCmdLet.ShouldProcess("Push $($_.Fullname) to default source")) {
            dotnet nuget push $_.FullName --source nuget.org --api-key $env:NugetOrgApiKey
        }
    }
