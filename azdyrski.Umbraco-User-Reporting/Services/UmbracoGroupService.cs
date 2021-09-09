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
    public class UmbracoGroupService : IUmbracoGroupService
    {
        private readonly IScopeProvider scopeProvider;
        private readonly IUserService userService;
        private readonly IContentService contentService;
        private readonly IMediaService mediaService;
        private readonly IExternalLoginService externalLoginService;

        public UmbracoGroupService(IScopeProvider scopeProvider, IUserService userService, IContentService contentService, IMediaService mediaService, IExternalLoginService externalLoginService) //add in content (maybe media) service to get the actual nodes
        {
            this.scopeProvider = scopeProvider;
            this.userService = userService;
            this.contentService = contentService;
            this.mediaService = mediaService;
            this.externalLoginService = externalLoginService;

        }

        public IEnumerable<FilterGroup> GetUserGroups()
        {
            var groupResult = userService.GetAllUserGroups();
            List<FilterGroup> result = new List<FilterGroup>();
            foreach (var group in groupResult)
            {
                result.Add(new FilterGroup() { Alias = group.Alias, Name = group.Name, Selected = false, Icon = group.Icon });
            }
            return result;
        }

        public List<UmbracoGroup> GetGroupsWithPermissions(GroupFilter filter, string orderBy = "Name", bool ascending = true)
        {
            var groupResult = userService.GetAllUserGroups();
            List<UmbracoGroup> result = new List<UmbracoGroup>();
            Dictionary<int, IMedia> startMediaMap = new Dictionary<int, IMedia>();
            Dictionary<int, IContent> startContentMap = new Dictionary<int, IContent>();
            foreach (var group in groupResult)
            {
                IContent startContent = null;
                IMedia startMedia = null;
                int mediaCount = 0;
                int contentCount = 0;
                var groupExplicitPermissionsObject = userService.GetPermissions(group, false, new int[0]); //contains the additional explicit content permissions (does not apply to media)
                if (group.StartContentId.HasValue)
                {
                    if (group.StartContentId != -1)
                    {
                        if (startContentMap.ContainsKey(group.StartContentId.Value))
                            startContent = startContentMap[group.StartContentId.Value];
                        else
                        {
                            startContent = contentService.GetById(group.StartContentId.Value);
                            startContentMap.Add(group.StartContentId.Value, startContent);
                        }
                        contentCount = contentService.CountDescendants(startContent.Id) + 1;
                    }
                    else
                        contentCount = contentService.Count();
                }
                if (group.StartMediaId.HasValue)
                {
                    if (group.StartMediaId != -1)
                    {
                        if (startMediaMap.ContainsKey(group.StartMediaId.Value))
                            startMedia = startMediaMap[group.StartMediaId.Value];
                        else
                        {
                            startMedia = mediaService.GetById(group.StartMediaId.Value);
                            startMediaMap.Add(group.StartMediaId.Value, startMedia);
                        }
                        mediaCount = mediaService.CountDescendants(startMedia.Id) + 1;
                    }
                    else
                        mediaCount = mediaService.Count();
                }
                result.Add(new UmbracoGroup() { Alias = group.Alias, Name = group.Name, Id = group.Id, Icon = group.Icon, CoreGroup = group, Permissions = Permissions.GetFromCoreGroup(group, startContent, contentCount, groupExplicitPermissionsObject, startMedia, mediaCount, group.StartContentId.HasValue && group.StartContentId.Value == -1, group.StartMediaId.HasValue && group.StartMediaId.Value == -1) });
            }
            if(filter != null)
            {
                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                    result = result.Where(g => g.Name.ToUpper().Contains(filter.SearchTerm.ToUpper())).ToList();
            }
            return result;
        }
    }
}
