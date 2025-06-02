using System.Linq.Expressions;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SampleWebsite.ViewExtensions;

public static class ViewExtensions
{
    private static readonly EmptyContent emptyContent = new();

    class EmptyContent : IHtmlContent
    {
        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {

        }
    }

    class CustomHtmlContent(Action<TextWriter, HtmlEncoder> operation) : IHtmlContent
    {
        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            operation(writer, encoder);
        }
    }

    public static string ValidationClass<TModel, TResult>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TResult>> expression)
    {
        var modelState = htmlHelper.ViewContext.ModelState;
        if (modelState.Count == 0)
            return "";

        return IsValid(htmlHelper, expression, out _, out _) ? "is-valid" : "is-invalid";
    }

    public static bool IsValid<TModel, TResult>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TResult>> expression, out string propertyName, out ModelStateEntry? entry)
    {
        propertyName = ((MemberExpression)expression.Body).Member.Name;
        var modelState = htmlHelper.ViewContext.ModelState;
        if (modelState.IsValid)
        {
            entry = default;
            return true;
        }

        entry = modelState[propertyName];
        if (entry == null)
            return true;
        if (entry.ValidationState == ModelValidationState.Valid)
            return true;

        return false;
    }

    public static IHtmlContent ValidationFor<TModel, TResult>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TResult>> expression)
    {
        var modelState = htmlHelper.ViewContext.ModelState;
        if (modelState.Count == 0)
            return emptyContent;
        if (IsValid(htmlHelper, expression, out var propertyName, out var entry))
            return emptyContent;

        return new CustomHtmlContent((writer, encoder) =>
        {
            foreach (var error in entry!.Errors)
            {
                writer.Write("""<div class="invalid-feedback">""");
                encoder.Encode(writer, error.ErrorMessage);
                writer.Write("</div>");
            }
        });
    }
}