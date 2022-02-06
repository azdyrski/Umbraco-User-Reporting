(function () {
    'use strict';
    angular.module("umbraco").controller("UserReports.UserAudits.Controller",
        function ($routeParams, navigationService, userReportsResources, userReportsConfig) {

            var vm = this;

            navigationService.syncTree({ tree: $routeParams.tree, path: [-1, $routeParams.method], forceReload: false });

            vm.config = userReportsConfig.config;
            vm.sort = {};
            vm.page = {};
            vm.currentPage = 1;
            vm.itemsPerPage = 15;
            vm.sort.column = "AuditDate";
            vm.sort.reverse = true;
            vm.filter = {};

            vm.columns = [];
            vm.selectedColumns = [];

            vm.users = [];
            vm.filter.selectedUserId = 0;

            vm.auditTypes = [];
            vm.filter.selectedAuditTypes = [];

            vm.setColumnFilter = function () {
                vm.selectedColumns = vm.columns.filter(function (c) {
                    return c.selected === true;
                });
            };

            vm.setAuditTypeFilter = function () {
                vm.filter.selectedAuditTypes = vm.auditTypes.filter(function (s) {
                    return s.selected === true;
                });
            };

            vm.getAudits = function (orderBy) {
                vm.isLoading = true;
                userReportsResources.getAudits(vm.currentPage, vm.itemsPerPage, vm.filter, vm.sort.column, !vm.sort.reverse).then(function (data) {
                    vm.page = data;
                    vm.isLoading = false;
                });
            }

            vm.getDefaultFilterValues = function () {
                vm.columns = userReportsResources.getInitialAuditColumns();
                vm.selectedColumns = userReportsResources.getInitialAuditColumns();
                vm.auditTypes = userReportsResources.getAuditTypes();
                //vm.filter.selectedAuditTypes = userReportsResources.getAuditTypes()[0];
                userReportsResources.getAuditUsers().then(function (data) {
                    vm.users = data;
                });
                vm.setColumnFilter();
            }

            var init = function () {
                vm.getDefaultFilterValues();
                vm.getAudits();
            };
            init();

            vm.search = function () {
                vm.currentPage = 1;
                vm.getAudits();
            }

            vm.prevPage = function () {
                if (vm.currentPage > 1) {
                    vm.currentPage--;
                    vm.getAudits();
                }
            };

            vm.nextPage = function () {
                if (vm.currentPage < vm.page.TotalPages) {
                    vm.currentPage++;
                    vm.getAudits();
                }
            };

            vm.setPage = function (pageNumber) {
                vm.currentPage = pageNumber;
                vm.getAudits();
            };

            vm.reload = function () {
                vm.getDefaultFilterValues();
                vm.currentPage = 1;
                vm.sort.column = "AuditDate";
                vm.sort.reverse = true;
                vm.getAudits();
            };

            vm.auditUserChange = function () {
                vm.currentPage = 1;
                vm.sort.column = "AuditDate";
                vm.sort.reverse = true;
                vm.getAudits();
            }

            vm.exportCsv = function () {
                vm.isLoading = true;
                userReportsResources.exportAuditCsv(vm.selectedColumns, vm.filter, vm.sort.column, !vm.sort.reverse).then(function (data) {
                    userReportsResources.downloadFile(data, 'csv', 'userAudits', 'csv');
                    vm.isLoading = false;
                });
            };

            vm.exportExcel = function () {
                vm.isLoading = true;
                userReportsResources.exportAuditExcel(vm.selectedColumns, vm.filter, vm.sort.column, !vm.sort.reverse).then(function (data) {
                    userReportsResources.downloadExcel(data, 'vnd.openxmlformats-officedocument.spreadsheetml.sheet', 'userAudits', 'xlsx');
                    vm.isLoading = false;
                });
            };

            vm.sortBy = function (column) {
                vm.currentPage = 1;
                vm.sort.column = column;
                vm.sort.reverse = !vm.sort.reverse;
                vm.getAudits();
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

            vm.getAuditTypesName = function (array) {
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

            vm.getTableValue = function (valName, auditModel) {
                switch (valName) {
                    case 'UserId':
                        return auditModel.UserId;
                    case 'UserEmail':
                        return auditModel.UserEmail;
                    case 'Comment':
                        return auditModel.Comment;
                    case 'Parameters':
                        return auditModel.Parameters;
                    case 'AuditType':
                        return auditModel.AuditType;
                    case 'EntityType':
                        return auditModel.EntityType;
                    case 'EntityId':
                        return auditModel.EntityId == "0" || auditModel.EntityId == "-1" ? "N/A" : auditModel.EntityId;
                    case 'AuditDate':
                        return auditModel.AuditDate;
                    default:
                        return "";
                }
            };
        });
})();