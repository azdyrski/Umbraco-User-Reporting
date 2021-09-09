using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using azdyrski.Umbraco.UserReports.Models;
using azdyrski.Umbraco.UserReports.Interfaces;
using NPoco;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Persistence;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Net.Http.Headers;
using System.Web.Http;
using OfficeOpenXml;
using System.IO;
using OfficeOpenXml.Style;

namespace azdyrski.Umbraco.UserReports.Controllers
{
    [UmbracoApplicationAuthorize(Constants.Applications.Settings)]
    public class PermissionsApiController : UmbracoAuthorizedJsonController
    {
        protected readonly IUmbracoUserService pluginUserService;
        protected readonly IUmbracoGroupService pluginGroupService;
        protected readonly IUserService coreUserService;

        public PermissionsApiController()
        {
            
        }

        public PermissionsApiController(IUmbracoUserService pluginUserService, IUserService coreUserService, IUmbracoGroupService pluginGroupService)
        {
            this.pluginUserService = pluginUserService;
            this.pluginGroupService = pluginGroupService;
            this.coreUserService = coreUserService;
        }

        public List<UmbracoUser> GetUsers(string searchTerm = "", string orderBy = "Name", bool ascending = true)
        {
            var filter = new UserFilter
            {
                SearchTerm = searchTerm, Groups = new FilterGroup[0], UserStates = new FilterUserState[0]
            };
            return pluginUserService.GetUsersWithPermissions(filter, orderBy, ascending);
        }

        public List<UmbracoGroup> GetGroups(string searchTerm = "", string orderBy = "Name", bool ascending = true)
        {
            var filter = new GroupFilter
            {
                SearchTerm = searchTerm
            };
            return pluginGroupService.GetGroupsWithPermissions(filter, orderBy, ascending);
        }
    }
}
