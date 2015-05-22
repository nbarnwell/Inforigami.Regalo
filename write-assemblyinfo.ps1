param($projectName, $assemblyInfoFilename)
Set-StrictMode -Version Latest

write-host "Updating version information for $projectName..."

$gitVersion = git describe --tags --long --match "v*.*.*" --abbrev=40
$gitVersion -match "^v(\d+)\.(\d+)\.(\d+)\-(\d+)-(g[a-f0-9]{40})`$"
($major, $minor, $build, $revision) = $Matches[1..4]

$assemblyVersion = "$major.$minor.$build.$revision"
$assemblyFileVersion = "$major.$minor.$build.$revision"
$assemblyInformationalVersion = "$major.$minor.$build-alpha$revision"

write-host "Building output as $assemblyVersion to $assemblyInfoFilename..."

$assemblyInfo = @"
using System.Reflection;

[assembly: AssemblyDescription("A basic implementation of Greg Young's CQRS/ES pattern. Built from version $gitVersion.")]
[assembly: AssemblyVersion("$assemblyVersion")]
[assembly: AssemblyFileVersion("$assemblyVersion")]
"@

$assemblyInfo > $assemblyInfoFilename
