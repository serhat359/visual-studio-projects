using System.Xml;
using static System.Net.Mime.MediaTypeNames;

namespace CasualConsoleCore.XmlParser
{
    public class XmlParserTest
    {
        public static void Test()
        {
            SimplestTest();
            RecursiveTest();
            InnerTextNormalizeTest();
            BasicAttributeTest();
            CustomAttributeTest();
            AttributeNormalizeTest();
            HtmlNodeTest();
            CDataTest();
            DataLengthTest();
            TrimTest();
        }

        private static void SimplestTest()
        {
            var text = @"
<node>Data</node>
";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(text);

            var regularData = doc.ChildNodes[0].InnerText;

            var mydoc = XmlParser.Parse(text);
            var myData = mydoc.ChildNodes[0].InnerText;

            if (myData != regularData) throw new System.Exception();
        }

        private static void RecursiveTest()
        {
            var text = @"
<nodes>
<node>Data1</node>
<node>Data2</node>
<node>Data3</node>
</nodes>
";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(text);

            var mydoc = XmlParser.Parse(text);

            if (doc.ChildNodes[0].ChildNodes[0].InnerText != mydoc.ChildNodes[0].ChildNodes[0].InnerText) throw new System.Exception();
            if (doc.ChildNodes[0].ChildNodes[1].InnerText != mydoc.ChildNodes[0].ChildNodes[1].InnerText) throw new System.Exception();
            if (doc.ChildNodes[0].ChildNodes[2].InnerText != mydoc.ChildNodes[0].ChildNodes[2].InnerText) throw new System.Exception();
        }

        private static void InnerTextNormalizeTest()
        {
            var text = @"
<nodes>R&amp;D</nodes>
";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(text);

            var mydoc = XmlParser.Parse(text);

            if (doc.ChildNodes[0].InnerText != "R&D") throw new System.Exception();
            if (mydoc.ChildNodes[0].InnerText != "R&D") throw new System.Exception();
        }

        private static void BasicAttributeTest()
        {
            var text = @"
<nodes text=""somedata"" text2=""somedata2"">
</nodes>
";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(text);

            var data = doc.ChildNodes[0].Attributes["text"];
            if (data.Name != "text") throw new System.Exception();
            if (data.Value != "somedata") throw new System.Exception();

            var mydoc = XmlParser.Parse(text);
            var attributes = mydoc.ChildNodes[0].Attributes;
            if (attributes["text"] != "somedata") throw new System.Exception();
            if (attributes["text2"] != "somedata2") throw new System.Exception();
        }

        private static void CustomAttributeTest()
        {
            var text = @"
<nodes text=""somedata"" text2="""" text3 text4>
</nodes>
";

            var mydoc = XmlParser.Parse(text);
            var attributes = mydoc.ChildNodes[0].Attributes;
            if (attributes["text"] != "somedata") throw new System.Exception();
            if (attributes["text2"] != "") throw new System.Exception();
            if (attributes["text3"] != null) throw new System.Exception();
            if (attributes["text4"] != null) throw new System.Exception();
        }

        private static void AttributeNormalizeTest()
        {
            var text = @"
<nodes text=""R&amp;D"">
</nodes>
";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(text);
            if(doc.ChildNodes[0].Attributes["text"].Value != "R&D") throw new System.Exception();

            var mydoc = XmlParser.Parse(text);
            var attributes = mydoc.ChildNodes[0].Attributes;
            if (attributes["text"] != "R&D") throw new System.Exception();
        }

        private static void HtmlNodeTest()
        {
            var text = @"
<some>
    <img src=""source1"" />
    <img src=""source2"" />
    <img src=""source3"" />
</some>
";

            var mydoc = XmlParser.Parse(text);
            var baseNode = mydoc.ChildNodes[0];
            var firstImgNode = baseNode.ChildNodes[0];

            if (baseNode.ChildNodes.Count != 3) throw new System.Exception();
            if (firstImgNode.TagName != "img") throw new System.Exception();
            if (firstImgNode.Attributes.Count != 1) throw new System.Exception();
            if (firstImgNode.Attributes["src"] != "source1") throw new System.Exception();
        }

        private static void CDataTest()
        {
            var text = @"
<description>
  <node><![CDATA[some data that contains < and >]]></node>
<node><![CDATA[<some xml data></sss>]]></node>
</description>
";

            var mydoc = XmlParser.Parse(text);
            if (mydoc.ChildNodes[0].ChildNodes[0].InnerText != "some data that contains < and >") throw new System.Exception();
            if (mydoc.ChildNodes[0].ChildNodes[1].InnerText != "<some xml data></sss>") throw new System.Exception();
        }

        private static void DataLengthTest()
        {
            var text = @"
<a>
<b c=""d"">
e
</b>
</a>
";

            // Test successfully parse
            var mydoc = XmlParser.Parse(text);
        }

        private static void TrimTest()
        {
            var text = @"
<a>
<b c=""d"">
e
</b>
</a>
";

            var mydoc = XmlParser.Parse(text);
            if (mydoc.ChildNodes[0].ChildNodes[0].InnerText != "e") throw new System.Exception();
        }
    }
}
