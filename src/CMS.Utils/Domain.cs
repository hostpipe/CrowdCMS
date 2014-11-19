using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CMS.Utils
{
    public static class Domain
    {
        public static Regex CreateRegex(string source)
        {
            // Perform replacements
            source = source.Replace("/", @"\/?");
            source = source.Replace(".", @"\.?");
            source = source.Replace("-", @"\-?");
            if (source.Contains("{*"))
            {
                source = source.Replace("{*", @"(?<");
                source = source.TrimEnd('}') + @">([a-zA-Z0-9_*\-\/]*))";
            }

            source = source.Replace("{", @"(?<");
            source = source.Replace("}", @">([a-zA-Z0-9_*\-]*))");

            return new Regex("^" + source + "$");
        }

        public static string GetDomain(System.Web.HttpContextBase httpContext)
        {
            string requestDomain = httpContext.Request.Headers["host"];
            if (!string.IsNullOrEmpty(requestDomain))
            {
                if (requestDomain.IndexOf(":") > 0)
                {
                    requestDomain = requestDomain.Substring(0, requestDomain.IndexOf(":"));
                }
            }
            else
            {
                requestDomain = httpContext.Request.Url.Host;
            }
            return requestDomain;
        }

        public static string GetPath(System.Web.HttpContextBase httpContext)
        {
            string requestPath = httpContext.Request.AppRelativeCurrentExecutionFilePath.Substring(2) + httpContext.Request.PathInfo;
            return requestPath;
        }

        public static List<KeyValuePair<string, string>> GetRouteData(System.Web.HttpContextBase httpContext, string domain, string url)
        {
            // Build regex
            Regex domainRegex = Utils.Domain.CreateRegex(domain);
            Regex pathRegex = Utils.Domain.CreateRegex(url);

            // Request information
            string requestDomain = Utils.Domain.GetDomain(httpContext);
            string requestPath = Utils.Domain.GetPath(httpContext);

            // Match domain and route
            Match domainMatch = domainRegex.Match(requestDomain);
            Match pathMatch = pathRegex.Match(requestPath);

            // Route data
            List<KeyValuePair<string, string>> result = null;
            if (domainMatch.Success && pathMatch.Success)
            {
                result = new List<KeyValuePair<string, string>>();

                // Iterate matching domain groups
                for (int i = 1; i < domainMatch.Groups.Count; i++)
                {
                    Group group = domainMatch.Groups[i];
                    if (group.Success)
                    {
                        string key = domainRegex.GroupNameFromNumber(i);

                        if (!string.IsNullOrEmpty(key) && !char.IsNumber(key, 0))
                        {
                            if (!string.IsNullOrEmpty(group.Value))
                            {
                                result.Add(new KeyValuePair<string, string>(key, group.Value));
                            }
                        }
                    }
                }

                // Iterate matching path groups
                for (int i = 1; i < pathMatch.Groups.Count; i++)
                {
                    Group group = pathMatch.Groups[i];
                    if (group.Success)
                    {
                        string key = pathRegex.GroupNameFromNumber(i);

                        if (!string.IsNullOrEmpty(key) && !char.IsNumber(key, 0))
                        {
                            if (!string.IsNullOrEmpty(group.Value))
                            {
                                result.Add(new KeyValuePair<string, string>(key, group.Value));
                            }
                        }
                    }
                }
            }

            return result;
        }
    }
}
