﻿using System;
using System.Xml.Linq;

// ReSharper disable CatchAllClause

namespace Fody.SemVer
{
  public sealed class Configuration
  {
    /// <exception cref="ArgumentNullException"><paramref name="element" /> is <see langword="null" />.</exception>
    /// <exception cref="WeavingException">
    ///   If 'UseProject' could not be read from <see cref="XElement.Attributes" /> of
    ///   <see cref="element" />.
    /// </exception>
    /// <exception cref="WeavingException">If 'UseProject' could not be parsed as <see cref="bool" />.</exception>
    public Configuration(XElement element)
    {
      if (element == null)
      {
        throw new ArgumentNullException(nameof(element));
      }

      {
        var attribute = element.Attribute("UseProject"); // Not L10N
        if (attribute != null)
        {
          try
          {
            this.UseProject = bool.Parse(attribute.Value);
          }
          catch (Exception exception)
          {
            throw new WeavingException($"Unable to parse {attribute.Value} as boolean from configuartion",
                                       exception);
          }
        }
      }

      {
        var attribute = element.Attribute("PatchFormat"); // Not L10N
        if (attribute != null)
        {
          this.PatchFormat = attribute.Value;
        }
        if (string.IsNullOrEmpty(this.PatchFormat))
        {
          this.PatchFormat = @"^fix(\(.*\))*: ";
        }
      }

      {
        var attribute = element.Attribute("FeatureFormat"); // Not L10N
        if (attribute != null)
        {
          this.FeatureFormat = attribute.Value;
        }
        if (string.IsNullOrEmpty(this.FeatureFormat))
        {
          this.FeatureFormat = @"^feat(\(.*\))*: ";
        }
      }

      {
        var attribute = element.Attribute("BreakingChangeFormat"); // Not L10N
        if (attribute != null)
        {
          this.BreakingChangeFormat = attribute.Value;
        }
        if (string.IsNullOrEmpty(this.BreakingChangeFormat))
        {
          this.BreakingChangeFormat = @"^perf(\(.*\))*: ";
        }
      }
    }

    public bool UseProject { get; }
    public string PatchFormat { get; }
    public string FeatureFormat { get; }
    public string BreakingChangeFormat { get; }
  }
}
