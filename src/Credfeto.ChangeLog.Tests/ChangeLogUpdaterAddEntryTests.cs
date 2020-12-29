using Credfeto.ChangeLog.Exceptions;
using Xunit;

namespace Credfeto.ChangeLog.Tests
{
    public sealed class ChangeLogUpdaterAddEntryTests
    {
        [Fact]
        public void AddToEmptyChangelog()
        {
            string result = ChangeLogUpdater.AddEntry(changeLog: string.Empty, type: "Added", message: "Added a new entry");

            const string expected = @"# Changelog
All notable changes to this project will be documented in this file.

<!--
Please ADD ALL Changes to the UNRELEASED SECTION and not a specific release
-->

## [Unreleased]
### Added
- Added a new entry
### Fixed
### Changed
### Removed
### Deployment Changes

<!--
Releases that have at least been deployed to staging, BUT NOT necessarily released to live.  Changes should be moved from [Unreleased] into here as they are merged into the appropriate release branch
-->
## [0.0.0] - Project created";

            Assert.Equal(expected: expected, actual: result);
        }

        [Fact]
        public void AddToExistingChangelog()
        {
            const string existing = @"# Changelog
All notable changes to this project will be documented in this file.

<!--
Please ADD ALL Changes to the UNRELEASED SECTION and not a specific release
-->

## [Unreleased]
### Added
- Added a new entry
### Fixed
### Changed
### Removed
### Deployment Changes

<!--
Releases that have at least been deployed to staging, BUT NOT necessarily released to live.  Changes should be moved from [Unreleased] into here as they are merged into the appropriate release branch
-->
## [0.0.0] - Project created";

            string result = ChangeLogUpdater.AddEntry(changeLog: existing, type: "Added", message: "Another entry");

            const string expected = @"# Changelog
All notable changes to this project will be documented in this file.

<!--
Please ADD ALL Changes to the UNRELEASED SECTION and not a specific release
-->

## [Unreleased]
### Added
- Added a new entry
- Another entry
### Fixed
### Changed
### Removed
### Deployment Changes

<!--
Releases that have at least been deployed to staging, BUT NOT necessarily released to live.  Changes should be moved from [Unreleased] into here as they are merged into the appropriate release branch
-->
## [0.0.0] - Project created";

            Assert.Equal(expected: expected, actual: result);
        }

        [Fact]
        public void AdddingDuplicateToExistingChangelogDoesNotAdd()
        {
            const string existing = @"# Changelog
All notable changes to this project will be documented in this file.

<!--
Please ADD ALL Changes to the UNRELEASED SECTION and not a specific release
-->

## [Unreleased]
### Added
- Added a new entry
### Fixed
### Changed
### Removed
### Deployment Changes

<!--
Releases that have at least been deployed to staging, BUT NOT necessarily released to live.  Changes should be moved from [Unreleased] into here as they are merged into the appropriate release branch
-->
## [0.0.0] - Project created";

            string result = ChangeLogUpdater.AddEntry(changeLog: existing, type: "Added", message: "Added a new entry");

            const string expected = @"# Changelog
All notable changes to this project will be documented in this file.

<!--
Please ADD ALL Changes to the UNRELEASED SECTION and not a specific release
-->

## [Unreleased]
### Added
- Added a new entry
### Fixed
### Changed
### Removed
### Deployment Changes

<!--
Releases that have at least been deployed to staging, BUT NOT necessarily released to live.  Changes should be moved from [Unreleased] into here as they are merged into the appropriate release branch
-->
## [0.0.0] - Project created";

            Assert.Equal(expected: expected, actual: result);
        }

        [Fact]
        public void AddToExistingChangelogWithTrailingBlanks()
        {
            const string existing = @"# Changelog
All notable changes to this project will be documented in this file.

<!--
Please ADD ALL Changes to the UNRELEASED SECTION and not a specific release
-->

## [Unreleased]
### Added
- Added a new entry

### Fixed
### Changed
### Removed
### Deployment Changes

<!--
Releases that have at least been deployed to staging, BUT NOT necessarily released to live.  Changes should be moved from [Unreleased] into here as they are merged into the appropriate release branch
-->
## [0.0.0] - Project created";

            string result = ChangeLogUpdater.AddEntry(changeLog: existing, type: "Added", message: "Another entry");

            const string expected = @"# Changelog
All notable changes to this project will be documented in this file.

<!--
Please ADD ALL Changes to the UNRELEASED SECTION and not a specific release
-->

## [Unreleased]
### Added
- Added a new entry
- Another entry

### Fixed
### Changed
### Removed
### Deployment Changes

<!--
Releases that have at least been deployed to staging, BUT NOT necessarily released to live.  Changes should be moved from [Unreleased] into here as they are merged into the appropriate release branch
-->
## [0.0.0] - Project created";

            Assert.Equal(expected: expected, actual: result);
        }

        [Fact]
        public void AddToExistingChangelogForSectionThatDoesNotExistFails()
        {
            const string existing = @"# Changelog
All notable changes to this project will be documented in this file.

<!--
Please ADD ALL Changes to the UNRELEASED SECTION and not a specific release
-->

## [Unreleased]
### Fixed
### Changed
### Removed
### Deployment Changes

<!--
Releases that have at least been deployed to staging, BUT NOT necessarily released to live.  Changes should be moved from [Unreleased] into here as they are merged into the appropriate release branch
-->
## [0.0.1] - 2020-12-29
### Added
- Added a new entry

## [0.0.0] - Project created";

            Assert.Throws<InvalidChangeLogException>(() => ChangeLogUpdater.AddEntry(changeLog: existing, type: "Added", message: "Another entry"));
        }
    }
}