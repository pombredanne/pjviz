# Description

Simple command line tool for generating DGML diagram from project.lock.json

# Usage

```
Usage: pjviz.exe --pj <path/to/project.lock.json> --nupkg <NuGet package name> --tfm <TFM> --out <path/to/out.dgml>
Example: pjviz.exe --pj project.lock.json --nupkg System.Runtime --tfm .NETStandardApp,Version=v1.5/win7-x64 --out test.dgml
```