//--------------------------------------------------------------------------
// <copyright file="CleaningUp.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>?abc?</summary>
//--------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.Tools.FormTransformation
{
  /// <summary>
  /// ?abc?
  /// </summary>
  internal static class CleaningUp
  {
    /// <summary>
    /// ?abc?
    /// </summary>
    /// 

    private static void UpdatePropertyValuesYesNoToTRUEFALSE(XmlNodeList formPropList)
    {
      foreach (XmlNode formPropNode in formPropList)
      {
        switch (formPropNode.InnerText)
        {
          case "No":
            formPropNode.InnerText = "FALSE";
            break;
          case "Yes":
            formPropNode.InnerText = "TRUE";
            break;
        }
      }
    }

    public static void CleanProperties()
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      bool isForm = (metaDataDocMgt.GetCurrentPageId > 0);

      UpdatePropertyValuesYesNoToTRUEFALSE(
        metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Controls//a:Properties/a:FontItalic", metaDataDocMgt.XmlNamespaceMgt));
      UpdatePropertyValuesYesNoToTRUEFALSE(
        metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Controls//a:Properties/a:FontStrikeThru", metaDataDocMgt.XmlNamespaceMgt));
      UpdatePropertyValuesYesNoToTRUEFALSE(
        metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Controls//a:Properties/a:Editable", metaDataDocMgt.XmlNamespaceMgt));
      UpdatePropertyValuesYesNoToTRUEFALSE(
        metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Properties/a:Enabled", metaDataDocMgt.XmlNamespaceMgt));
      UpdatePropertyValuesYesNoToTRUEFALSE(
        metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Properties/a:Visible", metaDataDocMgt.XmlNamespaceMgt));
      if (isForm)
      {
        UpdatePropertyValuesYesNoToTRUEFALSE(
          metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@"./a:Properties/a:LookupMode", metaDataDocMgt.XmlNamespaceMgt));
      }

      CodeTransformationRules.PerformMoveToTriggerActions(true);
      CodeTransformationRules.PerformMoveToPropertyActions();
      if (isForm)
      {
        CodeTransformationRules.LookupModePropertyToOnInitTrigger();
      }

      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, ".//a:Properties/a:tempNode_InstructionalTextML", metaDataDocMgt.XmlNamespaceMgt);
      UpdateNewIdNodes();

      /* PageProperties */
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Properties/a:Caption", metaDataDocMgt.XmlNamespaceMgt);
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Properties/a:DataCaptionExpr", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Properties/a:BorderStyle", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Properties/a:CaptionBar", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Properties/a:Minimizable", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Properties/a:Maximizable", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Properties/a:Sizeable", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Properties/a:LogWidth", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Properties/a:LogHeight", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Properties/a:Width", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Properties/a:Height", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Properties/a:XPos", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Properties/a:YPos", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Properties/a:BackColor", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Properties/a:Visible", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Properties/a:ActiveControlOnOpen", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Properties/a:MinimizedOnOpen", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Properties/a:MaximizedOnOpen", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Properties/a:AutoPosition", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Properties/a:TableBoxID", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Properties/a:LookupMode", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Properties/a:CalcFields", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Properties/a:SourceTablePlacement", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Properties/a:SourceTableRecord", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Properties/a:SaveTableView", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Properties/a:SaveControlInfo", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Properties/a:SaveColumnWidths", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Properties/a:SavePosAndSize", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Properties/a:UpdateOnActivate", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Properties/a:HorzGrid", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Properties/a:VertGrid", metaDataDocMgt.XmlNamespaceMgt);
            
      /* PageControls */
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:XPos", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:YPos", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:Width", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:Height", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:HorzGlue", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:VertGlue", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:Focusable", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:FocusOnClick", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:Default", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:Cancel", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:ParentControl", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:InFrame", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:InPage", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:InColumn", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:InMatrix", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:InMatrixHeading", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:Caption", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:ShowCaption", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:HorzAlign", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:VertAlign", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:BackColor", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:BackTransparent", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:Border", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:BorderColor", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:BorderStyle", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:BorderWidth", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:FontName", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:FontSize", metaDataDocMgt.XmlNamespaceMgt);
      
      SetEmphasis();
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:FontBold", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:ForeColor", metaDataDocMgt.XmlNamespaceMgt);
      
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:FontItalic", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:FontStrikethru", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:FontUnderline", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:PadChar", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:LeaderDots", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:MaxLength", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:AutoEnter", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:BitmapPos", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:AutoRepeat", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:InvalidActionAppearance", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:Bitmap", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:BitmapList", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:ShapeStyle", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:Orientation", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:Percentage", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:MenuItemType", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:MenuLevel", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:TopLineOnly", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:PageNames", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:PageNamesML", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:RowHeight", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:MatrixColumnWidth", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:HeadingHeight", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:ToolTip", metaDataDocMgt.XmlNamespaceMgt);
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:Lookup", metaDataDocMgt.XmlNamespaceMgt);
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:DrillDown", metaDataDocMgt.XmlNamespaceMgt);
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:AssistEdit", metaDataDocMgt.XmlNamespaceMgt);
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:DropDown", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:PermanentAssist", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:InlineEditing", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:OptionString", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:OptionCaption", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:OptionValue", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:StepValue", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:ClearOnLookup", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:Format", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:SignDisplacement", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:DropDown", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, ".//a:Control/a:Properties/a:Title", metaDataDocMgt.XmlNamespaceMgt);

      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:CaptionClass", metaDataDocMgt.XmlNamespaceMgt);
      //XmlNodeList captionClassNodes = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Controls//a:CaptionClass", metaDataDocMgt.XmlNamespaceMgt);
      //foreach (XmlNode captionClassNode in captionClassNodes)
      //{
      //  captionClassNode.ParentNode.ReplaceChild(
      //    XmlUtility.CreateXmlElement("CaptionExpression", captionClassNode.LastChild.Value),
      //    captionClassNode);
      //}

      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:Divisor", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:AutoCalcField", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:ValidateTableRelation", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:RunFormLinkType", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:RunCommand", metaDataDocMgt.XmlNamespaceMgt);
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:LookupFormID", metaDataDocMgt.XmlNamespaceMgt);
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:DrillDownFormID", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:Menu", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:MatrixSourceTable", metaDataDocMgt.XmlNamespaceMgt);
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:RunFormOnRec", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:UpdateOnAction", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:NextControl", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:PushAction", metaDataDocMgt.XmlNamespaceMgt);
      // temporarily commented out
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:ClosingDates", metaDataDocMgt.XmlNamespaceMgt);
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:MultiLine", metaDataDocMgt.XmlNamespaceMgt);

      /* For ActionGroup support */
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, ".//a:Control/a:Properties/a:PushAction", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, ".//a:Control/a:Properties/a:RunObject", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, ".//a:Control/a:Properties/a:RunFormView", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, ".//a:Control/a:Properties/a:RunFormLink", metaDataDocMgt.XmlNamespaceMgt);

      /* Group */
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:Group/a:Properties/a:Controltype", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:Group/a:Properties/a:SourceExpr", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:Group/a:Properties/a:OptionCaptionML", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:Group/a:Properties/a:TableRelation", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:Group/a:Properties/a:NotBlank", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:Group/a:Properties/a:AutoFormatType", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:Group/a:Properties/a:DecimalPlaces", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:Group/a:Properties/a:MinValue", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:Group/a:Properties/a:MaxValue", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:Group/a:Properties/a:ToolTipML", metaDataDocMgt.XmlNamespaceMgt);

      /* TODO Needs logging */
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:Group/a:Triggers", metaDataDocMgt.XmlNamespaceMgt);

      /* PageActions */
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:XPos", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:YPos", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:Width", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:Height", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:HorzGlue", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:VertGlue", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:Focusable", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:FocusOnClick", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:Default", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:Cancel", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:ParentControl", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:InFrame", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:InPage", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:InColumn", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:InMatrix", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:InMatrixHeading", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:Caption", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:ShowCaption", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:HorzAlign", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:VertAlign", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:ForeColor", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:BackColor", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:BackTransparent", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:Border", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:BorderColor", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:BorderStyle", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:BorderWidth", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:FontName", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:FontSize", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:FontBold", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:FontItalic", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:FontStrikethru", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:FontUnderline", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:PadChar", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:LeaderDots", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:MaxLength", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:AutoEnter", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:BitmapPos", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:AutoRepeat", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:InvalidActionAppearance", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:Bitmap", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:BitmapList", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:ShapeStyle", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:Orientation", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:Percentage", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:MenuItemType", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:MenuLevel", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:TopLineOnly", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:PageNames", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:PageNamesML", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:RowHeight", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:MatrixColumnWidth", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:HeadingHeight", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:ToolTip", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:Lookup", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:DrillDown", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:AssistEdit", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:DropDown", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:PermanentAssist", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:InlineEditing", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:OptionString", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:OptionCaption", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:OptionValue", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:StepValue", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:ClearOnLookup", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:Format", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:SignDisplacement", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:CaptionClass", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:Divisor", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:AutoCalcField", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:ValidateTableRelation", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:RunFormLinkType", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:RunCommand", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:LookupFormID", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:DrillDownFormID", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:Menu", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:MatrixSourceTable", metaDataDocMgt.XmlNamespaceMgt);
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:RunFormOnRec", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:UpdateOnAction", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:NextControl", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:Controltype", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:MultiLine", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:SourceExpr", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:PushAction", metaDataDocMgt.XmlNamespaceMgt);

      /* Temporarily removed */
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:Visible", metaDataDocMgt.XmlNamespaceMgt);
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:Enabled", metaDataDocMgt.XmlNamespaceMgt);
      /* Temporarily removed */

      /* TODO subtype see excel???? */
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:Numeric", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:PasswordText", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:DateFormula", metaDataDocMgt.XmlNamespaceMgt);

      ///* PageTriggers */
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Triggers/a:OnInit", metaDataDocMgt.XmlNamespaceMgt);
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Triggers/a:OnQueryCloseForm", metaDataDocMgt.XmlNamespaceMgt);
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Triggers/a:OnActivateForm", metaDataDocMgt.XmlNamespaceMgt);
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Triggers/a:OnDeactivateForm", metaDataDocMgt.XmlNamespaceMgt);
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Triggers/a:OnAfterGetCurrRecord", metaDataDocMgt.XmlNamespaceMgt);
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Triggers/a:OnBeforePutRecord", metaDataDocMgt.XmlNamespaceMgt);
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Triggers/a:OnTimer", metaDataDocMgt.XmlNamespaceMgt);
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Triggers/a:OnCreateHyperlink", metaDataDocMgt.XmlNamespaceMgt);
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Triggers/a:OnHyperlink", metaDataDocMgt.XmlNamespaceMgt);

      ///* ControlTriggers */
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:Triggers/a:OnActivate", metaDataDocMgt.XmlNamespaceMgt);
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:Triggers/a:OnDeactivate", metaDataDocMgt.XmlNamespaceMgt);
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:Triggers/a:OnFormat", metaDataDocMgt.XmlNamespaceMgt);
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:Triggers/a:OnBeforeInput", metaDataDocMgt.XmlNamespaceMgt);
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:Triggers/a:OnInputChange", metaDataDocMgt.XmlNamespaceMgt);
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:Triggers/a:OnAfterInput", metaDataDocMgt.XmlNamespaceMgt);
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:Triggers/a:OnAfterValidate", metaDataDocMgt.XmlNamespaceMgt);
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:Triggers/a:OnFindRecord", metaDataDocMgt.XmlNamespaceMgt);
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:Triggers/a:OnNextRecord", metaDataDocMgt.XmlNamespaceMgt);
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:Triggers/a:OnAfterGetRecord", metaDataDocMgt.XmlNamespaceMgt);
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:Triggers/a:OnAfterGetCurrRecord", metaDataDocMgt.XmlNamespaceMgt);
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:Triggers/a:OnBeforePutRecord", metaDataDocMgt.XmlNamespaceMgt);
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:Triggers/a:OnNewRecord", metaDataDocMgt.XmlNamespaceMgt);
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:Triggers/a:OnInsertRecord", metaDataDocMgt.XmlNamespaceMgt);
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:Triggers/a:OnModifyRecord", metaDataDocMgt.XmlNamespaceMgt);
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:Triggers/a:OnDeleteRecord", metaDataDocMgt.XmlNamespaceMgt);
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:Triggers/a:OnPush", metaDataDocMgt.XmlNamespaceMgt);

      ///* ActionTriggers */
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:Triggers/a:OnActivate", metaDataDocMgt.XmlNamespaceMgt);
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:Triggers/a:OnDeactivate", metaDataDocMgt.XmlNamespaceMgt);
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:Triggers/a:OnFormat", metaDataDocMgt.XmlNamespaceMgt);
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:Triggers/a:OnBeforeInput", metaDataDocMgt.XmlNamespaceMgt);
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:Triggers/a:OnInputChange", metaDataDocMgt.XmlNamespaceMgt);
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:Triggers/a:OnAfterInput", metaDataDocMgt.XmlNamespaceMgt);
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:Triggers/a:OnAfterValidate", metaDataDocMgt.XmlNamespaceMgt);
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:Triggers/a:OnFindRecord", metaDataDocMgt.XmlNamespaceMgt);
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:Triggers/a:OnNextRecord", metaDataDocMgt.XmlNamespaceMgt);
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:Triggers/a:OnAfterGetRecord", metaDataDocMgt.XmlNamespaceMgt);
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:Triggers/a:OnAfterGetCurrRecord", metaDataDocMgt.XmlNamespaceMgt);
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:Triggers/a:OnBeforePutRecord", metaDataDocMgt.XmlNamespaceMgt);
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:Triggers/a:OnNewRecord", metaDataDocMgt.XmlNamespaceMgt);
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:Triggers/a:OnInsertRecord", metaDataDocMgt.XmlNamespaceMgt);
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:Triggers/a:OnModifyRecord", metaDataDocMgt.XmlNamespaceMgt);
      //XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Actions//a:Triggers/a:OnDeleteRecord", metaDataDocMgt.XmlNamespaceMgt);

      /* Controls with control parent. */
      /* TODO Needs logging */
      XmlUtility.DeleteElements(metaDataDocMgt.XmlCurrentFormNode, "./a:Controls//a:Control/a:Control", metaDataDocMgt.XmlNamespaceMgt);
   
      /*Temporary until codewash */
      //TempCodeWash(metaDataDocMgt.XmlCurrentFormNode.SelectNodes("./a:Properties/a:DataCaptionExpr", metaDataDocMgt.XmlNamespaceMgt), "'Removed'", false);
      //if (pageId != 0)
      //{
      //  TempCodeWash(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(".//a:OnOpenForm", metaDataDocMgt.XmlNamespaceMgt), "BEGIN\r\nif true then;\r\nEND;\r\n", true);
      //}
      //TempCodeWash(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(".//a:OnFindRecord", metaDataDocMgt.XmlNamespaceMgt), "BEGIN\r\nif true then;\r\nEND;\r\n", true);
      //TempCodeWash(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(".//a:OnAfterGetRecord", metaDataDocMgt.XmlNamespaceMgt), "BEGIN\r\nif true then;\r\nEND;\r\n", true);
      //TempCodeWash(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(".//a:OnNewRecord", metaDataDocMgt.XmlNamespaceMgt), "BEGIN\r\nif true then;\r\nEND;\r\n", true);
      //TempCodeWash(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(".//a:OnDeleteRecord", metaDataDocMgt.XmlNamespaceMgt), "BEGIN\r\nif true then;\r\nEND;\r\n", true);
      //TempCodeWash(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(".//a:OnInsertRecord", metaDataDocMgt.XmlNamespaceMgt), "BEGIN\r\nif true then;\r\nEND;\r\n", true);
      //TempCodeWash(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(".//a:OnAssistEdit", metaDataDocMgt.XmlNamespaceMgt), "BEGIN\r\nif true then;\r\nEND;\r\n", true);
      //TempCodeWash(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(".//a:OnAction", metaDataDocMgt.XmlNamespaceMgt), "BEGIN\r\nif true then;\r\nEND;\r\n", true);
      //TempCodeWash(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(".//a:OnDrillDown", metaDataDocMgt.XmlNamespaceMgt), "BEGIN\r\nif true then;\r\nEND;\r\n", true);
      //TempCodeWash(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(".//a:OnValidate", metaDataDocMgt.XmlNamespaceMgt), "BEGIN\r\nif true then;\r\nEND;\r\n", true);
      //TempCodeWash(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(".//a:OnLookup", metaDataDocMgt.XmlNamespaceMgt), "BEGIN\r\nif true then;\r\nEND;\r\n", true);
      //TempCodeWash(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(".//a:OnNextRecord", metaDataDocMgt.XmlNamespaceMgt), "BEGIN\r\nif true then;\r\nEND;\r\n", true);
      //TempCodeWash(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(".//a:OnCloseForm", metaDataDocMgt.XmlNamespaceMgt), "BEGIN\r\nif true then;\r\nEND;\r\n", true);
      //TempCodeWash(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(".//a:OnModifyRecord", metaDataDocMgt.XmlNamespaceMgt), "BEGIN\r\nif true then;\r\nEND;\r\n", true);

      //TempCodeWash(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(".//a:Code", metaDataDocMgt.XmlNamespaceMgt), "BEGIN\r\nif true then;\r\nEND.\r\n", true);
    }

    //private static void TempCodeWash(XmlNodeList nodeList, String code, Boolean isCode )
    //{
    //  MetaDataDocumentManagement metaDataDocMgt = MetaDataDocumentManagement.Instance;
   
    //  foreach (XmlNode node in nodeList)
    //  {
    //    if (isCode)
    //    {
    //      node.RemoveAll();
    //      XmlCDataSection data = metaDataDocMgt.XmlDocument.CreateCDataSection(code);
    //      node.AppendChild(data);
    //    }
    //    else
    //      node.InnerXml = code;
    //    TransformationLog.GenericLogEntry(metaDataDocMgt.GetCurrentPageID + " : " + XmlUtility.GetNodePath(node) + " node is deleted.", LogCategory.TempCodeWash);
    //  }
    //}

    private static void SetEmphasis()
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      XmlNodeList propertyNodes;

      propertyNodes = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Properties [./a:FontBold='Yes' and ./a:ForeColor='255']", metaDataDocMgt.XmlNamespaceMgt);
      AddStyleNode(propertyNodes, "Unfavorable");

      propertyNodes = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Properties [./a:FontBold='Yes' and ./a:ForeColor='65280']", metaDataDocMgt.XmlNamespaceMgt);
      AddStyleNode(propertyNodes, "Favorable");

      propertyNodes = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Properties [./a:FontBold='Yes']", metaDataDocMgt.XmlNamespaceMgt);
      AddStyleNode(propertyNodes, "Strong");

      propertyNodes = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Properties [./a:ForeColor='255']", metaDataDocMgt.XmlNamespaceMgt);
      AddStyleNode(propertyNodes, "Attention");
    }

    /// <summary>
    /// Should add Style and StyleExpr properties and then remove FontBold and ForeColor
    /// </summary>
    private static void AddStyleNode(XmlNodeList propertyNodes, string style)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      foreach (XmlNode node in propertyNodes)
      {
        XmlNode styleNode = XmlUtility.CreateXmlElement("Style", style);
        XmlNode styleExpNode = XmlUtility.CreateXmlElement("StyleExpr", "TRUE");
        node.AppendChild(styleNode);
        node.AppendChild(styleExpNode);
        XmlUtility.DeleteElements(node, @"./a:FontBold", metaDataDocMgt.XmlNamespaceMgt);
        XmlUtility.DeleteElements(node, @"./a:ForeColor", metaDataDocMgt.XmlNamespaceMgt);
      }
    }

    private static void UpdateNewIdNodes()
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      XmlNodeList nodesToBeDeleted = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:NewID", metaDataDocMgt.XmlNamespaceMgt);
      for (Int32 i = nodesToBeDeleted.Count - 1; i >= 0; i--)
      {
        XmlUtility.DeleteElements(nodesToBeDeleted[i].ParentNode, ".//a:ID", metaDataDocMgt.XmlNamespaceMgt);        
      }

      XmlUtility.RenameNode(nodesToBeDeleted, "ID");
    }
  }
}
