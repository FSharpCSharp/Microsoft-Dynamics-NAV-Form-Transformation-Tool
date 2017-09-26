using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Dynamics.Nav.Tools.FormTransformation
{
  enum TokenType
  {
    IsSpace,
    IsString,
    IsMLComment,
    IsSLComment,
    IsNAVVarInQuotes,
    IsNAVVar,
    IsSymbol,
    IsSemiColon,
    IsAnyNAVVar,
    IsDeclaredVar,
    IsCurrForm,
    IsCurrControl
  }

  internal class NormalizedCode
  {
    public int  position;
    public string Token;
    public TokenType tokenType;
    public int variableNo;
    public int xPos;
  }
}
