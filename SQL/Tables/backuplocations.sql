CREATE TABLE `backuplocations` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Uuid` varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Path` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `IsRemote` tinyint(1) NOT NULL,
  `IsDefault` tinyint(1) NOT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  `CreatedBy` varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `UpdatedBy` varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `AK_BackupLocations_Uuid` (`Uuid`),
  KEY `FK_BackupLocations_Users_CreatedBy` (`CreatedBy`),
  KEY `FK_BackupLocations_Users_UpdatedBy` (`UpdatedBy`),
  CONSTRAINT `FK_BackupLocations_Users_CreatedBy` FOREIGN KEY (`CreatedBy`) REFERENCES `users` (`Uuid`) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `FK_BackupLocations_Users_UpdatedBy` FOREIGN KEY (`UpdatedBy`) REFERENCES `users` (`Uuid`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=36 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci