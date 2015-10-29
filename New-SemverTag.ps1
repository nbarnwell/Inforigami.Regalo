[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [Parameter(ParameterSetName = "BreakingChange")]
    [switch] $BreakingChange = $false,
    [Parameter(ParameterSetName = "NewFeature")]
    [switch] $NewFeature = $false,
    [Parameter(ParameterSetName = "BugFix")]
    [switch] $BugFix = $false)

$scriptDir = $MyInvocation.MyCommand.Path | split-path
. (Join-Path $scriptDir 'Get-Version.ps1')

$v = Get-GitVersion
[int] $Major = $v.Major;
[int] $Minor = $v.Minor;
[int] $Build = $v.Build;

write-host -Object ('Current tag: v{0}.{1}.{2}' -f $Major, $Minor, $Build)

if ($BreakingChange) {
    $Major += 1;
    $Minor = 0;
    $Build = 0;
} 

if ($NewFeature) {
    $Minor += 1;
    $Build = 0;
}

if ($BugFix) {
    $Build += 1;
}

$newTag = 'v{0}.{1}.{2}' -f $Major, $Minor, $Build

if ($PsCmdlet.ShouldProcess("Adding tag $newTag...")) {
    write-host "Adding tag $newTag..."
    git tag $newTag
}
