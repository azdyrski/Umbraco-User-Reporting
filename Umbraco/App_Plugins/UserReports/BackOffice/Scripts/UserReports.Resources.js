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
            downloadFile: function (data, type, extension) {
                var file = new Blob([data], {
                    type: 'application/' + type
                });
                var fileURL = URL.createObjectURL(file);
                var a = document.createElement('a');
                a.href = fileURL;
                a.target = '_blank';
                a.download = 'userReports-Export.' + extension;
                document.body.appendChild(a); //create the link "a"
                a.click(); //click the link "a"
                document.body.removeChild(a); //remove the link "a"
            },
            downloadExcel: function (data, type, extension) {
                var a = document.createElement('a');
                a.href = 'data:application/octet-stream;charset=utf-8;base64,' + data;
                a.target = '_blank';
                a.download = 'userReports-Export.' + extension;
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
            }
            //end permissions
        };
    });
})();