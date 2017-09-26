using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.Tools.FormTransformation
{
  internal class SimpleNAVCodeParser
  {
    public List<NormalizedCode> GenerateNormalisedCode(string NAVCode)
    {
      NormalizedCode token = new NormalizedCode();
      List<NormalizedCode> normalizedCode = new List<NormalizedCode>();

      int position = 0;
      int xPosition = 0;
      foreach (char c in NAVCode)
      {
        position++;
        bool isAlphaNumber = ((c >= 'a') & (c <= 'z')) | 
                             ((c >= 'A') & (c <= 'Z')) |
                             ((c >= '0') & (c <= '9')) |
                             (c == '_');
        bool isEOF = (c == '\n') || (c == '\r') || (position == NAVCode.Length);
        if (isEOF)
          xPosition = 0;
        xPosition++;
        if (!((c == ' ') & (token.tokenType == TokenType.IsSpace)))
          UpdateToken(normalizedCode, token, c, isAlphaNumber, isEOF, position, xPosition);
      }

      return normalizedCode;
    }

    [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Requires refactoring, but time doesn’t permit refactoring at this point")]
    void UpdateToken(List<NormalizedCode> normalizedCode ,NormalizedCode token, char c, bool isAlphaNumeric, bool isEOF, int position, int xPos)
    {
      switch (token.tokenType)
      {
        case TokenType.IsMLComment:
          {
            if (c.Equals('}'))
            {
              token.tokenType = TokenType.IsSpace;
            }
            break;
          }
        case TokenType.IsNAVVarInQuotes:
          {
            if (c.Equals('"'))
            {
              token.Token = token.Token + c;
              NormalizedCode nm = new NormalizedCode();
              nm.position = token.position;
              nm.xPos = token.xPos;
              nm.Token = token.Token;
              nm.tokenType = token.tokenType;
              normalizedCode.Add(nm);
              token.tokenType = TokenType.IsSpace;
            }
            else
              token.Token = token.Token + c;
            break;
          }
        case TokenType.IsAnyNAVVar:
          {
            if (c.Equals('!'))
            {
              token.Token = token.Token + c;
              NormalizedCode nm = new NormalizedCode();
              nm.position = token.position;
              nm.Token = token.Token;
              nm.xPos = token.xPos;
              if (nm.Token == "!declaredVariable!")
                nm.tokenType = TokenType.IsDeclaredVar;
              else if (nm.Token == "!currForm!")
                nm.tokenType = TokenType.IsCurrForm;
              else if (nm.Token == "!control!")
                nm.tokenType = TokenType.IsCurrControl;
              else
                nm.tokenType = token.tokenType;
              nm.variableNo = token.variableNo;
              normalizedCode.Add(nm);
              token.tokenType = TokenType.IsSpace;
              token.variableNo = 0;
            }
            else
            {
              token.Token = token.Token + c;
              if ((c >= '0') && (c <= '9'))
                token.variableNo = Convert.ToInt16(Convert.ToString(Convert.ToString(token.variableNo, CultureInfo.InvariantCulture) + c, CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
              else
                if (token.variableNo != 0)
                  token.variableNo = 0;
            }
            break;
          }
        case TokenType.IsNAVVar:
          {
            if (c == ' ')
            {
              NormalizedCode nm = new NormalizedCode();
              nm.position = token.position;
              nm.xPos = token.xPos;
              nm.Token = token.Token;
              nm.tokenType = token.tokenType;
              normalizedCode.Add(nm);
              token.tokenType = TokenType.IsSpace;
              token.Token = "";
              break;
            }
            if (!isAlphaNumeric)
            {
              NormalizedCode nm = new NormalizedCode();
              nm.position = token.position;
              nm.xPos = token.xPos;
              nm.Token = token.Token;
              nm.tokenType = token.tokenType;
              normalizedCode.Add(nm);

              token.tokenType = TokenType.IsSpace;
              token.Token = "";
              UpdateToken(normalizedCode, token, c, isAlphaNumeric, isEOF, position, xPos);
              break;
            }
            else if (isEOF)
            {
              token.Token = token.Token + c;
              NormalizedCode nm = new NormalizedCode();
              nm.position = token.position;
              nm.xPos = token.xPos;
              nm.Token = token.Token;
              nm.tokenType = token.tokenType;
              normalizedCode.Add(nm);
              token.Token = "";
              token.tokenType = TokenType.IsSpace;
              break;
            }
            token.Token = token.Token + c;
            break;
          }
        case TokenType.IsSLComment:
          {
            if (isEOF)
              token.tokenType = TokenType.IsSpace;
            break;
          }
        case TokenType.IsString:
          {
            if (c.Equals('\''))
            {
              token.Token = token.Token + c;
              NormalizedCode nm = new NormalizedCode();
              nm.position = token.position;
              nm.xPos = token.xPos;
              nm.Token = token.Token;
              nm.tokenType = token.tokenType;
              normalizedCode.Add(nm);
              token.Token = "";
              token.tokenType = TokenType.IsSpace;
            }
            else
              token.Token = token.Token + c;
            break;
          }
        case TokenType.IsSemiColon:
          {
            token.Token = token.Token + c;
            NormalizedCode nm = new NormalizedCode();
            nm.position = token.position;
            nm.xPos = token.xPos;
            nm.Token = token.Token;
            nm.tokenType = token.tokenType;
            normalizedCode.Add(nm);
            token.Token = "";
            token.tokenType = TokenType.IsSpace;
            break;
          }
        case TokenType.IsSymbol:
          {
            if ((token.Token.Equals("/")) & (c == '/'))
            {
              token.Token = "";
              token.tokenType = TokenType.IsSLComment;
            }
            else if (c == ' ')
            {
              NormalizedCode nm = new NormalizedCode();
              nm.position = token.position;
              nm.xPos = token.xPos;
              nm.Token = token.Token;
              nm.tokenType = token.tokenType;
              normalizedCode.Add(nm);
              token.Token = "";
              token.tokenType = TokenType.IsSpace;
            }
            else if (c == '"')
            {
              NormalizedCode nm = new NormalizedCode();
              nm.position = token.position;
              nm.xPos = token.xPos;
              nm.Token = token.Token;
              nm.tokenType = token.tokenType;
              normalizedCode.Add(nm);
              token.Token = "";
              token.tokenType = TokenType.IsSpace;
              UpdateToken(normalizedCode, token, c, isAlphaNumeric, isEOF, position, xPos);
            }
            else if (c == ';')
            {
              NormalizedCode nm = new NormalizedCode();
              nm.position = token.position;
              nm.xPos = token.xPos;
              nm.Token = token.Token;
              nm.tokenType = token.tokenType;
              normalizedCode.Add(nm);
              token.Token = "";
              token.tokenType = TokenType.IsSpace;
              UpdateToken(normalizedCode, token, c, isAlphaNumeric, isEOF, position, xPos);
            }
            else if (c == '\'')
            {
              NormalizedCode nm = new NormalizedCode();
              nm.position = token.position;
              nm.xPos = token.xPos;
              nm.Token = token.Token;
              nm.tokenType = token.tokenType;
              normalizedCode.Add(nm);
              token.Token = "";
              token.tokenType = TokenType.IsSpace;
              UpdateToken(normalizedCode, token, c, isAlphaNumeric, isEOF, position, xPos);
            }
            else if (c == '!')
            {
              NormalizedCode nm = new NormalizedCode();
              nm.position = token.position;
              nm.xPos = token.xPos;
              nm.Token = token.Token;
              nm.tokenType = token.tokenType;
              normalizedCode.Add(nm);
              token.Token = "";
              token.tokenType = TokenType.IsSpace;
              UpdateToken(normalizedCode, token, c, isAlphaNumeric, isEOF, position, xPos);
            }
            else if (isAlphaNumeric)
            {
              NormalizedCode nm = new NormalizedCode();
              nm.position = token.position;
              nm.xPos = token.xPos;
              nm.Token = token.Token;
              nm.tokenType = token.tokenType;
              normalizedCode.Add(nm);
              token.Token = "";
              token.tokenType = TokenType.IsSpace;
              UpdateToken(normalizedCode, token, c, isAlphaNumeric, isEOF, position, xPos);
            }
            else if (isEOF)
            {
              if ((c != '\r') && (c != '\n'))
                token.Token = token.Token + c;
              NormalizedCode nm = new NormalizedCode();
              nm.position = token.position;
              nm.xPos = token.xPos;
              nm.Token = token.Token;
              nm.tokenType = token.tokenType;
              normalizedCode.Add(nm);
              token.Token = "";
              token.tokenType = TokenType.IsSpace;
            }
            else
            {
              token.Token = token.Token + c;
            }
            break;
          }
        case TokenType.IsSpace:
          {
            token.Token = "";
            switch (c)
            {
              case '{':
                {
                  token.tokenType = TokenType.IsMLComment;
                  return;
                }
              case '/':
                {
                  token.tokenType = TokenType.IsSymbol;
                  token.Token = Convert.ToString(c, CultureInfo.InvariantCulture);
                  token.xPos = xPos;
                  token.position = position;
                  return;
                }
              case '"':
                {
                  token.tokenType = TokenType.IsNAVVarInQuotes;
                  token.Token = Convert.ToString(c, CultureInfo.InvariantCulture);
                  token.xPos = xPos;
                  token.position = position;
                  return;
                }
              case '!':
                {
                  token.tokenType = TokenType.IsAnyNAVVar;
                  token.Token = Convert.ToString(c, CultureInfo.InvariantCulture);
                  token.xPos = xPos;
                  token.position = position;
                  return;
                }
              case '\'':
                {
                  token.position = position;
                  token.tokenType = TokenType.IsString;
                  token.Token = Convert.ToString(c, CultureInfo.InvariantCulture);
                  token.xPos = xPos;
                  token.position = position;
                  return;
                }
            }
            if (isAlphaNumeric)
            {
              token.tokenType = TokenType.IsNAVVar;
              token.xPos = xPos;
              token.position = position;
              token.Token = "";
              UpdateToken(normalizedCode, token, c, isAlphaNumeric, isEOF, position, xPos);
            }
            else if ((c == '\r') || (c == '\n'))
            {
            }
            else if (c == ';')
            {
              token.tokenType = TokenType.IsSemiColon;
              token.xPos = xPos;
              token.position = position;
              token.Token = "";
              UpdateToken(normalizedCode, token, c, isAlphaNumeric, isEOF, position, xPos);
            }
            else
            {
              token.tokenType = TokenType.IsSymbol;
              token.xPos = xPos;
              token.position = position;
              UpdateToken(normalizedCode, token, c, isAlphaNumeric, isEOF, position, xPos);
            }
          }
          break;
      }
    }
  }
}
