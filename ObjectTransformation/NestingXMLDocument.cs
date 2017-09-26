//--------------------------------------------------------------------------
// <copyright file="NestingXMLDocument.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>?abc?</summary>
//--------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.IO;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.Tools.FormTransformation
{
  /// <summary>
  /// ?abc?
  /// </summary>
  enum ControlType1
  {
    none,
    Label,
    TextBox,
    CheckBox,
    OptionButton,
    CommandButton,
    MenuButton,
    Frame,
    Image,
    PictureBox,
    Shape,
    Indicator,
    TabControl,
    Subform,
    TableBox,
    MatrixBox
  }

  /// <summary>
  /// ?abc?
  /// </summary>
  public class NestingXmlDocument
  {
    private static Dictionary<string, ControlType1> actionsSequence = new Dictionary<string, ControlType1>();
    private static Dictionary<string, ControlType1> controlsSequence = new Dictionary<string, ControlType1>();
    private static bool errorInTransformation;
    private UserSetupManagement userMgt = UserSetupManagement.Instance;
    private MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
    private bool suppressErrors;

    /// <summary>
    /// ?abc?
    /// </summary>
    public NestingXmlDocument()
    {
      SuppressErrors = false;
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    public static bool ErrorInTransformation
    {
      get
      {
        return NestingXmlDocument.errorInTransformation;
      }

      internal set
      {
        NestingXmlDocument.errorInTransformation = value;
      }
    }

    /// <summary>
    /// If SuppressErrors==TRUE, transformation will transform all forms, even if they have some errors.
    /// You could check ErrorInTransformation after finishing transformation.
    /// Otherwise it will transform forms till first error.
    /// </summary>
    public bool SuppressErrors
    {
      internal get
      {
        return suppressErrors;
      }

      set
      {
        suppressErrors = value;
      }
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    public void StartTransformation()
    {
      ErrorInTransformation = false;
      
      ValidateInputSchemas();
      
      PreRemoveNonAffectedNodes();
      PreRenameNodes();
      PreReplaceSubstring();
      
      RunCodeTransfromation();
      RunTransformation(); 

      PostProcessControlTypeNode();
      
      // @TODO Why don't we validate output?
      ValidateInputSchemas();
    }

    private void ValidateInputSchemas()
    {
      bool errorExists = false;
      if ((!errorExists) && (!TestSchema(metaDataDocMgt.XmlDocument, userMgt.ApplicationObjectsSchema)))
        errorExists = true;

      if ((!errorExists) && (!TestSchema(metaDataDocMgt.InsertElementsDoc, userMgt.TransformationInputSchema)))
        errorExists = true;

      if ((!errorExists) && (!TestSchema(metaDataDocMgt.IgnorePagesDoc, userMgt.IgnorePageSchema)))
        errorExists = true;

      if ((!errorExists) && (!TestSchema(metaDataDocMgt.RenumberPagesDoc, userMgt.MovePageSchema)))
        errorExists = true;

      if ((!errorExists) && (!TestSchema(metaDataDocMgt.DeleteElementsDoc, userMgt.DeletePageElementSchema)))
        errorExists = true;

      if ((!errorExists) && (!TestSchema(metaDataDocMgt.MoveElementsDoc, userMgt.MovePageElementSchema)))
        errorExists = true;

      if (errorExists)
      {
        throw new TransformationException(Resources.CantValidateSchema);
      }
    }

    // TODO ValidateOutputScheames commented out because of a FxCop violation (method isn't called)
    // private void ValidateOutputScheames()
    // {
    //   TestSchema(metaDataDocMgt.XmlDocument, userMgt.ApplicationObjectsSchema);
    // }

    #region Pretransformation methods
    private void PreRemoveNonAffectedNodes()
    {
      XmlNodeList pageNodeList = metaDataDocMgt.XmlDocument.SelectNodes(@"./a:Objects/a:Page", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.DeleteNodeList(pageNodeList);
    }

    private void PreRenameNodes()
    {
      XmlNodeList formNodeList = metaDataDocMgt.XmlDocument.SelectNodes(@"./a:Objects/a:Form", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.RenameNode(formNodeList, "Page");

      XmlNodeList subFormIDNodeList = metaDataDocMgt.XmlDocument.SelectNodes(@"./a:Objects/a:Page//a:SubFormID", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.RenameNode(subFormIDNodeList, "PagePartID");

      XmlNodeList timerIntervalNodeList = metaDataDocMgt.XmlDocument.SelectNodes(@"./a:Objects/a:Page/a:Properties/a:TimerInterval", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.RenameNode(timerIntervalNodeList, "TimerUpdate");

      XmlNodeList updateOnActivateNodeList = metaDataDocMgt.XmlDocument.SelectNodes(@"./a:Objects/a:Page/a:Properties/a:UpdateOnActivate", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.RenameNode(updateOnActivateNodeList, "RefreshOnActivate");

      XmlNodeList passwordTextList = metaDataDocMgt.XmlDocument.SelectNodes(@"./a:Objects//a:Control/a:Properties/a:PasswordText", metaDataDocMgt.XmlNamespaceMgt);
      foreach (XmlNode passwordText in passwordTextList)
      {
        XmlNode page = passwordText.ParentNode.ParentNode.ParentNode.ParentNode;  // Node parent to Controls node
        int pageId = (int) LogEntryObjectId.NotSpecified;

        while (page != null)
        {
          if (page.Name == "Page")
          {
            pageId = Convert.ToInt32(page.Attributes["ID"].Value, CultureInfo.InvariantCulture);
            page = null;
          }
          else
          {
            page = page.ParentNode;
          }
        }

        TransformationLog.GenericLogEntry(Resources.PasswordText, LogCategory.Warning, pageId, null);            

        passwordText.ParentNode.AppendChild(XmlUtility.CreateXmlElement("ExtendedDataType", "Masked"));
        passwordText.ParentNode.RemoveChild(passwordText);
      }
    }

    private void PreReplaceSubstring()
    {
      XmlNodeList runObjectList = metaDataDocMgt.XmlDocument.SelectNodes(@".//a:RunObject", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.ReplaceSubstringInElement(runObjectList, "Form", "Page");

      XmlNodeList partIDList = metaDataDocMgt.XmlDocument.SelectNodes(@".//a:PagePartID", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.ReplaceSubstringInElement(partIDList, "Form", "Page");
    }
    #endregion

    private void PostProcessControlTypeNode()
    {
      XmlNodeList partNodeList = metaDataDocMgt.XmlDocument.SelectNodes(@".//a:Control[.//a:Controltype='Part']", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.RenameNode(partNodeList, "Part");

      XmlNodeList fieldNodeList = metaDataDocMgt.XmlDocument.SelectNodes(@".//a:Control[.//a:Controltype='Field']", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.RenameNode(fieldNodeList, "Field");

      XmlNodeList progressNodeList = metaDataDocMgt.XmlDocument.SelectNodes(@".//a:Control[.//a:Controltype='ProgressControl']", metaDataDocMgt.XmlNamespaceMgt);
      foreach (XmlNode node in progressNodeList)
      {
        node.FirstChild.AppendChild(XmlUtility.CreateXmlElement("ExtendedDataType", "Ratio"));
      }
      XmlUtility.RenameNode(progressNodeList, "Field");

      XmlNodeList radioNodeList = metaDataDocMgt.XmlDocument.SelectNodes(@".//a:Control[.//a:Controltype='RadioButton']", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.RenameNode(radioNodeList, "Field");

      // Delete Controltype propety
      XmlUtility.DeleteElements(metaDataDocMgt.XmlDocument, ".//a:Controls//a:Controltype", metaDataDocMgt.XmlNamespaceMgt);

      // Rename OnForm Triggers to OnPage Triggers
      XmlNodeList onFormOpenNodeList = metaDataDocMgt.XmlDocument.SelectNodes(@".//a:Triggers//a:OnOpenForm", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.RenameNode(onFormOpenNodeList, "OnOpenPage");
      XmlNodeList onFormCloseNodeList = metaDataDocMgt.XmlDocument.SelectNodes(@".//a:Triggers//a:OnCloseForm", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.RenameNode(onFormCloseNodeList, "OnClosePage");
      XmlNodeList onFormQuCloseNodeList = metaDataDocMgt.XmlDocument.SelectNodes(@".//a:Triggers//a:OnQueryCloseForm", metaDataDocMgt.XmlNamespaceMgt);
      XmlUtility.RenameNode(onFormQuCloseNodeList, "OnQueryClosePage");
    }

    #region  Schema Test
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "WriteAbort method writes all types of exceptions to log")]
    private bool TestSchema(XmlDocument doc, String schema)
    {
      string fileName = "tmp.xml";
      try
      {
        if (doc != null)
        {
          Console.Write("-> Validate " + schema);
          XmlReaderSettings pageSchemaSetting = new XmlReaderSettings();
          pageSchemaSetting.Schemas.Add("urn:schemas-microsoft-com:dynamics:NAV:ApplicationObjects", userMgt.SchemasPathLocation + schema);
          pageSchemaSetting.ValidationType = ValidationType.Schema;
          pageSchemaSetting.ValidationEventHandler += new ValidationEventHandler(ValidationCallBack);

          //XmlReader rdr = XmlReader.Create(new XmlNodeReader(doc), pageSchemaSetting);
          //doc.Save(FileName);

          FileInfo fi = new FileInfo(schema);
          fileName = fi.Name + "." + fileName;
          doc.Save(fileName);
          XmlReader rdr = XmlReader.Create(fileName, pageSchemaSetting);

          StringBuilder sb = new StringBuilder();
          while (rdr.Read())
          {
            sb.Append(rdr.Value);
          }

          rdr.Close();
          Console.WriteLine("...Done. ");

          try
          {
            System.IO.File.Delete(fileName);
          }
          catch (IOException ex)
          {
            string logStr = string.Format(CultureInfo.InvariantCulture, Resources.CanNotDeleteTmpFile, ex.Message);
            TransformationLog.GenericLogEntry(logStr, LogCategory.Warning, (int)LogEntryObjectId.NotSpecified, null);            
          }
        }

        return true;
      }
      catch (Exception e)
      {
        Console.WriteLine("...FAILED. ");
        Console.WriteLine(e.Message);
        TransformationLog.GenericLogEntry("Schema: " + schema, LogCategory.Error);
        TransformationLog.WriteErrorToLogFile(e, (int)LogEntryObjectId.Error);
        //TransformationLog.GenericLogEntry(e.Message, LogCategory.Error);

        //try
        //{
        //  FileInfo fi = new FileInfo(schema);
        //  File.Move(FileName, fi.Name + "." + FileName);
        //}
        //finally
        //{
        //  TransformationLog.GenericLogEntry(FileName, LogCategory.IgnoreWarning);
        //}

        return false;
      }
    }

    private static void ValidationCallBack(object sender, ValidationEventArgs e)
    {
      XmlReader reader = sender as XmlReader;
      String baseUri = "";
      if (reader != null)
      {
        baseUri = reader.BaseURI;
      }

      throw (new TransformationException(String.Format(CultureInfo.InvariantCulture, Resources.ValidationFailed, baseUri, e.Exception.LineNumber, e.Exception.LinePosition), e.Exception));
    }
    #endregion

    private void RunTransformation()
    {
      XmlNodeList nodeList = metaDataDocMgt.XmlDocument.SelectNodes(@"./a:Objects/a:Page", metaDataDocMgt.XmlNamespaceMgt);
      Decimal numberOfPages = nodeList.Count;
      Decimal pageCounter = 0;
      Console.WriteLine("Control and Property Transformation started.");
      const string Indentation = "\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b   ";

      Stack postponedPages = PostponedPagesList(nodeList);

      foreach (XmlNode formNode in nodeList)
      {
        metaDataDocMgt.XmlCurrentFormNode = formNode;

        if (!postponedPages.Contains(metaDataDocMgt.GetCurrentPageId))
        {
          string currentPage = metaDataDocMgt.GetCurrentPageId.ToString(CultureInfo.InvariantCulture);
          string s = Indentation +
            Convert.ToInt32(((pageCounter + 1) / numberOfPages) * 100) +
            "%  #" +
            currentPage;

          Console.Write(s);
          pageCounter++;

          try
          {
            TransformPage();
          }
          catch (TransformationException ex)
          {
            if (SuppressErrors)
            {
              TransformationLog.WriteErrorToLogFile(ex, metaDataDocMgt.GetCurrentPageId);
            }
            else
            {
              throw new TransformationException(string.Format(CultureInfo.InvariantCulture, Resources.RunTransformationError, currentPage), ex);
            }
          }
        }
      }

      if (postponedPages.Count > 0)
      {
        Console.WriteLine();
        Console.WriteLine("   Subforms tranformation started.");
      }

      int ilen = 0;
      foreach (int i in postponedPages)
      {
        XmlNode formNode = metaDataDocMgt.XmlDocument.SelectSingleNode(
          "./a:Objects/a:Page[@ID='" + i.ToString(CultureInfo.InvariantCulture) + "']",
          metaDataDocMgt.XmlNamespaceMgt);
        if (formNode != null)
        {
          metaDataDocMgt.XmlCurrentFormNode = formNode;

          if (postponedPages.Contains(metaDataDocMgt.GetCurrentPageId))
          {
            StringBuilder tmp = new StringBuilder();
            if (ilen > 0)
            {
              int length = ilen - i.ToString(CultureInfo.InvariantCulture).Length;
              if (length < 0)
              {
                length = 0;
              }

              tmp.Insert(0, " ", length);
            }

            string currentPage = i.ToString(CultureInfo.InvariantCulture);
            string s = Indentation +
              Convert.ToInt32(((pageCounter + 1) / numberOfPages) * 100) +
              "%  #" +
              currentPage + tmp.ToString();
            Console.Write(s);
            ilen = i.ToString(CultureInfo.InvariantCulture).Length;
            pageCounter++;

            try
            {
              TransformPage();
            }
            catch (TransformationException ex)
            {
              TransformationLog.WriteErrorToLogFile(ex, metaDataDocMgt.GetCurrentPageId);
            }
          }
        }
      }

      Console.WriteLine(String.Empty);
    }

    [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    private static void TransformPage()
    {
      try
      {
        ControlParentChildNesting();
        DeleteElements.Start();
      }
      catch (Exception e)
      {
        throw new TransformationException(Resources.StepNesting, e);
      }

      try
      {
        CodeTransformationRules.PerformMoveToTriggerActions(true);
        CodeTransformationRules.PerformMoveToPropertyActions();
      }
      catch (Exception e)
      {
        throw new TransformationException(Resources.StepCleanProperties, e);
      }

      try
      {
        PageProperties.AddDefaultProperties();
      }
      catch (Exception e)
      {
        throw new TransformationException(Resources.StepAddDefaultProperties, e);
      }

      try
      {
        XmlUtility.AlignControls();
      }
      catch (Exception e)
      {
        throw new TransformationException(Resources.StepAligningControls, e);
      }

      try
      {
        GetSortedControls();
      }
      catch (Exception e)
      {
        throw new TransformationException(Resources.StepGetSortedControls, e);
      }

      try
      {
        SourceObject.CreateSourceObject();
      }
      catch (Exception e)
      {
        throw new TransformationException(Resources.StepCreateSourceObject, e);
      }

      try
      {
        PageActions.RemoveNotSupportedActions();
      }
      catch (Exception e)
      {
        throw new TransformationException(Resources.StepRemoveUnsupportedActions, e);
      }

      try
      {
        PageActions.MoveMenuButtonsToActionPage();
      }
      catch (Exception e)
      {
        throw new TransformationException(Resources.StepMoveMenuButtons, e);
      }

      try
      {
        PageActions.MoveCommandButtonsToAction();
      }
      catch (Exception e)
      {
        throw new TransformationException(Resources.StepMoveCommandButtons, e);
      }
      
      try
      {
        PageControls.LogExtraInformation();
      }
      catch (Exception e)
      {
        throw new TransformationException(Resources.StepLogExtraInformation, e);
      }

      try
      {
        PageControls.ManageOptionButtons();
      }
      catch (Exception e)
      {
        throw new TransformationException(Resources.StepManageOptionButtons, e);
      }

      try
      {
        PageControls.ManageFixedLayout();
      }
      catch (Exception e)
      {
        throw new TransformationException(Resources.StepManageFixedLayout, e);
      }

      try
      {
        PageControls.ManageLabels();
      }
      catch (Exception e)
      {
        throw new TransformationException(Resources.StepManageLabels, e);
      }

      try
      {
        PageControls.SearchCaptionsInTextBoxes();
      }
      catch (Exception e)
      {
        throw new TransformationException(Resources.StepManageLabels, e);
      }

      try
      {
        PageControls.DeleteLabels();
      }
      catch (Exception e)
      {
        throw new TransformationException(Resources.StepDeleteLabels, e);
      }

      try
      {
        PageControls.TransformTabControlsToBands();
        PageControls.TransformFrameToGroup();
        PageControls.TransformTableBoxToRepeater();
      }
      catch (Exception e)
      {
        throw new TransformationException(Resources.StepCreateBands, e);
      }

      try
      {
        PageControls.MoveUntouchedControlsToContentArea();
      }
      catch (Exception e)
      {
        throw new TransformationException(Resources.StepMoveUntouchedControls, e);
      }

      try
      {
        PageControls.ManageControlType();
      }
      catch (Exception e)
      {
        throw new TransformationException(Resources.StepManageControlType, e);
      }

      try
      {
        PageControls.MoveElementsFromPropertiesToTriggerNode();
      }
      catch (Exception e)
      {
        throw new TransformationException(Resources.StepMoveElementsFromPropertiesToTriggerNode, e);
      }

      try
      {
        PageControls.CleanTriggerNode();
      }
      catch (Exception e)
      {
        throw new TransformationException(Resources.StepCleanTriggerNode, e);
      }

      try
      {
        MergeInput.StartMerging();  /* addelements */
        CleaningUp.CleanProperties();
      }
      catch (Exception e)
      {
        throw new TransformationException(Resources.StepCleanProperties, e);
      }

      try
      {
        PageActions.ActionsFinalProcessing();
      }
      catch (Exception e)
      {
        throw new TransformationException(Resources.StepActionsFinalProcessing, e);
      }

      try
      {
        ReSortActions();
        ReSortControls();
      }
      catch (Exception e)
      {
        throw new TransformationException(Resources.StepAddSortedControls, e);
      }

      try
      {
        PageControls.AlignConfirmationDialogs();
      }
      catch (Exception e)
      {
        throw new TransformationException(Resources.StepAlignConfirmationDialogs, e);
      }

      try
      {
        PageControls.FinalizeControlsProcessing();
      }
      catch (Exception e)
      {
        throw new TransformationException(Resources.StepFinalizeControlsProcessing, e);
      }
    }

    private Stack PostponedPagesList(XmlNodeList nodeList)
    {
      Stack pages = new Stack();
      foreach (XmlNode formNode in nodeList)
      {
        XmlNodeList subFormList = formNode.SelectNodes(@".//a:Control[./a:Properties/a:Controltype='SubForm']", metaDataDocMgt.XmlNamespaceMgt);
        foreach (XmlNode subForm in subFormList)
        {
          if (subForm != null)
          {
            XmlNodeList menuButtonList = formNode.SelectNodes(@"./a:Controls/a:Control[./a:Properties/a:Controltype='MenuButton']", metaDataDocMgt.XmlNamespaceMgt);
            for (int i = 0; i < menuButtonList.Count; i++)
            {
              string pagePartId = GetProperty(subForm, "PagePartID");
              if (!string.IsNullOrEmpty(pagePartId))
              {
                //string pageID = subForm.SelectSingleNode(@".//a:PagePartID", metaDataDocMgt.XmlNamespaceMgt).InnerText;
                pagePartId = pagePartId.Replace("Page", string.Empty);
                int id = Convert.ToInt32(pagePartId, CultureInfo.InvariantCulture);
                if (!pages.Contains(id))
                {
                  pages.Push(id);
                }
              }
              continue;
            }
          }
        }
      }

      return pages;
    }

    private void RunCodeTransfromation()
    {
      CodeTransformationRules.SetRules();

      // select forms nodes
      XmlNodeList nodeList = metaDataDocMgt.XmlDocument.SelectNodes(@"./a:Objects/a:Page", metaDataDocMgt.XmlNamespaceMgt);
      Double numberOfPages = nodeList.Count;
      Double pageCounter = 0;
      Console.WriteLine("Code Transformation started.");
      const String Indentation = "\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b   ";

      // for each form

      foreach (XmlNode formNode in nodeList)
      {
        metaDataDocMgt.XmlCurrentFormNode = formNode;
        string s = Indentation +
          Convert.ToInt32(((pageCounter + 1) / numberOfPages) * 100) +
          "%  #" +
          metaDataDocMgt.GetCurrentPageId.ToString(CultureInfo.InvariantCulture);
        Console.Write(s);

        pageCounter++;

        // select all trigger node[unsupported]

        try
        {
          CodeTransformationRules.TransformFormTriggers(formNode);
        }
        catch (Exception e)
        {
          if (SuppressErrors)
          {
            TransformationLog.GenericLogEntry(e.Message, LogCategory.Error, metaDataDocMgt.GetCurrentPageId);
          }
          else
          {
            throw new TransformationException(Resources.StepCodeTransformation + metaDataDocMgt.GetCurrentPageId, e);
          }
        }
      }

      Console.WriteLine("");
      CodeTransformationRules.SummarizeActions();
    }

    // private static void DeclareVariables(XmlNodeList triggerNodeList)
    // {
    //   //  for each trigger node
    //   foreach (XmlNode triggerNode in triggerNodeList)
    //   {
    //     try
    //     {
    //       CodeTransformationRules.DeclareVariables(triggerNode);
    //     }
    //     catch (Exception e)
    //     {
    //       throw new CodeTransformationException(Resources.StepCodeTransformation, e);
    //     }
    //   }
    // }

    private static void AppendChild(XmlNode parentNode, Int32 parentID)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      XmlNodeList idList = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:ID", metaDataDocMgt.XmlNamespaceMgt);
      for (Int32 j = 0; j < idList.Count; j++)
      {
        XmlNode idNode = idList[j];
        Int32 nodeID = Convert.ToInt32(idNode.InnerText, CultureInfo.InvariantCulture);

        if (parentID == nodeID)
        {
          XmlNode nodeParentProperties = idNode.ParentNode.SelectSingleNode(@"./a:Controltype", metaDataDocMgt.XmlNamespaceMgt);
          if (nodeParentProperties != null)
          {
            if (nodeParentProperties.InnerText == "Label")
            {
              XmlNode tmpNode = idNode.ParentNode.SelectSingleNode(@"./a:ParentControl", metaDataDocMgt.XmlNamespaceMgt);
              if (tmpNode != null)
              {
                Int32 labelParentID = Convert.ToInt32(tmpNode.InnerText, CultureInfo.InvariantCulture);
                AppendChild(parentNode, labelParentID);
                break;
              }
              else
              {
                tmpNode = metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode("./a:Controls", metaDataDocMgt.XmlNamespaceMgt);
                if (tmpNode != null)
                {
                  tmpNode.AppendChild(parentNode.ParentNode.ParentNode);
                }

                break;
              }
            }
          }

          idNode.ParentNode.ParentNode.AppendChild(parentNode.ParentNode.ParentNode);
          break;
        }
      }
    }

    private static void ControlParentChildNesting()
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      XmlNodeList parentControlList = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:ParentControl", metaDataDocMgt.XmlNamespaceMgt);

      for (Int32 i = 0; i < parentControlList.Count; i++)
      {
        XmlNode parentNode = parentControlList[i];
        Int32 parentID = Convert.ToInt32(parentNode.InnerText, CultureInfo.InvariantCulture);
        AppendChild(parentNode, parentID);
      }
    }

    private static void GetSortedControls()
    {
      controlsSequence.Clear();
      actionsSequence.Clear();
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      XmlNodeList controlList = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@"./a:Controls/a:Control", metaDataDocMgt.XmlNamespaceMgt);

      foreach (XmlNode controlNode in controlList)
      {
        string controlID = controlNode.SelectSingleNode(@"./a:Properties/a:ID", metaDataDocMgt.XmlNamespaceMgt).InnerText;

        switch (controlNode.SelectSingleNode(@"./a:Properties/a:Controltype", metaDataDocMgt.XmlNamespaceMgt).InnerText)
        {
          case "Label":
            controlsSequence.Add(controlID, ControlType1.Label);
            break;
          case "TextBox":
            controlsSequence.Add(controlID, ControlType1.TextBox);
            break;
          case "CheckBox":
            controlsSequence.Add(controlID, ControlType1.CheckBox);
            break;
          case "OptionButton":
            controlsSequence.Add(controlID, ControlType1.OptionButton);
            break;
          case "CommandButton":
            actionsSequence.Add(controlID, ControlType1.CommandButton);
            break;
          case "MenuButton":
            actionsSequence.Add(controlID, ControlType1.MenuButton);
            break;
          case "Frame":
            controlsSequence.Add(controlID, ControlType1.Frame);
            break;
          case "Image":
            controlsSequence.Add(controlID, ControlType1.Image);
            break;
          case "PictureBox":
            controlsSequence.Add(controlID, ControlType1.PictureBox);
            break;
          case "Shape":
            controlsSequence.Add(controlID, ControlType1.Shape);
            break;
          case "Indicator":
            controlsSequence.Add(controlID, ControlType1.Indicator);
            break;
          case "TabControl":
            controlsSequence.Add(controlID, ControlType1.TabControl);
            break;
          case "SubForm":
            controlsSequence.Add(controlID, ControlType1.Subform);
            break;
          case "TableBox":
            controlsSequence.Add(controlID, ControlType1.TableBox);
            break;
          case "MatrixBox":
            controlsSequence.Add(controlID, ControlType1.MatrixBox);
            break;
        }
      }
    }

    [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Requires refactoring, but time doesn’t permit refactoring at this point")]
    private static void ReSortControls()
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;

      XmlNode cntrlNode = metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(@"./a:Controls/a:ContentArea", metaDataDocMgt.XmlNamespaceMgt);
      XmlNode currCntrlContainer = null;
      if (cntrlNode != null)
      {
        XmlNode currNode = cntrlNode.SelectSingleNode(@"./a:Properties", metaDataDocMgt.XmlNamespaceMgt);
        foreach (KeyValuePair<string, ControlType1> control in controlsSequence)
        {
          XmlNode controlNode;
          switch (control.Value)
          {
            case ControlType1.OptionButton:
              controlNode = cntrlNode.SelectSingleNode(@"./a:Control[./a:Properties/a:ID='" + control.Key + "']", metaDataDocMgt.XmlNamespaceMgt);
              if (controlNode == null)
              {
                break;
              }

              String optionCaptionML = GetProperty(controlNode, "OptionCaptionML");
              if (!string.IsNullOrEmpty(optionCaptionML))
              {
                bool breakWork = false;
                ResortControlsForTrendscape(ref currCntrlContainer, cntrlNode, currNode, controlNode, ref breakWork, optionCaptionML);
                if (breakWork)
                {
                  break;
                }
              }

              currCntrlContainer = null;
              currNode = cntrlNode.InsertAfter(controlNode.ParentNode.RemoveChild(controlNode), currNode);
              break;
            case ControlType1.TextBox:
            case ControlType1.CheckBox:
              controlNode = cntrlNode.SelectSingleNode(@"./a:Control[./a:Properties/a:ID='" + control.Key + "']", metaDataDocMgt.XmlNamespaceMgt);

              // to avoid sorting problems with new groups.
              if (controlNode == null)
              {
                XmlNode groupControl = cntrlNode.SelectSingleNode(@"./a:Group[./a:Control/a:Properties/a:ID='" + control.Key + "']", metaDataDocMgt.XmlNamespaceMgt);
                if (groupControl == null)
                {
                  break;
                }

                if (currNode.Equals(groupControl))
                {
                  break;
                }

                XmlNode controlForCheck = cntrlNode.SelectSingleNode(@".//a:Control[./a:Properties/a:ID='" + control.Key + "']", metaDataDocMgt.XmlNamespaceMgt);
                if ((groupControl.ChildNodes.Count>2) && (!groupControl.ChildNodes[1].Equals(controlForCheck))) // so it's the first control in the group
                {
                  break;
                }

                controlNode = groupControl;
              }

              XmlNode removedTempNode = controlNode.ParentNode.RemoveChild(controlNode);
              currNode = cntrlNode.InsertAfter(removedTempNode, currNode);
              break;
            case ControlType1.Frame:
            case ControlType1.TabControl:
            case ControlType1.TableBox:
              controlNode = cntrlNode.SelectSingleNode(@"./a:Group[./a:Properties/a:ID='" + control.Key + "']", metaDataDocMgt.XmlNamespaceMgt);
              if (controlNode == null)
              {
                break;
              }

              currCntrlContainer = null;
              currNode = cntrlNode.InsertAfter(controlNode.ParentNode.RemoveChild(controlNode), currNode);
              break;
            case ControlType1.Subform:
              controlNode = cntrlNode.SelectSingleNode(@"./a:Control[./a:Properties/a:ID='" + control.Key + "']", metaDataDocMgt.XmlNamespaceMgt);
              if (controlNode == null)
              {
                break;
              }

              currCntrlContainer = null;
              currNode = cntrlNode.InsertAfter(controlNode.ParentNode.RemoveChild(controlNode), currNode);
              break;
            case ControlType1.Label:
              if (PageControls.IsObjectHasIndentation(metaDataDocMgt.GetCurrentPageId))
              {
                goto case ControlType1.TableBox;
              }

              break;

            default:
              break;
          }
        }       
      }
    }

    private static void ReSortActions()
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;

      XmlNode actionNode = metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(@"./a:Actions/a:ActionItems", metaDataDocMgt.XmlNamespaceMgt);
      if (actionNode != null)
      {
        XmlNode currNode = actionNode.SelectSingleNode(@"./a:Properties", metaDataDocMgt.XmlNamespaceMgt);
        foreach (KeyValuePair<string, ControlType1> control in actionsSequence)
        {
          switch (control.Value)
          {
            case ControlType1.CommandButton:
              XmlNode controlNode = actionNode.SelectSingleNode(@"./a:Action[./a:ID='" + control.Key + "']", metaDataDocMgt.XmlNamespaceMgt);
              if (controlNode != null)
              {
                currNode = actionNode.InsertAfter(controlNode.ParentNode.RemoveChild(controlNode), currNode);
              }

              break;
            case ControlType1.MenuButton:
              controlNode = actionNode.SelectSingleNode(@"./a:ActionGroup[./a:Properties/a:ID='" + control.Key + "']", metaDataDocMgt.XmlNamespaceMgt);
              if (controlNode != null)
              {
                currNode = actionNode.InsertAfter(controlNode.ParentNode.RemoveChild(controlNode), currNode);
              }

              break;
          }
        }
      }
    }

    private static void ResortControlsForTrendscape(
      ref XmlNode currCntrlContainer, 
      XmlNode cntrlNode, 
      XmlNode currNode, 
      XmlNode controlNode, 
      ref bool breakWork, 
      String optionCaptionML)
    {
      MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
      if (metaDataDocMgt.MoveElementsDoc == null)
      {
        return;
      }

      StringBuilder queryPromoted = new StringBuilder();
      queryPromoted.Append(@"./a:MovePageElements/a:Page[@ID='");
      queryPromoted.Append(((int)MovePageElements.NotFormsId.TrendscapeControls).ToString(CultureInfo.InvariantCulture));
      queryPromoted.Append("' and @OptionCaptionML ='");
      queryPromoted.Append(optionCaptionML.Replace("'", ""));
      queryPromoted.Append("']");

      XmlNode trend = metaDataDocMgt.MoveElementsDoc.SelectSingleNode(queryPromoted.ToString(), metaDataDocMgt.XmlNamespaceMgt);
      if (trend == null)
      {
        return;
      }

      if (GetProperty(controlNode, "CaptionML") == null)
      {
        XmlNode propNode = controlNode.SelectSingleNode(
          @"./a:Properties", metaDataDocMgt.XmlNamespaceMgt);

        string captionMlFromFile = GetAttribute(trend, "CaptionML");
        propNode.AppendChild(XmlUtility.CreateXmlElement("CaptionML", captionMlFromFile));

        propNode.AppendChild(XmlUtility.CreateXmlElement("TempProperty", "Trendscape"));

        XmlNode cntrlTypeNode = propNode.SelectSingleNode(
          @"./a:Controltype", metaDataDocMgt.XmlNamespaceMgt);
        if ((cntrlTypeNode != null) && (cntrlTypeNode.InnerText == "RadioButton"))
          cntrlTypeNode.InnerText = "Field";
      }

      if (currCntrlContainer == null)
      {
        Boolean done = false;
        XmlNode tempCurrNode = currNode;
        if (tempCurrNode != null)
        {
          while (!done)
          {
            if (((tempCurrNode.Name == "Control") && (GetProperty(tempCurrNode, "Controltype") == "Part")) ||
                ((tempCurrNode.Name == "Group") && (GetProperty(tempCurrNode, "GroupType") == "Repeater")))
            {
              if ((tempCurrNode.PreviousSibling != null) &&
                  (tempCurrNode.PreviousSibling.Name == "Group") &&
                  (GetProperty(tempCurrNode.PreviousSibling, "CaptionML").Contains("ENU=Options")))
              {
                currCntrlContainer = tempCurrNode.PreviousSibling;
                currCntrlContainer.AppendChild(controlNode.ParentNode.RemoveChild(controlNode));
              }
              else
              {
                tempCurrNode =
                  cntrlNode.InsertBefore(
                    PageControls.CreateBand(
                      /*metaDataDocMgt.GetNewId.ToString(CultureInfo.InvariantCulture)*/
                      metaDataDocMgt.CalcId(null, "Options", "ContentArea").ToString(CultureInfo.InvariantCulture), "ENU=Options"),
                      tempCurrNode);
                currCntrlContainer = tempCurrNode;
                currCntrlContainer.AppendChild(controlNode.ParentNode.RemoveChild(controlNode));
              }

              done = true;
            }

            if (tempCurrNode != null)
            {
              break;
            }
          }

          if (!done)
          {
            tempCurrNode = tempCurrNode.PreviousSibling;
            if (tempCurrNode != null)
            {
              if (tempCurrNode.Name == "Properties")
                breakWork = true;
            }
          }
        }

        if (done)
        {
          breakWork = true;
        }
      }
      else if ((currCntrlContainer.Name == "Group") &&
               (GetProperty(currCntrlContainer, "CaptionML").Contains("ENU=Options")))
      {
        currCntrlContainer.AppendChild(controlNode.ParentNode.RemoveChild(controlNode));
        breakWork = true;
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
    private static String GetAttribute(XmlNode controlNode, String attributeName)
    {
      XmlNode attributeNode = controlNode.Attributes.GetNamedItem(attributeName);
      if (attributeNode == null)
        return null;
      return attributeNode.Value;
    }
  }
}