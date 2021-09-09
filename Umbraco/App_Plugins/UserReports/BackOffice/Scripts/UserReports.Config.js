(function () {
    'use strict';
    angular.module("umbraco")
        .constant("userReportsConfig", {
            "baseApiUrl": "BackOffice/Api/UserReportsApi/",
            "permissionsApiUrl": "BackOffice/Api/PermissionsApi/",
            "config": {
                "version": "1.0.0",
                "editGroupUrl": "#/users/users/group/",
                "editUserUrl": "#/users/users/user/",
                "baseTreeUrl": "#/settings/userReports/",
                "baseViewUrl": "#/settings/userReports/"
            }
        })
        .directive('userReportsTrueFalse', function () {
            return {
                restrict: 'E',
                scope: {
                    value: '='
                },
                link: function (scope, element, attrs) {
                },
                template: '<span ng-show="value"><i class="icon icon-checkbox"></i> Yes</span><span ng-show="!value"><i class="icon icon-checkbox-empty"></i> No</span>'
            };
        })
        .directive('userReportsSortable', function () {
            return {
                restrict: 'E',
                scope: {
                    column: '@',
                    sort: "=",
                    sortBy: "&"
                },
                transclude: true,
                link: function (scope, element, attrs) {
                },
                template: '<a ng-click="sortBy()" ng-transclude></a><i class="icon" ng-show="sort.column === column && !sort.reverse">&#9650;</i><i class="icon" ng-show="sort.column === column && sort.reverse">&#9660;</i>'
            };
        })
        .directive('exportHeader', function ($route, userReportsConfig) {
            return {
                restrict: 'E',
                scope: {
                    heading: '@',
                    tooltip: '@',
                    onReload: '&',
                    exportExcel: '&',
                    exportCsv: '&'
                },
                link: function (scope, element, attrs) {
                    scope.version = userReportsConfig.config.version;
                },
                templateUrl: "/App_Plugins/UserReports/BackOffice/UserReports/exportHeader.html"
            };
        })
        .directive('userGroupSwitcherHeader', function ($route, userReportsConfig) {
            return {
                restrict: 'E',
                scope: {
                    heading: '@',
                    tooltip: '@',
                    onReload: '&',
                    onGroups: '&',
                    onUsers: '&',
                    usersActive: '=',
                    groupsActive: '='
                },
                link: function (scope, element, attrs) {
                    scope.version = userReportsConfig.config.version;
                },
                templateUrl: "/App_Plugins/UserReports/BackOffice/UserReports/userGroupSwitcherHeader.html"
            };
        })
        .directive('clearable', function () {
            return {
                require: 'ngModel',
                link: function (scope, element, attrs, control) {

                    var wrapper = angular.element('<div>');
                    var button = angular.element('<div>').addClass('close-button');

                    button.bind('click', function () {
                        control.$setViewValue('');
                        element.val('');
                        scope.$apply();
                    });

                    element.wrap(wrapper);
                    element.parent().append(button);
                }
            };
        })
        .filter('userReportsFileSize', function () {
            return function (bytes, precision) {
                if (!bytes) return "N/A";
                if (isNaN(parseFloat(bytes)) || !isFinite(bytes)) return '-';
                if (typeof precision === 'undefined') precision = 1;
                var units = ['bytes', 'KB', 'MB', 'GB'];
                var number = Math.floor(Math.log(bytes) / Math.log(1024));
                return (bytes / Math.pow(1024, Math.floor(number))).toFixed(precision) + ' ' + units[number];
            };
        })
        .filter('userReportsUnique', function () {

            return function (items, filterOn) {

                if (filterOn === false) {
                    return items;
                }

                if ((filterOn || angular.isUndefined(filterOn)) && angular.isArray(items)) {
                    var hashCheck = {}, newItems = [];

                    var extractValueToCompare = function (item) {
                        if (angular.isObject(item) && angular.isString(filterOn)) {
                            return item[filterOn];
                        } else {
                            return item;
                        }
                    };

                    angular.forEach(items, function (item) {
                        var valueToCheck, isDuplicate = false;

                        for (var i = 0; i < newItems.length; i++) {
                            if (angular.equals(extractValueToCompare(newItems[i]), extractValueToCompare(item))) {
                                isDuplicate = true;
                                break;
                            }
                        }
                        if (!isDuplicate) {
                            newItems.push(item);
                        }

                    });
                    items = newItems;
                }
                return items;
            };
        });
})();