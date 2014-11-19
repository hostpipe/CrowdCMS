using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CMS.Utils
{
    public class ErrorInfo
    {
        public string Key;
        public ModelErrorCollection Errors;
    }
    public class DebugHelper
    {

        public static ErrorInfo[] ModelStateErrors(ModelStateDictionary modelState)
        {
            return modelState.Where(x => x.Value.Errors.Count > 0)
            .Select(x => new ErrorInfo { Key = x.Key, Errors = x.Value.Errors })
            .ToArray();
        }
    }
}