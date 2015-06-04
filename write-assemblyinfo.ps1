param($projectName, $assemblyInfoFilename)
Set-StrictMode -Version Latest

write-host "Updating version information for $projectName..."

$scriptDir = $MyInvocation.MyCommand.Path | split-path

. (Join-Path $scriptDir 'Get-Version.ps1')

$v = Get-GitVersion

$AssemblyVersion = "$($v.Major).$($v.Minor).$($v.Build).$($v.Revision)"
$AssemblyFileVersion = "$($v.Major).$($v.Minor).$($v.Build).$($v.Revision)"

write-host "Building output as $assemblyVersion to $assemblyInfoFilename..."

$assemblyInfo = @"
using System.Reflection;

[assembly: AssemblyDescription("A basic implementation of Greg Young's CQRS/ES pattern. Built from version $assemblyVersion.")]
[assembly: AssemblyVersion("$assemblyVersion")]
[assembly: AssemblyFileVersion("$assemblyVersion")]
"@

$assemblyInfo > $assemblyInfoFilename
