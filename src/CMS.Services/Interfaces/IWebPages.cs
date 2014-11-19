using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMS.BL.Entity;
using System.Web.Mvc;

namespace CMS.Services
{
    public interface IWebPages
    {
        bool DeleteFormItem(int formItemID);
        List<tbl_FormItem> GetAllFormItems();
        List<tbl_FormItem> GetAllFormItemsLiveByFormID(int formID);
        SelectList GetAllFormItemTypes();

        int GetFormItemTypeIDByName(string name);
        tbl_FormItem GetFormItemByID(int formItemID);
        tbl_FormItem SaveFormItem(string name, string text, int itemTypeID, bool required, int formID, int formItemID, string placeholder);
        tbl_FormItem SaveFormItemVisibility(int formItemID);
        bool SaveFormItemsOrder(int[] orderedContactItemIDs);

        bool DeleteFormItemValue(int formItemValueID);
        List<tbl_FormItemValues> GetAllFormItemValues();
        tbl_FormItemValues GetFormItemValueByID(int formItemValueID);
        tbl_FormItemValues SaveFormItemValue(int value, string text, bool selected, int order, int formItemID, int formItemValueID);

        bool DeleteFormSubmission(int formSubmissionID);
        List<tbl_FormSubmission> GetAllFormSubmissions();
        List<tbl_FormSubmission> GetFormSubmissionsByFormID(int formID);
        tbl_FormSubmission GetFormSubmissionByID(int formSubmissionID);
        tbl_FormSubmission MarkFormSubmissionAsRead(int formSubmissionID);
        List<KeyValuePair<string, string>> SaveFormSubmission(FormCollection values, string recipients, DateTime date, int formID, int formSubmissionID = 0);

        tbl_Form GetFormByID(int formID);
        bool DeleteForm(int formID);
        List<tbl_Form> GetAllForms();
        List<tbl_Form> GetAllFormsByDomainID(int domainID);
        tbl_Form SaveForm(string name, string description, int domainID, int formID, bool captcha = true, int? sitemapID = null, string tracking = null);
                
        bool DeleteTestimonial(int testimonialID);
        List<tbl_Testimonials> GetAllTestimonials();
        List<tbl_Testimonials> GetAllTestimonialsLive();
        tbl_Testimonials GetTestimonialByID(int testimonialID);
        tbl_Testimonials SaveTestimonialVisibility(int testimonialID);
        tbl_Testimonials SaveTestimonial(int testimonialID, string testimonialDate, string client, string company, string content);

        List<tbl_CustomLayout> GetAllCustomLayouts();
        tbl_CustomLayout GetCustomLayoutById(int layoutId);
        tbl_CustomLayout SaveCustomLayout(int layoutId, string dir, string name);
        SelectList GetAllCustomlayoutsAsSelectlist(int customLayoutId);
    }
}
