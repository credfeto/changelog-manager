using FunFair.Test.Common;
using Xunit;

namespace Credfeto.ChangeLog.Tests;

public sealed class ChangeLogUpdaterRemoveEntryTests : TestBase
{
    [Fact]
    public void RemoveFromEmptyChangelog()
    {
        string result = ChangeLogUpdater.RemoveEntry(changeLog: string.Empty, type: "Added", message: "Added a new entry");

        const string expected = @"# Changelog
All notable changes to this project will be documented in this file.

<!--
Please ADD ALL Changes to the UNRELEASED SECTION and not a specific release
-->

## [Unreleased]
### Added
### Fixed
### Changed
### Removed
### Deployment Changes

<!--
Releases that have at least been deployed to staging, BUT NOT necessarily released to live.  Changes should be moved from [Unreleased] into here as they are merged into the appropriate release branch
-->
## [0.0.0] - Project created";

        Assert.Equal(expected.ToLocalEndLine(), actual: result);
    }
}