using System.Xml;

namespace CasualConsoleCore.Xml;

public class XmlParserTest
{
    public static void Test()
    {
        EmptyTest();
        SimplestTest();
        RecursiveTest();
        InnerTextTest();
        InnerTextNormalizeTest();
        InnerTextNormalizeTestInt();
        InnerTextNormalizeTestHexInt();
        BasicAttributeTest();
        CustomAttributeTest();
        AttributeNormalizeTest();
        HtmlNodeTest();
        CDataTest();
        DataLengthTest();
        TrimTest();
        BeautifyTest();
        BeautifyTest2();
        BeautifyTest3();
        InnerXmlAndNodeTest();
        SingleQuoteTest();
        HandleComments();
        MultiRootTest();
        UnclosedTagHtmlTag();
        AllowSelfClosingToImmediatelyClose();
    }

    private static void EmptyTest()
    {
        var text = "";

        var mydoc = XmlParser.Parse(text);
        if (mydoc.ChildNodes.Count != 0) throw new System.Exception();
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

    private static void InnerTextTest()
    {
        var text = @"
<nodes>This code says <code>hello world</code> and it's awesome</nodes>
";

        var mydoc = XmlParser.Parse(text);
        if (mydoc.InnerText != "This code says hello world and it's awesome") throw new System.Exception();
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

    private static void InnerTextNormalizeTestInt()
    {
        var text = @"
<nodes>I&#39;m gone</nodes>
";

        XmlDocument doc = new XmlDocument();
        doc.LoadXml(text);

        var mydoc = XmlParser.Parse(text);

        if (doc.ChildNodes[0].InnerText != "I'm gone") throw new System.Exception();
        if (mydoc.ChildNodes[0].InnerText != "I'm gone") throw new System.Exception();
    }

    private static void InnerTextNormalizeTestHexInt()
    {
        var text = @"
<nodes>I&#x27;m gone</nodes>
";

        XmlDocument doc = new XmlDocument();
        doc.LoadXml(text);

        var mydoc = XmlParser.Parse(text);

        if (doc.ChildNodes[0].InnerText != "I'm gone") throw new System.Exception();
        if (mydoc.ChildNodes[0].InnerText != "I'm gone") throw new System.Exception();
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
        if (doc.ChildNodes[0].Attributes["text"].Value != "R&D") throw new System.Exception();

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

    private static void BeautifyTest()
    {
        var text = @"<nodes text=""R&amp;D"" data="""" checked><node><age>23</age></node></nodes>";
        var textIndented = "<nodes text=\"R&amp;D\" data=\"\" checked>\r\n  <node>\r\n    <age>23</age>\r\n  </node>\r\n</nodes>";

        var mydoc = XmlParser.Parse(text);
        var beautified = mydoc.Beautify(indentChars: "", newLineChars: "");
        var beautifiedIndented = mydoc.Beautify();

        if (text != beautified) throw new System.Exception();
        if (textIndented != beautifiedIndented) throw new System.Exception();
    }

    private static void BeautifyTest2()
    {
        var text = @"<main>This is the text. <a>Click</a> to see more</main>";

        var mydoc = XmlParser.Parse(text);
        var beautified = mydoc.Beautify(indentChars: "", newLineChars: "");

        if (text != beautified) throw new System.Exception();
    }

    private static void BeautifyTest3()
    {
        var text = "<a></a>";

        var mydoc = XmlParser.Parse(text);
        mydoc.ChildNodes[0].InnerText = "<some text that contains less than>";
        var beautified = mydoc.Beautify();

        if (beautified != "<a>&lt;some text that contains less than&gt;</a>") throw new System.Exception();
    }

    private static void InnerXmlAndNodeTest()
    {
        var text = @"
<a>
<b>
e <c>DData</c>
</b>
</a>
";
        var mydoc = XmlParser.Parse(text);
        var node = mydoc.ChildNodes[0].ChildNodes[0];
        if (!(node.InnerText == "e DData" && node.ChildNodes.Count == 1)) throw new System.Exception();
    }

    private static void SingleQuoteTest()
    {
        var text = @"<node attr1=""somedata1"" attr2='somedata2'></node>";

        var mydoc = XmlParser.Parse(text);
        if (mydoc.ChildNodes[0].Attributes["attr1"] != "somedata1") throw new System.Exception();
        if (mydoc.ChildNodes[0].Attributes["attr2"] != "somedata2") throw new System.Exception();
    }

    private static void HandleComments()
    {
        var text = "<node><!--this is some comment, I can write whatever I wanna write here such as: >>>>>--></node>";

        XmlParser.Parse(text);
    }

    private static void MultiRootTest()
    {
        var text = "<node>1</node><node>2</node><node>3</node>";

        var mydoc = XmlParser.Parse(text);
        if (mydoc.ChildNodes.Count != 3) throw new System.Exception();
    }

    private static void UnclosedTagHtmlTag()
    {
        var text = @"
<picture>
  <source media=""(min-width:650px)"" srcset=""img_pink_flowers.jpg"">
  <source media=""(min-width:465px)"" srcset=""img_white_flower.jpg""/>
  <img src=""img_orange_flowers.jpg"" alt=""Flowers"" style=""width:auto;"">
  <img src=""img_orange_flowers.jpg"" alt=""Flowers"" style=""width:auto;""/>
</picture>
";

        var mydoc = XmlParser.Parse(text, isHtml: true);
        if (mydoc.ChildNodes.Count != 1) throw new System.Exception();
        if (mydoc.ChildNodes[0].ChildNodes.Count != 4) throw new System.Exception();
    }

    private static void AllowSelfClosingToImmediatelyClose()
    {
        var text = @"
        <dd>
        <source></source>
        </dd>
        ";
        var mydoc = XmlParser.Parse(text, isHtml: true);
    }
}
