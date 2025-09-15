using System.Collections;
using System.Text.Json;

namespace JSONPath;

public partial class JSONPathForm : Form
{
    private static readonly JsonElement jsonNull = JsonSerializer.Deserialize<JsonElement>("null");

    private bool ignoreNextChange = false;
    private RichTextBox richTextBox;
    private TextBox jsonPathTextBox;
    private CheckBox nullIfNotExistentCheckBox;
    private Label errorMessage;
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
            PlaceholderText = "$.store.books[*].author%c%k%u%kc%uc",
        };
        this.jsonPathTextBox.KeyPress += (object? sender, KeyPressEventArgs e) =>
        {
            if (e.KeyChar == '\r' || e.KeyChar == '\n')
            {
                e.Handled = true;
                RerenderJson();
            }
        };

        this.nullIfNotExistentCheckBox = new CheckBox
        {
            Text = "Non existent property as null",
        };
        this.nullIfNotExistentCheckBox.CheckedChanged += (object? sender, EventArgs e) =>
        {
            RerenderJson();
        };
        this.errorMessage = new Label
        {
            ForeColor = Color.Red,
        };

        SetSizes();

        this.Controls.Add(this.richTextBox);
        this.Controls.Add(this.jsonPathTextBox);
        this.Controls.Add(this.nullIfNotExistentCheckBox);
        this.Controls.Add(this.errorMessage);

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
        int richTextBoxHeight = height - 75;
        richTextBox.Location = new Point(10, richTextBoxYStart);
        richTextBox.Width = width - 20;
        richTextBox.Height = richTextBoxHeight;

        jsonPathTextBox.Location = new Point(10, richTextBoxYStart + richTextBoxHeight + 10);
        jsonPathTextBox.Width = width - 20;

        var nullIfNotExistentCheckBoxWidth = 200;
        nullIfNotExistentCheckBox.Location = new Point(10, richTextBoxYStart + richTextBoxHeight + 35);
        nullIfNotExistentCheckBox.Width = nullIfNotExistentCheckBoxWidth;

        errorMessage.Location = new Point(nullIfNotExistentCheckBoxWidth + 35, richTextBoxYStart + richTextBoxHeight + 38);
        errorMessage.Width = 300;
    }

    private JsonSerializerOptions jsonOptions = new()
    {
        WriteIndented = true,
        IndentCharacter = ' ',
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };
    private void RerenderJson()
    {
        try
        {
            errorMessage.Text = "";
            var jsonPath = jsonPathTextBox.Text;
            if (jsonPath == "")
            {
                ignoreNextChange = true;
                richTextBox.Text = JsonSerializer.Serialize(this.parsed, jsonOptions);
                return;
            }

            var (firstPart, jsonPathParts) = CustomSplit(jsonPath);

            IEnumerable<JsonElement> parsedList = new[] { this.parsed };

            {
                // Do this for the first part
                var (first, rest) = SplitFilter(firstPart);
                if (first != "$")
                    throw new Exception();

                if (rest != "")
                    parsedList = GetFiltered(rest, parsedList);
            }

            var nullIfNotExistent = nullIfNotExistentCheckBox.Checked;
            foreach (var (type, part) in jsonPathParts)
            {
                var (first, rest) = SplitFilter(part);

                if (type == '.')
                    parsedList = GetProperty(parsedList, first, nullIfNotExistent);
                else if (type == '%')
                    parsedList = ApplyDirective(part, parsedList);

                if (rest != "")
                    parsedList = GetFiltered(rest, parsedList);
            }

            var text = JsonSerializer.Serialize(parsedList, jsonOptions);
            ignoreNextChange = true;
            richTextBox.Text = text;
        }
        catch (Exception e)
        {
            errorMessage.Text = $"There was an error: {e.Message}";
        }
    }

    private static (string first, List<(char, string)> parts) CustomSplit(string s)
    {
        var indexes = s.Select((c, i) => (c, i)).Where(p => p.c == '.' || p.c == '%').Select(p => p.i).ToList();
        var list = new List<(char, string)>(indexes.Count);
        for (int j = 0; j < indexes.Count; j++)
        {
            var idx = indexes[j];
            var nextIdx = j + 1 < indexes.Count ? indexes[j + 1] : s.Length;
            list.Add((s[idx], s[(idx + 1)..nextIdx]));
        }
        return (indexes.Count == 0 ? s : s[..indexes[0]], list);
    }

    private static IEnumerable<JsonElement> GetProperty(IEnumerable<JsonElement> elements, string property, bool nullIfNotExistent)
    {
        foreach (var x in elements)
        {
            if (x.TryGetProperty(property, out var value))
                yield return value;
            else if (nullIfNotExistent)
                yield return jsonNull;
        }
    }

    private static IEnumerable<JsonElement> ApplyDirective(string type, IEnumerable<JsonElement> elements)
    {
        switch (type)
        {
            case "c":
                {
                    var count = elements.Count();
                    return [ConvertIntToJsonElement(count)];
                }
            case "k":
                {
                    var set = new HashSet<string>();
                    foreach (var item in elements)
                    {
                        foreach (var key in item.EnumerateObject())
                        {
                            set.Add(key.Name);
                        }
                    }
                    return set.Select(x => ConvertStringToJsonElement(x));
                }
            case "kc":
                {
                    var counts = new Dictionary<string, int>();
                    foreach (var item in elements)
                    {
                        foreach (var key in item.EnumerateObject())
                        {
                            counts.TryGetValue(key.Name, out int n);
                            counts[key.Name] = n + 1;
                        }
                    }
                    return [ConvertMapToJsonElement(counts)];
                }
            case "u":
                {
                    var set = new HashSet<string>();
                    var returnList = new List<JsonElement>();
                    foreach (var item in elements)
                    {
                        if (item.ValueKind == JsonValueKind.String)
                        {
                            if (set.Add(item.GetString() ?? throw new Exception()))
                                returnList.Add(item);
                        }
                        else if (item.ValueKind == JsonValueKind.Number)
                        {
                            if (item.TryGetInt32(out int i))
                            {
                                if (set.Add(i + ""))
                                    returnList.Add(item);
                            }
                            else if (item.TryGetDouble(out double d))
                            {
                                if (set.Add(d + ""))
                                    returnList.Add(item);
                            }
                        }
                    }
                    return returnList;
                }
            case "uc":
                {
                    var counts = new Dictionary<string, int>();
                    foreach (var item in elements)
                    {
                        if (item.ValueKind == JsonValueKind.String)
                        {
                            var key = item.GetString() ?? throw new Exception();
                            counts.TryGetValue(key, out int n);
                            counts[key] = n + 1;
                        }
                        else if (item.ValueKind == JsonValueKind.Number)
                        {
                            if (item.TryGetInt32(out int i))
                            {
                                var key = i + "";
                                counts.TryGetValue(key, out int n);
                                counts[key] = n + 1;
                            }
                            else if (item.TryGetDouble(out double d))
                            {
                                var key = d + "";
                                counts.TryGetValue(key, out int n);
                                counts[key] = n + 1;
                            }
                        }
                    }
                    return [ConvertMapToJsonElement(counts)];
                }
        }
        throw new Exception($"Unknown directive: '{type}'");
    }

    private static JsonElement ConvertIntToJsonElement(int i)
    {
        return JsonSerializer.Deserialize<JsonElement>(i + "");
    }

    private static JsonElement ConvertStringToJsonElement(string s)
    {
        return JsonSerializer.Deserialize<JsonElement>('"' + s + '"');
    }

    private static JsonElement ConvertMapToJsonElement<T>(T o) where T : IDictionary
    {
        return JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(o));
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
        var mappedValues = mapped.SelectMany(x =>
        {
            if (x.ValueKind == JsonValueKind.Array)
                return x.EnumerateArray();
            else if (x.ValueKind == JsonValueKind.Object)
                return x.EnumerateObject().Select(x => x.Value);
            else
                throw new Exception();
        });

        if (filterExpression == "[*]")
        {
            return mappedValues;
        }
        else
        {
            // TODO handle filtering
            throw new Exception();
        }
    }
}
