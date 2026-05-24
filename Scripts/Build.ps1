$Config = "Debug"
$Root = (Resolve-Path "$PSScriptRoot/..").Path

dotnet build $Root/FastHash.slnx -c $Config