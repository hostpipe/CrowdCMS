using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TweetSharp;

namespace CMS.Utils
{
    public class TweetManager
    {
        private TwitterService Service;

        public TweetManager(string accessToken, string accessTokenSecret, string consumerKey, string consumerSecret)
        {
            Service = new TwitterService(consumerKey, consumerSecret);
            Service.AuthenticateWith(accessToken, accessTokenSecret);
        }

        public void SendTweet(string message)
        {
            SendTweetOptions options = new SendTweetOptions() {
                Status = message
            };

            Service.SendTweet(options);
        }

        public List<string> GetLatestTweets(int count)
        {
            List<string> output = new List<string>();
            ListTweetsOnUserTimelineOptions options = new ListTweetsOnUserTimelineOptions()
            {
                Count = count,
                ExcludeReplies = true,

            };

            var tweets = Service.ListTweetsOnUserTimeline(options);

            if (tweets != null)
            {
                foreach (var tweet in tweets)
                {
                    output.Add(tweet.TextAsHtml);
                }
            }
            return output;
        }
    }
}
