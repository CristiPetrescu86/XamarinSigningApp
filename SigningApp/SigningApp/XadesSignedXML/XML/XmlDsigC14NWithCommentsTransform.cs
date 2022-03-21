// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace SigningApp.XadesSignedXML.XML
{
    public class XmlDsigC14NWithCommentsTransform : XmlDsigC14NTransform
    {
        public XmlDsigC14NWithCommentsTransform()
            : base(true)
        {
            Algorithm = SignedXml.XmlDsigC14NWithCommentsTransformUrl;
        }
    }
}
