using System;
using System.Collections.Generic;
using NPoco;
using azdyrski.Umbraco.UserReports.Models;


namespace azdyrski.Umbraco.UserReports.Interfaces
{
    public interface IUmbracoGroupService
    {
        IEnumerable<FilterGroup> GetUserGroups();
        List<UmbracoGroup> GetGroupsWithPermissions(GroupFilter filter, string orderBy = "Name", bool ascending = true);
    }
}
