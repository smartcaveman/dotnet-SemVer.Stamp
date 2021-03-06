﻿using System;
using SemVer.MSBuild;
using SemVer.Stamp;
using SemVer.Stamp.Git;

namespace SemVer.Git.MSBuild
{
  public class SemVerStampTask : SemVerStampTaskBase
  {
    /// <exception cref="ArgumentNullException"><paramref name="repositoryPath" /> is <see langword="null" />.</exception>
    protected override SemVersionGrabberBase GetSemVersionGrabber(string repositoryPath,
                                                                  string baseRevision)
    {
      var semVersionGrabber = new GitSemVersionGrabber(repositoryPath,
                                                       baseRevision,
                                                       arg => this.Log?.LogMessage(arg),
                                                       arg => this.Log?.LogWarning(arg),
                                                       arg => this.Log?.LogError(arg));

      return semVersionGrabber;
    }
  }
}
