using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CMS.Utils.Web;
using CMS.UI.Models;
using CMS.BL.Entity;

namespace CMS.UI
{
#pragma warning disable 1591
    /// <summary>
    /// Session variables
    /// </summary>
    public enum SessionVariables
    {
        CreditCard,
        SampleVariable,
        AdminBasket
    }
#pragma warning restore 1591

    public class SessionManager : SessionManagerBase<SessionVariables>
    {
        public static CreditCardModel CreditCard
        {
            get { return GetObject(SessionVariables.CreditCard) as CreditCardModel; }
            set { Store(SessionVariables.CreditCard, value); }
        }

        public static tbl_Basket AdminBasket
        {
            get { return GetObject(SessionVariables.AdminBasket) as tbl_Basket; }
            set { Store(SessionVariables.AdminBasket, value); }
        }

        public static int SampleVariable
        {
            get { return GetInt(SessionVariables.SampleVariable).GetValueOrDefault(0); }
            set { Store(SessionVariables.SampleVariable, value); }
        }

        public static string SessionID
        {
            get { return HttpContext.Current.Session.SessionID; }
        }

    }
}