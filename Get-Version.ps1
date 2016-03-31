Set-StrictMode -Version Latest

function Get-GitVersion {
    $gitVersion = git describe --tags --long --match "v*.*.*" --abbrev=40
    $gitVersion -match "^v(\d+)\.(\d+)\.(\d+)\-(\d+)-(g[a-f0-9]{40})`$" | out-null
    ($major, $minor, $build, $revision) = $Matches[1..4]

    $semanticVersion = "$major.$minor.$build"

    if ($revision -gt 0) {
        $semanticVersion = "$major.$minor.$build-alpha$revision"
    }

    @{
    	Major = $major
    	Minor = $minor
    	Build = $build
    	Revision = $revision
    	SemanticVersion = $semanticVersion
    }
}
