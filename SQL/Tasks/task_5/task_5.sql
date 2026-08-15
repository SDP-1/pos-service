-- ====================================================================================
-- TASK MIGRATION SCRIPT: Task 5
-- DESCRIPTION: Hierarchical Roles, System Reports & Item Price Audits Engine
-- ====================================================================================

-- 1. REQUIRED FILES & TRIGGERS TO APPLY
-- ====================================================================================
/*
Required Apply:
- tbl_roles.sql
- tbl_item_price_audits.sql
- tbl_item_prices.sql
       - trg_tbl_item_prices_after_insert
       - trg_tbl_item_prices_after_update
- sp_report_daily_sales.sql
- sp_report_sales_summary.sql
- sp_report_sales_details.sql
- sp_report_product_sales.sql
- sp_report_category_sales.sql
- sp_report_current_stock.sql
- sp_report_low_stock.sql
- sp_report_purchase.sql
- sp_report_expense.sql
- sp_report_profit_loss.sql
- sp_report_cash_register.sql
- sp_report_customer_sales.sql
- sp_report_supplier.sql
- sp_report_sales_return.sql
- sp_report_cashier_performance.sql
*/


-- 2. ALTER TABLE STATEMENTS & DATA UPDATES
-- ====================================================================================

-- Add Hierarchical Role Columns to tbl_roles if they do not exist
SET @dbname = DATABASE();
SET @tablename = 'tbl_roles';
SET @columnname = 'ParentRoleId';
SET @preparedStatement = (SELECT IF(
  (
    SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
    WHERE
      TABLE_SCHEMA = @dbname
      AND TABLE_NAME = @tablename
      AND COLUMN_NAME = @columnname
  ) > 0,
  'SELECT 1',
  'ALTER TABLE `tbl_roles` ADD COLUMN `ParentRoleId` INT NULL, ADD COLUMN `HierarchyLevel` INT NOT NULL DEFAULT 1, ADD COLUMN `HierarchyOrder` INT NOT NULL DEFAULT 1, ADD CONSTRAINT `FK_tbl_roles_ParentRole` FOREIGN KEY (`ParentRoleId`) REFERENCES `tbl_roles`(`Id`) ON DELETE RESTRICT;'
));
PREPARE alterIfNotExists FROM @preparedStatement;
EXECUTE alterIfNotExists;
DEALLOCATE PREPARE alterIfNotExists;

-- Update role name from SystemAdmin to SuperAdmin
UPDATE `tbl_roles` SET `Name` = 'SuperAdmin' WHERE `Name` = 'SystemAdmin';


-- 3. ONE-TIME DATA SEEDING / MIGRATION QUERIES
-- ====================================================================================

-- Seed baseline initial records for existing item price records into tbl_item_price_audits
INSERT INTO `tbl_item_price_audits` (
  `ItemsId`, `ItemsSubId`, `ItemUuid`, `BuyingPrice`, `MarkedPrice`, `RetailPrice`, `WholesalePrice`, `RetailDiscountRatio`, `WholesaleDiscountRatio`, `ChangedAt`, `ChangedBy`, `ChangeType`
)
SELECT 
  ip.`ItemsId`, ip.`ItemsSubId`, ip.`ItemUuid`, ip.`BuyingPrice`, ip.`MarkedPrice`, ip.`RetailPrice`, ip.`WholesalePrice`, ip.`RetailDiscountRatio`, ip.`WholesaleDiscountRatio`, NOW(), 'SYSTEM', 'INSERT'
FROM `tbl_item_prices` ip
WHERE NOT EXISTS (
  SELECT 1 FROM `tbl_item_price_audits` a WHERE a.`ItemUuid` = ip.`ItemUuid`
);

-- 4. Delete not needed permissions
-- (SQL template and Report template)
-- ===================================

DELETE FROM `tbl_permissions` WHERE `Id` IN (800, 801, 802, 803, 804, 805, 850, 851, 852, 853, 903, 904);

-- 5. Update permission 1000 enum name & description
-- ================================================
UPDATE `tbl_permissions` SET `PermissionType` = 'PERMISSION_SUPER_ADMIN_VIEW', `Description` = 'Can view the SuperAdmin role existence/details' WHERE `Id` = 1000;

