using OctoshiftCLI.Commands.AbortMigration;

namespace GitLabToGitHub.Commands.AbortMigration;

public sealed class AbortMigrationCommand : AbortMigrationCommandBase
{
    public AbortMigrationCommand() => AddOptions();
}
