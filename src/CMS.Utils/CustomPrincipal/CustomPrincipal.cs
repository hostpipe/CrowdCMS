using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;

namespace CMS.Utils
{
    public class CustomPrincipal : IPrincipal
    {
        public string UserName { get; private set; }
        public string Email { get; private set; }
        public int UserID { get; private set; }
        public int? UserGroupID { get; private set; }
        public bool IsAdmn { get; private set; }
        public IIdentity Identity { get; private set; }

        private List<string> Permissions { get; set; }

        public CustomPrincipal(CustomPrincipalSerializeModel adminUser, int? groupID, List<string> permissions, bool authenticated)
        {
            UserID = adminUser.UserID;
            UserName = adminUser.UserName;
            IsAdmn = adminUser.IsAdmn;
            UserGroupID = groupID;
            Email = adminUser.Email;
            Identity = new CustomIdentity(adminUser.UserName, authenticated);
            Permissions = permissions;
        }

        public bool IsInRole(string role)
        {
            throw new NotImplementedException();
        }

        public bool HasPermission(string permission)
        {
            return this.Permissions != null && this.Permissions.Contains(permission);
        }
    }

    public class CustomIdentity : GenericIdentity, IIdentity
    {
        private bool isAuthenticated;

        public CustomIdentity(string name) 
            : base(name)
        {
            IsAuthenticated = base.IsAuthenticated;
        }

        public CustomIdentity(string name, bool authenticated)
            : base(name)
        {
            IsAuthenticated = authenticated;
        }

        public bool IsAuthenticated
        {
            get { return this.isAuthenticated; }
            private set { this.isAuthenticated = value; }
        }
    }
}
