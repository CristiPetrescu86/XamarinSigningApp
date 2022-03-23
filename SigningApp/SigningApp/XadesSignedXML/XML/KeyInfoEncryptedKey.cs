// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Xml;

namespace SigningApp.XadesSignedXML.XML
{
    public class KeyInfoEncryptedKey : KeyInfoClause
    {
        private EncryptedKey _encryptedKey;

        public KeyInfoEncryptedKey() { }

        public KeyInfoEncryptedKey(EncryptedKey encryptedKey)
        {
            _encryptedKey = encryptedKey;
        }

        public EncryptedKey EncryptedKey
        {
            get { return _encryptedKey; }
            set { _encryptedKey = value; }
        }

        public override XmlElement GetXml()
        {
            if (_encryptedKey == null)
                throw new System.Exception();
            return _encryptedKey.GetXml();
        }

        internal override XmlElement GetXml(XmlDocument xmlDocument)
        {
            if (_encryptedKey == null)
                throw new System.Exception();
            return _encryptedKey.GetXml(xmlDocument);
        }

        public override void LoadXml(XmlElement value)
        {
            _encryptedKey = new EncryptedKey();
            _encryptedKey.LoadXml(value);
        }
    }
}