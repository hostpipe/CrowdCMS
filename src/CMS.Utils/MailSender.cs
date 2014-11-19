using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using CMS.Utils.Diagnostics;

namespace CMS.Utils
{
    public class MailSender
    {
        private SmtpClient smtpClient;

        private static object syncLock = new object();
        private static MailSender _instance;

        public static MailSender Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncLock)
                    {
                        if (_instance == null)
                            _instance = new MailSender();
                    }
                }
                return _instance;
            }
        }

        private MailSender()
        {
            smtpClient = new SmtpClient();
        }

        private void SendEmailAs(MailAddress senderAddress, List<string> recipients, List<string> ccrecipients, List<string> bccrecipents, string subject, string body, List<string> replyTo)
        {
            try
            {
                using (MailMessage mailMessage = new MailMessage())
                {
                    if (senderAddress != null)
                        mailMessage.From = senderAddress;

                    if (recipients == null || recipients.Count == 0)
                        throw new Exception("Recipients collection can not be null or empty.");

                    string recipientsAddresses = recipients.Aggregate((address, next) => address = String.Concat(address, ", ", next));
                    mailMessage.To.Add(recipientsAddresses);

                    if (ccrecipients != null && ccrecipients.Count > 0)
                    {
                        string ccrecipientsAddresses = ccrecipients.Aggregate((address, next) => address = String.Concat(address, ", ", next));
                        mailMessage.CC.Add(ccrecipientsAddresses);
                    }

                    if(bccrecipents != null && bccrecipents.Count > 0)
                    {
                        string bccrecipientsAddresses = bccrecipents.Aggregate((address, next) => address = String.Concat(address, ", ", next));
                        mailMessage.Bcc.Add(bccrecipientsAddresses);
                    }

                    mailMessage.Subject = subject;
                    mailMessage.Body = body;

                    mailMessage.SubjectEncoding = Encoding.UTF8;
                    mailMessage.BodyEncoding = Encoding.UTF8;
                    mailMessage.IsBodyHtml = true;
                    mailMessage.Priority = MailPriority.Normal;
                    if (replyTo != null && replyTo.Count > 0)
                    {
                        string replyToAddress = replyTo.Aggregate((address, next) => address = String.Concat(address, ", ", next));
                        mailMessage.ReplyToList.Add(replyToAddress);
                    }
                    smtpClient.Send(mailMessage);
                }
            }
            catch (Exception e)
            {
                Log.Error<MailSender>(String.Format("Error when sending email. Error message: {0}", e.Message), e);
            }
        }

        public void SendEmail(List<string> recipients, List<string> ccrecipients, List<string> bccrecipients, string subject, string body)
        {
            SendEmailAs(null, recipients, ccrecipients, bccrecipients, subject, body, null);
        }

        public void SendEmail(List<string> recipients, List<string> ccrecipients, List<string> bccrecipients, string subject, string body, List<string> replyTo)
        {
            SendEmailAs(null, recipients, ccrecipients, bccrecipients, subject, body, replyTo);
        }

        public void SendEmail(List<string> recipients, List<string> ccrecipients, List<string> bccrecipients, string subject, string body, List<string> replyTo, string senderEmail, string senderName)
        {
            MailAddress sender = new MailAddress(senderEmail, senderName);
            SendEmailAs(sender, recipients, ccrecipients, bccrecipients, subject, body, replyTo);
        }
    }
}
