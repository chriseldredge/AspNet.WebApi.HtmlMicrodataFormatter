﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace AspNet.WebApi.HtmlMicrodataFormatter
{
    public class LinkSerializer : DefaultSerializer
    {
        public override IEnumerable<Type> SupportedTypes { get { return new[] { typeof(Link) }; } }

        public override IEnumerable<XObject> Serialize(string propertyName, object obj, SerializationContext context)
        {
            if (obj == null) return Enumerable.Empty<XObject>();

            var link = (Link)obj;

            var element = new XElement("a",
                link.Attributes.Select(attr => new XAttribute(attr.Key, attr.Value)),
                new XText(link.Body));

            SetPropertyName(element, propertyName, context);

            return new[] {element};
        }
    }
}