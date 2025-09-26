using System.Collections;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JSONPath;

public partial class JSONPathForm : Form
{
    private static readonly JsonElement jsonNull = JsonSerializer.Deserialize<JsonElement>("null");

    private bool ignoreNextChange = false;
    private RichTextBox richTextBox;
    private RichTextBox richTextBox2;
    private TextBox jsonPathTextBox;
    private CheckBox nullIfNotExistentCheckBox;
    private Label errorMessage;
    private CheckBox ignoreNullCheckBox;
    private JsonElement parsed;

    private JsonSerializerOptions jsonOptions;
    private JsonSerializerOptions jsonOptionsIgnoreNull;

    public JSONPathForm()
    {
        jsonOptions = CreateOptions();
        jsonOptionsIgnoreNull = CreateOptions();
        jsonOptionsIgnoreNull.Converters.Add(new JsonObjectConverter(jsonOptions));

        this.ClientSize = new Size(1100, 750);
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
                richTextBox.Text = JsonSerializer.Serialize(parsed, jsonOptions);
                RerenderJson();
            }
            catch (Exception)
            {

            }
        };

        this.richTextBox2 = new RichTextBox
        {
            Multiline = true,
        };

        this.jsonPathTextBox = new TextBox
        {
            PlaceholderText = "$.store[*].books[?(@.id > 5)].author%c%k%u%kc%uc",
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

        this.ignoreNullCheckBox = new CheckBox
        {
            Text = "Ignore null valued keys",
        };
        this.ignoreNullCheckBox.CheckedChanged += (object? sender, EventArgs e) =>
        {
            RerenderJson();
        };

        this.errorMessage = new Label
        {
            ForeColor = Color.Red,
        };

        SetSizes();

        this.Controls.Add(this.richTextBox);
        this.Controls.Add(this.richTextBox2);
        this.Controls.Add(this.jsonPathTextBox);
        this.Controls.Add(this.nullIfNotExistentCheckBox);
        this.Controls.Add(this.ignoreNullCheckBox);
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

        var halfWidth = (width - 30) / 2;

        int richTextBoxYStart = 10;
        int richTextBoxHeight = height - 95;
        richTextBox.Location = new Point(10, richTextBoxYStart);
        richTextBox.Width = halfWidth;
        richTextBox.Height = richTextBoxHeight;

        var rightSideX = halfWidth + 20;
        richTextBox2.Location = new Point(rightSideX, richTextBoxYStart);
        richTextBox2.Width = halfWidth;
        richTextBox2.Height = richTextBoxHeight;

        jsonPathTextBox.Location = new Point(rightSideX, richTextBoxYStart + richTextBoxHeight + 10);
        jsonPathTextBox.Width = halfWidth;

        var nullIfNotExistentCheckBoxWidth = 200;
        nullIfNotExistentCheckBox.Location = new Point(rightSideX, richTextBoxYStart + richTextBoxHeight + 35);
        nullIfNotExistentCheckBox.Width = nullIfNotExistentCheckBoxWidth;

        errorMessage.Location = new Point(10, richTextBoxYStart + richTextBoxHeight + 10);
        errorMessage.Width = 900;

        ignoreNullCheckBox.Location = new Point(rightSideX, richTextBoxYStart + richTextBoxHeight + 55);
        ignoreNullCheckBox.Width = nullIfNotExistentCheckBoxWidth;
    }

    private JsonSerializerOptions CreateOptions()
    {
        return new()
        {
            WriteIndented = true,
            IndentCharacter = ' ',
            IndentSize = 3,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };
    }

    class JsonObjectConverter : JsonConverter<JsonElement>
    {
        private JsonSerializerOptions regularOptions;

        public JsonObjectConverter(JsonSerializerOptions regularOptions)
        {
            this.regularOptions = regularOptions;
        }

        public override JsonElement Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, JsonElement value, JsonSerializerOptions options)
        {
            if (value.ValueKind == JsonValueKind.Object)
            {
                writer.WriteStartObject();
                foreach (var item in value.EnumerateObject())
                {
                    if (item.Value.ValueKind == JsonValueKind.Null)
                        continue;

                    writer.WritePropertyName(item.Name);
                    JsonSerializer.Serialize(writer, item.Value, options);
                }
                writer.WriteEndObject();
                return;
            }

            if (value.ValueKind == JsonValueKind.Array)
            {
                writer.WriteStartArray();
                foreach (var item in value.EnumerateArray())
                {
                    JsonSerializer.Serialize(writer, item, options);
                }
                writer.WriteEndArray();
                return;
            }

            JsonSerializer.Serialize(writer, value, regularOptions);
        }
    }

    private void RerenderJson()
    {
        try
        {
            errorMessage.Text = "";
            var jsonPath = jsonPathTextBox.Text;
            if (jsonPath == "")
            {
                ignoreNextChange = true;
                richTextBox2.Text = JsonSerializer.Serialize(this.parsed, ignoreNullCheckBox.Checked ? jsonOptionsIgnoreNull : jsonOptions);
                return;
            }

            var (firstPart, jsonPathParts) = CustomSplit(jsonPath);

            IEnumerable<JsonElement> parsedList = new[] { this.parsed };

            {
                // Do this for the first part
                var (first, rest) = SplitFilter(firstPart);
                if (first != "$")
                    throw new Exception();

                foreach (var restPart in rest)
                    parsedList = GetFiltered(restPart, parsedList);
            }

            var nullIfNotExistent = nullIfNotExistentCheckBox.Checked;
            foreach (var (type, part) in jsonPathParts)
            {
                var (first, rest) = SplitFilter(part);

                if (type == '.')
                    parsedList = GetProperty(parsedList, first, nullIfNotExistent);
                else if (type == '%')
                    parsedList = ApplyDirective(part, parsedList);

                foreach (var restPart in rest)
                    parsedList = GetFiltered(restPart, parsedList);
            }

            var text = JsonSerializer.Serialize(parsedList, ignoreNullCheckBox.Checked ? jsonOptionsIgnoreNull : jsonOptions);
            ignoreNextChange = true;
            richTextBox2.Text = text;
        }
        catch (Exception e)
        {
            errorMessage.Text = $"Error: {e.Message}";
        }
    }

    private static (string first, List<(char, string)> parts) CustomSplit(string s)
    {
        var indexes = new List<int>();
        for (int i = 0; i < s.Length; i++)
        {
            if (s[i] == '.' || s[i] == '%')
                indexes.Add(i);
            else if (s[i] == '[')
            {
                i++;
                while (s[i] != ']')
                    i++;
            }
        }

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
        foreach (var je in elements)
        {
            if (je.ValueKind == JsonValueKind.Object && je.TryGetProperty(property, out var value))
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
                            if (TryGetNumber(item, out double d))
                            {
                                if (set.Add(d + ""))
                                    returnList.Add(item);
                            }
                        }
                        else if (item.ValueKind == JsonValueKind.True || item.ValueKind == JsonValueKind.False)
                        {
                            if (set.Add(item.ValueKind == JsonValueKind.True ? "true" : "false"))
                                returnList.Add(item);
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
                            if (TryGetNumber(item, out double d))
                            {
                                var key = d + "";
                                counts.TryGetValue(key, out int n);
                                counts[key] = n + 1;
                            }
                        }
                        else if (item.ValueKind == JsonValueKind.True || item.ValueKind == JsonValueKind.False)
                        {
                            var key = item.ValueKind == JsonValueKind.True ? "true" : "false";
                            counts.TryGetValue(key, out int n);
                            counts[key] = n + 1;
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

    private static (string first, List<string> rest) SplitFilter(string part)
    {
        var i = part.IndexOf('[');
        if (i < 0)
            return (part, []);

        var first = part[..i];
        var parts = new List<string>();
        while (true)
        {
            var i2 = part.IndexOf('[', i + 1);
            if (i2 < 0)
            {
                parts.Add(part[i..]);
                return (first, parts);
            }

            parts.Add(part[i..i2]);
            i = i2;
        }
    }

    private static bool TryGetNumber(JsonElement je, out double value)
    {
        if (je.TryGetInt32(out int n))
        {
            value = n;
            return true;
        }
        if (je.TryGetDouble(out double d))
        {
            value = d;
            return true;
        }

        value = default;
        return false;
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
        else if (filterExpression[1] == '?')
        {
            // Sample expression: [?(@.id < 2)]
            if (filterExpression[2] != '(') throw new Exception();
            if (filterExpression[^2] != ')') throw new Exception();

            var expressionText = filterExpression[3..^2];
            var expression = ToExpressionTree(expressionText);

            return mappedValues.Where(x => IsTruthy(expression.Evaluate(x)));
        }
        else
        {
            // TODO handle filtering
            throw new Exception();
        }
    }

    private static IExpression ToExpressionTree(string s)
    {
        var tokens = GetTokens(s);
        IExpression firstExpr = GetExpression(tokens[0]);
        int i = 1;
        while (i < tokens.Count)
        {
            var op = tokens[i++];
            var precedence = GetPrecedence(op);
            if (op == ".")
            {
                var prop = tokens[i++];
                AddToNode(ref firstExpr, precedence, expression =>
                {
                    return new DotAccessExpression { Expression = expression, Prop = prop };
                });
                continue;
            }

            var expr = GetExpression(tokens[i++]);
            AddToNode(ref firstExpr, precedence, expression =>
            {
                var newOne = new TreeExpression
                {
                    First = expression,
                    Precedence = precedence,
                    Rest = [(op, expr)],
                };
                return newOne;
            });
        }

        return firstExpr;
    }

    private static char[] singleCharOperators = { '@', '.', '<', '>' };
    private static string[] doubleCharOperators = { "==", "!=", ">=", "<=", "&&", "||" };
    private static List<string> GetTokens(string s)
    {
        var tokens = new List<string>();

        int i = 0;
        while (i < s.Length)
        {
            while (i < s.Length && s[i] == ' ')
                i++;

            if (i < s.Length - 1 && doubleCharOperators.Contains(s[i..(i + 2)]))
            {
                tokens.Add(s[i..(i + 2)]);
                i += 2;
                continue;
            }
            else if (singleCharOperators.Contains(s[i]))
            {
                tokens.Add(s[i].ToString());
                i++;
                continue;
            }
            else if (IsLetter(s[i]))
            {
                var start = i++;
                while (i < s.Length && IsLetter(s[i]))
                    i++;
                tokens.Add(s[start..i]);
                continue;
            }
            else if ((s[i] >= '0' && s[i] <= '9') || s[i] == '-')
            {
                var start = i++;
                while (i < s.Length && ((s[i] >= '0' && s[i] <= '9') || s[i] == '.'))
                    i++;
                tokens.Add(s[start..i]);
                continue;
            }
            else if (s[i] == '\'' || s[i] == '"')
            {
                var endChar = s[i];
                var start = i++;
                while (s[i] != endChar)
                    i++;
                i++;
                tokens.Add(s[start..i]);
                continue;
            }

            throw new Exception();
        }

        return tokens;
    }

    private static bool IsLetter(char c)
    {
        if (c >= 'a' && c <= 'z') return true;
        if (c >= 'A' && c <= 'Z') return true;
        return false;
    }

    private static int GetPrecedence(string s)
    {
        return s switch
        {
            "." => 17,
            ">" => 9,
            ">=" => 9,
            "<" => 9,
            "<=" => 9,
            "==" => 9,
            "!=" => 9,
            "&&" => 4,
            "||" => 3,
            _ => throw new Exception($"Unknown operator: '{s}'"),
        };
    }

    private static IExpression GetExpression(string s)
    {
        if (s == "@")
            return new AtExpression();
        if (s == "null")
            return new ValueExpression { Value = null };
        if (s == "true")
            return new ValueExpression { Value = true };
        if (s == "false")
            return new ValueExpression { Value = false };
        if (s[0] == '-' || (s[0] >= '0' && s[0] <= '9'))
            return new ValueExpression { Value = double.Parse(s, CultureInfo.InvariantCulture) };
        if (s[0] == '\'' || s[0] == '"')
            return new ValueExpression { Value = s[1..^1] };
        throw new Exception();
    }

    private static void AddToNode(ref IExpression expression, int precedence, Func<IExpression, IExpression> handler)
    {
        if (expression is TreeExpression treeExpression && precedence >= treeExpression.Precedence)
        {
            TreeExpression lowestTreeExpression = treeExpression;

            while (lowestTreeExpression.Rest[^1].expression is TreeExpression subTree && precedence > subTree.Precedence)
            {
                lowestTreeExpression = subTree;
            }

            var (treeLastElementOperator, treeLastElementExpression) = lowestTreeExpression.Rest[^1];

            if (precedence == treeExpression.Precedence)
            {
                var _ = handler(lowestTreeExpression);
            }
            else
            {
                var newExpression = handler(treeLastElementExpression);
                lowestTreeExpression.Rest[^1] = (treeLastElementOperator, newExpression);
            }
        }
        else
        {
            expression = handler(expression);
        }
    }

    private static object? GetValue(object? o)
    {
        if (o is JsonElement je)
        {
            if (je.ValueKind == JsonValueKind.Number)
            {
                if (TryGetNumber(je, out double d))
                    return d;
                throw new Exception();
            }
            if (je.ValueKind == JsonValueKind.String)
                return je.GetString();
            if (je.ValueKind == JsonValueKind.Null)
                return null;
            if (je.ValueKind == JsonValueKind.True)
                return true;
            if (je.ValueKind == JsonValueKind.False)
                return false;
        }
        return o;
    }

    private static bool IsTruthy(object? o)
    {
        o = GetValue(o);

        if (o == null) return false;
        if (o is bool b) return b;
        if (o is string s) return s.Length > 0;
        if (o is double d) return d != 0.0;
        return true;
    }

    private static bool Compare(object? o1, string token, object? o2)
    {
        o1 = GetValue(o1);
        o2 = GetValue(o2);

        if (o1 is double d1 && o2 is double d2)
        {
            return token switch
            {
                ">" => d1 > d2,
                "<" => d1 < d2,
                ">=" => d1 >= d2,
                "<=" => d1 <= d2,
                "==" => d1 == d2,
                "!=" => d1 != d2,
                _ => throw new Exception(),
            };
        }

        if (token == "==")
            return object.Equals(o1, o2);
        if (token == "!=")
            return !object.Equals(o1, o2);

        return false;
    }

    class TreeExpression : IExpression
    {
        public required int Precedence { get; set; }
        public required IExpression First { get; set; }
        public required List<(string token, IExpression expression)> Rest { get; set; }

        public object? Evaluate(JsonElement at)
        {
            var value = First.Evaluate(at);
            foreach (var (token, expression) in Rest)
            {
                var exprValue = expression.Evaluate(at);
                value = token switch
                {
                    "<" => Compare(value, token, exprValue),
                    ">" => Compare(value, token, exprValue),
                    ">=" => Compare(value, token, exprValue),
                    "<=" => Compare(value, token, exprValue),
                    "==" => Compare(value, token, exprValue),
                    "!=" => Compare(value, token, exprValue),
                    "&&" => IsTruthy(value) && IsTruthy(exprValue),
                    "||" => IsTruthy(value) || IsTruthy(exprValue),
                    _ => throw new Exception(),
                };
            }
            return value;
        }
    }

    interface IExpression
    {
        object? Evaluate(JsonElement at);
    }

    class AtExpression : IExpression
    {
        public object? Evaluate(JsonElement at)
        {
            return at;
        }
    }

    class DotAccessExpression : IExpression
    {
        public required IExpression Expression { get; set; }
        public required string Prop { get; set; }

        public object? Evaluate(JsonElement at)
        {
            var baseValue = Expression.Evaluate(at);
            if (baseValue == null)
                return null;

            var je = (JsonElement)baseValue;
            if (je.ValueKind == JsonValueKind.Object && je.TryGetProperty(Prop, out var value))
            {
                return value;
            }
            return null;
        }
    }

    class ValueExpression : IExpression
    {
        public required object? Value { get; set; }

        public object? Evaluate(JsonElement at)
        {
            return Value;
        }
    }
}
