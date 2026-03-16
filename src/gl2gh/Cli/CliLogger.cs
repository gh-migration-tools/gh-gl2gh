using GitHub.Migration.Cli;
using OctoshiftCLI.Services;

namespace GitLabToGitHub.Cli;

public sealed class CliLogger : ICliLogger
{
    private readonly OctoLogger _logger;

    public CliLogger(OctoLogger logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    public void Debug(string message) =>
        _logger.LogVerbose(message);
}
