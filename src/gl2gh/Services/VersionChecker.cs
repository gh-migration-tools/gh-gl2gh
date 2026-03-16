using OctoshiftCLI.Services;

namespace GitLabToGitHub.Services;

public sealed class VersionChecker : VersionCheckerBase
{
    protected override string ProductName => "gl2gh";
    protected override string LatestVersionFileUrl => "https://raw.githubusercontent.com/gh-migration-tools/gh-gl2gh/main/LATEST-VERSION.txt";

    public VersionChecker(HttpClient httpClient, OctoLogger logger)
        : base(httpClient, logger)
    {
    }
}
