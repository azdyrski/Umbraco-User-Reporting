using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;

namespace azdyrski.Umbraco.UserReports.Models
{
    [DataContract]
    public class UmbracoAudit
    {
        [DataMember]
        public int UserId { get; set; }
        [DataMember]
        public string UserEmail { get; set; }
        [DataMember]
        public string Comment { get; set; }
        [DataMember]
        public string Parameters { get; set; }
        [DataMember]
        public string AuditType { get; set; }
        [DataMember]
        public string EntityType { get; set; }
        [DataMember]
        public string EntityId { get; set; }
        [DataMember]
        public string AuditDate { get; set; } 


        //non interface props
        public IAuditItem CoreEntry { get; set; }
    }
}
