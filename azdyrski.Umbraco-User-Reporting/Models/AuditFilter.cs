using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco;
using Umbraco.Core.Models;

namespace azdyrski.Umbraco.UserReports.Models
{
    public class AuditFilter
    {
        public FilterAuditType[] AuditTypes { get; set; }
        public int SelectedUserId { get; set; }
        public AuditType[] UmbracoAuditTypes { 
            get
            {
                return AuditTypes.ToList().Select(x => (AuditType)int.Parse(x.Value)).ToArray();
            } 
        }
    }

    public class FilterAuditType
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public bool Selected { get; set; }

        public static FilterAuditType[] GetFiltersFromString(string filterString)
        {
            List<FilterAuditType> result = new List<FilterAuditType>();
            if (!string.IsNullOrWhiteSpace(filterString))
            {
                foreach (var item in filterString.Split(',').Select(s => s.Trim()))
                {
                    result.Add(new FilterAuditType() { Value = item });
                }
            }
            return result.ToArray();
        }
    }

    //public class FilterAuditUser
    //{
    //    public string Name { get; set; }
    //    public int Id { get; set; }
    //    public bool Selected { get; set; }
    //    public string Icon { get; set; }
    //}S
}
