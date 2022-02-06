using System.Net;
using System.Net.Http.Formatting;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees;
using Umbraco.Web.WebApi.Filters;

namespace azdyrski.Umbraco.UserReports.Controllers
{
    /// <summary>
    /// Custom Umbraco tree for User Reports under the Developer section of Umbraco
    /// </summary>
    [Tree(Constants.Applications.Settings, treeAlias: UserReportsSettings.TreeAlias, TreeTitle = "User Reporting", TreeGroup = Constants.Trees.Groups.ThirdParty, SortOrder = 12)]
    [UmbracoApplicationAuthorize(Constants.Applications.Settings)]
    [PluginController(UserReportsSettings.PluginAreaName)]
    public class UserReportsTreeController : TreeController
    {
        private static readonly string baseUrl = $"{Constants.Applications.Settings}/{UserReportsSettings.TreeAlias}/";

        public const string ReflectionTree = "reflectionTree";

        /// <summary>
        /// The method called to render the contents of the tree structure
        /// </summary>
        /// <param name="id">The parent Id</param>
        /// <param name="queryStrings">All of the query string parameters passed from jsTree</param>
        /// <returns>The tree nodes</returns>
        /// <exception cref="HttpResponseException">HTTP Not Found</exception>
        /// <remarks>
        /// We are allowing an arbitrary number of query strings to be pased in so that developers are able to persist custom data from the front-end
        /// to the back end to be used in the query for model data.
        /// </remarks>
        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            if (id != Constants.System.RootString && id != ReflectionTree)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var tree = new TreeNodeCollection();

            if (id == Constants.System.RootString)
            {
                tree.AddRange(PopulateTreeNodes(id, queryStrings));
            }

            return tree;
        }

        /// <summary>
        /// Enables the root node to have it's own view
        /// </summary>
        protected override TreeNode CreateRootNode(FormDataCollection queryStrings)
        {
            var root = base.CreateRootNode(queryStrings);
            root.RoutePath = string.Format("{0}/{1}/{2}", Constants.Applications.Settings, UserReportsSettings.TreeAlias, "intro");
            root.Icon = "icon-users";
            root.HasChildren = true;
            root.MenuUrl = null;

            return root;
        }

        /// <summary>
        /// Returns the menu structure for the node
        /// </summary>
        /// <param name="id">The id</param>
        /// <param name="queryStrings">Any querystring params</param>
        /// <returns>The menu item collection</returns>
        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            if (id == Constants.System.Root.ToInvariantString())
            {
                menu.Items.Add(new RefreshNode(Services.TextService, true)); // adds refresh link to right-click
            }

            return menu;
        }

        private TreeNodeCollection PopulateTreeNodes(string parentId, FormDataCollection qs)
        {
            // path is PluginController name + area name + template name eg. /App_Plugins/DiploGodMode/GodModeTree/
            // The first part of the name eg. docTypeBrowser is the Id - this is used by Angular navigationService to identify the node

            var tree = new TreeNodeCollection
            {
                CreateTreeNode("userBrowser", parentId, qs, "User Browser", "icon-umb-members", false, baseUrl + "userBrowser"),
                CreateTreeNode("permissions", parentId, qs, "Permissions", "icon-autofill", false, baseUrl + "permissions"),
                CreateTreeNode("userAudits", parentId, qs, "User Audits", "icon-chart-curve", false, baseUrl + "userAudits")
            };

            return tree;
        }
    }
}