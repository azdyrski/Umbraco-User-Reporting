(function () {
    'use strict';
    angular.module("umbraco").controller("UserReports.UserBrowser.Controller",
        function ($routeParams, navigationService, userReportsResources, userReportsConfig, editorService) {

            var vm = this;

            navigationService.syncTree({ tree: $routeParams.tree, path: [-1, $routeParams.method], forceReload: false });

            vm.config = userReportsConfig.config;
            vm.search = {};
            vm.sort = {};
            vm.page = {};
            vm.currentPage = 1;
            vm.itemsPerPage = 15;
            vm.sort.column = "Name";
            vm.sort.reverse = false;
            vm.filter = {};
            vm.filter.searchTerm = "";

            vm.columns = [];
            vm.selectedColumns = [];

            vm.userStates = [];
            vm.filter.selectedUserStates = [];

            vm.groups = [];
            vm.filter.selectedGroups = [];

            vm.setColumnFilter = function () {
                vm.selectedColumns = vm.columns.filter(function (c) {
                    return c.selected === true;
                });
            };

            vm.setUserStateFilter = function () {
                vm.filter.selectedUserStates = vm.userStates.filter(function (s) {
                    return s.selected === true;
                });
            };

            vm.setUserGroupsFilter = function () {
                vm.filter.selectedGroups = vm.groups.filter(function (g) {
                    return g.Selected === true;
                });
            };

            vm.getUsers = function (orderBy) {
                vm.isLoading = true;
                userReportsResources.getUsersPaged(vm.currentPage, vm.itemsPerPage, vm.filter, vm.sort.column, !vm.sort.reverse).then(function (data)  {
                    vm.page = data;
                    vm.isLoading = false;
                });
            }

            vm.getDefaultFilterValues = function () {
                vm.columns = userReportsResources.getInitialUserColumns();
                vm.selectedColumns = userReportsResources.getInitialUserColumns();
                vm.userStates = userReportsResources.getInitialUserStates();
                vm.filter.selectedUserStates = userReportsResources.getInitialUserStates();
                userReportsResources.getUserGroups().then(function (data) {
                    vm.groups = data;
                });
                vm.filter.selectedGroups = [];
                vm.setColumnFilter();
            }

            var init = function () {
                vm.getDefaultFilterValues();
                vm.getUsers();
            };
            init();

            vm.search = function () {
                vm.currentPage = 1;
                vm.getUsers();
            }

            vm.prevPage = function () {
                if (vm.currentPage > 1) {
                    vm.currentPage--;
                    vm.getUsers();
                }
            };

            vm.nextPage = function () {
                if (vm.currentPage < vm.page.TotalPages) {
                    vm.currentPage++;
                    vm.getUsers();
                }
            };

            vm.setPage = function (pageNumber) {
                vm.currentPage = pageNumber;
                vm.getUsers();
            };

            vm.reload = function () { 
                vm.getDefaultFilterValues();
                vm.currentPage = 1;
                vm.filter.searchTerm = "";
                vm.sort.column = "Name";
                vm.sort.reverse = false;
                vm.getUsers();
            };

            vm.exportCsv = function () {
                vm.isLoading = true;
                userReportsResources.exportUsersCsv(vm.selectedColumns, vm.filter, vm.sort.column, !vm.sort.reverse).then(function (data)  {
                    userReportsResources.downloadFile(data, 'csv', 'csv');
                    vm.isLoading = false;
                });
            };

            vm.exportExcel = function () {
                vm.isLoading = true;
                userReportsResources.exportUsersExcel(vm.selectedColumns, vm.filter, vm.sort.column, !vm.sort.reverse).then(function (data)  {
                    userReportsResources.downloadExcel(data, 'vnd.openxmlformats-officedocument.spreadsheetml.sheet', 'xlsx');
                    vm.isLoading = false;
                });
            };

            vm.sortBy = function (column) {
                vm.currentPage = 1;
                vm.sort.column = column;
                vm.sort.reverse = !vm.sort.reverse;
                vm.getUsers();
            };

            vm.getColName = function (array) {
                var name = 'All';
                var found = false;
                array.forEach(function (item) {
                    if (item.selected) {
                        if (!found) {
                            name = item.colName;
                            found = true;
                        } else {
                            name = name + ', ' + item.colName;
                        }
                    }
                });
                return name;
            };

            vm.getUserStateName = function (array) {
                var name = 'All';
                var found = false;
                array.forEach(function (item) {
                    if (item.selected) {
                        if (!found) {
                            name = item.name;
                            found = true;
                        } else {
                            name = name + ', ' + item.name;
                        }
                    }
                });
                return name;
            };

            vm.getUserGroupsName = function (array) {
                var name = 'All';
                var found = false;
                var count = 0;
                array.forEach(function (item) {
                    if (item.Selected) {
                        count++;
                        found = true;
                    }
                });
                if(found)
                    return count + (count === 1 ? " Group" : " Groups");
                else
                    return name;                
            };

            vm.getTableValue = function (valName, userModel) {
                switch (valName) {
                    case 'Name':
                        return userModel.Name;
                    case 'Culture':
                        return userModel.Culture;
                    case 'Email':
                        return userModel.Email;
                    case 'UserName':
                        return userModel.UserName;
                    case 'CreateDate':
                        return userModel.CreateDate;
                    case 'UpdateDate':
                        return userModel.UpdateDate;
                    case 'IsApproved':
                        return userModel.IsApproved;
                    case 'LockoutEnabled':
                        return userModel.LockoutEnabled;
                    case 'IsDisabled':
                        return userModel.IsDisabled;
                    case 'LastLoginDate':
                        return userModel.LastLoginDate;
                    case 'ExternalLoginEnabled':
                        return userModel.ExternalLoginEnabled;
                    case 'HasStartContentNode':
                        return userModel.HasStartContentNode;
                    case 'HasStartMediaNode':
                        return userModel.HasStartMediaNode;
                    case 'AccessFailedCount':
                        return userModel.AccessFailedCount;
                    case 'AllowedSections':
                        return userModel.AllowedSections.length === 0 ? "None" : userModel.AllowedSections.join(", ");
                    case 'Groups':
                        return userModel.Groups.join(", ");
                    case 'AccessFailedCount':
                        return userModel.AccessFailedCount;
                    case 'Roles':
                        return userModel.Roles;
                    default:
                        return "";
                }
            };

            vm.openUser = function (userId) {
                const editor = {
                    id: userId,
                    submit: function () {
                        vm.init();
                        editorService.close();
                    },
                    close: function () {
                        editorService.close();
                    }
                };
                editorService.memberEditor(editor); //not correct, need to find correct user editor
            };
        });
})();