# Build RESTA



### Run using .NET Core

To run RESTA from the source code, you need to install .NET Core. The latest version requires .NET Core 6.x framework. To run RESTA using .NET SDK use the following command:

```shell
cd Source/Resta
dotnet run {runbook} [options]
```



### Windows

To use RESTA on Windows without the .NET framework installed, you need to build a self-contained distribution. Specify the runtime and framework:

```shell
cd Source/Resta
dotnet publish -c Release -r win10-x64 -f net6.0 --self-contained -o {path-for-binaries}
```



### Mac OS

To use RESTA on Mac OS without the .NET framework installed, you need to build a self-contained distribution. Specify the runtime and framework:

```shell
cd Source/Resta
dotnet publish -c Release -r osx.11.0-x64 -f net6.0 --self-contained -o {path-for-binaries}
```



Here is the list of available runtimes: https://docs.microsoft.com/en-us/dotnet/core/rid-catalog



### 