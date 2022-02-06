using System;
using System.Collections.Generic;
using NPoco;
using azdyrski.Umbraco.UserReports.Models;


namespace azdyrski.Umbraco.UserReports.Interfaces
{
    public interface IUserAuditService
    {
        Page<UmbracoAudit> GetAudits(long page, long itemsPerPage, AuditFilter filter, string orderBy = "EventDate", bool ascending = false);
    }
}
