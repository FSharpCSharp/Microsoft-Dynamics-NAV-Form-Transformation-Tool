//dmytrk: we should support spaces etc.
//--------------------------------------------------------------------------
// <copyright file="CodeTransformationRules.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>?abc?</summary>
//--------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.Tools.FormTransformation
{
  /// <summary>
  /// ?abc?
  /// </summary>
  internal sealed class CodeTransformationRules
  {
    private static MetadataDocumentManagement metaDataDocMgt = MetadataDocumentManagement.Instance;
    private static Dictionary<int, CodeTransformationRule> rules = new Dictionary<int, CodeTransformationRule>();
    private static XmlDocument moveToPropertyDoc = new XmlDocument();
    private static XmlDocument moveToTriggerDoc = new XmlDocument();

    // private static Dictionary<String, CodeVariable> declareVariables = new Dictionary<String, CodeVariable>();

    private enum ReadState
    {
      Start,
      FindMatch,
      Find,
      ReplaceMatch,
      Replace,
      CommentMatch,
      Comment,
      LogMatch,
      Log,
      AtTriggerMatch,
      AtTrigger,
      MoveToPropertyMatch,
      MoveToProperty,
      MoveValueToPropertyMatch,
      MoveValueToProperty,
      MovePropertyToControlNameMatch,
      MovePropertyToControlName,
      DeclareVariableMatch,
      DeclareVariable,
      DeclareVariableTypeMatch,
      DeclareVariableType,
      DeclareVariablePropertyMatch,
      DeclareVariableProperty,
      MoveToTriggerMatch,
      MoveToTrigger,
      MoveCodeToTriggerMatch,
      MoveCodeToTrigger,
      IsReportMatch,
      IsReport
    }

    private static Regex findPattern = new Regex(@"<find>");
    private static Regex replacePattern = new Regex(@"<replace>");
    private static Regex commentPattern = new Regex(@"<comment>");
    private static Regex logPattern = new Regex(@"<log>");
    private static Regex atTriggerPattern = new Regex(@"<atTrigger>");
    private static Regex moveToPropertyPattern = new Regex(@"<moveToProperty>");
    private static Regex movePropertyToControlNamePattern = new Regex(@"<movePropertyToControlName>");
    private static Regex moveValueToPropertyPattern = new Regex(@"<moveValueToProperty>");
    private static Regex declareVariablePattern = new Regex(@"<declareVariable>");
    private static Regex declareVariableTypePattern = new Regex(@"<declareVariableType>");
    private static Regex declareVariablePropertyPattern = new Regex(@"<declareVariableProperty>");
    private static Regex moveToTriggerPattern = new Regex(@"<moveToTrigger>");
    private static Regex moveCodeToTriggerPattern = new Regex(@"<moveCodeToTrigger>");
    private static Regex isReportPattern = new Regex(@"<isReport>");

    private CodeTransformationRules()
    {
    }

    public static void SetRules()
    {
      moveToPropertyDoc = new XmlDocument();
      moveToTriggerDoc = new XmlDocument();
      rules = new Dictionary<int, CodeTransformationRule>();
      XmlNode xmlnode =
        moveToPropertyDoc.CreateNode(XmlNodeType.XmlDeclaration, "", "", metaDataDocMgt.XmlNamespace);
      moveToPropertyDoc.AppendChild(xmlnode);
      XmlElement xmlelem = moveToPropertyDoc.CreateElement("", "CodeToPropertyTransformation", "");
      moveToPropertyDoc.AppendChild(xmlelem);

      // MetaDataDocumentManagement metaDataDocMgt = MetaDataDocumentManagement.Instance;
      XmlNode xmlnode1 =
        moveToTriggerDoc.CreateNode(XmlNodeType.XmlDeclaration, "", "", metaDataDocMgt.XmlNamespace);
      moveToTriggerDoc.AppendChild(xmlnode1);
      XmlElement xmlelem1 =
        moveToTriggerDoc.CreateElement("", "CodeToTriggerTransformation", "");
      moveToTriggerDoc.AppendChild(xmlelem1);

      ReadState currState = ReadState.Start;
      int index = 0;
      CodeTransformationRule newRule = new CodeTransformationRule();
      if (MetadataDocumentManagement.Instance.CodeRulesDoc == null)
      {
        return;

        // UserSetupManagement userSetup = UserSetupManagement.Instance;
        // MetaDataDocumentManagement.Instance.CodeRulesDoc =
        // XMLUtility.LoadFromFileToString(userSetup.SchemasPathLocation + userSetup.CodeRulesFile);
      }

      foreach (string line in MetadataDocumentManagement.Instance.CodeRulesDoc)
      {
        if (findPattern.IsMatch(line))
        {
          if (currState != ReadState.Start)
          {
            while (newRule.replace.EndsWith("\r\n", StringComparison.OrdinalIgnoreCase))
            {
              newRule.replace = newRule.replace.Remove(newRule.replace.Length - 2);
            }

            rules.Add(index, newRule);
            index++;
          }

          newRule = new CodeTransformationRule();
          currState = UpdateState(ReadState.FindMatch, newRule, line);
        }
        else if (replacePattern.IsMatch(line))
          currState = UpdateState(ReadState.ReplaceMatch, newRule, line);
        else if (commentPattern.IsMatch(line))
          currState = UpdateState(ReadState.CommentMatch, newRule, line);
        else if (logPattern.IsMatch(line))
          currState = UpdateState(ReadState.LogMatch, newRule, line);
        else if (atTriggerPattern.IsMatch(line))
          currState = UpdateState(ReadState.AtTriggerMatch, newRule, line);
        else if (moveToPropertyPattern.IsMatch(line))
          currState = UpdateState(ReadState.MoveToPropertyMatch, newRule, line);
        else if (moveValueToPropertyPattern.IsMatch(line))
          currState = UpdateState(ReadState.MoveValueToPropertyMatch, newRule, line);
        else if (movePropertyToControlNamePattern.IsMatch(line))
          currState = UpdateState(ReadState.MovePropertyToControlNameMatch, newRule, line);
        else if (declareVariablePattern.IsMatch(line))
          currState = UpdateState(ReadState.DeclareVariableMatch, newRule, line);
        else if (declareVariableTypePattern.IsMatch(line))
          currState = UpdateState(ReadState.DeclareVariableTypeMatch, newRule, line);
        else if (declareVariablePropertyPattern.IsMatch(line))
          currState = UpdateState(ReadState.DeclareVariablePropertyMatch, newRule, line);
        else if (moveToTriggerPattern.IsMatch(line))
          currState = UpdateState(ReadState.MoveToTriggerMatch, newRule, line);
        else if (moveCodeToTriggerPattern.IsMatch(line))
          currState = UpdateState(ReadState.MoveCodeToTriggerMatch, newRule, line);
        else if (isReportPattern.IsMatch(line))
          currState = UpdateState(ReadState.IsReportMatch, newRule, line);
        else
          currState = UpdateState(currState, newRule, line);
      }

      if (currState != ReadState.Start)
      {
        // TODO shouldn't the lines below be cleaned out/removed???
        // string newString = ApplyRule(Convert.ToString((newRule.find)), "<var>", @"\w+|""[^""]+""", "hi");
        // newRule.find.Remove(0, newRule.find.Length);
        // newRule.find.Append(newString);
        while (newRule.replace.EndsWith("\r\n", StringComparison.OrdinalIgnoreCase))
        {
          newRule.replace = newRule.replace.Remove(newRule.replace.Length - 2);
        }

        rules.Add(index, newRule);
        index++;
      }
    }

    // TODO ApplyRule commented out because of a FxCop violation (method isn't called)
    //static string ApplyRule(string source, string find, string replace, string log)
    //{
    //  Regex expr = new Regex(find, RegexOptions.IgnoreCase);
    //  return expr.Replace(source, replace);
    //}

    private static Dictionary<int, CodeTransformationRule> GetRules()
    {
      return rules;
    }

    // A method that contains a large Switch statement is a candidate for exclusion
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    private static ReadState UpdateState(ReadState newState, CodeTransformationRule rule, String line)
    {
      switch (newState)
      {
        case ReadState.Start:
          return ReadState.Start;
        case ReadState.FindMatch:
          return ReadState.Find;
        case ReadState.Find:
          if (rule.find != null)
            rule.find = rule.find + "\r\n";
          rule.find = rule.find + line;
          SimpleNAVCodeParser sNAVCodeParser = new SimpleNAVCodeParser();
          rule.normalizedCode = sNAVCodeParser.GenerateNormalisedCode(Convert.ToString(rule.find, CultureInfo.InvariantCulture));
          break;
        case ReadState.ReplaceMatch:
          return ReadState.Replace;
        case ReadState.Replace:
          if (rule.replace != null)
            rule.replace = rule.replace + "\r\n";
          rule.replace = rule.replace + line;
          //          rule.replace.Append(line);
          break;
        case ReadState.CommentMatch:
          return ReadState.Comment;
        case ReadState.Comment:
          rule.comment.Append(line);
          break;
        case ReadState.LogMatch:
          return ReadState.Log;
        case ReadState.Log:
          rule.log.Append(line);
          break;
        case ReadState.AtTriggerMatch:
          return ReadState.AtTrigger;
        case ReadState.AtTrigger:
          rule.atTrigger.Append(line);
          break;
        case ReadState.MovePropertyToControlNameMatch:
          return ReadState.MovePropertyToControlName;
        case ReadState.MovePropertyToControlName:
          rule.movePropertyToControlName.Append(line);
          break;
        case ReadState.MoveToPropertyMatch:
          return ReadState.MoveToProperty;
        case ReadState.MoveToProperty:
          rule.moveToProperty.Append(line);
          break;
        case ReadState.MoveValueToPropertyMatch:
          return ReadState.MoveValueToProperty;
        case ReadState.MoveValueToProperty:
          rule.moveValueToProperty.Append(line);
          break;
        case ReadState.MoveToTriggerMatch:
          return ReadState.MoveToTrigger;
        case ReadState.MoveToTrigger:
          rule.moveToTrigger.Append(line);
          break;
        case ReadState.MoveCodeToTriggerMatch:
          return ReadState.MoveCodeToTrigger;
        case ReadState.MoveCodeToTrigger:
          if (rule.moveCodeToTrigger != null)
            rule.moveCodeToTrigger = rule.moveCodeToTrigger + "\r\n";
          else
            rule.moveCodeToTrigger = "\r\n";
          rule.moveCodeToTrigger = rule.moveCodeToTrigger + line;
          //rule.moveCodeToTrigger.Append(line);
          break;
        case ReadState.DeclareVariableMatch:
          return ReadState.DeclareVariable;
        case ReadState.DeclareVariable:
          rule.declareVariable.Append(line);
          break;
        case ReadState.DeclareVariableTypeMatch:
          return ReadState.DeclareVariableType;
        case ReadState.DeclareVariableType:
          rule.declareVariableType.Append(line);
          break;
        case ReadState.DeclareVariablePropertyMatch:
          return ReadState.DeclareVariableProperty;
        case ReadState.DeclareVariableProperty:
          rule.declareVarialeProperty.Append(line);
          break;
        case ReadState.IsReportMatch:
          return ReadState.IsReport;
        case ReadState.IsReport:
          rule.isReport.Append(line);
          break;
      }

      return newState;
    }

    public static void FindAndReplaceCode(XmlNode triggerNode, bool unsupportedTrigger, bool isTrigger)
    {
      bool restart = true;
      while (restart)
      {
        // MetaDataDocumentManagement metaDataDocMgt = MetaDataDocumentManagement.Instance;
        XmlNodeList pageMoveActionList =
          moveToTriggerDoc.SelectNodes(
            @".//Move[./Form='" + metaDataDocMgt.GetCurrentPageId + "' and ./Procedure='']");

        if (pageMoveActionList.Count != 0)
          return;

        restart = false;
        SimpleNAVCodeParser sNAVCodeParser = new SimpleNAVCodeParser();
        List<NormalizedCode> normNAVCode = sNAVCodeParser.GenerateNormalisedCode(triggerNode.InnerText);

        foreach (KeyValuePair<Int32, CodeTransformationRule> codeTransRule in GetRules())
        {
          restart = (FindCodeRuleAndReplace(codeTransRule.Value, triggerNode, normNAVCode));
          if (restart)
            break;
        }
      }

      CleanUpRemainingCode(triggerNode, unsupportedTrigger, isTrigger);
    }

    private static Boolean FindCodeRuleAndReplace(
      CodeTransformationRule codeTransRule, XmlNode triggerNode, List<NormalizedCode> normNAVCode)
    {
      for (int i = 0; i < normNAVCode.Count - codeTransRule.normalizedCode.Count + 1; i++)
      {
        if ((codeTransRule.atTrigger.Length > 0) && (i == 0))
          if (Convert.ToString(codeTransRule.atTrigger, CultureInfo.InvariantCulture) != triggerNode.Name)
            break;

        Dictionary<int, string> variables = new Dictionary<int, string>();
        for (int j = 0; j < codeTransRule.normalizedCode.Count; j++)
        {
          if (!(((codeTransRule.normalizedCode[j].tokenType == normNAVCode[i + j].tokenType) &&
                 (codeTransRule.normalizedCode[j].Token == normNAVCode[i + j].Token))
                ||
                ((codeTransRule.normalizedCode[j].tokenType == TokenType.IsCurrForm) &&
                 ((normNAVCode[i + j].Token == "CurrForm") || (normNAVCode[i + j].Token == "RequestOptionsForm")))
                ||
                ((codeTransRule.normalizedCode[j].tokenType == TokenType.IsAnyNAVVar) &&
                 ((normNAVCode[i + j].tokenType == TokenType.IsNAVVar) ||
                  (normNAVCode[i + j].tokenType == TokenType.IsNAVVarInQuotes) ||
                  (normNAVCode[i + j].tokenType == TokenType.IsString)))
               ))
            break;  //no match
          if ((codeTransRule.normalizedCode[j].tokenType == TokenType.IsAnyNAVVar) &&
              (codeTransRule.normalizedCode[j].variableNo != 0))
          {
            string varName;
            if (variables.TryGetValue(codeTransRule.normalizedCode[j].variableNo, out varName))
            {
              if (varName != normNAVCode[i + j].Token)
                break; //no match
            }
            else
              variables.Add(codeTransRule.normalizedCode[j].variableNo, normNAVCode[i + j].Token);
          }

          if (j == codeTransRule.normalizedCode.Count - 1) // code match found
          {
            if ((Convert.ToString(codeTransRule.isReport, CultureInfo.InvariantCulture) == "Yes") &&
                (triggerNode.Name == "Code") &&
                (!IsProcedureHandled(triggerNode, normNAVCode[i + j].position)))
            {
              DeclareProcedure(triggerNode, normNAVCode[i + j].position);
              return true;
            }

            if (ReplaceCode(triggerNode, normNAVCode, i, j, codeTransRule, variables))
            {
              codeTransRule.matchesFound++; // record number of matches found
              return true;
            }
          }
        }
      }
      return false;
    }

    private static void UpdateProcedureState(
      NormalizedCode normNAVCode,
      ref Boolean inProcedure, ref Boolean inCodeBody,
      ref Int32 beginPos, ref Int32 procedurePos,
      ref Int32 endPos, ref Int32 parameterPos,
      ref String parameters, ref Boolean hasReturnValue,
      ref String procedureName, ref Boolean nextIsProcedureName,
      ref Boolean nextIsParameterVar, ref Int32 parameterEndsAt)
    {
      if (inProcedure)
      {
        if (inCodeBody)  //inCode
        {
          if ((normNAVCode.xPos == 2) && (normNAVCode.Token == "END"))
          {
            endPos = normNAVCode.position;
            inProcedure = false;
            inCodeBody = false;
          }
          return;
        }
        if ((normNAVCode.xPos == 2) && (normNAVCode.Token == "BEGIN")) //inLocalVarSet
        {
          inCodeBody = true;
          beginPos = normNAVCode.position;
          return;
        }
        if (String.IsNullOrEmpty(procedureName)) //ProcedureName
        {
          if (nextIsProcedureName)
            procedureName = normNAVCode.Token;
          nextIsProcedureName = !nextIsProcedureName;
          return;
        }
        if (normNAVCode.Token.StartsWith("(", StringComparison.Ordinal)) //inParameter
        {
          parameterPos = normNAVCode.position;
          parameterEndsAt = 0;
          if (normNAVCode.Token.EndsWith(")", StringComparison.Ordinal))
          {
            parameterEndsAt = normNAVCode.position + normNAVCode.Token.Length - 1;
          }
          else
            nextIsParameterVar = true;
          return;
        }
        if (normNAVCode.Token.StartsWith(")", StringComparison.Ordinal)) //inReturnValue
        {
          parameterEndsAt = normNAVCode.position;
          return;
        }
        if ((parameterEndsAt + 1 == normNAVCode.position) && (normNAVCode.Token == ";")) //inLocalVar
        {
          hasReturnValue = false;
          return;
        }
        if ((parameterEndsAt == 0) && (parameterPos != 0))
        {
          if (nextIsParameterVar)
          {
            if (normNAVCode.Token != "VAR")
            {
              if (parameters.Length == 0)
                parameters = normNAVCode.Token;
              else
                parameters = parameters + ", " + normNAVCode.Token;

              nextIsParameterVar = false;
            }
          }
          if (normNAVCode.Token == ";")
            nextIsParameterVar = true;
          return;
        }
        return;
      }
      if (!((normNAVCode.xPos == 2) && ((normNAVCode.Token == "LOCAL") ||
                                        (normNAVCode.Token == "PROCEDURE") ||
                                        (normNAVCode.Token == "EVENT") ||
                                        (normNAVCode.Token == "BEGIN"))))
        return;

      inProcedure = (normNAVCode.Token != "BEGIN");
      nextIsProcedureName = normNAVCode.Token != "LOCAL";
      procedureName = "";
      beginPos = 0;
      parameterPos = 0;
      parameterEndsAt = 0;
      parameters = "";
      procedurePos = normNAVCode.position;
    }

    private static Boolean FindProcedure(
      XmlNode codeTriggerNode,
      Int32 procedureContainingPos, ref Int32 beginPos,
      ref Int32 procedurePos, ref Int32 endPos,
      ref Int32 parameterPos, ref String parameters,
      ref Boolean hasReturnValue, ref String procedureName)
    {
      Boolean inProcedure = false;
      Boolean inCodeBody = false;
      Int32 parameterEndsAt = 0;
      Boolean nextIsProcedureName = false;
      Boolean nextIsParameterVar = false;

      SimpleNAVCodeParser sNAVCodeParser = new SimpleNAVCodeParser();
      foreach (NormalizedCode normNAVCode in sNAVCodeParser.GenerateNormalisedCode(codeTriggerNode.InnerText))
      {
        UpdateProcedureState(
           normNAVCode,
           ref inProcedure, ref inCodeBody,
           ref beginPos, ref procedurePos,
           ref endPos, ref parameterPos,
           ref parameters, ref hasReturnValue,
           ref procedureName, ref nextIsProcedureName,
           ref nextIsParameterVar, ref parameterEndsAt);
        if (!inCodeBody && !inProcedure && (procedureName.Length > 0) &&
            (procedureContainingPos < normNAVCode.position)
           )
          return true;
      }
      return false;
    }

    private static Boolean FindVariableDeclaration(XmlNode codeTriggerNode, ref Int32 position)
    {
      Int32 beginPos = 0;
      Int32 procedurePos = 0;
      Int32 endPos = 0;
      Int32 parameterPos = 0;
      String parameters = null;
      Boolean hasReturnValue = false;
      String procedureName = null;
      Boolean inProcedure = false;
      Boolean inCodeBody = false;
      Int32 parameterEndsAt = 0;
      Boolean nextIsProcedureName = false;
      Boolean nextIsParameterVar = false;
      Boolean isFirst = true;

      SimpleNAVCodeParser sNAVCodeParser = new SimpleNAVCodeParser();
      foreach (NormalizedCode normNAVCode in sNAVCodeParser.GenerateNormalisedCode(codeTriggerNode.InnerText))
      {
        UpdateProcedureState(
           normNAVCode,
           ref inProcedure, ref inCodeBody,
           ref beginPos, ref procedurePos,
           ref endPos, ref parameterPos,
           ref parameters, ref hasReturnValue,
           ref procedureName, ref nextIsProcedureName,
           ref nextIsParameterVar, ref parameterEndsAt);
        if ((inProcedure || (normNAVCode.Token == "BEGIN")) && (normNAVCode.xPos == 2))
        {
          if (isFirst)
            return false;
          position = normNAVCode.position - 4;
          return true;
        }
        isFirst = false;
      }
      return false;
    }

    public static Boolean FindVariable(String varName, String codeTrigger)
    {
      Int32 beginPos = 0;
      Int32 procedurePos = 0;
      Int32 endPos = 0;
      Int32 parameterPos = 0;
      String parameters = null;
      Boolean hasReturnValue = false;
      String procedureName = null;
      Boolean inProcedure = false;
      Boolean inCodeBody = false;
      Int32 parameterEndsAt = 0;
      Boolean nextIsProcedureName = false;
      Boolean nextIsParameterVar = false;

      SimpleNAVCodeParser sNAVCodeParser = new SimpleNAVCodeParser();

      foreach (NormalizedCode normNAVCode in sNAVCodeParser.GenerateNormalisedCode(codeTrigger))
      {
        UpdateProcedureState(
           normNAVCode,
           ref inProcedure, ref inCodeBody,
           ref beginPos, ref procedurePos,
           ref endPos, ref parameterPos,
           ref parameters, ref hasReturnValue,
           ref procedureName, ref nextIsProcedureName,
           ref nextIsParameterVar, ref parameterEndsAt);

        if (inProcedure)
          return false;
        if (inCodeBody)
          return false;
        if (normNAVCode.Token == varName)
          return true;
      }
      return false;
    }

    public static String FindOptionList(String optionName, String codeTrigger)
    {
      Int32 beginPos = 0;
      Int32 procedurePos = 0;
      Int32 endPos = 0;
      Int32 parameterPos = 0;
      String parameters = null;
      Boolean hasReturnValue = false;
      String procedureName = null;
      Boolean inProcedure = false;
      Boolean inCodeBody = false;
      Int32 parameterEndsAt = 0;
      Boolean nextIsProcedureName = false;
      Boolean nextIsParameterVar = false;

      SimpleNAVCodeParser sNAVCodeParser = new SimpleNAVCodeParser();
      Boolean foundOption = false;
      Boolean beginCopying = false;
      String result = null;
      foreach (NormalizedCode normNAVCode in sNAVCodeParser.GenerateNormalisedCode(codeTrigger))
      {
        UpdateProcedureState(
           normNAVCode,
           ref inProcedure, ref inCodeBody,
           ref beginPos, ref procedurePos,
           ref endPos, ref parameterPos,
           ref parameters, ref hasReturnValue,
           ref procedureName, ref nextIsProcedureName,
           ref nextIsParameterVar, ref parameterEndsAt);

        if (foundOption)
        {
          if (beginCopying)
          {
            if (normNAVCode.tokenType == TokenType.IsSemiColon)
              return result;
            result = result + normNAVCode.Token;
          }

          if ((normNAVCode.tokenType == TokenType.IsSymbol) && (!beginCopying))
          {
            if (normNAVCode.Token == ":")
              beginCopying = true;
          }
        }
        if (inProcedure)
          return null;
        if (inCodeBody)
          return null;
        if (normNAVCode.Token == optionName)
        {
          foundOption = true;
          result = "";
        }
      }
      return null;
    }

    private static void DeclareProcedure(XmlNode codeTriggerNode, Int32 procedureContainingPos)
    {
      Int32 procedurePos = 0;
      Int32 beginPos = 0;
      Int32 endPos = 0;
      Int32 parameterPos = 0;

      String procedureName = "";
      String parameters = "";

      Boolean hasReturnValue = true;

      if (FindProcedure(
            codeTriggerNode,
            procedureContainingPos, ref beginPos,
            ref procedurePos, ref endPos,
            ref parameterPos, ref parameters,
            ref hasReturnValue, ref procedureName)
         )
      {
        // This declares code special for the page. 
        String beginningPart = codeTriggerNode.InnerText.Substring(0, beginPos + 6);
        String procedureHeaderPart =
          codeTriggerNode.InnerText.Substring(procedurePos - 1, beginPos - procedurePos);
        String codePart = codeTriggerNode.InnerText.Substring(beginPos + 7, endPos - beginPos - 8);
        String endPart = codeTriggerNode.InnerText.Substring(endPos - 1);

        // This stores the original code that should not be transformed.
        if (parameters.Length != 0)
          parameters = "(" + parameters + ")";

        // newCode = newCode.Replace("\r\n  ", "\r\n");
        if (hasReturnValue)
          UpdateMoveToTriggerDocument(
            Convert.ToString(metaDataDocMgt.GetCurrentPageId, CultureInfo.InvariantCulture),
            "",
            "Code",
            procedureName,
            String.Format(
              CultureInfo.InvariantCulture,
              "\r\n  IF ISSERVICETIER THEN \r\n    EXIT({0}{1});\r\n{2}",
              "Page" + procedureName, parameters, codePart),
            "append");
        else
          UpdateMoveToTriggerDocument(
            Convert.ToString(metaDataDocMgt.GetCurrentPageId, CultureInfo.InvariantCulture),
            "",
            "Code",
            procedureName,
            String.Format(
              CultureInfo.InvariantCulture,
              "\r\n  IF ISSERVICETIER THEN BEGIN\r\n    {0}{1};\r\n    EXIT;\r\n  END;\r\n{2}",
              "Page" + procedureName, parameters, codePart),
            "append");

        UpdateMoveToTriggerDocument(
          Convert.ToString(metaDataDocMgt.GetCurrentPageId, CultureInfo.InvariantCulture),
          "",
          "Code", "",
          String.Format(
            CultureInfo.InvariantCulture,
            "\r\nLOCAL PROCEDURE Page{0}@{1}{2}BEGIN\r\n{3}\r\nEND;\r\n",
            procedureName,
            metaDataDocMgt.CalcId(null, procedureName, "Code"),
            procedureHeaderPart.Remove(0, parameterPos - procedurePos),
            codePart),
          "append");

        codeTriggerNode.RemoveAll();
        XmlCDataSection data = metaDataDocMgt.XmlDocument.CreateCDataSection(
          String.Format(CultureInfo.InvariantCulture, "{0}{1}",
          beginningPart, endPart));
        codeTriggerNode.AppendChild(data);
      }
    }

    private static Boolean IsProcedureHandled(XmlNode codeTriggerNode, Int32 procedureContainingPos)
    {
      Int32 procedurePos = 0;
      Int32 beginPos = 0;
      Int32 endPos = 0;
      Int32 parameterPos = 0;

      String procedureName = "";
      String parameters = "";

      Boolean hasReturnValue = true;

      if (FindProcedure(
            codeTriggerNode,
            procedureContainingPos, ref beginPos,
            ref procedurePos, ref endPos,
            ref parameterPos, ref parameters,
            ref hasReturnValue, ref procedureName)
          )
      {
        return (codeTriggerNode.InnerText.Substring(procedurePos - 1).StartsWith("LOCAL PROCEDURE Page", StringComparison.Ordinal));
      }
      return false;
    }

    private static Int32 getIndentLength(String theCode)
    {
      Int32 indentLength = 0;
      for (Int32 i = theCode.Length; i > 0; i--)
        switch (theCode[i - 1])
        {
          case '\r':
          case '\n':
            return indentLength;
          case ' ':
            indentLength++;
            break;
          default:
            indentLength = 0;
            break;
        }
      return indentLength;
    }

    private static Boolean ReplaceCode(
      XmlNode triggerNode,
      List<NormalizedCode> normNAVCode,
      Int32 i,
      Int32 j,
      CodeTransformationRule codeTransRule,
      Dictionary<int, string> variables)
    {
      String oldCode = triggerNode.InnerText;
      // replace code with rule
      String beginningPart = triggerNode.InnerText.Substring(0, normNAVCode[i].position - 1);
      String endPart =
        triggerNode.InnerText.Substring(normNAVCode[i + j].position + normNAVCode[i + j].Token.Length - 1);
      triggerNode.RemoveAll();

      String indentation = "\r\n";
      //indentation = indentation.PadRight(beginningPart.Length - beginningPart.TrimEnd(' ').Length);
      for (Int32 indent = getIndentLength(beginningPart); indent > 0; indent--)
        indentation = indentation + " ";

      String replaceString =
        Convert.ToString(codeTransRule.replace.Replace("\r\n", indentation), CultureInfo.InvariantCulture);
      String moveValueToPropertyString =
        Convert.ToString(codeTransRule.moveValueToProperty, CultureInfo.InvariantCulture);
      String movePropertyToControlNameString =
        Convert.ToString(codeTransRule.movePropertyToControlName, CultureInfo.InvariantCulture);
      String moveToPropertyString =
        Convert.ToString(codeTransRule.moveToProperty, CultureInfo.InvariantCulture);

      String moveCodeToTriggerString =
        Convert.ToString(codeTransRule.moveCodeToTrigger, CultureInfo.InvariantCulture);
      String moveToTriggerString =
        Convert.ToString(codeTransRule.moveToTrigger, CultureInfo.InvariantCulture);

      String declareVariableString =
        Convert.ToString(codeTransRule.declareVariable, CultureInfo.InvariantCulture);
      String declareVariableTypeString =
        Convert.ToString(codeTransRule.declareVariableType, CultureInfo.InvariantCulture);
      String declareVariablePropertyString =
        Convert.ToString(codeTransRule.declareVarialeProperty, CultureInfo.InvariantCulture);

      if (declareVariableString.Length != 0)
      {
        // This could handle including search !control! in input
        String sourceExpr = GetProperty(triggerNode.ParentNode.ParentNode, "SourceExpr");
        if (sourceExpr != null)
          variables.Add(-1, sourceExpr);
        if (variables.Count != 0)
          declareVariableString = ReplaceVariables(declareVariableString, variables);

        declareVariableString = declareVariableString.Replace("\"", "");

        // trim to 30 spaces but try to keep begining of phrase
        if (declareVariableString.Length > 30)
        {
          string originaName = declareVariableString;
          declareVariableString = RemoveNonSignificantSymbols(declareVariableString);
          if (declareVariableString.Length > 30)
          {
            declareVariableString = declareVariableString.Remove(30, declareVariableString.Length - 30);
          }

          TransformationLog.GenericLogEntry(string.Format(CultureInfo.InvariantCulture, Resources.RenameVariable, originaName, declareVariableString), LogCategory.ValidateManually, metaDataDocMgt.GetCurrentPageId);
        }

        Regex invalidChars = new Regex(@"\W");

        if (invalidChars.Match(declareVariableString).Success)
        {
          declareVariableString = "\"" + declareVariableString + "\"";
        }
        else
        {
          Regex firstIsNumeric = new Regex(@"\b\d");
          if (firstIsNumeric.Match(declareVariableString).Success)
          {
            declareVariableString = "\"" + declareVariableString + "\"";
          }
        }

        //declareVariableString = String.Format(CultureInfo.InvariantCulture, "\"{0}\"", declareVariableString);
        if (!variables.ContainsKey(0))
          variables.Add(0, declareVariableString);
        //CodeVariable codeVariable;
        //if (!declareVariables.TryGetValue(declareVariableString, out codeVariable))
        //{
        //  codeVariable = new CodeVariable();
        //  codeVariable.Name = declareVariableString;
        //  codeVariable.Type = declareVariableTypeString;
        //  // codeVariable.Property = declareVariablePropertyString; INDATASET
        //  declareVariables.Add(declareVariableString, codeVariable);
        //}
      }
      if (variables.Count != 0)
      {
        replaceString = ReplaceVariables(replaceString, variables);
        moveValueToPropertyString = ReplaceVariables(moveValueToPropertyString, variables);
        movePropertyToControlNameString = ReplaceVariables(movePropertyToControlNameString, variables);
        moveToPropertyString = ReplaceVariables(moveToPropertyString, variables);

        moveCodeToTriggerString = ReplaceVariables(moveCodeToTriggerString, variables);
        moveToTriggerString = ReplaceVariables(moveToTriggerString, variables);

        declareVariableTypeString = ReplaceVariables(declareVariableTypeString, variables);
        declareVariablePropertyString = ReplaceVariables(declareVariablePropertyString, variables);
      }

      if ((codeTransRule.moveValueToProperty.Length != 0) || (codeTransRule.declareVariable.Length != 0))
        UpdateMoveToPropertyDocument(
          Convert.ToString(metaDataDocMgt.GetCurrentPageId, CultureInfo.InvariantCulture),
          movePropertyToControlNameString,
          moveToPropertyString, moveValueToPropertyString,
          declareVariableString, declareVariableTypeString, declareVariablePropertyString);

      if (!String.IsNullOrEmpty(codeTransRule.moveCodeToTrigger))
        UpdateMoveToTriggerDocument(
          Convert.ToString(metaDataDocMgt.GetCurrentPageId, CultureInfo.InvariantCulture),
          movePropertyToControlNameString,
          moveToTriggerString, "",
          moveCodeToTriggerString,
          "prepend");

      if (replaceString.StartsWith(";", StringComparison.Ordinal))
      {
        SimpleNAVCodeParser beginNAVCodeParser = new SimpleNAVCodeParser();
        List<NormalizedCode> normbeginNAVCode = beginNAVCodeParser.GenerateNormalisedCode(Convert.ToString(beginningPart, CultureInfo.InvariantCulture));
        String newBeginningPart = beginningPart.TrimEnd(' ');
        newBeginningPart = newBeginningPart.TrimEnd('\n');
        newBeginningPart = newBeginningPart.TrimEnd('\r');
        if ((beginningPart.Length != newBeginningPart.Length) &&
            (normbeginNAVCode[normbeginNAVCode.Count - 1].position +
            normbeginNAVCode[normbeginNAVCode.Count - 1].Token.Length - 1 ==
            newBeginningPart.Length))
          beginningPart = newBeginningPart;
      }
      XmlCDataSection data = metaDataDocMgt.XmlDocument.CreateCDataSection(beginningPart + replaceString + endPart);
      triggerNode.AppendChild(data);
      return (triggerNode.InnerText != oldCode);
    }

    private static String ReplaceVariables(String originalString, Dictionary<int, string> variables)
    {
      if (String.IsNullOrEmpty(originalString))
        return originalString;
      String newString = originalString;
      SimpleNAVCodeParser replaceNAVCodeParser = new SimpleNAVCodeParser();
      List<NormalizedCode> normreplaceNAVCode = replaceNAVCodeParser.GenerateNormalisedCode(Convert.ToString(originalString, CultureInfo.InvariantCulture));
      String varName;
      for (int k = normreplaceNAVCode.Count - 1; k >= 0; k--)
      {
        if ((normreplaceNAVCode[k].tokenType == TokenType.IsAnyNAVVar) &&
            (normreplaceNAVCode[k].variableNo != 0) &&
            (variables.TryGetValue(normreplaceNAVCode[k].variableNo, out varName)))
        {
          String beginningReplacePart = newString.Substring(0, normreplaceNAVCode[k].position - 1);
          String endReplacePart = newString.Substring(normreplaceNAVCode[k].position + normreplaceNAVCode[k].Token.Length - 1);
          newString = beginningReplacePart + varName + endReplacePart;
        }
        if ((normreplaceNAVCode[k].tokenType == TokenType.IsDeclaredVar) &&
            (variables.TryGetValue(0, out varName)))
        {
          String beginningReplacePart = newString.Substring(0, normreplaceNAVCode[k].position - 1);
          String endReplacePart = newString.Substring(normreplaceNAVCode[k].position + normreplaceNAVCode[k].Token.Length - 1);
          newString = beginningReplacePart + varName + endReplacePart;
        }
        if ((normreplaceNAVCode[k].tokenType == TokenType.IsCurrControl) &&
            (variables.TryGetValue(-1, out varName)))
        {
          String beginningReplacePart = newString.Substring(0, normreplaceNAVCode[k].position - 1);
          String endReplacePart = newString.Substring(normreplaceNAVCode[k].position + normreplaceNAVCode[k].Token.Length - 1);
          newString = beginningReplacePart + varName + endReplacePart;
        }
      }
      return newString;
    }

    public static void UpdateMoveToPropertyDocument(
      String pageId,
      String movePropertyToControlName,
      String moveToProperty, String moveToPropertyValue,
      String declareVariableName, String declareVariableType, String declareVariableProperty)
    {
      //add to move document.

      // XmlNode xmlnode;
      XmlElement xmlelem;
      xmlelem = moveToPropertyDoc.CreateElement("", "Move", "");
      moveToPropertyDoc.LastChild.AppendChild(xmlelem);

      xmlelem.AppendChild(CreateXmlElement(moveToPropertyDoc, "Form", pageId));
      xmlelem.AppendChild(CreateXmlElement(moveToPropertyDoc, "ControlName", movePropertyToControlName));
      xmlelem.AppendChild(CreateXmlElement(moveToPropertyDoc, "Property", moveToProperty));
      xmlelem.AppendChild(CreateXmlElement(moveToPropertyDoc, "PropertyValue", moveToPropertyValue));
      xmlelem.AppendChild(CreateXmlElement(moveToPropertyDoc, "DeclareVariableName", declareVariableName));
      xmlelem.AppendChild(CreateXmlElement(moveToPropertyDoc, "DeclareVariableType", declareVariableType));
      xmlelem.AppendChild(CreateXmlElement(moveToPropertyDoc, "DeclareVariableProperty", declareVariableProperty));

      moveToPropertyDoc.LastChild.AppendChild(xmlelem);
    }

    public static void UpdateMoveToTriggerDocument(
          String pageId,
          String movePropertyToControlName,
          String moveToTrigger,
          String moveToProcedure,
          String moveCodeToTrigger,
          String wheretoMove)
    {
      //add to move document.

      // XmlNode xmlnode;
      XmlElement xmlelem;

      xmlelem = moveToTriggerDoc.CreateElement("", "Move", "");
      moveToTriggerDoc.LastChild.AppendChild(xmlelem);
      xmlelem.AppendChild(CreateXmlElement(moveToTriggerDoc, "Form", pageId));
      xmlelem.AppendChild(CreateXmlElement(moveToTriggerDoc, "ControlName", movePropertyToControlName));
      xmlelem.AppendChild(CreateXmlElement(moveToTriggerDoc, "Trigger", moveToTrigger));
      xmlelem.AppendChild(CreateXmlElement(moveToTriggerDoc, "Procedure", moveToProcedure));
      xmlelem.AppendChild(CreateXmlElement(moveToTriggerDoc, "TriggerCode", moveCodeToTrigger));
      xmlelem.AppendChild(CreateXmlElement(moveToTriggerDoc, "Where", wheretoMove));
      moveToTriggerDoc.LastChild.AppendChild(xmlelem);
    }

    private static XmlElement CreateXmlElement(XmlDocument xmlDoc, String name, String innerText)
    {
      XmlElement element;

      element = xmlDoc.CreateElement("", name, "");
      if (innerText != null)
        element.InnerText = innerText;
      return element;
    }

    [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Requires refactoring, but time doesn’t permit refactoring at this point")]
    private static void CleanUpRemainingCode(XmlNode triggerNode, bool unSupportedTrigger, bool isTrigger)
    {
      //test if trigger is empty
      bool triggerIsEmpty = true;
      bool beginFound = false;
      SimpleNAVCodeParser sNAVCodeParser1 = new SimpleNAVCodeParser();
      foreach (NormalizedCode NAVCode in sNAVCodeParser1.GenerateNormalisedCode(triggerNode.InnerText))
      {
        //detect code in BEGIN END;
        if (!beginFound)
        {
          if (NAVCode.Token == "BEGIN")
            beginFound = true;
        }
        else
        {
          if ((NAVCode.Token != "END") && triggerIsEmpty)
            triggerIsEmpty = false;
          break;
        }
      }

      if (triggerIsEmpty && isTrigger)
      {
        triggerNode.RemoveAll();
        triggerNode.ParentNode.RemoveChild(triggerNode);
      }
      else if (unSupportedTrigger && !triggerIsEmpty)
      {
        Boolean isMoved = false;
        switch (triggerNode.Name)
        {
          case "OnAfterGetCurrRecord":
            MoveOnAfterGetCurrRecToOnAfterGetRec(triggerNode);
            isMoved = true;
            break;
          case "OnAfterValidate":
            MoveOnAfterValidateToOnValidate(triggerNode);
            isMoved = true;
            break;
          case "OnFormat":
            MoveOnFormatToOnAfterGetRecord(triggerNode);
            isMoved = true;
            break;
          case "OnDeactivateForm":
            MoveOnDeactivateForm(triggerNode);
            break;
          case "OnActivateForm":
            MoveOnActivateFormToOnOpenForm(triggerNode);
            break;
          case "OnDeactivate":
            MoveOnDeactivate(triggerNode);
            break;
          case "OnActivate":
            MoveOnActivate(triggerNode);
            break;
          case "OnBeforeInput":
            MoveOnBeforeInput(triggerNode);
            break;
          case "OnAfterInput":
            MoveOnAfterInput(triggerNode);
            break;
          case "OnInputChange":
            MoveOnInputChange(triggerNode);
            break;
          case "OnBeforePutRecord":
            MoveOnBeforePutRecord(triggerNode);
            break;
          case "OnTimer":
            MoveOnTimer(triggerNode);
            break;
          case "OnCreateHyperlink":
            MoveOnCreateHyperlink(triggerNode);
            break;
          case "OnHyperlink":
            MoveOnHyperlink(triggerNode);
            break;
          case "OnPush":
            if (GetProperty(triggerNode.ParentNode.ParentNode, "Controltype") == "CommandButton")
              MoveOnPushToOnQueryCloseForm(triggerNode);
            else
              MoveOnPushToOnValidate(triggerNode);
            isMoved = true;
            break;
        }

        // log
        if (!isMoved)
        {
          String logStr = String.Format(CultureInfo.InvariantCulture,
            Resources.SummarizeCodeTransformationAction,
            metaDataDocMgt.GetCurrentPageId + " " + triggerNode.Name,
            triggerNode.InnerText);
          TransformationLog.GenericLogEntry(logStr, LogCategory.GeneralInformation, 0);
        }

        triggerNode.RemoveAll();
        triggerNode.ParentNode.RemoveChild(triggerNode);
      }
      else
      {
        //test to see if unsupport function exits
        bool unSupportedFunctionExists = false;
        bool testFunction = false;
        bool isControlFunction = false;
        Int32 startFrom = 0;
        StringBuilder unsupportedProperty = new StringBuilder();

        foreach (NormalizedCode NAVCode in sNAVCodeParser1.GenerateNormalisedCode(triggerNode.InnerText))
        {
          if (!isTrigger)
            if ((NAVCode.Token == "PROCEDURE") || (NAVCode.Token == "EVENT"))
              startFrom = NAVCode.position;
          if (testFunction)
          {
            if ((NAVCode.tokenType != TokenType.IsAnyNAVVar) &&
                (NAVCode.tokenType != TokenType.IsNAVVar) &&
                (NAVCode.tokenType != TokenType.IsNAVVarInQuotes) &&
                (NAVCode.tokenType != TokenType.IsSpace) &&
                ((NAVCode.tokenType == TokenType.IsSymbol) && (NAVCode.Token != ".")))
              testFunction = false;
          }
          if (NAVCode.Token == "CurrPage")
          {
            testFunction = true;
            isControlFunction = false;
          }
          else if (testFunction && (NAVCode.tokenType == TokenType.IsNAVVar))
          {
            if (NAVCode.Token == "ACTIVATE" || NAVCode.Token == "ACTIVE" ||
                NAVCode.Token == "CLOSE" || NAVCode.Token == "EDITABLE" ||
                NAVCode.Token == "ENABLED" || NAVCode.Token == "VISIBLE" ||
                NAVCode.Token == "HEIGHT" || NAVCode.Token == "UPDATECONTROLS" ||
                NAVCode.Token == "UPDATEEDITABLE" || NAVCode.Token == "UPDATEFONTBOLD" ||
                NAVCode.Token == "UPDATEFORECOLOR" || NAVCode.Token == "UPDATEINDENT" ||
                NAVCode.Token == "WIDTH" || NAVCode.Token == "XPOS" ||
                NAVCode.Token == "YPOS" || NAVCode.Token == "UPDATE")
            {
              // log unsupported encountered;
              if (NAVCode.Token == "EDITABLE" || NAVCode.Token == "UPDATE")
                unSupportedFunctionExists = isControlFunction;
              else
                unSupportedFunctionExists = true;

              if (unSupportedFunctionExists)
              {
                if (unsupportedProperty.Length > 0)
                {
                  unsupportedProperty.Append(", ");
                }

                unsupportedProperty.Append(NAVCode.Token);
              }

              testFunction = false;
            }
            else
            {
              isControlFunction = true;
            }
          }

          if (!isTrigger && unSupportedFunctionExists && (NAVCode.xPos == 2) && (NAVCode.Token == "END"))
          {
            string log = string.Format(CultureInfo.InvariantCulture,
              Resources.UnsupportedProperty,
              unsupportedProperty.ToString(),
              triggerNode.InnerText.Substring(startFrom - 1, NAVCode.position - startFrom + NAVCode.Token.Length + 1));
            TransformationLog.GenericLogEntry(log, LogCategory.PossibleCompilationProblem, metaDataDocMgt.GetCurrentPageId);

            unSupportedFunctionExists = false;
            startFrom = 0;
          }

          if (!isTrigger && (NAVCode.xPos == 2) && (NAVCode.Token == "BEGIN") && (startFrom == 0))
            break;
        }

        if (unSupportedFunctionExists && isTrigger)
        {
          string log = string.Format(CultureInfo.InvariantCulture, Resources.UnsupportedProperty, unsupportedProperty.ToString(), triggerNode.InnerText);
          TransformationLog.GenericLogEntry(log, LogCategory.PossibleCompilationProblem, metaDataDocMgt.GetCurrentPageId);
        }

        unSupportedFunctionExists = false;
        // logSuccess
      }
    }

    private static Boolean HasToken(XmlNode triggerNode, String token)
    {
      Boolean inCode = false;
      SimpleNAVCodeParser sNAVCodeParser = new SimpleNAVCodeParser();
      foreach (NormalizedCode NAVCode in sNAVCodeParser.GenerateNormalisedCode(triggerNode.InnerText))
      {
        if (!inCode)
          if (NAVCode.Token == "BEGIN")
            inCode = true;
        if (inCode)
          if (NAVCode.Token == token)
            return true;
      }
      return false;
    }

    private static void MoveOnAfterValidateToOnValidate(XmlNode triggerNode)
    {
      String procedureName = GetStrippedSourceExpr(triggerNode.ParentNode.ParentNode) + "OnAfterValidate";
      String newCode =
        "\r\nLOCAL PROCEDURE " + procedureName + "@" +
        metaDataDocMgt.CalcId(null, procedureName, "Code") +
        "();\r\n" +
        triggerNode.InnerText;

      // newCode = newCode.Replace("\r\n  ", "\r\n");
      UpdateMoveToTriggerDocument(
        Convert.ToString(metaDataDocMgt.GetCurrentPageId, CultureInfo.InvariantCulture),
        "",
        "Code", "",
        newCode,
        "append");

      XmlNode onValidateNode = triggerNode.ParentNode.ParentNode.SelectSingleNode(@"./a:Triggers/a:OnValidate", metaDataDocMgt.XmlNamespaceMgt);
      if (onValidateNode != null)
      {
        if (HasToken(onValidateNode, "EXIT"))
        {
          procedureName = GetStrippedSourceExpr(triggerNode.ParentNode.ParentNode) + "OnValidate";
          newCode =
            "\r\nLOCAL PROCEDURE " + procedureName + "@" +
            metaDataDocMgt.CalcId(null, procedureName, "Code") +            
            "();\r\n" +
            onValidateNode.InnerText;

          // newCode = newCode.Replace("\r\n  ", "\r\n");
          UpdateMoveToTriggerDocument(
            Convert.ToString(metaDataDocMgt.GetCurrentPageId, CultureInfo.InvariantCulture),
            "",
            "Code", "",
            newCode,
            "append");

          onValidateNode.RemoveAll();

          UpdateMoveToTriggerDocument(
            Convert.ToString(metaDataDocMgt.GetCurrentPageId, CultureInfo.InvariantCulture),
            GetProperty(triggerNode.ParentNode.ParentNode, "ID"),
            "OnValidate", "",
            "\r\n" + GetStrippedSourceExpr(triggerNode.ParentNode.ParentNode) + "OnValidate;",
            "append");
        }
      }

      UpdateMoveToTriggerDocument(
        Convert.ToString(metaDataDocMgt.GetCurrentPageId, CultureInfo.InvariantCulture),
        triggerNode.ParentNode.ParentNode.SelectSingleNode(@".//a:ID", metaDataDocMgt.XmlNamespaceMgt).LastChild.Value,
        "OnValidate", "",
        //   "\r\nVALIDATE(" + sourceExpr + ");" +
        "\r\n" + GetStrippedSourceExpr(triggerNode.ParentNode.ParentNode) + "OnAfterValidate;",
        "append");
    }

    private static void MoveOnPushToOnQueryCloseForm(XmlNode triggerNode)
    {
      String pushActionValue = GetProperty(triggerNode.ParentNode.ParentNode, "PushAction");

      if (pushActionValue == null)
      {
        String captionML = GetProperty(triggerNode.ParentNode.ParentNode, "CaptionML");
        switch (captionML)
        {
          case "ENU=OK":
            pushActionValue = "LookupOK";
            break;
          case "ENU=Cancel":
            pushActionValue = "LookupCancel";
            break;
          case "ENU=Yes":
            pushActionValue = "Yes";
            break;
          case "ENU=No":
            pushActionValue = "No";
            break;
          default:
            pushActionValue = "";
            break;
        }
      }

      String procedureName = pushActionValue + "OnPush";
      String newCode =
        "\r\nLOCAL PROCEDURE " + procedureName + "@" +
        metaDataDocMgt.CalcId(null, procedureName, "Code") + 
        "();\r\n" +
        triggerNode.InnerText;

      // newCode = newCode.Replace("\r\n  ", "\r\n");
      UpdateMoveToTriggerDocument(
        Convert.ToString(metaDataDocMgt.GetCurrentPageId, CultureInfo.InvariantCulture),
        "",
        "Code", "",
        newCode,
        "append");

      if ((pushActionValue == "OK") || (pushActionValue == "Cancel"))
      {
        UpdateMoveToTriggerDocument(
          Convert.ToString(metaDataDocMgt.GetCurrentPageId, CultureInfo.InvariantCulture),
          "",
          "OnQueryCloseForm", "",
          //   "\r\nVALIDATE(" + sourceExpr + ");" +
          "\r\nIF CloseAction IN [Action::" + pushActionValue + ",Action::Lookup" + pushActionValue + "] THEN\r\n    " + pushActionValue + "OnPush;",
          "prepend");
      }
      else
      {
        UpdateMoveToTriggerDocument(
          Convert.ToString(metaDataDocMgt.GetCurrentPageId, CultureInfo.InvariantCulture),
          "",
          "OnQueryCloseForm", "",
          // "\r\nVALIDATE(" + sourceExpr + ");" +
          "\r\nIF CloseAction = Action::" + pushActionValue + " THEN\r\n    " + pushActionValue + "OnPush;",
          "prepend");
      }
    }

    private static void MoveOnPushToOnValidate(XmlNode triggerNode)
    {
      String namePrefix = GetStrippedSourceExpr(triggerNode.ParentNode.ParentNode);
      if (namePrefix.Length > 24)
        namePrefix = namePrefix.Remove(24);
      String procedureName = namePrefix + "OnPush";
      String newCode =
        "\r\nLOCAL PROCEDURE " + procedureName + "@" +
        metaDataDocMgt.CalcId(null, procedureName, "Code") + 
        "();\r\n" +
        triggerNode.InnerText;

      // newCode = newCode.Replace("\r\n  ", "\r\n");
      UpdateMoveToTriggerDocument(
        Convert.ToString(metaDataDocMgt.GetCurrentPageId, CultureInfo.InvariantCulture),
        "",
        "Code", "",
        newCode,
        "append");

      UpdateMoveToTriggerDocument(
        Convert.ToString(metaDataDocMgt.GetCurrentPageId, CultureInfo.InvariantCulture),
        triggerNode.ParentNode.ParentNode.SelectSingleNode(@".//a:ID", metaDataDocMgt.XmlNamespaceMgt).LastChild.Value,
        "OnValidate", "",
        //   "\r\nVALIDATE(" + sourceExpr + ");" +
        "\r\n" + namePrefix + "OnPush;",
        "prepend");
    }

    public static void MoveOptionButtonControlOnValidate(String id, String toId)
    {
      XmlNode optionCntrolNode = metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(
        @".//a:Controls//a:Control[./a:Properties/a:ID='" + id + "']", metaDataDocMgt.XmlNamespaceMgt);

      XmlNode onPushNode = optionCntrolNode.SelectSingleNode(@"./a:Triggers/a:OnPush", metaDataDocMgt.XmlNamespaceMgt);
      if (onPushNode != null)
      {
        MoveOnPushToOnValidate(onPushNode);
        PerformMoveToTriggerActions(false);
        onPushNode.RemoveAll();
        onPushNode.ParentNode.RemoveChild(onPushNode);
        // PerformMoveToTriggerActions(false);   
      }

      MoveOptionEditableToOnValidate(optionCntrolNode);
      PerformMoveToTriggerActions(false);

      XmlNode OnValidateNode = optionCntrolNode.SelectSingleNode("./a:Triggers/a:OnValidate", metaDataDocMgt.XmlNamespaceMgt);
      if (OnValidateNode == null)
        return;

      MoveOptionOnValidateToOnValidate(OnValidateNode, toId);
      OnValidateNode.RemoveAll();
      OnValidateNode.ParentNode.RemoveChild(OnValidateNode);
      PerformMoveToTriggerActions(false);
    }

    private static void MoveOptionEditableToOnValidate(XmlNode optionControlNode)
    {
      String sourceExpr = GetProperty(optionControlNode, "SourceExpr");
      String enabledValue = GetProperty(optionControlNode, "Enabled");
      String editableValue = GetProperty(optionControlNode, "Editable");
      String visibleValue = GetProperty(optionControlNode, "Visible");
      String idValue = GetProperty(optionControlNode, "ID");

      // TODO: refactor this part; create Text666 dynamicly; move Const value to resource file.
      if (enabledValue != null && enabledValue != "No" && enabledValue != "Yes")
      {
        UpdateMoveToPropertyDocument(
          Convert.ToString(metaDataDocMgt.GetCurrentPageId, CultureInfo.InvariantCulture),
          "", "", "", "Text666", "TextConst 'ENU=%1 is not a valid selection.'", "");

        UpdateMoveToTriggerDocument(
          Convert.ToString(metaDataDocMgt.GetCurrentPageId, CultureInfo.InvariantCulture),
          idValue,
          "OnValidate", "",
          "\r\nIF NOT (" + enabledValue + ") THEN\r\n  ERROR(Text666, " + sourceExpr + ");",
          "prepend");

        XmlNode enabledNode = optionControlNode.SelectSingleNode("./a:Properties/a:Enabled", metaDataDocMgt.XmlNamespaceMgt);
        enabledNode.ParentNode.RemoveChild(enabledNode);
      }

      if (editableValue != null && editableValue != "No" && editableValue != "Yes")
      {
        UpdateMoveToPropertyDocument(
          Convert.ToString(metaDataDocMgt.GetCurrentPageId, CultureInfo.InvariantCulture),
          "", "", "", "Text666", "TextConst 'ENU=%1 is not a valid selection.'", "");

        UpdateMoveToTriggerDocument(
          Convert.ToString(metaDataDocMgt.GetCurrentPageId, CultureInfo.InvariantCulture),
          idValue,
          "OnValidate", "",
          "\r\nIF NOT (" + editableValue + ") THEN\r\n  ERROR(Text666, " + sourceExpr + ");",
          "prepend");

        XmlNode editableNode = optionControlNode.SelectSingleNode("./a:Properties/a:Editable", metaDataDocMgt.XmlNamespaceMgt);
        editableNode.ParentNode.RemoveChild(editableNode);
      }

      if (visibleValue != null && visibleValue != "No" && visibleValue != "Yes")
      {
        UpdateMoveToPropertyDocument(
          Convert.ToString(metaDataDocMgt.GetCurrentPageId, CultureInfo.InvariantCulture),
          "", "", "", "Text666", "TextConst 'ENU=%1 is not a valid selection.'", "");

        UpdateMoveToTriggerDocument(
          Convert.ToString(metaDataDocMgt.GetCurrentPageId, CultureInfo.InvariantCulture),
          idValue,
          "OnValidate", "",
          "\r\nIF NOT (" + visibleValue + ") THEN\r\n  ERROR(Text666, " + sourceExpr + ");",
          "prepend");

        XmlNode visibleNode = optionControlNode.SelectSingleNode("./a:Properties/a:Visible", metaDataDocMgt.XmlNamespaceMgt);
        visibleNode.ParentNode.RemoveChild(visibleNode);
      }
    }

    private static void MoveOptionOnValidateToOnValidate(XmlNode triggerNode, String toID)
    {
      String sourceExpr = GetProperty(triggerNode.ParentNode.ParentNode, "SourceExpr");
      String optionValue = GetProperty(triggerNode.ParentNode.ParentNode, "OptionValue");

      XmlNode toOptionCntrlNode =
        metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(
          @".//a:Controls//a:Control[./a:Properties/a:ID='" + toID + "']",
          metaDataDocMgt.XmlNamespaceMgt);

      String procedureName = GetStrippedSourceExpr(triggerNode.ParentNode.ParentNode) + "OnValidate";
      String newCode =
        "\r\nLOCAL PROCEDURE " +
        procedureName + "@" +
        metaDataDocMgt.CalcId(null, procedureName, "Code") + 
        "();\r\n" +
        triggerNode.InnerText;

      // newCode = newCode.Replace("\r\n  ", "\r\n");
      UpdateMoveToTriggerDocument(
        Convert.ToString(metaDataDocMgt.GetCurrentPageId, CultureInfo.InvariantCulture),
        "",
        "Code", "",
        newCode,
        "append");

      UpdateMoveToTriggerDocument(
        Convert.ToString(metaDataDocMgt.GetCurrentPageId, CultureInfo.InvariantCulture),
        toOptionCntrlNode.SelectSingleNode(@".//a:ID", metaDataDocMgt.XmlNamespaceMgt).LastChild.Value,
        "OnValidate", "",
        "\r\nIF " + sourceExpr + " = " + sourceExpr + "::\"" + optionValue + "\" THEN\r\n  " + GetStrippedSourceExpr(triggerNode.ParentNode.ParentNode) + "OnValidate;",
        "prepend");
    }

    private static void MoveOnAfterGetCurrRecToOnAfterGetRec(XmlNode triggerNode)
    {
      int freeFunctionId = metaDataDocMgt.CalcId(null, "OnAfterGetCurrRecord", "Code");  
      {
        // ToDo: I think it should be rewrited with SendCodeToProcedure or similar function -- it could situation when be 2 controls will create the same function
        StringBuilder newCode = new StringBuilder("\r\nLOCAL PROCEDURE OnAfterGetCurrRecord@");
        newCode.Append(freeFunctionId++);
        newCode.Append("();\r\n");
        int myPosition = triggerNode.InnerText.IndexOf("BEGIN", StringComparison.OrdinalIgnoreCase) + 5;
        newCode.Append(triggerNode.InnerText.Substring(0, myPosition));
        newCode.Append("\r\n  xRec := Rec;");
        newCode.Append(triggerNode.InnerText.Substring(myPosition));

        UpdateMoveToTriggerDocument(
          Convert.ToString(metaDataDocMgt.GetCurrentPageId, CultureInfo.InvariantCulture),
          "",
          "Code", "",
          newCode.ToString(),
          "append");
      }

      XmlNode OnNewRecordNode = metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(@".//a:Triggers/a:OnNewRecord", metaDataDocMgt.XmlNamespaceMgt);
      if (OnNewRecordNode != null)
      {
        if (HasToken(triggerNode, "EXIT"))
        {
          StringBuilder newCode = new StringBuilder("\r\nLOCAL PROCEDURE OnNewRecord@");
          newCode.Append(metaDataDocMgt.CalcId(null, "OnNewRecord", "Code"));
          newCode.Append("(BelowxRec: Boolean);\r\n");
          newCode.Append(OnNewRecordNode.InnerText);

          UpdateMoveToTriggerDocument(
            Convert.ToString(metaDataDocMgt.GetCurrentPageId, CultureInfo.InvariantCulture),
            "",
            "Code", "",
            newCode.ToString(),
            "append");

          OnNewRecordNode.RemoveAll();

          UpdateMoveToTriggerDocument(
            Convert.ToString(metaDataDocMgt.GetCurrentPageId, CultureInfo.InvariantCulture),
            "",
            "OnNewRecord", "",
            "OnNewRecord(BelowxRec);",
            "append");
        }
      }
      UpdateMoveToTriggerDocument(
        Convert.ToString(metaDataDocMgt.GetCurrentPageId, CultureInfo.InvariantCulture),
        "",
        "OnNewRecord", "",
        "\r\nOnAfterGetCurrRecord;",
        "append");

      XmlNode OnAfterGetRecordNode = metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(@".//a:Triggers/a:OnAfterGetRecord", metaDataDocMgt.XmlNamespaceMgt);
      if (OnAfterGetRecordNode != null)
      {
        if (HasToken(triggerNode, "EXIT"))
        {
          SendCodeToProcedure("OnAfterGetRecord", OnAfterGetRecordNode.InnerText);

          OnAfterGetRecordNode.RemoveAll();

          UpdateMoveToTriggerDocument(
            Convert.ToString(metaDataDocMgt.GetCurrentPageId, CultureInfo.InvariantCulture),
            "",
            "OnAfterGetRecord", "",
            "OnAfterGetRecord;",
            "append");
        }
      }
      UpdateMoveToTriggerDocument(
        Convert.ToString(metaDataDocMgt.GetCurrentPageId, CultureInfo.InvariantCulture),
        "",
        "OnAfterGetRecord", "",
        "\r\nOnAfterGetCurrRecord;",
        "append");
    }

    private static void MoveOnFormatToOnAfterGetRecord(XmlNode triggerNode)
    {
      String newCode;
      Boolean hasText = HasToken(triggerNode, "Text");
      Boolean notEvaluateText = false;
      if (hasText)
      {
        notEvaluateText = GetProperty(triggerNode.ParentNode.ParentNode, "Editable") == "No";
        if (!notEvaluateText)
          notEvaluateText = GetProperty(triggerNode.ParentNode, "Editable") == "No";
        if (!notEvaluateText)
          notEvaluateText = GetProperty(triggerNode.ParentNode.ParentNode, "Editable") == "No";

        String procedureName = GetStrippedSourceExpr(triggerNode.ParentNode.ParentNode) + (notEvaluateText ? "Text" : "") + "OnFormat";
        Int32 nextVariableNo = metaDataDocMgt.CalcId(null, procedureName, "Code");

        newCode =
          "\r\nLOCAL PROCEDURE " + procedureName + "@" +
          nextVariableNo +
          "(" + (notEvaluateText ? "VAR" : "") + " Text@" + metaDataDocMgt.CalcId(null, procedureName + "@Text", "Code") + " : Text[1024]);\r\n" +
          triggerNode.InnerText;

        if (notEvaluateText)
        {
          UpdateMoveToPropertyDocument(
            Convert.ToString(metaDataDocMgt.GetCurrentPageId, CultureInfo.InvariantCulture),
            GetProperty(triggerNode.ParentNode.ParentNode, "ID"),
            "SourceExpr",
            GetStrippedSourceExpr(triggerNode.ParentNode.ParentNode) + "Text",
            GetStrippedSourceExpr(triggerNode.ParentNode.ParentNode) + "Text",
            "Text[1024] INDATASET", "");

          if ((GetProperty(triggerNode.ParentNode.ParentNode, "CationML") == null) &&
              !(FindVariable(GetProperty(triggerNode.ParentNode.ParentNode, "SourceExpr"),
                metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(@".//a:Code", metaDataDocMgt.XmlNamespaceMgt).InnerText)))
            UpdateMoveToPropertyDocument(
              Convert.ToString(metaDataDocMgt.GetCurrentPageId, CultureInfo.InvariantCulture),
              GetProperty(triggerNode.ParentNode.ParentNode, "ID"),
              "CaptionClass",
              "FIELDCAPTION(" + GetProperty(triggerNode.ParentNode.ParentNode, "SourceExpr") + ")",
              "",
              "", "");
        }

        //if (!notEvaluateText)
        //  UpdateMoveToTriggerDocument(
        //    Convert.ToString(metaDataDocMgt.GetCurrentPageID, CultureInfo.InvariantCulture),
        //    GetProperty(triggerNode.ParentNode.ParentNode, "ID"),
        //    "OnValidate", "",
        //    "\r\nEVALUATE(" + GetProperty(triggerNode.ParentNode.ParentNode, "SourceExpr") + "," + GetStrippedSourceExpr(triggerNode) + "Text" + ");",
        //    "prepend");
      }
      else
      {
        String procedureName = GetStrippedSourceExpr(triggerNode.ParentNode.ParentNode) + "OnFormat";
        Int32 nextVariableNo = metaDataDocMgt.CalcId(null, procedureName, "Code");

        newCode =
          "\r\nLOCAL PROCEDURE " + procedureName + "@" +
          nextVariableNo +
          "();\r\n" +
          triggerNode.InnerText;
      }

      // newCode = newCode.Replace("\r\n  ", "\r\n");
      UpdateMoveToTriggerDocument(
        Convert.ToString(metaDataDocMgt.GetCurrentPageId, CultureInfo.InvariantCulture),
        "",
        "Code", "",
        newCode,
        "append");

      String codeOnAfterGetRecord = "";
      XmlNode OnAfterGetRecordNode = metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(@".//a:Triggers/a:OnAfterGetRecord", metaDataDocMgt.XmlNamespaceMgt);
      if (OnAfterGetRecordNode != null)
      {
        if (HasToken(triggerNode, "EXIT"))
        {
          SendCodeToProcedure("OnAfterGetRecord", OnAfterGetRecordNode.InnerText);

          OnAfterGetRecordNode.RemoveAll();

          codeOnAfterGetRecord = "\r\nOnAfterGetRecord;";
        }
      }

      if (hasText)
      {
        if (notEvaluateText)
          codeOnAfterGetRecord =
            codeOnAfterGetRecord +
            "\r\n" + GetStrippedSourceExpr(triggerNode.ParentNode.ParentNode) + "Text := " +
            "FORMAT(" + GetProperty(triggerNode.ParentNode.ParentNode, "SourceExpr") + ");" +
            "\r\n" + GetStrippedSourceExpr(triggerNode.ParentNode.ParentNode) + "TextOnFormat" +
            "(" + GetStrippedSourceExpr(triggerNode.ParentNode.ParentNode) + "Text);";
        else
          codeOnAfterGetRecord =
            codeOnAfterGetRecord +
            "\r\n" + GetStrippedSourceExpr(triggerNode.ParentNode.ParentNode) + "OnFormat" +
            "(FORMAT(" + GetProperty(triggerNode.ParentNode.ParentNode, "SourceExpr") + "));";
      }
      else
        codeOnAfterGetRecord =
          codeOnAfterGetRecord +
          "\r\n" + GetStrippedSourceExpr(triggerNode.ParentNode.ParentNode) + "OnFormat;";

      UpdateMoveToTriggerDocument(
        Convert.ToString(metaDataDocMgt.GetCurrentPageId, CultureInfo.InvariantCulture),
        "",
        "OnAfterGetRecord", "",
        codeOnAfterGetRecord,
        "append");
    }

    private static void SendCodeToProcedure(string functionName, string functionCode)
    {
      XmlNode code = metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(@"./a:Code", metaDataDocMgt.XmlNamespaceMgt);
      if (code == null)
      {
        return;
      }

      if (functionName.Length > 30)
      {
        functionName = functionName.Remove(30);
      }

      int pos = code.InnerText.IndexOf("LOCAL PROCEDURE " + functionName, StringComparison.OrdinalIgnoreCase);
      if (pos == -1)
      {
        StringBuilder newProcedureCode = new StringBuilder("\r\nLOCAL PROCEDURE ");
        newProcedureCode.Append(functionName);
        newProcedureCode.Append("@");
        newProcedureCode.Append((metaDataDocMgt.CalcId(null, functionName, "Code")));
        newProcedureCode.Append("();\r\n");
        newProcedureCode.Append(functionCode);
        
        UpdateMoveToTriggerDocument(
          Convert.ToString(metaDataDocMgt.GetCurrentPageId, CultureInfo.InvariantCulture),
          "",
          "Code",
          "",
          newProcedureCode.ToString(),
          "append");
      }
      else
      {
        StringBuilder tmpBuilder = new StringBuilder("\r\n;");

        tmpBuilder.Append("\r\n");
        
        tmpBuilder.Append(SendCodeToProcedure_PrepareFunctionCode(functionCode,functionName));
        tmpBuilder.Append(";\r\n");

        System.Text.RegularExpressions.Regex beginOrEnd = new System.Text.RegularExpressions.Regex(@"\bBEGIN\b|\bEND\b", System.Text.RegularExpressions.RegexOptions.Compiled | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        System.Text.RegularExpressions.Match matchBeginOrEnd;
        int beginsCount = 0;

        matchBeginOrEnd = beginOrEnd.Match(code.InnerText, pos);
        

        while (matchBeginOrEnd.Success)
        {
          pos = matchBeginOrEnd.Index;
          if (matchBeginOrEnd.Value.Equals("BEGIN", StringComparison.OrdinalIgnoreCase))
          {
            beginsCount++;
          }
          else
          {
            beginsCount--;
          }

          if (beginsCount == 0)
          {
            break;
          }

          matchBeginOrEnd = matchBeginOrEnd.NextMatch();
        }

        code.InnerText = code.InnerText.Insert(pos, tmpBuilder.ToString());
      }
    }

    private static string SendCodeToProcedure_PrepareFunctionCode(string functionCode, string functionName)
    {
      if (string.IsNullOrEmpty(functionCode) || string.IsNullOrEmpty(functionName))
        return functionCode;
      functionCode = functionCode.Replace(functionName, string.Empty);

      System.Text.RegularExpressions.Regex beginOrEnd = new System.Text.RegularExpressions.Regex(@"\bBEGIN\b|\bEND\b", System.Text.RegularExpressions.RegexOptions.Compiled | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
      System.Text.RegularExpressions.Match matchBeginOrEnd;
      int beginsCount = 0;

      matchBeginOrEnd = beginOrEnd.Match(functionCode);

      if ((matchBeginOrEnd.Success))
      {
        beginsCount = 1;
        int pos = matchBeginOrEnd.Index;
        functionCode = functionCode.Substring(0, pos) + functionCode.Substring(pos + 5);
        functionCode = functionCode.Replace("\r\n;\r\n", "\r\n");
        matchBeginOrEnd = beginOrEnd.Match(functionCode);
      }

      while (matchBeginOrEnd.Success)
      {
        if (matchBeginOrEnd.Value.Equals("BEGIN", StringComparison.OrdinalIgnoreCase))
        {
          beginsCount++;
        }
        else
        {
          beginsCount--;
        }

        if (beginsCount == 0)
        {
          int pos = matchBeginOrEnd.Index;
          functionCode = functionCode.Substring(0, pos) + functionCode.Substring(pos + 3);
          functionCode = functionCode.Replace("\r\n;\r\n", "\r\n");

          break;
        }

        matchBeginOrEnd = matchBeginOrEnd.NextMatch();
      }

      return functionCode;
    }

    private static void MoveOnBeforeInput(XmlNode triggerNode)
    {
      String procedureName = GetStrippedSourceExpr(triggerNode.ParentNode.ParentNode) + "OnBeforeInput";
      Int32 nextVariableNo = metaDataDocMgt.CalcId(null, procedureName, "Code");

      String newCode =
        "\r\nLOCAL PROCEDURE " + procedureName + "@" +
        nextVariableNo +
        "();\r\n" +
        triggerNode.InnerText;

      // newCode = newCode.Replace("\r\n  ", "\r\n");
      UpdateMoveToTriggerDocument(
        Convert.ToString(metaDataDocMgt.GetCurrentPageId, CultureInfo.InvariantCulture),
        "",
        "Code", "",
        newCode,
        "append");
    }

    private static void MoveOnAfterInput(XmlNode triggerNode)
    {
      String procedureName = GetStrippedSourceExpr(triggerNode.ParentNode.ParentNode) + "OnAfterInput";
      Int32 nextVariableNo = metaDataDocMgt.CalcId(null, procedureName, "Code");
      if (nextVariableNo < 1000)
        nextVariableNo = 1000;

      String newCode =
        "\r\nLOCAL PROCEDURE " + procedureName + "@" +
        nextVariableNo +
        "(VAR Text@" + (nextVariableNo + 1) + " : Text[1024]);\r\n" +
        triggerNode.InnerText;

      // newCode = newCode.Replace("\r\n  ", "\r\n");
      UpdateMoveToTriggerDocument(
        Convert.ToString(metaDataDocMgt.GetCurrentPageId, CultureInfo.InvariantCulture),
        "",
        "Code", "",
        newCode,
        "append");
    }

    private static void MoveOnInputChange(XmlNode triggerNode)
    {     
      String procedureName = GetStrippedSourceExpr(triggerNode.ParentNode.ParentNode) + "OnInputChange";
      Int32 nextVariableNo = metaDataDocMgt.CalcId(null, procedureName, "Code");

      if (nextVariableNo < 1000)
        nextVariableNo = 1000;

      String newCode =
        "\r\nLOCAL PROCEDURE " + procedureName + "@" +
        nextVariableNo +
        "(VAR Text@" + metaDataDocMgt.CalcId(null, "Text", "Code") + " : Text[1024]);\r\n" +
        triggerNode.InnerText;

      // newCode = newCode.Replace("\r\n  ", "\r\n");
      UpdateMoveToTriggerDocument(
        Convert.ToString(metaDataDocMgt.GetCurrentPageId, CultureInfo.InvariantCulture),
        "",
        "Code", "",
        newCode,
        "append");
    }

    private static String GetStrippedSourceExpr(XmlNode cntrlNode)
    {
      String optionValue = GetProperty(cntrlNode, "OptionValue");
      String sourceExpr = GetProperty(cntrlNode, "SourceExpr");
      String newOptionValue = optionValue;
      String newSourceExpr = sourceExpr;
      if (optionValue != null)
      {
        newOptionValue = GetAlphaNumeric(optionValue);
        if (newOptionValue.Length > 15)
          newOptionValue = newOptionValue.Remove(15);
        newSourceExpr = newOptionValue + newSourceExpr;
      }

      if (sourceExpr == null)
      {
        String nameNode = GetProperty(cntrlNode, "Name");
        if (nameNode != null)
          newSourceExpr = nameNode;
        else
          newSourceExpr = "Control" + GetProperty(cntrlNode, "ID");
      }
      else
      {
        // TODO: We have problem if control has: <SourceExpr>STRSUBSTNO('Resource Card %1',ResourceCode)</SourceExpr>.
        //       Original problem -- trasformation crash. Now it don't crash but search dont't really work (Count==0).
        string patchedSourceExpr = CleanValueForXQuery(sourceExpr);
        string patchedOptionValue = CleanValueForXQuery(optionValue);
        patchedOptionValue = (optionValue == null ? "" : " and ./Properties/a:OptionValue='" + optionValue + "'");
        string quwery = string.Format(CultureInfo.InvariantCulture, ".//a:Control[./a:Properties/a:SourceExpr='{0}' {1}]", patchedSourceExpr, patchedOptionValue);
        XmlNodeList anotherNodeHasSameSourceExpr = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(quwery, metaDataDocMgt.XmlNamespaceMgt);

        //XmlNodeList anotherNodeHasSameSourceExpr = metaDataDocMgt.XmlCurrentFormNode.SelectNodes(
        //   @".//a:Control[./a:Properties/a:SourceExpr=" + pathedsourceExpr +
        //  (optionValue == null ? "" : " and ./Properties/a:OptionValue='" + optionValue + "'") + "]"
        //  , metaDataDocMgt.XmlNamespaceMgt);
        if (anotherNodeHasSameSourceExpr.Count > 1)
          newSourceExpr = newSourceExpr + "C" + GetProperty(cntrlNode, "ID");
      }
      return GetAlphaNumeric(newSourceExpr);
    }

    private static String CleanValueForXQuery(string value)
    {
      if (string.IsNullOrEmpty(value))
      {
        return value;
      }

      if (value.Contains("'"))
        value = value.Replace("'", "&apos;");

      return value;
    }

    private static String GetAlphaNumeric(String stringExpr)
    {
      StringBuilder strippedSourceExpr = new StringBuilder();
      bool begining = true;

      stringExpr = RemoveNonSignificantSymbols(stringExpr);

      foreach (char c in stringExpr)
      {
        bool isAlphaNumber = ((c >= 'a') && (c <= 'z')) ||
                   ((c >= 'A') && (c <= 'Z')) ||
                   ((c >= '0') && (c <= '9')) ||
                   (c == '_');
        if (isAlphaNumber)
        {
          if (!((c >= '0') && (c <= '9')))
          {
            begining = false;
            strippedSourceExpr = strippedSourceExpr.Append(c);
          }
          else
          {
            if (!begining)
            {
              strippedSourceExpr = strippedSourceExpr.Append(c);
            }
          }
        }
        else
        {
          if (!begining)
          {
            strippedSourceExpr = strippedSourceExpr.Append((int)c);
          }
        }
      }
      return strippedSourceExpr.ToString();
    }

    private static String RemoveNonSignificantSymbols(String stringExpr)
    {
      stringExpr = stringExpr.Replace("\"", string.Empty);
      stringExpr = stringExpr.Replace(" ", string.Empty);
      stringExpr = stringExpr.Replace(".", string.Empty);
      stringExpr = stringExpr.Replace("-", string.Empty);
      stringExpr = stringExpr.Replace("_", string.Empty);
      stringExpr = stringExpr.Replace("(", string.Empty);
      stringExpr = stringExpr.Replace(")", string.Empty);
      stringExpr = stringExpr.Replace("[", string.Empty);
      stringExpr = stringExpr.Replace("]", string.Empty);
      stringExpr = stringExpr.Replace("/", string.Empty);
      stringExpr = stringExpr.Replace("\\", string.Empty);
      return stringExpr;
    }

    private static String GetProperty(XmlNode controlNode, String propertyName)
    {
      XmlNode propNode = controlNode.SelectSingleNode(@"./a:Properties/a:" + propertyName, metaDataDocMgt.XmlNamespaceMgt);
      if (propNode == null)
        return null;
      return propNode.LastChild.InnerText;
    }

    private static void MoveOnActivate(XmlNode triggerNode)
    {
      String procedureName = GetStrippedSourceExpr(triggerNode.ParentNode.ParentNode) + "OnActivate";
      Int32 nextVariableNo = metaDataDocMgt.CalcId(null, procedureName, "Code");

      String newCode =
        "\r\nLOCAL PROCEDURE " + procedureName + "@" +
        nextVariableNo +
        "();\r\n" +
        triggerNode.InnerText;

      // newCode = newCode.Replace("\r\n  ", "\r\n");
      UpdateMoveToTriggerDocument(
        Convert.ToString(metaDataDocMgt.GetCurrentPageId, CultureInfo.InvariantCulture),
        "",
        "Code", "",
        newCode,
        "append");
    }

    private static void MoveOnDeactivate(XmlNode triggerNode)
    {
      String procedureName = GetStrippedSourceExpr(triggerNode.ParentNode.ParentNode) + "OnDeactivate";
      Int32 nextVariableNo = metaDataDocMgt.CalcId(null, procedureName, "Code");

      String newCode =
        "\r\nLOCAL PROCEDURE " + procedureName + "@" +
        nextVariableNo +
        "();\r\n" +
        triggerNode.InnerText;

      // newCode = newCode.Replace("\r\n  ", "\r\n");
      UpdateMoveToTriggerDocument(
        Convert.ToString(metaDataDocMgt.GetCurrentPageId, CultureInfo.InvariantCulture),
        "",
        "Code", "",
        newCode,
        "append");
    }

    private static void MoveOnActivateFormToOnOpenForm(XmlNode triggerNode)
    {
      Int32 nextVariableNo = metaDataDocMgt.CalcId(null, "OnActivateForm", "Code");

      String newCode =
        "\r\nLOCAL PROCEDURE OnActivateForm@" +
        nextVariableNo +
        "();\r\n" +
        triggerNode.InnerText;

      // newCode = newCode.Replace("\r\n  ", "\r\n");
      UpdateMoveToTriggerDocument(
        Convert.ToString(metaDataDocMgt.GetCurrentPageId, CultureInfo.InvariantCulture),
        "",
        "Code", "",
        newCode,
        "append");

      UpdateMoveToTriggerDocument(
        Convert.ToString(metaDataDocMgt.GetCurrentPageId, CultureInfo.InvariantCulture),
        "",
        "OnOpenForm", "",
        "\r\n  OnActivateForm;",
        "append");
    }

    private static void MoveOnDeactivateForm(XmlNode triggerNode)
    {
      // MetaDataDocumentManagement metaDataDocMgt = MetaDataDocumentManagement.Instance;
      Int32 nextVariableNo = metaDataDocMgt.CalcId(null, "OnDeactivateForm", "Code");

      String newCode =
        "\r\nLOCAL PROCEDURE OnDeactivateForm@" +
        nextVariableNo +
        "();\r\n" +
        triggerNode.InnerText;

      // newCode = newCode.Replace("\r\n  ", "\r\n");
      UpdateMoveToTriggerDocument(
        Convert.ToString(metaDataDocMgt.GetCurrentPageId, CultureInfo.InvariantCulture),
        "",
        "Code", "",
        newCode,
        "append");
    }

    private static void MoveOnBeforePutRecord(XmlNode triggerNode)
    {
      Int32 nextVariableNo = metaDataDocMgt.CalcId(null, "OnBeforePutRecord", "Code");

      String newCode =
        "\r\nLOCAL PROCEDURE OnBeforePutRecord@" +
        nextVariableNo +
        "();\r\n" +
        triggerNode.InnerText;

      // String newCode = newCode.Replace("\r\n  ", "\r\n");
      UpdateMoveToTriggerDocument(
        Convert.ToString(metaDataDocMgt.GetCurrentPageId, CultureInfo.InvariantCulture),
        "",
        "Code", "",
        newCode,
        "append");
    }

    private static void MoveOnTimer(XmlNode triggerNode)
    {
      Int32 nextVariableNo = metaDataDocMgt.CalcId(null, "OnTimer", "Code");

      String newCode =
        "\r\nLOCAL PROCEDURE OnTimer@" +
        nextVariableNo +
        "();\r\n" +
        triggerNode.InnerText;

      // newCode = newCode.Replace("\r\n  ", "\r\n");
      UpdateMoveToPropertyDocument(
        Convert.ToString(metaDataDocMgt.GetCurrentPageId, CultureInfo.InvariantCulture),
        "", "TimerUpdate", "1", "", "", "");

      UpdateMoveToTriggerDocument(
        Convert.ToString(metaDataDocMgt.GetCurrentPageId, CultureInfo.InvariantCulture),
        "",
        "Code", "",
        newCode,
        "append");
    }

    private static void MoveOnHyperlink(XmlNode triggerNode)
    {
      Int32 nextVariableNo = metaDataDocMgt.CalcId(null, "OnHyperlink", "Code");
      if (nextVariableNo < 1000)
        nextVariableNo = 1000;

      String newCode =
        "\r\nLOCAL PROCEDURE OnHyperlink@" +
        nextVariableNo +
        "(URL@" + metaDataDocMgt.CalcId(null, "URL", "Code") + " : Text[1024]);\r\n" +
        triggerNode.InnerText;

      // newCode = newCode.Replace("\r\n  ", "\r\n");
      UpdateMoveToTriggerDocument(
        Convert.ToString(metaDataDocMgt.GetCurrentPageId, CultureInfo.InvariantCulture),
        "",
        "Code", "",
        newCode,
        "append");
    }

    private static void MoveOnCreateHyperlink(XmlNode triggerNode)
    {
      Int32 nextVariableNo = metaDataDocMgt.CalcId(null, "OnCreateHyperlink", "Code");
      if (nextVariableNo < 1000)
        nextVariableNo = 1000;

      String newCode =
        "\r\nLOCAL PROCEDURE OnCreateHyperlink@" +
        nextVariableNo +
        "(URL@" + metaDataDocMgt.CalcId(null, "URL", "Code") + " : Text[1024]);\r\n" +
        triggerNode.InnerText;

      // newCode = newCode.Replace("\r\n  ", "\r\n");
      UpdateMoveToTriggerDocument(
        Convert.ToString(metaDataDocMgt.GetCurrentPageId, CultureInfo.InvariantCulture),
        "",
        "Code", "",
        newCode,
        "append");
    }

    private static void DeclareVariable(XmlNode triggerNode, String varName, String varType, String varProp)
    {
      Int32 positionToAdd = 4;
      String beginningPart = "VAR";
      String endPart = triggerNode.InnerText;
      if (FindVariableDeclaration(triggerNode, ref positionToAdd))
      {
        beginningPart = triggerNode.InnerText.Substring(0, positionToAdd - 1);
        endPart = triggerNode.InnerText.Substring(positionToAdd - 1);
      }
      triggerNode.RemoveAll();

      String newPart;
      if (varProp.Length == 0)
      {
        newPart = String.Format(
            CultureInfo.InvariantCulture, "  {0}@{1} : {2};", varName, metaDataDocMgt.CalcId(null, varName, "Code"), varType);
      }
      else
      {
        newPart = String.Format(
            CultureInfo.InvariantCulture, "  {0}@{1} : {2}; {3};", varName, metaDataDocMgt.CalcId(null, varName, "Code") ,varType, varProp);
      }

      XmlCDataSection data =
        metaDataDocMgt.XmlDocument.CreateCDataSection(
           String.Format(
             CultureInfo.InvariantCulture, "{0}\r\n{1}",
             beginningPart, newPart + (beginningPart == "VAR" ? "\r\n" : "") + endPart));

      triggerNode.AppendChild(data);
    }

    private static void InsertCode(String triggerName, XmlNode triggerNode, String procedure, String newCode, String where)
    {
      String previousToken = null;
      if ((triggerNode != null) && !String.IsNullOrEmpty(triggerNode.InnerText))
      {
        Boolean inProcedure = false;
        Boolean inCodeBody = false;
        String procedureName = "";
        Boolean nextIsProcedureName = false;
        Int32 beginPos = 0;
        Int32 procedurePos = 0;
        Int32 endPos = 0;
        Int32 parameterPos = 0;
        String parameters = "";
        Boolean hasReturnValue = true;
        Boolean nextIsParameterVar = false;
        Int32 parameterEndsAt = 0;

        SimpleNAVCodeParser sNAVCodeParser1 = new SimpleNAVCodeParser();
        foreach (NormalizedCode normNAVCode in sNAVCodeParser1.GenerateNormalisedCode(triggerNode.InnerText))
        {
          if (where == "prepend")
          {
            if (normNAVCode.Token == "BEGIN")
            {
              String beginningPart =
                triggerNode.InnerText.Substring(0, normNAVCode.position + normNAVCode.Token.Length - 1);
              String endPart =
                triggerNode.InnerText.Substring(normNAVCode.position + normNAVCode.Token.Length - 1);
              triggerNode.RemoveAll();

              XmlCDataSection data = metaDataDocMgt.XmlDocument.CreateCDataSection(
                String.Format(CultureInfo.InvariantCulture, "{0}\r\n  {1}{2}",
                beginningPart, newCode, endPart));
              triggerNode.AppendChild(data);
              break;
            }
          }
          else
          {
            if (triggerName == "Code")
            {
              UpdateProcedureState(
                 normNAVCode,
                 ref inProcedure, ref inCodeBody,
                 ref beginPos, ref procedurePos,
                 ref endPos, ref parameterPos,
                 ref parameters, ref hasReturnValue,
                 ref procedureName, ref nextIsProcedureName,
                 ref nextIsParameterVar, ref parameterEndsAt);

              if ((normNAVCode.xPos == 2) && !inProcedure && (procedureName.Length == procedure.Length))
                if (((normNAVCode.Token == "BEGIN") && String.IsNullOrEmpty(procedure)) ||
                    ((normNAVCode.Token == "END") &&
                     (procedureName.StartsWith(procedure, StringComparison.Ordinal))))
                {
                  String beginningPart = triggerNode.InnerText.Substring(0, normNAVCode.position - 3);
                  String endPart = triggerNode.InnerText.Substring(normNAVCode.position - 3);
                  triggerNode.RemoveAll();

                  // newCode = newCode.Replace("\r\n", "\r\n  ");

                  //if (previousToken != ";")
                  //  newCode = ";" + newCode;
                  XmlCDataSection data = metaDataDocMgt.XmlDocument.CreateCDataSection(
                    String.Format(CultureInfo.InvariantCulture, "{0}{1}{2}",
                    beginningPart, newCode, endPart));
                  triggerNode.AppendChild(data);
                  break;
                }
            }
            else
              if (normNAVCode.Token == "END")
                if (normNAVCode.position + 5 == triggerNode.InnerText.Length)
                {
                  String beginningPart = triggerNode.InnerText.Substring(0, normNAVCode.position - 3);
                  String endPart = triggerNode.InnerText.Substring(normNAVCode.position - 3);
                  triggerNode.RemoveAll();

                  newCode = newCode.Replace("\r\n", "\r\n  ");

                  if (previousToken != ";")
                    newCode = ";" + newCode;
                  XmlCDataSection data = metaDataDocMgt.XmlDocument.CreateCDataSection(
                    String.Format(CultureInfo.InvariantCulture, "{0}{1}{2}",
                    beginningPart, newCode, endPart));
                  triggerNode.AppendChild(data);
                  break;
                }
            previousToken = normNAVCode.Token;
          }
        }
      }
      else
      {
        XmlCDataSection data = metaDataDocMgt.XmlDocument.CreateCDataSection(
          String.Format(CultureInfo.InvariantCulture, "BEGIN{0}\r\nEND;\r\n", newCode));
        triggerNode.AppendChild(data);
      }
    }

    public static void PerformMoveToPropertyActions()
    {
      Dictionary<String, String> variables = new Dictionary<String, String>();

      XmlNodeList pageMoveActionList =
        moveToPropertyDoc.SelectNodes(@".//Move[./Form='" + metaDataDocMgt.GetCurrentPageId + "']");

      foreach (XmlNode pageMoveAction in pageMoveActionList)
      {
        XmlNode controlNameNode = pageMoveAction.SelectSingleNode(@"./ControlName");
        XmlNode propertyNode = pageMoveAction.SelectSingleNode(@"./Property");
        XmlNode propertyValueNode = pageMoveAction.SelectSingleNode(@"./PropertyValue");
        XmlNode declareVariableNameNode = pageMoveAction.SelectSingleNode(@"./DeclareVariableName");
        XmlNode declareVariableTypeNode = pageMoveAction.SelectSingleNode(@"./DeclareVariableType");
        XmlNode declareVariablePropertyNode = pageMoveAction.SelectSingleNode(@"./DeclareVariableProperty");
        // XmlNode procedureNode = pageMoveAction.SelectSingleNode(@"./Procedure");

        if (declareVariableNameNode.FirstChild.Value.Length != 0)
        {
          // Add DECLAREVARIABLE
          if (!variables.ContainsKey(declareVariableNameNode.FirstChild.Value))
          {
            variables.Add(
              declareVariableNameNode.FirstChild.Value, declareVariableNameNode.FirstChild.Value);
            // declared successfully add INITIALISE
            DeclareVariable(
              metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(
                @".//a:Code", metaDataDocMgt.XmlNamespaceMgt),
              declareVariableNameNode.FirstChild.Value,
              declareVariableTypeNode.FirstChild.Value,
              declareVariablePropertyNode.FirstChild.Value);

          }
        }

        XmlNode formControlNode;
        Boolean initializeProperty = false;

        String initializeTrigger = "OnInit";
        String initializeValue = "TRUE";

        if (controlNameNode.FirstChild.Value.Length == 0)
        {
          formControlNode =
            metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(
              @".//a:Properties", metaDataDocMgt.XmlNamespaceMgt);
        }
        else
        {
          formControlNode =
            metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(
              @".//a:Properties[./a:SourceExpr='" + controlNameNode.FirstChild.Value + "']",
              metaDataDocMgt.XmlNamespaceMgt);
          if (formControlNode == null)
            formControlNode =
              metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(
                @".//a:Properties[./a:Name='" + controlNameNode.FirstChild.Value.Trim('\"') + "']",
                metaDataDocMgt.XmlNamespaceMgt);
          if (formControlNode == null)
            formControlNode =
              metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(
                @".//a:Properties[./a:ID='" + controlNameNode.FirstChild.Value + "']",
                metaDataDocMgt.XmlNamespaceMgt);
        }
        if (formControlNode != null)
        {
          if (propertyNode.FirstChild.Value.Length != 0)
          {
            // Add Indentation PROPERTY
            if (propertyNode.FirstChild.Value == "IndentationColumnName")
            {
              XmlNode parentNode = formControlNode.ParentNode.ParentNode.SelectSingleNode(
                @"./a:Properties", metaDataDocMgt.XmlNamespaceMgt);

              XmlNode indentationNode =
                parentNode.SelectSingleNode(
                  @"./a:IndentationColumnName", metaDataDocMgt.XmlNamespaceMgt);
              if (indentationNode != null)
              {
                indentationNode.FirstChild.InnerText = propertyValueNode.FirstChild.Value;
              }
              else
              {
                parentNode.AppendChild(
                  XmlUtility.CreateXmlElement(
                    propertyNode.FirstChild.Value, propertyValueNode.FirstChild.Value));

                parentNode.AppendChild(
                  XmlUtility.CreateXmlElement(
                    "IndentationControls", controlNameNode.FirstChild.Value));

                initializeProperty = true;
                initializeTrigger = "OnAfterGetRecord";
                initializeValue = "0";
              }
            }
            else
            {
              XmlNode editableNode =
                formControlNode.SelectSingleNode(
                  @"./a:" + propertyNode.FirstChild.Value, metaDataDocMgt.XmlNamespaceMgt);
              if (editableNode != null)
              {
                initializeProperty = editableNode.FirstChild.Value == "Yes";
                if (propertyValueNode.FirstChild.Value == " ")
                  editableNode.ParentNode.RemoveChild(editableNode);
                else
                  editableNode.FirstChild.InnerText = propertyValueNode.FirstChild.Value;
              }
              else
              {
                formControlNode.AppendChild(
                  XmlUtility.CreateXmlElement(
                    propertyNode.FirstChild.Value, propertyValueNode.FirstChild.Value));
                initializeProperty =
                  (propertyNode.FirstChild.Value == "Visible") ||
                  (propertyNode.FirstChild.Value == "Enabled") ||
                  (propertyNode.FirstChild.Value == "Editable") ||
                  (propertyNode.FirstChild.Value == "HideValue");

                if (propertyNode.FirstChild.Value == "HideValue")
                {
                  initializeTrigger = "OnAfterGetRecord";
                  initializeValue = "FALSE";
                }
              }
            }
          }
        }
        if (initializeProperty)
        {
          formControlNode =
            metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(
                @".//a:Triggers/a:" + initializeTrigger, metaDataDocMgt.XmlNamespaceMgt);
          if (formControlNode != null)
            InsertCode(
              initializeTrigger,
              metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(
                @".//a:Triggers/a:" + initializeTrigger, metaDataDocMgt.XmlNamespaceMgt),
              "",
              String.Format(CultureInfo.InvariantCulture,
                "{0} := {1};", declareVariableNameNode.FirstChild.Value, initializeValue),
              "prepend"
            );
          else
          {
            formControlNode =
              metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(
                  @".//a:Triggers", metaDataDocMgt.XmlNamespaceMgt);
            if (formControlNode != null)
              formControlNode.AppendChild(XmlUtility.CreateXmlElement(initializeTrigger, null));

            InsertCode(
              initializeTrigger,
              metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(
                @".//a:Triggers/a:" + initializeTrigger, metaDataDocMgt.XmlNamespaceMgt),
              "",
              String.Format(CultureInfo.InvariantCulture,
                "\r\n  {0} := {1};", declareVariableNameNode.FirstChild.Value, "TRUE"),
              "prepend"
              );
          }
        }
        pageMoveAction.RemoveAll();
      }
    }

    public static Boolean PerformMoveToTriggerActions(Boolean isFinal)
    {
      XmlNodeList pageMoveActionList;
      if (!isFinal)
        pageMoveActionList =
          moveToTriggerDoc.SelectNodes(
            @".//Move[./Form='" + metaDataDocMgt.GetCurrentPageId + "' and ./Procedure='']");
      else
        pageMoveActionList =
          moveToTriggerDoc.SelectNodes(
            @".//Move[./Form='" + metaDataDocMgt.GetCurrentPageId + "']");
      
      if (pageMoveActionList.Count == 0)
        return false;

      foreach (XmlNode pageMoveAction in pageMoveActionList)
      {
        XmlNode controlNameNode = pageMoveAction.SelectSingleNode(@"./ControlName");
        XmlNode TriggerNode = pageMoveAction.SelectSingleNode(@"./Trigger");
        XmlNode TriggerCodeNode = pageMoveAction.SelectSingleNode(@"./TriggerCode");
        XmlNode Where = pageMoveAction.SelectSingleNode(@"./Where");
        XmlNode procedure = pageMoveAction.SelectSingleNode(@"./Procedure");

        XmlNode formControlNode;
        if (controlNameNode.FirstChild.Value.Length == 0)
        {
          formControlNode =
            metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(
                @".//a:" + TriggerNode.FirstChild.Value, metaDataDocMgt.XmlNamespaceMgt);
          if (formControlNode != null)
            InsertCode(
              TriggerNode.FirstChild.Value,
              formControlNode,
              procedure.FirstChild.Value,
              TriggerCodeNode.FirstChild.Value,
              Where.FirstChild.Value
              );
          else
          {
            formControlNode =
              metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(
                  @".//a:Triggers",
                  metaDataDocMgt.XmlNamespaceMgt);
            if (formControlNode != null)
              formControlNode.AppendChild(
                XmlUtility.CreateXmlElement(
                  TriggerNode.FirstChild.Value,
                  null));

            InsertCode(
              TriggerNode.FirstChild.Value,
              metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(
                @".//a:Triggers/a:" + TriggerNode.FirstChild.Value, metaDataDocMgt.XmlNamespaceMgt),
              procedure.FirstChild.Value,
              TriggerCodeNode.FirstChild.Value.Replace("\r\n", "\r\n  "),
              Where.FirstChild.Value
              );
          }
        }
        else
        {
          formControlNode =
            metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(
              @".//a:Properties[./a:SourceExpr='" + controlNameNode.FirstChild.Value + "']",
              metaDataDocMgt.XmlNamespaceMgt);
          if (formControlNode == null)
            formControlNode =
              metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(
                @".//a:Properties[./a:Name='" + controlNameNode.FirstChild.Value.Trim('\"') + "']",
                metaDataDocMgt.XmlNamespaceMgt);
          if (formControlNode == null)
            formControlNode =
              metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(
                @".//a:Properties[./a:ID='" + controlNameNode.FirstChild.Value + "']",
                metaDataDocMgt.XmlNamespaceMgt);
          if (formControlNode != null)
          {
            XmlNode controlNode = formControlNode.ParentNode;
            XmlNode triggersNode = controlNode.SelectSingleNode(
              @".//a:Triggers", metaDataDocMgt.XmlNamespaceMgt);
            if (triggersNode == null)
            {
              // formControlNode = controlNode;
              controlNode.AppendChild(XmlUtility.CreateXmlElement("Triggers", null));
              triggersNode = controlNode.SelectSingleNode(
                  @"./a:Triggers", metaDataDocMgt.XmlNamespaceMgt);
            }
            XmlNode triggerNode = triggersNode.SelectSingleNode(
                  @"./a:" + TriggerNode.FirstChild.Value, metaDataDocMgt.XmlNamespaceMgt);
            if (triggerNode != null)
            {
              InsertCode(
                TriggerNode.FirstChild.Value,
                triggerNode,
                "",
                TriggerCodeNode.FirstChild.Value.Replace("\r\n", "\r\n  "),
                Where.FirstChild.Value
                );
            }
            else
            {
              triggersNode.AppendChild(XmlUtility.CreateXmlElement(TriggerNode.FirstChild.Value, null));

              triggerNode = triggersNode.SelectSingleNode(
                    @"./a:" + TriggerNode.FirstChild.Value, metaDataDocMgt.XmlNamespaceMgt);

              InsertCode(
                TriggerNode.FirstChild.Value,
                triggerNode,
                "",
                TriggerCodeNode.FirstChild.Value.Replace("\r\n", "\r\n  "),
                Where.FirstChild.Value
                );
            }
          }
        }
        pageMoveAction.RemoveAll();
      }

      return true;
    }

    public static void LookupModePropertyToOnInitTrigger()
    {
      XmlNode lookupModeProperty = metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(@"./a:Properties/a:LookupMode", metaDataDocMgt.XmlNamespaceMgt);

      if (lookupModeProperty != null)
      {
        XmlNode triggerNode = metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(@"./a:Triggers/a:OnInit", metaDataDocMgt.XmlNamespaceMgt);
        string triggerCode = "CurrPage.LOOKUPMODE := " + lookupModeProperty.InnerText + "; \r\n ";
        
        if (triggerNode == null)
        {
          XmlNode triggersNode = metaDataDocMgt.XmlCurrentFormNode.SelectSingleNode(@"./a:Triggers", metaDataDocMgt.XmlNamespaceMgt);
          triggersNode.AppendChild(XmlUtility.CreateXmlElement("OnInit"));
          
          triggerNode = triggersNode.LastChild;
          triggerCode = "\r\n  " + triggerCode;
        }
        
        InsertCode("OnInit",triggerNode,"",triggerCode,"prepend");
      }
    }

    public static void TransformFormTriggers(XmlNode formNode)
    {
      metaDataDocMgt.XmlCurrentFormNode = formNode;

      TransformTriggerCode(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Triggers/a:OnAfterValidate", metaDataDocMgt.XmlNamespaceMgt), true, true);
      TransformTriggerCode(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Triggers/a:OnAfterGetRecord", metaDataDocMgt.XmlNamespaceMgt), false, true);
      TransformTriggerCode(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Triggers/a:OnAfterGetCurrRecord", metaDataDocMgt.XmlNamespaceMgt), true, true);
      TransformTriggerCode(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Triggers/a:OnDeactivate", metaDataDocMgt.XmlNamespaceMgt), true, true);
      TransformTriggerCode(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Triggers/a:OnActivate", metaDataDocMgt.XmlNamespaceMgt), true, true);
      TransformTriggerCode(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Triggers/a:OnDeactivateForm", metaDataDocMgt.XmlNamespaceMgt), true, true);
      TransformTriggerCode(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Triggers/a:OnActivateForm", metaDataDocMgt.XmlNamespaceMgt), true, true);
      TransformTriggerCode(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Triggers/a:OnQueryCloseForm", metaDataDocMgt.XmlNamespaceMgt), false, true);
      TransformTriggerCode(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Triggers/a:OnAfterInput", metaDataDocMgt.XmlNamespaceMgt), true, true);
      TransformTriggerCode(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Triggers/a:OnBeforePutRecord", metaDataDocMgt.XmlNamespaceMgt), true, true);
      TransformTriggerCode(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Triggers/a:OnBeforeInput", metaDataDocMgt.XmlNamespaceMgt), true, true);

      TransformTriggerCode(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Triggers/a:OnInit", metaDataDocMgt.XmlNamespaceMgt), false, true);
      TransformTriggerCode(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Triggers/a:OnTimer", metaDataDocMgt.XmlNamespaceMgt), true, true);
      TransformTriggerCode(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Triggers/a:OnCreateHyperlink", metaDataDocMgt.XmlNamespaceMgt), true, true);
      TransformTriggerCode(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Triggers/a:OnHyperlink", metaDataDocMgt.XmlNamespaceMgt), true, true);

      TransformTriggerCode(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Triggers/a:OnInputChange", metaDataDocMgt.XmlNamespaceMgt), true, true);
      TransformTriggerCode(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Triggers/a:OnFindRecord", metaDataDocMgt.XmlNamespaceMgt), false, true);
      TransformTriggerCode(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Triggers/a:OnNextRecord", metaDataDocMgt.XmlNamespaceMgt), false, true);
      TransformTriggerCode(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Triggers/a:OnNewRecord", metaDataDocMgt.XmlNamespaceMgt), false, true);
      TransformTriggerCode(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Triggers/a:OnInsertRecord", metaDataDocMgt.XmlNamespaceMgt), false, true);
      TransformTriggerCode(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Triggers/a:OnModifyRecord", metaDataDocMgt.XmlNamespaceMgt), false, true);
      TransformTriggerCode(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Triggers/a:OnDeleteRecord", metaDataDocMgt.XmlNamespaceMgt), false, true);

      TransformTriggerCode(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Triggers/a:OnAssistEdit", metaDataDocMgt.XmlNamespaceMgt), false, true);
      TransformTriggerCode(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Triggers/a:OnDrillDown", metaDataDocMgt.XmlNamespaceMgt), false, true);
      TransformTriggerCode(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Triggers/a:OnValidate", metaDataDocMgt.XmlNamespaceMgt), false, true);
      TransformTriggerCode(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Triggers/a:OnLookup", metaDataDocMgt.XmlNamespaceMgt), false, true);
      TransformTriggerCode(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Triggers/a:OnOpenForm", metaDataDocMgt.XmlNamespaceMgt), false, true);
      TransformTriggerCode(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Triggers/a:OnCloseForm", metaDataDocMgt.XmlNamespaceMgt), false, true);
      TransformTriggerCode(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(
        @".//a:Control[./a:Properties/a:Controltype='CheckBox' or 
                       ./a:Properties/a:Controltype='OptionButton' or 
                       ./a:Properties/a:Controltype='Indicator' or 
                       ./a:Properties/a:Controltype='PictureBox']/a:Triggers/a:OnPush", metaDataDocMgt.XmlNamespaceMgt), true, true);
      TransformTriggerCode(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(
        @".//a:Control[./a:Properties/a:Controltype='CommandButton' and (
                       ./a:Properties/a:PushAction='LookupOK' or 
                       ./a:Properties/a:PushAction='LookupCancel' or 
                       ./a:Properties/a:PushAction='Cancel' or 
                       ./a:Properties/a:PushAction='Yes' or
                       ./a:Properties/a:PushAction='No' or 
                       ./a:Properties/a:PushAction='OK' or 
                       ./a:Properties/a:PushAction='Close' or
                       ./a:Properties/a:PushAction='LookupTable' or  
                       ./a:Properties/a:PushAction='RunSystem' or  
                       ./a:Properties/a:PushAction='Close')]/a:Triggers/a:OnPush", metaDataDocMgt.XmlNamespaceMgt), true, true);
      TransformTriggerCode(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Triggers/a:OnPush", metaDataDocMgt.XmlNamespaceMgt), false, true);
      TransformTriggerCode(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Triggers/a:OnFormat", metaDataDocMgt.XmlNamespaceMgt), true, true);

      TransformTriggerCode(metaDataDocMgt.XmlCurrentFormNode.SelectNodes(@".//a:Code", metaDataDocMgt.XmlNamespaceMgt), false, false);

      if (PerformMoveToTriggerActions(false))
        TransformFormTriggers(formNode);
    }

    private static void TransformTriggerCode(XmlNodeList triggerNodeList, bool unSupportedTrigger, bool isTrigger)
    {
      foreach (XmlNode triggerNode in triggerNodeList)
        FindAndReplaceCode(triggerNode, unSupportedTrigger, isTrigger);
    }

    public static void SummarizeActions()
    {
      foreach (KeyValuePair<int, CodeTransformationRule> codeTransRule in GetRules())
      {
        String logStr = String.Format(CultureInfo.InvariantCulture,
          Resources.SummarizeCodeTransformationAction,
          codeTransRule.Value.matchesFound,
          codeTransRule.Value.find);
        TransformationLog.GenericLogEntry(logStr, LogCategory.GeneralInformation, 0);
      }
    }
  }
}