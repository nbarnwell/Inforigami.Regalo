function Get-PathFromEnvironmentVariable {
    [CmdletBinding()]
    param([string] $environmentVariableName, [string] $defaultPath)
  $result = $defaultPath
  if (Test-Path "env:\$environmentVariableName") {
    $path = (gc "env:\$environmentVariableName")
    if (Test-Path "$path") {
      $result = $path
    }
  }
  return $result -replace '\\+$', ''
}

function Assert-DotSourced {
    [CmdletBinding()]
    param($Invocation)

    $invocationName = $Invocation.InvocationName
    if ($invocationName -ne '.') {
        Write-Error "This script should be dot-sourced, e.g. `". $invocationName`"."
    }
}

function Install-GetEventStore {
    Write-Host -Foreground Green "Installing GetEventStore..."
    $eventStorePath = Get-PathFromEnvironmentVariable "EVENTSTORE_LOCATION" "C:\GetEventStore\"
    md $eventStorePath -Force | Out-Null
    pushd $eventStorePath
    try {
        $zipFile = (Join-Path $env:TEMP es.zip)
        Invoke-WebRequest -Uri "http://s3-eu-west-1.amazonaws.com/eventstore-binaries/binaries/4.1.4/windows/EventStore-OSS-Win-v4.1.4.zip" -OutFile $zipFile
        Expand-Archive $zipFile -DestinationPath $eventStorePath -Force
    } finally {
        popd
    }
}

function Start-EventStore {
    [CmdletBinding()]
    param()
    $eventStorePath = Get-PathFromEnvironmentVariable "EVENTSTORE_LOCATION" "C:\GetEventStore\"
    $dataRoot = Get-PathFromEnvironmentVariable "EVENTSTORE_DATA" (Join-Path $eventStorePath "Data")

    if (@(Get-ChildItem C:\GetEventStore -ErrorAction SilentlyContinue).Length -eq 0) {
        Install-GetEventStore
    }

    # Note you can set the env var as follows: $env:EVENTSTORE_CLUSTERSIZE = 1
    Write-Host -Foreground Green "Starting GetEventStore..."
    $clusterSize = 3
    if (Test-Path "env:\EVENTSTORE_CLUSTERSIZE") {
        $clusterSize = (gc "env:\EVENTSTORE_CLUSTERSIZE")
    }

    $gossipSeeds = ""

    $start = 1
    $end = $clusterSize

    $start..$end |
        %{
            $thou = $_ * 1000;
            $port = $thou + 113;
            $gossipSeeds += "127.0.0.1:$port,"
        }

    $gossipSeeds = $gossipSeeds.Trim(',');

    $start..$end |
        %{
            $id = $_;
            $thou = $id * 1000;

            $intTcpPort = $thou + 111;
            $extTcpPort = $thou + 112;
            $intHttpPort = $thou + 113;
            $extHttpPort = $thou + 114;

            $dataRoot = Join-Path $eventStorePath "databases\database$id"
            $database = Join-Path $dataRoot "data"
            $logs = Join-Path $dataRoot "logs"

            $args = "--db=$database --log=$logs --int-ip=127.0.0.1 --ext-ip=127.0.0.1 --int-tcp-port=$intTcpPort --ext-tcp-port=$extTcpPort --int-http-port=$intHttpPort --ext-http-port=$extHttpPort --run-projections=all --start-standard-projections=true"

            if ($clusterSize -gt 1) {
                $args += " --cluster-size=$clusterSize --discover-via-dns=false --gossip-seed=$gossipSeeds"
            }

            Write-Verbose "Starting event store with args: $args"

            Start "$eventStorePath\EventStore.ClusterNode.exe" -ArgumentList $args
    }
}

Assert-DotSourced $MyInvocation -ErrorAction Stop

Write-Host @'
Now you can using the following functions:
   Install-GetEventStore
   Start-EventStore
'@
