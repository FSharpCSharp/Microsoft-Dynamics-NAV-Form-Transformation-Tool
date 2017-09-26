//--------------------------------------------------------------------------
// <copyright file="IgnoreForms.cs" company="Microsoft">
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
  public static class IgnoreForms
  {
    /// <summary>
    /// ?abc?
    /// </summary>
    public static void RemoveIgnoredForms()
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      if (metaDataDocMgt.IgnorePagesDoc == null)
      {
        return;
      }

      XmlNodeList pageIDList = metaDataDocMgt.IgnorePagesDoc.SelectNodes(@"./a:IgnorePages/a:Page/@ID", metaDataDocMgt.XmlNamespaceMgt);

      foreach (XmlNode pageID in pageIDList)
      {
        RemoveIgnoredForm(pageID.Value);
      }
    }

    public static void RemoveReplacedForms()
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      if (metaDataDocMgt.RenumberPagesDoc == null)
      {
        return;
      }

      XmlNodeList pageIDList = metaDataDocMgt.IgnorePagesDoc.SelectNodes(@"./a:MovePages/a:Page/@destinationID", metaDataDocMgt.XmlNamespaceMgt);

      foreach (XmlNode pageID in pageIDList)
      {
        if (RenumberPages.PageWillBeReplaced(pageID.Value))
          RemoveIgnoredForm(pageID.Value);
      }
    }

    public static void RemoveFormsWithMatrixControls()
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      XmlNodeList nodeList = metaDataDocMgt.XmlDocument.SelectNodes(@"./a:Objects/a:Form", metaDataDocMgt.XmlNamespaceMgt);
      foreach (XmlNode formNode in nodeList)
      {
        XmlNode ControlIDNode = formNode.SelectSingleNode(".//a:Control[.//a:Controltype='MatrixBox']", metaDataDocMgt.XmlNamespaceMgt);
        if (ControlIDNode != null)
          RemoveIgnoredForm(formNode.Attributes.GetNamedItem("ID").FirstChild.Value);
      }
    }

    private static void RemoveIgnoredForm(String pageID)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      XmlNode formToBeIgnored = metaDataDocMgt.XmlDocument.SelectSingleNode("./a:Objects/a:Form[@ID =" + pageID + "]", metaDataDocMgt.XmlNamespaceMgt);
      if (formToBeIgnored == null)
        return;

      XmlNode codeTriggerNode = formToBeIgnored.SelectSingleNode(@".//a:Code", metaDataDocMgt.XmlNamespaceMgt);
      Boolean restart = true;
      Int32 startAfter = 0;

      while (restart)
      {
        restart = false;
        Boolean inProcedure = false;
        Boolean inCodeBody = false;           
        Int32 beginPosition = 0;

        SimpleNAVCodeParser sNAVCodeParser = new SimpleNAVCodeParser();
        foreach (NormalizedCode normNAVCode in sNAVCodeParser.GenerateNormalisedCode(codeTriggerNode.InnerText))
        {
          if (inProcedure)
          {
            if (inCodeBody)
            {
              if ((normNAVCode.xPos == 2) && (normNAVCode.Token == "END"))
              {
                inProcedure = false;
                inCodeBody = false;

                String beginningPart = codeTriggerNode.InnerText.Substring(0, beginPosition + 6);
                String endPart = codeTriggerNode.InnerText.Substring(normNAVCode.position - 1);
                codeTriggerNode.RemoveAll();

                XmlCDataSection data = metaDataDocMgt.XmlDocument.CreateCDataSection(
                  String.Format(CultureInfo.InvariantCulture, "{0}{1}", beginningPart, endPart));
                codeTriggerNode.AppendChild(data);

                startAfter = beginPosition;
                beginPosition = 0;
                inProcedure = false;
                inCodeBody = false;
                restart = true;
                break;
              }
            }
            else
            {
              if ((normNAVCode.xPos == 2) && (normNAVCode.Token == "BEGIN"))
              {
                inCodeBody = true;
                beginPosition = normNAVCode.position;
              }
            }
          }
          else
          {
            if ((normNAVCode.position >= startAfter) && (normNAVCode.xPos == 2) && ((normNAVCode.Token == "LOCAL") ||
                                                                                    (normNAVCode.Token == "PROCEDURE") ||
                                                                                    (normNAVCode.Token == "EVENT")))
            {
              inProcedure = true;
            }
          }
        }  
      }

      XmlNode controlNode = formToBeIgnored.SelectSingleNode(@".//a:Controls", metaDataDocMgt.XmlNamespaceMgt);
      controlNode.ParentNode.ReplaceChild(XmlUtility.CreateXmlElement("Controls", null), controlNode);

      XmlNode triggerNode = formToBeIgnored.SelectSingleNode(@".//a:Triggers", metaDataDocMgt.XmlNamespaceMgt);
      triggerNode.ParentNode.ReplaceChild(XmlUtility.CreateXmlElement("Triggers", null), triggerNode);

      // TransformationLog.IgnoreFormsLog(Convert.ToInt32(pageID.Value, CultureInfo.InvariantCulture));
      String logStr = Resources.FormIgnored;
      TransformationLog.GenericLogEntry(logStr, LogCategory.IgnoreForms, pageID, null);
    }
  }
}
