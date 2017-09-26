//--------------------------------------------------------------------------
// <copyright file="RenumberPages.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>?abc?</summary>
//--------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.Tools.FormTransformation
{
  /// <summary>
  /// ?abc?
  /// </summary>
  public static class RenumberPages
  {
    public static Boolean PageWillBeReplaced(String pageId)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      XmlNode pageNode = metaDataDocMgt.RenumberPagesDoc.SelectSingleNode(@"./a:MovePages/a:Page[.//@destinationID='" + pageId + "']", metaDataDocMgt.XmlNamespaceMgt);
      // XmlNode pageNode = metaDataDocMgt.RenumberPagesDoc.SelectSingleNode(@"./a:MovePages/a:Page", metaDataDocMgt.XmlNamespaceMgt);
      if (pageNode == null)
        return false;
      XmlNode nodeToRenumber = metaDataDocMgt.XmlDocument.SelectSingleNode(@"./a:Objects/a:Page[./@ID='" + pageNode.Attributes["ID"].Value + "']", metaDataDocMgt.XmlNamespaceMgt);
      if (nodeToRenumber == null)
        return false;
      return true;
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    public static void Start()
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      if (metaDataDocMgt.RenumberPagesDoc != null)
      {
        XmlNodeList pageNodeList = metaDataDocMgt.RenumberPagesDoc.SelectNodes(@"./a:MovePages/a:Page", metaDataDocMgt.XmlNamespaceMgt);
        foreach (XmlNode pageNode in pageNodeList)
        {
          String pageID = pageNode.Attributes["ID"].Value;
          XmlNode nodeToRenumber = metaDataDocMgt.XmlDocument.SelectSingleNode("./a:Objects/a:Page[./@ID='" + pageID + "']", metaDataDocMgt.XmlNamespaceMgt);
          if (nodeToRenumber != null)
          {
            String destinationID = pageNode.Attributes["destinationID"].Value;
            XmlNode existingNode = metaDataDocMgt.XmlDocument.SelectSingleNode("./a:Objects/a:Page[./@ID='" + destinationID + "']", metaDataDocMgt.XmlNamespaceMgt);
            nodeToRenumber.Attributes["ID"].Value = destinationID;
            nodeToRenumber.Attributes["Name"].Value = pageNode.Attributes["destinationName"].Value;
            if (existingNode != null)
            {
              XmlNode goodCaptionML = existingNode.SelectSingleNode("./a:Properties/a:CaptionML", metaDataDocMgt.XmlNamespaceMgt);
              if (goodCaptionML != null)
              {
                XmlNode originalCaptionML = nodeToRenumber.SelectSingleNode("./a:Properties/a:CaptionML", metaDataDocMgt.XmlNamespaceMgt);
                if (originalCaptionML != null)
                {
                  originalCaptionML.InnerText = goodCaptionML.InnerText;
                }
                else
                {
                  XmlNode tmpNode = nodeToRenumber.SelectSingleNode("./a:Properties", metaDataDocMgt.XmlNamespaceMgt);
                  if (tmpNode != null)
                  {
                    tmpNode.AppendChild(XmlUtility.CreateXmlElement("CaptionML", goodCaptionML.InnerText));
                  }
                }
              }

              existingNode.ParentNode.RemoveChild(existingNode);
            }
          }
        }
      }

      if (!System.IO.File.Exists(UserSetupManagement.Instance.TranslationFile))
      {
        return;
      }

      Console.WriteLine(Resources.ImportingLocalizedStrings);
      XmlNodeList nodeList = metaDataDocMgt.XmlDocument.SelectNodes(@"./a:Objects/a:Page", metaDataDocMgt.XmlNamespaceMgt);
      foreach (XmlNode formNode in nodeList)
      {
        AddMlCaptions(formNode);

        if (AddMultiLanguageSupport.FileState == AddMultiLanguageSupport.TranslationFileStates.NotReady)
        {
          return;
        }
      }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "WriteAbort method writes all types of exceptions to log")]
    private static void AddMlCaptions(XmlNode page)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      XmlNodeList idList = page.SelectNodes(@".//a:ID", metaDataDocMgt.XmlNamespaceMgt);
      int destinationID = Convert.ToInt32(page.Attributes["ID"].Value, CultureInfo.InvariantCulture);
      foreach (XmlNode id in idList)
      {
        if (!string.IsNullOrEmpty(id.InnerText))
        {
          try
          {
            AddMultiLanguageSupport.PopulateMlStrings(id.ParentNode, destinationID, Convert.ToInt32(id.InnerText, CultureInfo.InvariantCulture));
          }
          catch (Exception ex)
          {
            string log = String.Format(CultureInfo.InvariantCulture, Resources.CannotGetMlCaption, "control ", id.InnerText);
            TransformationLog.GenericLogEntry(log, LogCategory.Warning, destinationID, "Ignore");
            TransformationLog.WriteErrorToLogFile(ex, destinationID, LogCategory.Warning);
          }
        }
      }

      XmlNode pageCaption = page.SelectSingleNode(@"./a:Properties", metaDataDocMgt.XmlNamespaceMgt);
      if (pageCaption != null)
      {
        try
        {
          AddMultiLanguageSupport.PopulateMlStrings(pageCaption, destinationID, 0);
        }
        catch (Exception ex)
        {
          string log = String.Format(CultureInfo.InvariantCulture, Resources.CannotGetMlCaption, "page", string.Empty);
          TransformationLog.GenericLogEntry(log, LogCategory.Warning, destinationID, "Ignore");
          TransformationLog.WriteErrorToLogFile(ex, destinationID, LogCategory.Warning);
        }
      }

      XmlNode code = page.SelectSingleNode(@"./a:Code", metaDataDocMgt.XmlNamespaceMgt);
      if (code == null)
      {
        return;
      }

      try
      {
        System.Text.RegularExpressions.Regex textVariableExp = new System.Text.RegularExpressions.Regex(@"\@(?<varId>\d+)\s*\:\s*TextConst\s*\'", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (!textVariableExp.Match(code.InnerText).Success)
        {
          return;
        }

        string[] codeArr = code.InnerText.Split(new char[] { '\r' });
        System.Text.RegularExpressions.Match textVariableMatch;
        int textVarId;
        string textVarNewCaption;
        for (int i = 0; i < codeArr.Length; i++)
        {
          textVariableMatch = textVariableExp.Match(codeArr[i]);
          if (textVariableMatch.Success)
          {
            textVarId = Convert.ToInt32(textVariableMatch.Result("${varId}"), CultureInfo.InvariantCulture);
            textVarNewCaption = AddMultiLanguageSupport.GetCaptionML(destinationID, textVarId, AddMultiLanguageSupport.TranslationType.TextVariable);
            if (!string.IsNullOrEmpty(textVarNewCaption))
            {
              textVarNewCaption = textVarNewCaption.Replace("'", "''");
              codeArr[i] = codeArr[i].Remove(textVariableMatch.Length + textVariableMatch.Index) + textVarNewCaption + "';";
            }
          }
        }

        XmlNode codeNode = XmlUtility.CreateXmlElement("Code");
        codeNode.AppendChild(XmlUtility.CreateCDataSection(string.Join("\r", codeArr)));
        code.ParentNode.AppendChild(codeNode);
        code.ParentNode.RemoveChild(code);
      }
      catch (Exception ex)
      {
        string log = String.Format(CultureInfo.InvariantCulture, Resources.CannotGetMlCaption, "code", string.Empty);
        TransformationLog.GenericLogEntry(log, LogCategory.Warning, destinationID, "Ignore");
        TransformationLog.WriteErrorToLogFile(ex, destinationID, LogCategory.Warning);
      }

    }

  }
}
