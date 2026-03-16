using GitLabToGitHub.Services;
using OctoshiftCLI;
using OctoshiftCLI.Contracts;
using OctoshiftCLI.Services;

namespace GitLabToGitHub.Factories;

public sealed class GitHubApiFactory
{
    private const string DefaultApiUrl = "https://api.github.com";
    private const string DefaultUploadsUrl = "https://uploads.github.com";

    private readonly OctoLogger _logger;
    private readonly IHttpClientFactory _clientFactory;
    private readonly EnvironmentVariableProvider _environmentVariableProvider;
    private readonly DateTimeProvider _dateTimeProvider;
    private readonly RetryPolicy _retryPolicy;
    private readonly IVersionProvider _versionProvider;

    public GitHubApiFactory(
        OctoLogger logger,
        IHttpClientFactory clientFactory,
        EnvironmentVariableProviderExtended environmentVariableProvider,
        DateTimeProvider dateTimeProvider,
        RetryPolicy retryPolicy,
        IVersionProvider versionProvider)
    {
        _logger = logger;
        _clientFactory = clientFactory;
        _environmentVariableProvider = environmentVariableProvider;
        _dateTimeProvider = dateTimeProvider;
        _retryPolicy = retryPolicy;
        _versionProvider = versionProvider;
    }

    public GithubApiExtended Create(
        string? apiUrl = null,
        string? uploadsUrl = null,
        string? targetPersonalAccessToken = null)
    {
        apiUrl ??= DefaultApiUrl;
        uploadsUrl ??= DefaultUploadsUrl;
        targetPersonalAccessToken ??= _environmentVariableProvider.TargetGithubPersonalAccessToken();

        var githubClient = new GithubClient(
            _logger,
            _clientFactory.CreateClient("Default"),
            _versionProvider,
            _retryPolicy,
            _dateTimeProvider,
            targetPersonalAccessToken);

        var multipartUploader = new ArchiveUploader(
            githubClient,
            uploadsUrl,
            _logger,
            _retryPolicy,
            _environmentVariableProvider);

        return new GithubApiExtended(
            githubClient,
            apiUrl,
            _retryPolicy,
            multipartUploader);
    }
}
