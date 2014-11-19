using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.IO;
using NVelocity.App;
using NVelocity;
using Commons.Collections;

namespace CMS.Utils
{
    public class TemplateEngine
    {
        public static string ProcessTemplate(string templatePath, IDictionary<string, object> templateData, CultureInfo culture = null)
        {
            if (culture == null)
            {
                culture = System.Threading.Thread.CurrentThread.CurrentCulture;
            }

            var resultWriter = new StringWriter(culture);
            VelocityEngine velocityEngine = new VelocityEngine();
            VelocityContext velocityContext = new VelocityContext();
            ExtendedProperties properties = new ExtendedProperties();

            velocityEngine.Init(properties);
            foreach (var entry in templateData ?? new Dictionary<string, object>())
            {
                velocityContext.Put(entry.Key, entry.Value);
            }

            StreamReader template = File.OpenText(templatePath);

            velocityEngine.Evaluate(velocityContext, resultWriter, string.Empty, template);

            return resultWriter.ToString();
        }
    }
}
