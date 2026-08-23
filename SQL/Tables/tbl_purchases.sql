CREATE TABLE IF NOT EXISTS `tbl_purchases` (
  `Id`              int NOT NULL AUTO_INCREMENT,
  `Uuid`            varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `PurchaseNumber`  varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `SupplierUuid`    varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `InvoiceNumber`   varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `PurchaseDate`    date NOT NULL,
  `TotalCost`       decimal(18,2) NOT NULL DEFAULT '0.00',
  `TotalItems`      int NOT NULL DEFAULT '0',
  `Status`          varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL DEFAULT 'Received',
  `Notes`           varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,

  -- IAuditable
  `CreatedAt`       datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt`       datetime(6) DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  `CreatedBy`       varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `UpdatedBy`       varchar(36) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `IsActive`        tinyint(1) NOT NULL DEFAULT '1',

  PRIMARY KEY (`Id`),
  UNIQUE KEY `AK_tbl_purchases_Uuid` (`Uuid`),
  UNIQUE KEY `IX_tbl_purchases_PurchaseNumber` (`PurchaseNumber`),
  KEY `IX_tbl_purchases_SupplierUuid` (`SupplierUuid`),
  KEY `IX_tbl_purchases_PurchaseDate` (`PurchaseDate`),
  KEY `FK_tbl_purchases_tbl_users_CreatedBy` (`CreatedBy`),
  KEY `FK_tbl_purchases_tbl_users_UpdatedBy` (`UpdatedBy`),
  CONSTRAINT `FK_tbl_purchases_tbl_suppliers_SupplierUuid` FOREIGN KEY (`SupplierUuid`) REFERENCES `tbl_suppliers` (`Uuid`) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `FK_tbl_purchases_tbl_users_CreatedBy` FOREIGN KEY (`CreatedBy`) REFERENCES `tbl_users` (`Uuid`) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `FK_tbl_purchases_tbl_users_UpdatedBy` FOREIGN KEY (`UpdatedBy`) REFERENCES `tbl_users` (`Uuid`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
