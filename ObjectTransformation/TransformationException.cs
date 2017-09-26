//--------------------------------------------------------------------------
// <copyright file="TransformtionException.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>?abc?</summary>
//--------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Tools.FormTransformation
{
  /// <summary>
  /// ?abc?
  /// </summary>
  [Serializable] 
  public class TransformationException : Exception
  {
    /// <summary>
    /// ?abc?
    /// </summary>
    /// <param name="message">?abc?</param>
    public TransformationException(String message)
      : base(message)
    {
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    /// <param name="message">?abc?</param>
    /// <param name="innerException">?abc?</param>
    public TransformationException(String message, Exception innerException)
      : base(message, innerException)
    {
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    public TransformationException()
    {
    }

    /// <summary>
    /// ?abc?
    /// </summary>
    /// <param name="info">?abc?</param>
    /// <param name="context">?abc?</param>
    protected TransformationException(SerializationInfo info, StreamingContext context) :
      base(info, context) 
    {
    }
  }
}
