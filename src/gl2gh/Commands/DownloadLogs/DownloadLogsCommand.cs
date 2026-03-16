using OctoshiftCLI.Commands.DownloadLogs;

namespace GitLabToGitHub.Commands.DownloadLogs;

public sealed class DownloadLogsCommand : DownloadLogsCommandBase
{
    public DownloadLogsCommand() => AddOptions();
}
