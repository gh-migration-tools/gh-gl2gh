using OctoshiftCLI;
using OctoshiftCLI.Extensions;
using OctoshiftCLI.Services;

namespace GitLabToGitHub.Services;

public sealed class EnvironmentVariableProviderExtended : EnvironmentVariableProvider
{
    private readonly OctoLogger _logger;

    public EnvironmentVariableProviderExtended(OctoLogger logger)
        : base(logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    public string GitLabPat(bool throwIfNotFound = true) =>
        GetSecret("GL_PAT", throwIfNotFound);

    public string GitLabUsername(bool throwIfNotFound = true) =>
        GetSecret("GL_USERNAME", throwIfNotFound);

    private static string? GetValue(string name, bool throwIfNotFound)
    {
        var value = Environment.GetEnvironmentVariable(name);

        if (value.IsNullOrWhiteSpace())
        {
            return throwIfNotFound
                ? throw new OctoshiftCliException($"{name} environment variable is not set.")
                : null;
        }

        return value;
    }

    private string GetSecret(string secretName, bool throwIfNotFound)
    {
        var secret = GetValue(secretName, throwIfNotFound);
        _logger.RegisterSecret(secret);
        return secret!;
    }
}
