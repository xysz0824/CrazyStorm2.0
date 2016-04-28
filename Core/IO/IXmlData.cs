/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace CrazyStorm.Core
{
    interface IXmlData
    {
        XmlElement  BuildFromXml(XmlDocument doc, XmlElement node);
        XmlElement  StoreAsXml(XmlDocument doc, XmlElement node);
    }
}
