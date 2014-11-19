using System;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace CMS.Utils.Extension
{
    public static class InputExtensions
    {
        public static MvcHtmlString NameLessTextBoxFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression)
        {
            const string pattern = @" name=""([^""]*)""";

            var textBox = htmlHelper.TextBoxFor(expression);
            
            var fixedHtml = Regex.Replace(textBox.ToHtmlString(), pattern, "");

            return new MvcHtmlString(fixedHtml);
        }

        public static MvcHtmlString NameLessTextBoxFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, object htmlAttributes)
        {
            const string pattern = @" name=""([^""]*)""";

            var textBox = htmlHelper.TextBoxFor(expression,htmlAttributes);

            var fixedHtml = Regex.Replace(textBox.ToHtmlString(), pattern, "");

            return new MvcHtmlString(fixedHtml);
        }
    }
}
