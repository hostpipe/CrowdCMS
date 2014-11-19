using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CMS.Utils;
using System.IO;
using CMS.BL.Entity;
using System.Globalization;
using CMS.BL;
using CMS.Services.Extensions;

namespace CMS.UI
{
    public class MailingService
    {
        /// <summary>
        /// MailingService variables (email templates names)
        /// </summary>
        public enum EmailVariables
        {
            ForgottenPassword,
            ContactForm,
            NewComment,
            WelcomeMessage,
            OrderConfirmation,
            OrderConfirmationAdmin,
            DonationConfirmation,
            DonationConfirmationAdmin,
            OrderCancelConfirmation
        }

        private static string GetTemplate(EmailVariables template, Dictionary<string, object> data, CultureInfo culture = null, string language = "")
        {
            string templateName = template.ToString();
            if (!String.IsNullOrEmpty(language))
                templateName += language;

            var templatePath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath(SettingsManager.Email.Templates.Path), templateName.ToString() + SettingsManager.Email.Templates.Type);

            return TemplateEngine.ProcessTemplate(templatePath, data, culture);
        }

        public static void SendForgottenPassword(string userName, string pass, string email, string domain, string path)
        {
            var templateData = new Dictionary<string, object>();
            templateData.Add("USER", userName);
            templateData.Add("PASS", pass);
            templateData.Add("DOMAIN", domain);
            templateData.Add("PATH", path);

            var template = GetTemplate(EmailVariables.ForgottenPassword, templateData);

            var recipients = new List<string>() { email };
            MailSender.Instance.SendEmail(recipients, null, null, "Forgotten password", template);
        }

        public static void SendWelcomeMessage(string userName, string email, string company, string domain, string comPhone)
        {
            var templateData = new Dictionary<string, object>();
            templateData.Add("FULLUSERNAME", userName);
            templateData.Add("COMPANY", company);
            templateData.Add("USEREMAIL", email);
            templateData.Add("PHONE", comPhone);
            templateData.Add("DOMAIN", domain);
            var template = GetTemplate(EmailVariables.WelcomeMessage, templateData);
            var recipients = new List<string>() { email };
            MailSender.Instance.SendEmail(recipients, null, null, String.Format("Welcome to {0}", domain), template);
        }

        public static void SendCustomForm(List<KeyValuePair<string, string>> items, string recipients)
        {
            if (!string.IsNullOrEmpty(recipients))
            {
                var templateData = new Dictionary<string, object>();
                templateData.Add("FORMITEMS", items);

                var template = GetTemplate(EmailVariables.ContactForm, templateData);

                MailSender.Instance.SendEmail(recipients.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList<string>(), null, null, "Contact Us", template);
            }
        }

        public static void SendNewComment(string postlink, string adminlink, string companyName, string domain, string recipient)
        {
            if (!string.IsNullOrEmpty(recipient))
            {
                var templateData = new Dictionary<string, object>();
                templateData.Add("POSTLINK", postlink);
                templateData.Add("ADMINLINK", adminlink);
                templateData.Add("COMPANYNAME", companyName);
                templateData.Add("DOMAIN", domain);

                var template = GetTemplate(EmailVariables.NewComment, templateData);

                var recipients = new List<string>() { recipient };
                MailSender.Instance.SendEmail(recipients, null, null, "New Comment", template);
            }
        }

        public static void SendOrderConfirmation(tbl_Orders order)
        {
            if (order != null && !string.IsNullOrEmpty(order.CustomerEMail))
            {
                var templateData = new Dictionary<string, object>();
                templateData.Add("ORDERDATE", order.O_Timestamp.GetValueOrDefault());
                templateData.Add("DELIVERYDATE", order.O_DeliveryDate ?? String.Empty);
                templateData.Add("DOMAIN", order.tbl_Domains.DO_Domain);
                templateData.Add("ORDERID", order.OrderID);
                templateData.Add("INSTRUCTION", order.O_DeliveryNotes ?? String.Empty);
                templateData.Add("ISDISCOUNT", order.O_DiscountID.GetValueOrDefault(0) != 0);
                templateData.Add("ISDONATION", order.DependentOrders.Any());
                templateData.Add("DISCOUNTAMOUNT", order.GetDiscountAmountString());
                templateData.Add("DELIVERYAMOUNT", order.GetDeliveryAmountString());
                templateData.Add("TOTALAMOUNT", order.GetPriceString());
                templateData.Add("TOTALAMOUNTPAID", order.TotalAmountToPay.ToString("C"));

                templateData.Add("BADDRESS1", order.BillingAddress1 ?? String.Empty);
                templateData.Add("BADDRESS2", order.BillingAddress2 ?? String.Empty);
                templateData.Add("BADDRESS3", order.BillingAddress3 ?? String.Empty);
                templateData.Add("BNAME", order.BillingFirstnames + " " + order.BillingSurname);
                templateData.Add("BTOWN", order.BillingCity ?? String.Empty);
                templateData.Add("BCOUNTY", order.BillingState ?? String.Empty);
                templateData.Add("BPOSTCODE", order.BillingPostCode ?? String.Empty);
                templateData.Add("BCOUNTRY", order.BillingCountry ?? String.Empty);
                templateData.Add("BPHONE", order.BillingPhone ?? String.Empty);

                templateData.Add("DADDRESS1", order.DeliveryAddress1 ?? String.Empty);
                templateData.Add("DADDRESS2", order.DeliveryAddress2 ?? String.Empty);
                templateData.Add("DADDRESS3", order.DeliveryAddress3 ?? String.Empty);
                templateData.Add("DNAME", order.DeliveryFirstnames + " " + order.DeliverySurname);
                templateData.Add("DTOWN", order.DeliveryCity ?? String.Empty);
                templateData.Add("DCOUNTY", order.DeliveryState ?? String.Empty);
                templateData.Add("DPOSTCODE", order.DeliveryPostCode ?? String.Empty);
                templateData.Add("DCOUNTRY", order.DeliveryCountry ?? String.Empty);
                templateData.Add("DPHONE", order.DeliveryPhone ?? String.Empty);

                var products = order.tbl_OrderContent.Select(oc => new
                {
                    Name = oc.OC_Title ?? String.Empty,
                    Price = oc.GetItemPriceString(),
                    Quantity = oc.OC_Quantity.GetValueOrDefault(0),
                    TotalPrice = oc.GetPriceString()
                }).ToList();

                if (order.DependentOrders.Any())
                {
                    products.AddRange(order.DependentOrders.Select(o => new
                    {
                        Name = "Donation",
                        Price = o.TotalAmount.ToString(),
                        Quantity = (short)1,
                        TotalPrice = o.TotalAmount.ToString()
                    }));
                }
                templateData.Add("PRODUCTS", products);

                // switch between donations, products and events templates 
                string template = order.O_ProductTypeID.HasValue && order.tbl_ProductTypes.PT_Name == ProductType.Donation.ToString() ?
                    GetTemplate(EmailVariables.DonationConfirmation, templateData) :
                    GetTemplate(EmailVariables.OrderConfirmation, templateData);

                // change email title accordingly
                string title = order.O_ProductTypeID.HasValue && order.tbl_ProductTypes.PT_Name == ProductType.Donation.ToString() ? "Donation Confirmation" : "Order Confirmation";

                var recipients = new List<string>() { order.CustomerEMail };
                MailSender.Instance.SendEmail(recipients, null, null, title, template);
            }
        }

        public static void SendOrderConfirmationAdmin(tbl_Orders order, string adminEmail)
        {
            if (order != null && !string.IsNullOrEmpty(adminEmail))
            {
                var templateData = new Dictionary<string, object>();
                templateData.Add("ORDERDATE", order.O_Timestamp.GetValueOrDefault());
                templateData.Add("DELIVERYDATE", order.O_DeliveryDate ?? String.Empty);
                templateData.Add("DOMAIN", order.tbl_Domains.DO_Domain);
                templateData.Add("ORDERID", order.OrderID);
                templateData.Add("INSTRUCTION", order.O_DeliveryNotes ?? String.Empty);
                templateData.Add("ISDISCOUNT", order.O_DiscountID.GetValueOrDefault(0) != 0);
                templateData.Add("ISDONATION", order.DependentOrders.Any());
                templateData.Add("DISCOUNTAMOUNT", order.GetDiscountAmountString());
                templateData.Add("DELIVERYAMOUNT", order.GetDeliveryAmountString());
                templateData.Add("TOTALAMOUNT", order.GetPriceString());
                templateData.Add("TOTALAMOUNTPAID", order.TotalAmountToPay.ToString("C"));

                templateData.Add("BADDRESS1", order.BillingAddress1 ?? String.Empty);
                templateData.Add("BADDRESS2", order.BillingAddress2 ?? String.Empty);
                templateData.Add("BADDRESS3", order.BillingAddress3 ?? String.Empty);
                templateData.Add("BNAME", order.BillingFirstnames + " " + order.BillingSurname);
                templateData.Add("BTOWN", order.BillingCity ?? String.Empty);
                templateData.Add("BCOUNTY", order.BillingState ?? String.Empty);
                templateData.Add("BPOSTCODE", order.BillingPostCode ?? String.Empty);
                templateData.Add("BCOUNTRY", order.BillingCountry ?? String.Empty);
                templateData.Add("BPHONE", order.BillingPhone ?? String.Empty);

                templateData.Add("DADDRESS1", order.DeliveryAddress1 ?? String.Empty);
                templateData.Add("DADDRESS2", order.DeliveryAddress2 ?? String.Empty);
                templateData.Add("DADDRESS3", order.DeliveryAddress3 ?? String.Empty);
                templateData.Add("DNAME", order.DeliveryFirstnames + " " + order.DeliverySurname);
                templateData.Add("DTOWN", order.DeliveryCity ?? String.Empty);
                templateData.Add("DCOUNTY", order.DeliveryState ?? String.Empty);
                templateData.Add("DPOSTCODE", order.DeliveryPostCode ?? String.Empty);
                templateData.Add("DCOUNTRY", order.DeliveryCountry ?? String.Empty);
                templateData.Add("DPHONE", order.DeliveryPhone ?? String.Empty);

                var products = order.tbl_OrderContent.Select(oc => new
                {
                    Name = oc.OC_Title ?? String.Empty,
                    Price = oc.GetItemPriceString(),
                    Quantity = oc.OC_Quantity.GetValueOrDefault(0),
                    TotalPrice = oc.GetPriceString()
                }).ToList();

                if (order.DependentOrders.Any())
                {
                    products.AddRange(order.DependentOrders.Select(o => new
                    {
                        Name = "Donation",
                        Price = o.TotalAmount.ToString(),
                        Quantity = (short)1,
                        TotalPrice = o.TotalAmount.ToString()
                    }));
                }
                templateData.Add("PRODUCTS", products);

                // switch between donations, products and events templates 
                string template = order.O_ProductTypeID.HasValue && order.tbl_ProductTypes.PT_Name == ProductType.Donation.ToString() ?
                    GetTemplate(EmailVariables.DonationConfirmationAdmin, templateData) :
                    GetTemplate(EmailVariables.OrderConfirmationAdmin, templateData);

                // change email title accordingly
                string title = order.O_ProductTypeID.HasValue && order.tbl_ProductTypes.PT_Name == ProductType.Donation.ToString() ? "Donation Confirmation" : "Order Confirmation";

                var recipients = new List<string>() { adminEmail };
                MailSender.Instance.SendEmail(recipients, null, null, title, template);
            }
        }

        public static void SendOrderCancelConfirmation(tbl_Orders order)
        {
            if (order != null && !string.IsNullOrEmpty(order.CustomerEMail))
            {
                var templateData = new Dictionary<string, object>();
                templateData.Add("$ORDERNO", order.OrderID.ToString());

                string template = GetTemplate(EmailVariables.OrderCancelConfirmation, templateData);

                var recipients = new List<string>() { order.CustomerEMail };
                MailSender.Instance.SendEmail(recipients, null, null, "Order Canceled", template);
            }
        }
    }
}