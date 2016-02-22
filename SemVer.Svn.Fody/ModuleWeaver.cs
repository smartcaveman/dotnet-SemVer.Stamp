﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using SharpSvn;

// ReSharper disable CheckNamespace
// ReSharper disable ExceptionNotDocumented
// ReSharper disable ExceptionNotDocumentedOptional
// ReSharper disable EventExceptionNotDocumented
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace SemVer.Fody
{
  public sealed partial class ModuleWeaver
  {
    /// <exception cref="ArgumentNullException"><paramref name="repositoryPath" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="patchFormat" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="featureFormat" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="breakingChangeFormat" /> is <see langword="null" />.</exception>
    private Version GetVersion(string repositoryPath,
                               string patchFormat,
                               string featureFormat,
                               string breakingChangeFormat)
    {
      if (repositoryPath == null)
      {
        throw new ArgumentNullException(nameof(repositoryPath));
      }

      Collection<SvnLogEventArgs> logItems;
      using (var svnClient = new SvnClient())
      {
        var svnLogArgs = new SvnLogArgs
                         {
                           StrictNodeHistory = true
                         };
        if (!svnClient.GetLog(repositoryPath,
                              svnLogArgs,
                              out logItems) ||
            logItems == null)
        {
          this.LogError($"Could not get log for repository in {repositoryPath}");
          return null;
        }
      }

      var commitMessages = logItems.Select(arg => arg.LogMessage);
      var version = this.GetVersionAccordingToSemVer(commitMessages,
                                                     patchFormat,
                                                     featureFormat,
                                                     breakingChangeFormat);

      return version;
    }
  }
}