//--------------------------------------------------------------------------
// <copyright file="PageProperties.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>?abc?</summary>
//--------------------------------------------------------------------------
//using System;
//using System.Collections.Generic;
//using System.Text;
using System.Xml;

namespace Microsoft.Dynamics.Nav.Tools.FormTransformation
{
  /// <summary>
  /// ?abc?
  /// </summary>
  internal static class PageProperties
  {
   //// It seems like we don’t need this function anymore.
   //// /// <summary>
   //// /// ?abc?
   //// /// </summary>
   //// /// <param name="xmlNode">?abc?</param>
   //// /// <param name="xmlNamespaceMgt">?abc?</param>
   //// public static void CleanProperties(XmlNode xmlNode, XmlNamespaceManager xmlNamespaceMgt)
   //// {
   /////*   XmlUtility.DeleteElements(xmlNode.SelectSingleNode(@"./a:Properties", xmlNamespaceMgt), "./a:Caption", xmlNamespaceMgt);
   ////   XmlUtility.DeleteElements(xmlNode.SelectSingleNode(@"./a:Properties", xmlNamespaceMgt), "./a:DataCaptionExpr", xmlNamespaceMgt);
   ////   XmlUtility.DeleteElements(xmlNode.SelectSingleNode(@"./a:Properties", xmlNamespaceMgt), "./a:DataCaptionFields", xmlNamespaceMgt);
   ////   XmlUtility.DeleteElements(xmlNode.SelectSingleNode(@"./a:Properties", xmlNamespaceMgt), "./a:BorderStyle", xmlNamespaceMgt);
   ////   XmlUtility.DeleteElements(xmlNode.SelectSingleNode(@"./a:Properties", xmlNamespaceMgt), "./a:CaptionBar", xmlNamespaceMgt);
   ////   XmlUtility.DeleteElements(xmlNode.SelectSingleNode(@"./a:Properties", xmlNamespaceMgt), "./a:Minimizable", xmlNamespaceMgt);
   ////   XmlUtility.DeleteElements(xmlNode.SelectSingleNode(@"./a:Properties", xmlNamespaceMgt), "./a:Maximizable", xmlNamespaceMgt);
   ////   XmlUtility.DeleteElements(xmlNode.SelectSingleNode(@"./a:Properties", xmlNamespaceMgt), "./a:Sizeable", xmlNamespaceMgt);
   ////   XmlUtility.DeleteElements(xmlNode.SelectSingleNode(@"./a:Properties", xmlNamespaceMgt), "./a:LogWidth", xmlNamespaceMgt);
   ////   XmlUtility.DeleteElements(xmlNode.SelectSingleNode(@"./a:Properties", xmlNamespaceMgt), "./a:LogHeight", xmlNamespaceMgt);
   ////   XmlUtility.DeleteElements(xmlNode.SelectSingleNode(@"./a:Properties", xmlNamespaceMgt), "./a:Width", xmlNamespaceMgt);
   ////   XmlUtility.DeleteElements(xmlNode.SelectSingleNode(@"./a:Properties", xmlNamespaceMgt), "./a:Height", xmlNamespaceMgt);
   ////   XmlUtility.DeleteElements(xmlNode.SelectSingleNode(@"./a:Properties", xmlNamespaceMgt), "./a:XPos", xmlNamespaceMgt);
   ////   XmlUtility.DeleteElements(xmlNode.SelectSingleNode(@"./a:Properties", xmlNamespaceMgt), "./a:YPos", xmlNamespaceMgt);
   ////   XmlUtility.DeleteElements(xmlNode.SelectSingleNode(@"./a:Properties", xmlNamespaceMgt), "./a:BackColor", xmlNamespaceMgt);
   ////   XmlUtility.DeleteElements(xmlNode.SelectSingleNode(@"./a:Properties", xmlNamespaceMgt), "./a:Visible", xmlNamespaceMgt);
   ////   XmlUtility.DeleteElements(xmlNode.SelectSingleNode(@"./a:Properties", xmlNamespaceMgt), "./a:ActiveControlOnOpen", xmlNamespaceMgt);
   ////   XmlUtility.DeleteElements(xmlNode.SelectSingleNode(@"./a:Properties", xmlNamespaceMgt), "./a:MinimizedOnOpen", xmlNamespaceMgt);
   ////   XmlUtility.DeleteElements(xmlNode.SelectSingleNode(@"./a:Properties", xmlNamespaceMgt), "./a:MaximizedOnOpen", xmlNamespaceMgt);
   ////   XmlUtility.DeleteElements(xmlNode.SelectSingleNode(@"./a:Properties", xmlNamespaceMgt), "./a:AutoPosition", xmlNamespaceMgt);
   ////   XmlUtility.DeleteElements(xmlNode.SelectSingleNode(@"./a:Properties", xmlNamespaceMgt), "./a:TableBoxId", xmlNamespaceMgt);
   ////   XmlUtility.DeleteElements(xmlNode.SelectSingleNode(@"./a:Properties", xmlNamespaceMgt), "./a:LookupMode", xmlNamespaceMgt);
   ////   XmlUtility.DeleteElements(xmlNode.SelectSingleNode(@"./a:Properties", xmlNamespaceMgt), "./a:CalcFields", xmlNamespaceMgt);
   ////   XmlUtility.DeleteElements(xmlNode.SelectSingleNode(@"./a:Properties", xmlNamespaceMgt), "./a:SourceTablePlacement", xmlNamespaceMgt);
   ////   XmlUtility.DeleteElements(xmlNode.SelectSingleNode(@"./a:Properties", xmlNamespaceMgt), "./a:SourceTableRecord", xmlNamespaceMgt);
   ////   XmlUtility.DeleteElements(xmlNode.SelectSingleNode(@"./a:Properties", xmlNamespaceMgt), "./a:SaveControlInfo", xmlNamespaceMgt);
   ////   XmlUtility.DeleteElements(xmlNode.SelectSingleNode(@"./a:Properties", xmlNamespaceMgt), "./a:SaveColumnWidths", xmlNamespaceMgt);
   ////   XmlUtility.DeleteElements(xmlNode.SelectSingleNode(@"./a:Properties", xmlNamespaceMgt), "./a:SavePosAndSize", xmlNamespaceMgt);
   ////   XmlUtility.DeleteElements(xmlNode.SelectSingleNode(@"./a:Properties", xmlNamespaceMgt), "./a:UpdateOnActivate", xmlNamespaceMgt);
   ////   XmlUtility.DeleteElements(xmlNode.SelectSingleNode(@"./a:Properties", xmlNamespaceMgt), "./a:DelayedInsert", xmlNamespaceMgt);
   ////   XmlUtility.DeleteElements(xmlNode.SelectSingleNode(@"./a:Properties", xmlNamespaceMgt), "./a:PopulateAllFields", xmlNamespaceMgt);
   ////   XmlUtility.DeleteElements(xmlNode.SelectSingleNode(@"./a:Properties", xmlNamespaceMgt), "./a:Horzgrid", xmlNamespaceMgt);
   ////   XmlUtility.DeleteElements(xmlNode.SelectSingleNode(@"./a:Properties", xmlNamespaceMgt), "./a:VertGrid", xmlNamespaceMgt);
   ////   XmlUtility.DeleteElements(xmlNode.SelectSingleNode(@"./a:Properties", xmlNamespaceMgt), "./a:TimerInterval", xmlNamespaceMgt);*/
   //// }

    /// <summary>
    /// ?abc?
    /// </summary>
    public static void AddDefaultProperties()
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;

      // Add PageType=Card
      const string PageType = "PageType";
      string defaultPageType = "Card";

      XmlNode properties = metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode("./a:Properties", metaDataDocMgt.XmlNamespaceMgt);      
      if (properties != null)
      {
        if (properties.SelectSingleNode("./a:" + PageType, metaDataDocMgt.XmlNamespaceMgt) == null)
        {
          // Only assign default Pagetype for Page objects   
          if (metaDataDocMgt.GetCurrentPageId > 0)
          {
            XmlElement element = XmlUtility.CreateXmlElement(PageType, defaultPageType);
            properties.AppendChild(element);
          }
        }
      }
      else
      {
        string logStr = string.Format(
          System.Globalization.CultureInfo.InvariantCulture,
          Resources.CanNotFindNode,
          "Properties",
          PageType);
        TransformationLog.GenericLogEntry(logStr, LogCategory.Error, metaDataDocMgt.GetCurrentPageId);
      }
    }
  }
}
