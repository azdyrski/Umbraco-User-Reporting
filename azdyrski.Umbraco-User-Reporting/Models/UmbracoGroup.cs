using System;
using System.Runtime.Serialization;
using Umbraco.Core.Models.Membership;

namespace azdyrski.Umbraco.UserReports.Models
{
    [DataContract]
    public class UmbracoGroup
    {
        //general group props
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Alias { get; set; }
        [DataMember]
        public string Icon { get; set; }
        [DataMember]
        public int UserCount { 
            get {
                return CoreGroup.UserCount;
            }
        }
        [DataMember]
        public bool IsOpen { get; set; }

        //permissions specific props
        [DataMember]
        public Permissions Permissions { get; set; }

        //non interface props
        public IUserGroup CoreGroup { get; set; }
    }
}
