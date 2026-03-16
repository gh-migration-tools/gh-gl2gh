using OctoshiftCLI;
using OctoshiftCLI.Services;

namespace GitLabToGitHub.Services;

public sealed class GithubApiExtended : GithubApi
{
    private readonly GithubClient _client;
    private readonly string _apiUrl;
    private readonly RetryPolicy _retryPolicy;
    private readonly Dictionary<string, string> _glExporterHeaders = new() { { "GraphQL-Features", "octoshift_gl_exporter" } };

    public GithubApiExtended(
        GithubClient client,
        string apiUrl,
        RetryPolicy retryPolicy,
        ArchiveUploader multipartUploader)
        : base(client, apiUrl, retryPolicy, multipartUploader)
    {
        _client = client;
        _apiUrl = apiUrl;
        _retryPolicy = retryPolicy;
    }

    public async Task<string> CreateGitLabMigrationSource(string orgId)
    {
        var url = $"{_apiUrl}/graphql";

        const string query = "mutation createMigrationSource($name: String!, $url: String!, $ownerId: ID!, $type: MigrationSourceType!)";
        const string gql = "createMigrationSource(input: {name: $name, url: $url, ownerId: $ownerId, type: $type}) { migrationSource { id, name, url, type } }";

        var payload = new
        {
            query = $"{query} {{ {gql} }}",
            variables = new
            {
                name = "GitLab Source",
                url = "https://not-used",
                ownerId = orgId,
                type = "GL_EXPORTER_ARCHIVE"
            },
            operationName = "createMigrationSource"
        };

        var data = await _client.PostGraphQLAsync(url, payload, _glExporterHeaders).ConfigureAwait(false);
        return (string)data["data"]["createMigrationSource"]["migrationSource"]["id"];
    }

    public async Task<(string State, string RepositoryName, int WarningsCount, string FailureReason, string MigrationLogUrl)> GetMigration(
        string migrationId,
        Dictionary<string, string>? customHeaders = null)
    {
        var url = $"{_apiUrl}/graphql";

        const string query = "query($id: ID!)";
        const string gql = """
                           node(id: $id) {
                               ... on Migration {
                                   id,
                                   sourceUrl,
                                   migrationLogUrl,
                                   migrationSource {
                                       name
                                   },
                                   state,
                                   warningsCount,
                                   failureReason,
                                   repositoryName
                               }
                           }
                           """;

        var payload = new { query = $"{query} {{ {gql} }}", variables = new { id = migrationId } };

        try
        {
            return await _retryPolicy.Retry(async () =>
            {
                var data = await _client.PostGraphQLAsync(url, payload).ConfigureAwait(false);

                return (
                    State: (string)data["data"]["node"]["state"],
                    RepositoryName: (string)data["data"]["node"]["repositoryName"],
                    WarningsCount: (int)data["data"]["node"]["warningsCount"],
                    FailureReason: (string)data["data"]["node"]["failureReason"],
                    MigrationLogUrl: (string)data["data"]["node"]["migrationLogUrl"]);
            })
            .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new OctoshiftCliException($"Failed to get migration state for migration {migrationId}", ex);
        }
    }

    public Task<string> StartGitLabMigration(
        string migrationSourceId,
        string gitlabRepoUrl,
        string orgId,
        string repo,
        string targetToken,
        string? archiveUrl,
        string? targetRepoVisibility = null) =>
        StartMigration(
            migrationSourceId,
            sourceRepoUrl: gitlabRepoUrl,
            orgId,
            repo,
            sourceToken: "not-used",
            targetToken,
            gitArchiveUrl: archiveUrl,
            metadataArchiveUrl: archiveUrl,
            skipReleases: false,
            targetRepoVisibility,
            lockSource: false,
            _glExporterHeaders);

    public async Task<string> StartMigration(
        string migrationSourceId,
        string sourceRepoUrl,
        string orgId,
        string repo,
        string sourceToken,
        string targetToken,
        string? gitArchiveUrl = null,
        string? metadataArchiveUrl = null,
        bool skipReleases = false,
        string? targetRepoVisibility = null,
        bool lockSource = false,
        Dictionary<string, string>? customHeaders = null)
    {
        var url = $"{_apiUrl}/graphql";

        const string query = """
                             mutation startRepositoryMigration(
                                 $sourceId: ID!,
                                 $ownerId: ID!,
                                 $sourceRepositoryUrl: URI!,
                                 $repositoryName: String!,
                                 $continueOnError: Boolean!,
                                 $gitArchiveUrl: String,
                                 $metadataArchiveUrl: String,
                                 $accessToken: String!,
                                 $githubPat: String,
                                 $skipReleases: Boolean,
                                 $targetRepoVisibility: String,
                                 $lockSource: Boolean)
                             """;
        const string gql = """
                           startRepositoryMigration(
                               input: { 
                                   sourceId: $sourceId,
                                   ownerId: $ownerId,
                                   sourceRepositoryUrl: $sourceRepositoryUrl,
                                   repositoryName: $repositoryName,
                                   continueOnError: $continueOnError,
                                   gitArchiveUrl: $gitArchiveUrl,
                                   metadataArchiveUrl: $metadataArchiveUrl,
                                   accessToken: $accessToken,
                                   githubPat: $githubPat,
                                   skipReleases: $skipReleases,
                                   targetRepoVisibility: $targetRepoVisibility,
                                   lockSource: $lockSource
                               }) {
                               repositoryMigration {
                                   id,
                                   databaseId,
                                   migrationSource {
                                       id,
                                       name,
                                       type
                                   },
                                   sourceUrl,
                                   state,
                                   failureReason
                               }
                           }
                           """;

        var payload = new
        {
            query = $"{query} {{ {gql} }}",
            variables = new
            {
                sourceId = migrationSourceId,
                ownerId = orgId,
                sourceRepositoryUrl = sourceRepoUrl,
                repositoryName = repo,
                continueOnError = true,
                gitArchiveUrl,
                metadataArchiveUrl,
                accessToken = sourceToken,
                githubPat = targetToken,
                skipReleases,
                targetRepoVisibility,
                lockSource
            },
            operationName = "startRepositoryMigration"
        };

        var data = await _client.PostGraphQLAsync(url, payload, customHeaders).ConfigureAwait(false);

        return (string)data["data"]["startRepositoryMigration"]["repositoryMigration"]["id"];
    }

    public Task<(string State, string RepositoryName, int WarningsCount, string FailureReason, string MigrationLogUrl)> GetGitLabMigration(string migrationId) =>
        GetMigration(migrationId, _glExporterHeaders);
}
