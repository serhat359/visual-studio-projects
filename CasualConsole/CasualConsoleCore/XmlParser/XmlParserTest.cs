using System.Xml;

namespace CasualConsoleCore.XmlParser
{
    public class XmlParserTest
    {
        public static void Test()
        {
            Test1();
            Test2();
            Test3();
        }

        private static void Test1()
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

        private static void Test2()
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

        private static void Test3()
        {
            var text = @"
<nodes text=""somedata"" text2=""somedata2"">
</nodes>
";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(text);

            var data = doc.ChildNodes[0].Attributes["text"];
            if(data.Name != "text") throw new System.Exception();
            if (data.Value != "somedata") throw new System.Exception();

            var mydoc = XmlParser.Parse(text);
            var attributes = mydoc.ChildNodes[0].Attributes;
            if (attributes["text"] != "somedata") throw new System.Exception();
            if (attributes["text2"] != "somedata2") throw new System.Exception();
        }
    }
}
