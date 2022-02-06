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
using Umbraco.Web;

namespace azdyrski.Umbraco.UserReports.Services
{
    public class UmbracoUserService : IUmbracoUserService
    {
        private readonly IScopeProvider scopeProvider;
        private readonly IUserService userService;
        private readonly IExternalLoginService externalLoginService;
        private readonly IUmbracoGroupService pluginGroupService;
        private readonly UmbracoHelper umbHelper;

        public UmbracoUserService(IScopeProvider scopeProvider, IUserService userService, IExternalLoginService externalLoginService, UmbracoHelper umbracoHelper, IUmbracoGroupService pluginGroupService)
        {
            this.scopeProvider = scopeProvider;
            this.userService = userService;
            this.externalLoginService = externalLoginService;
            this.umbHelper = umbracoHelper;
            this.pluginGroupService = pluginGroupService;
        }

        public Page<UmbracoUser> GetUsers(long page, long itemsPerPage, UserFilter filter, string orderBy = "Name", bool ascending = true)
        {
            var result = new Page<UmbracoUser>() { Items = new List<UmbracoUser>(), CurrentPage = page, ItemsPerPage = itemsPerPage };
            long totalRecords = 0;
            //List<IQuery<IUser>> activeQueries = new List<IQuery<IUser>>();
            //var finalQuery = new Query<IUser>(scopeProvider.SqlContext);
            IQuery<IUser> filterQuery = new Query<IUser>(scopeProvider.SqlContext);
            if (filter != null)
            {
                if(!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    //IQuery<IUser> filterQuery = new Query<IUser>(scopeProvider.SqlContext);
                    filterQuery = filterQuery.Where(x => x.Name.Contains(filter.SearchTerm) || x.Username.Contains(filter.SearchTerm) || x.Email.Contains(filter.SearchTerm));
                    //activeQueries.Add(filterQuery);
                }
                //if(filter.UserStates != null && filter.UserStates.Count() > 0) possible how to append additional "wheres" to the IQuery ?
                //{
                //    //IQuery<IUser> filterQuery = new Query<IUser>(scopeProvider.SqlContext);
                //    filterQuery = filterQuery.Where(x => UserHasStateFiltered(x.UserState, filter.UserStates));
                //    //activeQueries.Add(filterQuery);
                //}
            }
            //var userResult = userService.GetAll(page, (int)itemsPerPage, out totalRecords, orderBy, ascending ? Direction.Ascending : Direction.Descending, null, null, string.IsNullOrWhiteSpace(filter.SearchTerm) ? null : filter.SearchTerm);
            var userResult = userService.GetAll(page - 1, (int)itemsPerPage, out totalRecords, orderBy, ascending ? Direction.Ascending : Direction.Descending, filter.UserStates.Length == 0 ? null : GetUserStates(filter.UserStates), filter.Groups.Length == 0 ? null : GetGroupsFilterValue(filter.Groups), null, filterQuery);
            foreach (var user in userResult)
            {
                var userModel = GetUserModel(user);
                result.Items.Add(userModel);
            }
            result.TotalItems = totalRecords;
            result.TotalPages = totalRecords / itemsPerPage;
            return result;
        }

        public List<UmbracoUser> GetUsersWithPermissions(UserFilter filter, string orderBy = "Name", bool ascending = true)
        {
            var users = GetUsers(1, int.MaxValue, filter, "Name", true);
            var groupsWithPerms = pluginGroupService.GetGroupsWithPermissions(new GroupFilter() { SearchTerm = string.Empty });
            foreach (var user in users.Items)
            {
                var userGroupsPerms = groupsWithPerms.Where(g => user.Groups.ToList().Contains(g.Name)).Select(g => g.Permissions).ToList();
                user.Permissions = Permissions.GetUserCombinedPermissions(userGroupsPerms, user.ExplicitStartContent, user.ExplicitStartMedia);
            }
            return users.Items;
        }

        public List<UmbracoUser> GetAuditUsers()
        {
            var filter = new UserFilter
            {
                SearchTerm = string.Empty,
                Groups = new FilterGroup[0],
                UserStates = new FilterUserState[0]
            };
            var users = GetUsers(1, int.MaxValue, filter, "Name", true);
            return users.Items;
        }
        #region Private Helper Methods
        private UserState[] GetUserStates(FilterUserState[] filterUserStates)
        {
            var result = new List<UserState>();
            foreach (var filterUserState in filterUserStates)
            {
                result.Add(GetUserState(filterUserState));
            }
            return result.ToArray();
        }

        private UserState GetUserState(FilterUserState state)
        {
            switch (state.Value)
            {
                case "active":
                    return UserState.Active;
                case "locked":
                    return UserState.LockedOut;
                case "disabled":
                    return UserState.Disabled;
                case "inactive":
                    return UserState.Inactive;
                case "invited":
                    return UserState.Invited;
                default:
                    return UserState.All;
            }
        }

        private string[] GetGroupsFilterValue(FilterGroup[] filterGroups)
        {
            var result = new List<string>();
            foreach (var filterGroup in filterGroups)
            {
                result.Add(filterGroup.Alias);
            }
            return result.ToArray();
        }

        private UmbracoUser GetUserModel(IUser user)
        {
            var result = new UmbracoUser() { CoreUser = user, AccessFailedCount = user.FailedPasswordAttempts, AllowedSections = user.AllowedSections.Select(s => s.ToFirstUpper()).ToArray(), Email = string.IsNullOrEmpty(user.Email) ? "Not Set" : user.Email, Groups = user.Groups.Select(g => g.Name).ToArray(), Id = user.Id, Name = user.Name, LastLoginDate = user.LastLoginDate != DateTime.MinValue ? user.LastLoginDate.ToString("MMM d yyyy HH:mm") : "N/A", LockoutEnabled = user.IsLockedOut ? "Locked" : "Unlocked", UserName = user.Username, CreateDate = user.CreateDate.ToString("MMM d yyyy HH:mm"), UpdateDate = user.UpdateDate.ToString("MMM d yyyy HH:mm"), IsApproved = user.IsApproved ? "Yes" : "No", Culture = user.Language, Avatar = user.Avatar };
            result.HasStartContentNode = user.StartContentIds.Length > 0 ? "Yes" : "No";
            if (user.StartContentIds.Length > 0)
            {
                result.HasStartContentNode = "Yes";
                if (user.StartContentIds.Contains(-1))
                    result.ExplicitStartContent = new List<UmbracoNode>() { new UmbracoNode() { Icon = "icon-home", Id = -1, Name = "Content Root" } };
                else
                    result.ExplicitStartContent = user.StartContentIds.ToList().Select(id => UmbracoNode.GetFromPublishedContent(umbHelper.Content(id))).ToList();
            }
            else
            {
                if (user.Groups.Any(g => g.StartContentId.HasValue))
                    result.HasStartContentNode = "Yes (inheretied from Group)";
                else
                    result.HasStartContentNode = "No";
            }
            if (user.StartMediaIds.Length > 0)
            {
                result.HasStartMediaNode = "Yes";
                if (user.StartMediaIds.Contains(-1))
                    result.ExplicitStartMedia = new List<UmbracoNode>() { new UmbracoNode() { Icon = "icon-home", Id = -1, Name = "Media Root" } };
                else
                    result.ExplicitStartMedia = user.StartMediaIds.ToList().Select(id => UmbracoNode.GetFromPublishedContent(umbHelper.Media(id))).ToList();
            }
            else
            {
                if (user.Groups.Any(g => g.StartMediaId.HasValue))
                    result.HasStartMediaNode = "Yes (inherited from Group)";
                else
                    result.HasStartMediaNode = "No";
            }
            result.HasStartMediaNode = user.StartMediaIds.Length > 0 ? "Yes" : "No";
            var externalLogins = externalLoginService.GetAll(user.Id);
            result.ExternalLoginEnabled = externalLogins != null && externalLogins.Count() > 0 ? "Yes" : "No";
            if (user.LastLoginDate != DateTime.MinValue && user.LastLoginDate < DateTime.Now.AddDays(90) && user.UpdateDate < DateTime.Now.AddDays(90))
                result.IsDisabled = "Yes";
            else
                result.IsDisabled = "No";
            return result;
        }
        #endregion
    }
}
