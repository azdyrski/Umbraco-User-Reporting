(function () {
    'use strict';
    angular.module("umbraco").controller("UserReports.Intro.Controller",
        function ($routeParams, navigationService, userReportsConfig) {

            var vm = this;
            vm.config = userReportsConfig.config;

            navigationService.syncTree({ tree: $routeParams.tree, path: [], forceReload: false });

            vm.pages = [
                { name: "User Browser", url: "userBrowser", desc: "Browse, filter, and export your Umbraco Back Office users" },
                { name: "Permissions", url: "permissions", desc: "View Permissions of your Users and Groups" },
                { name: "Permissions by Content", url: "permissionsByContent", desc: "Browse User Permissions based Content" },
                { name: "User Activity", url: "userActivity", desc: "View and Export metrics about your Users' Back Office activity" }
            ];

        });
})();