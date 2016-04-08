var kilometersPerMile = 1.609344;
var app = angular.module('StarterApp', ['ngMaterial', 'ngMdIcons', 'ngRoute']);

app.controller('AppCtrl', [
    '$scope', '$http', '$mdBottomSheet', '$mdSidenav', '$mdDialog', '$q', 'VehicleService', 'WaitForLogonService', function ($scope, $http, $mdBottomSheet, $mdSidenav, $mdDialog, $q, VehicleService, WaitForLogonService) {
        $scope.toggleSidenav = function(menuId) {
            $mdSidenav(menuId).toggle();
        };
        $scope.email = '';
        $scope.kilometersPerMile = kilometersPerMile;
        $scope.menu = [
            {
                link: '/vehicle',
                title: 'Start',
                icon: 'explore'
            },
            {
                link: '/report',
                title: 'Reports',
                icon: 'dashboard'
            }
        ];
        $scope.admin = [
            {
                link: 'showListBottomSheet($event)',
                title: 'Settings',
                icon: 'settings'
            }
        ];
        
        $scope.signOut = function() {
            $http.delete('api/token').then(function () {
                $scope.email = '';
                VehicleService.reset();
                $scope.trips = [];
                $scope.showSignIn();
            });
        };

        $scope.showListBottomSheet = function($event) {
            $mdBottomSheet.show({
                template: '<md-bottom-sheet class="md-list md-has-header"> <md-subheader>Settings</md-subheader> <md-list> <md-list-item ng-repeat="item in items"><md-list-item-text md-ink-ripple flex class="inset"> <a flex aria-label="{{item.name}}" ng-click="listItemClick($index)"> <span class="md-inline-list-icon-label">{{ item.name }}</span> </a></md-list-item-text> </md-item> </md-list-item></md-bottom-sheet>',
                controller: 'ListBottomSheetCtrl',
                targetEvent: $event
            });
        };

        $scope.setToken = function (obj) {
            if (angular.isDefined(obj.data)) {
                obj = obj.data;
            }
            $scope.email = obj.email;
            $http.defaults.headers.common.ApiKey = obj.token;
            $http.defaults.headers.common.Email = obj.email;

            var deferred = $q.defer();
            deferred.resolve(obj);
            return deferred.promise;
        }

        WaitForLogonService.promise().then(VehicleService.run);
        
        $scope.showSignIn = function() {
            $http.get('api/token', { withCredentials: true }).then($scope.setToken,
                function() {
                    return $mdDialog.show({
                            controller: DialogController,
                            template: '<md-dialog aria-label="Sign in form"> <md-dialog-content class="md-padding"> <h3>Enter your Tesla credentials</h3><md-progress-circular md-mode="indeterminate" md-diameter="60" ng-show="working"></md-progress-circular><form name="userForm" ng-hide="working"> <md-input-container class="md-block"> <label>User name</label> <input ng-model="email" placeholder="User name" type="email"> </md-input-container><md-input-container class="md-block"> <label>Password</label> <input ng-model="password" type="password" autocorrect="false"> </md-input-container> </form> <div class="md-dialog-actions" layout="row"> <span flex></span> <md-button ng-click="signIn()" class="md-primary"> Sign in </md-button> </div></md-dialog-content></md-dialog>',
                            escapeToClose: false,
                        })
                        .then($scope.setToken);
                }).then(WaitForLogonService.set);
        };

        $scope.showSignIn();
    }
]);

app.factory('WaitForLogonService', [
        '$q', function($q) {
            var waitSerivce = {};
            var deferred = $q.defer();

            waitSerivce.set = function() {
                deferred.resolve({});
            }

            waitSerivce.promise = function() {
                return deferred.promise;
            }

            return waitSerivce;
        }
    ]
);
app.factory('VehicleService', ['$http','$q', function ($http,$q) {
    var vehicleService = {};
    vehicleService.vehicles = [];

    vehicleService.run = function () {
        var deferred = $q.defer();
        if (vehicleService.vehicles.length === 0) {
            $http.get('api/vehicle').then(function(result) {
                vehicleService.vehicles = result.data;
                for (var i = 0; i < vehicleService.vehicles.length; i++) {
                    var vehicle = vehicleService.vehicles[i];
                    vehicle.working = true;
                    $http.get('api/vehiclestate/?vehicleId=' + vehicle.vehicleId + '&token=' + vehicle.token).then(function(reading) {
                        vehicle.mileage = parseFloat(reading.data.odometer, 10);
                        vehicle.est_lat = reading.data.est_lat;
                        vehicle.est_lng = reading.data.est_lng;
                        vehicle.working = false;
                    });
                }
                deferred.resolve({});
            });
        } else {
            deferred.resolve({});
        }
        return deferred.promise;
    }

    vehicleService.list = function () {
        return vehicleService.vehicles;
    };

    vehicleService.findVehicle = function (vehicleId) {
        for (var i = 0; i < vehicleService.vehicles.length; i++) {
            if (vehicleService.vehicles[i].vehicleId === vehicleId) {
                return vehicleService.vehicles[i];
            }
        }
        return null;
    }

    vehicleService.restore = function() {
        vehicleService.vehicles = [];
    }

    return vehicleService;
}]);

app.controller('VehicleReportController', [
    '$scope', '$http', 'VehicleService', 'WaitForLogonService', function ($scope, $http, VehicleService, WaitForLogonService) {
        $scope.kilometersPerMile = kilometersPerMile;
        $scope.reports = [];

        WaitForLogonService.promise().then(function () {
            $http.get('api/worktripreport').then(function(result) {
                $scope.reports = result.data;
                for (var i = 0; i < $scope.reports.length; i++) {
                    var vehicle = VehicleService.findVehicle($scope.reports[i].VehicleId);
                    if (vehicle) {
                        $scope.reports[i].Name = vehicle.displayName;
                    }
                }
            });
        });
}]);

app.controller('VehicleController', [
    '$scope', '$http', '$q', 'VehicleService', 'WaitForLogonService', function ($scope, $http, $q, VehicleService, WaitForLogonService) {
        $scope.kilometersPerMile = kilometersPerMile;
        $scope.trips = [];

        $scope.startWorkTrip = function (vehicle) {
            var workTrip = {
                waiting: true,
                VehicleId: vehicle.vehicleId,
                StartLat: vehicle.est_lat,
                StartLng: vehicle.est_lng,
                StartMileage: vehicle.mileage
            };
            $scope.trips.unshift(workTrip);
            return $http.post('api/worktrip', workTrip).then(function (result) {
                workTrip.Id = result.data.Id;
                workTrip.Commenced = result.data.Commenced;
                workTrip.waiting = false;
            });
        }

        $scope.deleteWorkTrip = function (workTrip) {
            return $http.delete('api/worktrip/' + workTrip.Id).then(function () {
                for (var i = 0; i < $scope.trips.length; i++) {
                    if ($scope.trips[i].Id === workTrip.Id) {
                        $scope.trips.splice(i, 1);
                        return;
                    }
                }
            });
        }

        $scope.endWorkTrip = function (workTrip) {
            workTrip.working = true;
            var vehicle = VehicleService.findVehicle(workTrip.VehicleId);
            $http.get('api/vehicle/' + vehicle.id).then(function (result) {
                var refreshedVehicle = result.data;
                $http.get('api/vehiclestate/?vehicleId=' + refreshedVehicle.vehicleId + '&token=' + refreshedVehicle.token).then(function (reading) {
                    var deferred = $q.defer();
                    if (vehicle) {
                        vehicle.mileage = parseFloat(reading.data.odometer);
                        vehicle.est_lat = reading.data.est_lat;
                        vehicle.est_lng = reading.data.est_lng;
                        deferred.resolve(vehicle);
                    } else {
                        deferred.reject({});
                    }

                    return deferred.promise;
                }).then(function (v) {
                    workTrip.EndMileage = v.mileage;
                    workTrip.EndLat = v.est_lat;
                    workTrip.EndLng = v.est_lng;
                    return $http.put('api/worktrip/' + workTrip.Id, workTrip).then(function (r) {
                        workTrip.Distance = r.data.Distance;
                        workTrip.waiting = false;
                    });
                });
            });
        }

        $scope.storeMileage = function (vehicle) {
            var mileage = {
                waiting: true,
                VehicleId: vehicle.vehicleId,
                Mileage: vehicle.mileage,
                Date: new Date()
            };
            return $http.post('api/mileage', mileage).then(function (result) {
                mileage.Id = result.data.Id;
                mileage.waiting = false;
            });
        }

        $scope.vehicles = [];

        var loadTrips = function () {
            return $http.get('api/worktrip').then(function (result) {
                $scope.trips = result.data;
            });
        }

        var loadStuff = function () {
            return $q.all([loadTrips(), VehicleService.run()]).then(function() {
                $scope.vehicles = VehicleService.list();
            });
        }

        WaitForLogonService.promise().then(loadStuff);
    }
]);

app.controller('ListBottomSheetCtrl', function ($scope, $mdBottomSheet, $window) {
    $scope.items = [
        { name: 'Configure units', icon: 'gear', func: function() {} },
        { name: 'Print this page', icon: 'print', func: function() { $window.print(); } },
    ];

    $scope.listItemClick = function($index) {
        var clickedItem = $scope.items[$index];
        if (clickedItem.func) clickedItem.func();
        $mdBottomSheet.hide(clickedItem);
    };
});

function DialogController($scope, $mdDialog, $http) {
    $scope.working = false;

    function hasOwnProperty(obj, prop) {
        var proto = obj.__proto__ || obj.constructor.prototype;
        return (prop in obj) &&
            (!(prop in proto) || proto[prop] !== obj[prop]);
    }

    $scope.signIn = function () {
        $scope.working = true;
        $http.post('api/token', { email: $scope.email, password: $scope.password }).then(function (result) {
            if (!hasOwnProperty(result.data, 'error')) {
                $mdDialog.hide(result.data);
            }
        }, function() {
            $scope.working = false;
        });
    };
};

app.directive('userAvatar', function() {
    return {
        replace: true,
        template: '<svg class="user-avatar" viewBox="0 0 128 128" height="64" width="64" pointer-events="none" display="block" style="fill-rule:evenodd;clip-rule:evenodd;stroke-linecap:round;stroke-linejoin:round;stroke-miterlimit:1.41421;"><g transform="matrix(0.873876,0,0,0.873876,-59.0728,-35.7177)"><path d="M75.827,88.703C75.827,88.703 83.395,73.423 125.629,75.615C158.591,77.326 177.055,100.988 177.055,100.988C177.055,100.988 203.414,112.338 208.718,131.021C213.47,147.765 186.796,155.866 180.156,157.832C173.517,159.798 147.496,154.113 133.035,146.335C111.209,134.596 86.497,119.832 77.593,112.369C65.431,102.175 75.827,88.703 75.827,88.703Z" style="fill:rgb(51,51,51);stroke-width:0.86px;stroke:black;"/></g><g transform="matrix(0.873876,0,0,0.873876,-59.0728,-35.7177)"><path d="M94.317,90.143C94.317,90.143 118.991,96.457 138.852,116.471C88.624,102.225 94.317,90.143 94.317,90.143Z" style="fill:black;stroke-width:0.86px;stroke:white;"/></g><g transform="matrix(0.873876,0,0,0.873876,-59.0728,-35.7177)"><path d="M186.133,145.091C186.133,145.091 178.504,136.167 153.763,121.131C142.689,114.401 129.986,98.83 105.322,89.941" style="fill:none;stroke-width:0.86px;stroke:black;"/></g><g transform="matrix(0.873876,0,0,0.873876,-59.0728,-35.7177)"><path d="M180.061,147.487C180.061,147.487 171.52,150.495 131.53,129.028" style="fill:none;stroke-width:0.86px;stroke:white;"/></g></svg>'
    };
});

app.config(function ($mdThemingProvider, $routeProvider, $locationProvider) {

    var customBlueMap = $mdThemingProvider.extendPalette('light-blue', {
        'contrastDefaultColor': 'light',
        'contrastDarkColors': ['50'],
        '50': 'ffffff'
    });
    $mdThemingProvider.definePalette('customBlue', customBlueMap);
    $mdThemingProvider.theme('default')
        .primaryPalette('customBlue', {
            'default': '500',
            'hue-1': '50'
        })
        .accentPalette('pink');
    $mdThemingProvider.theme('input', 'default')
        .primaryPalette('grey');

    $routeProvider
     .when('/report', {
         templateUrl: 'report.html',
         controller: 'VehicleReportController',
     })
    .otherwise({
        templateUrl: 'vehicle.html',
        controller: 'VehicleController'
    });

    // configure html5 to get links working on jsfiddle
    $locationProvider.html5Mode(true);
});
