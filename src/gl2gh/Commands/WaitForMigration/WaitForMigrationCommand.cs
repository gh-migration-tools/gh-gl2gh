using OctoshiftCLI.Commands.WaitForMigration;

namespace GitLabToGitHub.Commands.WaitForMigration;

public sealed class WaitForMigrationCommand : WaitForMigrationCommandBase
{
    public WaitForMigrationCommand() => AddOptions();
}
