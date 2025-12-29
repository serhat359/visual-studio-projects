using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JSONPath;

public partial class JSONPathForm : Form
{
    private bool ignoreNextChange = false;
    private RichTextBox richTextBox;
    private RichTextBox richTextBox2;
    private TextBox jsonPathTextBox;
    private CheckBox nullIfNotExistentCheckBox;
    private Label errorMessage;
    private CheckBox ignoreNullCheckBox;

    private object? parsed;
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
        this.richTextBox.TextChanged += (object? sender, EventArgs args) =>
        {
            try
            {
                if (ignoreNextChange)
                {
                    ignoreNextChange = false;
                    return;
                }

                try
                {
                    parsed = Deserialize(richTextBox.Text);
                }
                catch (Exception)
                {
                    errorMessage!.Text = $"Could not parse JSON";
                    return;
                }
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
            PlaceholderText = "$.store[*].books[?(@.id > 5)].[-ex,ex2].author%c%k%u%kc%uc%kcs%ucs",
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

    class JsonObjectConverter : JsonConverter<Dictionary<string, object?>>
    {
        private JsonSerializerOptions regularOptions;

        public JsonObjectConverter(JsonSerializerOptions regularOptions)
        {
            this.regularOptions = regularOptions;
        }

        public override Dictionary<string, object?> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Dictionary<string, object?> value, JsonSerializerOptions options)
        {
            if (GetObject(value, out var map))
            {
                writer.WriteStartObject();
                foreach (var pair in map)
                {
                    if (pair.Value == null)
                        continue;

                    writer.WritePropertyName(pair.Key);
                    JsonSerializer.Serialize(writer, pair.Value, options);
                }
                writer.WriteEndObject();
                return;
            }

            if (GetArray(value, out var list))
            {
                writer.WriteStartArray();
                foreach (var item in list)
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

            var jsonPathParts = CustomSplit(jsonPath);

            IEnumerable<object?> parsedList = new[] { this.parsed };

            var nullIfNotExistent = nullIfNotExistentCheckBox.Checked;
            foreach (var part in jsonPathParts)
            {
                if (part[0] == '.')
                    parsedList = GetProperty(parsedList, part[1..], nullIfNotExistent);
                else if (part[0] == '%')
                    parsedList = ApplyDirective(part[1..], parsedList);
                else
                    parsedList = GetFiltered(part, parsedList);
            }

            var text = JsonSerializer.Serialize(parsedList, ignoreNullCheckBox.Checked ? jsonOptionsIgnoreNull : jsonOptions);
            ignoreNextChange = true;
            richTextBox2.Text = text;
            richTextBox2.ScrollToCaret(); // Scroll to top
        }
        catch (Exception e)
        {
            errorMessage.Text = $"Error: {e.Message}";
        }
    }

    private static List<string> CustomSplit(string s)
    {
        if (s[0] != '$')
            throw new Exception();
        var parts = new List<string>();
        int i = 1;
        while (i < s.Length)
        {
            char c = s[i];
            if (c == '.' || c == '%')
            {
                int start = i++;
                if (s[i] == '[')
                {
                    while (s[i] != ']')
                        i++;
                    i++;
                    parts.Add(s[start..i]);
                }
                else
                {
                    while (i < s.Length && char.IsAsciiLetter(s[i]))
                        i++;
                    parts.Add(s[start..i]);
                }
                continue;
            }
            if (c == '[')
            {
                int start = i++;
                while (s[i] != ']')
                    i++;
                i++;
                parts.Add(s[start..i]);
                continue;
            }
            throw new Exception($"Unexpected char: {c}");
        }
        return parts;
    }

    private static IEnumerable<object?> GetProperty(IEnumerable<object?> elements, string property, bool nullIfNotExistent)
    {
        bool isExcludeMapping = false;
        string[]? mappingParts = null;
        if (property[0] == '[')
        {
            isExcludeMapping = property[1] == '-';
            int i = isExcludeMapping ? 2 : 1;
            mappingParts = property[i..^1].Split(",");
        }

        foreach (var item in elements)
        {
            if (mappingParts != null)
            {
                var map = new Dictionary<string, object?>();
                foreach (var pair in EnumerateObject(item))
                {
                    if (mappingParts.Contains(pair.Key) != isExcludeMapping)
                        map[pair.Key] = pair.Value;
                }
                yield return map;
            }
            else if (GetObject(item, out var map) && map.TryGetValue(property, out var value))
                yield return value;
            else if (nullIfNotExistent)
                yield return null;
        }
    }

    private static IEnumerable<object?> ApplyDirective(string type, IEnumerable<object?> elements)
    {
        bool sortByCountDesc = false;
        switch (type)
        {
            case "c":
                {
                    var count = elements.Count();
                    return [(double)count];
                }
            case "k":
                {
                    var set = new HashSet<string>();
                    foreach (var item in elements)
                    {
                        foreach (var pair in EnumerateObject(item))
                        {
                            set.Add(pair.Key);
                        }
                    }
                    return set;
                }
            case "kcs":
                sortByCountDesc = true;
                goto case "kc";
            case "kc":
                {
                    var counts = new Dictionary<string, int>();
                    foreach (var item in elements)
                    {
                        foreach (var pair in EnumerateObject(item))
                        {
                            counts.TryGetValue(pair.Key, out int n);
                            counts[pair.Key] = n + 1;
                        }
                    }
                    return [ConvertToSorted(counts, sortByCountDesc)];
                }
            case "u":
                {
                    var set = new HashSet<string>();
                    var returnList = new List<object?>();
                    foreach (var item in elements)
                    {
                        if (item is string s)
                        {
                            if (set.Add(s))
                                returnList.Add(item);
                        }
                        else if (TryGetDouble(item, out var d))
                        {
                            if (set.Add(d + ""))
                                returnList.Add(item);
                        }
                        else if (item is bool b)
                        {
                            if (set.Add(b ? "true" : "false"))
                                returnList.Add(item);
                        }
                    }
                    return returnList;
                }
            case "ucs":
                sortByCountDesc = true;
                goto case "uc";
            case "uc":
                {
                    var counts = new Dictionary<string, int>();
                    foreach (var item in elements)
                    {
                        if (item is string s)
                        {
                            var key = s;
                            counts.TryGetValue(key, out int n);
                            counts[key] = n + 1;
                        }
                        else if (TryGetDouble(item, out var d))
                        {
                            var key = d + "";
                            counts.TryGetValue(key, out int n);
                            counts[key] = n + 1;
                        }
                        else if (item is bool b)
                        {
                            var key = b ? "true" : "false";
                            counts.TryGetValue(key, out int n);
                            counts[key] = n + 1;
                        }
                    }
                    return [ConvertToSorted(counts, sortByCountDesc)];
                }
        }
        throw new Exception($"Unknown directive: '{type}'");
    }

    private static Dictionary<string, int> ConvertToSorted(Dictionary<string, int> o, bool sortByCountDesc)
    {
        if (sortByCountDesc)
            return o.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
        return o;
    }

    private static object? Deserialize(string s)
    {
        var parsed = JsonSerializer.Deserialize<JsonElement>(s);
        return JsonElementToObject(parsed);
    }

    static object? JsonElementToObject(JsonElement jsonElement)
    {
        switch (jsonElement.ValueKind)
        {
            case JsonValueKind.Null:
                return null;
            case JsonValueKind.True:
                return true;
            case JsonValueKind.False:
                return false;
            case JsonValueKind.Number:
                return jsonElement.Deserialize<double>();
            case JsonValueKind.String:
                return jsonElement.Deserialize<string>();
            case JsonValueKind.Array:
                return jsonElement.EnumerateArray().Select(JsonElementToObject).ToList();
            case JsonValueKind.Object:
                var map = new Dictionary<string, object?>();
                foreach (var pair in jsonElement.EnumerateObject())
                {
                    map[pair.Name] = JsonElementToObject(pair.Value);
                }
                return map;
        }
        throw new Exception($"Unexpected json kind: {jsonElement.ValueKind}");
    }

    private static Dictionary<string, object?> EnumerateObject(object? o)
    {
        return (o as Dictionary<string, object?>) ?? throw new Exception("could not cast to map");
    }

    private static bool GetArray(object? o, [MaybeNullWhen(false)] out List<object?> list)
    {
        if (o is List<object?> l)
        {
            list = l;
            return true;
        }
        list = default;
        return false;
    }

    private static bool GetObject(object? o, [MaybeNullWhen(false)] out Dictionary<string, object?> map)
    {
        if (o is Dictionary<string, object?> m)
        {
            map = m;
            return true;
        }
        map = default;
        return false;
    }

    private static IEnumerable<object?> GetFiltered(string filterExpression, IEnumerable<object?> mapped)
    {
        var mappedValues = mapped.SelectMany(x =>
        {
            if (GetArray(x, out var list))
                return list;
            else if (GetObject(x, out var m))
                return m.Select(x => x.Value);
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

    private static bool TryGetDouble(object? o, out double ret)
    {
        if (o is double d)
        {
            ret = d;
            return true;
        }
        if (o is int i)
        {
            ret = (double)i;
            return true;
        }
        ret = 0;
        return false;
    }

    private static bool IsTruthy(object? o)
    {
        if (o == null) return false;
        if (o is bool b) return b;
        if (o is string s) return s.Length > 0;
        if (TryGetDouble(o, out var d)) return d != 0.0;
        return true;
    }

    private static bool Compare(object? o1, string token, object? o2)
    {
        if (TryGetDouble(o1, out var d1) && TryGetDouble(o2, out var d2))
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

        public object? Evaluate(object? at)
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
        object? Evaluate(object? at);
    }

    class AtExpression : IExpression
    {
        public object? Evaluate(object? at)
        {
            return at;
        }
    }

    class DotAccessExpression : IExpression
    {
        public required IExpression Expression { get; set; }
        public required string Prop { get; set; }

        public object? Evaluate(object? at)
        {
            var baseValue = Expression.Evaluate(at);
            if (baseValue == null)
                return null;

            if (GetObject(baseValue, out var map) && map.TryGetValue(Prop, out var value))
            {
                return value;
            }
            return null;
        }
    }

    class ValueExpression : IExpression
    {
        public required object? Value { get; set; }

        public object? Evaluate(object? at)
        {
            return Value;
        }
    }
}
