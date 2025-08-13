using System.Text.Json;

namespace JSONPath;

public partial class JSONPathForm : Form
{
    private bool ignoreNextChange = false;
    private RichTextBox richTextBox;
    private TextBox jsonPathTextBox;
    private JsonElement parsed;

    public JSONPathForm()
    {
        this.ClientSize = new Size(700, 700);
        this.Text = "JSONPath";

        this.richTextBox = new RichTextBox
        {
            Multiline = true,
        };
        this.richTextBox.TextChanged += (object? sender, EventArgs e) =>
        {
            try
            {
                if (ignoreNextChange)
                {
                    ignoreNextChange = false;
                    return;
                }

                parsed = JsonSerializer.Deserialize<JsonElement>(richTextBox.Text);
                RerenderJson();
            }
            catch (Exception)
            {

            }
        };

        this.jsonPathTextBox = new TextBox
        {
            PlaceholderText = "$.store.books[*].author",
        };
        jsonPathTextBox.KeyPress += (object? sender, KeyPressEventArgs e) =>
        {
            if (e.KeyChar == '\r' || e.KeyChar == '\n')
            {
                e.Handled = true;
                RerenderJson();
            }
        };

        SetSizes();

        this.Controls.Add(richTextBox);
        this.Controls.Add(jsonPathTextBox);

        this.Resize += (object? sender, EventArgs e) =>
        {
            SetSizes();
        };
    }

    private void SetSizes()
    {
        var width = this.ClientSize.Width;
        var height = this.ClientSize.Height;

        int richTextBoxYStart = 10;
        int richTextBoxHeight = height - 60;
        richTextBox.Location = new Point(10, richTextBoxYStart);
        richTextBox.Width = width - 20;
        richTextBox.Height = richTextBoxHeight;

        jsonPathTextBox.Location = new Point(10, richTextBoxYStart + richTextBoxHeight + 10);
        jsonPathTextBox.Width = width - 20;
    }

    private JsonSerializerOptions jsonOptions = new()
    {
        WriteIndented = true,
        IndentCharacter = ' ',
    };
    private void RerenderJson()
    {
        try
        {
            var jsonPath = jsonPathTextBox.Text;
            if (jsonPath == "")
            {
                ignoreNextChange = true;
                richTextBox.Text = JsonSerializer.Serialize(this.parsed, jsonOptions);
                return;
            }

            var jsonPathParts = jsonPath.Split(".");

            IEnumerable<JsonElement> parsedList = new[] { this.parsed };

            {
                // Do this for the first part
                var (first, rest) = SplitFilter(jsonPathParts[0]);
                if (first != "$")
                    throw new Exception();

                if (rest != "")
                    parsedList = GetFiltered(rest, parsedList);
            }

            foreach (var part in jsonPathParts.Skip(1))
            {
                var (first, rest) = SplitFilter(part);

                parsedList = parsedList.Select(x => x.GetProperty(first));

                if (rest != "")
                    parsedList = GetFiltered(rest, parsedList);
            }

            var text = JsonSerializer.Serialize(parsedList, jsonOptions);
            ignoreNextChange = true;
            richTextBox.Text = text;
        }
        catch (Exception)
        {

        }
    }

    private static (string first, string rest) SplitFilter(string part)
    {
        var i = part.IndexOf('[');
        if (i < 0)
            return (part, "");
        else
            return (part[..i], part[i..]);
    }

    private static IEnumerable<JsonElement> GetFiltered(string filterExpression, IEnumerable<JsonElement> mapped)
    {
        if (filterExpression == "[*]")
        {
            return mapped.SelectMany(x => x.EnumerateArray());
        }
        else
        {
            // TODO handle filtering
            throw new Exception();
        }
    }
}
