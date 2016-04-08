CREATE TABLE `TDJ_User` (
 `Id` int(11) NOT NULL AUTO_INCREMENT,
 `Email` varchar(400) NOT NULL,
 `Token` varchar(255) DEFAULT NULL,
 `TokenUpdatedOn` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
 `UseMetricSystem` tinyint(1) NOT NULL DEFAULT '1',
 PRIMARY KEY (`Id`),
 UNIQUE KEY `Email` (`Email`)
) AUTO_INCREMENT=11 DEFAULT CHARSET=latin1

GO

CREATE TABLE `TDJ_Vehicle` (
 `Id` int(11) NOT NULL AUTO_INCREMENT,
 `TeslaId` varchar(50) NOT NULL,
 `TeslaVehicleId` varchar(50) NOT NULL,
 `Name` varchar(300) DEFAULT NULL,
 `UserId` int(11) NOT NULL,
 PRIMARY KEY (`Id`)
) DEFAULT CHARSET=latin1

GO

CREATE TABLE `TDJ_WorkTrip` (
 `Id` int(11) NOT NULL AUTO_INCREMENT,
 `UserId` int(11) NOT NULL,
 `Commenced` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
 `VehicleId` bigint(11) NOT NULL,
 `StartMileage` bigint(20) NOT NULL,
 `EndMileage` bigint(20) DEFAULT NULL,
 `StartLat` varchar(50) DEFAULT NULL,
 `StartLng` varchar(50) DEFAULT NULL,
 `EndLat` varchar(50) DEFAULT NULL,
 `EndLng` varchar(50) DEFAULT NULL,
 `Note` varchar(400) DEFAULT NULL,
 PRIMARY KEY (`Id`)
) AUTO_INCREMENT=8 DEFAULT CHARSET=latin1