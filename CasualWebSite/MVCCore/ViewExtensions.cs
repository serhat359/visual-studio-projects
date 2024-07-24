using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MVCCore.View;

public static class ViewExtensions
{
    public static IHtmlContent VueEscapedText<T>(this IHtmlHelper<T> html, string text)
    {
        var encodedText = html.Encode(text).Replace("`", "\\`").Replace("$", "\\$");
        var escaped = $"v-text=\"`{encodedText}`\"";
        return html.Raw(escaped);
    }

    public static string EscapeVancat(this string text)
    {
        return text.Replace("{{", "{\\{");
    }
}
