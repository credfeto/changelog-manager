# Changelog manager .net tool

## Installation

```shell
dotnet tool install Credfeto.ChangeLog.Cmd
```

## Usage
Note the parameter `-changelog CHANGELOG.md` is optional and the changelog will be searched for in the current git repo.

### Extracting Release Notes

### Extract the release notes for a pre-release build
```shell
dotnet changelog -changelog CHANGELOG.md -extract RELEASE_NOTES.md -version 1.0.1.27-master
```

Extract the release notes for a release build
```shell
dotnet changelog -changelog CHANGELOG.md -extract RELEASE_NOTES.md -version 1.0.2.77
```


### Add a new entry to the [Unreleased] section

```shell
dotnet changelog -changelog CHANGELOG.md -add Added -message "Change description"
```

```shell
dotnet changelog -changelog CHANGELOG.md -add Changed -message "Change description"
```

```shell
dotnet changelog -changelog CHANGELOG.md -add Fixed -message "Change description"
```

```shell
dotnet changelog -changelog CHANGELOG.md -add Removed -message "Change description"
```

```shell
dotnet changelog -changelog CHANGELOG.md -add "Deployment Changes" -message "Change description"
```

### Create a release

```shell
dotnet changelog -changelog CHANGELOG.md -create-release 1.2.3
```

### Check insert position of changes are all in [Unreleased] section

Where `origin/master` is the name of the branch a PR is to be merged into.

```shell
dotnet changelog -changelog CHANGELOG.md -check-insert origin/master
```

Note - assumes that all entries have been commited.  Ignores all staged and unstaged files.