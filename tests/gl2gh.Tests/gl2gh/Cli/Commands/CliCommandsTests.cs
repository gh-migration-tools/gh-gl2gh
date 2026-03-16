using FluentAssertions;
using GitHub.Migration.Cli;
using GitLabToGitHub.Cli.Commands;

namespace GitLabToGitHub.Tests.gl2gh.Cli.Commands;

public class CliCommandsTests
{
    private readonly string _newLine = Environment.NewLine;
    private readonly string _separator = $"{Environment.NewLine}  ";

    [Fact]
    public void Should_Return_Command_For_Checking_If_Docker_Is_Installed()
    {
        // Arrange
        var command = new DockerVersionCommand();

        // Act
        var result = command.ToCommandString();

        // Assert
        result.Should().Be("docker --version");
    }

    [Fact]
    public void Should_Return_Command_For_Checking_If_Docker_Image_Is_Installed()
    {
        // Arrange
        var command = new DockerImagesCommand { Filter = "reference=github/gl-exporter:latest" };

        // Act
        var result = command.ToCommandString();

        // Assert
        result.Should().Be("docker images --filter reference=github/gl-exporter:latest");
    }

    [Fact]
    public void Should_Return_Command_For_Checking_If_GlExporter_Is_Installed()
    {
        // Arrange
        var command = new GlExporterVersionCommand();

        // Act
        var result = command.ToCommandString();

        // Assert
        result.Should().Be("gl_exporter --version");
    }

    [Fact]
    public void Should_Return_Command_For_Executing_GlExporter()
    {
        // Arrange
        var command = GetGlExporterCommand();

        const string expectedResult = """
                                      gl_exporter
                                        --out-file archive.tar.gz
                                        --namespace namespace
                                        --project project
                                        --only issues,merge_requests,commit_comments,hooks,wiki
                                        --except issues,merge_requests,commit_comments,hooks,wiki
                                        --lock-projects transient
                                        --without-renumbering issues,merge_requests
                                        --manifest manifest
                                        --ssl-no-verify
                                        --debug
                                      """;

        // Act
        var result = command.ToCommandString(separator: _separator).ReplaceLineEndings(_newLine);

        // Assert
        result.Should().Be(expectedResult.ReplaceLineEndings(_newLine));
    }

    [Fact]
    public void Should_Return_Command_For_Executing_GlExporter_As_Docker_Container()
    {
        // Arrange
        var glExporterCommand = GetGlExporterCommand();
        var dockerCommand = new DockerRunCommand
        {
            Remove = true,
            Interactive = true,
            EnvironmentVariables = [
                "GITLAB_API_ENDPOINT=https://gitlab.com/api/v4",
                "GITLAB_USERNAME=johndoe",
                "GITLAB_API_PRIVATE_TOKEN=glpat-***"
            ],
            Volumes = ["${PWD}:/workspace"],
            Image = "github/gl-exporter:latest",
            Command = glExporterCommand.ToCommandString(separator: _separator)
        };

        const string expectedResult = """
                                      docker run
                                        --rm
                                        --interactive
                                        --env GITLAB_API_ENDPOINT=https://gitlab.com/api/v4
                                        --env GITLAB_USERNAME=johndoe
                                        --env GITLAB_API_PRIVATE_TOKEN=glpat-***
                                        --volume ${PWD}:/workspace
                                        github/gl-exporter:latest
                                        gl_exporter
                                        --out-file archive.tar.gz
                                        --namespace namespace
                                        --project project
                                        --only issues,merge_requests,commit_comments,hooks,wiki
                                        --except issues,merge_requests,commit_comments,hooks,wiki
                                        --lock-projects transient
                                        --without-renumbering issues,merge_requests
                                        --manifest manifest
                                        --ssl-no-verify
                                        --debug
                                      """;

        // Act
        var result = dockerCommand.ToCommandString(separator: _separator).ReplaceLineEndings(_newLine);

        // Assert
        result.Should().Be(expectedResult.ReplaceLineEndings(_newLine));
    }

    private static GlExporterCommand GetGlExporterCommand() =>
        new()
        {
            OutFile = "archive.tar.gz",
            Namespace = "namespace",
            Project = "project",
            Only = "issues,merge_requests,commit_comments,hooks,wiki",
            Except = "issues,merge_requests,commit_comments,hooks,wiki",
            LockProjects = "transient",
            WithoutRenumbering = "issues,merge_requests",
            Manifest = "manifest",
            SslNoVerify = true,
            Debug = true
        };
}
