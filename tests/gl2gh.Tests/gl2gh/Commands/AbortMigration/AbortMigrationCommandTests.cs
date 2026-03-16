using FluentAssertions;
using GitLabToGitHub.Commands.AbortMigration;

namespace GitLabToGitHub.Tests.gl2gh.Commands.AbortMigration;

public class ArchiveGeneratorTests
{
    [Fact]
    public void Should_Have_Options()
    {
        var command = new AbortMigrationCommand();
        command.Should().NotBeNull();
        command.Name.Should().Be("abort-migration");
        command.Options.Count.Should().Be(4);

        TestHelpers.VerifyCommandOption(command.Options, "migration-id", true);
        TestHelpers.VerifyCommandOption(command.Options, "github-pat", false);
        TestHelpers.VerifyCommandOption(command.Options, "verbose", false);
        TestHelpers.VerifyCommandOption(command.Options, "target-api-url", false);
    }
}
