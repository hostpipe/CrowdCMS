using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.Services
{
    public interface ICRM
    {
        bool Subscribe(string email, int domainID);
        bool UnSubscribe(string email, int domainID);
        bool GetSubscriptionStatus(string email, int domainID);
    }
}
