<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">
    void redirectToHttps()
    {
#if !DEBUG
        if (Request.Url.Scheme.ToLowerInvariant() != "https")
        {
            var hostBefore = Request.Url.Host;
            var host = hostBefore.Replace(".", "-") + ".loopiasecure.com";
            Response.RedirectPermanent(new UriBuilder("https", host, 443, Request.Url.PathAndQuery).Uri.ToString(), true);
        }
#endif
    }
</script>

<% redirectToHttps(); %>

<!DOCTYPE html>
<html lang="en" ng-app="StarterApp">
<head>
    <title>T Driving Journal</title>
    <base href="/">
    <link rel="stylesheet" href="//ajax.googleapis.com/ajax/libs/angular_material/1.0.0/angular-material.min.css">
    <link rel="stylesheet" href="css/style.css" />
</head>
<body layout="row" ng-controller="AppCtrl">
    <md-sidenav layout="column" class="md-sidenav-left md-whiteframe-z2" md-component-id="left" md-is-locked-open="$mdMedia('gt-md')">
      <md-toolbar class="md-tall md-hue-2">
        <span flex></span>
        <div layout="column" class="md-toolbar-tools-bottom inset">
          <user-avatar></user-avatar>
          <span></span>
          <div>{{email}}</div>
          <div><a ng-click="signOut()">Sign out</a></div>
        </div>
      </md-toolbar>
      <md-list>
      <md-list-item ng-repeat="item in menu">
        <a href="{{item.link}}">
          <md-list-item-text md-ink-ripple layout="row" layout-align="start center">
            <div class="inset">
              <ng-md-icon icon="{{item.icon}}"></ng-md-icon>
            </div>
            <div class="inset">{{item.title}}
            </div>
          </md-list-item-text>
        </a>
      </md-list-item>
      <md-divider></md-divider>
      <md-subheader>Management</md-subheader>
      <md-list-item ng-repeat="item in admin">
        <a ng-click="$parent.$eval(item.link)">
          <md-list-item-text md-ink-ripple layout="row" layout-align="start center">
            <div class="inset">
              <ng-md-icon icon="{{item.icon}}"></ng-md-icon>
            </div>
            <div class="inset">{{item.title}}
            </div>
          </md-list-item-text>
        </a>
      </md-list-item>
    </md-list>
    </md-sidenav>
    <div layout="column" class="relative" layout-fill role="main">
        <md-toolbar>
        <div class="md-toolbar-tools">
          <md-button ng-click="toggleSidenav('left')" hide-gt-md aria-label="Menu">
            <ng-md-icon icon="menu"></ng-md-icon>
          </md-button>
          <h3>
            T Driving Journal
          </h3>
          <span flex></span>
          <md-button aria-label="Open Settings" ng-click="showListBottomSheet($event)">
            <ng-md-icon icon="more_vert"></ng-md-icon>
          </md-button>
        </div>
      </md-toolbar>
        <md-content flex md-scroll-y>
        <ui-view layout="column" layout-fill layout-padding ng-view>
        </ui-view>
      </md-content>
    </div>
    <!-- Angular Material Dependencies -->
    <script src="//ajax.googleapis.com/ajax/libs/angularjs/1.4.8/angular.min.js"></script>
    <script src="//ajax.googleapis.com/ajax/libs/angularjs/1.4.8/angular-animate.min.js"></script>
    <script src="//ajax.googleapis.com/ajax/libs/angularjs/1.4.8/angular-aria.min.js"></script>
    <script src="//ajax.googleapis.com/ajax/libs/angularjs/1.4.8/angular-route.min.js"></script>

    <script src="//ajax.googleapis.com/ajax/libs/angular_material/1.0.0/angular-material.min.js"></script>

    <script src="//cdn.jsdelivr.net/angular-material-icons/0.4.0/angular-material-icons.min.js"></script>
    <script src="js/index.js" type="text/javascript"></script>
</body>
</html>
