using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.UI.Models;
using System.Xml.Linq;
using System.Globalization;

namespace CMS.UI
{
    public class SitemapXML
    {
        public static void Create(List<WebsiteMenuModel> pages, string path, string domain)
        {
            XNamespace xn = "http://www.sitemaps.org/schemas/sitemap/0.9";
            XNamespace xsi = "http://www.sitemaps.org/schemas/sitemap/0.9";
            XDocument xDoc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
            XElement root = new XElement(xn + "urlset"
                //new XAttribute(XNamespace.Xmlns + "xsi", xsi.NamespaceName),
                //new XAttribute(xsi + "schemaLocation", "http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd")
                );
            CreateElements(pages, root, domain, xn);
            xDoc.Add(root);
            xDoc.Save(path);
        }

        private static void CreateElements(List<WebsiteMenuModel> pages, XElement root, string domain, XNamespace xn)
        {
            foreach (var page in pages)
            {
                XElement urlElement = new XElement(xn + "url");
                urlElement.Add(new XElement(xn + "loc", domain + page.Url));
                urlElement.Add(new XElement(xn + "lastmod", page.ModificationDate.ToString("yyyy-MM-dd")));
                urlElement.Add(new XElement(xn + "changefreq", GetFrequency(page.ModificationDate)));
                urlElement.Add(new XElement(xn + "priority", page.Priority.ToString("0.0")));

                root.Add(urlElement);

                if (page.SubMenuItems.Count > 0)
                    CreateElements(page.SubMenuItems, root, domain, xn);
            }
        }

        private static string GetFrequency(DateTime modificationDate)
        {
            if (DateTime.Now.Year != modificationDate.Year)
                return "yearly";
            else if (DateTime.Now.Month != modificationDate.Month)
                return "monthly";
            else if (DateTime.Now.DayOfYear < modificationDate.DayOfYear + 7)
                return "weekly";
            else 
                return "daily";
        }
    }
}
