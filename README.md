# PlayEconomy.Play.Common

My common class library where all my PlayEconomy microservices project will hold its common code, this library will be
exposed to the microservices via NuGet packages.

Packages are published on Github packages.

## Create and publish package
```powershell
$version=1.10.0
$owner="PlayEcomony-Microservices"
dotnet pack src\Play.Common --configuration Release -p:PackageVersion=$version -p:RepositoryUrl=https://github.com/$owner/Play.Common -o ..\packages
```