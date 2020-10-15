# DotNet.Mage

DotNet.Mage (currently known as Mage.NET) is open-sourced version of a familiar .NET FX tool Mage.

It is available at Nuget.org. Latest version `https://www.nuget.org/packages/Microsoft.DotNet.Mage/5.0.0-rc.2.20513.1

We are using 'Mage.NET' name in the document from here on out, as the current name of the tool. It will be updated to dotnet-mage before .NET 5 release. After that change, the tool would be used by running `dotnet-mage` or `dotnet mage`.

Mage.NET supports all existing command-line options for the old Mage tool, with few exceptions:
- no support for partial trust
- no support for sha1 hashing
- no support for ia64 architecture

For the full list of Mage command line options please visit https://docs.microsoft.com/en-us/dotnet/framework/tools/mage-exe-manifest-generation-and-editing-tool

There is one new option, to add launcher. Here's the short documentation for this option:

`
  -AddLauncher <binary_to_launch>    -al
      Adds Launcher to target directory and sets its binary to launch.
      Example:
        -AddLauncher myapp.dll -TargetDirectory bin/release
`

Launcher is required for all .NET 5 (and .NET Core 3.1) apps in ClickOnce.

You can obtain all command-line options by running `Mage.NET` or for verbose help `Mage.NET -help verbose`.

## Prerequisites for using this tool

* [Install .NET 5 RC.2 SDK](https://dotnet.microsoft.com/download/dotnet/5.0)

* Install Mage.NET (DotNet.Mage) global tool:

```dotnet tool install --global microsoft.dotnet.mage --version 5.0.0-rc.2.20513.1

## Common usage scenario

* Build the project and copy the produced project output (binaries, json files, etc.) to a new folder
* Add launcher
* Create application manifest
* Create deployment manifest

## Example steps

Suppose that we have copied project output to a sub-folder `files` and our .NET 5 application entry point is `myapp.exe`

* Add Launcher

```mage.net -al myapp.exe -td files

* Create application manifest

```mage.net -new Application -t files\MyApp.manifest -fd files -v 1.0.0.1

* Create deployment manifest

```mage.net -new Deployment -Install true -pub "My Publisher" -v 1.0.0.1 -AppManifest files\MyApp.manifest -t MyApp.application
