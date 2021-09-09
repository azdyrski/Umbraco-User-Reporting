using System;
using System.Collections.Generic;
using NPoco;
using azdyrski.Umbraco.UserReports.Models;


namespace azdyrski.Umbraco.UserReports.Interfaces
{
    public interface IUmbracoUserService
    {
        Page<UmbracoUser> GetUsers(long page, long itemsPerPage, UserFilter filter, string orderBy = "Name", bool ascending = true);
        List<UmbracoUser> GetUsersWithPermissions(UserFilter filter, string orderBy = "Name", bool ascending = true);
    }
}
