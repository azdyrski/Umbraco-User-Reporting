(function () {
    'use strict';
    angular.module("umbraco").controller("UserReports.Permissions.Controller",
        function ($routeParams, navigationService, userReportsResources, userReportsConfig, editorService) {

            var vm = this;

            navigationService.syncTree({ tree: $routeParams.tree, path: [-1, $routeParams.method], forceReload: false });

            vm.config = userReportsConfig.config;
            vm.showingGroups = false;
            vm.search = {};
            vm.sort = {};
            vm.items = [];
            vm.sort.column = "Name";
            vm.sort.reverse = false;
            vm.filter = {};
            vm.filter.searchTerm = "";


            vm.groups = [];
            vm.filter.selectedGroups = [];

            vm.getDefaultFilterValues = function () {
                vm.filter.searchTerm = "";
                vm.sort.column = "Name";
                vm.sort.reverse = false;
            };

            vm.getUsers = function (orderBy) {
                vm.isLoading = true;
                userReportsResources.getUsersWithPermissions(vm.filter, vm.sort.column, !vm.sort.reverse).then(function (data)  {
                    console.log("User Data: ", data);
                    vm.items = data;
                    vm.isLoading = false;
                });
            }

            vm.getGroups = function (orderBy) {
                vm.isLoading = true;
                userReportsResources.getGroupsWithPermissions(vm.filter, vm.sort.column, !vm.sort.reverse).then(function (data)  {
                    console.log("Group Data: ", data);
                    vm.items = data;
                    vm.isLoading = false;
                });
            } 

            vm.getResults = function (orderBy) {
                if(vm.showingGroups)
                    vm.getGroups(orderBy);
                else
                    vm.getUsers(orderBy);
            }


            var init = function () {
                vm.getDefaultFilterValues();
                vm.getResults();
            };
            init();

            vm.showGroups = function() {
                vm.showingGroups = true;
                vm.items = [];
                vm.reload();
            };

            vm.showUsers = function() {
                vm.showingGroups = false;
                vm.items = [];
                vm.reload();
            };

            vm.search = function () {
                vm.currentPage = 1;
                vm.getResults();
            };

            

            vm.reload = function () { 
                vm.getDefaultFilterValues();
                vm.filter.searchTerm = "";
                vm.sort.column = "Name";
                vm.sort.reverse = false;
                vm.getResults();
            };

            vm.sortBy = function (column) {
                vm.currentPage = 1;
                vm.sort.column = column;
                vm.sort.reverse = !vm.sort.reverse;
                vm.getResults();
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

            vm.getAllowedSectionList = function (record) {
                if(!record.Permissions.AllowedSections)
                    return "None";
                if(record.Permissions.AllowedSections.length < 1)
                    return "None";
                return record.Permissions.AllowedSections.map(function(section) { return section.Name }).join(", ");
            }

            vm.getActionsList = function (record) {
                if(!record.Permissions.Actions)
                    return "None";
                if(record.Permissions.Actions.length < 1)
                    return "None";
                return record.Permissions.Actions.map(function(action) { return action.Name }).join(", ");
            }

            vm.getUserGroupList = function (record) {
                if(!record.Groups)
                    return "None";
                if(record.Groups < 1)
                    return "None";
                return record.Groups.map(function(group) { return group }).join(", ");
            }

            vm.getStartNodeLink = function (startNode, media) {
                if(!media)
                    return startNode.Id == -1 ? "/umbraco/#/content" : ("/umbraco/#/content/content/edit/" + startNode.Id);
                else
                    return startNode.Id == -1 ? "/umbraco/#/media" : ("/umbraco/#/media/media/edit/" + startNode.Id);
            }

            vm.openGroup = function (groupId) {
                const editor = {
                    id: groupId,
                    submit: function () {
                        vm.reload();
                        editorService.close();
                    },
                    close: function () {
                        editorService.close();
                    }
                };
                editorService.userGroupEditor(editor);
            };
        });
})();