using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;

namespace azdyrski.Umbraco.UserReports.Models
{
    [DataContract]
    public class UmbracoUser
    {
        //general user props
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public string Avatar { get; set; }
        [DataMember]
        public string LastLoginDate { get; set; }
        [DataMember]
        public string CreateDate { get; set; }
        [DataMember]
        public string UpdateDate { get; set; }
        [DataMember]
        public string IsApproved { get; set; }
        [DataMember]
        public string ExternalLoginEnabled { get; set; }
        [DataMember]
        public string HasStartContentNode { get; set; }
        [DataMember]
        public string HasStartMediaNode { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int AccessFailedCount { get; set; }
        [DataMember]
        public string[] AllowedSections { get; set; }
        [DataMember]
        public string Culture { get; set; }
        [DataMember]
        public string[] Groups { get; set; }
        [DataMember]
        public string LockoutEnabled { get; set; }
        [DataMember]
        public string IsDisabled { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string[] Roles { get; set; }
        [DataMember]
        public bool IsOpen { get; set; }
        [DataMember]
        public List<UmbracoNode> ExplicitStartContent { get; set; }
        [DataMember]
        public List<UmbracoNode> ExplicitStartMedia { get; set; }

        //permissions specific props
        [DataMember]
        public Permissions Permissions { get; set; }

        //non interface props
        public IUser CoreUser { get; set; }
    }
}
