using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Dynamics.Nav.Tools.FormTransformation
{
  internal class CodeTransformationRule
  {
    public String find;
    public String replace;
    public StringBuilder comment = new StringBuilder();
    public StringBuilder log = new StringBuilder();
    public StringBuilder atTrigger = new StringBuilder();

    public StringBuilder moveToProperty = new StringBuilder();
    public StringBuilder moveValueToProperty = new StringBuilder();
    public StringBuilder movePropertyToControlName = new StringBuilder();

    public StringBuilder moveToTrigger = new StringBuilder();
    public String moveCodeToTrigger;

    public StringBuilder declareVariable = new StringBuilder();
    public StringBuilder declareVariableType = new StringBuilder();
    public StringBuilder declareVarialeProperty = new StringBuilder();

    public StringBuilder isReport = new StringBuilder();
    public int matchesFound;
    public List<NormalizedCode> normalizedCode = new List<NormalizedCode>();
  }
}
