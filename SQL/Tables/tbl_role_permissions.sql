CREATE TABLE `tbl_role_permissions` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `RoleId` int NOT NULL,
  `PermissionId` int NOT NULL,
  `Uuid` varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `AK_tbl_role_permissions_Uuid` (`Uuid`),
  KEY `IX_tbl_role_permissions_PermissionId` (`PermissionId`),
  KEY `IX_tbl_role_permissions_RoleId` (`RoleId`),
  CONSTRAINT `FK_tbl_role_permissions_tbl_permissions_PermissionId` FOREIGN KEY (`PermissionId`) REFERENCES `tbl_permissions` (`Id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_tbl_role_permissions_tbl_roles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `tbl_roles` (`Id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
