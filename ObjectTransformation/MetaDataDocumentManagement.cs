//--------------------------------------------------------------------------
// <copyright file="MetaDataDocumentManagement.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>?abc?</summary>
//--------------------------------------------------------------------------
namespace Microsoft.Dynamics.Nav.Tools.FormTransformation
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Xml;
    using System.Globalization;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// ?abc?
    /// </summary>
    public class MetadataDocumentManagement
    {
        #region Private members variables
        private static MetadataDocumentManagement instance = new MetadataDocumentManagement();

        private XmlNode xmlCurrentFormNode;
        private XmlDocument xmlDocument;

        private XmlDocument insertElementsDoc;
        private XmlDocument ignorePagesDoc;
        private XmlDocument renumberPagesDoc;
        private XmlDocument deleteElementsDoc;
        private XmlDocument moveElementsDoc;
        private List<string> codeRulesDoc = new List<string>();

        private XmlNamespaceManager xmlNamespaceMgt;
        private String xmlNamespace = "urn:schemas-microsoft-com:dynamics:NAV:ApplicationObjects";
        private Dictionary<String, Dictionary<String, String>> hashValue = new Dictionary<string, Dictionary<string, string>>();
        private static Int32 hashConflicts;

        #endregion


        #region Constructors
        private MetadataDocumentManagement()
        {
        }
        #endregion

        #region Properties

        /// <summary>
        /// ?abc?
        /// </summary>
        public static void ReleaseInstance()
        {
            instance = new MetadataDocumentManagement();
        }

        /// <summary>
        /// ?abc?
        /// </summary>
        public static MetadataDocumentManagement Instance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        /// ?abc?
        /// </summary>
        public XmlNode XmlCurrentFormNode
        {
            get
            {
                return xmlCurrentFormNode;
            }

            set
            {
                xmlCurrentFormNode = value;
            }
        }

        /// <summary>
        /// ?abc?
        /// </summary>
        public XmlDocument XmlDocument
        {
            get
            {
                return xmlDocument;
            }

            set
            {
                xmlDocument = value;
                if (xmlDocument == null)
                {
                    return;
                }
                xmlNamespaceMgt = new XmlNamespaceManager(xmlDocument.NameTable);
                xmlNamespaceMgt.AddNamespace("a", XmlNamespace);
            }
        }

        /// <summary>
        /// ?abc?
        /// </summary>
        public XmlNamespaceManager XmlNamespaceMgt
        {
            get
            {
                return xmlNamespaceMgt;
            }
        }

        /// <summary>
        /// ?abc?
        /// </summary>
        public String XmlNamespace
        {
            get
            {
                return xmlNamespace;
            }
        }

        /// <summary>
        /// ?abc?
        /// </summary>
        public Int32 GetCurrentPageId
        {
            get
            {
                return (Convert.ToInt32(this.XmlCurrentFormNode.SelectSingleNode("@ID").Value, CultureInfo.InvariantCulture));
            }
        }

        /// <summary>
        /// ?abc?
        /// </summary>
        public XmlDocument InsertElementsDoc
        {
            get
            {
                return insertElementsDoc;
            }

            set
            {
                insertElementsDoc = value;
            }
        }

        /// <summary>
        /// ?abc?
        /// </summary>
        public XmlDocument IgnorePagesDoc
        {
            get
            {
                return ignorePagesDoc;
            }

            set
            {
                ignorePagesDoc = value;
            }
        }

        /// <summary>
        /// ?abc?
        /// </summary>
        public XmlDocument RenumberPagesDoc
        {
            get
            {
                return renumberPagesDoc;
            }

            set
            {
                renumberPagesDoc = value;
            }
        }

        /// <summary>
        /// ?abc?
        /// </summary>
        public XmlDocument DeleteElementsDoc
        {
            get
            {
                return deleteElementsDoc;
            }

            set
            {
                deleteElementsDoc = value;
            }
        }

        /// <summary>
        /// ?abc?
        /// </summary>
        public XmlDocument MoveElementsDoc
        {
            get
            {
                return moveElementsDoc;
            }

            set
            {
                moveElementsDoc = value;
            }
        }

        /// <summary>
        /// ?abc?
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Keeping List<> for extra functionality")]
        public List<String> CodeRulesDoc
        {
            get
            {
                return codeRulesDoc;
            }
        }

        /// <summary>
        /// ?abc?er44
        /// </summary>
        public Int32 CalcId(Int32 pageId, String newId, String hashInput, String pagePart)
        {
            if (newId != null)
            {
                return Convert.ToInt32(newId, CultureInfo.InvariantCulture);
            }

            if (!String.IsNullOrEmpty(pagePart))
            {
                return CalcIdUsingHash(pageId, hashInput, pagePart);
            }

            return -1;
        }

        /// <summary>
        /// ?abc?er44
        /// </summary>
        public Int32 CalcId(String newId, String hashInput, String pagePart)
        {
            return CalcId(GetCurrentPageId, newId, hashInput, pagePart);
        }

        private static Int32 GetPagePartIdOffset(String pagePart, out Int32 minimumIdValue, out Int32 hashValueOffset, out Int32 hashValueRange)
        {
            minimumIdValue = 1900000000;
            hashValueOffset = 2;
            if (pagePart.Equals("Code"))
            {
                minimumIdValue = 19000000;
                hashValueOffset = 0;
            }
            hashValueRange = 80000;

            switch (pagePart)
            {
                case "ContentArea":
                    return 1;
                case "ReportOptions": // Options Band for reports
                    return 2;
                case "RelatedInformation":
                    return 3;
                case "ActionItems":
                    return 4;
                case "NewDocumentItems":
                    return 5;
                case "Reports":
                    return 6;
                case "FactBoxArea":
                    return 7;
                case "RoleCenterArea":
                    return 8;
                case "HomeItems":
                    return 11;
                case "ActivityButtons":
                    return 12;
                case "Code":
                    return 0;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// ?abc?
        /// </summary>
        private Int32 CalcIdUsingHash(Int32 pageId, String hashInput, String pagePart)
        {
            Int32 minimumIdValue;
            Int32 hashValueOffset;
            Int32 hashValueRange;
            Int32 pagePartOffset = GetPagePartIdOffset(pagePart, out minimumIdValue, out hashValueOffset, out hashValueRange);

            if (String.IsNullOrEmpty(hashInput))
                return minimumIdValue + pagePartOffset;

            Int32 newID;
            newID =
              minimumIdValue +
              (CalcHashValueFromString(hashInput, hashValueRange) * ((int)Math.Pow(10, hashValueOffset))) +
              pagePartOffset;

            Dictionary<String, String> partHashValues;
            if (!hashValue.TryGetValue(pagePart, out partHashValues))
                hashValue.Add(pagePart, new Dictionary<string, string>());
            if (hashValue.TryGetValue(pagePart, out partHashValues) &&
                partHashValues.ContainsKey(newID + "@" + pageId))
            {
                // conflict resolution
                while (partHashValues.ContainsKey(newID + "@" + pageId))
                {
                    hashValueRange++;
                    newID = minimumIdValue + (hashValueRange * ((int)Math.Pow(10, hashValueOffset))) + pagePartOffset;
                }
                hashConflicts++;
            }
            partHashValues.Add(newID + "@" + pageId, hashInput);
            return newID;
        }

        /// <summary>
        /// ?abc?
        /// </summary>
        private static Int32 CalcHashValueFromString(String hashInput, Int32 hashValueRange)
        {
            Int32 newValue;
            Int32 hashValueOffset = Math.DivRem(hashValueRange, 2, out newValue);
            Math.DivRem(GetHashCode(hashInput), hashValueOffset, out newValue);
            newValue += hashValueOffset;
            return newValue;
        }

        /// <summary>
        /// We have taken the GetHashCode from Sysetm.String.GetHashCode (32bit v2.0.50727)
        /// to freeze the results and keep our Control IDs stable.
        /// </summary>
        /// <param name="hashInput"></param>
        /// <returns></returns>
        private static int GetHashCode(String s)
        {
            unsafe
            {
                fixed (char* src = s)
                {
                    //BCLDebug.Assert(src[this.Length] == '\0', "src[this.Length] == '\\0'"); 
                    //BCLDebug.Assert( ((int)src)%4 == 0, "Managed string should start at 4 bytes boundary");

                    int hash1 = (5381 << 16) + 5381;
                    int hash2 = hash1;

                    // 32bit machines. 
                    int* pint = (int*)src;
                    int len = s.Length;
                    while (len > 0)
                    {
                        hash1 = ((hash1 << 5) + hash1 + (hash1 >> 27)) ^ pint[0];
                        if (len <= 2)
                        {
                            break;
                        }
                        hash2 = ((hash2 << 5) + hash2 + (hash2 >> 27)) ^ pint[1];
                        pint += 2;
                        len -= 4;
                    }
                    return hash1 + (hash2 * 1566083941);
                }
            }
        }
        #endregion
    }
}