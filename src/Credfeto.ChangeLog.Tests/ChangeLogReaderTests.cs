using Credfeto.ChangeLog.Management;
using Xunit;

namespace Credfeto.ChangeLog.Tests
{
    public sealed class ChangeLogReaderTests
    {
        [Theory]
        [InlineData("")]
        [InlineData("1.0.0.1-master")]
        [InlineData("1.0.0.1")]
        public void ReadEmptyChangeLogReturnsEmpty(string version)
        {
            string result = ChangeLogReader.ExtractReleaseNotes(changeLog: string.Empty, version: version);
            Assert.Empty(result);
        }
    }
}