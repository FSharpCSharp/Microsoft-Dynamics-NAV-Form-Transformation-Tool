//--------------------------------------------------------------------------
// <copyright file="TransformationLog.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>?abc?</summary>
//--------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Xml;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.Tools.FormTransformation
{
  /// <summary>
  /// Log categories. You can prioritize them in FormTransformation.exe.config
  /// </summary>
  public enum LogCategory
  {
    /// <summary>
    /// Any error. Usualy Transformation tool can’t recover after such error
    /// </summary>
    Error,

    /// <summary>
    /// Different warnings. Usuali it’s related to file IO operations
    /// </summary>
    Warning,

    /// <summary>
    /// Any information. If evrithing is ok, user can suppress it
    /// </summary>
    GeneralInformation,

    /// <summary>
    /// Form was ignored. It could be included in IgnorePages.xml or becouse of matrix comtrol
    /// </summary>
    IgnoreForms,

    /// <summary>
    /// User can ignor this warning if evrithing is ok
    /// </summary>
    IgnoreWarning,

    /// <summary>
    /// Messages from CodeWash
    /// </summary>
    //  TempCodeWash,

    /// <summary>
    /// Control was removed
    /// </summary>
    RemoveControls,

    /// <summary>
    /// Property was removed
    /// </summary>
    //  RemoveProperties,

    /// <summary>
    /// User should change this code. Transformation tool can’t complitly transform it automatically. Usualy user should add some additional input (e.g. add InstructionalText instruction)
    /// </summary>
    ChangeCodeManually,

    /// <summary>
    /// Input files and so on
    /// </summary>
    InputInformation,

    /// <summary>
    /// Trigger can’t be supported and will be deleted
    /// </summary>
    //  DeleteTriggers,

    /// <summary>
    /// This control/code was changed. But user should validate it manually
    /// </summary>
    ValidateManually,

    /// <summary>
    /// Different issues with code transformation.
    /// </summary>
    CodeCannotBeTransformed,

    /// <summary>
    /// Some error or inconsistent in input file
    /// </summary>
    CheckInputFile,

    /// <summary>
    /// Possible compilation problem with unsupported property/method
    /// </summary>
    PossibleCompilationProblem
  }

  /// <summary>
  /// Form/Page or RequestForm/RequestPage ID.
  /// </summary>
  public enum LogEntryObjectId
  {
    /// <summary>
    /// Some error occured. Object ID n/a or unknown
    /// </summary>
    Error = -2,

    /// <summary>
    /// Object ID wasn't specified
    /// </summary>
    NotSpecified = -1,

    /// <summary>
    /// Log entry not connected to exact object
    /// </summary>
    None = 0
  }

  /// <summary>
  /// Generate log entries
  /// </summary>
  static public class TransformationLog
  {

    /// <summary>
    /// We have to correctly initialize Xml log file.
    /// It must be guaranteed to occur before a static method of the type is called.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
    static TransformationLog()
    {
      sw = new System.IO.StreamWriter("TransformationLog.xml");
      sw.Write(@"<?xml version=""1.0""?>");
      sw.Write(string.Format(CultureInfo.InvariantCulture, @"<{0} xmlns=""urn:schemas-microsoft-com:dynamics:NAV:ApplicationObjects"">", rootElementName));
    }

    private static System.Diagnostics.TraceSwitch generalSwitch = new System.Diagnostics.TraceSwitch("TraceLevelSwitch", "Entire application", "3");
    private static System.IO.StreamWriter sw;
    private const string rootElementName = "TransformationLog";

    /// <summary>
    /// Generates log entry for selected object id (Form/Page or RequestForm/RequestPage) with some suggestion how to resolve this issue
    /// </summary>
    /// <param name="description">System.String with description</param>
    /// <param name="logCategory">Some categories could be suppressed by FormTransformation.exe.config file</param>
    /// <param name="objectId">Form or Page ID related to this log entry. If it's n/a, use LogEntryObjectId</param>
    /// <param name="suggestion">Add posible suggestions how to resolve this issue to log file</param>
    public static void GenericLogEntry(String description, LogCategory logCategory, int objectId, String suggestion)
    {
      TraceSwitch appSwitch = new TraceSwitch(logCategory.ToString(), "");
      if ((generalSwitch.Level >= appSwitch.Level) || (objectId == (int)LogEntryObjectId.Error))
      {
        string tmpDescription = description;
        if (objectId != (int)LogEntryObjectId.NotSpecified)
        {
          tmpDescription = String.Format(CultureInfo.InvariantCulture, Resources.Form, objectId.ToString(CultureInfo.InvariantCulture), description);
        }
        Trace.WriteLine(tmpDescription, logCategory.ToString());
        Trace.Flush();
        WriteToXMLLogFile(logCategory, description, objectId, suggestion);

        if (logCategory == LogCategory.Error)
        {
          NestingXmlDocument.ErrorInTransformation = true;
        }
      }
    }

    /// <summary>
    /// Generates log entry for selected object id (Form/Page or RequestForm/RequestPage) with some suggestion how to resolve issue
    /// </summary>
    /// <param name="description">System.String with description</param>
    /// <param name="logCategory">Some categories could be suppressed by FormTransformation.exe.config file</param>
    /// <param name="objectId">Form or Page ID related to this log entry. If it's n/a, use LogEntryObjectId</param>
    /// <param name="suggestion">Add posible suggestions how to resolve this issue to log file</param>
    public static void GenericLogEntry(String description, LogCategory logCategory, String objectId, String suggestion)
    {
      TraceSwitch appSwitch = new TraceSwitch(logCategory.ToString(), "");
      if (generalSwitch.Level >= appSwitch.Level)
      {
        try
        {
          int i = Convert.ToInt32(objectId, CultureInfo.InvariantCulture);
          GenericLogEntry(description, logCategory, i, suggestion);
        }
        catch (System.FormatException ex)
        {
          GenericLogEntry(ex.Message, LogCategory.IgnoreWarning, (int)LogEntryObjectId.NotSpecified, null);

          string tmpDescription = String.Format(CultureInfo.InvariantCulture, Resources.Form, objectId, description);
          GenericLogEntry(tmpDescription, logCategory, (int)LogEntryObjectId.NotSpecified, suggestion);
        }
      }
    }

    /// <summary>
    /// Generates log entry for selected object id (Form/Page or RequestForm/RequestPage)
    /// </summary>
    /// <param name="description">System.String with description</param>
    /// <param name="logCategory">Some categories could be suppressed by FormTransformation.exe.config file</param>
    /// <param name="objectId">Form or Page ID related to this log entry. If it's n/a, use LogEntryObjectId</param>
    public static void GenericLogEntry(String description, LogCategory logCategory, int objectId)
    {
      GenericLogEntry(description, logCategory, objectId, null);
    }

    /// <summary>
    /// Generates very general log entry. Try to avoid it.
    /// </summary>
    /// <param name="description">System.String with description</param>
    /// <param name="logCategory">Some categories could be suppressed by FormTransformation.exe.config file</param>
    public static void GenericLogEntry(String description, LogCategory logCategory)
    {
      GenericLogEntry(description, logCategory, (int)LogEntryObjectId.NotSpecified, null);
    }

    private static void WriteToXMLLogFile(LogCategory logCategory, string description, int objectID, String suggestion)
    {
      try
      {
        XmlTextWriter writer = new XmlTextWriter(sw);
        writer.WriteStartElement("LogEntry");
        {
          writer.WriteElementString("LogCategory", logCategory.ToString());
          writer.WriteElementString("ObjectID", objectID.ToString(CultureInfo.InvariantCulture));
          writer.WriteElementString("Description", description);
          if (!string.IsNullOrEmpty(suggestion))
          {
            writer.WriteElementString("Suggestion", suggestion);
          }
        }

        writer.WriteEndElement();
        writer.Flush();
      }
      catch (AccessViolationException ex)
      {
        GenericLogEntry(ex.Message, LogCategory.Warning, (int)LogEntryObjectId.Error);
      }
    }

    /// <summary>
    /// Log basic information about input files
    /// </summary>
    public static void TransformationStarted()
    {
      UserSetupManagement userSetupManagement = UserSetupManagement.Instance;
      TraceSwitch appSwitch = new TraceSwitch(LogCategory.InputInformation.ToString(), "");

      if ((generalSwitch.Level >= appSwitch.Level))
      {
        string log = String.Format(CultureInfo.InvariantCulture, Resources.TransformationStarted, GetCurrentDateTimeString());
        Trace.WriteLine(log);

        if (userSetupManagement.FormsFile != null)
        {
          Trace.WriteLine("Forms file: " + userSetupManagement.FormsFile);
        }

        if (userSetupManagement.PagesFile != null)
        {
          Trace.WriteLine("Pages file: " + userSetupManagement.PagesFile);
        }

        if (userSetupManagement.TransformPages != null)
        {
          Trace.WriteLine("InsertElements file: " + userSetupManagement.TransformPages);
        }

        if (userSetupManagement.IgnorePages != null)
        {
          Trace.WriteLine("IgnoreForms file: " + userSetupManagement.IgnorePages);
        }

        if (userSetupManagement.DeletePageElements != null)
        {
          Trace.WriteLine("DeleteElements file: " + userSetupManagement.DeletePageElements);
        }

        if (userSetupManagement.MovePageElements != null)
        {
          Trace.WriteLine("MoveElements file: " + userSetupManagement.MovePageElements);
        }

        if (userSetupManagement.MovePages != null)
        {
          Trace.WriteLine("RenumberPages file: " + userSetupManagement.MovePages);
        }

        if (userSetupManagement.CodeRulesFile != null)
        {
          Trace.WriteLine("CodeRules file: " + userSetupManagement.CodeRulesFile);
        }

        Trace.WriteLine("-----------------------------------------------------------------------");
        Trace.Flush();
      }
    }

    /// <summary>
    /// Log general information
    /// </summary>
    /// <param name="duration">Duration of transformation process in seconds</param>
    public static void TransformationFinished(double duration)
    {
      TraceSwitch appSwitch = new TraceSwitch(LogCategory.InputInformation.ToString(), "");
      if ((generalSwitch.Level >= appSwitch.Level))
      {
        string log = String.Format(CultureInfo.InvariantCulture, Resources.TransformationFinished,
          GetCurrentDateTimeString(),
          Convert.ToString(Convert.ToInt32(duration, CultureInfo.InvariantCulture), CultureInfo.InvariantCulture));
        Trace.WriteLine(log);
        Trace.Flush();
      }
    }


    [SuppressMessage("Nav.Reliability", "DN0001:ConvertTimeZonesCorrectly", MessageId = "System.DateTime.ToLocalTime")]
    private static string GetCurrentDateTimeString()
    {
      return DateTime.UtcNow.ToLocalTime().ToString(CultureInfo.CurrentCulture);
    }

    /// <summary>
    /// Log error with all inner exceptions to log file
    /// </summary>
    /// <param name="exceptionToLog">System.Exception</param>
    /// <param name="objectId">Form or Page ID related to this log entry. If it's n/a, use LogEntryObjectId</param>
    public static void WriteErrorToLogFile(Exception exceptionToLog, int objectId)
    {
      WriteErrorToLogFile(exceptionToLog, objectId, LogCategory.Error);
    }

    /// <summary>
    /// Log error with all inner exceptions to log file
    /// </summary>
    /// <param name="exceptionToLog">System.Exception</param>
    /// <param name="objectId">Form or Page ID related to this log entry. If it's n/a, use LogEntryObjectId</param>
    /// <param name="logCategory">LogCategory if not Error</param>
    public static void WriteErrorToLogFile(Exception exceptionToLog, int objectId, LogCategory logCategory)
    {
      if (exceptionToLog != null)
      {
        GenericLogEntry(exceptionToLog.Message, logCategory, objectId);
        while (exceptionToLog.InnerException != null)
        {
          exceptionToLog = exceptionToLog.InnerException;
          TransformationLog.GenericLogEntry(exceptionToLog.Message, logCategory, objectId);
        }
      }
    }

    ///<summary>
    /// 
    ///</summary>
    public static void CloseXmlFile()
    {
      sw.Write(string.Format(CultureInfo.InvariantCulture, @"</{0}>", rootElementName));
      sw.Flush();
      sw = null;
    }
  }
}
