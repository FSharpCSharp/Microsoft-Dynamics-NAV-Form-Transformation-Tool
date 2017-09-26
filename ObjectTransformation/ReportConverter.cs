using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.Tools.FormTransformation
{
  public class ReportConverter
  {
    private XmlDocument document;
    private Dictionary<String, XmlElement> reportElements = new Dictionary<String, XmlElement>();

    /// <summary>
    /// Class used to support reports inside FormTransformation 
    /// </summary>
    /// <param name="sourceDocument">The import xml delivered from CSide</param>
    public ReportConverter(XmlDocument sourceDocument)
    {
      this.document = (XmlDocument)sourceDocument.Clone();
    }

    /// <summary>
    /// This delivers the input to form transformation
    /// </summary>
    /// <returns>XmlDocument compliant with form transformation</returns>
    public XmlDocument ReturnSourceForms()
    {
      XmlNamespaceManager xmlNamespace = new XmlNamespaceManager(document.NameTable);
      xmlNamespace.AddNamespace("a", "urn:schemas-microsoft-com:dynamics:NAV:ApplicationObjects");

      foreach (XmlElement report in document.SelectNodes(@"./a:Objects/a:Report", xmlNamespace))
      {
        XmlElement rawPage = (XmlElement)report.SelectSingleNode("./a:RequestPage", xmlNamespace);
        if (rawPage != null)
        {
          String logStr = String.Format(CultureInfo.InvariantCulture,
            Resources.RequestPageIsAlreadyExists, report.Attributes["ID"].InnerText);
          TransformationLog.GenericLogEntry(logStr, LogCategory.Warning, report.Attributes["ID"].InnerText, null);
          report.ParentNode.RemoveChild(report);
          continue;
        }
        reportElements.Add(report.Attributes["ID"].InnerText, report);

        XmlElement code = (XmlElement)report.SelectSingleNode("./a:Code", xmlNamespace).Clone();
        XmlElement rawForm = (XmlElement)report.SelectSingleNode("./a:RequestForm", xmlNamespace);

        XmlElement form = document.CreateElement("Form", "urn:schemas-microsoft-com:dynamics:NAV:ApplicationObjects");
        form.InnerXml = rawForm.InnerXml;
        form.AppendChild(code);

        XmlAttribute attribute = document.CreateAttribute("ID");
        attribute.InnerText = "-" + report.Attributes["ID"].InnerText;
        form.Attributes.Append(attribute);

        attribute = document.CreateAttribute("Name");
        attribute.InnerText = report.Attributes["Name"].InnerText;
        form.Attributes.Append(attribute);

        if (report.Attributes["Date"] != null)
        {
          attribute = document.CreateAttribute("Date");
          attribute.InnerText = report.Attributes["Date"].InnerText;
          form.Attributes.Append(attribute);
        }
        if (report.Attributes["Time"] != null)
        {
          attribute = document.CreateAttribute("Time");
          attribute.InnerText = report.Attributes["Time"].InnerText;
          form.Attributes.Append(attribute);
        }
        document.DocumentElement.ReplaceChild(form, report);
      }

      return document;
    }

    /// <summary>
    /// Changes the output from form transformation back into the format CSide expects when it does imports of reports 
    /// </summary>
    /// <param name="transformationOutput">Result from form transformation</param>
    /// <returns>Export format ready for CSide</returns>
    public XmlDocument GetDestinationPages(XmlDocument transformationOutput)
    {
      XmlNamespaceManager xmlNamespace = new XmlNamespaceManager(document.NameTable);
      xmlNamespace.AddNamespace("a", "urn:schemas-microsoft-com:dynamics:NAV:ApplicationObjects");

      foreach (XmlElement page in transformationOutput.SelectNodes(@"./a:Objects/a:Page", xmlNamespace))
      {
        String id = page.Attributes["ID"].InnerText;
        if (!id.StartsWith("-", StringComparison.Ordinal))
        {
          continue;
        }

        XmlElement report;
        if (!reportElements.TryGetValue(id.Substring(1), out report))
        {
          continue;
        }

        XmlElement reportCode = (XmlElement)report.SelectSingleNode("./a:Code", xmlNamespace);
        XmlElement pageCode = (XmlElement)page.SelectSingleNode("./a:Code", xmlNamespace);
        XmlElement reportPage = (XmlElement)report.SelectSingleNode("./a:RequestPage", xmlNamespace);

        reportCode.InnerXml = pageCode.InnerXml;
        page.RemoveChild(pageCode);

        reportPage.InnerXml = page.InnerXml;

        XmlDocumentFragment fragment = transformationOutput.CreateDocumentFragment();
        fragment.InnerXml = report.OuterXml;
        transformationOutput.DocumentElement.ReplaceChild(fragment, page);
      }

      return transformationOutput;
    }
  }
}
