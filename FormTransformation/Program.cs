//--------------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>?abc?</summary>
//--------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using Microsoft.Dynamics.Nav.Tools.FormTransformation;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.Transformation
{
  /// <summary>
  /// Form Transformation 
  /// </summary>
  internal sealed class Program
  {
    /// <summary>
    /// Added private constructor
    /// </summary>
    private Program()
    {
    }

    /// <summary>
    /// Main
    /// </summary>
    /// <param name="args">?abc?</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "WriteAbort method writes all types of exceptions to log")]
    public static int Main(string[] args)
    {
      DateTime st = DateTime.UtcNow;
      Console.WriteLine("Transformation Tool");

      UserSetupManagement userSetup = UserSetupManagement.Instance;
      MetadataDocumentManagement metadataDocMgt = MetadataDocumentManagement.Instance;

      if (!GetUserInput(args))
      {
        TransformationLog.CloseXmlFile();
        return 1;
      }

      if (!LoadFiles(metadataDocMgt, userSetup))
      {
        TransformationLog.CloseXmlFile();
        return 1;
      }

      ReportConverter reportConvertor = new ReportConverter(metadataDocMgt.XmlDocument);
      metadataDocMgt.XmlDocument = reportConvertor.ReturnSourceForms();
      TransformationLog.TransformationStarted();

      IgnoreForms.RemoveIgnoredForms();
      IgnoreForms.RemoveFormsWithMatrixControls();
      IgnoreForms.RemoveReplacedForms();

      try
      {
        NestingXmlDocument nestingXml = new NestingXmlDocument();
        nestingXml.SuppressErrors = true;
        nestingXml.StartTransformation();
        if (NestingXmlDocument.ErrorInTransformation)
        {
          TransformationLog.GenericLogEntry("Transformation tool can't transform this file. Check Transformation log file.", LogCategory.Error);
        }
      }
      catch (Exception e)
      {
        WriteAbortTransformation(e);
        TransformationLog.CloseXmlFile();
        return 1;
      }

      MovePageElements.Start();
      RenumberPages.Start();

      if (!XmlUtility.SaveXmlToFile(reportConvertor.GetDestinationPages(metadataDocMgt.XmlDocument), userSetup.PagesFile))
      {
        TransformationLog.CloseXmlFile();
        return 1;
      }

      Console.WriteLine("The Transformation is completed.");
      TransformationLog.TransformationFinished(System.DateTime.UtcNow.Subtract(st).TotalSeconds);
      TransformationLog.CloseXmlFile();
      return 0;
    }

        private static bool LoadFiles(MetadataDocumentManagement metaDataDocMgt, UserSetupManagement userSetup)
    {
      bool errorExists = false;

      errorExists = !userSetup.CheckIfAllSchemaFilesExists();

      try
      {
        metaDataDocMgt.XmlDocument = XmlUtility.LoadFromFileToXml(userSetup.FormsFile);
        System.IO.File.Delete(userSetup.PagesFile);
      }
      catch (Exception e)
      {
        errorExists = true;
        TransformationLog.GenericLogEntry(e.Message, LogCategory.Error);
      }

      try
      {
        metaDataDocMgt.DeleteElementsDoc = XmlUtility.LoadFromFileToXml(userSetup.DeletePageElements);
      }
      catch (Exception e)
      {
        TransformationLog.GenericLogEntry(e.Message, LogCategory.CheckInputFile);
      }

      try
      {
        metaDataDocMgt.IgnorePagesDoc = XmlUtility.LoadFromFileToXml(userSetup.IgnorePages);
      }
      catch (Exception e)
      {
        TransformationLog.GenericLogEntry(e.Message, LogCategory.CheckInputFile);
      }

      try
      {
        metaDataDocMgt.InsertElementsDoc = XmlUtility.LoadFromFileToXml(userSetup.TransformPages);
      }
      catch (Exception e)
      {
        TransformationLog.GenericLogEntry(e.Message, LogCategory.CheckInputFile);
      }

      try
      {
        metaDataDocMgt.MoveElementsDoc = XmlUtility.LoadFromFileToXml(userSetup.MovePageElements);
      }
      catch (Exception e)
      {
        TransformationLog.GenericLogEntry(e.Message, LogCategory.CheckInputFile);
      }

      try
      {
        metaDataDocMgt.RenumberPagesDoc = XmlUtility.LoadFromFileToXml(userSetup.MovePages);
      }
      catch (Exception e)
      {
        TransformationLog.GenericLogEntry(e.Message, LogCategory.CheckInputFile);
      }

      try
      {
        metaDataDocMgt.CodeRulesDoc.Clear();        
        metaDataDocMgt.CodeRulesDoc.AddRange(XmlUtility.LoadFromFileToString(userSetup.CodeRulesFile));
      }
      catch (Exception e)
      {
        TransformationLog.GenericLogEntry(e.Message, LogCategory.CheckInputFile);
      }

      if (errorExists)
      {
        TransformationLog.GenericLogEntry("Transformation aborted", LogCategory.Error);        
        return false;
      }

      return true;
    }

    private static Boolean GetUserInput(String[] args)
    {
      UserSetupManagement userSetup = UserSetupManagement.Instance;

      /* Load from App.Config */
      userSetup.FormsFile = System.Configuration.ConfigurationManager.AppSettings["FormsFile"];
      userSetup.PagesFile = System.Configuration.ConfigurationManager.AppSettings["PagesFile"];
      userSetup.IgnorePages = System.Configuration.ConfigurationManager.AppSettings["IgnorePages"];
      userSetup.MovePages = System.Configuration.ConfigurationManager.AppSettings["MovePages"];
      userSetup.TransformPages = System.Configuration.ConfigurationManager.AppSettings["TransformPages"];
      userSetup.DeletePageElements = System.Configuration.ConfigurationManager.AppSettings["DeletePageElements"];
      userSetup.MovePageElements = System.Configuration.ConfigurationManager.AppSettings["MovePageElements"];
      userSetup.SchemasPathLocation = System.Configuration.ConfigurationManager.AppSettings["SchemasPathLocation"];
      userSetup.CodeRulesFile = System.Configuration.ConfigurationManager.AppSettings["CodeRules"];
      userSetup.TranslationFile = System.Configuration.ConfigurationManager.AppSettings["TranslationFile"];

      /* parameters from args. */
      if (!ParseArgs(args))
      {
        return false;
      }

      return true;
    }

    private static void WriteAbortTransformation(Exception e)
    {
      TransformationLog.WriteErrorToLogFile(e, (int) LogEntryObjectId.NotSpecified);
      Console.WriteLine("Transformation aborted, look in the log file for details.");
    }

    private static Boolean ParseArgs(String[] args)
    {
      UserSetupManagement userSetup = UserSetupManagement.Instance;
      for (Int32 i = 0; i < args.Length; i++)
      {
        String arg = args[i];
        switch (arg.ToUpper(CultureInfo.InvariantCulture))
        {
          case "-F":
            i++;
            userSetup.FormsFile = args[i];
            break;

          case "-P":
            i++;
            userSetup.PagesFile = args[i];
            break;

          case "-I":
            i++;
            userSetup.IgnorePages = args[i];
            break;

          case "-R":
            i++;
            userSetup.MovePages = args[i];
            break;

          case "-A":
            i++;
            userSetup.TransformPages = args[i];
            break;

          case "-D":
            i++;
            userSetup.DeletePageElements = args[i];
            break;

          case "-M":
            i++;
            userSetup.MovePageElements = args[i];
            break;

          case "-C":
            i++;
            userSetup.CodeRulesFile = args[i];
            break;

          case "-T":
            i++;
            userSetup.TranslationFile = args[i];
            break;

          default:
            Console.WriteLine("-f : Form file.");
            Console.WriteLine("-p : Page file.");
            Console.WriteLine("-i : Ignore forms file.");
            Console.WriteLine("-r : Renumber page file.");
            Console.WriteLine("-a : Insert elements file.");
            Console.WriteLine("-d : Delete elements file.");
            Console.WriteLine("-m : Move elements file.");
            Console.WriteLine("-c : Code rules file.");
            Console.WriteLine("-t : Translations file.");

            Console.WriteLine("-h : Help");
            return false;
        }
      }

      return true;
    }
  }
}
