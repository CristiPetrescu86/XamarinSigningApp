// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Xml;

namespace SignatureXML.Library
{
    public abstract class EncryptedReference
    {
        private string _uri;
        private string _referenceType;
        private TransformChain _transformChain;
        internal XmlElement _cachedXml;

        protected EncryptedReference() : this(string.Empty, new TransformChain())
        {
        }

        protected EncryptedReference(string uri) : this(uri, new TransformChain())
        {
        }

        protected EncryptedReference(string uri, TransformChain transformChain)
        {
            TransformChain = transformChain;
            Uri = uri;
            _cachedXml = null;
        }

        public string Uri
        {
            get { return _uri; }
            set
            {
                if (value == null)
                    throw new System.Exception();
                _uri = value;
                _cachedXml = null;
            }
        }

        public TransformChain TransformChain
        {
            get
            {
                if (_transformChain == null)
                    _transformChain = new TransformChain();
                return _transformChain;
            }
            set
            {
                _transformChain = value;
                _cachedXml = null;
            }
        }

        public void AddTransform(Transform transform)
        {
            TransformChain.Add(transform);
        }

        protected string ReferenceType
        {
            get { return _referenceType; }
            set
            {
                _referenceType = value;
                _cachedXml = null;
            }
        }

        protected internal bool CacheValid
        {
            get
            {
                return (_cachedXml != null);
            }
        }

        public virtual XmlElement GetXml()
        {
            if (CacheValid) return _cachedXml;

            XmlDocument document = new XmlDocument();
            document.PreserveWhitespace = true;
            return GetXml(document);
        }

        internal XmlElement GetXml(XmlDocument document)
        {
            if (ReferenceType == null)
                throw new System.Exception();

            // Create the Reference
            XmlElement referenceElement = document.CreateElement(ReferenceType, EncryptedXml.XmlEncNamespaceUrl);
            if (!string.IsNullOrEmpty(_uri))
                referenceElement.SetAttribute("URI", _uri);

            // Add the transforms to the CipherReference
            if (TransformChain.Count > 0)
                referenceElement.AppendChild(TransformChain.GetXml(document, SignedXml.XmlDsigNamespaceUrl));

            return referenceElement;
        }

        public virtual void LoadXml(XmlElement value)
        {
            ReferenceType = value.LocalName;

            string uri = Utils.GetAttribute(value, "URI", EncryptedXml.XmlEncNamespaceUrl);
            if (uri == null)
                throw new System.Exception();
            Uri = uri;

            // Transforms
            XmlNamespaceManager nsm = new XmlNamespaceManager(value.OwnerDocument.NameTable);
            nsm.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);
            XmlNode transformsNode = value.SelectSingleNode("ds:Transforms", nsm);
            if (transformsNode != null)
                TransformChain.LoadXml(transformsNode as XmlElement);

            // cache the Xml
            _cachedXml = value;
        }
    }
}
