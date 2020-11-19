# Changelog manager .net tool

## Installation

```shell
dotnet tool install Credfeto.ChangeLog.Cmd
```

## Usage

Extract the release notes for a pre-release build
```shell
dotnet changelog -changelog CHANGELOG.md -extract RELEASE_NOTES.md -version 1.0.1.27-master
```

Extract the release notes for a release build
```shell
dotnet changelog -changelog CHANGELOG.md -extract RELEASE_NOTES.md -version 1.0.2.77
```

