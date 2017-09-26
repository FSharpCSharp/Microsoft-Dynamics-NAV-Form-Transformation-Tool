//--------------------------------------------------------------------------
// <copyright file="XMLUtility.cs" company="Microsoft">
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
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.Tools.FormTransformation
{
  /// <summary>
  /// ?abc?
  /// </summary>
  public static class XmlUtility
  {
    /// <summary>
    /// ?abc?
    /// </summary>
    /// <param name="nodeToMove">?abc?</param>
    /// <param name="destinationNode">?abc?</param>
    internal static void MoveNode(XmlNode nodeToMove, XmlNode destinationNode)
    {
      if (destinationNode == null)
      {
        throw new ArgumentNullException("destinationNode");
      }

      if ((nodeToMove != null) && (destinationNode != null))
      {
        destinationNode.AppendChild(nodeToMove);
      }
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    /// <param name="sourceNode">?abc?</param>
    /// <param name="name">?abc?</param>
    /// <param name="namespaceMgt">?abc?</param>
    internal static void DeleteElements(XmlNode sourceNode, String name, XmlNamespaceManager namespaceMgt)
    {
      if (sourceNode == null)
      {
        throw new ArgumentNullException("sourceNode");
      }

      XmlNodeList nodesToBeDeleted = sourceNode.SelectNodes(@"" + name, namespaceMgt);
      for (Int32 i = nodesToBeDeleted.Count - 1; i >= 0; i--)
      {
        nodesToBeDeleted[i].ParentNode.RemoveChild(nodesToBeDeleted[i]);
      }
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    /// <param name="nodeList">?abc?</param>
    /// <param name="oldText">?abc?</param>
    /// <param name="newText">?abc?</param>
    internal static void ReplaceSubstringInElement(XmlNodeList nodeList, String oldText, String newText)
    {
      if (nodeList == null)
      {
        throw new ArgumentNullException("nodeList");
      }

      foreach (XmlNode node in nodeList)
      {
        ReplaceSubstringInElement(node, oldText, newText);
      }
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    /// <param name="node">?abc?</param>
    /// <param name="oldText">?abc?</param>
    /// <param name="newText">?abc?</param>
    internal static void ReplaceSubstringInElement(XmlNode node, String oldText, String newText)
    {
      if (node == null)
      {
        throw new ArgumentNullException("node");
      }

      node.InnerText = node.InnerText.Replace(oldText, newText);
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    /// <param name="sourceNodeList">?abc?</param>
    internal static void DeleteNodeList(XmlNodeList sourceNodeList)
    {
      if (sourceNodeList == null)
      {
        throw new ArgumentNullException("sourceNodeList");
      }

      for (Int32 i = sourceNodeList.Count - 1; i >= 0; i--)
      {
        sourceNodeList[i].ParentNode.RemoveChild(sourceNodeList[i]);
      }
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    /// <param name="sourceNode">?abc?</param>
    /// <param name="destinationNode">?abc?</param>
    /// <param name="insertOrder">?abc?</param>
    internal static void MoveChilds(XmlNode sourceNode, XmlNode destinationNode, InsertOrder insertOrder)
    {
      if (sourceNode == null)
      {
        throw new ArgumentNullException("sourceNode");
      }

      if (destinationNode == null)
      {
        throw new ArgumentNullException("destinationNode");
      }

      for (Int32 k = sourceNode.ChildNodes.Count - 1; k >= 0; k--)
      {
        if (insertOrder == InsertOrder.Append)
        {
          destinationNode.AppendChild(sourceNode.ChildNodes[k]);
        }
        else
        {
          destinationNode.PrependChild(sourceNode.ChildNodes[k]);
        }
      }
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    /// <param name="name">?abc?</param>
    /// <param name="innerText">?abc?</param>
    /// <returns>?abc?</returns>
    internal static XmlElement CreateXmlElement(String name, String innerText)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      XmlElement element = metaDataDocMgt.XmlDocument.CreateElement(name, metaDataDocMgt.XmlNamespace);
      if (innerText != null)
      {
        element.InnerText = innerText;
      }

      return element;
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    /// <param name="name">?abc?</param>
    /// <returns>?abc?</returns>
    internal static XmlElement CreateXmlElement(String name)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      return metaDataDocMgt.XmlDocument.CreateElement(name, metaDataDocMgt.XmlNamespace);
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    /// <param name="data">?abc?</param>
    /// <returns>?abc?</returns>
    internal static XmlCDataSection CreateCDataSection(String data)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      return metaDataDocMgt.XmlDocument.CreateCDataSection(data);
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    /// <param name="node">?abc?</param>
    /// <param name="newName">?abc?</param>
    private static void RenameNode(XmlNode node, String newName)
    {
      if (node == null)
      {
        throw new ArgumentNullException("node");
      }

      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      XmlNode renameNode = metaDataDocMgt.XmlDocument.CreateNode(node.NodeType, newName, node.NamespaceURI);
      renameNode.InnerXml = node.InnerXml;
      for (Int32 i = node.Attributes.Count - 1; i >= 0; i--)
      {
        if (node.Attributes[i].Name != "xmlns")
        {
          renameNode.Attributes.Prepend(node.Attributes[i]);
        }
      }

      node.ParentNode.ReplaceChild(renameNode, node);
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    /// <param name="nodeList">?abc?</param>
    /// <param name="newName">?abc?</param>
    internal static void RenameNode(XmlNodeList nodeList, String newName)
    {
      if (nodeList == null)
      {
        throw new ArgumentNullException("nodeList");
      }

      for (Int32 i = nodeList.Count - 1; i >= 0; i--)
      {
        XmlUtility.RenameNode(nodeList[i], newName);
      }
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    /// <param name="node">?abc?</param>
    /// <param name="newName">?abc?</param>
    /// <returns>?abc?</returns>
    internal static XmlNode GetNodeWithNewName(XmlNode node, String newName)
    {
      if (node == null)
      {
        throw new ArgumentNullException("node");
      }

      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      XmlNode renameNode = metaDataDocMgt.XmlDocument.CreateNode(node.NodeType, newName, node.NamespaceURI);
      renameNode.InnerXml = node.InnerXml;
      for (Int32 i = node.Attributes.Count - 1; i >= 0; i--)
      {
        if (node.Attributes[i].Name != "xmlns")
        {
          renameNode.Attributes.Prepend(node.Attributes[i]);
        }
      }

      return renameNode;
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    /// <param name="fileName">?abc?</param>
    /// <returns>?abc?</returns>
    /// 
    public static string[] LoadFromFileToString(String fileName)
    {
      if (!String.IsNullOrEmpty(fileName))
      {
        try
        {
          return File.ReadAllLines(fileName);
        }
        catch (IOException ioe)
        {
          throw new IOException(String.Format(CultureInfo.InvariantCulture, Resources.FileLoadError, fileName), ioe);
        }
      }

      return null;
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    /// <param name="fileName">?abc?</param>
    /// <returns>?abc?</returns>
    public static XmlDocument LoadFromFileToXml(String fileName)
    {
      if (!String.IsNullOrEmpty(fileName))
      {
        try
        {
          XmlDocument newXMLDoc = new XmlDocument();
          newXMLDoc.Load(fileName);
          return newXMLDoc;
        }
        catch (IOException ioe)
        {
          throw new IOException(String.Format(CultureInfo.InvariantCulture, Resources.FileLoadError, fileName), ioe);
        }
        catch (XmlException xe)
        {
          throw new XmlException(String.Format(CultureInfo.InvariantCulture, Resources.FileIsNotAWellFormattedXmlDocument, fileName), xe);
        }
      }

      return null;
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    /// <param name="xmlDocument">?abc?</param>
    /// <param name="fileName">?abc?</param>
    /// <returns>?abc?</returns>
    // We need to use xmlDocument.Save in this function
    [SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode")]
    public static Boolean SaveXmlToFile(XmlDocument xmlDocument, String fileName)
    {
      if (xmlDocument == null)
      {
        throw new ArgumentNullException("xmlDocument");
      }

      try
      {
        xmlDocument.Save(fileName);
      }
      catch (XmlException ex)
      {
        string logStr = String.Format(System.Globalization.CultureInfo.InvariantCulture, Resources.CouldNotSave, fileName);
        TransformationLog.GenericLogEntry(logStr, LogCategory.Error, (int)LogEntryObjectId.None, ex.Message);
        return false;
      }
      catch (IOException ex)
      {
        string logStr = String.Format(System.Globalization.CultureInfo.InvariantCulture, Resources.CouldNotSave, fileName);
        TransformationLog.GenericLogEntry(logStr, LogCategory.Error, (int)LogEntryObjectId.None, ex.Message);
        return false;
      }

      return true;
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    /// <param name="parentNode">?abc?</param>
    /// <param name="newName">?abc?</param>
    /// <returns>?abc?</returns>
    internal static XmlNode InsertNodeWithPropertyChild(XmlNode parentNode, String newName)
    {
      if (parentNode == null)
      {
        throw new ArgumentNullException("parentNode");
      }

      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      XmlNode newNode = metaDataDocMgt.XmlDocument.CreateNode(XmlNodeType.Element, newName, parentNode.NamespaceURI);
      newNode.AppendChild(metaDataDocMgt.XmlDocument.CreateNode(XmlNodeType.Element, "Properties", parentNode.NamespaceURI));
      parentNode.AppendChild(newNode);

      return newNode;
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    /// <param name="parentNode">?abc?</param>
    /// <param name="newName">?abc?</param>
    /// <param name="id">?abc?</param>
    /// <returns>?abc?</returns>
    internal static XmlNode InsertNodeWithPropertyChild(XmlNode parentNode, String newName, Int32 id)
    {
      if (parentNode == null)
      {
        throw new ArgumentNullException("parentNode");
      }

      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      XmlNode newNode = metaDataDocMgt.XmlDocument.CreateNode(XmlNodeType.Element, newName, parentNode.NamespaceURI);
      XmlNode propertyNode = metaDataDocMgt.XmlDocument.CreateNode(XmlNodeType.Element, "Properties", parentNode.NamespaceURI);
      newNode.AppendChild(propertyNode);
      parentNode.AppendChild(newNode);

      XmlNode newID = metaDataDocMgt.XmlDocument.CreateNode(XmlNodeType.Element, "ID", parentNode.NamespaceURI);
      newID.InnerText = id.ToString(CultureInfo.InvariantCulture);
      propertyNode.AppendChild(newID);
      return newNode;
    }

    /// <summary>
    /// Function will return value of element specified in XPath query. 
    /// Function will return default value if node can’t be found. 
    /// You can use String.Empty as default and then validate by String.IsNullOrEmpty.
    /// Attention! Function will return the first found element’s value if more than one element can be found.
    /// </summary>
    /// <param name="node">Node in which search will be perform</param>
    /// <param name="element">XPath query</param>
    /// <param name="defaultValue">default value</param>
    /// <returns>Element value or default value if element’s node not found.</returns>
    internal static string GetNodeValue(System.Xml.XPath.IXPathNavigable node, String element, String defaultValue)
    {
      if (node != null)
      {
        // TODO: Throw error if more than one element can be found.
        MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
        System.Xml.XPath.XPathNavigator position = node.CreateNavigator().SelectSingleNode(element, metaDataDocMgt.XmlNamespaceMgt);
        if (position == null)
        {
          return defaultValue;
        }

        return position.Value;
      }

      return null;
    }
    #region Sort controls

    /// <summary>
    /// ?abc?
    /// </summary>
    internal static void AlignControls()
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;

      XmlNodeList controlList = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@"./a:Controls", metaDataDocMgt.XmlNamespaceMgt);
      XmlNodeList tabControlList = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Control[./a:Properties/a:Controltype='TabControl']", metaDataDocMgt.XmlNamespaceMgt);
      XmlNodeList frameControlList = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Control[./a:Properties/a:Controltype='Frame']", metaDataDocMgt.XmlNamespaceMgt);

      StringBuilder query0 = new StringBuilder(@"./a:Controls/a:Control");
      StringBuilder query1 = new StringBuilder(query0.ToString());
      int tabYPos = 0;

      XmlNodeList tabelBoxList = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@"./a:Controls/a:Control[./a:Properties/a:Controltype='TableBox']", metaDataDocMgt.XmlNamespaceMgt);
      if (tabelBoxList.Count > 0)
      {
        tabYPos = Convert.ToInt32(tabelBoxList[0].SelectSingleNode("./a:Properties/a:YPos", metaDataDocMgt.XmlNamespaceMgt).InnerText, CultureInfo.InvariantCulture);
        if (tabelBoxList.Count > 1)
        {
          foreach (XmlNode tabControl in tabelBoxList)
          {
            int tmpYPos = Convert.ToInt32(tabControl.SelectSingleNode("./a:Properties/a:YPos", metaDataDocMgt.XmlNamespaceMgt).InnerText, CultureInfo.InvariantCulture);
            if (tabYPos > tmpYPos)
            {
              tabYPos = tmpYPos;
            }
          }
        }

        string ypos = tabYPos.ToString(CultureInfo.InvariantCulture);
        query0.Append(" [ ./a:Properties/a:YPos<");
        query1.Append(" [ ./a:Properties/a:YPos>=");
        query0.Append(ypos);
        query1.Append(ypos);
        query0.Append("]");
        query1.Append("]");
      }

      foreach (XmlNode tabControl in tabControlList)
      {
        SortControls(tabControl, 1);
      }

      XmlNodeList controlList0 = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(query0.ToString(), metaDataDocMgt.XmlNamespaceMgt);
      XmlNodeList controlList1 = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(query1.ToString(), metaDataDocMgt.XmlNamespaceMgt);

      XmlNode controls = controlList[0].Clone();
      controls.RemoveAll();

      foreach (XmlNode anode in controlList0)
      {
        controls.AppendChild(anode);
      }

      SortControls(controls, 0);

      foreach (XmlNode bnode in controlList1)
      {
        controls.AppendChild(bnode);
      }

      SortControls(controls, controlList0.Count);

      XmlNode controlsNode = controlList[0];
      controlsNode.RemoveAll();
      foreach (XmlNode sortedNodes in controls.ChildNodes)
      {
        controlsNode.AppendChild(sortedNodes.Clone());
      }

      Boolean countOK = false;
      while (!countOK)
      {
        countOK = true;
        Int32 noOfFrames = 0;
        foreach (XmlNode frameControl in frameControlList)
        {
          if (noOfFrames == 0)
            noOfFrames = frameControlList.Count;
          SortControls(frameControl, 1);
          noOfFrames--;
          countOK = noOfFrames == 0;
        }
      }
    }

    private static void SortControls(XmlNode parentNode, int startAt)
    {
      List<XmlNode> nodeList = new List<XmlNode>();

      for (Int32 i = startAt; i <= parentNode.ChildNodes.Count - 1; i++)
      {
        nodeList.Add(parentNode.ChildNodes[i]);
      }

      nodeList.Sort(Compare);

      for (Int32 i = parentNode.ChildNodes.Count - 1; i >= startAt; i--)
      {
        parentNode.RemoveChild(parentNode.ChildNodes[i]);
      }

      foreach (XmlNode x in nodeList)
      {
        parentNode.AppendChild(x);
      }
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    /// <param name="node">?abc?</param>
    /// <returns>?abc?</returns>
    public static String GetNodePath(XmlNode node)
    {
      if (node == null)
      {
        throw new ArgumentNullException("node");
      }

      XmlNode newNode = node;
      String path = node.Name;

      while (newNode.ParentNode != null)
      {
        newNode = newNode.ParentNode;
        path = String.Join(String.Empty, new string[] { newNode.Name, "/", path });
      }

      return path.Replace("#document", ".");
    }

    enum sequenceDiagram { NotStarted, LangPart, CaptionPart }

    internal static String GetCaption(String captionML)
    {
        if (String.IsNullOrEmpty(captionML))
            return null;

        captionML = captionML.Replace("\r", string.Empty);
        captionML = captionML.Replace("\n", string.Empty);

        Dictionary<String, String> captions = new Dictionary<string, string>();
        sequenceDiagram trackPos = sequenceDiagram.NotStarted;
        String lang = null;
        String caption = null;

        foreach (char c in captionML)
        {
            if (trackPos == sequenceDiagram.NotStarted)
            {
                if (!Char.IsLetter(c))
                    continue;
                trackPos = sequenceDiagram.LangPart;
                lang = null;
                caption = null;
            }

            if (trackPos == sequenceDiagram.LangPart)
            {
                if (c.Equals('='))
                {
                    trackPos = sequenceDiagram.CaptionPart;
                    continue;
                }
                lang = lang + c;
            }

            if (trackPos == sequenceDiagram.CaptionPart)
            {
                if (c.Equals(';'))
                {
                    trackPos = sequenceDiagram.NotStarted;
                    if (lang.Equals("ENU"))
                        return caption;
                    captions.Add(lang, caption);
                    continue;
                }
                caption = caption + c;
            }
        }

        return caption;
    }

    private static Int32 Compare(XmlNode control1, XmlNode control2)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      const Int32 HorzCompareTolerance = 3500;
      Int32 x1 = Convert.ToInt32(control1.SelectSingleNode("./a:Properties/a:XPos", metaDataDocMgt.XmlNamespaceMgt).InnerText, CultureInfo.InvariantCulture);
      Int32 y1 = Convert.ToInt32(control1.SelectSingleNode("./a:Properties/a:YPos", metaDataDocMgt.XmlNamespaceMgt).InnerText, CultureInfo.InvariantCulture);

      Int32 x2 = Convert.ToInt32(control2.SelectSingleNode("./a:Properties/a:XPos", metaDataDocMgt.XmlNamespaceMgt).InnerText, CultureInfo.InvariantCulture);
      Int32 y2 = Convert.ToInt32(control2.SelectSingleNode("./a:Properties/a:YPos", metaDataDocMgt.XmlNamespaceMgt).InnerText, CultureInfo.InvariantCulture);

      if (Math.Abs(x1 - x2) < HorzCompareTolerance)  /* same column */
      {
        if (y1 == y2)
        {
          if (x1 < x2)
          {
            return -1;
          }

          if (x1 > x2)
          {
            return 1;
          }

          if (x1 == x2)
          {
            return 0;
          }
        }
        else
        {
          if (y1 < y2)
          {
            return -1;
          }
          else
          {
            return 1;
          }
        }
      }
      else
      {
        if (x1 < x2)
        {
          return -1;
        }
        else
        {
          return 1;
        }
      }

      return 0;
    }

    #endregion

    /// <summary>
    /// Will update InnerText property if element exists. Otherwise will create new Element with InnerText set to "newValue"
    /// </summary>
    /// <param name="nodeToUpdate">This node will contain nodeName</param>
    /// <param name="nodeName">Node name</param>
    /// <param name="newValue">New Value</param>
    internal static void UpdateNodeInnerText(XmlNode nodeToUpdate, string nodeName, string newValue)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      XmlNode node = nodeToUpdate.SelectSingleNode("./a:" + nodeName, metaDataDocMgt.XmlNamespaceMgt);
      if (node == null)
      {
        nodeToUpdate.AppendChild(XmlUtility.CreateXmlElement(nodeName, newValue));
      }
      else
      {
        node.InnerText = newValue;
      }
    }

  }
}