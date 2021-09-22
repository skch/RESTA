# Build RESTA



### Run using .NET Core

To run RESTA from the source code, you need to install .NET Core. Current version support .NET Core 3.1 and 5.x frameworks. You need to specify which framework you want to use:

```shell
cd Source/Resta
dotnet run {runbook} [options] --framework netcoreapp5.0
```



### Windows

To use RESTA on Windows without the .NET framework installed, you need to build a self-contained distribution. Specify the runtime and framework:

```shell
cd Source/Resta
dotnet publish -c Release -r win10-x64 -f netcoreapp5.0 --self-contained -o {path-for-binaries}
```



### Mac OS

To use RESTA on Mac OS without the .NET framework installed, you need to build a self-contained distribution. Specify the runtime and framework:

```shell
cd Source/Resta
dotnet publish -c Release -r osx.11.0-x64 -f netcoreapp5.0 --self-contained -o {path-for-binaries}
```



Here is the list of available runtimes: https://docs.microsoft.com/en-us/dotnet/core/rid-catalog



### 