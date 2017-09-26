//--------------------------------------------------------------------------
// <copyright file="UserSetupManagement.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>?abc?</summary>
//--------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Microsoft.Dynamics.Nav.Tools.FormTransformation
{
  /// <summary>
  /// ?abc?
  /// </summary>
  public class UserSetupManagement
  {
    #region Private member variables
    private static UserSetupManagement instance = new UserSetupManagement();

    private String formsFile = "";
    private String pagesFile = "";
    private String transformPages = "";
    private String ignorePages = "";
    private String movePages = "";
    private String deletePageElements = "";
    private String movePageElements = "";
    private String schemasPathLocation = "";
    private String codeRules = "";
    private String applicationObjectsSchema = "\\ApplicationObjects.xsd";
    private String commonSchema = "\\Common.xsd";
    private String deletePageElementSchema = "\\DeletePageElement.xsd";
    private String ignorePageSchema = "\\IgnorePage.xsd";
    private String movePageSchema = "\\MovePage.xsd";
    private String movePageElementSchema = "\\MovePageElement.xsd";
    private String pageSchema = "\\Page.xsd";
    private String reportSchema = "\\Report.xsd";
    private String transformationInputSchema = "\\TransformationInput.xsd";
    private String translationFile = "\\translation.txt";
    #endregion

    #region Constructors
    private UserSetupManagement()
    {
      if (String.IsNullOrEmpty(schemasPathLocation))
      {
        schemasPathLocation = Directory.GetParent(GetType().Assembly.Location).FullName;
      }
    }
    #endregion

    #region Properties

    /// <summary>
    /// ?abc?
    /// </summary>
    public static void ReleaseInstance()
    {
      instance = new UserSetupManagement();
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    public static UserSetupManagement Instance
    {
      get
      {
        return instance;
      }
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    public String MovePageElements
    {
      get
      {
        return movePageElements;
      }

      set
      {
        movePageElements = value;
      }
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    public String DeletePageElements
    {
      get
      {
        return deletePageElements;
      }

      set
      {
        deletePageElements = value;
      }
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    public String MovePages
    {
      get
      {
        return movePages;
      }

      set
      {
        movePages = value;
      }
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    public String FormsFile
    {
      get
      {
        return formsFile;
      }

      set
      {
        formsFile = value;
      }
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    public String IgnorePages
    {
      get
      {
        return ignorePages;
      }

      set
      {
        ignorePages = value;
      }
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    public String PagesFile
    {
      get
      {
        return pagesFile;
      }

      set
      {
        pagesFile = value;
      }
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    public String TransformPages
    {
      get
      {
        return transformPages;
      }

      set
      {
        transformPages = value;
      }
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    public String SchemasPathLocation
    {
      get
      {
        return schemasPathLocation;
      }

      set
      {
        schemasPathLocation = value;
      }
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    public String CodeRulesFile
    {
      get
      {
        return codeRules;
      }

      set
      {
        codeRules = value;
      }
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    public String ApplicationObjectsSchema
    {
      get
      {
        return applicationObjectsSchema;
      }
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    public String CommonSchema
    {
      get
      {
        return commonSchema;
      }
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    public String IgnorePageSchema
    {
      get
      {
        return ignorePageSchema;
      }
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    public String MovePageSchema
    {
      get
      {
        return movePageSchema;
      }
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    public String MovePageElementSchema
    {
      get
      {
        return movePageElementSchema;
      }
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    public String PageSchema
    {
      get
      {
        return pageSchema;
      }
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    public String ReportSchema
    {
      get
      {
        return reportSchema;
      }
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    public String TransformationInputSchema
    {
      get
      {
        return transformationInputSchema;
      }
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    public String DeletePageElementSchema
    {
      get
      {
        return deletePageElementSchema;
      }
    }

    /// <summary>
    /// Pages’ translation file (in NAV classic: Tools->Translate->Export) 
    /// </summary>
    public String TranslationFile
    {
      get
      {
        return translationFile;
      }

      set
      {
        translationFile = value;
      }
    }

    /// <summary>
    /// Will check if all schema files exists
    /// </summary>
    /// <returns>Will return True if everything is ok and False if some error exists. Error will be logged with LogCategory equal to Error</returns>
    public bool CheckIfAllSchemaFilesExists()
    {
      bool cantFindSchemaFile = false;

      CheckIfFileExists(applicationObjectsSchema, ref cantFindSchemaFile);

      CheckIfFileExists(commonSchema, ref cantFindSchemaFile);
      CheckIfFileExists(deletePageElementSchema, ref cantFindSchemaFile);
      CheckIfFileExists(ignorePageSchema, ref cantFindSchemaFile);
      CheckIfFileExists(movePageSchema, ref cantFindSchemaFile);
      CheckIfFileExists(movePageElementSchema, ref cantFindSchemaFile);
      CheckIfFileExists(pageSchema, ref cantFindSchemaFile);
      CheckIfFileExists(reportSchema, ref cantFindSchemaFile);
      CheckIfFileExists(transformationInputSchema, ref cantFindSchemaFile);

      return !cantFindSchemaFile;
    }
    #endregion

    #region private methods
    private static void CheckIfFileExists(String fileName, ref bool fileNotFound)
    {
      if (!File.Exists(fileName.Substring(1)))
      {
        fileNotFound = true;
        TransformationLog.GenericLogEntry(String.Format(System.Globalization.CultureInfo.InvariantCulture, Resources.FileLoadError, fileName), LogCategory.Error);
      }

      RemoveTempFile(fileName);
    }

    /// <summary>
    /// Will delete temporary file from previouse run
    /// </summary>
    /// <param name="schemaFileName">?abc?</param>
    private static void RemoveTempFile(string schemaFileName)
    {
      string fileName = schemaFileName.Substring(1) + ".tmp.xml";
      if (File.Exists(fileName))
      {
        File.Delete(fileName);
      }
    }
    #endregion
  }
}