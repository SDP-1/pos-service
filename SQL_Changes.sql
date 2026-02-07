
-- User table

ALTER TABLE `pos-system`.`users` 
CHANGE COLUMN `ProfileImageUrl` `ProfileImage` LONGTEXT CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NULL DEFAULT NULL ;

ALTER TABLE `pos-system`.`users` 
CHANGE COLUMN `NIC` `NIC` VARCHAR(12) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_0900_ai_ci' NULL DEFAULT NULL ;

-- order table
-- fix : remove system user , he created orders are going to remove

ALTER TABLE `pos-system`.`orders` 
DROP FOREIGN KEY `FK_Orders_Users_CashierId`;
ALTER TABLE `pos-system`.`orders` 
CHANGE COLUMN `CashierId` `CashierId` INT NULL ;
ALTER TABLE `pos-system`.`orders` 
ADD CONSTRAINT `FK_Orders_Users_CashierId`
  FOREIGN KEY (`CashierId`)
  REFERENCES `pos-system`.`users` (`Id`)
  ON DELETE SET NULL;

  -- permission table
  -- chnaege permission  name and description

  UPDATE `pos-system`.`permissions` SET `PermissionType` = 'USER_ACTIVE_STATUS_CHANGE', `Description` = 'Can change user active status users' WHERE (`Id` = '303');

