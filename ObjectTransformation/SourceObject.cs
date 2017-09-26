//--------------------------------------------------------------------------
// <copyright file="SourceObject.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>?abc?</summary>
//--------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Microsoft.Dynamics.Nav.Tools.FormTransformation
{
  /// <summary>
  /// ?abc?
  /// </summary>
  internal static class SourceObject
  {
    #region Public static method

    /// <summary>
    /// ?abc?
    /// </summary>
    public static void CreateSourceObject()
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;

      XmlNode formSourceObjectNode = metaDataDocMgt.XmlDocument.CreateNode(XmlNodeType.Element, "SourceObject", metaDataDocMgt.XmlNamespace);
      XmlNode formPropertyNode = metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(@"./a:Properties", metaDataDocMgt.XmlNamespaceMgt);
      if (formPropertyNode != null)
      {
        metaDataDocMgt.XmlCurrentFormNode.InsertAfter(formSourceObjectNode, formPropertyNode);
      }
      else
      {
        metaDataDocMgt.XmlCurrentFormNode.AppendChild(formSourceObjectNode);
      }

      XmlUtility.MoveNode(metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(@"./a:Properties/a:SourceTable", metaDataDocMgt.XmlNamespaceMgt), formSourceObjectNode);
      XmlUtility.MoveNode(metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(@"./a:Properties/a:SourceTableView", metaDataDocMgt.XmlNamespaceMgt), formSourceObjectNode);
      XmlUtility.MoveNode(metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(@"./a:Properties/a:InsertAllowed", metaDataDocMgt.XmlNamespaceMgt), formSourceObjectNode);
      XmlUtility.MoveNode(metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(@"./a:Properties/a:ModifyAllowed", metaDataDocMgt.XmlNamespaceMgt), formSourceObjectNode);
      XmlUtility.MoveNode(metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(@"./a:Properties/a:InsertAllowed", metaDataDocMgt.XmlNamespaceMgt), formSourceObjectNode);
      XmlUtility.MoveNode(metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(@"./a:Properties/a:DeleteAllowed", metaDataDocMgt.XmlNamespaceMgt), formSourceObjectNode);
      XmlUtility.MoveNode(metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(@"./a:Properties/a:DelayedInsert", metaDataDocMgt.XmlNamespaceMgt), formSourceObjectNode);
      XmlUtility.MoveNode(metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(@"./a:Properties/a:MultipleNewLines", metaDataDocMgt.XmlNamespaceMgt), formSourceObjectNode);
      XmlUtility.MoveNode(metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(@"./a:Properties/a:SourceTableTemporary", metaDataDocMgt.XmlNamespaceMgt), formSourceObjectNode);
      XmlUtility.MoveNode(metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(@"./a:Properties/a:DataCaptionFields", metaDataDocMgt.XmlNamespaceMgt), formSourceObjectNode);
      XmlUtility.MoveNode(metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(@"./a:Properties/a:AutoSplitKey", metaDataDocMgt.XmlNamespaceMgt), formSourceObjectNode);
    }
    #endregion
  }
}
