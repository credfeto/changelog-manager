# Changelog manager .net tool

Changelog manager is a .net tool that works on all supported .net platforms (windows/linux/mac).

## Release Notes

See [CHANGELOG.md](CHANGELOG.md) managed by this tool.

## Installation

### Install as a global tool
```shell
dotnet tool install Credfeto.ChangeLog.Cmd
```

To update to latest released version
```shell
dotnet tool update Credfeto.ChangeLog.Cmd
```

### Install as a local too tool

```shell
dotnet new tool-manifest
dotnet tool install Credfeto.ChangeLog.Cmd --local
```

To update to latest released version
```shell
dotnet tool update Credfeto.ChangeLog.Cmd --local
```

## Usage

Common notes

- The parameter `-changelog CHANGELOG.md` is optional and the changelog will be searched for in the current git repo.  Its is given explicitly in the examples assuming CHANGELOG.md is in the current directory.
- If CHANGELOG.md doesn't exist calling `-add` with and when using `-changelog CHANGELOG.md` will create a file at that location.  It will error if `-changelog CHANGELOG.md` is not used.


### Extracting Release Notes

#### Extract the release notes for a pre-release build
```shell
dotnet changelog -changelog CHANGELOG.md -extract RELEASE_NOTES.md -version 1.0.1.27-master
```

#### Extract the release notes for a release build
```shell
dotnet changelog -changelog CHANGELOG.md -extract RELEASE_NOTES.md -version 1.0.2.77
```

### Add a new entry to the [Unreleased] section

Note the value of the `-add` parameter matches exactly the heading section in the [Unreleased] section.  It will not create/update a new section if it that section does not exist.

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

This pulls out all the changes in the [Unreleased] section and adds them to to the release given by the version number.

```shell
dotnet changelog -changelog CHANGELOG.md -create-release 1.2.3
```

Notes:
- If the specified version already exists then an error will occur.
- If the specified version is older than the latest release then an error will occur.

### Check insert position of changes are all in [Unreleased] section

Where `origin/target` is the name of the branch a PR is to be merged into.

```shell
dotnet changelog -changelog CHANGELOG.md -check-insert origin/target
```

Notes
- Assumes that the branch to be merged into is up-to-date with the latest changes.
- Assumes that all entries have been commited to the local repo.
- All changes in non-committed (staged and unstaged) files are ignored.
