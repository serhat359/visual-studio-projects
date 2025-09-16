using System.Collections;
using System.Globalization;
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
            PlaceholderText = "$.store[?(@.id > 5)].books[*].author%c%k%u%kc%uc",
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
        errorMessage.Width = 900;
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
        IExpression firstExpr = tokens[0] == "@" ? new AtExpression() : throw new Exception("Expected '@' character");
        int i = 1;
        while (i < tokens.Count)
        {
            var op = tokens[i++];
            if (op == ".")
            {
                var prop = tokens[i++];
                firstExpr = new DotAccessExpression { Expression = firstExpr, Prop = prop };
                continue;
            }

            var precedence = GetPrecedence(op);
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
    private static string[] doubleCharOperators = { "==", "!=", ">=", "<=" };
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
            _ => throw new Exception($"Unknown operator: '{s}'"),
        };
    }

    private static IExpression GetExpression(string s)
    {
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
