//--------------------------------------------------------------------------
// <copyright file="AddMultiLanguageSupport.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>?abc?</summary>
//--------------------------------------------------------------------------
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Collections.Generic;
using System.Xml;

namespace Microsoft.Dynamics.Nav.Tools.FormTransformation
{
  /// <summary>
  /// This class will move pre-saved ML captions from translation file (in NAV classic: Tools->Translate->Export) to newly transformed pages.
  /// </summary>
  internal static class AddMultiLanguageSupport
  {
    internal enum TranslationFileStates
    {
      NotRead,
      NotReady,
      Read
    }

    internal enum TranslationType
    {
      CaptionMl,
      OptionCaptionMl,
      TextVariable,
      ToolTip
    }

    private static TranslationFileStates fileState = TranslationFileStates.NotRead;
    private static Dictionary<int, Dictionary<TranslationType, Dictionary<int, string>>> translations = new Dictionary<int, Dictionary<TranslationType, Dictionary<int, string>>>();

    internal static TranslationFileStates FileState
    {
      get
      {
        return fileState;
      }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "WriteAbort method writes all types of exceptions to log")]
    private static void ReadTranslationFile()
    {
      if (fileState == TranslationFileStates.NotRead)
      {
        int pageId = 0;
        if (string.IsNullOrEmpty(UserSetupManagement.Instance.TranslationFile))
        {
          fileState = TranslationFileStates.NotRead;
          return;
        }

        try
        {
          System.Text.Encoding encoding;
          string encodingPage = System.Configuration.ConfigurationManager.AppSettings["EncodingPage"];
          int encodingPageCode;
          try
          {
            encodingPageCode = Convert.ToInt32(encodingPage, CultureInfo.InvariantCulture);
            try
            {
              encoding = System.Text.Encoding.GetEncoding(encodingPageCode);
            }
            catch (Exception ex2)
            {
              string log = string.Format(CultureInfo.InvariantCulture, Resources.IncorrectEncodingPage, ex2.Message);
              TransformationLog.GenericLogEntry(log, LogCategory.Error, (int)LogEntryObjectId.None);
              encoding = System.Text.Encoding.GetEncoding(850);
            }
          }
          catch (FormatException)
          {
            try
            {
              encoding = System.Text.Encoding.GetEncoding(encodingPage);
            }
            catch (Exception ex3)
            {
              string log = string.Format(CultureInfo.InvariantCulture, Resources.IncorrectEncodingPage, ex3.Message);
              TransformationLog.GenericLogEntry(log, LogCategory.Error, (int)LogEntryObjectId.None);
              encoding = System.Text.Encoding.GetEncoding(850);
            }
          }

          string[] readFile = File.ReadAllLines(UserSetupManagement.Instance.TranslationFile, encoding);

          Regex pageIdExp = new Regex(@"N(?<pagelId>\d+)-", RegexOptions.IgnoreCase);
          Regex controlIdExp = new Regex(@"-(?<controlType>[CGQ])(?<controlId>\d+)-", RegexOptions.IgnoreCase);
          Regex languageIdExp = new Regex(@"-A(?<languageId>\d+)-", RegexOptions.IgnoreCase);
          Regex captionExp = new Regex(@"-L999:(?<caption>[\W\w\s]*)", RegexOptions.IgnoreCase);
          Regex translationTypeExp = new Regex(@"-P(?<translationType>\d+)-", RegexOptions.IgnoreCase);
          Regex actionIdExp = new Regex(@"-G(?<actionId>\d+)-", RegexOptions.IgnoreCase); 
          Match translationTypeMatche;

          foreach (string s in readFile)
          {
            pageId = 0;
            int controlId = 0;
            int languageId = 0;
            string caption = string.Empty;
            int translationTypeValue = 0;
            TranslationType translationType = TranslationType.CaptionMl;
            bool goNext = false;

            if (translationTypeExp.Match(s).Success)
            {
              translationTypeMatche = translationTypeExp.Matches(s)[translationTypeExp.Matches(s).Count - 1];

              translationTypeValue = Convert.ToInt32(translationTypeMatche.Result("${translationType}"), CultureInfo.InvariantCulture);

              switch (translationTypeValue)
              {
                case 8629:
                case 55242:
                  translationType = TranslationType.CaptionMl;
                  break;
                case 8631:
                  translationType = TranslationType.ToolTip;
                  break;
                case 8632:
                  translationType = TranslationType.OptionCaptionMl;
                  break;
                case 26171:
                  translationType = TranslationType.TextVariable;
                  break;
                default:
                  goNext = true;
                  break;
              }

              if (!goNext)
              {
                if (captionExp.Match(s).Success)
                {
                  caption = captionExp.Match(s).Result("${caption}");

                  if (languageIdExp.Match(s).Success)
                  {
                    languageId = Convert.ToInt32(languageIdExp.Match(s).Result("${languageId}"), CultureInfo.InvariantCulture);

                    if (pageIdExp.Match(s).Success)
                    {
                      pageId = Convert.ToInt32(pageIdExp.Match(s).Result("${pagelId}"), CultureInfo.InvariantCulture);

                      controlId = GetControlIdFromTranslation(controlIdExp, actionIdExp, s, controlId);

                      Dictionary<TranslationType, Dictionary<int, string>> pageWithTranslationType;
                      if (translations.TryGetValue(pageId, out pageWithTranslationType))
                      {
                        Dictionary<int, string> controlsWithCaptions = new Dictionary<int, string>();
                        if (pageWithTranslationType.TryGetValue(translationType, out controlsWithCaptions))
                        {
                          string captionValue = string.Empty;
                          if (controlsWithCaptions.TryGetValue(controlId, out captionValue))
                          {
                            if (translationType == TranslationType.TextVariable)
                            {
                              captionValue = String.Format(CultureInfo.InvariantCulture, "{0};{1}", captionValue, PrepareCaptionValue(pageId, languageId, caption, translationType));
                            }
                            else
                            {
                              captionValue = String.Format(CultureInfo.InvariantCulture, "{0};\r\n{1}", captionValue, PrepareCaptionValue(pageId, languageId, caption, translationType));
                            }

                            controlsWithCaptions[controlId] = captionValue;
                          }
                          else
                          {
                            captionValue = PrepareCaptionValue(pageId, languageId, caption, translationType);
                            controlsWithCaptions.Add(controlId, captionValue);
                          }
                        }
                        else
                        {
                          controlsWithCaptions = new Dictionary<int, string>();
                          string captionValue = PrepareCaptionValue(pageId, languageId, caption, translationType);
                          controlsWithCaptions.Add(controlId, captionValue);

                          pageWithTranslationType.Add(translationType, controlsWithCaptions);
                        }
                      }
                      else
                      {
                        Dictionary<int, string> controlsWithCaptions = new Dictionary<int, string>();
                        string captionValue = PrepareCaptionValue(pageId, languageId, caption, translationType);
                        controlsWithCaptions.Add(controlId, captionValue);

                        pageWithTranslationType = new Dictionary<TranslationType, Dictionary<int, string>>();
                        pageWithTranslationType.Add(translationType, controlsWithCaptions);

                        translations.Add(pageId, pageWithTranslationType);
                      }
                    }
                  }
                }
              }
            }
          }

          fileState = TranslationFileStates.Read;
        }
        catch (Exception ex)
        {
          fileState = TranslationFileStates.NotReady;
          TransformationLog.GenericLogEntry(Resources.ImportingLocalizedStrings, LogCategory.Warning, pageId, "Check translation file");
          TransformationLog.WriteErrorToLogFile(ex, pageId, LogCategory.Warning);
        }
      }
    }

    private static int GetControlIdFromTranslation(Regex controlIdExp, Regex actionIdExp, string s, int controlId)
    {
      if (actionIdExp.Match(s).Success)
      {
        /* SE bug 58068. To get correct Control ID in case like this:
         * N9060-C1-P8629-A1033-L999:For Release
         * N9060-C1-P8629-A2055-L999:Freizugeben
         * N9060-C1-P55242-G3-P8629-A1033-L999:New Sales Quote
         * N9060-C1-P55242-G3-P8629-A2055-L999:Neue Verkaufsofferte */
        controlId = Convert.ToInt32(actionIdExp.Match(s).Result("${actionId}"), CultureInfo.InvariantCulture);
      }
      else
      {
        if (controlIdExp.Match(s).Success)
        {
          controlId = Convert.ToInt32(controlIdExp.Match(s).Result("${controlId}"), CultureInfo.InvariantCulture);
        }
      }

      return controlId;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "WriteAbort method writes all types of exceptions to log")]
    private static string PrepareCaptionValue(int pageId, int langId, string captionValue, TranslationType translationType)
    {
      if (langId == 0)
      {
        return string.Empty;
      }

      try
      {
        string lang;
        if (langId == 1034)
        {
          lang = "ESP"; // Otherwise it will be ESN
        }
        else
        {
          CultureInfo myCItrad = new CultureInfo(langId, false);
          lang = myCItrad.ThreeLetterWindowsLanguageName;
        }

        if (translationType == TranslationType.TextVariable)
        {
          Regex textConstWithDoubleQuotesExp = new System.Text.RegularExpressions.Regex(@"\%\d*\s*=\s*\%", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
          if (textConstWithDoubleQuotesExp.Match(captionValue).Success)
          {
            captionValue = @"""" + captionValue + @"""";
          }
        }

        return String.Format(CultureInfo.InvariantCulture, "{0}={1}", lang, captionValue);
      }
      catch
      {
        string logStr = String.Format(CultureInfo.InvariantCulture, Resources.IncorrectLanguageId, langId);
        TransformationLog.GenericLogEntry(logStr, LogCategory.Warning, pageId, "Check translation file");
        return string.Empty;
      }
    }

    /// <summary>
    /// It will return CaptionML with all languages founded in input translation file if it’s possible to found CaptionML for PageId/ControlId pair. 
    /// </summary>
    /// <param name="pageId">page ID for search</param>
    /// <param name="controlId">controlId==0 will represent page CaptionML</param>
    /// <param name="translationType">translation for: CaptionML, OptionCaptionML, TextVariable</param>
    /// <returns>CaptionML if PageId/ControlId pair exists. Otherwise – empty string.</returns>
    internal static string GetCaptionML(int pageId, int controlId, TranslationType translationType)
    {
      ReadTranslationFile();
      string returnValue = string.Empty;

      Dictionary<TranslationType, Dictionary<int, string>> pageWithTranslationType;
      if (translations.TryGetValue(pageId, out pageWithTranslationType))
      {
        Dictionary<int, string> controlsWithCaptions = new Dictionary<int, string>();
        if (pageWithTranslationType.TryGetValue(translationType, out controlsWithCaptions))
        {
          controlsWithCaptions.TryGetValue(controlId, out returnValue);
        }
      }

      return returnValue;
    }

    /// <summary>
    /// It will populate available ML strings (CaptionML, OptionCaptionML, and ToolTip) but NOT TextConst.
    /// </summary>
    /// <param name="propertiesNode">Control/Action node to update</param>
    /// <param name="pageId">page ID for search</param>
    /// <param name="controlId">controlId==0 will represent page CaptionML</param>
    internal static void PopulateMlStrings(XmlNode propertiesNode, int pageId, int controlId)
    {
      if (propertiesNode == null)
      {
        return;
      }

      if (!(propertiesNode.Name == "Properties" || propertiesNode.Name == "Action" || propertiesNode.Name == "Separator"))
      {
        return;
      }

      string captionMl = GetCaptionML(pageId, controlId, TranslationType.CaptionMl);
      if (!string.IsNullOrEmpty(captionMl))
      {
        XmlUtility.UpdateNodeInnerText(propertiesNode, "CaptionML", captionMl);
      }

      string optionCaptionMl = GetCaptionML(pageId, controlId, TranslationType.OptionCaptionMl);
      if (!string.IsNullOrEmpty(optionCaptionMl))
      {
        XmlUtility.UpdateNodeInnerText(propertiesNode, "OptionCaptionML", optionCaptionMl);
      }

      string toolTip = GetCaptionML(pageId, controlId, TranslationType.ToolTip);
      if (!string.IsNullOrEmpty(toolTip))
      {
        XmlUtility.UpdateNodeInnerText(propertiesNode, "ToolTipML", toolTip);
      }
    }
  }
}