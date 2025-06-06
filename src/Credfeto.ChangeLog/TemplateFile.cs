﻿using Credfeto.ChangeLog.Helpers;

namespace Credfeto.ChangeLog;

internal static class TemplateFile
{
    public const string Initial =
        @"# Changelog
All notable changes to this project will be documented in this file.

<!--
Please ADD ALL Changes to the UNRELEASED SECTION and not a specific release
-->

## ["
        + Constants.Unreleased
        + @"]
### Added
### Fixed
### Changed
### Removed
### Deployment Changes

<!--
Releases that have at least been deployed to staging, BUT NOT necessarily released to live.  Changes should be moved from [Unreleased] into here as they are merged into the appropriate release branch
-->
## [0.0.0] - Project created";
}
