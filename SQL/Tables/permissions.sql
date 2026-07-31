CREATE TABLE `permissions` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `PermissionType` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Description` varchar(250) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `PermissionCatagory` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Permissions_PermissionType` (`PermissionType`)
) ENGINE=InnoDB AUTO_INCREMENT=1109 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci