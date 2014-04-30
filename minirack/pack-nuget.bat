del *.nupkg
copy "..\LICENSE.md" bin
copy README.md bin
NuGet.exe pack sam.minirack.nuspec -BasePath bin -Verbosity detailed
pause
