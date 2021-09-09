using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco;

namespace azdyrski.Umbraco.UserReports.Models
{
    public class UserFilter
    {
        public string SearchTerm { get; set; }
        public FilterUserState[] UserStates { get; set; }
        public FilterGroup[] Groups { get; set; }
    }

    public class GroupFilter
    {
        public string SearchTerm { get; set; }
    }

    public class FilterUserState
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public bool Selected { get; set; }

        public static FilterUserState[] GetFiltersFromString(string filterString)
        {
            List<FilterUserState> result = new List<FilterUserState>();
            if (!string.IsNullOrWhiteSpace(filterString))
            {
                foreach (var item in filterString.Split(',').Select(s => s.Trim()))
                {
                    result.Add(new FilterUserState() { Value = item });
                }
            }
            return result.ToArray();
        }
    }

    public class FilterGroup
    {
        public string Name { get; set; }
        public string Alias { get; set; }
        public bool Selected { get; set; }
        public string Icon { get; set; }

        public static FilterGroup[] GetFiltersFromString(string filterString)
        {
            List<FilterGroup> result = new List<FilterGroup>();
            if(!string.IsNullOrWhiteSpace(filterString))
            { 
                foreach (var item in filterString.Split(',').Select(s => s.Trim()))
                {
                    result.Add(new FilterGroup() { Alias = item });
                }
            }
            return result.ToArray();
        }
    }

    public class UserDisplayColumn
    {
        public string Name { get; set; }

        public static UserDisplayColumn[] GetColumnsFromString(string filterString)
        {
            List<UserDisplayColumn> result = new List<UserDisplayColumn>();
            if (!string.IsNullOrWhiteSpace(filterString))
            {
                foreach (var item in filterString.Split(',').Select(s => s.Trim()))
                {
                    result.Add(new UserDisplayColumn() { Name = item });
                }
            }
            return result.ToArray();
        }
    }
}
