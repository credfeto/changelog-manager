using FunFair.Test.Common;
using Xunit;
using Xunit.Abstractions;

namespace Credfeto.ChangeLog.Tests;

public sealed class ChangeLogDetectorTests : LoggingTestBase
{
    public ChangeLogDetectorTests(ITestOutputHelper output)
        : base(output: output)
    {
    }

    [Fact]
    public void DoIt()
    {
        bool found = ChangeLogDetector.TryFindChangeLog(out string? changeLogFileName);
        Assert.True(condition: found, userMessage: "Should have found a changelog");

        this.Output.WriteLine("Found changelog at: ${changeLogFileName}");
    }
}