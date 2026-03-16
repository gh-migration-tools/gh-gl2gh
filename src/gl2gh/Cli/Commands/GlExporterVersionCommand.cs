using GitHub.Migration.Cli;

namespace GitLabToGitHub.Cli.Commands;

public sealed class GlExporterVersionCommand : ICliCommand
{
    public string CliCommand => "gl_exporter --version";
}
