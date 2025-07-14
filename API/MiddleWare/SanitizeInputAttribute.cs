using Pack.Common.Convertor;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Reflection;

namespace Monitoring_Support_Server.MiddleWare
{

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class SanitizeInputAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var sanitizer = new Ganss.Xss.HtmlSanitizer();

            if (filterContext.ActionArguments != null)
            {
                foreach (var parameter in filterContext.ActionArguments)
                {
                    if (parameter.Value != null)
                    {
                        if (parameter.Value.GetType() == typeof(string))
                        {
                            if (!string.IsNullOrWhiteSpace(string.Concat(parameter.Value)))
                                filterContext.ActionArguments[parameter.Key] = sanitizer.Sanitize(parameter.Value.ToString().ToStringSafe());
                        }
                        else
                        {
                            var properties = parameter.Value.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                               .Where(x => x.CanRead && x.CanWrite && x.PropertyType == typeof(string) && x.GetGetMethod(true).IsPublic && x.GetSetMethod(true).IsPublic);
                            foreach (var propertyInfo in properties)
                            {
                                if (propertyInfo.GetValue(parameter.Value) != null)
                                    propertyInfo.SetValue(parameter.Value,
                                        sanitizer.Sanitize(propertyInfo.GetValue(parameter.Value).ToString().ToStringSafe()));
                            }
                        }

                    }

                }
            }
        }
    }
}
