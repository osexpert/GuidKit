REM set myKey=
set ver=0.0.10

nuget push bin\Packages\Release\NuGet\GuidKit.%ver%.nupkg -src https://api.nuget.org/v3/index.json -ApiKey %myKey%
