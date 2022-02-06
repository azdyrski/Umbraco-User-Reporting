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
    public class UserAuditsApiController : UmbracoAuthorizedJsonController
    {
        protected readonly IUserAuditService pluginAuditService;
        protected readonly IUmbracoUserService pluginUserService;

        public UserAuditsApiController()
        {

        }

        public UserAuditsApiController(IUserAuditService pluginAuditService, IUmbracoUserService pluginUserService)
        {
            this.pluginAuditService = pluginAuditService;
            this.pluginUserService = pluginUserService;
        }
        public Page<UmbracoAudit> GetAudits(long page = 1, int pageSize = 15, string userId = "", string auditTypes = "", string orderBy = "EventDate", bool ascending = false)
        {
            if (!string.IsNullOrEmpty(userId) && userId == "0")
                userId = "-1";
            var filter = new AuditFilter
            {
                SelectedUserId = int.Parse(userId),
                AuditTypes = FilterAuditType.GetFiltersFromString(auditTypes)
            };
            return pluginAuditService.GetAudits(page, pageSize, filter, orderBy, ascending);
        }

        public List<UmbracoUser> GetAuditUsers()
        {
            return pluginUserService.GetAuditUsers();
        }
        #region User Browser Exporting
        public HttpResponseMessage GetExportCsv(string columns = "", string userId = "", string auditTypes = "", string orderBy = "EventDate", bool ascending = false)
        {
            string csvResult = "";
            long page = 1;
            int pageSize = int.MaxValue;
            var userResults = GetAudits(page, pageSize, userId, auditTypes, orderBy, ascending).Items;
            var activeColumns = UserDisplayColumn.GetColumnsFromString(columns);
            if (userResults.Count == 0)
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
            response.Content.Headers.ContentDisposition.FileName = "auditReports-Export-" + DateTime.Now.ToString("dd-MM-YYYY") + ".csv";
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/csv");
            response.Content.Headers.ContentLength = byteArray.Length;

            return response;
        }

        public HttpResponseMessage GetExportExcel(string columns = "", string userId = "", string auditTypes = "", string orderBy = "EventDate", bool ascending = false)
        {
            int colIndex = 1;
            int rowIndex = 2;
            long page = 1;
            int pageSize = int.MaxValue;
            var userResults = GetAudits(page, pageSize, userId, auditTypes, orderBy, ascending).Items;
            var activeColumns = UserDisplayColumn.GetColumnsFromString(columns);
            if (userResults.Count == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("User Audit Export");

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

        private string GetUserFieldForColumn(UmbracoAudit audit, UserDisplayColumn column) //prolly should have though some mapping through better here, don't be mean
        {
            switch (column.Name)
            {
                case "User ID":
                    return audit.UserId.ToString();
                case "User Email":
                    return audit.UserEmail;
                case "Comment":
                    return audit.Comment;
                case "Parameters":
                    return audit.Parameters;
                case "Audit Type":
                    return audit.AuditType;
                case "Entity Type":
                    return audit.EntityType;
                case "Event Date":
                    return audit.AuditDate;
                default:
                    return "placeholder";
            }
        }
        #endregion
    }
}
