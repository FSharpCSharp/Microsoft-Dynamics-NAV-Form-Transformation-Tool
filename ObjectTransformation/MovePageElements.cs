//--------------------------------------------------------------------------
// <copyright file="MovePageElements.cs" company="Microsoft">
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
  public static class MovePageElements
  {
    /// <summary>
    /// To work with not specific form but with all forms
    /// </summary>
    public enum NotFormsId
    {
      /// <summary>
      /// Controls on trendscape forms will be moved to the top tab
      /// </summary>
      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Trendscape")]
      TrendscapeControls = -7,

      /// <summary>
      /// To add Images as Big to Actions by CaptionML value
      /// </summary>
      AddPromotedIsBig = -6,

      /// <summary>
      /// To add Images to Actions by CaptionML value
      /// </summary>
      AddImages = -5,

      /// <summary>
      /// To add Shortcuts to Actions by CaptionML value
      /// </summary>
      AddShortcuts = -4,

      /// <summary>
      /// To move element to not deffault ActionGroup by it CaptionML value
      /// </summary>
      ActionGroup = -3,

      /// <summary>
      /// To move element to PromotedActions by it CaptionML value
      /// </summary>
      PromotedActions = -2,

      /// <summary>
      /// To process Shortcut keys update and remove
      /// </summary>
      ShortcutKeys = -1,

      /// <summary>
      /// ?abc?
      /// </summary>
      None = 0
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    public static void Start()
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      if (metaDataDocMgt.MoveElementsDoc == null)
      {
        return;
      }

      XmlNodeList pageNodeList = metaDataDocMgt.MoveElementsDoc.SelectNodes(@"./a:MovePageElements/a:Page", metaDataDocMgt.XmlNamespaceMgt);

      foreach (XmlNode pageNode in pageNodeList)
      {
        String pageID = pageNode.Attributes["ID"].Value;
        metaDataDocMgt.XmlCurrentFormNode = metaDataDocMgt.XmlDocument.SelectSingleNode("./a:Objects/a:Page[@ID='" + pageID + "']", metaDataDocMgt.XmlNamespaceMgt);

        if (metaDataDocMgt.XmlCurrentFormNode == null)
        {
          continue;
        }

        foreach (XmlNode pageElement in pageNode.ChildNodes)
        {
          if (pageElement.Name == "ElementToType")
          {
            String elementID = pageElement.Attributes["ID"].Value;
            String destinationType = pageElement.Attributes["destinationType"].Value;

            XmlNode idToBeMoved = metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(".//a:ID[../a:ID='" + elementID + "']", metaDataDocMgt.XmlNamespaceMgt);
            if (idToBeMoved != null)
            {
              XmlNode nodeToMove = null;
              if (idToBeMoved.ParentNode.Name == "Properties")
              {
                nodeToMove = idToBeMoved.ParentNode.ParentNode;
                if (nodeToMove != null)
                {
                  XmlNode destinationNode = metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(".//a:" + destinationType, metaDataDocMgt.XmlNamespaceMgt);
                  if (destinationNode == null)
                  {
                    if ((nodeToMove.Name == "Control") || (nodeToMove.Name == "Group"))
                    {
                      destinationNode = PageControls.GetPageControlNode(destinationType);
                    }
                    else
                    {
                      destinationNode = PageActions.GetPageActionNode(destinationType);
                    }
                  }

                  destinationNode.AppendChild(nodeToMove);
                }
              }
              else /* Action */
              {
                nodeToMove = idToBeMoved.ParentNode;
                if (nodeToMove != null)
                {
                  XmlNode destinationNode = metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(".//a:" + destinationType, metaDataDocMgt.XmlNamespaceMgt);
                  if (destinationNode == null)
                  {
                    destinationNode = PageActions.GetPageActionNode(destinationType);
                  }

                  XmlNode destinationElement = destinationNode.ParentNode;
                  destinationElement.AppendChild(nodeToMove);
                }
              }
            }

            /* TODO append ID node from metaPage to metaPage destinationType - if type not exist then create it. */
          }

          if (pageElement.Name == "ElementToID")
          {
            String elementID = pageElement.Attributes["ID"].Value;
            String destinationID = pageElement.Attributes["destinationID"].Value;

            XmlNode idToBeMoved = metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(".//a:ID[../a:ID='" + elementID + "']", metaDataDocMgt.XmlNamespaceMgt);
            if (idToBeMoved != null)
            {
              XmlNode nodeToMove = null;
              if (idToBeMoved.ParentNode.Name == "Properties")
              {
                nodeToMove = idToBeMoved.ParentNode.ParentNode;
                if (nodeToMove != null)
                {
                  XmlNode destinationIDNode = metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(".//a:ID[../a:ID='" + destinationID + "']", metaDataDocMgt.XmlNamespaceMgt);
                  XmlNode destinationElementNode = destinationIDNode.ParentNode.ParentNode;
                  destinationElementNode.AppendChild(nodeToMove);
                }
              }
              else /* Action */
              {
                nodeToMove = idToBeMoved.ParentNode;
                if (nodeToMove != null)
                {
                  XmlNode destinationIDNode = metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(".//a:ID[../a:ID='" + destinationID + "']", metaDataDocMgt.XmlNamespaceMgt);
                  XmlNode destinationElement = destinationIDNode.ParentNode;
                  destinationElement.AppendChild(nodeToMove);
                }
              }
            }
          }

          AssignImportance(pageElement, metaDataDocMgt);
        }
      }
    }

    private static void AssignImportance(XmlNode pageElement, MetadataDocumentManagement metaDataDocMgt)
    {
      if (pageElement.Name == "PromotedField")
      {
        String elementID = pageElement.Attributes["ID"].Value;
        XmlNode idToBeMoved = metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode("./a:Controls//a:Field/a:Properties[./a:ID='" + elementID + "']", metaDataDocMgt.XmlNamespaceMgt);
        if (idToBeMoved != null)
        {
          XmlNode importanceNode = idToBeMoved.SelectSingleNode("./a:Importance", metaDataDocMgt.XmlNamespaceMgt);
          if (importanceNode != null)
          {
            importanceNode.InnerText = "Promoted";
          }
          else
          {
            idToBeMoved.AppendChild(XmlUtility.CreateXmlElement("Importance", "Promoted"));
          }
        }
      }

      if (pageElement.Name == "AdditionalField")
      {
          String elementID = pageElement.Attributes["ID"].Value;
          XmlNode idToBeMoved = metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode("./a:Controls//a:Field/a:Properties[./a:ID='" + elementID + "']", metaDataDocMgt.XmlNamespaceMgt);
          if (idToBeMoved != null)
          {
              XmlNode importanceNode = idToBeMoved.SelectSingleNode("./a:Importance", metaDataDocMgt.XmlNamespaceMgt);
              if (importanceNode != null)
              {
                  importanceNode.InnerText = "Additional";
              }
              else
              {
                  idToBeMoved.AppendChild(XmlUtility.CreateXmlElement("Importance", "Additional"));
              }
          }
      }
    }
  }
}