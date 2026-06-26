$Config = "Debug"
$Root = (Resolve-Path "$PSScriptRoot/..").Path

dotnet build $Root/FastHash.slnx -c $Config

# DPR-059: CI build workflow invokes this script, so run tests here too.
dotnet test $Root/FastHash.slnx -c $Config --no-build