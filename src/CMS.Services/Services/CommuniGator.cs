using CMS.BL.Entity;
using CMS.DAL.Logging;
using CMS.DAL.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Xml;

namespace CMS.Services
{
    public class CommuniGator : ServiceBase, ICRM
    {
        private IDomainsRepository DomainsRepository { get; set; }

        public CommuniGator()
            : base()
        {
            this.DomainsRepository = new DomainsRepository(this.Context);
        }

        public bool Subscribe(string email, int domainID)
        {
            CommuniGatorService.AuthHeader header = null;
            CommuniGatorService.SDKSoapClient client = GetClient(domainID, out header);
            if (client == null)
                return false;

            var userService = (IUser)DependencyResolver.Current.GetService(typeof(IUser));
            var customer = userService.GetCustomerByEmail(email, domainID);
            string contactXML = "<?xml version=\"1.0\"?>" + (customer != null ? CreateContactXML(email, customer.CU_FirstName, customer.CU_Surname) : CreateContactXML(email, String.Empty, String.Empty));

            try
            {
                var result = client.UpdateContact(header, contactXML);
                return !String.IsNullOrEmpty(result) && !result.StartsWith("Error Message") && result != "0" ? true : false;
            }
            catch (Exception e)
            {
                Log.Error("CommuniGator Subscribe Exception", e);
            }
            return false;
        }

        public bool UnSubscribe(string email, int domainID)
        {
            throw new NotImplementedException();
        }

        public bool GetSubscriptionStatus(string email, int domainID)
        {
            CommuniGatorService.AuthHeader header = null;
            CommuniGatorService.SDKSoapClient client = GetClient(domainID, out header);
            if (client == null)
                return false;

            try
            {
                string result = (string)client.ReturnContactRecord(header, email);
                if (String.IsNullOrEmpty(result))
                    return false;

                XmlDocument document = new XmlDocument();
                document.LoadXml(result);
                var elements = document.GetElementsByTagName("id");

                return elements != null && elements.Count > 0 ? true : false;
            }
            catch (Exception e)
            {
                Log.Error("CommuniGator Status Exception", e);
            }
            return false;
        }

        private CommuniGatorService.SDKSoapClient GetClient(int domainID, out CommuniGatorService.AuthHeader header)
        {
            tbl_Domains domain = DomainsRepository.GetByID(domainID);
            if (domain == null || !domain.DO_EnableCommuniGator)
            {
                header = null;
                return null;
            }

            header = new CommuniGatorService.AuthHeader();
            header.Password = domain.DO_CommuniGatorPassword;
            header.Username = domain.DO_CommuniGatorUserName;

            try
            {
                CommuniGatorService.SDKSoapClient client = new CommuniGatorService.SDKSoapClient();
                var result = client.AuthenticationCheck(header);

                if (!String.IsNullOrEmpty(result) && result.Equals("Success"))
                    return client;
                else
                    return null;
            }
            catch (Exception e)
            {
                Log.Error("CommuniGator Client Exception", e);
            }
            return null;
        }

        private string CreateContactXML(string email, string fName, string lName)
        {
            XmlDocument document = new XmlDocument();

            XmlNode cdEmailLogin = document.CreateNode(XmlNodeType.CDATA, email, "");
            cdEmailLogin.InnerText = email;
            XmlNode cdFirstName = document.CreateNode(XmlNodeType.CDATA, fName, "");
            cdFirstName.InnerText = fName;
            XmlNode cdLastName = document.CreateNode(XmlNodeType.CDATA, lName, "");
            cdLastName.InnerText = lName;

            XmlNode emailLogin = document.CreateNode(XmlNodeType.Element, "EmailLogin", "");
            emailLogin.AppendChild(cdEmailLogin);
            XmlNode firstName = document.CreateNode(XmlNodeType.Element, "FirstName", "");
            firstName.AppendChild(cdFirstName);
            XmlNode lastName = document.CreateNode(XmlNodeType.Element, "LastName", "");
            lastName.AppendChild(cdLastName);

            XmlNode boAttrs = document.CreateNode(XmlNodeType.Element, "sbBOAttrs", "");
            boAttrs.AppendChild(emailLogin);
            boAttrs.AppendChild(firstName);
            boAttrs.AppendChild(lastName);

            XmlNode boName = document.CreateNode(XmlNodeType.Element, "sbBOName", "");
            boName.InnerText = "Person";

            XmlNode contacts_rv = document.CreateNode(XmlNodeType.Element, "sbContacts_RV", "");

            XmlNode content = document.CreateNode(XmlNodeType.Element, "sbCommContent", "");
            content.AppendChild(boName);
            content.AppendChild(boAttrs);
            content.AppendChild(contacts_rv);

            return content.OuterXml;
        }

    }
}
