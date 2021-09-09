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
    public class UserReportsApiController : UmbracoAuthorizedJsonController
    {
        protected readonly IUmbracoUserService pluginUserService;
        protected readonly IUmbracoGroupService pluginGroupService;
        protected readonly IUserService coreUserService;

        public UserReportsApiController()
        {

        }

        public UserReportsApiController(IUmbracoUserService pluginUserService, IUserService coreUserService, IUmbracoGroupService pluginGroupService)
        {
            this.pluginUserService = pluginUserService;
            this.pluginGroupService = pluginGroupService;
            this.coreUserService = coreUserService;
        }
        public Page<UmbracoUser> GetUsers(long page = 1, int pageSize = 15, string searchTerm = "", string userStates = "", string groups = "", string orderBy = "Name", bool ascending = true)
        {
            var filter = new UserFilter
            {
                SearchTerm = searchTerm,
                UserStates = FilterUserState.GetFiltersFromString(userStates),
                Groups = FilterGroup.GetFiltersFromString(groups)
            };
            return pluginUserService.GetUsers(page, pageSize, filter, orderBy, ascending);
        }

        public IEnumerable<FilterGroup> GetUserGroups()
        {
            return pluginGroupService.GetUserGroups();
        }
        #region User Browser Exporting
        public HttpResponseMessage GetExportCsv(string columns = "", string searchTerm = "", string userStates = "", string groups = "", string orderBy = "Name", bool ascending = true)
        {
            string csvResult = "";
            long page = 1;
            int pageSize = int.MaxValue;
            var userResults = GetUsers(page, pageSize, searchTerm, userStates, groups, orderBy, ascending).Items;
            var activeColumns = UserDisplayColumn.GetColumnsFromString(columns);
            if(userResults.Count == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            csvResult += string.Join(",", activeColumns.Select(c => c.Name)) + Environment.NewLine;
            foreach (var userRecord in userResults)
            {
                foreach (var col in activeColumns)
                {
                    csvResult += GetUserFieldForColumn(userRecord, col) + ",";
                }
                csvResult = csvResult.TrimEnd(",");
                csvResult += Environment.NewLine;
            }
            csvResult = csvResult.TrimEnd(Environment.NewLine);

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            var byteArray = Encoding.ASCII.GetBytes(csvResult);
            response.Content = new ByteArrayContent(byteArray);
            response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
            response.Content.Headers.ContentDisposition.FileName = "userReports-Export-" + DateTime.Now.ToString("dd-MM-YYYY") + ".csv";
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/csv");
            response.Content.Headers.ContentLength = byteArray.Length;
                
            return response;
        }

        public HttpResponseMessage GetExportExcel(string columns = "", string searchTerm = "", string userStates = "", string groups = "", string orderBy = "Name", bool ascending = true)
        {
            int colIndex = 1;
            int rowIndex = 2;
            long page = 1;
            int pageSize = int.MaxValue;
            var userResults = GetUsers(page, pageSize, searchTerm, userStates, groups, orderBy, ascending).Items;
            var activeColumns = UserDisplayColumn.GetColumnsFromString(columns);
            if (userResults.Count == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("User List Export");

                worksheet.Row(1).Height = 20;
                worksheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Row(1).Style.Font.Bold = true;

                //csvResult += string.Join(",", activeColumns.Select(c => c.Name)) + Environment.NewLine; //columns 
                var colList = activeColumns.Select(c => c.Name);
                foreach (var col in colList)
                {
                    worksheet.Cells[1, colIndex].Value = col;
                    colIndex++;
                }
                foreach (var userRecord in userResults) //rows
                {
                    colIndex = 1;
                    foreach (var col in activeColumns)
                    {
                        worksheet.Cells[rowIndex, colIndex].Value = GetUserFieldForColumn(userRecord, col);
                        colIndex++;
                    }
                    rowIndex++;
                }
                colIndex = 1;
                foreach (var col in colList)
                {
                    worksheet.Column(colIndex).AutoFit();
                    colIndex++;
                } 
                //var byteArray = package.GetAsByteArray();
                var dataBytes = package.GetAsByteArray();
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                string stringForm = System.Convert.ToBase64String(dataBytes, 0, dataBytes.Length);
                response.Content = new StringContent(stringForm);
                //response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
                //response.Content.Headers.ContentDisposition.FileName = "userReports-Export-" + DateTime.Now.ToString("dd-MM-YYYY") + ".xslx";
                //response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/excel");

                return response;
            }
        }

        private string GetUserFieldForColumn(UmbracoUser user, UserDisplayColumn column) //prolly should have though some mapping through better here, don't be mean
        {
            switch (column.Name)
            {
                case "Name":
                    return user.Name;
                case "User Language":
                    return user.Culture;
                case "Email":
                    return user.Email;
                case "User Name":
                    return user.UserName;
                case "Created Date":
                    return user.CreateDate;
                case "Updated Date":
                    return user.UpdateDate;
                case "Approved":
                    return user.IsApproved;
                case "Locked":
                    return user.LockoutEnabled;
                case "Last Login Date":
                    return user.LastLoginDate;
                case "Failed Login Count":
                    return user.AccessFailedCount.ToString();
                case "External Login Enabled":
                    return user.ExternalLoginEnabled;
                case "Has Content Start Node":
                    return user.HasStartContentNode;
                case "Has Media Start Node":
                    return user.HasStartMediaNode;
                case "Allowed Sections":
                    return user.AllowedSections != null && user.AllowedSections.Length > 0 ? string.Join("|", user.AllowedSections) : "None";
                case "Groups":
                    return user.Groups != null && user.Groups.Length > 0 ? string.Join("|", user.Groups) : "None";
                case "Roles":
                    return user.Roles != null && user.Roles.Length > 0 ? string.Join("|", user.Roles) : "None";
                default:
                    return "placeholder";
            }
        }
#endregion
    }
}
