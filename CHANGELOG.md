# Changelog
All notable changes to this project will be documented in this file.

<!--
Please ADD ALL Changes to the UNRELEASED SECTION and not a specific release
-->

## [Unreleased]
### Added
### Fixed
### Changed
- FF-1429 - Updated SonarAnalyzer.CSharp to 8.18.0.27296
- FF-1429 - Updated TeamCity.VSTest.TestAdapter to 1.0.25
- FF-1429 - Updated coverlet to 3.0.3
### Removed
### Deployment Changes

<!--
Releases that have at least been deployed to staging, BUT NOT necessarily released to live.  Changes should be moved from [Unreleased] into here as they are merged into the appropriate release branch
-->
## [1.5.0] - TBD
### Added
- Added ability to set date on release or to have it marked as TBD\pending
### Fixed
- Fixed bug where an entry would be added to an existing release if the heading did not exist in the [unreleased] section
- Fixing changelog manager erroring on latest ubuntu on github actions
### Changed
- FF-1429 - Updated AsyncFixer to 1.4.0
- FF-1429 - Updated AsyncFixer to 1.5.1
- FF-1429 - Updated SonarAnalyzer.CSharp to 8.17.0.26580
- FF-1429 - Updated Roslynator.Analyzers to 3.1.0

## [1.4.0] - 2020-12-28
### Changed
- FF-1429 - Updated SonarAnalyzer.CSharp to 8.16.0.25740
- Changed to use CommandLineParser library to parse command line

## [1.3.2] - 2020-12-08
### Fixed
- Index of release in difference is +1 from the zero based index when comparing diffs
### Changed
- FF-1429 - Updated SonarAnalyzer.CSharp to 8.15.0.24505
- FF-1429 - Updated Microsoft.NET.Test.Sdk to 16.8.3

## [1.3.1] - 2020-11-30
### Fixed
- Fixed changelog reading/writing so it supports different line endings

## [1.3.0] - 2020-11-26
### Added
- Command to create a new release from unreleased content
- Detection of changelog if it is in the git repo
### Fixed
- Diff of whitespace at EOF is reported as error

## [1.2.0] - 2020-11-21
### Changed
- Updated to .NET 5.0

## [1.1.0] - 2020-10-25
### Added
- Added support for checking where changelog entries were entered

## [1.0.0] - 2020-10-25
### Added
- Unit tests
- Support for adding new entries
- Support for extracting the changelog for a specific release
- Support for extracting the changelog for a unreleased content

## [0.0.0] - Project created