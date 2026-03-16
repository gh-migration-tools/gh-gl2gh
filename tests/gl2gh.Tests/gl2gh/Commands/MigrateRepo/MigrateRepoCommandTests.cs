using GitLabToGitHub.Commands.MigrateRepo;

namespace GitLabToGitHub.Tests.gl2gh.Commands.MigrateRepo;

public class MigrateRepoCommandTests
{
    [Fact]
    public void Should_Have_Options()
    {
        var command = new MigrateRepoCommand();
        Assert.NotNull(command);
        Assert.Equal("migrate-repo", command.Name);
        Assert.Equal(25, command.Options.Count);

        TestHelpers.VerifyCommandOption(command.Options, "docker-image", false);
        TestHelpers.VerifyCommandOption(command.Options, "gitlab-server-url", false);
        TestHelpers.VerifyCommandOption(command.Options, "gitlab-api-url", false);
        TestHelpers.VerifyCommandOption(command.Options, "gitlab-pat", false);
        TestHelpers.VerifyCommandOption(command.Options, "gitlab-username", false);
        TestHelpers.VerifyCommandOption(command.Options, "gitlab-namespace", true);
        TestHelpers.VerifyCommandOption(command.Options, "gitlab-project", true);
        TestHelpers.VerifyCommandOption(command.Options, "gitlab-only", false);
        TestHelpers.VerifyCommandOption(command.Options, "gitlab-except", false);
        TestHelpers.VerifyCommandOption(command.Options, "gitlab-lock-projects", false);
        TestHelpers.VerifyCommandOption(command.Options, "gitlab-without-renumbering", false);
        TestHelpers.VerifyCommandOption(command.Options, "gitlab-manifest", false);
        TestHelpers.VerifyCommandOption(command.Options, "gitlab-ssl-no-verify", false);
        TestHelpers.VerifyCommandOption(command.Options, "gitlab-debug", false);
        TestHelpers.VerifyCommandOption(command.Options, "github-pat", false);
        TestHelpers.VerifyCommandOption(command.Options, "github-org", true);
        TestHelpers.VerifyCommandOption(command.Options, "github-repo", true);
        TestHelpers.VerifyCommandOption(command.Options, "queue-only", false);
        TestHelpers.VerifyCommandOption(command.Options, "target-repo-visibility", false);
        TestHelpers.VerifyCommandOption(command.Options, "target-api-url", false);
        TestHelpers.VerifyCommandOption(command.Options, "verbose", false);
        TestHelpers.VerifyCommandOption(command.Options, "archive-url", false);
        TestHelpers.VerifyCommandOption(command.Options, "archive-path", false);
        TestHelpers.VerifyCommandOption(command.Options, "keep-archive", false);
    }
}
