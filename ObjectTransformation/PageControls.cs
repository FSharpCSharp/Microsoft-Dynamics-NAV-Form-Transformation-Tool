//--------------------------------------------------------------------------
// <copyright file="PageControls.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>?abc?</summary>
//--------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Collections;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Microsoft.Dynamics.Nav.Tools.FormTransformation
{
  /// <summary>
  /// ?abc?
  /// </summary>
  internal static class PageControls
  {
    private static System.Collections.ArrayList objectsHasIndentation = new ArrayList(100);
    private struct OptionButton
    {
      private String sourceExpr;
      private String parentId;
      private String optionValue;
      private String optionCaption;

      /// <summary>
      /// ?abc?
      /// </summary>
      public String SourceExpression
      {
        get
        {
          return sourceExpr;
        }

        set
        {
          sourceExpr = value;
        }
      }

      /// <summary>
      /// ?abc?
      /// </summary>
      public String ParentId
      {
        get
        {
          return parentId;
        }

        set
        {
          parentId = value;
        }
      }

      /// <summary>
      /// ?abc?
      /// </summary>
      public String OptionValue
      {
        get
        {
          return optionValue;
        }

        set
        {
          optionValue = value;
        }
      }

      /// <summary>
      /// ?abc?
      /// </summary>
      public String OptionCaption
      {
        get
        {
          return optionCaption;
        }

        set
        {
          optionCaption = value;
        }
      }
    }

    #region Public static methods

    /// <summary>
    /// If object has Indentation by label
    /// </summary>
    /// <param name="objectId">?abc?</param>
    /// <returns>?abc?</returns>
    public static bool IsObjectHasIndentation(int objectId)
    {
      return PageControls.objectsHasIndentation.Contains(objectId);
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    public static void ManageOptionButtons()
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      Dictionary<String, Dictionary<String, OptionButton>> optionSourceExprList = new Dictionary<String, Dictionary<String, OptionButton>>();

      // List<String> optionSourceExprList = new List<String>();
      String sourceExpr = "";

      XmlNodeList optionButtonList =
        metaDataDocMgt.XmlCurrentFormNode.SelectNodes(
          @".//a:Controls//a:Control[./a:Properties/a:Controltype='OptionButton']", metaDataDocMgt.XmlNamespaceMgt);

      for (Int32 i = 0; i <= optionButtonList.Count - 1; i++)
      {
        XmlNode optionButton = optionButtonList[i];

        //FixCaptionsForOptionButtons(optionButton);

        String id = GetProperty(optionButton, "ID");
        sourceExpr = GetProperty(optionButton, "SourceExpr");
        String parentId = GetProperty(optionButton.ParentNode, "ID");
        if (parentId == null)
          parentId = "";

        Dictionary<String, OptionButton> currOptionSourceExpr;
        OptionButton optionSourceExpr = new OptionButton();
        //optionSourceExpr.Id = id;
        optionSourceExpr.ParentId = parentId;
        optionSourceExpr.SourceExpression = sourceExpr;
        optionSourceExpr.OptionValue = GetProperty(optionButton, "OptionValue");
        optionSourceExpr.OptionCaption = GetProperty(optionButton, "CaptionML");

        if (optionSourceExprList.TryGetValue(optionSourceExpr.ParentId + "_" + optionSourceExpr.SourceExpression, out currOptionSourceExpr))
        {
          String remainingCtrlID;
          foreach (KeyValuePair<String, OptionButton> optionValue in currOptionSourceExpr)
          {
            remainingCtrlID = optionValue.Key;
            CodeTransformationRules.MoveOptionButtonControlOnValidate(id, remainingCtrlID);
            break;
          }

          currOptionSourceExpr.Add(id, optionSourceExpr);
          ProcessOptionButtonCaptionML(false, optionButton, optionSourceExpr);
          RemoveThisNodeFromParent(optionButton);
        }
        else
        {
          CodeTransformationRules.MoveOptionButtonControlOnValidate(id, id);
          Dictionary<String, OptionButton> currOptionSourceExpr1 = new Dictionary<string, OptionButton>();
          currOptionSourceExpr1.Add(id, optionSourceExpr);
          optionSourceExprList.Add(optionSourceExpr.ParentId + "_" + optionSourceExpr.SourceExpression, currOptionSourceExpr1);
          ProcessOptionButtonCaptionML(true, optionButton, optionSourceExpr);
        }
      }

      optionButtonList =
              metaDataDocMgt.XmlCurrentFormNode.SelectNodes(
                @".//a:Controls//a:Control[./a:Properties/a:Controltype='OptionButton']", metaDataDocMgt.XmlNamespaceMgt);

      for (Int32 i = 0; i <= optionButtonList.Count - 1; i++)
      {
        XmlNode optionButton = optionButtonList[i];
        String id = GetProperty(optionButton, "ID");
        sourceExpr = GetProperty(optionButton, "SourceExpr");
        String parentId = GetProperty(optionButton.ParentNode, "ID");
        if (parentId == null)
          parentId = "";

        String optionList = CodeTransformationRules.FindOptionList(
          sourceExpr,
          metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(@".//a:Code", metaDataDocMgt.XmlNamespaceMgt).InnerText);

        // String[] optionsArray = null;
        Dictionary<String, OptionButton> currOptionSourceExpr;
        if (optionList == null)
        {
          if (optionSourceExprList.TryGetValue(parentId + "_" + sourceExpr, out currOptionSourceExpr))
          {
            String valuesAllowed = null;
            foreach (KeyValuePair<String, OptionButton> optionValue in currOptionSourceExpr)
            {
              valuesAllowed = (valuesAllowed == null ? optionValue.Value.OptionValue : valuesAllowed + ";" + optionValue.Value.OptionValue);
            }

            if (valuesAllowed != null)
            {
              XmlNode optionButtonCaptionMLNode = optionButton.SelectSingleNode(@"./a:Properties", metaDataDocMgt.XmlNamespaceMgt);
              optionButtonCaptionMLNode.AppendChild(XmlUtility.CreateXmlElement("ValuesAllowed", valuesAllowed));
            }
          }
        }

        if (optionList != null)
        {
          ProcessOptionButtonOptionCaptionML(optionList, optionButton, optionSourceExprList, parentId, sourceExpr);
        }
      }
    }

    //private static void FixCaptionsForOptionButtons(XmlNode optionButton)
    //{
    //  if (optionButton == null)
    //  {
    //    return;
    //  }

    //  MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
    //  if (GetProperty(optionButton, "ShowCaption").Equals("NO", StringComparison.OrdinalIgnoreCase))
    //  {
    //    string ypos = GetProperty(optionButton, "YPos");
    //    XmlNodeList labelList = optionButton.SelectNodes(@".//a:Control[./a:Properties/a:Controltype='Label']", metaDataDocMgt.XmlNamespaceMgt);
    //    foreach (XmlNode label in labelList)
    //    {
    //      if (string.IsNullOrEmpty(GetProperty(label, "CaptionML")))
    //      {
    //        XmlNode showCaption = optionButton.SelectSingleNode(@"./a:Properties/a:ShowCaption", metaDataDocMgt.XmlNamespaceMgt);
    //        RemoveThisNodeFromParent(showCaption);

    //        XmlNode xposNode = optionButton.SelectSingleNode(@"./a:Properties/a:XPos", metaDataDocMgt.XmlNamespaceMgt);
    //        if (xposNode != null)
    //        {
    //          string xpos = GetProperty(label, "XPos");
    //          xposNode.InnerXml = xpos;
    //          RemoveThisNodeFromParent(label);
    //        }
    //      }
    //      else
    //      {
    //        if (GetProperty(label, "YPos").Equals(ypos, StringComparison.OrdinalIgnoreCase))
    //        {
    //          if (string.IsNullOrEmpty(GetProperty(optionButton, "CaptionML")))
    //          {
    //            //optionButton.FirstChild.AppendChild(XmlUtility.CreateXmlElement("CaptionML"
    //          }
    //        }
    //      }
    //    }
    //  }
    //}

    /// <summary>
    /// ?abc?
    /// </summary>
    public static void TransformNavigationButtons()
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      Dictionary<String, Dictionary<String, OptionButton>> optionSourceExprList = new Dictionary<String, Dictionary<String, OptionButton>>();
      String sourceExpr = "";

      XmlNodeList optionButtonList = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(
        @".//a:Controls//a:Control[./a:Properties/a:Controltype='PictureBox' and ./a:Properties/a:BitmapList='35,39']",
        metaDataDocMgt.XmlNamespaceMgt);

      for (Int32 i = 0; i <= optionButtonList.Count - 1; i++)
      {
        XmlNode optionButton = optionButtonList[i];
        String id = GetProperty(optionButton, "ID");
        sourceExpr = GetProperty(optionButton, "SourceExpr");
        String[] splitSourceExpr = sourceExpr.Split("=".ToCharArray());
        sourceExpr = splitSourceExpr[1].Trim();
        String parentId = GetProperty(optionButton.ParentNode, "ID");
        if (parentId == null)
          parentId = "";

        Dictionary<String, OptionButton> currOptionSourceExpr;
        OptionButton optionSourceExpr = new OptionButton();
        optionSourceExpr.ParentId = parentId;
        optionSourceExpr.SourceExpression = sourceExpr;
        optionSourceExpr.OptionValue = "x" + splitSourceExpr[0].Trim();

        if (optionSourceExprList.TryGetValue(optionSourceExpr.ParentId + "_" + optionSourceExpr.SourceExpression, out currOptionSourceExpr))
        {
          if (GetProperty(optionButton.NextSibling, "Controltype") == "CommandButton")
          {
            optionButton.NextSibling.SelectSingleNode(@"./a:Properties/a:Controltype", metaDataDocMgt.XmlNamespaceMgt).FirstChild.Value = "OptionButton";
            optionButton.NextSibling.SelectSingleNode(@"./a:Properties", metaDataDocMgt.XmlNamespaceMgt).AppendChild(
              XmlUtility.CreateXmlElement("SourceExpr", optionSourceExpr.SourceExpression + "Opt"));
            optionButton.NextSibling.SelectSingleNode(@"./a:Properties", metaDataDocMgt.XmlNamespaceMgt).AppendChild(
              XmlUtility.CreateXmlElement("OptionValue", optionSourceExpr.OptionValue));

            if (GetProperty(optionButton.NextSibling.NextSibling, "Controltype") == "TextBox")
            {
              if ((GetProperty(optionButton.NextSibling.NextSibling, "CaptionML") == null) &&
                  (GetProperty(optionButton.NextSibling, "CaptionML") != null))
              {
                optionButton.NextSibling.NextSibling.SelectSingleNode(@"./a:Properties", metaDataDocMgt.XmlNamespaceMgt).AppendChild(
                  XmlUtility.CreateXmlElement("CaptionML", GetProperty(optionButton.NextSibling, "CaptionML")));
              }
            }

            RemoveThisNodeFromParent(optionButton);
            currOptionSourceExpr.Add(id, optionSourceExpr);
          }
        }
        else
        {
          if (GetProperty(optionButton.NextSibling, "Controltype") == "CommandButton")
          {
            optionButton.NextSibling.SelectSingleNode(@"./a:Properties/a:Controltype", metaDataDocMgt.XmlNamespaceMgt).FirstChild.Value = "OptionButton";
            optionButton.NextSibling.SelectSingleNode(@"./a:Properties", metaDataDocMgt.XmlNamespaceMgt).AppendChild(
              XmlUtility.CreateXmlElement("SourceExpr", optionSourceExpr.SourceExpression + "Opt"));
            optionButton.NextSibling.SelectSingleNode(@"./a:Properties", metaDataDocMgt.XmlNamespaceMgt).AppendChild(
              XmlUtility.CreateXmlElement("OptionValue", optionSourceExpr.OptionValue));
            if (GetProperty(optionButton.NextSibling.NextSibling, "Controltype") == "TextBox")
            {
              if ((GetProperty(optionButton.NextSibling.NextSibling, "CaptionML") == null) &&
                  (GetProperty(optionButton.NextSibling, "CaptionML") != null))
              {
                optionButton.NextSibling.NextSibling.SelectSingleNode(@"./a:Properties", metaDataDocMgt.XmlNamespaceMgt).AppendChild(
                  XmlUtility.CreateXmlElement("CaptionML", GetProperty(optionButton.NextSibling, "CaptionML")));
              }
            }

            RemoveThisNodeFromParent(optionButton);

            Dictionary<String, OptionButton> currOptionSourceExpr1 = new Dictionary<string, OptionButton>();
            currOptionSourceExpr1.Add(id, optionSourceExpr);
            optionSourceExprList.Add(optionSourceExpr.ParentId + "_" + optionSourceExpr.SourceExpression, currOptionSourceExpr1);
          }
        }
      }

      foreach (KeyValuePair<String, Dictionary<String, OptionButton>> optionSource in optionSourceExprList)
      {
        String options = null;
        String option = null;
        foreach (KeyValuePair<String, OptionButton> optionValue1 in optionSource.Value)
        {
          option = optionValue1.Value.SourceExpression;
          options = (options == null ? optionValue1.Value.OptionValue : options + "," + optionValue1.Value.OptionValue);
        }

        CodeTransformationRules.UpdateMoveToPropertyDocument(
          Convert.ToString(metaDataDocMgt.GetCurrentPageId, CultureInfo.InvariantCulture),
          "", "", "", option + "Opt", "'" + options + "'", "");
        CodeTransformationRules.UpdateMoveToTriggerDocument(
          Convert.ToString(metaDataDocMgt.GetCurrentPageId, CultureInfo.InvariantCulture),
          "", "OnAfterGetRecord", "", "\r\n" + option + "Opt := " + option + ";", "append");
        CodeTransformationRules.PerformMoveToPropertyActions();
      }
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    public static void MoveUntouchedControlsToContentArea()
    {
      DoMoveUntouchedControlsToContentArea(@".//a:Controls/a:Control");
      DoMoveUntouchedControlsToContentArea(@".//a:Controls/a:Group");
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    public static void ManageControlType()
    {
      RemoveSpecielControls();
      ControlTypeTransformation();
      SetTreeViewProperties();
      ModifyBitmapsInGrid();
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    public static void TransformTabControlsToBands()
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      XmlNodeList tabControlNodeList = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Control[./a:Properties/a:Controltype='TabControl']", metaDataDocMgt.XmlNamespaceMgt);
      foreach (XmlNode tabControlNode in tabControlNodeList)
      {
        Dictionary<string,string> fixedIdMappingDictionary = new Dictionary<string,string>();
        
        if (metaDataDocMgt.MoveElementsDoc != null)
        {
          string tabId = GetProperty(tabControlNode, "ID");
          StringBuilder query = new StringBuilder("./a:MovePageElements/a:Page/a:TabControlNewIDs[../@ID=");
          query.Append(metaDataDocMgt.GetCurrentPageId.ToString(CultureInfo.InvariantCulture));
          query.Append(" and (@ID = '");
          query.Append(tabId);
          query.Append("')]");

          XmlNodeList FixedIdNodeList = metaDataDocMgt.MoveElementsDoc.SelectNodes(query.ToString(), metaDataDocMgt.XmlNamespaceMgt);
          foreach (XmlNode FixedIdNode in FixedIdNodeList)
          {
            string pageName = FixedIdNode.Attributes["PageNameML"].Value;
            string fixedIdValue = FixedIdNode.Attributes["NewID"].Value;
            if ((!string.IsNullOrEmpty(pageName)) || (!string.IsNullOrEmpty(fixedIdValue)))
            {
              fixedIdMappingDictionary.Add(pageName, fixedIdValue);
            }
          }
        }

        XmlNode tabControlPageNameMLNode = tabControlNode.FirstChild.SelectSingleNode(@"./a:PageNamesML", metaDataDocMgt.XmlNamespaceMgt);
        String pageNamesML = "ENU=General";

        Boolean hasPages = false;

        if (tabControlPageNameMLNode != null)
        {
          pageNamesML = tabControlPageNameMLNode.InnerText;

          hasPages = true;
        }

        XmlNode bandContainer = metaDataDocMgt.XmlDocument.CreateNode(XmlNodeType.Element, "Group", metaDataDocMgt.XmlNamespace);

        String[] tabPagesArrayML = GetPageNamesCaptionsArray(pageNamesML);

        Dictionary<int, int> tabMapping = new Dictionary<int, int>();
        int nextTab = 0;

        for (Int32 i = 0; i <= tabPagesArrayML.Length - 1; i++)
        {
          tabMapping.Add(i, nextTab);
          nextTab++;
          String parentControlNo;
          if (i == 0)
          {
            parentControlNo = tabControlNode.SelectSingleNode(@"./a:Properties/a:ID", metaDataDocMgt.XmlNamespaceMgt).InnerText;
          }
          else
          {
            parentControlNo =
              metaDataDocMgt.CalcId(null, XmlUtility.GetCaption(tabPagesArrayML[i]), "ContentArea").ToString(CultureInfo.InvariantCulture);
          }

          string fixedIdValue;
          if (fixedIdMappingDictionary.TryGetValue(tabPagesArrayML[i], out fixedIdValue))
          {
            parentControlNo = fixedIdValue;
          }

          XmlNode band = CreateBand(parentControlNo, tabPagesArrayML[i]);
          bandContainer.AppendChild(CopyPropertiesFromControl(tabControlNode, band));
          AddAdditionalProperiesToBands(tabControlNode, hasPages, i, band);
        }

        XmlNodeList inpageList = tabControlNode.SelectNodes(@"./a:Control/a:Properties/a:InPage", metaDataDocMgt.XmlNamespaceMgt);
        foreach (XmlNode inpageElement in inpageList)
        {
          int inPageId = tabMapping[Convert.ToInt32(inpageElement.InnerText, CultureInfo.InvariantCulture)];
          bandContainer.ChildNodes[inPageId].AppendChild(inpageElement.ParentNode.ParentNode);
        }

        inpageList = tabControlNode.SelectNodes(@"./a:Group/a:Properties/a:InPage", metaDataDocMgt.XmlNamespaceMgt);
        foreach (XmlNode inpageElement in inpageList)
        {
          int newTabId = tabMapping[Convert.ToInt32(inpageElement.InnerText, CultureInfo.InvariantCulture)];
          bandContainer.ChildNodes[newTabId].AppendChild(inpageElement.ParentNode.ParentNode);
        }

        // Move related Actions
        XmlNodeList inpageListActions =
          tabControlNode.SelectNodes(
              @".//a:ActionGroup//a:Action//a:InPage",
              metaDataDocMgt.XmlNamespaceMgt);
        foreach (XmlNode inpageElement in inpageListActions)
        {
          int currentTab = 0;
          if (hasPages)
          {
            currentTab = Convert.ToInt32(inpageElement.InnerText, CultureInfo.InvariantCulture);
          }

          XmlNode actionGroup = PageActions.GetBandActionGroupNode(bandContainer.ChildNodes[currentTab]);
          XmlNode actions =
            tabControlNode.SelectSingleNode(
            ".//a:ActionGroup//a:Action[./a:InPage=" + inpageElement.InnerText + "]",
            metaDataDocMgt.XmlNamespaceMgt);

          CleanBandsAction(actions);
          actionGroup.AppendChild(actions);
          XmlNode propNode = bandContainer.ChildNodes[currentTab].SelectSingleNode("./a:Properties", metaDataDocMgt.XmlNamespaceMgt);
          if (propNode != null)
          {
            bandContainer.ChildNodes[currentTab].InsertAfter(actionGroup, propNode);
          }
          else
          {
            TransformationLog.GenericLogEntry("Can't find Properties node. Can't move Action to related Band", LogCategory.IgnoreWarning);
          }
        }

        if (tabControlNode.ParentNode.Name == "Group")
        {
          XmlNode contentArea = tabControlNode;
          for (Int32 i = bandContainer.ChildNodes.Count - 1; i >= 0; i--)
          {
            tabControlNode.ParentNode.InsertAfter(bandContainer.ChildNodes[i], tabControlNode);
          }
        }
        else
        {
          XmlNode contentArea = GetPageControlNode("ContentArea");
          for (Int32 i = bandContainer.ChildNodes.Count - 1; i >= 0; i--)
          {
            contentArea.InsertAfter(bandContainer.ChildNodes[i], contentArea.FirstChild);
          }
        }
      }

      for (Int32 i = tabControlNodeList.Count - 1; i >= 0; i--)
      {
        RemoveThisNodeFromParent(tabControlNodeList[i]);
      }
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    /// <param name="id">?abc?</param>
    /// <param name="captionML">?abc?</param>
    /// <returns>?abc?</returns>
    public static XmlNode CreateBand(String id, String captionML)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;

      XmlNode band = metaDataDocMgt.XmlDocument.CreateNode(XmlNodeType.Element, "Group", metaDataDocMgt.XmlNamespace);

      XmlNode bandProperties = metaDataDocMgt.XmlDocument.CreateNode(XmlNodeType.Element, "Properties", metaDataDocMgt.XmlNamespace);
      band.AppendChild(bandProperties);
      if (id != null)
      {
        bandProperties.AppendChild(XmlUtility.CreateXmlElement("ID", id));
      }

      if (captionML != null)
      {
        bandProperties.AppendChild(XmlUtility.CreateXmlElement("CaptionML", captionML));
      }

      return band;
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    /// 
    public static void TransformTableBoxToRepeater()
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      XmlNodeList tabControlNodeList = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Control[./a:Properties/a:Controltype='TableBox']", metaDataDocMgt.XmlNamespaceMgt);
      foreach (XmlNode tabControlNode in tabControlNodeList)
      {
        string parentControlNo = tabControlNode.SelectSingleNode(@"./a:Properties/a:ID", metaDataDocMgt.XmlNamespaceMgt).InnerText;
        XmlNode bandContainer = metaDataDocMgt.XmlDocument.CreateNode(XmlNodeType.Element, "Group", metaDataDocMgt.XmlNamespace);
        bandContainer.AppendChild(CopyPropertiesFromControl(tabControlNode, CreateBand(parentControlNo, null)));

        XmlNodeList inpageList =
          metaDataDocMgt.XmlCurrentFormNode.SelectNodes(
              @".//a:Control[./a:Properties/a:ParentControl='" + parentControlNo + "']",
              metaDataDocMgt.XmlNamespaceMgt);

        foreach (XmlNode inpageElement in inpageList)
        {
          XmlNode properties = bandContainer.ChildNodes[0].SelectSingleNode("./a:Properties", metaDataDocMgt.XmlNamespaceMgt);
          if (properties == null)
          {
            properties = bandContainer.ChildNodes[0].AppendChild(XmlUtility.CreateXmlElement("Properties"));
          }

          if (properties.SelectSingleNode("./a:GroupType", metaDataDocMgt.XmlNamespaceMgt) == null)
          {
            properties.AppendChild(XmlUtility.CreateXmlElement("GroupType", "Repeater"));
          }

          bandContainer.ChildNodes[0].AppendChild(inpageElement);
        }

        if (tabControlNode.ParentNode.Name == "Group")
        {
          XmlNode contentArea = tabControlNode;
          for (Int32 i = bandContainer.ChildNodes.Count - 1; i >= 0; i--)
          {
            tabControlNode.ParentNode.InsertAfter(bandContainer.ChildNodes[i], tabControlNode);
          }
        }
        else
        {
          XmlNode contentArea = GetPageControlNode("ContentArea");
          for (Int32 i = bandContainer.ChildNodes.Count - 1; i >= 0; i--)
          {
            contentArea.InsertAfter(bandContainer.ChildNodes[i], contentArea.FirstChild);
          }
        }
      }

      for (Int32 i = tabControlNodeList.Count - 1; i >= 0; i--)
      {
        RemoveThisNodeFromParent(tabControlNodeList[i]);
      }
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    public static void TransformFrameToGroup()
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      XmlNodeList tabControlNodeList = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Control[./a:Properties/a:Controltype='Frame']", metaDataDocMgt.XmlNamespaceMgt);

      bool addStackGroup = false;
      if (metaDataDocMgt.InsertElementsDoc != null)
      {
        string query = "./a:TransformPages/a:Page[@ID='" + metaDataDocMgt.GetCurrentPageId.ToString(CultureInfo.InvariantCulture) + "' and ./a:Transformation/@FormType = 'Stack']";
        XmlNode stackNode = metaDataDocMgt.InsertElementsDoc.SelectSingleNode(query, metaDataDocMgt.XmlNamespaceMgt);
        if (stackNode != null)
        {
          addStackGroup = true;
        }
      }

      // foreach (XmlNode tabControlNode in tabControlNodeList)
      for (Int32 itoFixProblem = 0; itoFixProblem <= tabControlNodeList.Count - 1; itoFixProblem++)
      {
        XmlNode tabControlNode = tabControlNodeList[itoFixProblem];
        String captionML = null;
        String parentControlNo = tabControlNode.SelectSingleNode(@"./a:Properties/a:ID", metaDataDocMgt.XmlNamespaceMgt).InnerText;
        XmlNode frameCaptionNode = tabControlNode.SelectSingleNode(@"./a:Properties/a:CaptionML", metaDataDocMgt.XmlNamespaceMgt);
        if (frameCaptionNode != null)
        {
          captionML = frameCaptionNode.InnerText;
        }

        XmlNode bandContainer = metaDataDocMgt.XmlDocument.CreateNode(XmlNodeType.Element, "Group", metaDataDocMgt.XmlNamespace);
        XmlNode band = CreateBand(parentControlNo, captionML);
        if (addStackGroup)
        {
          band.FirstChild.AppendChild(XmlUtility.CreateXmlElement("GroupType", "CueGroup"));
        }

        bandContainer.AppendChild(CopyPropertiesFromControl(tabControlNode, band));
        AddAdditionalProperiesToBands(tabControlNode, false, 0, band);

        XmlNodeList groupNodeList = tabControlNode.SelectNodes(@"./a:Group", metaDataDocMgt.XmlNamespaceMgt);
        foreach (XmlNode group in groupNodeList)
        {
          band.AppendChild(group);
        }

        if (addStackGroup)
        {
          XmlNodeList actionsList = tabControlNode.SelectNodes(@"./a:Action", metaDataDocMgt.XmlNamespaceMgt);
          foreach (XmlNode inpageElement in actionsList)
          {
            RemoveThisNodeFromParent(inpageElement);
            bandContainer.ChildNodes[0].AppendChild(inpageElement);
          }
        }

        XmlNodeList inpageList =
          metaDataDocMgt.XmlCurrentFormNode.SelectNodes(
            @".//a:Control[./a:Properties/a:ParentControl='" + parentControlNo + "']",
            metaDataDocMgt.XmlNamespaceMgt);
        foreach (XmlNode inpageElement in inpageList)
        {
          bandContainer.ChildNodes[0].AppendChild(inpageElement);
        }

        if (tabControlNode.ParentNode.Name == "Group")
        {
          XmlNode contentArea = tabControlNode;
          for (Int32 i = bandContainer.ChildNodes.Count - 1; i >= 0; i--)
          {
            tabControlNode.ParentNode.InsertAfter(bandContainer.ChildNodes[i], tabControlNode);
          }
        }
        else
        {
          XmlNode contentArea = GetPageControlNode("ContentArea");
          for (Int32 i = bandContainer.ChildNodes.Count - 1; i >= 0; i--)
          {
            contentArea.InsertAfter(bandContainer.ChildNodes[i], contentArea.FirstChild);
          }
        }
      }

      for (Int32 i = tabControlNodeList.Count - 1; i >= 0; i--)
      {
        RemoveThisNodeFromParent(tabControlNodeList[i]);
      }
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    public static void CleanTriggerNode()
    {
      /* TODO SHOULD BE MOVED!!! */
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      XmlUtility.DeleteNodeList(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@"./a:Controls//a:Control/a:Triggers/a:OnAfterValidate", metaDataDocMgt.XmlNamespaceMgt));
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    public static void MoveElementsFromPropertiesToTriggerNode()
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      InsertTriggerChild(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@"./a:Controls//a:Control/a:Properties/a:OnActivate", metaDataDocMgt.XmlNamespaceMgt));
      InsertTriggerChild(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@"./a:Controls//a:Control/a:Properties/a:OnDeactivate", metaDataDocMgt.XmlNamespaceMgt));
      InsertTriggerChild(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@"./a:Controls//a:Control/a:Properties/a:OnFormat", metaDataDocMgt.XmlNamespaceMgt));
      InsertTriggerChild(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@"./a:Controls//a:Control/a:Properties/a:OnBeforeInput", metaDataDocMgt.XmlNamespaceMgt));
      InsertTriggerChild(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@"./a:Controls//a:Control/a:Properties/a:OnInputChange", metaDataDocMgt.XmlNamespaceMgt));
      InsertTriggerChild(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@"./a:Controls//a:Control/a:Properties/a:OnAfterInput", metaDataDocMgt.XmlNamespaceMgt));
      InsertTriggerChild(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@"./a:Controls//a:Control/a:Properties/a:OnValidate", metaDataDocMgt.XmlNamespaceMgt));
      InsertTriggerChild(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@"./a:Controls//a:Control/a:Properties/a:OnAfterValidate", metaDataDocMgt.XmlNamespaceMgt));
      InsertTriggerChild(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@"./a:Controls//a:Control/a:Properties/a:OnAfterGetRecord", metaDataDocMgt.XmlNamespaceMgt));
      InsertTriggerChild(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@"./a:Controls//a:Control/a:Properties/a:OnLookup", metaDataDocMgt.XmlNamespaceMgt));
      InsertTriggerChild(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@"./a:Controls//a:Control/a:Properties/a:OnDrillDown", metaDataDocMgt.XmlNamespaceMgt));
      InsertTriggerChild(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@"./a:Controls//a:Control/a:Properties/a:OnAssistEdit", metaDataDocMgt.XmlNamespaceMgt));
      InsertTriggerChild(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@"./a:Controls//a:Control/a:Properties/a:OnPush", metaDataDocMgt.XmlNamespaceMgt));
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    /// <param name="pageControlName">?abc?</param>
    /// <returns>?abc?</returns>
    public static XmlNode GetPageControlNode(String pageControlName)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      if (metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode("./a:Controls", metaDataDocMgt.XmlNamespaceMgt) == null)
      {
        XmlNode pageControlsNode = metaDataDocMgt.XmlDocument.CreateNode(XmlNodeType.Element, "Controls", metaDataDocMgt.XmlNamespace);
        XmlNode pageNode = metaDataDocMgt.XmlCurrentFormNode;
        pageNode.AppendChild(pageControlsNode);
        XmlUtility.InsertNodeWithPropertyChild(pageControlsNode, pageControlName, metaDataDocMgt.CalcId(GetProperty(pageControlsNode, "NewID"), "Controls", "ContentArea") /*metaDataDocMgt.GetNewId*/);

        return (metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode("./a:Controls/a:" + pageControlName, metaDataDocMgt.XmlNamespaceMgt));
      }

      if (metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode("./a:Controls/a:" + pageControlName, metaDataDocMgt.XmlNamespaceMgt) == null)
      {
        XmlNode pageActionsNode = metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode("./a:Controls", metaDataDocMgt.XmlNamespaceMgt);
        XmlUtility.InsertNodeWithPropertyChild(pageActionsNode, pageControlName, metaDataDocMgt.CalcId(null, null, pageControlName) /*metaDataDocMgt.GetNewId*/);
        return (metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode("./a:Controls/a:" + pageControlName, metaDataDocMgt.XmlNamespaceMgt));
      }
      else
      {
        return (metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode("./a:Controls/a:" + pageControlName, metaDataDocMgt.XmlNamespaceMgt));
      }
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    public static void ManageLabels()
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      string logStr = string.Empty;

      XmlNodeList labelList = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(
            @"./a:Controls//a:Control[./a:Properties/a:Controltype='Label']",
            metaDataDocMgt.XmlNamespaceMgt);

      foreach (XmlNode label in labelList)
      {
        string labelID = GetProperty(label, "ID");
        if (string.IsNullOrEmpty(labelID))
        {
          continue;
        }

        if (label.ParentNode.Name == "Controls")
        {
          XmlNode properties = metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode("./a:Properties", metaDataDocMgt.XmlNamespaceMgt);
          if (properties == null)
          {
            properties = metaDataDocMgt.XmlCurrentFormNode.AppendChild(XmlUtility.CreateXmlElement("Properties"));
          }

          ManageInstructionalTextML(labelID, label, true, properties);
        }
        else
        {
          XmlNode nodeParentProperties = label.ParentNode.SelectSingleNode("./a:Properties/a:Controltype", metaDataDocMgt.XmlNamespaceMgt);

          if (nodeParentProperties == null)
          {
            logStr = string.Format(
              CultureInfo.InvariantCulture,
              Resources.ParentControlDoesNotSupportControlTypeProperty,
              labelID);
            TransformationLog.GenericLogEntry(logStr, LogCategory.ValidateManually, metaDataDocMgt.GetCurrentPageId, Resources.ParentControlDoesNotSupportControlTypePropertySuggestion);
            continue;
          }

          switch (nodeParentProperties.InnerText)
          {
            case "Label":
            case "TextBox":
            case "CheckBox":
            case "OptionButton":
            case "CommandButton":
            case "MenuButton":
            case "MenuItem":
            case "Indicator":
            case "PictureBox":
              {
                ManageLabelsForMainControlTypes(label, labelID, nodeParentProperties);
                continue;
              }
            case "TabControl":
            case "Frame":
              {
                XmlNode parentNode = label.ParentNode.FirstChild;
                ManageInstructionalTextML(labelID, label, false, parentNode);
                continue;
              }

            default:
              continue;
          }
        }
      }

      ManageIndentation();
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    public static void ManageFixedLayout()
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;

      if (metaDataDocMgt.MoveElementsDoc == null)
      {
        return;
      }

      StringBuilder query = new StringBuilder("./a:MovePageElements/a:Page[@ID=");
      query.Append(metaDataDocMgt.GetCurrentPageId.ToString(CultureInfo.InvariantCulture));
      query.Append("]");

      XmlNodeList movePageElementList = metaDataDocMgt.MoveElementsDoc.SelectNodes(query.ToString(), metaDataDocMgt.XmlNamespaceMgt);
      foreach (XmlNode movePageElement in movePageElementList)
      {
        XmlNodeList fixedLayoutList = movePageElement.SelectNodes(@"./a:FixedLayout", metaDataDocMgt.XmlNamespaceMgt);
        XmlNode[] firstControls = new XmlNode[fixedLayoutList.Count];
        String[] firstControlsCaptions = new string[fixedLayoutList.Count];
        int counter = 0;
        foreach (XmlNode fixedLayout in fixedLayoutList)
        {
          string controlID = fixedLayout.Attributes.GetNamedItem("ID").Value;
          XmlNode control = metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(@"./a:Controls//a:Control[./a:Properties/a:ID=" + controlID + "]", metaDataDocMgt.XmlNamespaceMgt);
          if (control == null)
          {
            string logStr = string.Format(CultureInfo.InvariantCulture, Resources.MatrixLikeLayoutCannotFindControl, controlID);
            TransformationLog.GenericLogEntry(logStr, LogCategory.ChangeCodeManually, metaDataDocMgt.GetCurrentPageId);
            continue;
          }

          firstControls[counter] = control;
          firstControlsCaptions[counter] = GetProperty(control, "ID");
          counter++;
        }

        if (counter == 0)
        {
          continue;
        }

        SortedList containerGroup = new SortedList();
        for (int i = 0; counter - 1 >= i; i++)
        {
          XmlNode columnCaptionLabel = firstControls[i];
          String newID =
            metaDataDocMgt.CalcId(
                GetProperty(firstControls[i], "NewID"), firstControlsCaptions[i], "ContentArea").ToString(CultureInfo.InvariantCulture);

          XmlNode group = 
            CreateGroup(columnCaptionLabel, new string[1] { "CaptionML" }, null, newID);

          XmlNode parentControl = columnCaptionLabel;
          XmlNode parentGroupControl = parentControl.ParentNode;
          while (!IsGroup(parentGroupControl))
          {
            if (parentGroupControl == null)
            {
              break;
            }

            parentControl = parentControl.ParentNode;
            parentGroupControl = parentControl.ParentNode;
          }

          string parentControlID = NodeValue(parentGroupControl, "./a:Properties/a:ID");
          string parentControlInPage = NodeValue(parentControl, "./a:Properties/a:InPage");
          string key = parentControlID + "." + parentControlInPage;

          StringBuilder queryBuilder = new StringBuilder(@"./a:Controls//a:Control/a:Properties[");
          if (!string.IsNullOrEmpty(parentControlInPage))
          {
            queryBuilder.Append(" ./a:InPage=");
            queryBuilder.Append(parentControlInPage);
            queryBuilder.Append(" and ");
          }

          string search = string.Empty;
          int xPos = 0, yPos = 0;
          const int Xmove = 0;
          bool success = false;

          XmlNode widthNode = columnCaptionLabel.SelectSingleNode(".//a:Width", metaDataDocMgt.XmlNamespaceMgt);
          if (widthNode != null)
          {
            int width = Convert.ToInt32(widthNode.InnerText, CultureInfo.InvariantCulture) / 2;
            success = GenerateIndentationSearchString(columnCaptionLabel, ref xPos, ref yPos, Xmove, ref search, width);
          }
          else
          {
            success = GenerateIndentationSearchString(columnCaptionLabel, ref xPos, ref yPos, Xmove, ref search);
          }

          if (!success)
          {
            // can't find XPos or YPos node
            continue;
          }

          queryBuilder.Append(search);
          XmlNodeList subControlsList = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(queryBuilder.ToString(), metaDataDocMgt.XmlNamespaceMgt);
          foreach (XmlNode parentTextBoxNode in subControlsList)
          {
            string parentTextBoxYPos = parentTextBoxNode.SelectSingleNode("./a:YPos", metaDataDocMgt.XmlNamespaceMgt).InnerText;

            // Clean CaptionML nodes.
            StringBuilder queryLabelsToBeCleaned = new StringBuilder(@"./a:Controls//a:Control/a:Properties/a:CaptionML [");
            if (!string.IsNullOrEmpty(parentControlInPage))
            {
              queryLabelsToBeCleaned.Append(" ../a:InPage=");
              queryLabelsToBeCleaned.Append(parentControlInPage);
              queryLabelsToBeCleaned.Append(" and ");
            }

            queryLabelsToBeCleaned.Append("../a:Controltype = 'Label' and ../a:YPos = ");
            queryLabelsToBeCleaned.Append(parentTextBoxYPos);
            queryLabelsToBeCleaned.Append(" and ../a:XPos <= ");
            queryLabelsToBeCleaned.Append(xPos.ToString(CultureInfo.InvariantCulture));
            queryLabelsToBeCleaned.Append("]");
            {
              // Clean CaptionML nodes.
              XmlNodeList captionMLsToClean = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(queryLabelsToBeCleaned.ToString(), metaDataDocMgt.XmlNamespaceMgt);
              foreach (XmlNode captionML in captionMLsToClean)
              {
                SearchForParentForStandaloneLabel(captionML.ParentNode);
                RemoveThisNodeFromParent(captionML);
              }
            }

            int ycompare = (Convert.ToInt32(parentTextBoxYPos, CultureInfo.InvariantCulture) - yPos);
            if ((ycompare < 0) || (ycompare > 660))
            {
              continue;
            }

            yPos = Convert.ToInt32(parentTextBoxYPos, CultureInfo.InvariantCulture);
            RemoveThisNodeFromParent(parentTextBoxNode.ParentNode);
            group.AppendChild(parentTextBoxNode.ParentNode);
          }

          XmlNode mainGroup;
          if (containerGroup.Contains(key))
          {
            mainGroup = (XmlNode)containerGroup.GetByIndex(containerGroup.IndexOfKey(key));
          }
          else
          {
            mainGroup = CreateGroup(columnCaptionLabel, null, null, metaDataDocMgt.CalcId(null, key.ToString(CultureInfo.InvariantCulture), "ContentArea").ToString(CultureInfo.InvariantCulture));
            mainGroup.FirstChild.AppendChild(XmlUtility.CreateXmlElement("GroupType", "FixedLayout"));
            parentGroupControl.AppendChild(mainGroup);
            containerGroup.Add(key, mainGroup);
          }

          mainGroup.AppendChild(group);
          {
            // Clean CaptionML nodes.
            RemoveThisNodeFromParent(columnCaptionLabel.SelectSingleNode(".//a:CaptionML", metaDataDocMgt.XmlNamespaceMgt));
          }
        }
      }
    }

    /// <summary>
    /// Finalize controls processing
    /// </summary>
    public static void FinalizeControlsProcessing()
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;

      FinalizeTrendscape();

      XmlNodeList groupList = metaDataDocMgt.XmlCurrentFormNode.SelectNodes("./a:Controls//a:Group", metaDataDocMgt.XmlNamespaceMgt);
      foreach (XmlNode group in groupList)
      {
        if (group.ChildNodes.Count < 2)
        {
          RemoveThisNodeFromParent(group);
        }
      }

      // add Options band to Request Pages
      if (metaDataDocMgt.GetCurrentPageId < 0)
      {
        XmlNodeList requestPageControls = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Controls/a:ContentArea/*", metaDataDocMgt.XmlNamespaceMgt);
        if (requestPageControls.Count > 1)
        {
          XmlNode options = 
            CreateGroup(null, null, "ENU=Options", metaDataDocMgt.CalcId(null, null, "ReportOptions").ToString(CultureInfo.InvariantCulture));
          foreach (XmlNode control in requestPageControls)
          {
            if (control.Name != "Properties")
            {
              RemoveThisNodeFromParent(control);
              options.AppendChild(control);
            }
            else
            {
              control.ParentNode.AppendChild(options);
            }
          }
        }
      }
    }

    /// <summary>
    /// Search for text boxes which serves as Labels. (for mor information search for Dynamic Captions).
    /// </summary>
    public static void SearchCaptionsInTextBoxes()
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      XmlNodeList textBoxPropertiesList = metaDataDocMgt.XmlCurrentFormNode.SelectNodes("./a:Controls//a:Control/a:Properties [ ./a:Controltype = 'TextBox' and ./a:Focusable = 'No' and boolean(./a:SourceExpr)]", metaDataDocMgt.XmlNamespaceMgt);

      foreach (XmlNode textBoxProperties in textBoxPropertiesList)
      {
        string sourceExpr = "FORMAT (" + XmlUtility.GetNodeValue(textBoxProperties, "./a:SourceExpr", string.Empty) + ")";

        string parentControlId = XmlUtility.GetNodeValue(textBoxProperties, "./a:ParentControl", string.Empty);
        if (!string.IsNullOrEmpty(parentControlId))
        {
          XmlNode parentControl = metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(
            string.Format(
              CultureInfo.InvariantCulture,
              "./a:Controls//a:Control/a:Properties [ ( ./a:Controltype = 'TextBox' or ./a:Controltype = 'OptionButton') and ./a:ID = '{0}' ]",
              parentControlId),
            metaDataDocMgt.XmlNamespaceMgt);

          if (parentControl != null)
          {
            CopyProperty(parentControl, "CaptionClass", sourceExpr);
            RemoveThisNodeFromParent(textBoxProperties.ParentNode);

            string logStr = string.Format(
              CultureInfo.InvariantCulture,
              Resources.TextBoxAsLabel,
              Resources.TextBoxAsLabelByParentChild,
              XmlUtility.GetNodeValue(textBoxProperties, "./a:ID", string.Empty),
              sourceExpr,
              parentControlId);
            TransformationLog.GenericLogEntry(logStr, LogCategory.ValidateManually, metaDataDocMgt.GetCurrentPageId);

            continue;
          }
        }

        if (metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(
          string.Format(
            CultureInfo.InvariantCulture,
            "./a:Controls//a:Control [ ./a:Properties/a:Controltype = 'Label' and ./a:Properties/a:ParentControl = '{0}' ]",
            XmlUtility.GetNodeValue(textBoxProperties, "./a:ID", string.Empty)),
          metaDataDocMgt.XmlNamespaceMgt) != null)
        {
          continue;
        }

        if (metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(
          string.Format(
            CultureInfo.InvariantCulture,
            "./a:Controls//a:Group [ ./a:Properties/a:GroupType = 'FixedLayout' and .//a:Control/a:Properties/a:ID = '{0}' ]",
            XmlUtility.GetNodeValue(textBoxProperties, "./a:ID", string.Empty)),
          metaDataDocMgt.XmlNamespaceMgt) != null)
        {
          continue;
        }

        // if (GetNodeValue(textBoxProperties, "./a:SourceExpr", "Not set").Contains("Post Code"))
        // {
        //   continue;
        // }

        string searchString = string.Empty;
        if (!GenerateLabelSearchString(textBoxProperties, ref searchString, "./a:Controls//a:Control/a:Properties"))
        {
          continue;
        }

        XmlNodeList candidateList = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(searchString, metaDataDocMgt.XmlNamespaceMgt);
        if (candidateList.Count == 0)
        {
          if (GetNodeValue(textBoxProperties, "./a:Width") > 6100)
          {
            CopyProperty(textBoxProperties, "CaptionClass", sourceExpr);
            RemoveThisNodeFromParent(textBoxProperties.SelectSingleNode(@"./a:SourceExpr", metaDataDocMgt.XmlNamespaceMgt));

            string logStr = string.Format(
              CultureInfo.InvariantCulture,
              Resources.TextBoxAsLabel,
              Resources.TextBoxAsLabelBywidth,
              XmlUtility.GetNodeValue(textBoxProperties, "./a:ID", string.Empty),
              sourceExpr,
              XmlUtility.GetNodeValue(textBoxProperties, "./a:ID", string.Empty));

            TransformationLog.GenericLogEntry(logStr, LogCategory.ValidateManually, metaDataDocMgt.GetCurrentPageId);
          }

          continue;
        }
        else if (candidateList.Count == 1)
        {
          string logStr = string.Format(
            CultureInfo.InvariantCulture,
            Resources.TextBoxAsLabel,
            Resources.TextBoxAsLabelByPosition,
            XmlUtility.GetNodeValue(textBoxProperties, "./a:ID", string.Empty),
            sourceExpr,
            XmlUtility.GetNodeValue(candidateList[0], ".//a:ID", string.Empty));

          CopyProperty(candidateList[0], "CaptionClass", sourceExpr);
          RemoveThisNodeFromParent(textBoxProperties.ParentNode);

          TransformationLog.GenericLogEntry(logStr, LogCategory.ValidateManually, metaDataDocMgt.GetCurrentPageId);
          continue;
        }
        else
        {
          string logStr = string.Format(
            CultureInfo.InvariantCulture,
            Resources.TextBoxAsLabelProbably,
            XmlUtility.GetNodeValue(textBoxProperties, "./a:ID", string.Empty),
            sourceExpr,
            candidateList.Count.ToString(CultureInfo.InvariantCulture));
          TransformationLog.GenericLogEntry(logStr, LogCategory.ValidateManually, metaDataDocMgt.GetCurrentPageId);
        }
      }
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    public static void DeleteLabels()
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;

      XmlNodeList labelsToDelete = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(
        @"./a:Controls//a:Control/a:Properties[./a:Controltype='Label' and boolean(./a:CaptionML) and boolean(./a:ID) and (./a:ForeColor!=12632256 or not (boolean(./a:ForeColor)))]",
        metaDataDocMgt.XmlNamespaceMgt);

      // move CaptionMLs to variable (for localization) and assign this variable to CaptionClass.
      if (labelsToDelete.Count > 0)
      {
        foreach (XmlNode label in labelsToDelete)
        {
          if (!SearchForParentForStandaloneLabel(label))
          {
            string captionClassValue = XmlUtility.GetNodeValue(label, "./a:CaptionML", string.Empty);
            captionClassValue = captionClassValue.Replace(Environment.NewLine, string.Empty);
            captionClassValue = captionClassValue.Replace("'", "''");
            string newVarId = metaDataDocMgt.CalcId(null, "ENU=" + XmlUtility.GetCaption(captionClassValue), "Code").ToString(CultureInfo.InvariantCulture);
            string newVarName = "Text" + newVarId; 

            CodeTransformationRules.UpdateMoveToPropertyDocument(
              Convert.ToString(metaDataDocMgt.GetCurrentPageId, CultureInfo.InvariantCulture),
              "", "", "", newVarName, "TextConst '" + captionClassValue + "'", "");

            CopyProperty(label, "CaptionClass", newVarName);

            XmlNode controltype = label.SelectSingleNode(@"./a:Controltype", metaDataDocMgt.XmlNamespaceMgt);
            controltype.InnerText = "exLabel";

            RemoveThisNodeFromParent(label.SelectSingleNode(@"./a:CaptionML", metaDataDocMgt.XmlNamespaceMgt));

            string logStr = string.Format(
              CultureInfo.InvariantCulture,
              Resources.MoveCaptionMLtoCaptionClass,
              captionClassValue,
              XmlUtility.GetNodeValue(label, "./a:ID", string.Empty),
              newVarName,
              newVarId);

            TransformationLog.GenericLogEntry(logStr, LogCategory.ValidateManually, metaDataDocMgt.GetCurrentPageId);
          }
        }
      }

      XmlUtility.DeleteNodeList(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@"./a:Controls//a:Control[./a:Properties/a:Controltype='Label']", metaDataDocMgt.XmlNamespaceMgt));
    }

    /// <summary>
    /// Aligning Confirmation Dialogs pages with UX requirements
    /// </summary>
    public static void AlignConfirmationDialogs()
    {
      //TODO: Finish this function according to 11453, 11957, 11965 in M7
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      if (metaDataDocMgt.InsertElementsDoc == null)
      {
        return;
      }

      string pageId = metaDataDocMgt.GetCurrentPageId.ToString(CultureInfo.InvariantCulture);
      XmlNode TMP = metaDataDocMgt.InsertElementsDoc.SelectSingleNode(
        string.Format(CultureInfo.InvariantCulture, "./a:TransformPages/a:Page[@ID='{0}' and ./a:Properties/a:PageType = 'ConfirmationDialog']", pageId),
        metaDataDocMgt.XmlNamespaceMgt);
      if (TMP == null)
      {
        return;
      }

      XmlNode firstGroupNode = metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(
        ".//a:Controls/a:ContentArea/a:Group",
        metaDataDocMgt.XmlNamespaceMgt);
      if (firstGroupNode == null)
      {
        // We don't have any group, so we can't:
        //    Add the first FastTab to CaptionML="Details"
        //    Move the question (labels) from tab directly onto the page;
        return;
      }

      //    Add the first FastTab to CaptionML="Details"
      CopyProperty(firstGroupNode.SelectSingleNode("./a:Properties", metaDataDocMgt.XmlNamespaceMgt), "CaptionML", "ENU=Details");

      // It cause problems in forms 591, 99000833, and 6079.
      // //    Move the question (labels) from tab directly onto the page;
      // XmlNode instructionalTextNode = firstGroupNode.ParentNode.SelectSingleNode("./a:Properties/a:InstructionalTextML", metaDataDocMgt.XmlNamespaceMgt);
      // if (instructionalTextNode == null)
      // {
      //   if (firstGroupNode.SelectSingleNode("./a:Properties/a:InstructionalTextML", metaDataDocMgt.XmlNamespaceMgt) == null)
      //   {
      //     XmlNode firstInstructionalText = firstGroupNode.SelectSingleNode("./a:Control[ boolean(./a:Properties/a:CaptionClass) and not(boolean(./a:Properties/a:SourceExpr))]", metaDataDocMgt.XmlNamespaceMgt);
      //     if (firstInstructionalText != null)
      //     {
      //       firstInstructionalText RemoveThisNodeFromParent(firstInstructionalText);
      //       firstGroupNode.ParentNode.InsertBefore(firstInstructionalText, firstGroupNode);
      //     }
      //   }
      // }
    }

    /// <summary>
    /// We need to log some exta information regarding each form.
    /// </summary>
    public static void LogExtraInformation()
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      // Bug 27809: RTC TableRelation validated on Request Forms, even if ValidateTableRelation=No
      XmlNodeList controlListForBug27809 =
        metaDataDocMgt.XmlCurrentFormNode.SelectNodes(
        @".//a:Controls//a:Control [./a:Properties/a:ValidateTableRelation='No' and boolean(./a:Properties/a:TableRelation)]", metaDataDocMgt.XmlNamespaceMgt);
      foreach (XmlNode control in controlListForBug27809)
      {
        string id = GetProperty(control, "ID");
        string logStr = string.Format(
          CultureInfo.InvariantCulture,
          Resources.ControlWithTableRelationAndValidateTableRelationEqualNo,
          id);
        TransformationLog.GenericLogEntry(logStr, LogCategory.Warning, metaDataDocMgt.GetCurrentPageId, null);
      }
    }
    #endregion

    #region Private static methods

    private static string NodeValue(XmlNode node, string query)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      XmlNode subNode = node.SelectSingleNode(query, metaDataDocMgt.XmlNamespaceMgt);
      if (subNode != null)
      {
        return subNode.InnerText;
      }

      return string.Empty;
    }

    private static void InsertTriggerChild(XmlNodeList triggerChilds)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      for (Int32 i = triggerChilds.Count - 1; i >= 0; i--)
      {
        XmlNode triggerChild = triggerChilds[i];
        if (triggerChild != null)
        {
          XmlNode controlNode = triggerChild.ParentNode.ParentNode;
          if (controlNode.SelectSingleNode(".\a:Triggers") == null)
          {
            XmlNode newNode = metaDataDocMgt.XmlDocument.CreateNode(XmlNodeType.Element, "Triggers", metaDataDocMgt.XmlNamespace);
            controlNode.AppendChild(newNode);
          }

          XmlNode triggerNode = controlNode.SelectSingleNode(".\a:Triggers");
          triggerNode.AppendChild(triggerChild);
        }
      }
    }

    private static void RemoveSpecielControls()
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;

      XmlUtility.DeleteNodeList(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@"./a:Controls//a:Control[./a:Properties/a:Controltype='Shape']", metaDataDocMgt.XmlNamespaceMgt));
      XmlUtility.DeleteNodeList(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@"./a:Controls//a:Control[./a:Properties/a:Controltype='Matrix']", metaDataDocMgt.XmlNamespaceMgt));
      XmlUtility.DeleteNodeList(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@"./a:Controls//a:Control[./a:Properties/a:Controltype='Image']", metaDataDocMgt.XmlNamespaceMgt));
      XmlUtility.DeleteNodeList(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@"./a:Controls//a:Control[./a:Properties/a:Controltype='Frame']", metaDataDocMgt.XmlNamespaceMgt));
    }

    private static void SetTreeViewProperties()
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      XmlNode expansionControl = metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(
        @".//a:Group/a:Properties[../a:Control/a:Properties/a:BitmapList = '47,46']",
        metaDataDocMgt.XmlNamespaceMgt);

      if (expansionControl != null)
      {
        expansionControl.AppendChild(XmlUtility.CreateXmlElement("ShowAsTree", "Yes"));
      }

      RemoveThisNodeFromParent(metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(
        @".//a:Group//a:Control[./a:Properties/a:BitmapList = '47,46']",
        metaDataDocMgt.XmlNamespaceMgt));
    }

    private static void ControlTypeTransformation()
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;

      XmlNodeList uiPartNodeList = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Controls//a:Control/a:Properties/a:Controltype[../a:Controltype!='MenuButton' and ../a:Controltype!='CommandButton' and ../a:Controltype='SubForm']", metaDataDocMgt.XmlNamespaceMgt);
      foreach (XmlNode controlType in uiPartNodeList)
      {
        controlType.InnerText = "Part";
      }

      XmlNodeList radionNodeList = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Controls//a:Control/a:Properties/a:Controltype[../a:Controltype='OptionButton']", metaDataDocMgt.XmlNamespaceMgt);
      foreach (XmlNode controlType in radionNodeList)
      {
        controlType.InnerText = "RadioButton";
      }

      XmlNodeList indicatorList = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Controls//a:Control/a:Properties/a:Controltype[../a:Controltype='Indicator']", metaDataDocMgt.XmlNamespaceMgt);
      foreach (XmlNode controlType in indicatorList)
      {
        controlType.InnerText = "ProgressControl";
      }

      XmlNodeList notUIPartNodeList = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Controls//a:Control/a:Properties/a:Controltype[../a:Controltype!='MenuButton' and ../a:Controltype!='CommandButton' and ../a:Controltype!='Part' and ../a:Controltype!='RadioButton' and ../a:Controltype!='ProgressControl']", metaDataDocMgt.XmlNamespaceMgt);
      foreach (XmlNode controlType in notUIPartNodeList)
      {
        if (controlType.InnerText == "PictureBox")
        {
          if (!string.IsNullOrEmpty(GetProperty(controlType.ParentNode.ParentNode, "BitmapList")))
          {
            XmlUtility.UpdateNodeInnerText(controlType.ParentNode, "Editable", "False");
          }
        }

        controlType.InnerText = "Field";
      }
    }

    /// <summary>
    /// This structure hold label's properties (Name, Value) and information about label (FormID, LabelID).
    /// </summary>
    /// <param name="controlType">?abc?</param>
    private struct LabelPropertyInfo
    {
      public int FormID;
      public string LabelID;
      public string PropName;
      public string PropValue;

      /// <summary>
      /// ?abc?
      /// </summary>
      /// <param name="formID">Form (Page) ID</param>
      /// <param name="labelID">Label ID</param>
      public LabelPropertyInfo(int formID, string labelID)
        : this(formID, labelID, "", "")
      {
      }

      /// <summary>
      /// ?abc?
      /// </summary>
      /// <param name="formID">Form (Page) ID</param>
      /// <param name="labelID">Label ID</param>
      /// <param name="propName">Property Name</param>
      /// <param name="propValue">Property Value</param>
      public LabelPropertyInfo(int formID, string labelID, string propName, string propValue)
      {
        FormID = formID;
        LabelID = labelID;
        PropName = propName;
        PropValue = propValue;
      }
    }

    private static string FindProperty(string propName, XmlNode node, MetadataDocumentManagement metaDataDocMgt)
    {
      //TODO we also have GetProperty. I think we should have only one.
      XmlNode propertiesList = node.SelectSingleNode("./a:Properties/a:" + propName, metaDataDocMgt.XmlNamespaceMgt);
      if (propertiesList != null)
      {
        return propertiesList.InnerText;
      }

      return string.Empty;
    }

    private static void LogIgnoreLabelProperty(LabelPropertyInfo propInfo, XmlNode label, MetadataDocumentManagement metaDataDocMgt)
    {
      string propVal = FindProperty(propInfo.PropName, label, metaDataDocMgt);
      if (!string.IsNullOrEmpty(propVal))
      {
        string logStr = string.Format(
          CultureInfo.InvariantCulture,
          Resources.PropertyWillBeIgnored,
          propInfo.LabelID,
          propInfo.PropName);
        TransformationLog.GenericLogEntry(logStr, LogCategory.IgnoreWarning, propInfo.FormID, null);
      }
    }

    private static void MovePropertyToParentControl(XmlNode parent, LabelPropertyInfo propInfo, MetadataDocumentManagement metaDataDocMgt)
    {
      if (!String.IsNullOrEmpty(propInfo.PropValue))
      {
        XmlNode nodeLabelProperty = parent.SelectSingleNode("./a:" + propInfo.PropName, metaDataDocMgt.XmlNamespaceMgt);
        if (nodeLabelProperty != null)
        {
          if (nodeLabelProperty.InnerText != propInfo.PropValue)
          {
            nodeLabelProperty.InnerText = propInfo.PropValue;
            string logStr = string.Format(
              CultureInfo.InvariantCulture,
              Resources.ParentControlPropertyWillBeChangedToLabelProperty,
              propInfo.LabelID,
              propInfo.PropName,
              nodeLabelProperty.InnerText,
              propInfo.PropValue);
            TransformationLog.GenericLogEntry(logStr, LogCategory.GeneralInformation, propInfo.FormID, null);
          }
        }
        else
        {
          XmlNode elem = XmlUtility.CreateXmlElement(propInfo.PropName, propInfo.PropValue);
          parent.AppendChild(elem);
        }
      }
    }

    private static void CleanBandsAction(XmlNode actionsToClean)
    {
      for (int i = 0; i < actionsToClean.ChildNodes.Count; i++)
      {
        switch (actionsToClean.ChildNodes[i].Name)
        {
          case "Controltype":
            CommentPropertyRemoval(actionsToClean.ChildNodes[i], actionsToClean.ChildNodes[i].Name);
            actionsToClean.RemoveChild(actionsToClean.ChildNodes[i]);
            break;
          default:
            break;
        }
      }
    }

    private static void CommentPropertyRemoval(XmlNode nodeToComment, string propertyName)
    {
      string newDescription =
        string.Format(
          CultureInfo.InvariantCulture,
          Resources.PropertyName,
          propertyName);
      PageActions.CommentRemoval(nodeToComment.ParentNode, newDescription);
    }

    private static void ManageInstructionalTextML(string labelID, XmlNode label, bool directAssignment, XmlNode parentNode)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;

      XmlNode nodeLabelParentControl = label.SelectSingleNode(
        "./a:Properties/a:ParentControl",
        metaDataDocMgt.XmlNamespaceMgt);
      if (nodeLabelParentControl != null)
      {
        XmlUtility.DeleteElements(nodeLabelParentControl, "./a:Properties/a:ID", metaDataDocMgt.XmlNamespaceMgt);
      }

      string captionML = FindProperty("CaptionML", label, metaDataDocMgt);
      if (parentNode != null)
      {
        if (String.IsNullOrEmpty(captionML))
        {
          return;
        }

        if (metaDataDocMgt.MoveElementsDoc != null)
        {
          StringBuilder query = new StringBuilder("./a:MovePageElements/a:Page[@ID=");
          query.Append(metaDataDocMgt.GetCurrentPageId.ToString(CultureInfo.InvariantCulture));
          query.Append(" and (./a:InstructionalTextML/@ID = '");
          query.Append(labelID);
          query.Append("')]");
          XmlNode movePageElementList = metaDataDocMgt.MoveElementsDoc.SelectSingleNode(query.ToString(), metaDataDocMgt.XmlNamespaceMgt);
          if (movePageElementList != null)
          {
            XmlNode tmpnode;
            if (directAssignment)
            {
              tmpnode = parentNode.AppendChild(XmlUtility.CreateXmlElement("InstructionalTextML", captionML));
            }
            else
            {
              tmpnode = parentNode.AppendChild(XmlUtility.CreateXmlElement("tempNode_InstructionalTextML"));
              tmpnode.AppendChild(XmlUtility.CreateXmlElement("InstructionalTextML", captionML));
            }

            LabelPropertyInfo propInfo = new LabelPropertyInfo(metaDataDocMgt.GetCurrentPageId, labelID);
            propInfo.PropName = "InPage";
            propInfo.PropValue = FindProperty(propInfo.PropName, label, metaDataDocMgt);
            MovePropertyToParentControl(tmpnode, propInfo, metaDataDocMgt);

            XmlUtility.DeleteElements(label, "./a:Properties/a:ID", metaDataDocMgt.XmlNamespaceMgt);
            return;
          }
        }

        if (!captionML.Contains("'"))
        {
          XmlNodeList dump = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(
            @"./a:Controls//a:Control[./a:Properties/a:CaptionML='" + captionML + "']",
            metaDataDocMgt.XmlNamespaceMgt);
          if (dump.Count != 1)
          {
            return;
          }
        }

        string search = string.Empty;
        int xPos = 0, yPos = 0;
        const int Xmove = 220;
        if (GenerateIndentationSearchString(label, ref xPos, ref yPos, Xmove, ref search))
        {
          XmlNodeList labelhierarchyList = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(
            @"./a:Controls//a:Control/a:Properties[./a:Controltype='Label' and " + search,
            metaDataDocMgt.XmlNamespaceMgt);

          if (labelhierarchyList.Count > 0)
          {
            return;
          }
        }

        if (metaDataDocMgt.MoveElementsDoc != null)
        {
          StringBuilder query = new StringBuilder("./a:MovePageElements/a:Page[@ID=");
          query.Append(metaDataDocMgt.GetCurrentPageId.ToString(CultureInfo.InvariantCulture));
          query.Append(" and (./a:FixedLayout/@ID = '");
          query.Append(labelID);
          query.Append("')]");
          XmlNode movePageElementList = metaDataDocMgt.MoveElementsDoc.SelectSingleNode(query.ToString(), metaDataDocMgt.XmlNamespaceMgt);
          if (movePageElementList != null)
          {
            return;
          }
        }
      }

      UserSetupManagement userSetup = UserSetupManagement.Instance;
      string logStr = string.Format(
        CultureInfo.InvariantCulture,
        Resources.InstructionalTextSuggestion,
        labelID,
        captionML,
        userSetup.MovePageElements);
      TransformationLog.GenericLogEntry(logStr, LogCategory.ChangeCodeManually, metaDataDocMgt.GetCurrentPageId, null);
      return;
    }

    [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    private static void ManageIndentation()
    {
      const int Xmove = 220;
      const int Ymove = 230;
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;

      XmlNodeList labelList = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(
            @"./a:Controls//a:Control[./a:Properties/a:Controltype='Label']",
            metaDataDocMgt.XmlNamespaceMgt);

      for (int i = 0; i < labelList.Count; i++)
      {
        XmlNode label = labelList[i];

        if (label.SelectSingleNode("./a:Properties/a:ID", metaDataDocMgt.XmlNamespaceMgt) == null)
        {
          // This label already have been used
          continue;
        }

        XmlNode group_Node = null;
        string labelParentControl = string.Empty, labelInPage = string.Empty;
        XmlNode nodeParentProperties = label.ParentNode.FirstChild.SelectSingleNode("./a:Controltype", metaDataDocMgt.XmlNamespaceMgt);
        if (nodeParentProperties != null)
        {
          switch (nodeParentProperties.InnerText)
          {
            case "TabControl":
              labelInPage = XmlUtility.GetNodeValue(label, ".//a:InPage", string.Empty);
              if (String.IsNullOrEmpty(labelInPage))
              {
                string logStr = string.Format(
                  CultureInfo.InvariantCulture,
                  Resources.InPageValueNotSet,
                  GetNodeValue(label, ".//a:ID"),
                  XmlUtility.GetNodeValue(label, ".//a:CaptionML", "Not set"));
                TransformationLog.GenericLogEntry(logStr, LogCategory.Error, metaDataDocMgt.GetCurrentPageId);
                continue;
              }

              break;
            case "Frame":
              labelInPage = string.Empty;
              break;
            default:
              continue;
          }

          XmlNode currentLabelParentControlIDnode = label.ParentNode.FirstChild.SelectSingleNode("./a:ID", metaDataDocMgt.XmlNamespaceMgt);
          if (currentLabelParentControlIDnode != null)
          {
            labelParentControl = currentLabelParentControlIDnode.InnerText;
          }
        }

        string search = string.Empty;
        int xPos = 0, labelBottomByY = 0, currentLabelYPosition = 0;
        if (!GenerateIndentationSearchString(label, ref xPos, ref labelBottomByY, Xmove, ref search))
        {
          continue;
        }

        StringBuilder query = new StringBuilder("./a:Controls//a:Control/a:Properties[(./a:Controltype='Label' or ./a:Controltype='CheckBox') and ");
        query.Append(search);

        XmlNodeList labelhierarchyList = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(
          query.ToString(),
          metaDataDocMgt.XmlNamespaceMgt);

        if (labelhierarchyList.Count == 0)
        {
          continue;
        }

        foreach (XmlNode n in labelhierarchyList)
        {
          XmlNode parentTextBoxNode;
          if (XmlUtility.GetNodeValue(n, ".//a:Controltype", string.Empty) == "Label")
          {
            parentTextBoxNode = GetParentControl(n);
            if (parentTextBoxNode == null)
            {
              continue;
            }
          }
          else
          {
            parentTextBoxNode = n.ParentNode;
          }

          XmlNode parentTextBoxNode_ParentControl = parentTextBoxNode.SelectSingleNode("./a:Properties/a:ParentControl", metaDataDocMgt.XmlNamespaceMgt);
          if (parentTextBoxNode_ParentControl != null)
          {
            string parentTextBoxID = parentTextBoxNode_ParentControl.InnerText;
            if ((!string.IsNullOrEmpty(parentTextBoxID)) && (parentTextBoxID != labelParentControl))
            {
              continue;
            }
          }

          XmlNode parentTextBoxNode_InPage = parentTextBoxNode.SelectSingleNode("./a:Properties/a:InPage", metaDataDocMgt.XmlNamespaceMgt);
          if (parentTextBoxNode_InPage != null)
          {
            string parentTextBox_InPageValue = parentTextBoxNode_InPage.InnerText;
            if (parentTextBox_InPageValue != "-1")
            {
              if (parentTextBox_InPageValue != labelInPage)
              {
                continue;
              }
            }
          }

          int nextLabelYPosition = GetNodeValue(parentTextBoxNode, ".//a:YPos");
          int ycompare = nextLabelYPosition - labelBottomByY;
          if (!(currentLabelYPosition == nextLabelYPosition) && (ycompare < 0) || (ycompare > Ymove))
          {
            continue;
          }

          int nextLabelHeight = GetNodeValue(parentTextBoxNode, ".//a:Height");
          labelBottomByY = nextLabelYPosition + nextLabelHeight;
          currentLabelYPosition = nextLabelYPosition;

          if (group_Node == null)
          {
            group_Node = metaDataDocMgt.XmlDocument.CreateNode(XmlNodeType.Element, "Group", metaDataDocMgt.XmlNamespace);
            XmlNode group_PropertiesNode = metaDataDocMgt.XmlDocument.CreateNode(XmlNodeType.Element, "Properties", metaDataDocMgt.XmlNamespace);

            group_PropertiesNode.AppendChild(XmlUtility.CreateXmlElement("ID", label.SelectSingleNode("./a:Properties/a:ID", metaDataDocMgt.XmlNamespaceMgt).InnerText));

            XmlNode label_Visible = label.SelectSingleNode(".//a:Visible", metaDataDocMgt.XmlNamespaceMgt);
            bool hideCaption = false;
            if (label_Visible != null)
            {
              if (label_Visible.InnerText == "No")
              {
                hideCaption = true;
              }
            }

            if (!hideCaption)
            {
              XmlNode label_CaptionML = label.SelectSingleNode(".//a:CaptionML", metaDataDocMgt.XmlNamespaceMgt);
              if (label_CaptionML != null)
              {
                group_PropertiesNode.AppendChild(XmlUtility.CreateXmlElement("CaptionML", label_CaptionML.InnerText));
              }
            }

            if (!string.IsNullOrEmpty(labelParentControl))
            {
              group_PropertiesNode.AppendChild(XmlUtility.CreateXmlElement("ParentControl", labelParentControl));
            }

            if (!string.IsNullOrEmpty(labelInPage))
            {
              group_PropertiesNode.AppendChild(XmlUtility.CreateXmlElement("InPage", labelInPage));
            }

            group_Node.AppendChild(group_PropertiesNode);
            XmlNode previousSiblingNode = label.PreviousSibling;
            if (previousSiblingNode == null)
            {
              previousSiblingNode = label.NextSibling;
            }

            RemoveThisNodeFromParent(label);
            previousSiblingNode.ParentNode.InsertAfter(group_Node, previousSiblingNode);

            if (!objectsHasIndentation.Contains(metaDataDocMgt.GetCurrentPageId))
            {
              objectsHasIndentation.Add(metaDataDocMgt.GetCurrentPageId);
            }
          }

          RemoveThisNodeFromParent(parentTextBoxNode);
          group_Node.AppendChild(parentTextBoxNode);
        }

        RemoveThisNodeFromParent(label.SelectSingleNode(".//a:CaptionML", metaDataDocMgt.XmlNamespaceMgt));
      }
    }

    private static XmlNode GetParentControl(XmlNode control)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      XmlNode parentControl = control.SelectSingleNode("./a:ParentControl", metaDataDocMgt.XmlNamespaceMgt);
      if (parentControl == null)
      {
        return null;
      }

      string parentControlID = parentControl.InnerText;

      parentControl = metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(".//a:Control[./a:Properties/a:ID='" + parentControlID + "']", metaDataDocMgt.XmlNamespaceMgt);
      return parentControl;
    }

    private static bool GenerateIndentationSearchString(XmlNode control, ref int xPos, ref int yPos, int xMove, ref string search)
    {
      return GenerateIndentationSearchString(control, ref xPos, ref yPos, xMove, ref search, 110);
    }

    private static bool GenerateIndentationSearchString(XmlNode control, ref int xPos, ref int yPos, int xMove, ref string search, int precisionX)
    {
      return GenerateSearchString(control, ref xPos, ref yPos, xMove, ref search, precisionX, true);
    }

    private static bool GenerateSearchString(XmlNode control, ref int xPos, ref int yPos, int xMove, ref string search, int precisionX, bool doNotIncludeThisNode)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      int ymove = 550;
      XmlNode position = control.SelectSingleNode(".//a:XPos", metaDataDocMgt.XmlNamespaceMgt);
      if (position == null)
      {
        return false;
      }

      xPos = Convert.ToInt32(position.InnerText, CultureInfo.InvariantCulture);

      position = control.SelectSingleNode(".//a:YPos", metaDataDocMgt.XmlNamespaceMgt);
      if (position == null)
      {
        return false;
      }

      yPos = Convert.ToInt32(position.InnerText, CultureInfo.InvariantCulture);
      if (doNotIncludeThisNode)
      {
        position = control.SelectSingleNode(".//a:Height", metaDataDocMgt.XmlNamespaceMgt);
        if (position != null)
        {
          yPos = yPos + Convert.ToInt32(position.InnerText, CultureInfo.InvariantCulture);
          ymove = 110;
        }
      }
      else
      {
        ymove = 110; // Equals to move in the search string
      }

      search = String.Format(
        CultureInfo.InvariantCulture,
        " ./a:XPos>='{0}' and ./a:XPos<='{1}' and ./a:YPos>='{2}']",
        (xPos + xMove - 110).ToString(CultureInfo.InvariantCulture),
        (xPos + xMove + precisionX).ToString(CultureInfo.InvariantCulture),
        (yPos + ymove - 110).ToString(CultureInfo.InvariantCulture));
      return true;
    }

    private static bool GenerateLabelSearchString(XmlNode control, ref string search, string path)
    {
      const int XMoveMin = (int)(110 * 0.5), XMoveMax = (int)(110 * 1.5); // 110 it’s correct distanse between control and Label. 0.5 and 1.5 -– accuracy.
      int ymove;
      int xPos, yPos;
      string inPage;

      xPos = GetNodeValue(control, "./a:XPos");
      yPos = GetNodeValue(control, "./a:YPos");
      xPos = xPos + GetNodeValue(control, "./a:Width");
      ymove = GetNodeValue(control, "./a:Height");
      inPage = XmlUtility.GetNodeValue(control, "./a:InPage", string.Empty);
      if (!string.IsNullOrEmpty(inPage))
      {
        inPage = String.Format(CultureInfo.InvariantCulture, " and ./a:InPage = '{0}'", inPage);
      }

      search = String.Format(
        CultureInfo.InvariantCulture,
        "{0} [ ./a:XPos>='{1}' and ./a:XPos<='{2}' and ./a:YPos>='{3}' and ./a:YPos<='{4}' {5}]",
        path,
        (xPos + XMoveMin).ToString(CultureInfo.InvariantCulture),
        (xPos + XMoveMax).ToString(CultureInfo.InvariantCulture),
        yPos,
        yPos + ymove - 330,
        inPage);
      return true;
    }

    /// <summary>
    /// Only for int. Otherwise use XmlUtility.GetNodeValue.
    /// Function will return value of element specified in XPath query. 
    /// Function will return 0 if node can’t be found. 
    /// Attention! Function will return the first found element’s value if more than one element can be found.
    /// </summary>
    /// <param name="control">Node in which search will be perform</param>
    /// <param name="query">XPath query</param>
    /// <returns>Element value or 0 if element’s node not found.</returns>
    private static int GetNodeValue(XmlNode control, string query)
    {
      return Convert.ToInt32(XmlUtility.GetNodeValue(control, query, "0"), CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// It will create new group. Sample with desired InPage value could be provided.
    /// </summary>
    /// <param name="sample">New group will have the same InPage value. Can be null</param>
    /// <param name="element">Array of parameters to copy from sample to new group</param>
    /// <param name="captionML">Create CaptionML. Don't use with element[CaptionML]</param>
    /// <returns>?abc?</returns>
    private static XmlNode CreateGroup(XmlNode sample, string[] element, string captionML, string suggestedID)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;

      XmlNode group = metaDataDocMgt.XmlDocument.CreateNode(XmlNodeType.Element, "Group", metaDataDocMgt.XmlNamespace);
      XmlNode group_PropertiesNode = metaDataDocMgt.XmlDocument.CreateNode(XmlNodeType.Element, "Properties", metaDataDocMgt.XmlNamespace);
      if (!string.IsNullOrEmpty(captionML))
      {
        group_PropertiesNode.AppendChild(XmlUtility.CreateXmlElement("CaptionML", captionML));
      }

      group_PropertiesNode.AppendChild(XmlUtility.CreateXmlElement("ID", suggestedID /*metaDataDocMgt.GetNewId.ToString(CultureInfo.InvariantCulture) */));
      if (sample != null)
      {
        XmlNode elementInPage = sample.SelectSingleNode(@"./a:Properties/a:InPage", metaDataDocMgt.XmlNamespaceMgt);
        if (elementInPage != null)
        {
          group_PropertiesNode.AppendChild(XmlUtility.CreateXmlElement("InPage", elementInPage.InnerText));
        }

        if (element != null)
        {
          for (int i = 0; i < element.Length; i++)
          {
            XmlNode tmpNode = sample.SelectSingleNode(@"./a:Properties/a:" + element[i], metaDataDocMgt.XmlNamespaceMgt);
            if (tmpNode == null)
            {
              tmpNode = sample.ParentNode.SelectSingleNode(@"./a:Properties/a:" + element[i], metaDataDocMgt.XmlNamespaceMgt);
            }

            if (tmpNode != null)
            {
              group_PropertiesNode.AppendChild(XmlUtility.CreateXmlElement(element[i], tmpNode.InnerText));
            }
          }
        }
      }

      group.AppendChild(group_PropertiesNode);
      return group;
    }

    private static bool IsGroup(XmlNode childControl)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      if (childControl == null)
      {
        TransformationLog.GenericLogEntry(Resources.ParentControlDoesNotSupportControlTypePropertySuggestion, LogCategory.ValidateManually, metaDataDocMgt.GetCurrentPageId);
        return true;
      }

      XmlNode controltype = childControl.SelectSingleNode("./a:Properties/a:Controltype", metaDataDocMgt.XmlNamespaceMgt);

      // If place label dirrectly on the form
      if (controltype == null)
      {
        TransformationLog.GenericLogEntry(Resources.ParentControlDoesNotSupportControlTypePropertySuggestion, LogCategory.ValidateManually, metaDataDocMgt.GetCurrentPageId);
        return true;
      }

      switch (controltype.InnerText)
      {
        case "Label":
        case "TextBox":
        case "CheckBox":
        case "OptionButton":
        case "CommandButton":
        case "MenuButton":
        case "MenuItem":
        case "Indicator":
        case "PictureBox":
          return false;
        default:
          return true;
      }
    }

    private static void AddAdditionalProperiesToBands(XmlNode tabControlNode, bool hasPages, int inPage, XmlNode band)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;

      string query;
      if (hasPages)
      {
        query = string.Format(CultureInfo.InvariantCulture, ".//a:tempNode_InstructionalTextML/a:InstructionalTextML [../a:InPage = '{0}']", inPage.ToString(CultureInfo.InvariantCulture));
      }
      else
      {
        query = ".//a:tempNode_InstructionalTextML/a:InstructionalTextML";
      }

      XmlNode tempNode_InstructionalTextML = tabControlNode.SelectSingleNode(query, metaDataDocMgt.XmlNamespaceMgt);
      if (tempNode_InstructionalTextML != null)
      {
        band.FirstChild.AppendChild(tempNode_InstructionalTextML);
      }
    }

    /// <summary>
    /// Function will create or change property in selected node
    /// </summary>
    /// <param name="nodeToCopy">New property will be added or changed in this node</param>
    /// <param name="prpertyName">Prperty name</param>
    /// <param name="properyValue">Prperty value</param>
    private static void CopyProperty(XmlNode nodeToCopy, string prpertyName, string properyValue)
    {
      XmlNode newProperty = nodeToCopy.SelectSingleNode(@"./a:" + prpertyName, MetadataDocumentManagement.Instance.XmlNamespaceMgt);
      if (newProperty != null)
      {
        newProperty.InnerText = properyValue;
      }
      else
      {
        newProperty = XmlUtility.CreateXmlElement(prpertyName, properyValue);
        nodeToCopy.AppendChild(newProperty);
      }
    }

    private static void ModifyBitmapsInGrid()
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      XmlNodeList bitmapList = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(
        @".//a:Group/a:Control/a:Properties/a:BitmapList [../../../a:Properties/a:GroupType='Repeater' ]",
        metaDataDocMgt.XmlNamespaceMgt);

      foreach (XmlNode bitmap in bitmapList)
      {
        if (String.IsNullOrEmpty(GetProperty(bitmap.ParentNode.ParentNode, "OptionCaptionML")))
        {
          string optionValue = bitmap.InnerText;
          optionValue = "ENU=Bitmap" + optionValue.Replace(",", ",Bitmap");
          string sourceExpr = GetProperty(bitmap.ParentNode.ParentNode, "SourceExpr");
          XmlNode code = metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(@".//a:Code", metaDataDocMgt.XmlNamespaceMgt);

          if (code != null)
          {
            string optionList = CodeTransformationRules.FindOptionList(sourceExpr, code.InnerText);

            if (!String.IsNullOrEmpty(optionList))
            {
              optionList = optionList.Trim("'".ToCharArray());

              StringBuilder sbOption = new StringBuilder();
              sbOption.Append("ENU=");

              foreach (char c in optionList.ToCharArray())
              {
                sbOption.Append(c);
              }

              optionValue = sbOption.ToString();
            }
          }

          bitmap.ParentNode.InsertAfter(XmlUtility.CreateXmlElement("OptionCaptionML", optionValue), bitmap);
        }
      }
    }

    /// <summary>
    /// Finalize Trendscape form
    /// </summary>
    private static void FinalizeTrendscape()
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;

      XmlNodeList trendscapeList = metaDataDocMgt.XmlCurrentFormNode.SelectNodes("./a:Controls/a:ContentArea/a:Control[./a:Properties/a:TempProperty='Trendscape']", metaDataDocMgt.XmlNamespaceMgt);
      if (trendscapeList.Count != 0)
      {
        XmlNode newGroup = CreateGroup(null, null, null, metaDataDocMgt.CalcId(null, "ViewAsViewBy", "ContentArea").ToString(CultureInfo.InvariantCulture));
        trendscapeList[0].ParentNode.InsertBefore(newGroup, trendscapeList[0]);

        foreach (XmlNode trendscapeControl in trendscapeList)
        {
          RemoveThisNodeFromParent(trendscapeControl);
          newGroup.AppendChild(trendscapeControl);
        }
      }

      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:TempProperty", metaDataDocMgt.XmlNamespaceMgt);
    }

    public static String[] GetPageNamesCaptionsArray(String pageNamesML)
    {      
      pageNamesML = pageNamesML.Replace("\r", string.Empty);
      pageNamesML = pageNamesML.Replace("\n", string.Empty);
      if (pageNamesML.Length < 5)
      { 
        return new string[0];
      }

      Regex languageIdExpFirst = new Regex(@"(?<fLanguageId>\w{3})=", RegexOptions.IgnoreCase);
      Regex languageIdExp = new Regex(@";(?<languageId>\w{3})=", RegexOptions.IgnoreCase);
      
      if(!languageIdExpFirst.Match(pageNamesML).Success)
      {
        return new string[0];
      }
      
      MatchCollection extraLanguages = languageIdExp.Matches(pageNamesML);
      int languageCouter = extraLanguages.Count + 1; //1 for the first language
      String[] tabLanguageArray = new string[languageCouter];
      String[] languageArray = new string[languageCouter];

      if (languageCouter == 1)
      {
        int startPosition = languageIdExpFirst.Match(pageNamesML).Index;
        tabLanguageArray[0] = pageNamesML;
        tabLanguageArray[0] = CleanInputForFunctionGetPageNamesCaptionsArray(pageNamesML.Substring(startPosition + 4));
        languageArray[0] = pageNamesML.Substring(startPosition, 4);
      }
      else
      {
        int startPosition = languageIdExpFirst.Match(pageNamesML).Index - 1;
        int endPosition = 0;
        int counter = 0;
        Match m = extraLanguages[0];
        while (m.Success)
        {
          endPosition = m.Index;

          tabLanguageArray[counter] = CleanInputForFunctionGetPageNamesCaptionsArray(pageNamesML.Substring(startPosition + 5, endPosition - startPosition - 5));
          languageArray[counter] = pageNamesML.Substring(startPosition + 1, 4);

          m = m.NextMatch();
          startPosition = endPosition;
          counter++;
        }

        tabLanguageArray[counter] = CleanInputForFunctionGetPageNamesCaptionsArray(pageNamesML.Substring(startPosition + 5));
        languageArray[counter] = pageNamesML.Substring(startPosition + 1, 4);
      }

      // we need it only in order to correctly init StringBuilder[] builderArray
      int maxTabsCount = 0;
      for (int i = 0; (tabLanguageArray.Length) > i; i++)
      {
        String[] tmpArray = tabLanguageArray[i].Split(",".ToCharArray());
        if (tmpArray.Length > maxTabsCount)
        {
          maxTabsCount = tmpArray.Length;
        }
      }

      StringBuilder[] builderArray = new StringBuilder[maxTabsCount];

      for (int i = 0; (tabLanguageArray.Length) > i; i++)
      {
        string tabNames = tabLanguageArray[i].Trim();
        String[] captionsArray = tabNames.Split(",".ToCharArray());

        for (int ii = 0; captionsArray.Length > ii; ii++)
        {
          if ((builderArray[ii]) == null)
          {
            builderArray[ii] = new StringBuilder(languageArray[i]);
          }
          else
          {
            builderArray[ii].Append(";");
            builderArray[ii].Append(languageArray[i]);
          }

          builderArray[ii].Append(CheckInputForFunctionGetPageNamesCaptionsArray(captionsArray[ii]));
        }
      }

      String[] returnArray = new string[maxTabsCount];
      for (int i = 0; i < builderArray.Length; i++)
      {
        returnArray[i] = builderArray[i].ToString();
      }

      return returnArray;
    }

    private static string CleanInputForFunctionGetPageNamesCaptionsArray(String tabLanguageElement)
    {
      tabLanguageElement = tabLanguageElement.Trim();
      if (tabLanguageElement.StartsWith("\"", StringComparison.Ordinal))
      {
        tabLanguageElement = tabLanguageElement.Substring(1);
      }

      if (tabLanguageElement.EndsWith("\"", StringComparison.Ordinal))
      {
        tabLanguageElement = tabLanguageElement.Substring(0, tabLanguageElement.Length - 1);
      }

      return tabLanguageElement;
    }

    private static string CheckInputForFunctionGetPageNamesCaptionsArray(String caption)
    {
      Regex test1 = new Regex(@"[;=]", RegexOptions.IgnoreCase);

      if (test1.Match(caption).Success)
      {
        return "\"" + caption + "\"";
      }

      if (caption.StartsWith(" ", StringComparison.Ordinal) || caption.StartsWith("\"", StringComparison.Ordinal) || caption.EndsWith(" ", StringComparison.Ordinal))
      {
        return "\"" + caption + "\"";
      }

      return caption;
    }

    private static void DoMoveUntouchedControlsToContentArea(string query)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;

      XmlNodeList controlList = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(query, metaDataDocMgt.XmlNamespaceMgt);
      if (controlList.Count != 0)
      {
        XmlNode contentArea = GetPageControlNode("ContentArea");

        // TODO This is only iterating on First level, maybe it should go recursive through the three?
        for (Int32 i = controlList.Count - 1; i >= 0; i--)
        {
          XmlNode control = controlList[i].CloneNode(true);
          if (control.SelectNodes(@"./a:Control", metaDataDocMgt.XmlNamespaceMgt).Count != 0)
          {
            contentArea.AppendChild(XmlUtility.GetNodeWithNewName(control, "Group"));
          }
          else
          {
            contentArea.AppendChild(control);
          }

          RemoveThisNodeFromParent(controlList[i]);
        }
      }
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    /// <param name="fromControl">?abc?</param>
    /// <param name="toControlNode">?abc?</param>
    /// <returns>?abc?</returns>
    private static XmlNode CopyPropertiesFromControl(XmlNode fromControl, XmlNode toControlNode)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      XmlNode fromPropNode = fromControl.SelectSingleNode(@"./a:Properties", metaDataDocMgt.XmlNamespaceMgt);
      if (fromPropNode.ChildNodes == null)
        return toControlNode;

      XmlNode toPropNode = toControlNode.SelectSingleNode(@"./a:Properties", metaDataDocMgt.XmlNamespaceMgt);

      for (Int32 i = 0; i < fromPropNode.ChildNodes.Count; i++)
      {
        XmlNode propertyNode = fromPropNode.ChildNodes[i];

        if (toPropNode.SelectSingleNode(@"./a:" + propertyNode.Name + "", metaDataDocMgt.XmlNamespaceMgt) == null)
          toPropNode.AppendChild(XmlUtility.CreateXmlElement(propertyNode.Name, propertyNode.InnerText));
      }

      return toControlNode;
    }

    private static String GetProperty(XmlNode controlNode, String propertyName)
    {
      //TODO we also have FindProperty. I think we should have only one.

      if (controlNode == null)
      {
        return null;
      }

      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;

      XmlNode propNode = controlNode.SelectSingleNode(@"./a:Properties/a:" + propertyName, metaDataDocMgt.XmlNamespaceMgt);
      if (propNode == null)
        return null;
      return propNode.LastChild.InnerText;
    }

    private static void ProcessOptionButtonOptionCaptionML(String optionList, XmlNode optionButton, Dictionary<String, Dictionary<String, OptionButton>> optionSourceExprList, String parentId, String sourceExpr)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      String[] optionsArray = null;
      Dictionary<String, OptionButton> currOptionSourceExpr;

      if (optionList.StartsWith("'", StringComparison.Ordinal))
      {
        optionList = optionList.Remove(0, 1);
      }

      if (optionList.EndsWith("'", StringComparison.Ordinal))
      {
        optionList = optionList.Remove(optionList.Length - 1);
      }

      optionsArray = optionList.Split(",".ToCharArray());

      if (optionSourceExprList.TryGetValue(parentId + "_" + sourceExpr, out currOptionSourceExpr))
      {
        string[] captionsArray = new string[optionsArray.Length];
        SortedList languages = new SortedList();
        for (Int32 j = 0; j <= optionsArray.Length - 1; j++)
        {
          captionsArray[j] = string.Empty;

          foreach (KeyValuePair<String, OptionButton> currOptionSourceExpr1 in currOptionSourceExpr)
          {
            if (currOptionSourceExpr1.Value.OptionValue == optionsArray[j])
            {
              string currOptionCaption = currOptionSourceExpr1.Value.OptionCaption;
              if (string.IsNullOrEmpty(currOptionCaption))
              {
                currOptionCaption = optionsArray[j];
              }

              captionsArray[j] = currOptionCaption.Replace("\r\n", string.Empty);

              // To support ENU=Day,Week,Month,Quarter,Year,Accounting Period
              if ((captionsArray[j].Length < 4) || (captionsArray[j].Substring(3, 1) != "="))
              {
                captionsArray[j] = "ENU=" + captionsArray[j];
              }

              string[] tmpList = captionsArray[j].Split(";".ToCharArray());
              for (int tmpCounter = 0; tmpCounter < tmpList.Length; tmpCounter++)
              {
                if (tmpList[tmpCounter].Length > 4)
                {
                  string newLang = tmpList[tmpCounter].Remove(4, tmpList[tmpCounter].Length - 4);
                  if (!languages.ContainsKey(newLang))
                  {
                    languages.Add(newLang, newLang);
                  }
                }
              }

              break;
            }
          }
        }

        for (Int32 i = 0; i < optionsArray.Length; i++)
        {
          for (int iLangCount = 0; iLangCount < languages.Count; iLangCount++)
          {
            string key = (string)languages.GetKey(iLangCount);
            string langCaption = (string)languages.GetByIndex(iLangCount);

            string currentOptionCaption = captionsArray[i];
            string caption;
            int langStartPosition = currentOptionCaption.IndexOf(key, StringComparison.OrdinalIgnoreCase);
            if (langStartPosition >= 0)
            {
              langStartPosition = langStartPosition + 4;
              int langEndPosition = currentOptionCaption.IndexOf(";", langStartPosition, StringComparison.OrdinalIgnoreCase);
              if (langEndPosition < 1)
              {
                if (i < optionsArray.Length - 1)
                {
                    caption = currentOptionCaption.Substring(langStartPosition).TrimStart('"').TrimEnd('"') + ",";
                }
                else
                {
                    caption = currentOptionCaption.Substring(langStartPosition).TrimStart('"').TrimEnd('"');
                }
              }
              else
              {
                if (i < optionsArray.Length - 1)
                {
                    caption = currentOptionCaption.Substring(langStartPosition, langEndPosition - langStartPosition).TrimStart('"').TrimEnd('"') + ",";
                }
                else
                {
                    caption = currentOptionCaption.Substring(langStartPosition, langEndPosition - langStartPosition).TrimStart('"').TrimEnd('"');
                }
              }
            }
            else
            {
              if (i < optionsArray.Length - 1)
              {
                caption = ",";
              }
              else
              {
                caption = string.Empty;
              }
            }

            languages.SetByIndex(iLangCount, (langCaption + caption));
          }
        }

        StringBuilder optionsCaptionML = new StringBuilder();
        foreach (DictionaryEntry captionReadyToUse in languages)
        {
          optionsCaptionML.Append(captionReadyToUse.Value);
          optionsCaptionML.Append(";");
        }

        if (optionsCaptionML.Length > 0)
        {
          optionsCaptionML.Remove(optionsCaptionML.Length - 1, 1);
        }

        optionsCaptionML = optionsCaptionML.Replace("&&", "ZZZXXXZZZampZZZXXXZZZ");
        optionsCaptionML = optionsCaptionML.Replace("&", "");
        optionsCaptionML = optionsCaptionML.Replace("ZZZXXXZZZampZZZXXXZZZ", "&&");

        if (!String.IsNullOrEmpty(optionsCaptionML.ToString()))  // update the optionCaption
        {
          XmlNode optionCaptionNode = optionButton.SelectSingleNode(@"./a:Properties/a:OptionCaptionML", metaDataDocMgt.XmlNamespaceMgt);
          if (optionCaptionNode == null)
          {
            XmlNode optionCaptionProperties = optionButton.SelectSingleNode(@"./a:Properties", metaDataDocMgt.XmlNamespaceMgt);
            optionCaptionProperties.AppendChild(XmlUtility.CreateXmlElement("OptionCaptionML", optionsCaptionML.ToString()));
          }
          else
          {
            optionCaptionNode.InnerText = optionsCaptionML.ToString();
          }
        }
      }
    }

    private static void ProcessOptionButtonCaptionML(Boolean isParent, XmlNode optionButton, OptionButton optionSourceExpr)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      Boolean hasLabelAsPrevSibling = false;
      XmlNode previousSiblingNode = null;

      RemoveThisNodeFromParent(optionButton.SelectSingleNode("./a:Properties/a:CaptionML", metaDataDocMgt.XmlNamespaceMgt));

      XmlNode labelNode = optionButton.SelectSingleNode(@"./a:Control [boolean (./a:Properties/a:CaptionML) and ./a:Properties/a:Controltype = 'Label']", metaDataDocMgt.XmlNamespaceMgt);
      if (labelNode == null)
      {
        if (isParent && (optionButton.PreviousSibling != null))
        {
          previousSiblingNode = optionButton.PreviousSibling.SelectSingleNode(@"./a:Properties/a:Controltype", metaDataDocMgt.XmlNamespaceMgt);
          if (previousSiblingNode != null)
          {
            if (previousSiblingNode.InnerText == "Label")
            {
              hasLabelAsPrevSibling = true;
            }
          }
        }


        if (hasLabelAsPrevSibling)
        {
          labelNode = previousSiblingNode.ParentNode.ParentNode;
        }
        else
        {
          labelNode = optionButton.SelectSingleNode(@".//a:Control[./a:Properties/a:Controltype='Label']", metaDataDocMgt.XmlNamespaceMgt);
        }
      }

      if (labelNode != null)
      {
        String labelCaptionML = GetProperty(labelNode, "CaptionML");
        if (optionSourceExpr.OptionCaption == null)
        {
          optionSourceExpr.OptionCaption = labelCaptionML;
          RemoveThisNodeFromParent(labelNode);
        }
        else
        {
          if (isParent &&
              (!hasLabelAsPrevSibling ||
              (Convert.ToInt64(GetProperty(labelNode, "YPos"), CultureInfo.InvariantCulture) <=
               Convert.ToInt64(GetProperty(optionButton, "YPos"), CultureInfo.InvariantCulture))))
          {
            XmlNode optionButtonCaptionMLNode = optionButton.SelectSingleNode(@"./a:Properties/a:CaptionML", metaDataDocMgt.XmlNamespaceMgt);
            optionButtonCaptionMLNode = optionButton.SelectSingleNode(@"./a:Properties", metaDataDocMgt.XmlNamespaceMgt);
            optionButtonCaptionMLNode.AppendChild(XmlUtility.CreateXmlElement("CaptionML", labelCaptionML));
            XmlUtility.DeleteElements(labelNode, "./a:Properties/a:ID", metaDataDocMgt.XmlNamespaceMgt);
            hasLabelAsPrevSibling = false;
          }
          else
          {
            RemoveThisNodeFromParent(labelNode.SelectSingleNode(@"./a:Properties/a:ParentControl", metaDataDocMgt.XmlNamespaceMgt));

            optionButton.ParentNode.InsertBefore(labelNode, optionButton);
          }
        }
      }

      if (hasLabelAsPrevSibling && isParent)
      {
        XmlNode labelCaptionMLNode = previousSiblingNode.ParentNode.SelectSingleNode(@"./a:CaptionML", metaDataDocMgt.XmlNamespaceMgt);
        if (labelCaptionMLNode != null)
        {
          XmlNode optionButtonCaptionMLNode = optionButton.SelectSingleNode(@"./a:Properties/a:CaptionML", metaDataDocMgt.XmlNamespaceMgt);
          optionButtonCaptionMLNode = optionButton.SelectSingleNode(@"./a:Properties", metaDataDocMgt.XmlNamespaceMgt);
          optionButtonCaptionMLNode.AppendChild(XmlUtility.CreateXmlElement("CaptionML", labelCaptionMLNode.InnerText));

          RemoveThisNodeFromParent(labelCaptionMLNode);
        }
      }
    }

    /// <summary>
    /// Function will remove nodeToRemove from parent node.
    /// using this function will prevent errors when nodeToRemove is null.
    /// </summary>
    /// <param name="nodeToRemove">This node will be removed from parent node</param>
    /// <returns>Result of nodeToRemove.ParentNode.RemoveChild(nodeToRemove) or null if nodeToRemove is null</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode")]
    private static XmlNode RemoveThisNodeFromParent(XmlNode nodeToRemove)
    {
      if (nodeToRemove != null)
      {
        return nodeToRemove.ParentNode.RemoveChild(nodeToRemove);
      }

      return null;
    }

    private static bool SearchForParentForStandaloneLabel(XmlNode label)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;

      int x = 0, y = 0, width = 0;
      string inpage = string.Empty, searchString = string.Empty;

      x = GetNodeValue(label, "./a:XPos");
      y = GetNodeValue(label, "./a:YPos");
      width = GetNodeValue(label, "./a:Width");
      inpage = XmlUtility.GetNodeValue(label, "./a:InPage", string.Empty);
      if (!string.IsNullOrEmpty(inpage))
      {
        inpage = String.Format(CultureInfo.InvariantCulture, " and ./a:InPage = '{0}'", inpage);
      }

      searchString = String.Format(
        CultureInfo.InvariantCulture,
         "./a:Controls//a:Control/a:Properties [./a:Controltype = 'CheckBox' and ( ./a:XPos>='{0}' and ./a:XPos<='{1}') and (./a:YPos>={2} and ./a:YPos<={3}){4}]",
         (x - 3600).ToString(CultureInfo.InvariantCulture),
         x.ToString(CultureInfo.InvariantCulture),
         (y - 25).ToString(CultureInfo.InvariantCulture),
         (y + 25).ToString(CultureInfo.InvariantCulture),
         inpage);

      XmlNodeList candidateList = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(searchString, metaDataDocMgt.XmlNamespaceMgt);
      if (candidateList.Count == 0)
      {
        x = x + width;
        searchString = String.Format(
          CultureInfo.InvariantCulture,
           "./a:Controls//a:Control/a:Properties [(./a:Controltype = 'TextBox' or ./a:Controltype = 'CheckBox') and ( ./a:XPos>='{0}' and ./a:XPos<='{1}') and (./a:YPos>={2} and ./a:YPos<={3}){4}]",
           x.ToString(CultureInfo.InvariantCulture),
           (x + 220).ToString(CultureInfo.InvariantCulture),
           (y - 25).ToString(CultureInfo.InvariantCulture),
           (y + 25).ToString(CultureInfo.InvariantCulture),
           inpage);

        candidateList = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(searchString, metaDataDocMgt.XmlNamespaceMgt);

        if (candidateList.Count == 0)
        {
          return false;
        }
      }

      string logStr = string.Format(
        CultureInfo.InvariantCulture,
        Resources.CheckParentlessLabel,
        candidateList.Count,
        XmlUtility.GetNodeValue(label, "./a:ID", string.Empty));
      TransformationLog.GenericLogEntry(logStr, LogCategory.ValidateManually, metaDataDocMgt.GetCurrentPageId);

      XmlUtility.UpdateNodeInnerText(candidateList[0], "CaptionML", XmlUtility.GetNodeValue(label, "./a:CaptionML", string.Empty));
      XmlUtility.DeleteElements(label, "./a:ID", metaDataDocMgt.XmlNamespaceMgt);
      return true;
    }

    /// <summary>
    /// Method extracted to reduce complexity of ManageLabels
    /// </summary>
    private static void ManageLabelsForMainControlTypes(XmlNode label, string labelID, XmlNode nodeParentProperties)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      string logStr = string.Empty;

      if (nodeParentProperties.InnerText == "CheckBox")
      {
        if (XmlUtility.GetNodeValue(label.ParentNode, "./a:Properties/a:ShowCaption", "Yes").Equals("yes", StringComparison.OrdinalIgnoreCase))
        {
          if (!string.IsNullOrEmpty(GetProperty(label.ParentNode, "CaptionML")))
          {
            string groupCaption = GetProperty(label, "CaptionML");
            if (!string.IsNullOrEmpty(groupCaption))
            {
              XmlNode group = CreateGroup(null, null, groupCaption, metaDataDocMgt.CalcId(null, XmlUtility.GetCaption(groupCaption), "ContentArea").ToString(CultureInfo.InvariantCulture));
              label.ParentNode.ParentNode.AppendChild(group);
              int xpos = 0, ypos = 0;
              string search = string.Empty;
              if (GenerateSearchString(label.ParentNode, ref xpos, ref ypos, 0, ref search, 110, false))
              {
                string search2 = string.Empty;
                int width = Convert.ToInt32(GetProperty(label.ParentNode, "Width"), CultureInfo.InvariantCulture);
                if (GenerateSearchString(label.ParentNode, ref xpos, ref ypos, width + 220, ref search2, 110, false))
                {
                  search = "(" + search.Replace("]", ") or (") + search2.Replace("]", ")]");
                }

                string searchQuery = @"./a:Controls//a:Control/a:Properties[./a:Controltype='CheckBox' and " + search;
                XmlNodeList checkBoxesPropList = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(
                  searchQuery,
                  metaDataDocMgt.XmlNamespaceMgt);

                // foreach (XmlNode checkBoxe in checkBoxesPropList) some problems in this case -- up to 246976	childs could be found (if not in debug), instead of 3...
                for (int i = 0; i < checkBoxesPropList.Count; i++)
                {
                  XmlNode checkBoxe = checkBoxesPropList[i].ParentNode;
                  if (((i == 0) || (i > 0) && (string.IsNullOrEmpty(XmlUtility.GetNodeValue(checkBoxe, ".//a:Properties [./a:Controltype='Label'] ", string.Empty)))))
                  {
                    group.AppendChild(RemoveThisNodeFromParent(checkBoxe));
                  }
                }

                XmlUtility.DeleteElements(label, "./a:Properties/a:ID", metaDataDocMgt.XmlNamespaceMgt);

                //TODO continue;
                return;
              }
            }
          }
        }
      }

      if (label.ParentNode.SelectNodes("./a:Control [./a:Properties/a:Controltype='Label']", metaDataDocMgt.XmlNamespaceMgt).Count > 1)
      {
        logStr = string.Format(
          CultureInfo.InvariantCulture,
          Resources.MoreThanOneChild,
          nodeParentProperties.InnerText,
          GetProperty(label.ParentNode, "ID"));
        TransformationLog.GenericLogEntry(logStr, LogCategory.ChangeCodeManually, metaDataDocMgt.GetCurrentPageId);
      }

      LabelPropertyInfo propInfo = new LabelPropertyInfo(metaDataDocMgt.GetCurrentPageId, labelID);

      XmlNode nodeLabelParentControl = label.SelectSingleNode(
              "./a:Properties/a:ParentControl",
              metaDataDocMgt.XmlNamespaceMgt);
      if (nodeLabelParentControl != null)
      {
        XmlUtility.DeleteElements(nodeLabelParentControl, "./a:Properties/a:ID", metaDataDocMgt.XmlNamespaceMgt);
        XmlNode labelParentElement = label.ParentNode.FirstChild;
        if (labelParentElement != null)
        {
          propInfo.PropName = "Name";
          LogIgnoreLabelProperty(propInfo, label, metaDataDocMgt);
          propInfo.PropName = "Visible";
          LogIgnoreLabelProperty(propInfo, label, metaDataDocMgt);

          propInfo.PropName = "MultiLine";
          propInfo.PropValue = FindProperty(propInfo.PropName, label, metaDataDocMgt);
          if (!string.IsNullOrEmpty(propInfo.PropValue))
          {
            MovePropertyToParentControl(labelParentElement, propInfo, metaDataDocMgt);
            RemoveThisNodeFromParent(label.SelectSingleNode("./a:Properties/a:MultiLine", metaDataDocMgt.XmlNamespaceMgt));
          }

          propInfo.PropName = "CaptionML";
          propInfo.PropValue = FindProperty(propInfo.PropName, label, metaDataDocMgt);

          if (!string.IsNullOrEmpty(propInfo.PropValue))
          {
            propInfo.PropValue = propInfo.PropValue.Replace("&&", "ZZZXXXZZZampZZZXXXZZZ");
            propInfo.PropValue = propInfo.PropValue.Replace("&", "");
            propInfo.PropValue = propInfo.PropValue.Replace("ZZZXXXZZZampZZZXXXZZZ", "&&");

            MovePropertyToParentControl(labelParentElement, propInfo, metaDataDocMgt);
            RemoveThisNodeFromParent(label.SelectSingleNode("./a:Properties/a:CaptionML", metaDataDocMgt.XmlNamespaceMgt));
          }
        }
      }
      else
      {
        logStr = string.Format(
          CultureInfo.InvariantCulture,
          Resources.ParentControlNotSet,
          propInfo.LabelID);
        TransformationLog.GenericLogEntry(logStr, LogCategory.IgnoreWarning, propInfo.FormID, null);
      }

      // TODO continue;
      return;
    }

    #endregion
  }
}
