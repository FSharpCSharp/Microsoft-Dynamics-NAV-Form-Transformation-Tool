//--------------------------------------------------------------------------
// <copyright file="DeleteElements.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>?abc?</summary>
//--------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

namespace Microsoft.Dynamics.Nav.Tools.FormTransformation
{
  /// <summary>
  /// ?abc?
  /// </summary>
  internal static class DeleteElements
  {
    /// <summary>
    /// ?abc?
    /// </summary>
    public static void Start()
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      if (metaDataDocMgt.DeleteElementsDoc == null)
      {
        return;
      }

      XmlNodeList pageNodeList = metaDataDocMgt.DeleteElementsDoc.SelectNodes(@"./a:DeletePageElements/a:Page[@ID='" + metaDataDocMgt.GetCurrentPageId + "']", metaDataDocMgt.XmlNamespaceMgt);
      foreach (XmlNode pageNode in pageNodeList)
      {
        XmlNodeList nodesToBeDeleted = pageNode.SelectNodes(@"./a:Element/@ID", metaDataDocMgt.XmlNamespaceMgt);
        foreach (XmlNode nodeToBeDeleted in nodesToBeDeleted)
        {
          XmlNode idToBeDeleted = metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(".//a:ID[../a:ID='" + nodeToBeDeleted.Value + "']", metaDataDocMgt.XmlNamespaceMgt);
          if (idToBeDeleted != null)
          {
            if (idToBeDeleted.ParentNode.Name == "Properties")
            {
              /* TODO Refactor this into XPATh */
              idToBeDeleted.ParentNode.ParentNode.ParentNode.RemoveChild(idToBeDeleted.ParentNode.ParentNode);
            }
            else
            {
              idToBeDeleted.ParentNode.ParentNode.RemoveChild(idToBeDeleted.ParentNode);
            }
          }
        }
      }
    }
  }
}
