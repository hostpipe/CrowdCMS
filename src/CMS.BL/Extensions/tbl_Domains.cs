namespace CMS.BL.Entity
{
    public partial class tbl_Domains
    {
        public bool IsAnyCRMEnabled
        {
            get
            {
                return this._DO_EnableMailChimp || this._DO_EnableCommuniGator;
            }
        }

        public string CookieConsentAllSites 
        {
            get; set;
        }

        public string CookieConsentSinglePage {
            get; set;
        }
    }
}
