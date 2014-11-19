using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ServiceModel.Syndication;
using System.Xml;

namespace CMS.UI
{
    public class RssResult: ActionResult
    {
        private SyndicationFeed _feed;
        private string _contentType;

        public string ContentType
        {
            get { return _contentType; }
            set { _contentType = value; }
        }

        public SyndicationFeed Feed
        {
            get { return _feed; }
            set { _feed = value; }
        }

        public RssResult(SyndicationFeed feed)
        {
            this.Feed = feed;
        }

        public RssResult(SyndicationFeed feed, string contentType)
        {
            this.Feed = feed;
            this.ContentType = contentType;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            context.HttpContext.Response.ContentType = "text/xml";

            if (Feed != null)
                using (var rssWriter = new XmlTextWriter(context.HttpContext.Response.Output))
                {
                    rssWriter.Formatting = Formatting.Indented;
                    Rss20FeedFormatter rssFormatter = new Rss20FeedFormatter(Feed);
                    rssFormatter.WriteTo(rssWriter);
                }
        }
    }
}