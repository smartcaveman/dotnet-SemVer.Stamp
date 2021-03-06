﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibGit2Sharp;

// ReSharper disable EventExceptionNotDocumented

namespace SemVer.Stamp.Git
{
  public sealed class GitSemVersionGrabber : SemVersionGrabberBase
  {
    /// <exception cref="ArgumentNullException"><paramref name="repositoryPath" /> is <see langword="null" />.</exception>
    public GitSemVersionGrabber(string repositoryPath,
                                string baseRevision,
                                Action<string> logInfo,
                                Action<string> logWarning,
                                Action<string> logError)
      : base(logInfo,
             logWarning,
             logError)
    {
      if (repositoryPath == null)
      {
        throw new ArgumentNullException(nameof(repositoryPath));
      }

      this.RepositoryPath = repositoryPath;
      this.BaseRevision = baseRevision;
    }

    private string BaseRevision { get; }
    private string RepositoryPath { get; }

    /*
    private void IncludeNativeBinariesFolderInPathEnvironmentVariable()
    {
      string architectureSubFolder;
      if (Environment.Is64BitProcess)
      {
        architectureSubFolder = "amd64";
      }
      else
      {
        architectureSubFolder = "x86";
      }

      var nativeBinariesPath = Path.Combine(this.AddinDirectoryPath,
                                            "NativeBinaries",
                                            architectureSubFolder);

      this.LogInfo?.Invoke($"NativeBinaries path: {nativeBinariesPath}");

      var existingPath = Environment.GetEnvironmentVariable("PATH");
      var newPath = string.Concat(nativeBinariesPath,
                                  Path.PathSeparator,
                                  existingPath);
      Environment.SetEnvironmentVariable("PATH",
                                         newPath);
    }
    */

    protected override IEnumerable<string> GetCommitMessages()
    {
      var gitDirectory = Repository.Discover(this.RepositoryPath);
      if (gitDirectory == null)
      {
        this.LogWarning?.Invoke($"found no git repository in {this.RepositoryPath}");
        return Enumerable.Empty<string>();
      }

      this.LogInfo?.Invoke($"found git repository in {this.RepositoryPath}: {gitDirectory}");

      var relativePath = this.GetRelativePath(gitDirectory,
                                              this.RepositoryPath);

      this.LogInfo?.Invoke($"relative path to {nameof(this.RepositoryPath)}: {relativePath}");

      ICollection<string> commitMessages;
      using (var repository = new Repository(gitDirectory))
      {
        var statusOptions = new StatusOptions
                            {
                              ExcludeSubmodules = true,
                              IncludeUnaltered = false,
                              DetectRenamesInIndex = false,
                              DetectRenamesInWorkDir = false,
                              DisablePathSpecMatch = true,
                              PathSpec = new[]
                                         {
                                           relativePath
                                         },
                              RecurseIgnoredDirs = false,
                              Show = StatusShowOption.WorkDirOnly
                            };
        var status = repository.RetrieveStatus(statusOptions);
        if (status.IsDirty)
        {
          this.LogError?.Invoke($"Could not calculate version for {relativePath} due to local uncommitted changes");
          return Enumerable.Empty<string>();
        }

        var branch = repository.Head;

        List<object> excludeReachableFrom;
        List<object> includeReachableFrom;
        if (string.IsNullOrEmpty(this.BaseRevision))
        {
          this.LogInfo?.Invoke($"retrieving commits from {branch.CanonicalName}");
          excludeReachableFrom = new List<object>
                                 {
                                   branch.CanonicalName
                                 };
          includeReachableFrom = null;
        }
        else if (!string.IsNullOrEmpty(relativePath))
        {
          this.LogError?.Invoke($"retrieving the commits from a {nameof(relativePath)} and a {nameof(this.BaseRevision)} is currently not implemented");
          return Enumerable.Empty<string>();
        }
        else
        {
          this.LogInfo?.Invoke($"retrieving commits from {branch.CanonicalName} since {this.BaseRevision}");
          excludeReachableFrom  = new List<object>
                                  {
                                    this.BaseRevision
                                  };
          includeReachableFrom= new List<object>
                                {
                                  branch.Tip.Id
                                };
        }

        var commitFilter = new CommitFilter
                           {
                             IncludeReachableFrom = includeReachableFrom,
                             ExcludeReachableFrom = excludeReachableFrom,
                             SortBy = CommitSortStrategies.Reverse | CommitSortStrategies.Time
                           };
        var commits = repository.Commits.QueryBy(commitFilter);

        commitMessages = commits.Select(arg => arg.Message)
                                .ToArray();
      }

      return commitMessages;
    }

    private string GetRelativePath(string gitDirectoryPath,
                                   string targetPath)
    {
      var repositoryPath = Directory.GetParent(gitDirectoryPath)
        ?.Parent?.FullName;
      if (string.IsNullOrEmpty(repositoryPath))
      {
        return repositoryPath;
      }

      var result = targetPath.Substring(repositoryPath.Length)
                             .TrimStart(Path.DirectorySeparatorChar);

      return result;
    }
  }
}
