CREATE TABLE `tbl_suppliers` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Description` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Uuid` varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  `CreatedBy` varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `UpdatedBy` varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `AK_tbl_suppliers_Uuid` (`Uuid`),
  UNIQUE KEY `IX_tbl_suppliers_Name` (`Name`),
  KEY `FK_tbl_suppliers_tbl_users_CreatedBy` (`CreatedBy`),
  KEY `FK_tbl_suppliers_tbl_users_UpdatedBy` (`UpdatedBy`),
  CONSTRAINT `FK_tbl_suppliers_tbl_users_CreatedBy` FOREIGN KEY (`CreatedBy`) REFERENCES `tbl_users` (`Uuid`) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `FK_tbl_suppliers_tbl_users_UpdatedBy` FOREIGN KEY (`UpdatedBy`) REFERENCES `tbl_users` (`Uuid`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
