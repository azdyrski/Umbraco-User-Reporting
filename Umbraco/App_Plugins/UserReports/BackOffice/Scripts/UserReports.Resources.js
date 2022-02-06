(function () {
    'use strict';
    angular.module('umbraco.resources').factory('userReportsResources', function ($q, $http, umbRequestHelper, userReportsConfig) {
        return {
            //user browser
            getUsersPaged: function (page, pageSize, filter, orderBy, ascending) {
                return umbRequestHelper.resourcePromise(
                    $http.get(userReportsConfig.baseApiUrl + "GetUsers",
                        {
                            params: { page: page, pageSize: pageSize, searchTerm: filter.searchTerm, userStates: filter.selectedUserStates.map(function (item) { return item.value; }).join(","), groups: filter.selectedGroups.map(function (item) { return item.Alias; }).join(","), orderBy: orderBy, ascending: ascending }
                        })
                );
            },
            getUserGroups: function() {
                return umbRequestHelper.resourcePromise(
                    $http.get(userReportsConfig.baseApiUrl + "GetUserGroups",
                        {
                            params: { }
                        })
                );
            },
            exportUsersCsv: function(columns, filter, orderBy, ascending) {
                return umbRequestHelper.resourcePromise(
                    $http.get(userReportsConfig.baseApiUrl + "GetExportCsv",
                        {
                            params: { columns: columns.map(function (item) { return item.colName; }).join(","), searchTerm: filter.searchTerm, userStates: filter.selectedUserStates.map(function (item) { return item.value; }).join(","), groups: filter.selectedGroups.map(function (item) { return item.Alias; }).join(","), orderBy: orderBy, ascending: ascending }
                        })
                );
            },
            exportUsersExcel: function(columns, filter, orderBy, ascending) {
                return umbRequestHelper.resourcePromise(
                    $http.get(userReportsConfig.baseApiUrl + "GetExportExcel",
                        {
                            params: { columns: columns.map(function (item) { return item.colName; }).join(","), searchTerm: filter.searchTerm, userStates: filter.selectedUserStates.map(function (item) { return item.value; }).join(","), groups: filter.selectedGroups.map(function (item) { return item.Alias; }).join(","), orderBy: orderBy, ascending: ascending }
                        })
                );
            },
            downloadFile: function (data, type, name, extension) {
                var file = new Blob([data], {
                    type: 'application/' + type
                });
                var fileURL = URL.createObjectURL(file);
                var a = document.createElement('a');
                a.href = fileURL;
                a.target = '_blank';
                a.download = name + '-Export.' + extension;
                document.body.appendChild(a); //create the link "a"
                a.click(); //click the link "a"
                document.body.removeChild(a); //remove the link "a"
            },
            downloadExcel: function (data, type, name, extension) {
                var a = document.createElement('a');
                a.href = 'data:application/octet-stream;charset=utf-8;base64,' + data;
                a.target = '_blank';
                a.download = name + '-Export.' + extension;
                document.body.appendChild(a); //create the link "a"
                a.click(); //click the link "a"
                document.body.removeChild(a); //remove the link "a"
            },
            getInitialUserColumns: function () { 
                return [
                    { valueName: 'Name', colName: 'Name', sortable: true, arrayValue: false, selected: true },
                    { valueName: 'Culture', colName: 'User Language', sortable: true, arrayValue: false, selected: false },
                    { valueName: 'Email', colName: 'Email' , sortable: true, arrayValue: false, selected: true },
                    { valueName: 'UserName', colName: 'User Name' , sortable: true, arrayValue: false, selected: false },
                    { valueName: 'CreateDate', colName: 'Created Date', sortable: true, arrayValue: false, selected: true },
                    { valueName: 'UpdateDate', colName: 'Updated Date', sortable: true, arrayValue: false, selected: false },
                    { valueName: 'IsApproved', colName: 'Approved', sortable: true, arrayValue: false, selected: false },
                    //{ valueName: 'IsDisabled', colName: 'Disabled', sortable: false, arrayValue: false, selected: false}, why is this not exposed in IUser????
                    { valueName: 'LockoutEnabled', colName: 'Locked', sortable: false, arrayValue: false, selected: true },
                    { valueName: 'LastLoginDate', colName: 'Last Login Date', sortable: true, arrayValue: false, selected: true },
                    { valueName: 'AccessFailedCount', colName: 'Failed Login Count', sortable: false, arrayValue: false, selected: true },
                    { valueName: 'ExternalLoginEnabled', colName: 'External Login Enabled', sortable: false, arrayValue: false, selected: true },
                    { valueName: 'HasStartContentNode', colName: 'Has Content Start Node', sortable: false, arrayValue: false, selected: false },
                    { valueName: 'HasStartMediaNode', colName: 'Has Media Start Node', sortable: false, arrayValue: false, selected: false },
                    { valueName: 'AllowedSections', colName: 'Allowed Sections', sortable: false, arrayValue: true, selected: false },
                    { valueName: 'Groups', colName: 'Groups', sortable: false, arrayValue: true, selected: false },
                    { valueName: 'Roles', colName: 'Roles', sortable: false, arrayValue: true, selected: false }
                ];
            },
            getInitialUserStates: function () {
                return [
                    { name: 'Active', value: 'active', selected: true, colorLabel: 'success' },
                    { name: 'Invited', value: 'locked', selected: true, colorLabel: 'success' },
                    { name: 'Inactive', value: 'inactive', selected: true, colorLabel: 'warning' },
                    { name: 'Locked', value: 'locked', selected: true, colorLabel: 'warning' },
                    { name: 'Disabled', value: 'disabled', selected: true, colorLabel: 'danger' }
                ]
            },
            //end user browser
            //permissions
            getUsersWithPermissions: function (filter, orderBy, ascending) {
                return umbRequestHelper.resourcePromise(
                    $http.get(userReportsConfig.permissionsApiUrl + "GetUsers",
                        {
                            params: { searchTerm: filter.searchTerm, orderBy: orderBy, ascending: ascending }
                        })
                );
            },
            getGroupsWithPermissions: function (filter, orderBy, ascending) {
                return umbRequestHelper.resourcePromise(
                    $http.get(userReportsConfig.permissionsApiUrl + "GetGroups",
                        {
                            params: { searchTerm: filter.searchTerm, orderBy: orderBy, ascending: ascending }
                        })
                );
            },
            //end permissions
            //user activity audits
            getAudits: function (page, pageSize, filter, orderBy, ascending) {
                console.log("audit filter object", filter);
                return umbRequestHelper.resourcePromise(
                    $http.get(userReportsConfig.auditsApiUrl + "GetAudits",
                        {
                            params: { page: page, pageSize: pageSize, userId: filter.selectedUserId, auditTypes: filter.selectedAuditTypes != null ? filter.selectedAuditTypes.value : '', orderBy: orderBy, ascending: ascending }
                        })
                );
            },
            getAuditUsers: function () {
                return umbRequestHelper.resourcePromise(
                    $http.get(userReportsConfig.auditsApiUrl + "GetAuditUsers",
                        {
                            params: {}
                        })
                );
            },
            exportAuditCsv: function (columns, filter, orderBy, ascending) {
                return umbRequestHelper.resourcePromise(
                    $http.get(userReportsConfig.auditsApiUrl + "GetExportCsv",
                        {
                            params: { columns: columns.map(function (item) { return item.colName; }).join(","), userId: filter.selectedUserId, auditTypes: filter.selectedAuditTypes != null ? filter.selectedAuditTypes.value : '', orderBy: orderBy, ascending: ascending }
                        })
                );
            },
            exportAuditExcel: function (columns, filter, orderBy, ascending) {
                return umbRequestHelper.resourcePromise(
                    $http.get(userReportsConfig.auditsApiUrl + "GetExportExcel",
                        {
                            params: { columns: columns.map(function (item) { return item.colName; }).join(","), userId: filter.selectedUserId, auditTypes: filter.selectedAuditTypes != null ? filter.selectedAuditTypes.value : '', orderBy: orderBy, ascending: ascending }
                        })
                );
            },
            getAuditTypes: function () {
                return [
                    { name: 'New', value: '0', selected: true },
                    { name: 'Save', value: '1', selected: true },
                    { name: 'Save Variant', value: '2', selected: true },
                    { name: 'Open', value: '3', selected: true },
                    { name: 'Delete', value: '4', selected: true },
                    { name: 'Publish', value: '5', selected: true },
                    { name: 'Publish Variant', value: '6', selected: true },
                    { name: 'Send to Publish', value: '7', selected: true },
                    { name: 'Send to Publish Variant', value: '8', selected: true },
                    { name: 'Unpublish', value: '9', selected: true },
                    { name: 'Unpublish Variant', value: '10', selected: true },
                    { name: 'Move', value: '11', selected: true },
                    { name: 'Copy', value: '12', selected: true },
                    { name: 'Assign Domain', value: '13', selected: true },
                    { name: 'Public Access', value: '14', selected: true },
                    { name: 'Sort', value: '15', selected: true },
                    { name: 'Notify', value: '16', selected: true },
                    { name: 'Umbraco System', value: '17', selected: true },
                    { name: 'Rollback', value: '18', selected: true },
                    { name: 'Package Install', value: '19', selected: true },
                    { name: 'Package Uninstall', value: '20', selected: true },
                    { name: 'Custom', value: '21', selected: true }
                ];
            },
            getInitialAuditColumns: function () {
                return [
                    { valueName: 'UserId', colName: 'User ID', sortable: false, arrayValue: false, selected: true },
                    { valueName: 'UserEmail', colName: 'User Email', sortable: false, arrayValue: false, selected: true },
                    { valueName: 'Comment', colName: 'Comment', sortable: false, arrayValue: false, selected: true },
                    { valueName: 'Parameters', colName: 'Parameters', sortable: false, arrayValue: false, selected: false },
                    { valueName: 'AuditType', colName: 'Audit Type', sortable: false, arrayValue: false, selected: true },
                    { valueName: 'EntityType', colName: 'Entity Type', sortable: false, arrayValue: false, selected: true },
                    { valueName: 'EntityId', colName: 'Entity ID', sortable: false, arrayValue: false, selected: true },
                    { valueName: 'AuditDate', colName: 'Event Date', sortable: true, arrayValue: false, selected: true }
                ];
            }
            //end audits
        };
    });
})();