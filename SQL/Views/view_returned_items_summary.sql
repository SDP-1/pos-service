-- View: view_returned_items_summary
-- Description: Summarizes returned items against original order items.

USE `pos-system`;

DROP VIEW IF EXISTS `view_returned_items_summary`;

CREATE OR REPLACE VIEW `view_returned_items_summary` AS
    SELECT 
        `o`.`Id` AS `OrderId`,
        `o`.`OrderNumber` AS `OrderNumber`,
        `o`.`Uuid` AS `OrderUuid`,
        `original_item`.`PrintName` AS `PrintName`,
        `original_item`.`Uuid` AS `ReturnedOrderItemUuid`,
        `original_item`.`Quantity` AS `OriginalPurchasedQty`,
        SUM(`return_item`.`Quantity`) AS `TotalReturnedQty`,
        (`original_item`.`Quantity` - SUM(`return_item`.`Quantity`)) AS `RemainingQty`,
        `original_item`.`PriceAtSale` AS `PriceAtSale`,
        CAST((SUM(`return_item`.`Quantity`) * `original_item`.`PriceAtSale`)
            AS DECIMAL (18 , 2 )) AS `TotalRefundAmountValue`
    FROM
        ((`tbl_order_items` `return_item`
        JOIN `tbl_order_items` `original_item` ON ((`return_item`.`ReturnedOrderItemUuid` = `original_item`.`Uuid`)))
        JOIN `tbl_orders` `o` ON ((`original_item`.`OrderId` = `o`.`Id`)))
    WHERE
        (`return_item`.`IsReturnItem` = 1)
    GROUP BY `o`.`Id` , `o`.`OrderNumber` , `o`.`Uuid` , `original_item`.`Uuid` , `original_item`.`PrintName` , `original_item`.`Quantity` , `original_item`.`PriceAtSale`;

-- Applied on prod