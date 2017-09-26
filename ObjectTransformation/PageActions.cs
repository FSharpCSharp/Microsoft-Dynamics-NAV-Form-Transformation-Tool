//--------------------------------------------------------------------------
// <copyright file="PageActions.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>?abc?</summary>
//--------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Xml;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Microsoft.Dynamics.Nav.Tools.FormTransformation
{
  #region Enumeration types

  /// <summary>
  /// ?abc?
  /// </summary>
  public enum InsertOrder : int
  {
    /// <summary>
    /// ?abc?
    /// </summary>
    Append = 0,

    /// <summary>
    /// ?abc?
    /// </summary>
    Prepend = 1
  }
  #endregion

  /// <summary>
  /// ?abc?
  /// </summary>
  internal static class PageActions
  {
    private const string DefaultPageActionType = "RelatedInformation";
    private const string PatternForCallToSubform = @"CurrPage\.\b(?<SubFormControl>\w+)\b\.FORM\.";


    #region Public static methods

    /// <summary>
    /// ?abc?
    /// </summary>
    public static void RemoveNotSupportedActions()
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      string removeDescription = string.Empty;

      const string XpathQueryBegin = @".//a:Controls//a:Control[./a:Properties/a:Controltype='CommandButton' ";

      removeDescription = "The Phone Buttons";
      XmlNodeList phoneNodeList = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(
        XpathQueryBegin + " and ./a:Properties/a:ShowCaption='No' and ./a:Properties/a:Bitmap='34']", metaDataDocMgt.XmlNamespaceMgt);
      CommentControlRemoval(phoneNodeList, removeDescription);
      XmlUtility.DeleteNodeList(phoneNodeList);

      removeDescription = "The E-mail Buttons";
      XmlNodeList emailNodeList = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(
        XpathQueryBegin + "and ./a:Properties/a:ShowCaption='No' and ./a:Properties/a:Bitmap='21']", metaDataDocMgt.XmlNamespaceMgt);
      CommentControlRemoval(emailNodeList, removeDescription);
      XmlUtility.DeleteNodeList(emailNodeList);

      removeDescription = "The Homepage Buttons";
      XmlNodeList homePageNodeList = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(
        XpathQueryBegin + "and ./a:Properties/a:ShowCaption='No' and ./a:Properties/a:Bitmap='20']", metaDataDocMgt.XmlNamespaceMgt);
      CommentControlRemoval(homePageNodeList, removeDescription);
      XmlUtility.DeleteNodeList(homePageNodeList);

      removeDescription = "The Mappoint Buttons";
      XmlNodeList mapPointNodeList = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(
        XpathQueryBegin + "and ./a:Properties/a:ShowCaption='No' and ./a:Properties/a:Bitmap='53']", metaDataDocMgt.XmlNamespaceMgt);
      CommentControlRemoval(mapPointNodeList, removeDescription);
      XmlUtility.DeleteNodeList(mapPointNodeList);

      removeDescription = "The FormHelp Buttons";
      XmlNodeList formHelpList = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(
        XpathQueryBegin + "and ./a:Properties/a:PushAction='FormHelp']", metaDataDocMgt.XmlNamespaceMgt);
      CommentControlRemoval(formHelpList, removeDescription);
      XmlUtility.DeleteNodeList(formHelpList);

      removeDescription = "Buttons: OK/Cancel, Yes/No, LookupOK, LookupCancel, and Close";
      XmlNodeList miscPushActionsList = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(
        @".//a:Controls//a:Control[
        ./a:Properties/a:PushAction='OK' or 
        ./a:Properties/a:PushAction='Cancel' or 
        ./a:Properties/a:PushAction='Yes' or 
        ./a:Properties/a:PushAction='No' or 
        ./a:Properties/a:PushAction='LookupOK' or 
        ./a:Properties/a:PushAction='LookupCancel' or 
        ./a:Properties/a:PushAction='LookupTable' or 
        ./a:Properties/a:PushAction='RunSystem' or 
        ./a:Properties/a:PushAction='Close' 
        ]",
        metaDataDocMgt.XmlNamespaceMgt);
      CommentControlRemoval(miscPushActionsList, removeDescription);
      XmlUtility.DeleteNodeList(miscPushActionsList);

      // "The Comment Buttons";
      RemoveCommentButtons();

      // Transform Navigation Button
      PageControls.TransformNavigationButtons();

      if (metaDataDocMgt.InsertElementsDoc != null)
      {
        removeDescription = "The Card Buttons (Shift+F5) that match default Page Actions (CardFormID)";

        XmlNode transformPageProperties = metaDataDocMgt.InsertElementsDoc.SelectSingleNode(
          @".//a:TransformPages//a:Page/a:Properties[../@ID='" +
          metaDataDocMgt.GetCurrentPageId.ToString(CultureInfo.InvariantCulture) + "']",
          metaDataDocMgt.XmlNamespaceMgt);

        if (transformPageProperties != null)
        {
          XmlNode defaultPageAction = transformPageProperties.SelectSingleNode(@"./a:CardFormID", metaDataDocMgt.XmlNamespaceMgt);

          if (defaultPageAction != null)
          {
            XmlNodeList cardActionList = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(
              @".//a:Controls//a:Control[" +
              "./a:Properties/a:ShortCutKey='Shift+F5' and " +
              "./a:Properties/a:RunObject='Page " + defaultPageAction.InnerText + "' ]",
              metaDataDocMgt.XmlNamespaceMgt);
            CommentControlRemoval(cardActionList, removeDescription);
            XmlUtility.DeleteNodeList(cardActionList);
          }
        }
      }
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    public static void MoveCommandButtonsToAction()
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      XmlNodeList commandButtonList = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@"./a:Controls//a:Control[./a:Properties/a:Controltype='CommandButton']", metaDataDocMgt.XmlNamespaceMgt);

      for (Int32 i = commandButtonList.Count - 1; i >= 0; i--)
      {
        XmlNode current = commandButtonList[i];

        if (TryToMoveCommandButtonToSubPage(current))
        {
          continue;
        }

        /* Move CommandButtons to Actions on related Band */
        ////XmlNode parentControlElement = current.SelectSingleNode(
        ////"./a:Properties/a:ParentControl",
        ////metaDataDocMgt.XmlNamespaceMgt);
        ////if (parentControlElement != null)
        ////{
        ////  string xqueryForTabControl =
        ////  "./a:Properties[./a:Controltype='TabControl' and ./a:ID=" + parentControlElement.InnerText + "]";

        ////  XmlNode parenPropControltype = current.ParentNode.SelectSingleNode(xqueryForTabControl, metaDataDocMgt.XmlNamespaceMgt);
        ////  if (parenPropControltype != null)
        ////  {
        ////    string id = string.Empty;
        ////    XmlNode idNode = current.SelectSingleNode(".//a:ID", metaDataDocMgt.XmlNamespaceMgt);
        ////    if (idNode != null)
        ////    {
        ////      id = idNode.InnerText;
        ////    }

        ////    string logStr = string.Format(
        ////      CultureInfo.InvariantCulture,
        ////      Resources.CommandButtonWillBeMovedToActionGroup,
        ////      id);
        ////    TransformationLog.GenericLogEntry(logStr, LogCategory.GeneralInformation, metaDataDocMgt.GetCurrentPageID);
        ////    MoveCommandButtonsToActionBand(current, parenPropControltype);
        ////    commandButtonList[i].ParentNode.RemoveChild(commandButtonList[i]);
        ////    continue;
        ////  }
        ////}

        MoveCommandButtonsToActionPage(current);
        commandButtonList[i].ParentNode.RemoveChild(commandButtonList[i]);

      }
    }

    /// <summary>
    /// ?abc?
    /// </summary>
   [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Requires refactoring, but time doesn’t permit refactoring at this point")]
    public static void MoveMenuButtonsToActionPage()
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      bool isStack = IsPageWithStacks();  //??? combine with ListPart?

      StringBuilder transformPageQuery = new StringBuilder();
      transformPageQuery.Append("./a:TransformPages/a:Page[./@ID='");
      transformPageQuery.Append(metaDataDocMgt.GetCurrentPageId.ToString(CultureInfo.InvariantCulture));
      transformPageQuery.Append("' and (./a:Properties/a:PageType = 'ListPart' or ./a:Transformation/@FormType = 'InfoPart')]");

      string defaultPageActionType = DefaultPageActionType;

      if (metaDataDocMgt.InsertElementsDoc != null)
      {
        XmlNode transformPage = metaDataDocMgt.InsertElementsDoc.SelectSingleNode(transformPageQuery.ToString(), metaDataDocMgt.XmlNamespaceMgt);
        if (transformPage != null)
        {
          defaultPageActionType = "ActionItems";
        }
      }

      Boolean isHomePage = false;
      StringBuilder homePageQuery = new StringBuilder();
      homePageQuery.Append("./a:TransformPages/a:Page/a:Properties[../@ID='");
      homePageQuery.Append(metaDataDocMgt.GetCurrentPageId.ToString(CultureInfo.InvariantCulture));
      homePageQuery.Append("' and ./a:PageType = 'RoleCenter']");

      XmlNode actionReportNode = null;
      XmlNode actionHomeNode = null;
      XmlNode actionActivityNode = null;
      XmlNode actionDetailsNode = null;

      if (metaDataDocMgt.InsertElementsDoc != null)
      {
        XmlNode homePageNode = metaDataDocMgt.InsertElementsDoc.SelectSingleNode(homePageQuery.ToString(), metaDataDocMgt.XmlNamespaceMgt);
        if (homePageNode != null)
        {
          isHomePage = true;
          actionReportNode = GetPageActionNode("Reports");
          actionHomeNode = GetPageActionNode("HomeItems");
          actionActivityNode = GetPageActionNode("ActivityButtons");
          actionDetailsNode = GetPageActionNode("ActionItems");
        }
      }

      if (!isHomePage)
      {
        actionDetailsNode = GetPageActionNode(defaultPageActionType);
      }

      XmlNode subformControl = metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(
        "./a:Controls//a:Control[./a:Properties/a:Controltype='SubForm']",
        metaDataDocMgt.XmlNamespaceMgt);

      XmlNodeList menuButtonList = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Control[./a:Properties/a:Controltype='MenuButton']", metaDataDocMgt.XmlNamespaceMgt);
      foreach (XmlNode menuButton in menuButtonList)
      {
        if (subformControl != null)
        {
          MoveMenuButtonsToSubForm(menuButton);
        }

        XmlNode topLevelContainer = null;
        XmlNode topLevelContainerProperties = null;

        XmlNode idNode = menuButton.SelectSingleNode("./a:Properties/a:ID", metaDataDocMgt.XmlNamespaceMgt);
        XmlNode nameMLNode = menuButton.SelectSingleNode("./a:Properties/a:Name", metaDataDocMgt.XmlNamespaceMgt);
        XmlNode captionMLNode = menuButton.SelectSingleNode("./a:Properties/a:CaptionML", metaDataDocMgt.XmlNamespaceMgt);
        XmlNode visibleNode = menuButton.SelectSingleNode("./a:Properties/a:Visible", metaDataDocMgt.XmlNamespaceMgt);

        // XmlNode enabledMLNode = menuButton.SelectSingleNode("./a:Properties/a:Enabled", metaDataDocMgt.XmlNamespaceMgt);
        // XmlNode testAutomationIDMLNode = menuButton.SelectSingleNode("./a:Properties/a:TestAutomationID", metaDataDocMgt.XmlNamespaceMgt);

        Boolean includeContainer = true;
        if (isHomePage)
        {
          includeContainer = false;
          if (captionMLNode.LastChild.Value.Contains("ENU=Home"))
            topLevelContainer = actionHomeNode;
          else
            if (captionMLNode.LastChild.Value.Contains("ENU=Reports"))
              topLevelContainer = actionReportNode;
            else
              if (captionMLNode.LastChild.Value.Contains("ENU=Actions"))
                topLevelContainer = actionDetailsNode;
              else
              {
                topLevelContainer = XmlUtility.InsertNodeWithPropertyChild(actionActivityNode, "ActionGroup");
                actionActivityNode.AppendChild(topLevelContainer);
                includeContainer = true;
              }
        }
        else
        {
          topLevelContainer = XmlUtility.InsertNodeWithPropertyChild(actionDetailsNode, "ActionGroup");
          actionDetailsNode.AppendChild(topLevelContainer);
        }

        if (includeContainer)
        {
          topLevelContainerProperties = topLevelContainer.SelectSingleNode("./a:Properties", metaDataDocMgt.XmlNamespaceMgt);

          XmlUtility.MoveNode(idNode, topLevelContainerProperties);
          XmlUtility.MoveNode(nameMLNode, topLevelContainerProperties);
          XmlUtility.MoveNode(captionMLNode, topLevelContainerProperties);
          XmlUtility.MoveNode(visibleNode, topLevelContainerProperties);

          //// We need X and Y position for MergeDuplicatedMenuButtons.
          {
            XmlNode xposNode = menuButton.SelectSingleNode("./a:Properties/a:XPos", metaDataDocMgt.XmlNamespaceMgt);
            if (xposNode != null)
            {
              XmlUtility.MoveNode(xposNode, topLevelContainerProperties);
            }

            XmlNode yposNode = menuButton.SelectSingleNode("./a:Properties/a:YPos", metaDataDocMgt.XmlNamespaceMgt);
            if (yposNode != null)
            {
              XmlUtility.MoveNode(yposNode, topLevelContainerProperties);
            }
          }

          /* Client dosn't support this properties
          XMLUtility.MoveNode(enabledMLNode, topLevelContainerProperties);
          XMLUtility.MoveNode(testAutomationIDMLNode, topLevelContainerProperties);
          */
        }

        XmlNode menuNode = menuButton.SelectSingleNode("./a:Menu", metaDataDocMgt.XmlNamespaceMgt);

        List<XmlNode> containerList = new List<XmlNode>();
        for (Int32 i = 0; i < 100; i++)
        {
          if (includeContainer)
          {
            containerList.Add(metaDataDocMgt.XmlDocument.CreateNode(XmlNodeType.Element, "Init", metaDataDocMgt.XmlNamespace));
          }
        }

        for (Int32 i = 1; i <= menuNode.ChildNodes.Count; i++)
        {
          Int32 thisMenuLevel = 0;
          Int32 nextMenuLevel = 0;

          XmlNode thisMenuLevelNode = menuNode.SelectSingleNode("./a:Control[" + i + "]/a:Properties/a:MenuLevel", metaDataDocMgt.XmlNamespaceMgt);
          XmlNode thisMenuControllNode = menuNode.SelectSingleNode("./a:Control[" + i + "]", metaDataDocMgt.XmlNamespaceMgt);

          UpdateAndRemoveShortcuts(thisMenuControllNode);

          if (thisMenuLevelNode != null)
          {
            thisMenuLevel = Convert.ToInt32(thisMenuLevelNode.InnerText, CultureInfo.InvariantCulture);
            if (thisMenuLevel > 1)
            {
              XmlNode thisMenuCaptionMLNode = thisMenuControllNode.SelectSingleNode("./a:Properties/a:CaptionML", metaDataDocMgt.XmlNamespaceMgt);
              string thisMenuCaptionML = "Unknown";
              if (thisMenuCaptionMLNode != null)
              {
                thisMenuCaptionML = thisMenuCaptionMLNode.InnerText;
              }

              string logStr = string.Format(CultureInfo.InvariantCulture, Resources.DeepMenuStructure, thisMenuLevel.ToString(CultureInfo.InvariantCulture), thisMenuCaptionML);
              TransformationLog.GenericLogEntry(logStr, LogCategory.ChangeCodeManually, metaDataDocMgt.GetCurrentPageId);
            }
          }

          XmlNode nextMenuLevelNode = menuNode.SelectSingleNode("./a:Control[" + (i + 1) + "]/a:Properties/a:MenuLevel", metaDataDocMgt.XmlNamespaceMgt);

          if (nextMenuLevelNode != null)
          {
            nextMenuLevel = Convert.ToInt32(nextMenuLevelNode.InnerText, CultureInfo.InvariantCulture);
          }

          if (thisMenuLevel < nextMenuLevel)  /* if true then container */
          {
            XmlNode actionGroup = metaDataDocMgt.XmlDocument.CreateNode(XmlNodeType.Element, "ActionGroup", metaDataDocMgt.XmlNamespace);

            if (thisMenuLevel == 0)  /* insert on toplevel*/
            {
              topLevelContainer.AppendChild(actionGroup);
            }
            else
            {
              XmlNode parentNode = containerList[thisMenuLevel - 1];
              parentNode.AppendChild(actionGroup);
            }

            actionGroup.InnerXml = thisMenuControllNode.InnerXml;
            RemoveUnsupportedPropertiesFromActionGroup(actionGroup);
            for (Int32 j = thisMenuControllNode.Attributes.Count - 1; j >= 0; j--)
            {
              actionGroup.Attributes.Prepend(thisMenuControllNode.Attributes[j]);
            }

            containerList.Insert(thisMenuLevel, actionGroup);
          }
          else  /* action */
          {
            XmlNode nodeToInsert = thisMenuControllNode.CloneNode(true);

            string nodeName = "Action";
            bool itsSeparator = false;
            bool itsOfficeSeparator = false;

            XmlNode separatorActionControlType = thisMenuControllNode.SelectSingleNode("./a:Properties[./a:MenuItemType='Separator']", metaDataDocMgt.XmlNamespaceMgt);
            if (separatorActionControlType != null)
            {
              nodeName = "Separator";
              itsSeparator = true;
            }
            else
            {
              if (isHomePage)
              {
                if (isOfficeSeparator(thisMenuControllNode, metaDataDocMgt))
                {
                  nodeName = "Separator";
                  itsOfficeSeparator = true;
                }
              }
            }

            XmlNode actionNode = metaDataDocMgt.XmlDocument.CreateNode(XmlNodeType.Element, nodeName, metaDataDocMgt.XmlNamespace);
            for (Int32 k = nodeToInsert.FirstChild.ChildNodes.Count - 1; k >= 0; k--)
            {
              if (itsSeparator || itsOfficeSeparator)
              {
                string tmpValue = nodeToInsert.FirstChild.ChildNodes[k].Name;
                if (tmpValue == "ID" || tmpValue == "CaptionML")
                {
                  actionNode.AppendChild(nodeToInsert.FirstChild.ChildNodes[k]);
                }
              }
              else
              {
                actionNode.AppendChild(nodeToInsert.FirstChild.ChildNodes[k]);
              }
            }

            XmlNode pushAction = nodeToInsert.SelectSingleNode("./a:Triggers/a:OnPush", metaDataDocMgt.XmlNamespaceMgt);
            if (pushAction != null)
            {
              XmlNode onActioNode = metaDataDocMgt.XmlDocument.CreateNode(XmlNodeType.Element, "OnAction", metaDataDocMgt.XmlNamespace);
              XmlCDataSection data = metaDataDocMgt.XmlDocument.CreateCDataSection(pushAction.InnerText);
              onActioNode.AppendChild(data);
              actionNode.AppendChild(onActioNode);
            }

            if (thisMenuLevel == 0) /* if true then insert on toplevel */
            {
              if (isStack)
              {
                menuButton.ParentNode.AppendChild(actionNode);

                if (actionNode.SelectSingleNode("./a:CaptionML", metaDataDocMgt.XmlNamespaceMgt).InnerText.StartsWith("ENU=New ", StringComparison.Ordinal))
                {
                  actionNode.AppendChild(XmlUtility.CreateXmlElement("RunFormMode", "Create"));
                }
              }
              else
              {
                topLevelContainer.AppendChild(actionNode);
              }
            }
            else
            {
              XmlNode parentNode = containerList[thisMenuLevel - 1];
              parentNode.AppendChild(actionNode);
            }

            if (!isStack && !isHomePage)
            {
              string runObjectValue = ReplaceCardWithListInAction(actionNode);

              if (!String.IsNullOrEmpty(runObjectValue))
              {
                XmlNode runObject = actionNode.SelectSingleNode(@".//a:RunObject", metaDataDocMgt.XmlNamespaceMgt);

                runObject.InnerText = runObjectValue;
              }
            }

            if (itsOfficeSeparator)
            {
              actionNode.AppendChild(XmlUtility.CreateXmlElement("IsHeader", "Yes"));
            }
          }
        }
      }

      /* delete old Menubuttons. */
      for (Int32 i = menuButtonList.Count - 1; i >= 0; i--)
      {
        menuButtonList[i].ParentNode.RemoveChild(menuButtonList[i]);
      }
    }

    private static void RemoveUnsupportedPropertiesFromActionGroup(XmlNode actionGroup)
    {
      RemoveNode(actionGroup, "./a:Triggers");
      RemoveNode(actionGroup, "./a:Properties/a:PushAction");
      RemoveNode(actionGroup, "./a:Properties/a:RunObject");
      RemoveNode(actionGroup, "./a:Properties/a:RunFormOnRec");
      RemoveNode(actionGroup, "./a:Properties/a:RunFormView");
      RemoveNode(actionGroup, "./a:Properties/a:RunFormLink");
    }

    private static void RemoveNode(XmlNode node, string xpath)
    {
      if ((node == null) || (string.IsNullOrEmpty(xpath)))
      {
        return;
      }

      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      XmlNode nodeToRemove = node.SelectSingleNode(xpath, metaDataDocMgt.XmlNamespaceMgt);
      if (nodeToRemove != null)
      {
        nodeToRemove.ParentNode.RemoveChild(nodeToRemove);
        string logStr = string.Format(CultureInfo.InvariantCulture, Resources.NodeWillBeDeleted, xpath);
        TransformationLog.GenericLogEntry(logStr, LogCategory.ChangeCodeManually, metaDataDocMgt.GetCurrentPageId);
      }
    }

    private static void UpdateAndRemoveShortcuts(XmlNode menuControllNode)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      if (menuControllNode == null)
      {
        return;
      }

      XmlNode shortCutKeyNode = menuControllNode.SelectSingleNode("./a:Properties/a:ShortCutKey", metaDataDocMgt.XmlNamespaceMgt);
      if (shortCutKeyNode != null)
      {
        if (metaDataDocMgt.MoveElementsDoc != null)
        {
          string query = "./a:MovePageElements/a:Page[@ID='" + ((int)MovePageElements.NotFormsId.ShortcutKeys).ToString(CultureInfo.InvariantCulture) + "' and @Name='" + shortCutKeyNode.InnerText + "']";
          XmlNode newNameNode = metaDataDocMgt.MoveElementsDoc.SelectSingleNode(query, metaDataDocMgt.XmlNamespaceMgt);
          if (newNameNode != null)
          {
            XmlNode shotcutNewNameNode = newNameNode.Attributes.GetNamedItem("NewName");
            if (shotcutNewNameNode != null)
            {
              shortCutKeyNode.InnerText = shotcutNewNameNode.Value;
            }
            else
            {
              XmlUtility.DeleteElements(menuControllNode, "./a:Properties/a:ShortCutKey", metaDataDocMgt.XmlNamespaceMgt);
            }
          }
          else
          {
            string logStr = string.Format(CultureInfo.InvariantCulture, Resources.UnknownShortcut, shortCutKeyNode.InnerText);
            TransformationLog.GenericLogEntry(logStr, LogCategory.ValidateManually, metaDataDocMgt.GetCurrentPageId);
          }
        }
      }
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    /// <param name="pageActionName">?abc?</param>
    /// <returns>?abc?</returns>
    public static XmlNode GetPageActionNode(String pageActionName)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      if (metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode("./a:Actions", metaDataDocMgt.XmlNamespaceMgt) == null)
      {
        XmlNode pageActionsNode = metaDataDocMgt.XmlDocument.CreateNode(XmlNodeType.Element, "Actions", metaDataDocMgt.XmlNamespace);
        XmlNode pageTriggersNode = metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode("./a:Triggers", metaDataDocMgt.XmlNamespaceMgt);
        if (pageTriggersNode != null)
        {
          metaDataDocMgt.XmlCurrentFormNode.InsertAfter(pageActionsNode, pageTriggersNode);
        }
        else
        {
          metaDataDocMgt.XmlCurrentFormNode.AppendChild(pageActionsNode);
        }
        XmlUtility.InsertNodeWithPropertyChild(pageActionsNode, pageActionName, metaDataDocMgt.CalcId(null, null, pageActionName)/*metaDataDocMgt.GetNewId*/);
      }
      else
      {
        if (metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode("./a:Actions/a:" + pageActionName, metaDataDocMgt.XmlNamespaceMgt) == null)
        {
          XmlNode pageActionsNode = metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode("./a:Actions", metaDataDocMgt.XmlNamespaceMgt);
          XmlUtility.InsertNodeWithPropertyChild(pageActionsNode, pageActionName, metaDataDocMgt.CalcId(null, null, pageActionName) /*metaDataDocMgt.GetNewId*/);
        }
      }

      return (metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode("./a:Actions/a:" + pageActionName, metaDataDocMgt.XmlNamespaceMgt));
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    /// <param name="parentBand">?abc?</param>
    /// <returns>?abc?</returns>
    public static XmlNode GetBandActionGroupNode(XmlNode parentBand)
    {
      if (parentBand == null)
      {
        throw new ArgumentNullException("parentBand");
      }

      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      if (parentBand.SelectSingleNode("./a:ActionGroup", metaDataDocMgt.XmlNamespaceMgt) == null)
      {
        XmlUtility.InsertNodeWithPropertyChild(parentBand, "ActionGroup", metaDataDocMgt.CalcId(null, "ActionGroup", "ActionItems"));
      }

      return (parentBand.SelectSingleNode("./a:ActionGroup", metaDataDocMgt.XmlNamespaceMgt));
    }

    /// <summary>
    /// Add log entry for removed node.
    /// </summary>
    /// <param name="nodeToComment">?abc?</param>
    /// <param name="removeDescription">?abc?</param>
    public static void CommentRemoval(XmlNode nodeToComment, string removeDescription)
    {
      if (nodeToComment == null)
      {
        throw new ArgumentNullException("nodeToComment");
      }

      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;

      string prefix;
      if (nodeToComment.Name == "Control")
      {
        prefix = "./a:Properties/a:";
      }
      else
      {
        prefix = "./a:";
      }

      string controlID = string.Empty;
      XmlNode tmpNode = nodeToComment.SelectSingleNode(prefix + "ID", metaDataDocMgt.XmlNamespaceMgt);
      if (tmpNode != null)
      {
        controlID = tmpNode.InnerText;
      }

      string logStr = string.Format(
        CultureInfo.InvariantCulture,
        Resources.ControlWillBeRemoved,
        removeDescription,
        controlID);
      TransformationLog.GenericLogEntry(logStr, LogCategory.RemoveControls, metaDataDocMgt.GetCurrentPageId);
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    public static void ActionsFinalProcessing()
    {
      ResetPageActionType();
      SetPromotedProperty();

      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      XmlNodeList actionList = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Action", metaDataDocMgt.XmlNamespaceMgt);
      foreach (XmlNode action in actionList)
      {
        if (IsEmptyAction(action))
        {
          StringBuilder captionML = new StringBuilder("Empty Action");
          XmlNode tmpNode = action.SelectSingleNode("./a:CaptionML", metaDataDocMgt.XmlNamespaceMgt);
          if (tmpNode != null)
          {
            captionML.Append(" '");
            captionML.Append(tmpNode.InnerText);
            captionML.Append("'");
          }

          CommentRemoval(action, captionML.ToString());
          action.ParentNode.RemoveChild(action);
        }
        else if ((action.SelectSingleNode(@"./a:ToolTipML", metaDataDocMgt.XmlNamespaceMgt) != null) &&
                 (action.SelectSingleNode(@"./a:CaptionML", metaDataDocMgt.XmlNamespaceMgt) == null))
        {
          action.AppendChild(
            XmlUtility.CreateXmlElement(
              "CaptionML",
              action.SelectSingleNode(@"./a:ToolTipML", metaDataDocMgt.XmlNamespaceMgt).InnerText));
        }
      }

      // Prepare ActionGroups for remove empty nodes: remove groups with only separators inside.
      XmlNodeList actionGroupList = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:ActionGroup", metaDataDocMgt.XmlNamespaceMgt);
      foreach (XmlNode group in actionGroupList)
      {
        XmlNodeList separatorCountList = group.SelectNodes(@"./a:Separator", metaDataDocMgt.XmlNamespaceMgt);
        if (group.ChildNodes.Count - 1 == separatorCountList.Count)
        {
          group.ParentNode.RemoveChild(group);
        }
      }

      CleanRedundantSeparators();

      RemoveEmptyNodes(@".//a:ActionGroup");
      RemoveEmptyNodes(@".//a:RelatedInformation");
      RemoveEmptyNodes(@".//a:ActionItems");

      AddShortcutsByCaption();
      AddImageByCaption();
      AddImageAsBigByCaption();
    }

    #endregion

    #region Private static methods

    private static void AddShortcutsByCaption()
    {
      ApplyMoveElementsRules(MovePageElements.NotFormsId.AddShortcuts, "ShortCutKey", null);
    }

    private static void AddImageByCaption()
    {
      ApplyMoveElementsRules(MovePageElements.NotFormsId.AddImages, "Image", null);
    }

    private static void AddImageAsBigByCaption()
    {
      ApplyMoveElementsRules(MovePageElements.NotFormsId.AddPromotedIsBig, "PromotedIsBig", "Yes");
    }

    private static void SetPromotedProperty()
    {
      ApplyMoveElementsRules(MovePageElements.NotFormsId.PromotedActions, "Promoted", "Yes");
    }

    /// <summary>
    /// To apply some -x CaptionMLbased changes from MoveElements file.
    /// </summary>
    /// <param name="notFormsID">?abc?</param>
    /// <param name="nodeName">?abc?</param>
    /// <param name="newValue">?abc?</param>
    private static void ApplyMoveElementsRules(MovePageElements.NotFormsId notFormsID, string nodeName, string newValue)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      if (metaDataDocMgt.MoveElementsDoc == null)
      {
        return;
      }

      bool fetchNewValue = false;

      if (newValue == null)
      {
        fetchNewValue = true;
      }

      StringBuilder queryPromoted = new StringBuilder();
      queryPromoted.Append(@"./a:MovePageElements/a:Page[@ID='");
      queryPromoted.Append(((int)notFormsID).ToString(CultureInfo.InvariantCulture));
      queryPromoted.Append("']");

      XmlNodeList promotedList = metaDataDocMgt.MoveElementsDoc.SelectNodes(queryPromoted.ToString(), metaDataDocMgt.XmlNamespaceMgt);

      foreach (XmlNode promoted in promotedList)
      {
        XmlNode captionMLNode = promoted.Attributes.GetNamedItem("CaptionML");
        string captionML = null;

        if (captionMLNode == null)
        {
          continue;
        }

        if (fetchNewValue)
        {
          XmlNode shortCutKeyNode = promoted.Attributes.GetNamedItem("NewName");

          if (shortCutKeyNode == null)
          {
            continue;
          }

          newValue = shortCutKeyNode.Value;
        }

        captionML = captionMLNode.Value;

        XmlNodeList actionList = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Action[./a:CaptionML]", metaDataDocMgt.XmlNamespaceMgt);

        foreach (XmlNode action in actionList)
        {
          if (MatchCaptionML(action, captionML))
          {
            XmlNode promotedTMP = action.SelectSingleNode(@"./a:" + nodeName, metaDataDocMgt.XmlNamespaceMgt);
            if (promotedTMP == null)
            {
              action.AppendChild(XmlUtility.CreateXmlElement(nodeName, newValue));
            }
            else
            {
              promotedTMP.InnerText = newValue;
            }

            if (notFormsID == MovePageElements.NotFormsId.PromotedActions)
            {
              XmlNode promotedCategory = action.SelectSingleNode(@"./a:PromotedCategory", metaDataDocMgt.XmlNamespaceMgt);
              if (promotedCategory == null)
              {
                action.AppendChild(XmlUtility.CreateXmlElement("PromotedCategory", "Process"));
              }
            }
          }
        }
      }
    }

    private static void RemoveEmptyNodes(string searchIn)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      XmlNodeList actionGroupList = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(searchIn, metaDataDocMgt.XmlNamespaceMgt);
      foreach (XmlNode group in actionGroupList)
      {
        if (group.ChildNodes.Count < 2)
        {
          group.ParentNode.RemoveChild(group);
          RemoveEmptyNodes(searchIn);
        }
      }
    }

    private static void ResetPageActionType()
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;

      if (metaDataDocMgt.MoveElementsDoc == null)
      {
        return;
      }

      StringBuilder queryPromoted = new StringBuilder();
      queryPromoted.Append(@"./a:MovePageElements/a:Page[@ID='");
      queryPromoted.Append(((int)MovePageElements.NotFormsId.ActionGroup).ToString(CultureInfo.InvariantCulture));
      queryPromoted.Append("']");

      XmlNodeList moveCaptionsList = metaDataDocMgt.MoveElementsDoc.SelectNodes(queryPromoted.ToString(), metaDataDocMgt.XmlNamespaceMgt);
      foreach (XmlNode n in moveCaptionsList)
      {
        XmlNode captionMLNode = n.Attributes.GetNamedItem("CaptionML");
        if (captionMLNode == null)
        {
          continue;
        }

        XmlNode destinationTypeNode = n.Attributes.GetNamedItem("destinationType");
        if (destinationTypeNode == null)
        {
          continue;
        }

        StringBuilder queryActionGroupProperties = new StringBuilder();
        queryActionGroupProperties.Append(".//a:Actions/a:");
        queryActionGroupProperties.Append(DefaultPageActionType);
        queryActionGroupProperties.Append("//a:ActionGroup/a:Properties[./a:CaptionML]");

        XmlNodeList actionGroupPropertiesList = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(
          queryActionGroupProperties.ToString(),
          metaDataDocMgt.XmlNamespaceMgt);

        if (actionGroupPropertiesList.Count == 0)
        {
          continue;
        }

        foreach (XmlNode actionGroupProperties in actionGroupPropertiesList)
        {
          if (MatchCaptionML(actionGroupProperties, captionMLNode.Value))
          {
            XmlNode newPageActionType = GetPageActionNode(destinationTypeNode.Value);
            actionGroupProperties.ParentNode.ParentNode.RemoveChild(actionGroupProperties.ParentNode);
            newPageActionType.AppendChild(actionGroupProperties.ParentNode);
          }
        }
      }
    }

    private static void MovePushAction(XmlNode sourceNode, XmlNode destionationNode)
    {
      if (sourceNode != null)
      {
        XmlNode onActioNode = XmlUtility.CreateXmlElement("OnAction");
        XmlCDataSection data = XmlUtility.CreateCDataSection(sourceNode.InnerText);
        onActioNode.AppendChild(data);
        destionationNode.AppendChild(onActioNode);
      }
    }

    private static bool MatchCaptionML(XmlNode propertyNode, string captionML)
    {
      string propertyCaptionML = RemoveQuickAccessKey(propertyNode);
      string cleanedCaptionML = RemoveAmpersant(captionML);

      propertyCaptionML = propertyCaptionML.Replace("\r", string.Empty);
      propertyCaptionML = propertyCaptionML.Replace("\n", string.Empty);

      if ((propertyCaptionML == captionML) || (propertyCaptionML == cleanedCaptionML))
      {
        return true;
      }

      // Support multilanguage string in CaptionML (separated by ";")
      // The CaptionML string doesn't contain any start/end delimiter in Forms.xml exported from Object Designer
      if ((propertyCaptionML.Contains(captionML + ";")) || (propertyCaptionML.Contains(cleanedCaptionML + ";")))
      {
        return true;
      }

      if ((propertyCaptionML.EndsWith(";" + captionML, StringComparison.Ordinal)) || (propertyCaptionML.EndsWith(";" + cleanedCaptionML, StringComparison.Ordinal)))
      {
        return true;
      }

      return false;
    }

    private static string RemoveQuickAccessKey(XmlNode propertyNode)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      XmlNode captionMLNode = propertyNode.SelectSingleNode("./a:CaptionML", metaDataDocMgt.XmlNamespaceMgt);

      if (captionMLNode != null)
      {
        return RemoveAmpersant(captionMLNode.InnerXml);
      }

      return string.Empty;
    }

    private static string RemoveAmpersant(string captionML)
    {
      string s = "&amp;";

      captionML = captionML.Replace("&&", "ZZZXXXZZZampZZZXXXZZZ");
      captionML = captionML.Replace(s, string.Empty);
      captionML = captionML.Replace("ZZZXXXZZZampZZZXXXZZZ", "&&");

      return captionML;
    }

    private static void CommentControlRemoval(XmlNodeList listToComment, string removeDescription)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;

      foreach (XmlNode n in listToComment)
      {
        string prefix;
        if (n.Name == "Control")
        {
          prefix = "./a:Properties/a:";
        }
        else
        {
          prefix = "./a:";
        }

        string controlType = string.Empty;
        XmlNode tmpNode = n.SelectSingleNode(prefix + "Controltype", metaDataDocMgt.XmlNamespaceMgt);
        if (tmpNode != null)
        {
          controlType = tmpNode.InnerText;
        }

        string newDescription;
        if (!string.IsNullOrEmpty(removeDescription))
        {
          newDescription = removeDescription + ". " + controlType;
        }
        else
        {
          newDescription = controlType;
        }

        CommentRemoval(n, newDescription);
      }
    }

    private static void RemoveCommentButtons()
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      XmlNodeList commentsPBList = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(
        @".//a:Controls//a:Control/a:Properties[./a:Controltype='PictureBox' and ./a:BitmapList='7,6']",
        metaDataDocMgt.XmlNamespaceMgt);
      foreach (XmlNode commentPB in commentsPBList)
      {
        string xpos = XmlUtility.GetNodeValue(commentPB, "./a:XPos", string.Empty);
        string ypos = XmlUtility.GetNodeValue(commentPB, "./a:YPos", string.Empty);

        string query = String.Format(
          CultureInfo.InvariantCulture,
          @".//a:Controls//a:Control/a:Properties[./a:Controltype='CommandButton' and ./a:XPos={0} and ./a:YPos={1}]",
          xpos,
          ypos);
        XmlNode commandButton = metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(query, metaDataDocMgt.XmlNamespaceMgt);

        if (commandButton != null)
        {
          string log = string.Format(
            CultureInfo.InvariantCulture,
            Resources.RemoveCommentButton,
            XmlUtility.GetNodeValue(commandButton, "./a:ID", string.Empty),
            XmlUtility.GetNodeValue(commentPB, "./a:ID", string.Empty));
          TransformationLog.GenericLogEntry(log, LogCategory.RemoveControls);

          commentPB.ParentNode.ParentNode.RemoveChild(commentPB.ParentNode);
          commandButton.ParentNode.ParentNode.RemoveChild(commandButton.ParentNode);
        }
      }
    }

    private static void MoveCommandButtonsToActionPage(XmlNode commandButtonNode)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      XmlNode actionDetailsNode = GetPageActionNode("ActionItems");

      XmlNode actionNode = XmlUtility.CreateXmlElement("Action");
      actionDetailsNode.AppendChild(actionNode);

      if (IsWizard())
      {
        actionNode.AppendChild(XmlUtility.CreateXmlElement("InFooterBar", "Yes"));
      }
      else
      {
        actionNode.AppendChild(XmlUtility.CreateXmlElement("Promoted", "Yes"));
        actionNode.AppendChild(XmlUtility.CreateXmlElement("PromotedCategory", "Process"));
      }

      XmlNode commandButtonsToMove = commandButtonNode.SelectSingleNode("./a:Properties", metaDataDocMgt.XmlNamespaceMgt);

      XmlUtility.MoveChilds(commandButtonsToMove, actionNode, InsertOrder.Prepend);

      MovePushAction(commandButtonNode.SelectSingleNode("./a:Triggers/a:OnPush", metaDataDocMgt.XmlNamespaceMgt), actionNode);
    }

    ////private static void MoveCommandButtonsToActionBand(XmlNode commandButtonNode, XmlNode parentPropNode)
    ////{
    ////  MetaDataDocumentManagement metaDataDocMgt = MetaDataDocumentManagement.Instance;
    ////  XmlNode actionGroupNode = GetBandActionGroupNode(parentPropNode);

    ////  XmlNode actionNode = XMLUtility.CreateXmlElement("Action");
    ////  actionGroupNode.AppendChild(actionNode);

    ////  XmlNode commandButtonsToMove = commandButtonNode.SelectSingleNode("./a:Properties", metaDataDocMgt.XmlNamespaceMgt);
    ////  RemoveAmpersand(commandButtonsToMove);

    ////  XMLUtility.MoveChilds(commandButtonsToMove, actionNode, InsertOrder.Prepend);

    ////  XmlNode triggersToMove = commandButtonNode.SelectSingleNode("./a:Triggers/a:OnPush", metaDataDocMgt.XmlNamespaceMgt);
    ////  MovePushAction(triggersToMove, actionNode);
    ////}

    private static XmlNode CreateMenuButton(Int32 subFormPageId, string captionML)
    {
      XmlNode newMenuButon = XmlUtility.CreateXmlElement("Control");
      XmlNode newProperties = XmlUtility.CreateXmlElement("Properties");

      string newID = MetadataDocumentManagement.Instance.CalcId(
        subFormPageId, null, XmlUtility.GetCaption(captionML), "ActionItems").ToString(CultureInfo.InvariantCulture); 

      newProperties.AppendChild(XmlUtility.CreateXmlElement("ID", newID));
      newProperties.AppendChild(XmlUtility.CreateXmlElement("Controltype", "MenuButton"));
      newProperties.AppendChild(XmlUtility.CreateXmlElement("CaptionML", captionML));
      newProperties.AppendChild(XmlUtility.CreateXmlElement("XPos", "0"));
      newProperties.AppendChild(XmlUtility.CreateXmlElement("YPos", "0"));
      newMenuButon.AppendChild(newProperties);

      XmlNode newMenu = XmlUtility.CreateXmlElement("Menu");
      newMenuButon.AppendChild(newMenu);

      return newMenuButon;
    }

    private static XmlNode GetSubForm(ref String subFormID, string subformControlName)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      XmlNode subformControl = metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(
        string.Format(CultureInfo.InvariantCulture, "./a:Controls//a:Control[./a:Properties/a:Name='{0}']", subformControlName),
        metaDataDocMgt.XmlNamespaceMgt);

      if (subformControl != null)
      {
        XmlNode partID = subformControl.SelectSingleNode(".//a:PagePartID", metaDataDocMgt.XmlNamespaceMgt);
        if (partID != null)
        {
          subFormID = partID.InnerText.Replace("Page", String.Empty);
          XmlNode subForm = metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(
            "../a:Page[@ID=" + subFormID + "]",
            metaDataDocMgt.XmlNamespaceMgt);
          return subForm;
        }
      }

      return null;
    }

    private static bool GetSubFormControl(ref XmlNode subformControls, ref XmlNode subForm, string subformControlName)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      String subFormID = string.Empty;
      subForm = GetSubForm(ref subFormID, subformControlName);
      if (subForm == null)
      {
        string logStr = string.Format(
          CultureInfo.InvariantCulture,
          Resources.CanNotFindSubform,
          string.Empty,
          subFormID);
        TransformationLog.GenericLogEntry(logStr, LogCategory.Error, metaDataDocMgt.GetCurrentPageId);
        subformControls = null;
        return false;
      }

      if (subformControls == null)
      {
        subformControls = subForm.SelectSingleNode("./a:Controls", metaDataDocMgt.XmlNamespaceMgt);
        if (subformControls == null)
        {
          string logStr = string.Format(
            CultureInfo.InvariantCulture,
            Resources.CanNotFindSubform,
            "Controls on ",
            subFormID);
          TransformationLog.GenericLogEntry(logStr, LogCategory.RemoveControls, metaDataDocMgt.GetCurrentPageId);
          return false;
        }
      }

      return true;
    }

    private static bool IsEmptyAction(XmlNode action)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      XmlNode test;

      test = action.SelectSingleNode("./a:Properties/a:RunObject", metaDataDocMgt.XmlNamespaceMgt);
      if (test != null)
      {
        return false;
      }

      test = action.SelectSingleNode("./a:RunObject", metaDataDocMgt.XmlNamespaceMgt);
      if (test != null)
      {
        return false;
      }

      test = action.SelectSingleNode("./a:Triggers/a:OnPush", metaDataDocMgt.XmlNamespaceMgt);
      if (test != null)
      {
        return false;
      }

      test = action.SelectSingleNode("./a:OnAction", metaDataDocMgt.XmlNamespaceMgt);
      if (test != null)
      {
        return false;
      }

      test = action.SelectSingleNode("./a:Properties[./a:MenuItemType='Separator']", metaDataDocMgt.XmlNamespaceMgt);
      if (test != null)
      {
        return false;
      }

      test = action.SelectSingleNode("./a:ActionControlType", metaDataDocMgt.XmlNamespaceMgt);
      if (test != null)
      {
        return false;
      }

      test = action.SelectSingleNode("./a:Description", metaDataDocMgt.XmlNamespaceMgt);
      if (test != null)
      {
        return false;
      }

      return true;
    }

    private static void InsertMenuItem(Int32 subFormPageId, XmlNode subformMenu, XmlNode newMenuItem)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;

      XmlNode subformMenuID = newMenuItem.SelectSingleNode("./a:Properties/a:ID", metaDataDocMgt.XmlNamespaceMgt);
      subformMenuID.InnerText = 
        metaDataDocMgt.CalcId(subFormPageId, null, subformMenuID.InnerText.ToString(CultureInfo.InvariantCulture), "ActionItems").ToString(CultureInfo.InvariantCulture);  

      subformMenu.AppendChild(newMenuItem.Clone());
    }

    private static void MoveMenuButtonsToSubForm(XmlNode menuButton)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      XmlNode subForm = null;
      string previouseSubFormName = string.Empty;
      string menuButtonCaption = string.Empty;

      menuButtonCaption = GetControlCaption(menuButton);

      XmlNodeList menuItemList = menuButton.SelectNodes(@"./a:Menu/a:Control", metaDataDocMgt.XmlNamespaceMgt);
      if (menuItemList.Count > 0)
      {
        XmlNode subformControls = null;
        Regex currPageSubFormCodeExpression = new Regex(PatternForCallToSubform, RegexOptions.IgnoreCase);
        XmlNode delayedInsert = null;

        foreach (XmlNode n in menuItemList)
        {
          if (IsEmptyAction(n))
          {
            if (delayedInsert != null && subForm != null)
            {
              XmlNode subformMenu = subformControls.SelectSingleNode(
                "./a:Control/a:Menu[../a:Properties/a:Controltype='MenuButton' and ../a:Properties/a:CaptionML='" + menuButtonCaption + "']",
                metaDataDocMgt.XmlNamespaceMgt);
              if (subformMenu != null)
              {
                InsertMenuItem(0, subformMenu, delayedInsert.Clone());
              }
            }

            delayedInsert = n;
            continue;
          }

          XmlNode onPush = n.SelectSingleNode("./a:Triggers/a:OnPush", metaDataDocMgt.XmlNamespaceMgt);
          if (onPush != null)
          {
            string triggerCodeOld = onPush.InnerText;

            if (currPageSubFormCodeExpression.Match(triggerCodeOld).Success)
            {
              string currentSubformName = currPageSubFormCodeExpression.Match(triggerCodeOld).Result("${SubFormControl}");
              if (string.Compare(currentSubformName, previouseSubFormName, StringComparison.OrdinalIgnoreCase) != 0)
              {
                previouseSubFormName = currentSubformName;
                if (!GetSubFormControl(ref subformControls, ref subForm, currentSubformName))
                {
                  continue;
                }
              }

              bool moveCodeToSubform = CheckIfTriggerCanBeMovedToSubpage(currPageSubFormCodeExpression, triggerCodeOld);

              if (moveCodeToSubform)
              {
                if (subForm != null)
                {
                  MoveMenuItemToSubform(subForm, menuButtonCaption, delayedInsert, n, onPush);
                  delayedInsert = null;
                }
              }
            }
          }
        }
      }
    }

    private static string GetControlCaption(XmlNode menuButton)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      string menuButtonCaption;
      XmlNode menuCaptionML;

      menuCaptionML = menuButton.SelectSingleNode("./a:Properties/a:CaptionML", metaDataDocMgt.XmlNamespaceMgt);
      if (menuCaptionML == null)
      {
        menuCaptionML = menuButton.SelectSingleNode("./a:Properties/a:Name", metaDataDocMgt.XmlNamespaceMgt);
        if (menuCaptionML == null)
        {
          menuCaptionML = menuButton.SelectSingleNode("./a:Properties/a:ID", metaDataDocMgt.XmlNamespaceMgt);
          menuButtonCaption = "ENU=Control_" + menuCaptionML.InnerText;
        }
        else
        {
          menuButtonCaption = "ENU=" + menuCaptionML.InnerText;
        }
      }
      else
      {
        menuButtonCaption = menuCaptionML.InnerText;
      }

      return menuButtonCaption;
    }

    private static bool TryToMoveCommandButtonToSubPage(XmlNode commandButton)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      Regex currPageSubFormCodeExpression = new Regex(PatternForCallToSubform, RegexOptions.IgnoreCase);
      //XmlNode delayedInsert = null;

      if (!IsEmptyAction(commandButton))
      {
        XmlNode onPush = commandButton.SelectSingleNode("./a:Triggers/a:OnPush", metaDataDocMgt.XmlNamespaceMgt);
        if (onPush != null)
        {
          string triggerCodeOld = onPush.InnerText;

          if (currPageSubFormCodeExpression.Match(triggerCodeOld).Success)
          {
            string currentSubformName = currPageSubFormCodeExpression.Match(triggerCodeOld).Result("${SubFormControl}");
            XmlNode subForm = null;
            if (!GetSubFormControl(ref commandButton, ref subForm, currentSubformName))
            {
              return false;
            }

            bool moveCodeToSubform = CheckIfTriggerCanBeMovedToSubpage(currPageSubFormCodeExpression, triggerCodeOld);

            if (moveCodeToSubform)
            {
              if (subForm != null)
              {
                XmlNode subFormControls = subForm.SelectSingleNode("./a:Controls", metaDataDocMgt.XmlNamespaceMgt);
                if (subFormControls != null)
                {
                  if (PrepareTriggerCodeForMovedAction(subForm, onPush))
                  {
                    XmlNode subformMenuID = commandButton.SelectSingleNode("./a:Properties/a:ID", metaDataDocMgt.XmlNamespaceMgt);
                    Int32 subFormPageId = Convert.ToInt32(subForm.SelectSingleNode("@ID").Value, CultureInfo.InvariantCulture);
                    subformMenuID.InnerText =
                      metaDataDocMgt.CalcId(subFormPageId, null, subformMenuID.InnerText.ToString(CultureInfo.InvariantCulture), "ActionItems").ToString(CultureInfo.InvariantCulture);  

                    subFormControls.AppendChild(commandButton);
                    return true;
                  }
                }
              }
            }
          }
        }
      }

      return false;
    }

    /// <summary>
    /// Check if trigger could be moved automatically to subpage
    /// </summary>
    /// <param name="currPageSubFormCodeExpression">Pattern with call to subform</param>
    /// <param name="OriginalCode">Trigger's original code</param>
    /// <returns>?abc?</returns>
    private static bool CheckIfTriggerCanBeMovedToSubpage(Regex currPageSubFormCodeExpression, string OriginalCode)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;

      // Change code (remove subform, etc.)
      char[] charSeparators = new char[] { ';', '\r', '\n' };
      bool moveCodeToSubform = true;
      string[] triggerCodeTMP = OriginalCode.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
      for (int i = 0; i < triggerCodeTMP.Length; i++)
      {
        if (!(triggerCodeTMP[i] == "END" || triggerCodeTMP[i] == "BEGIN"))
        {
          string checkForCommentString = triggerCodeTMP[i].Trim();
          if ((checkForCommentString.Length > 1) && (checkForCommentString.Substring(0, 2) != "//"))
          {
            if (!currPageSubFormCodeExpression.Match(triggerCodeTMP[i]).Success)
            {
              // TODO: should we make it uncompilable?
              // triggerCodeTMP[i] = "!!!TEST!!!" + triggerCodeTMP[i];
              moveCodeToSubform = false;
            }
            else
            {
              Regex functionNameExpPlusParameters =
                new Regex(@"\((?<params>\w+)\)", RegexOptions.IgnoreCase);

              Match anyParams = functionNameExpPlusParameters.Match(checkForCommentString);
              if (anyParams.Success)
              {
                string[] parameters = anyParams.Result("${params}").Split(new char[] { ',' });
                foreach (string p in parameters)
                {
                  if (p.StartsWith("'", StringComparison.Ordinal))
                    continue;
                  if ((string.Compare(p, "TRUE", StringComparison.OrdinalIgnoreCase) == 0) || (string.Compare(p, "FALSE", StringComparison.OrdinalIgnoreCase) == 0))
                    continue;

                  try
                  {
                    Convert.ToInt32(p, CultureInfo.InvariantCulture);
                    continue;
                  }
                  catch (OverflowException)
                  {
                    continue;
                  }
                  catch (FormatException)
                  {
                    // TODO: should we make it uncompilable?
                    // triggerCodeTMP[i] = "!!!TEST!!!" + triggerCodeTMP[i];
                    moveCodeToSubform = false;
                    break;
                  }
                }
              }
            }
          }
        }

        if (!moveCodeToSubform)
        {
          // TODO: should we make it uncompilable?
          // onPush.InnerText = string.Concat(triggerCodeTMP);
          TransformationLog.GenericLogEntry(Resources.TriggerCanNotBeConvertedAutomatically, LogCategory.ChangeCodeManually, metaDataDocMgt.GetCurrentPageId, OriginalCode);
          break;
        }
      }
      return moveCodeToSubform;
    }

    private static void MoveMenuItemToSubform(
      XmlNode subForm,
      string menuButtonCaption,
      XmlNode delayedInsert,
      XmlNode n,
      XmlNode onPush)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      Int32 subFormPageId = Convert.ToInt32(subForm.SelectSingleNode("@ID").Value, CultureInfo.InvariantCulture);
      XmlNode subformControls = subForm.SelectSingleNode("./a:Controls", metaDataDocMgt.XmlNamespaceMgt);

      // Make a MenuButton
      XmlNode subformMenuButton_tmp = subformControls.SelectSingleNode(
        "./a:Control[./a:Properties/a:Controltype='MenuButton' and ./a:Properties/a:CaptionML='" + menuButtonCaption + "']",
        metaDataDocMgt.XmlNamespaceMgt);

      if (subformMenuButton_tmp == null)
      {
        subformMenuButton_tmp = CreateMenuButton(subFormPageId, menuButtonCaption);
        subformControls.AppendChild(subformMenuButton_tmp);
      }

      if (!PrepareTriggerCodeForMovedAction(subForm, onPush))
      {
        return;
      }

      XmlNode subformMenu = subformMenuButton_tmp.SelectSingleNode("./a:Menu", metaDataDocMgt.XmlNamespaceMgt);
      XmlNode tmpMF = n.SelectSingleNode("./a:Properties/a:MenuLevel", metaDataDocMgt.XmlNamespaceMgt);
      if (tmpMF != null && delayedInsert != null)
      {

        InsertMenuItem(subFormPageId, subformMenu, delayedInsert.Clone());
        delayedInsert = null;
      }

      // Delete old MItem
      n.ParentNode.RemoveChild(n);

      InsertMenuItem(subFormPageId, subformMenu, n);
      return;
    }

    /// <summary>
    /// This function will update trigger code for Action that should be moved to subpage
    /// </summary>
    /// <param name="subForm">subpage node</param>
    /// <param name="onPush">onPush trigger. Code will be updated</param>
    /// <returns>False - action couldn't be moved to subpage</returns>
    private static bool PrepareTriggerCodeForMovedAction(XmlNode subForm, XmlNode onPush)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      Regex functionCallExp = new Regex(@"CurrPage\.\b\w*\b\.FORM\.\b\w*\b", RegexOptions.IgnoreCase);

      string triggerCodeOld = onPush.InnerText;
      string functionName = functionCallExp.Match(triggerCodeOld).Value;
      // check if function have a spaces
      if (string.IsNullOrEmpty(functionName))
      {
        TransformationLog.GenericLogEntry(Resources.TriggerCanNotBeConvertedAutomatically, LogCategory.ChangeCodeManually, metaDataDocMgt.GetCurrentPageId, triggerCodeOld);
        return false;
      }

      Regex currPageSubFormCodeExpression = new Regex(PatternForCallToSubform, RegexOptions.IgnoreCase);
      Regex functionNameExp = new Regex(@"\.FORM\.\b\w*\b", RegexOptions.IgnoreCase);

      string functionName_PreconditionStep1 = functionCallExp.Match(triggerCodeOld).Value;
      string functionName_PreconditionStep2 = functionNameExp.Match(functionName_PreconditionStep1).Value;

      functionName = functionName_PreconditionStep2.Remove(0, 6);
      string triggerCodeNew;
      if (FunctionUsingCounter(subForm, functionName) > 1)
      {
        string newFunctionName = GetNewFunctionName(subForm, functionName);
        TransformationLog.GenericLogEntry(Resources.TriggerConvertedAutomatically, LogCategory.ValidateManually, metaDataDocMgt.GetCurrentPageId, triggerCodeOld);

        string comment = string.Format(
          CultureInfo.InvariantCulture,
          Resources.MoveMenuButtonsToSubForm,
          metaDataDocMgt.GetCurrentPageId.ToString(CultureInfo.InvariantCulture),
          currPageSubFormCodeExpression.Match(triggerCodeOld).Value);

        triggerCodeNew = currPageSubFormCodeExpression.Replace(triggerCodeOld, comment);
        triggerCodeNew = triggerCodeNew.Replace(functionName, newFunctionName);
      }
      else
      {
        TransformationLog.GenericLogEntry(Resources.TriggerConvertedAutomatically, LogCategory.ValidateManually, metaDataDocMgt.GetCurrentPageId, triggerCodeOld);

        string comment = string.Format(
          CultureInfo.InvariantCulture,
          Resources.MoveMenuButtonsToSubForm,
          metaDataDocMgt.GetCurrentPageId.ToString(CultureInfo.InvariantCulture),
          currPageSubFormCodeExpression.Match(triggerCodeOld).Value);

        triggerCodeNew = currPageSubFormCodeExpression.Replace(triggerCodeOld, comment);
      }

      onPush.InnerText = triggerCodeNew;
      return true;
    }

    private static int FunctionUsingCounter(XmlNode subform, string functionName)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      XmlNode functionNode = subform.SelectSingleNode("./a:Code", metaDataDocMgt.XmlNamespaceMgt);
      if (functionNode != null)
      {
        string cmd = @"\b" + functionName + @"\b";
        Regex functionRegex = new Regex(cmd, RegexOptions.IgnoreCase);
        string functionCode = functionNode.InnerText;
        int counter = functionRegex.Matches(functionCode).Count;
        return counter;
      }

      return 0;
    }

    private static string GetNewFunctionName(XmlNode subform, string functionName)
    {
      const string NewFunctionPrefix = "_";
      const int MaxFunctionNameLenght = 30;
      string functionNameNew = NewFunctionPrefix + functionName;
      Int32 subFormPageId = Convert.ToInt32(subform.SelectSingleNode("@ID").Value, CultureInfo.InvariantCulture);
      if (functionNameNew.Length > MaxFunctionNameLenght)
      {
        functionNameNew = functionNameNew.Substring(0, MaxFunctionNameLenght);
      }

      if (FunctionUsingCounter(subform, functionNameNew) > 0)
      {
        return functionNameNew;
      }

      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      XmlNode functionNode = subform.SelectSingleNode("./a:Code", metaDataDocMgt.XmlNamespaceMgt);
      if (functionNode != null)
      {
        string functionCode = functionNode.InnerXml;

        Regex functions =
          new Regex(@"[/r/n]*[LOCAL\s]*PROCEDURE \b(?<ProcedureName>\w+)\b@\d*", RegexOptions.IgnoreCase);

        MatchCollection ms = functions.Matches(functionCode);

        int codeStrat = 0, codeEnd = 0;
        string toreplace = string.Empty;
        foreach (Match m in ms)
        {
          if (m.Result("${ProcedureName}").Equals(functionName, StringComparison.OrdinalIgnoreCase))
          {
            toreplace = m.Value;
            codeStrat = m.Index;
            Match nxt = m.NextMatch();
            if (nxt.Success)
            {
              codeEnd = nxt.Index;
            }
            else
            {
              Regex codeEndExpression =          //to find Documentation section
                new Regex(@"[/r/n]*BEGIN\r\n\{|\r\nBEGIN\r\nEND\.", RegexOptions.IgnoreCase);
              Match codeEndExpressionMatch;
              codeEndExpressionMatch = codeEndExpression.Match(functionCode.Substring(codeStrat));
              if (codeEndExpressionMatch.Success)
              {
                codeEnd = codeEndExpressionMatch.Index + codeStrat;
              }
            }

            break;
          }
        }

        string functionCodeNew;
        if (codeEnd > 0)
        {
          functionCodeNew = functionCode.Substring(codeStrat, codeEnd - codeStrat);
        }
        else
        {
          functionCodeNew = functionCode.Substring(codeStrat);
        }

        functionCodeNew = functionCodeNew.Replace(toreplace, toreplace.Replace(functionName, functionNameNew));
        Regex idExpression = new Regex(@"\@\d+", RegexOptions.IgnoreCase);

        functionCodeNew = idExpression.Replace(functionCodeNew, "@" + metaDataDocMgt.CalcId(subFormPageId,null,functionNameNew,"Code"), 1);
        functionNode.InnerXml = functionNode.InnerXml.Insert(codeStrat, functionCodeNew);
        return functionNameNew;
      }

      return string.Empty;
    }

    private static bool IsPageWithStacks()
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      if (metaDataDocMgt.InsertElementsDoc == null)
      {
        return false;
      }

      StringBuilder query = new StringBuilder("./a:TransformPages/a:Page[@ID='");
      query.Append(metaDataDocMgt.GetCurrentPageId.ToString(CultureInfo.InvariantCulture));
      query.Append("' and ./a:Transformation/@FormType = 'Stack']");

      if (metaDataDocMgt.InsertElementsDoc.SelectSingleNode(query.ToString(), metaDataDocMgt.XmlNamespaceMgt) == null)
      {
        return false;
      }

      return true;
    }

    private static bool IsWizard()
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      if (metaDataDocMgt.InsertElementsDoc == null)
      {
        return false;
      }

      StringBuilder query = new StringBuilder("./a:TransformPages/a:Page[@ID='");
      query.Append(metaDataDocMgt.GetCurrentPageId.ToString(CultureInfo.InvariantCulture));
      query.Append("' and ./a:Transformation/@FormType = 'Wizard']");

      if (metaDataDocMgt.InsertElementsDoc.SelectSingleNode(query.ToString(), metaDataDocMgt.XmlNamespaceMgt) == null)
      {
        return false;
      }

      return true;
    }

    private static bool isOfficeSeparator(XmlNode actionNode, MetadataDocumentManagement metaDataDocMgt)
    {
      XmlNode testRunObject = actionNode.SelectSingleNode(@".//a:RunObject", metaDataDocMgt.XmlNamespaceMgt);
      XmlNode testPushAction = actionNode.SelectSingleNode("./a:Triggers/a:OnPush", metaDataDocMgt.XmlNamespaceMgt);

      if ((testRunObject == null) && (testPushAction == null))
      {
        return true;
      }

      return false;
    }

    private static String ReplaceCardWithListInAction(XmlNode actionNode)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;

      if ((actionNode == null) || (metaDataDocMgt.InsertElementsDoc == null))
      {
        return null;
      }

      XmlNode runObject = actionNode.SelectSingleNode(@".//a:RunObject", metaDataDocMgt.XmlNamespaceMgt);
      XmlNode shortCutKey = actionNode.SelectSingleNode(@".//a:ShortCutKey", metaDataDocMgt.XmlNamespaceMgt);

      if (runObject == null)
      {
        return null;
      }

      if (!runObject.InnerText.StartsWith("Page", StringComparison.Ordinal))
      {
        return null;
      }

      // Card or details - list is not required or expected
      if (shortCutKey != null)
      {
        if ((shortCutKey.InnerText == "Shift+F7") || (shortCutKey.InnerText == "Shift+F5"))
        {
          return null;
        }
      }

      string runObjectID = runObject.InnerText.Remove(0, 5);

      XmlNode runObjectPageType = metaDataDocMgt.InsertElementsDoc.SelectSingleNode(
        @".//a:TransformPages//a:Page/a:Properties[../@ID=" + runObjectID + " and ./a:PageType]",
        metaDataDocMgt.XmlNamespaceMgt);

      if (runObjectPageType == null)
      {
        return null;
      }

      if ((runObjectPageType.InnerText == "Card") || (runObjectPageType.InnerText == "Document") || (runObjectPageType.InnerText == "ListPlus"))
      {
        XmlNodeList listFoundByCardFormID = metaDataDocMgt.InsertElementsDoc.SelectNodes(
          @".//a:TransformPages//a:Page[./a:Properties/a:CardFormID=" + runObjectID + "]",
          metaDataDocMgt.XmlNamespaceMgt);

        if (listFoundByCardFormID.Count > 0)
        {
          //@TODO Log change (Replaced ID in action) + if more than 1 form is found by CardFormID

          return "Page " + listFoundByCardFormID[0].Attributes.GetNamedItem("ID").Value;
        }
      }

      return null;
    }

    /// <summary>
    /// To clean redundant Separators from ActionGroups
    /// </summary>
    private static void CleanRedundantSeparators()
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      XmlNodeList actionGroups = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Actions//a:ActionGroup", metaDataDocMgt.XmlNamespaceMgt);
      const string Separator = "Separator";
      foreach (XmlNode group in actionGroups)
      {
        bool separatorFound = false;
        for (int i = 0; i < group.ChildNodes.Count; i++)
        {
          XmlNode action = group.ChildNodes[i];
          if (separatorFound)
          {
            if (action.Name == Separator)
            {
              action.ParentNode.RemoveChild(action);
              i--;
            }
            else
            {
              separatorFound = false;
            }
          }
          else
          {
            separatorFound = (action.Name == Separator);
          }
        }

        if (separatorFound && (group.ChildNodes.Count == 2))
        {
          group.RemoveAll();
        }
      }
    }
    #endregion
  }
}
