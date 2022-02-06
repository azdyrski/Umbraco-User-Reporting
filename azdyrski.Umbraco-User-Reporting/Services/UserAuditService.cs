using System;
using System.Collections.Generic;
using System.Linq;
using azdyrski.Umbraco.UserReports.Models;
using azdyrski.Umbraco.UserReports.Interfaces;
using NPoco;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Core.Models.Membership;

namespace azdyrski.Umbraco.UserReports.Services
{
    public class UserAuditService : IUserAuditService
    {
        private readonly IScopeProvider scopeProvider;
        private readonly IUserService userService;
        private readonly IAuditService auditService;

        //filters to
        //multi select auditType
        //single select on user

        public UserAuditService(IScopeProvider scopeProvider, IUserService userService, IAuditService auditService) //add in content (maybe media) service to get the actual nodes
        {
            this.scopeProvider = scopeProvider;
            this.userService = userService;
            this.auditService = auditService;
        }

        public Page<UmbracoAudit> GetAudits(long page, long itemsPerPage, AuditFilter filter, string orderBy = "EventDate", bool ascending = false)
        {
            var result = new Page<UmbracoAudit>() { Items = new List<UmbracoAudit>(), CurrentPage = page, ItemsPerPage = itemsPerPage };
            long totalRecords = 0;
            var auditResult = auditService.GetPagedItemsByUser(filter.SelectedUserId, page - 1, (int)itemsPerPage, out totalRecords, ascending ? Direction.Ascending : Direction.Descending, filter.UmbracoAuditTypes);
            foreach (var audit in auditResult)
            {
                var auditModel = GetAuditModel(audit);
                result.Items.Add(auditModel);
            }
            result.TotalItems = totalRecords;
            result.TotalPages = totalRecords / itemsPerPage;
            return result;
        }

        #region Private Helper Methods
        private string GetAuditTypeString(AuditType auditType)
        {
            switch (auditType)
            {
                case AuditType.New:
                    return "New";
                case AuditType.Save:
                    return "Save";
                case AuditType.SaveVariant:
                    return "Save Variant";
                case AuditType.Open:
                    return "Open";
                case AuditType.Delete:
                    return "Delete";
                case AuditType.Publish:
                    return "Publish";
                case AuditType.PublishVariant:
                    return "Publish Variant";
                case AuditType.SendToPublish:
                    return "Send To Publish";
                case AuditType.SendToPublishVariant:
                    return "Send To Publish Variant";
                case AuditType.Unpublish:
                    return "Unpublish";
                case AuditType.UnpublishVariant:
                    return "Unpublish Variant";
                case AuditType.Move:
                    return "Move";
                case AuditType.Copy:
                    return "Copy";
                case AuditType.AssignDomain:
                    return "Assign Domain";
                case AuditType.PublicAccess:
                    return "Public Access";
                case AuditType.Sort:
                    return "Sort";
                case AuditType.Notify:
                    return "Notify";
                case AuditType.System:
                    return "Umbraco System";
                case AuditType.RollBack:
                    return "Rollback";
                case AuditType.PackagerInstall:
                    return "Package Install";
                case AuditType.PackagerUninstall:
                    return "Package Uninstall";
                case AuditType.Custom:
                    return "Custom";
                default:
                    return "Unknown";
            }
        }

        private UmbracoAudit GetAuditModel(IAuditItem auditItem)
        {
            var model = new UmbracoAudit() { CoreEntry = auditItem, Comment = auditItem.Comment, Parameters = auditItem.Parameters, UserId = auditItem.UserId, EntityType = auditItem.EntityType, EntityId = auditItem.Id.ToString(), AuditDate = auditItem.CreateDate.ToString("MMM d yyyy HH:mm") };
            model.UserEmail = userService.GetUserById(auditItem.UserId).Email;
            model.AuditType = GetAuditTypeString(auditItem.AuditType);
            return model;
        }
        #endregion
    }
}
