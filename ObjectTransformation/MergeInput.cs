//--------------------------------------------------------------------------
// <copyright file="MergeInput.cs" company="Microsoft">
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
  #region public types
  /// <summary>
  /// ?abc?
  /// </summary>
  public enum SectionType : int
  {
    /// <summary>
    /// ?abc?
    /// </summary>
    Actions = 0,

    /// <summary>
    /// ?abc?
    /// </summary>
    Controls = 1
  }

  #endregion

  /// <summary>
  /// ?abc?
  /// </summary>
  public static class MergeInput
  {
    /// <summary>
    /// ?abc?
    /// </summary>
    public static void StartMerging()
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      if (metaDataDocMgt.InsertElementsDoc == null)
      {
        return;
      }

      XmlNodeList pageList = metaDataDocMgt.InsertElementsDoc.SelectNodes(@"./a:TransformPages/a:Page[@ID=" + metaDataDocMgt.GetCurrentPageId + "]", metaDataDocMgt.XmlNamespaceMgt);

      foreach (XmlNode page in pageList)
      {
        PageSectionDelegation(page);
      }
    }

    private static void PageSectionDelegation(XmlNode currentNode)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;

      foreach (XmlNode section in currentNode.ChildNodes)
      {
        switch (section.Name)
        {
          case "Properties":
            MergeSimpleSection(section, metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode("./a:" + section.Name, metaDataDocMgt.XmlNamespaceMgt));
            break;

          case "SourceObject":
            MergeSimpleSection(section, metaDataDocMgt.XmlCurrentFormNode);
            break;

          case "Triggers":
            MergeSimpleSection(section, metaDataDocMgt.XmlCurrentFormNode);
            break;

          case "Actions":
            MergeStructure(section, SectionType.Actions);
            break;

          case "Controls":
            MergeStructure(section, SectionType.Controls);
            break;

          case "Code":
            MergeSimpleSection(section, metaDataDocMgt.XmlCurrentFormNode);
            break;
        }
      }
    }

    private static void MergeSimpleSection(XmlNode sourceNode, XmlNode destinationNode)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      foreach (XmlNode childNode in sourceNode.ChildNodes)
      {
        XmlNode propertyNode = destinationNode.SelectSingleNode("./a:" + childNode.Name, metaDataDocMgt.XmlNamespaceMgt);

        XmlNode newNode = metaDataDocMgt.XmlDocument.ImportNode(childNode, true);
        if (propertyNode == null)
        {
          destinationNode.AppendChild(newNode);
        }
        else
        {
          destinationNode.ReplaceChild(newNode, propertyNode);
        }
      }
    }

    #region Structure

    private static void MergeStructure(XmlNode sectionNode, SectionType sectionType)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      if (sectionType == SectionType.Actions)
      {
        foreach (XmlNode childNode in sectionNode.ChildNodes)
        {
          /* TODO  !!! TEMPORARY !!!  we should change TransformPages.xml instead! */
          XmlNode destinationNode = metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode("./a:Actions/a:" + childNode.Name, metaDataDocMgt.XmlNamespaceMgt);
          if (destinationNode == null)
          {
            XmlNode actionsNode = metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode("./a:Actions", metaDataDocMgt.XmlNamespaceMgt);
            XmlUtility.InsertNodeWithPropertyChild(actionsNode, childNode.Name, metaDataDocMgt.CalcId(null, null, childNode.Name) /*metaDataDocMgt.GetNewId */);
            destinationNode = metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode("./a:Actions/a:" + childNode.Name, metaDataDocMgt.XmlNamespaceMgt);
          }

          MergeComplexStructure(childNode, destinationNode);
        }
      }

      if (sectionType == SectionType.Controls)
      {
        foreach (XmlNode childNode in sectionNode.ChildNodes)
        {
          XmlNode destinationNode = metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode("./a:Controls/a:" + childNode.Name, metaDataDocMgt.XmlNamespaceMgt);
          if (destinationNode == null)
          {
            XmlNode controlsNode = metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode("./a:Controls", metaDataDocMgt.XmlNamespaceMgt);
            XmlUtility.InsertNodeWithPropertyChild(controlsNode, childNode.Name, metaDataDocMgt.CalcId(null, null, childNode.Name) /* metaDataDocMgt.GetNewId */);              
            destinationNode = metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode("./a:Controls/a:" + childNode.Name, metaDataDocMgt.XmlNamespaceMgt);
          }

          MergeComplexStructure(childNode, destinationNode);
        }
      }
    }

    private static void InsertIDAsChildInNodeList(XmlNodeList nodeList, String where)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      foreach (XmlNode node in nodeList)
      { 
 
        string newId = string.Empty;
        XmlNode fixedIdNode = node.SelectSingleNode("./a:FixedID", metaDataDocMgt.XmlNamespaceMgt);
        if (fixedIdNode != null)
        {
          newId = fixedIdNode.InnerText;
          RemoveNodeFromParent(fixedIdNode);
        }
        else
        {
          if (node.ParentNode.Name.Equals("Part"))
          {
            switch (node.SelectSingleNode("./a:PartType", metaDataDocMgt.XmlNamespaceMgt).InnerText)
            {
              case "Page":
                newId = metaDataDocMgt.CalcId(GetProperty(node.ParentNode, "NewID"), node.SelectSingleNode("./a:PagePartID", metaDataDocMgt.XmlNamespaceMgt).InnerText, where).ToString(CultureInfo.InvariantCulture);
                break;
              case "System":
                newId = metaDataDocMgt.CalcId(GetProperty(node.ParentNode, "NewID"), node.SelectSingleNode("./a:SystemPartID", metaDataDocMgt.XmlNamespaceMgt).InnerText, where).ToString(CultureInfo.InvariantCulture);
                break;
              case "Chart":
                newId = metaDataDocMgt.CalcId(GetProperty(node.ParentNode, "NewID"), node.SelectSingleNode("./a:ChartPartID", metaDataDocMgt.XmlNamespaceMgt).InnerText, where).ToString(CultureInfo.InvariantCulture);
                break;
            }
          }
          else
          {
            newId = newId + 0;
          }

          //XmlNode partNode = node.SelectSingleNode(@"./a:Properties/a:Visible", metaDataDocMgt.XmlNamespaceMgt);                                                                      
          //newId = metaDataDocMgt.GetNewId.ToString(CultureInfo.InvariantCulture);
        }

        node.AppendChild(XmlUtility.CreateXmlElement("ID", newId));
      }
    }

    private static String GetProperty(XmlNode controlNode, String propertyName)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      XmlNode propNode = controlNode.SelectSingleNode(@"./a:Properties/a:" + propertyName, metaDataDocMgt.XmlNamespaceMgt);
      if (propNode == null)
        return null;
      return propNode.LastChild.InnerText;
    }

    private static void MergeComplexStructure_Action(XmlNode childNode, XmlNode destinationNode)
    {
      if (childNode == null)
      {
        return;
      }

      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      XmlNode actionIDNode = childNode.SelectSingleNode("./a:ID", metaDataDocMgt.XmlNamespaceMgt);

      if ((actionIDNode == null) || (String.IsNullOrEmpty(actionIDNode.InnerText)))
      {
        XmlNode captionMlNode = childNode.SelectSingleNode("./a:CaptionML", metaDataDocMgt.XmlNamespaceMgt);
        if (captionMlNode == null)
        {
          captionMlNode = childNode.FirstChild;
          string log = string.Format(CultureInfo.InvariantCulture, Resources.AddCaptionMl, childNode.InnerXml);
          TransformationLog.GenericLogEntry(log, LogCategory.Warning, metaDataDocMgt.GetCurrentPageId);
        }

        String newID =
          metaDataDocMgt.CalcId(GetProperty(captionMlNode.ParentNode, "NewID"),
            XmlUtility.GetCaption(captionMlNode.InnerText), captionMlNode.ParentNode.ParentNode.Name).ToString(CultureInfo.InvariantCulture);

        actionIDNode = metaDataDocMgt.XmlDocument.CreateNode(XmlNodeType.Element, "ID", metaDataDocMgt.XmlNamespace);
        actionIDNode.InnerText = newID;

        XmlNode newNode = metaDataDocMgt.XmlDocument.ImportNode(childNode, true);
        newNode.PrependChild(actionIDNode);
        destinationNode.AppendChild(newNode);
      }
      else
      {
        XmlNode destinationActionNode = destinationNode.SelectSingleNode("./a:" + childNode.Name + "[./a:ID = " + actionIDNode.InnerText + "]", metaDataDocMgt.XmlNamespaceMgt);
        if (destinationActionNode != null)
        {
          MergeSimpleSection(childNode, destinationActionNode);
        }
        else
        {
          /* TODO  !!! TEMPORARY !!!  we should change TransformPages.xml instead! */
          // Lets try to find it by ID:
          XmlNodeList actionListById = childNode.SelectNodes(@".//a:ID", metaDataDocMgt.XmlNamespaceMgt);
          if (actionListById.Count == 0)
          {
            /* Something is wrong in the transformation input file... */
            string logStr = string.Format(
              CultureInfo.InvariantCulture,
              Resources.IDNotFound,
              actionIDNode.InnerText);
            TransformationLog.GenericLogEntry(logStr, LogCategory.CheckInputFile, metaDataDocMgt.GetCurrentPageId);
            
            // break;
            return;
          }

          foreach (XmlNode actionById in actionListById)
          {
            string query = ".//a:Action[./a:ID = " + actionById.InnerText + "]";
            XmlNode destinationActionNodeById = destinationNode.ParentNode.SelectSingleNode(query, metaDataDocMgt.XmlNamespaceMgt);
            if (destinationActionNodeById != null)
            {
              MergeSimpleSection(actionById.ParentNode, destinationActionNodeById);
            }
            else
            {
              /* Something is wrong in the transformation input file... */
              string logStr = string.Format(
                CultureInfo.InvariantCulture,
                Resources.IDNotFound,
                actionById.InnerText);
              string tmpAddStr = string.Empty;
              XmlNode tmp = metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(".//a:ID[../a:ID = " + actionById.InnerText + "]", metaDataDocMgt.XmlNamespaceMgt);
              if (tmp != null)
              {
                tmpAddStr = tmp.ParentNode.ParentNode.InnerXml;
              }
              TransformationLog.GenericLogEntry(logStr, LogCategory.CheckInputFile, metaDataDocMgt.GetCurrentPageId, tmpAddStr);
            }
          }
        }
      }
    }

    private static void MergeComplexStructure(XmlNode sectionNode, XmlNode destinationNode)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      Int32 GroupCount = 0;
      foreach (XmlNode childNode in sectionNode.ChildNodes)
      {
        switch (childNode.Name)
        {
          case "Properties":
            XmlNode destinationPropertiesNode = destinationNode.SelectSingleNode("./a:" + childNode.Name, metaDataDocMgt.XmlNamespaceMgt);
            if (destinationPropertiesNode == null)
            {
              destinationPropertiesNode = metaDataDocMgt.XmlDocument.CreateNode(XmlNodeType.Element, childNode.Name, metaDataDocMgt.XmlNamespace);
              destinationNode.AppendChild(destinationPropertiesNode);
            }

            MergeSimpleSection(childNode, destinationPropertiesNode);
            break;
          case "Action":
            {
              MergeComplexStructure_Action(childNode, destinationNode);
              break;
            }       
          /*
          case "ID":
            {
              XmlNode destinationActionNode = destinationNode.ParentNode.SelectSingleNode(".//a:Action[./a:ID = " + childNode.InnerText + "]", metaDataDocMgt.XmlNamespaceMgt);
              if (destinationActionNode != null)
              {
                MergeSimpleSection(childNode.ParentNode, destinationActionNode);
              }
              else
              {
                string logStr = string.Format(
                  CultureInfo.InvariantCulture,
                  Resources.IDNotFound,
                  childNode.InnerText);
                TransformationLog.GenericLogEntry(logStr, LogCategory.CheckInputFile, metaDataDocMgt.GetCurrentPageID);
              }

              break;
            }
          */

          case "ActionGroup":
          case "Control":
          case "Group":
          case "Part":
          case "Field":
            XmlNode complexIDNode = childNode.SelectSingleNode("./a:Properties/a:ID", metaDataDocMgt.XmlNamespaceMgt);

            if ((complexIDNode == null) || (String.IsNullOrEmpty(complexIDNode.InnerText)))
            {
              complexIDNode = null;
              XmlNode newNode = metaDataDocMgt.XmlDocument.ImportNode(childNode, true);
              InsertIDAsChildInNodeList(newNode.SelectNodes(@".//a:Properties", metaDataDocMgt.XmlNamespaceMgt), sectionNode.Name);
              InsertIDAsChildInNodeList(newNode.SelectNodes(@".//a:Action", metaDataDocMgt.XmlNamespaceMgt), sectionNode.Name);
              if ((newNode.Name == "Group") || (newNode.Name == "Control") || (newNode.Name == "Field") || (newNode.Name == "Part") || (newNode.Name == "ActionGroup"))
              {
                if (newNode.SelectSingleNode("./a:Properties", metaDataDocMgt.XmlNamespaceMgt) == null)
                {
                  XmlNode propertiesNode = metaDataDocMgt.XmlDocument.CreateNode(XmlNodeType.Element, "Properties", metaDataDocMgt.XmlNamespace);
                  XmlNode idNode = metaDataDocMgt.XmlDocument.CreateNode(XmlNodeType.Element, "ID", metaDataDocMgt.XmlNamespace);
                  //idNode.InnerText = metaDataDocMgt.CalcID(null, null, sectionNode.Name);
                  //idNode.InnerText = metaDataDocMgt.GetNewId.ToString(CultureInfo.InvariantCulture); // not possible to generate id using content
                  idNode.InnerText = metaDataDocMgt.CalcId(null, (GroupCount++).ToString(CultureInfo.InvariantCulture), sectionNode.Name).ToString(CultureInfo.InvariantCulture);
                  propertiesNode.AppendChild(idNode);
                  newNode.PrependChild(propertiesNode);
                }
              }

              destinationNode.AppendChild(newNode);
            }
            else
            {
              XmlNode destinationActionGroupNode = destinationNode.SelectSingleNode("./a:" + childNode.Name + "[./a:Properties/a:ID = '" + complexIDNode.InnerText + "']", metaDataDocMgt.XmlNamespaceMgt);
              if (destinationActionGroupNode != null)
              {
                MergeComplexStructure(childNode, destinationActionGroupNode);
              }
              else
              {
                /* Something is wrong in the transformation input file... */
                string logStr = string.Format(
                  CultureInfo.InvariantCulture,
                  Resources.IDNotFound,
                  complexIDNode.InnerText);
                TransformationLog.GenericLogEntry(logStr, LogCategory.CheckInputFile, metaDataDocMgt.GetCurrentPageId);
              }
            }

            break;

          case "Triggers":
            XmlNode triggerNode = metaDataDocMgt.XmlDocument.ImportNode(childNode, true);
            destinationNode.AppendChild(triggerNode);
            break;
        }
      }
    }
    #endregion

    /// <summary>
    /// Function will remove selfNode from parent node.
    /// using this function will prevent errors when selfNode is null.
    /// </summary>
    /// <param name="selfNode">This node will be removed from parent node</param>
    /// <returns>Result of selfNode.ParentNode.RemoveChild(selfNode) or null if selfNode is null</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode")]
    private static XmlNode RemoveNodeFromParent(XmlNode selfNode)
    {
      if (selfNode != null)
      {
        return selfNode.ParentNode.RemoveChild(selfNode);
      }

      return null;
    }
  }
}