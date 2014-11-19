using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.DAL.Repository;
using CMS.BL.Entity;
using System.Globalization;
using Microsoft.Security.Application;
using System.Web.Mvc;
using System.Data.Objects.SqlClient;
using CMS.Utils.Extension;

namespace CMS.Services
{
    public class WebPages : ServiceBase, IWebPages
    {
        private ITestimonialsRepository TestimonialsRepository { get; set; }
        private IFormItemRepository FormItemRepository { get; set; }
        private IFormItemValuesRepository FormItemValuesRepository { get; set; }
        private IFormRepository FormRepository { get; set; }
        private IFormSubmissionRepository FormSubmissionRepository { get; set; }
        private ICustomLayoutRepository CustomLayoutRepository { get; set; }

        public WebPages()
            : base()
        {
            this.TestimonialsRepository = new TestimonialsRepository(this.Context);
            this.FormRepository = new FormRepository(this.Context);
            this.FormItemRepository = new FormItemRepository(this.Context);
            this.FormItemValuesRepository = new FormItemValuesRepository(this.Context);
            this.FormSubmissionRepository = new FormSubmissionRepository(this.Context);
            this.CustomLayoutRepository = new CustomLayoutRepository(this.Context);
        }

        #region Form Item repo

        public tbl_FormItem GetFormItemByID(int formItemID)
        {
            return FormItemRepository.GetByID(formItemID);
        }

        public bool DeleteFormItem(int formItemID)
        {
            return FormItemRepository.DeleteByID(formItemID);
        }

        public List<tbl_FormItem> GetAllFormItems()
        {
            return FormItemRepository.GetAll().ToList();
        }

        public List<tbl_FormItem> GetAllFormItemsLiveByFormID(int formID)
        {
            return FormItemRepository.GetAllLiveByFormID(formID).ToList();
        }

        public SelectList GetAllFormItemTypes()
        {
            return new SelectList(FormItemRepository.GetAllFormItemTypes(), "FormItemTypeID", "FIT_Name");
        }

        public int GetFormItemTypeIDByName(string name)
        {
            var formItemType = FormItemRepository.GetFormItemTypeByName(name);
            return (formItemType != null) ? 
                formItemType.FormItemTypeID : 
                0;
        }

        public tbl_FormItem SaveFormItem(string name, string text, int itemTypeID, bool required, int formID, int formItemID, string placeholder)
        {
            return (String.IsNullOrEmpty(name)) ?
                null:
                FormItemRepository.SaveFormItem(name, text, itemTypeID, required, formID, formItemID, placeholder);
        }

        public tbl_FormItem SaveFormItemVisibility(int formItemID)
        {
            return (formItemID == 0) ?
                null:
                FormItemRepository.SaveVisibility(formItemID);
        }

        public bool SaveFormItemsOrder(int[] orderedFormItemIDs)
        {
            return FormItemRepository.SaveOrder(orderedFormItemIDs);
        }

        #endregion


        #region Form Item Values repo

        public bool DeleteFormItemValue(int formItemValueID)
        {
            return FormItemValuesRepository.DeleteByID(formItemValueID);
        }

        public List<tbl_FormItemValues> GetAllFormItemValues()
        {
            return FormItemValuesRepository.GetAll().ToList();
        }

        public tbl_FormItemValues GetFormItemValueByID(int formItemValueID)
        {
            return FormItemValuesRepository.GetByID(formItemValueID);
        }

        public tbl_FormItemValues SaveFormItemValue(int value, string text, bool selected, int order, int formItemID, int formItemValueID)
        {
            return FormItemValuesRepository.SaveFormItemValue(value, text, selected, order, formItemID, formItemValueID);
        }

        #endregion


        #region Form submission repo

        public bool DeleteFormSubmission(int formSubmissionID)
        {
            return FormSubmissionRepository.DeleteByID(formSubmissionID);
        }

        public List<tbl_FormSubmission> GetAllFormSubmissions()
        {
            return FormSubmissionRepository.GetAll().ToList();
        }

        public List<tbl_FormSubmission> GetFormSubmissionsByFormID(int formID)
        {
            return FormSubmissionRepository.GetByFormID(formID).ToList();
        }

        public tbl_FormSubmission GetFormSubmissionByID(int formSubmissionID)
        {
            return formSubmissionID == 0 ? FormSubmissionRepository.GetNewest() : FormSubmissionRepository.GetByID(formSubmissionID);
        }

        public tbl_FormSubmission MarkFormSubmissionAsRead(int formSubmissionID)
        {
            return FormSubmissionRepository.MarkAsRead(formSubmissionID);
        }

        public List<KeyValuePair<string, string>> SaveFormSubmission(FormCollection values, string recipients, DateTime date, int formID, int formSubmissionID = 0)
        {
            Dictionary<string, string> dictValues = values.AllKeys.ToDictionary(k => k, v => values[v]);
            string stringValues = String.Empty;
            foreach (var item in dictValues)
            {
                if (item.Key.Contains("recaptcha") || item.Key.Contains("FormID") || item.Key.Contains("RecipientEmail") || item.Key.Contains("subscribe"))
                    continue;

                if (item.Key.Contains("radio"))
                {
                    var itemID = int.Parse(item.Key.Replace("radio", ""));
                    var contactItem = FormItemRepository.GetByID(itemID);
                    var value = contactItem.tbl_FormItemValues.FirstOrDefault(fiv => fiv.FIV_Value == int.Parse(item.Value));
                    stringValues += String.Format("{0}:{1}###", contactItem.FI_Text, value.FIV_Text);
                    continue;
                }

                if (item.Key.Contains("select"))
                {
                    var itemID = int.Parse(item.Key.Replace("select", ""));
                    var contactItem = FormItemRepository.GetByID(itemID);
                    var value = contactItem.tbl_FormItemValues.FirstOrDefault(fiv => fiv.FIV_Value == int.Parse(item.Value));
                    stringValues += String.Format("{0}:{1}###", contactItem.FI_Text, value.FIV_Text);
                    continue;
                }

                if (item.Key.Contains("checkbox"))
                {
                    var itemID = int.Parse(item.Key.Replace("checkbox", ""));
                    var contactItem = FormItemRepository.GetByID(itemID);
                    foreach (var key in item.Value.Split(','))
                    {
                        var value = contactItem.tbl_FormItemValues.FirstOrDefault(fiv => fiv.FIV_Value == int.Parse(key));
                        stringValues += String.Format("{0}:{1}###", contactItem.FI_Text, value.FIV_Text);
                    }
                    continue;
                }

                stringValues += String.Format("{0}:{1}###", item.Key, item.Value);
            }
            stringValues.TrimEnd('#');

            List<KeyValuePair<string, string>> listValues = stringValues.Split(new[] { "###" }, StringSplitOptions.RemoveEmptyEntries).Select(s => new KeyValuePair<string, string>(s.Substring(0, s.IndexOf(':')), s.Substring(s.IndexOf(':') + 1))).ToList();
            FormSubmissionRepository.SaveFormSubmission(stringValues, recipients, date, formID, formSubmissionID);
            return listValues;
        }

        #endregion


        #region Form repo

        public tbl_Form GetFormByID(int formID)
        {
            return FormRepository.GetByID(formID);
        }

        public List<tbl_Form> GetAllForms()
        {
            return FormRepository.GetAll().ToList();
        }

        public List<tbl_Form> GetAllFormsByDomainID(int domainID)
        {
            return FormRepository.GetByDomainID(domainID).ToList();
        }

        public tbl_Form SaveForm(string name, string description, int domainID, int formID, bool captcha = true, int? sitemapID = null, string tracking = null)
        {
            return FormRepository.SaveForm(name, description, domainID, formID, captcha, sitemapID, tracking);
        }

        public bool DeleteForm(int formID)
        {
            return FormRepository.DeleteByID(formID);
        }

        #endregion


        #region Testimonials repo

        public bool DeleteTestimonial(int testimonialID)
        {
            return TestimonialsRepository.DeleteTestimonial(testimonialID);
        }

        public List<tbl_Testimonials> GetAllTestimonials()
        {
            return TestimonialsRepository.GetAll().OrderByDescending(t => t.TE_Date).ToList();
        }

        public List<tbl_Testimonials> GetAllTestimonialsLive()
        {
            return TestimonialsRepository.GetAll().Where(t => t.TE_Live).OrderByDescending(t => t.TE_Date).ToList();
        }

        public tbl_Testimonials GetTestimonialByID(int testimonialID)
        {
            return TestimonialsRepository.GetByID(testimonialID);
        }

        public tbl_Testimonials SaveTestimonialVisibility(int testimonialID)
        {
            if (testimonialID == 0)
                return null;

            return TestimonialsRepository.SaveVisibility(testimonialID);
        }

        public tbl_Testimonials SaveTestimonial(int testimonialID, string testimonialDate, string client, string company, string content)
        {
            if (String.IsNullOrEmpty(client) || String.IsNullOrEmpty(company) || String.IsNullOrEmpty(testimonialDate))
                return null;

            DateTime date = testimonialDate.ParseDateTime();

            return TestimonialsRepository.SaveTestimonial(testimonialID, date, Sanitizer.GetSafeHtmlFragment(client), Sanitizer.GetSafeHtmlFragment(company),
                content ?? String.Empty);
        }

        #endregion

        #region Custom Layout repo

        public List<tbl_CustomLayout> GetAllCustomLayouts()
        {
            return CustomLayoutRepository.GetALL().ToList();
        }

        public SelectList GetAllCustomlayoutsAsSelectlist(int selectedId)
        {
            var items = CustomLayoutRepository.GetALL().ToList();
            items.Add(new tbl_CustomLayout() { CL_DisplayName = "", CL_Directory = "", CustomLayoutID = 0 });
            items = items.OrderBy(x => x.CustomLayoutID).ToList();
            return new SelectList(items, "CustomLayoutID", "CL_DisplayName", selectedId);
        }

        public tbl_CustomLayout GetCustomLayoutById(int layoutId)
        {
            return CustomLayoutRepository.GetByID(layoutId);
        }

        public tbl_CustomLayout SaveCustomLayout(int layoutId, string dir, string name)
        {
            return CustomLayoutRepository.SaveCustomLayout(layoutId, dir, name);
        }

        #endregion
    }
}
