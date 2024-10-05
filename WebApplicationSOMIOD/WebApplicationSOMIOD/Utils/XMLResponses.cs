

using System.Collections.Generic;
using System.Xml;
namespace WebApplicationSOMIOD.Utils
{
    public class XMLResponses
    {

        public XmlDocument GetDiscoverXMLType(List<string> names, string parentXMLTagName)
        {

            var xmlDoc = new XmlDocument();
            var rootElement = xmlDoc.CreateElement(parentXMLTagName);

            foreach (var name in names)
            {
                var nameElement = xmlDoc.CreateElement("name");
                nameElement.InnerText = name;
                rootElement.AppendChild(nameElement);
            }

            xmlDoc.AppendChild(rootElement);

            return xmlDoc;
        }

        public XmlDocument GetMessageXMLType(string message)
        {

            var xmlDoc = new XmlDocument();
            var rootElement = xmlDoc.CreateElement("message");

            rootElement.InnerText = message;
            
            

            xmlDoc.AppendChild(rootElement);

            return xmlDoc;
        }


    }
}