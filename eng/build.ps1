[CmdletBinding(PositionalBinding=$false)]
Param(
  [switch][Alias('h')]$help,
  [switch][Alias('t')]$test,
  [ValidateSet("Debug","Release","Checked")][string[]][Alias('c')]$configuration = @("Debug"),
  [string]$vs,
  [string][Alias('v')]$verbosity = "minimal",
  [ValidateSet("Windows_NT","Linux","OSX","Browser")][string]$os,
  [switch]$testnobuild,
  [ValidateSet("x86","x64","arm","arm64","wasm")][string[]][Alias('a')]$arch = @([System.Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture.ToString().ToLowerInvariant()),
  [string][Alias('s')]$subset,
  [Parameter(ValueFromRemainingArguments=$true)][String[]]$properties
)

function Get-Help() {
  Write-Host "Common settings:"
  Write-Host "  -arch (-a)                     Target platform: x86, x64, arm, arm64, or wasm."
  Write-Host "                                 Pass a comma-separated list to build for multiple architectures."
  Write-Host "                                 [Default: Your machine's architecture.]"
  Write-Host "  -binaryLog (-bl)               Output binary log."
  Write-Host "  -configuration (-c)            Build configuration: Debug, Release or Checked."
  Write-Host "                                 Checked is exclusive to the CLR subset. It is the same as Debug, except code is"
  Write-Host "                                 compiled with optimizations enabled."
  Write-Host "                                 Pass a comma-separated list to build for multiple configurations."
  Write-Host "                                 [Default: Debug]"
  Write-Host "  -help (-h)                     Print help and exit."
  Write-Host "  -os                            Target operating system: Windows_NT, Linux, OSX, or Browser."
  Write-Host "                                 [Default: Your machine's OS.]"
  Write-Host "  -subset (-s)                   Build a subset, print available subsets with -subset help."
  Write-Host "                                 [Default: Builds the entire repo.]"
  Write-Host "  -verbosity (-v)                MSBuild verbosity: q[uiet], m[inimal], n[ormal], d[etailed], and diag[nostic]."
  Write-Host "                                 [Default: Minimal]"
  Write-Host "  -vs                            Open the solution with Visual Studio using the locally acquired SDK."
  Write-Host "                                 Path or any project or solution name is accepted."
  Write-Host "                                 (Example: -vs Microsoft.CSharp)"
  Write-Host ""

  Write-Host "Actions (defaults to -restore -build):"
  Write-Host "  -build (-b)             Build all source projects."
  Write-Host "                          This assumes -restore has been run already."
  Write-Host "  -clean                  Clean the solution."
  Write-Host "  -pack                   Package build outputs into NuGet packages."
  Write-Host "  -publish                Publish artifacts (e.g. symbols)."
  Write-Host "                          This assumes -build has been run already."
  Write-Host "  -rebuild                Rebuild all source projects."
  Write-Host "  -restore                Restore dependencies."
  Write-Host "  -sign                   Sign build outputs."
  Write-Host "  -test (-t)              Incrementally builds and runs tests."
  Write-Host "                          Use in conjuction with -testnobuild to only run tests."
  Write-Host ""

  Write-Host "Libraries settings:"
  Write-Host "  -testnobuild            Skip building tests when invoking -test."
  Write-Host ""

  Write-Host "Command-line arguments not listed above are passed through to MSBuild."
  Write-Host "The above arguments can be shortened as much as to be unambiguous."
  Write-Host "(Example: -con for configuration, -t for test, etc.)."
  Write-Host ""

  Write-Host "Here are some quick examples. These assume you are on a Windows x64 machine:"
  Write-Host ""
  Write-Host "* Build ClickOnce tools for Windows x64 on Release configuration:"
  Write-Host ".\build.cmd -subset clickonce -c release"
  Write-Host ""
  Write-Host "* Build ClickOnce tools and installers for Windows x64 on Release configuration:"
  Write-Host ".\build.cmd -subset clickonce+installer -c release"
  Write-Host ""
  Write-Host "For more information, check out https://github.com/dotnet/runtime/blob/main/docs/workflow/README.md"
}

if ($help -or (($null -ne $properties) -and ($properties.Contains('/help') -or $properties.Contains('/?')))) {
  Get-Help
  exit 0
}

# Check if an action is passed in
$actions = "b","build","r","restore","rebuild","sign","testnobuild","publish","clean","pack"
$actionPassedIn = @(Compare-Object -ReferenceObject @($PSBoundParameters.Keys) -DifferenceObject $actions -ExcludeDifferent -IncludeEqual).Length -ne 0
if ($null -ne $properties -and $actionPassedIn -ne $true) {
  $actionPassedIn = @(Compare-Object -ReferenceObject $properties -DifferenceObject $actions.ForEach({ "-" + $_ }) -ExcludeDifferent -IncludeEqual).Length -ne 0
}

if (!$actionPassedIn) {
  $arguments = "-restore -build"
}

foreach ($argument in $PSBoundParameters.Keys)
{
  switch($argument)
  {
    "subset"                 { $arguments += " /p:Subset=$($PSBoundParameters[$argument].ToLowerInvariant())" }
    "os"                     { $arguments += " /p:TargetOS=$($PSBoundParameters[$argument])" }
    "properties"             { $arguments += " " + $properties }
    "verbosity"              { $arguments += " -$argument " + $($PSBoundParameters[$argument]) }
    # configuration and arch can be specified multiple times, so they should be no-ops here
    "configuration"          {}
    "arch"                   {}
    default                  { $arguments += " /p:$argument=$($PSBoundParameters[$argument])" }
  }
}

$failedBuilds = @()

foreach ($config in $configuration) {
  $argumentsWithConfig = $arguments + " -configuration $((Get-Culture).TextInfo.ToTitleCase($config))";
  foreach ($singleArch in $arch) {
    $argumentsWithArch =  "/p:TargetArchitecture=$singleArch " + $argumentsWithConfig
    $env:__DistroRid="win-$singleArch"
    Invoke-Expression "& `"$PSScriptRoot/common/build.ps1`" $argumentsWithArch"
    if ($lastExitCode -ne 0) {
        $failedBuilds += "Configuration: $config, Architecture: $singleArch"
    }
  }
}

if ($failedBuilds.Count -ne 0) {
    Write-Host "Some builds failed:"
    foreach ($failedBuild in $failedBuilds) {
        Write-Host "`t$failedBuild"
    }
    exit 1
}

exit 0
