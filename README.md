# gh-gl2gh

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/gh-migration-tools/gh-gl2gh/blob/main/LICENSE)
[![CI](https://github.com/gh-migration-tools/gh-gl2gh/actions/workflows/CI.yml/badge.svg)](https://github.com/gh-migration-tools/gh-gl2gh/actions/workflows/CI.yml)

**gh-gl2gh** is a GitHub CLI extension for GitLab to GitHub migrations. This extension uses under the hood the `gl-exporter` project to create an GitLab export archive.

By using the [Octoshift](https://github.com/gh-migration-tools/Octoshift) library this extension is build upon the same codebase and provides the same set of commands as the `gh-gei`, `gh-ado2gh` or `gh-bbs2gh` extensions.

> [!IMPORTANT]
> The `gl-exporter` project is currently in an closed preview and only accessible to GitHub partners.

## Setup

### Install extension

```
gh extension install gh-migration-tools/gh-gl2gh
```

### Update extension

```
gh extension upgrade gh-migration-tools/gh-gl2gh
```

### Skipping version checks

When the CLI is launched, it logs if a newer version of the CLI is available. You can skip this check by setting the `GEI_SKIP_VERSION_CHECK` environment variable to `true`.

```powershell
# Windows PowerShell
$env:GEI_SKIP_VERSION_CHECK = "true"
```

```bash
# macOS/Linux
export GEI_SKIP_VERSION_CHECK=true
```

### Skipping GitHub status checks

When the CLI is launched, it logs a warning if there are any ongoing [GitHub incidents](https://www.githubstatus.com/) that might affect your use of the CLI. You can skip this check by setting the `GEI_SKIP_STATUS_CHECK` environment variable to `true`.

```powershell
# Windows PowerShell
$env:GEI_SKIP_STATUS_CHECK = "true"
```

```bash
# macOS/Linux
export GEI_SKIP_STATUS_CHECK=true
```

### Configuring multipart upload chunk size

Set the `GITHUB_OWNED_STORAGE_MULTIPART_MEBIBYTES` environment variable to change the archive upload part size. Provide the value in mebibytes (MiB); For example:

```powershell
# Windows PowerShell
$env:GITHUB_OWNED_STORAGE_MULTIPART_MEBIBYTES = "10"
```

```bash
# macOS/Linux
export GITHUB_OWNED_STORAGE_MULTIPART_MEBIBYTES=10
```

This sets the chunk size to 10 MiB (10,485,760 bytes). The minimum supported value is 5 MiB, and the default remains 100 MiB.

This might be needed to improve upload reliability in environments with proxies or very slow connections.

## Usage

To get an overview of all commands run:

```
gh gl2gh --help
```

### Migrate repo

To get an overview of all options for the GitLab to GitHub migration run:

```
gh gl2gh migrate-repo --help
```

```
Description:
  Invokes the GitHub API's to migrate the repo and all PR data
  Note: Expects GL_PAT and GH_PAT env variables or --gitlab-pat and --github-pat options to be set.

Usage:
  gl2gh migrate-repo [options]

Options:
  --docker-image <docker-image>                         The gl-exporter Docker image name (e.g. ghcr.io/ORG/gl-exporter:latest). If the Docker image is not provided it is assumed the gl_exporter is installed.
  --gitlab-server-url <gitlab-server-url>               The full URL of the GitLab Server to migrate from. [default: https://gitlab.com]
  --gitlab-api-url <gitlab-api-url>                     The URL of the GitLab API. [default: https://gitlab.com/api/v4]
  --gitlab-pat <gitlab-pat>                             The GitLab PAT. If not set will be read from GL_PAT environment variable.
  --gitlab-username <gitlab-username>                   The GitLab username of a user with site admin privileges. If not set will be read from GL_USERNAME environment variable.
  --gitlab-namespace <gitlab-namespace> (REQUIRED)      The GitLab namespace to migrate.
  --gitlab-project <gitlab-project> (REQUIRED)          The GitLab project to migrate.
  --gitlab-only <gitlab-only>                           A comma-separated list of models to export. Valid values: issues, merge_requests, commit_comments, hooks, wiki.
  --gitlab-except <gitlab-except>                       A comma-separated list of models exclude from export. Valid values: issues, merge_requests, commit_comments, hooks, wiki.
  --gitlab-lock-projects <false|transient|true>         Lock source project when migrating. Valid values: true, false, transient.
  --gitlab-without-renumbering <issues|merge_requests>  Do not renumber either issues or merge requests. Valid values: issues, merge_requests.
  --gitlab-manifest <gitlab-manifest>                   A file of projects to export.
  --gitlab-ssl-no-verify                                Do not validate the GitLab SSL certificate.
  --gitlab-debug                                        Enable debug logging.
  --github-pat <github-pat>                             The GitHub personal access token to be used for the migration. If not set will be read from GH_PAT environment variable.
  --github-org <github-org> (REQUIRED)
  --github-repo <github-repo> (REQUIRED)
  --queue-only                                          Only queues the migration, does not wait for it to finish. Use the wait-for-migration command to subsequently wait for it to finish and view the status.
  --target-repo-visibility <internal|private|public>    The visibility of the target repo. Defaults to private. Valid values are public, private, or internal.
  --target-api-url <target-api-url>                     The URL of the target API, if not migrating to github.com. Defaults to https://api.github.com
  --target-uploads-url <target-uploads-url>             The URL of the target uploads API, if not migrating to github.com. Defaults to https://uploads.github.com
  --verbose
  --archive-url <archive-url>                           URL used to download GitLab migration archive. Only needed if you want to manually retrieve the archive instead of letting this CLI do that for you.
  --archive-path <archive-path>                         Path to GitLab migration archive on disk.
  --keep-archive                                        Keeps the export archive after successfully uploading it. By default, it will be automatically deleted.
  -?, -h, --help                                        Show help and usage information
```

As stated above this extension requires the `gl-exporter` CLI to create an GitLab export archive. The `gl-exporter` CLI must be available on the running host or must be provided as an Docker container.

To run the `gl-exporter` CLI via a Docker container you must add the `--docker-image IMAGE_NAME` option (e.g. `--docker-image ghcr.io/OWNER/gl-exporter:latest`) to the `gh gl2gh migrate-repo` command.

All options with the `--gitlab` prefix are passed down to the `gl-exporter` CLI.

```
gh gl2gh migrate-repo
  --gitlab-pat GL_PAT
  --gitlab-username GL_USERNAME
  --gitlab-namespace GL_NAMESPACE
  --gitlab-project GL_PROJECT
  --gitlab-debug
  --github-org GH_ORG
  --github-repo GH_REPO
  --github-pat GH_PAT
  --verbose
```

## Tips

The export of GitLab webhooks requires elevated permissions. If the provided GitLab PAT do not have the permissions to export webhooks, the archive export will fail. To prevent this use the option `--gitlab-except hooks`.
