// TimeStamp.cs
//
// XAdES Starter Kit for Microsoft .NET 3.5 (and above)
// 2010 Microsoft France
// Published under the CECILL-B Free Software license agreement.
// (http://www.cecill.info/licences/Licence_CeCILL-B_V1-en.txt)
// 
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// WHETHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED 
// WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// THE ENTIRE RISK OF USE OR RESULTS IN CONNECTION WITH THE USE OF THIS CODE 
// AND INFORMATION REMAINS WITH THE USER. 
//

using System;
using System.Xml;
using System.Security.Cryptography;
using System.Collections;

namespace SignatureXML.Library
{
	/// <summary>
	/// This class contains timestamp information
	/// </summary>
	public class TimeStamp
	{
		#region Private variables
		private string tagName;
		private HashDataInfoCollection hashDataInfoCollection;
		private EncapsulatedPKIData encapsulatedTimeStamp;
		private XMLTimeStamp xmlTimeStamp;
		#endregion

		#region Public properties
		/// <summary>
		/// The name of the element when serializing
		/// </summary>
		public string TagName
		{
			get
			{
				return this.tagName;
			}
			set
			{
				this.tagName = value;
			}
		}

		/// <summary>
		/// A collection of hash data infos
		/// </summary>
		public HashDataInfoCollection HashDataInfoCollection
		{
			get
			{
				return this.hashDataInfoCollection;
			}
			set
			{
				this.hashDataInfoCollection = value;
			}
		}

		/// <summary>
		/// The time-stamp generated by a TSA encoded as an ASN.1 data
		/// object
		/// </summary>
		public EncapsulatedPKIData EncapsulatedTimeStamp
		{
			get
			{
				return this.encapsulatedTimeStamp;
			}
			set
			{
				this.encapsulatedTimeStamp = value;
				if (this.encapsulatedTimeStamp != null)
				{
					this.xmlTimeStamp = null;
				}
			}
		}

		/// <summary>
		/// The time-stamp generated by a TSA encoded as a generic XML
		/// timestamp
		/// </summary>
		public XMLTimeStamp XMLTimeStamp
		{
			get
			{
				return this.xmlTimeStamp;
			}
			set
			{
				this.xmlTimeStamp = value;
				if (this.xmlTimeStamp != null)
				{
					this.encapsulatedTimeStamp = null;
				}
			}
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Default constructor
		/// </summary>
		public TimeStamp()
		{
			this.hashDataInfoCollection = new HashDataInfoCollection();
			this.encapsulatedTimeStamp = new EncapsulatedPKIData("EncapsulatedTimeStamp");
			this.xmlTimeStamp = null;
		}

		/// <summary>
		/// Constructor with TagName
		/// </summary>
		/// <param name="tagName">Name of the tag when serializing with GetXml</param>
		public TimeStamp(string tagName) : this()
		{
			this.tagName = tagName;
		}
		#endregion

		#region Public methods
		/// <summary>
		/// Check to see if something has changed in this instance and needs to be serialized
		/// </summary>
		/// <returns>Flag indicating if a member needs serialization</returns>
		public bool HasChanged()
		{
			bool retVal = false;

			if (this.hashDataInfoCollection.Count > 0)
			{
				retVal = true;
			}

			if (this.encapsulatedTimeStamp != null && this.encapsulatedTimeStamp.HasChanged())
			{
				retVal = true;
			}

			if (this.xmlTimeStamp != null && this.xmlTimeStamp.HasChanged())
			{
				retVal = true;
			}

			return retVal;
		}

		/// <summary>
		/// Load state from an XML element
		/// </summary>
		/// <param name="xmlElement">XML element containing new state</param>
		public void LoadXml(System.Xml.XmlElement xmlElement)
		{
			XmlNamespaceManager xmlNamespaceManager;
			XmlNodeList xmlNodeList;
			IEnumerator enumerator;
			XmlElement iterationXmlElement;
			HashDataInfo newHashDataInfo;
			
			if (xmlElement == null)
			{
				throw new ArgumentNullException("xmlElement");
			}

			xmlNamespaceManager = new XmlNamespaceManager(xmlElement.OwnerDocument.NameTable);
			xmlNamespaceManager.AddNamespace("xsd", SignedXml.XadesNamespaceUri);

			this.hashDataInfoCollection.Clear();
			xmlNodeList = xmlElement.SelectNodes("xsd:HashDataInfo", xmlNamespaceManager);
			enumerator = xmlNodeList.GetEnumerator();
			try 
			{
				while (enumerator.MoveNext()) 
				{
					iterationXmlElement = enumerator.Current as XmlElement;
					if (iterationXmlElement != null)
					{
						newHashDataInfo = new HashDataInfo();
						newHashDataInfo.LoadXml(iterationXmlElement);
						this.hashDataInfoCollection.Add(newHashDataInfo);
					}
				}
			}
			finally 
			{
				IDisposable disposable = enumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}

			xmlNodeList = xmlElement.SelectNodes("xsd:EncapsulatedTimeStamp", xmlNamespaceManager);
			if (xmlNodeList.Count != 0)
			{
				this.encapsulatedTimeStamp = new EncapsulatedPKIData("EncapsulatedTimeStamp");
				this.encapsulatedTimeStamp.LoadXml((XmlElement)xmlNodeList.Item(0));
				this.xmlTimeStamp = null;
			}
			else
			{
				xmlNodeList = xmlElement.SelectNodes("xsd:XMLTimeStamp", xmlNamespaceManager);
				if (xmlNodeList.Count != 0)
				{
					this.xmlTimeStamp = new XMLTimeStamp();
					this.xmlTimeStamp.LoadXml((XmlElement)xmlNodeList.Item(0));
					this.encapsulatedTimeStamp = null;

				}
				else
				{
					throw new CryptographicException("EncapsulatedTimeStamp or XMLTimeStamp missing");
				}
			}
		}

		/// <summary>
		/// Returns the XML representation of the this object
		/// </summary>
		/// <returns>XML element containing the state of this object</returns>
		public XmlElement GetXml()
		{
			XmlDocument creationXmlDocument;
			XmlElement retVal;

			creationXmlDocument = new XmlDocument();
            retVal = creationXmlDocument.CreateElement("xades", this.tagName, SignedXml.XadesNamespaceUri);

			if (this.hashDataInfoCollection.Count > 0)
			{
				foreach (HashDataInfo hashDataInfo in this.hashDataInfoCollection)
				{
					if (hashDataInfo.HasChanged())
					{
						retVal.AppendChild(creationXmlDocument.ImportNode(hashDataInfo.GetXml(), true));
					}
				}
			}
			else
			{
				throw new CryptographicException("HashDataInfoCollection is empty.  TimeStamp needs at least one HashDataInfo element");
			}

			if (this.encapsulatedTimeStamp != null && this.encapsulatedTimeStamp.HasChanged())
			{
				retVal.AppendChild(creationXmlDocument.ImportNode(this.encapsulatedTimeStamp.GetXml(), true));
			}
			else
			{
				if (this.xmlTimeStamp != null && this.xmlTimeStamp.HasChanged())
				{
					retVal.AppendChild(creationXmlDocument.ImportNode(this.xmlTimeStamp.GetXml(), true));
				}
				else
				{
					throw new CryptographicException("EncapsulatedTimeStamp or XMLTimeStamp element missing");
				}
			}

			return retVal;
		}
		#endregion
	}
}
