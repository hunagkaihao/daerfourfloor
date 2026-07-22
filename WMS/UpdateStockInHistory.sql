-- 更新StockInHistory表结构，使其与StockOutHistory保持一致

-- 创建临时存储过程来安全删除列
DELIMITER //
CREATE PROCEDURE DropColumnIfExists(IN tableName VARCHAR(64), IN columnName VARCHAR(64))
BEGIN
    DECLARE columnExists INT;
    SELECT COUNT(*) INTO columnExists 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() 
      AND TABLE_NAME = tableName 
      AND COLUMN_NAME = columnName;
    
    IF columnExists > 0 THEN
        SET @sql = CONCAT('ALTER TABLE ', tableName, ' DROP COLUMN ', columnName);
        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;
END //

-- 创建临时存储过程来安全添加列
CREATE PROCEDURE AddColumnIfNotExists(IN tableName VARCHAR(64), IN columnName VARCHAR(64), IN columnType VARCHAR(128))
BEGIN
    DECLARE columnExists INT;
    SELECT COUNT(*) INTO columnExists 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = DATABASE() 
      AND TABLE_NAME = tableName 
      AND COLUMN_NAME = columnName;
    
    IF columnExists = 0 THEN
        SET @sql = CONCAT('ALTER TABLE ', tableName, ' ADD COLUMN ', columnName, ' ', columnType);
        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;
END //
DELIMITER ;

-- 1. 删除旧的ValueObject相关列
CALL DropColumnIfExists('StockInHistories', 'CheckData_CheckDate');
CALL DropColumnIfExists('StockInHistories', 'CheckData_CheckNo');
CALL DropColumnIfExists('StockInHistories', 'CheckData_CheckOrderCode');
CALL DropColumnIfExists('StockInHistories', 'CheckData_CheckResult');
CALL DropColumnIfExists('StockInHistories', 'Material_Code');
CALL DropColumnIfExists('StockInHistories', 'Material_Name');
CALL DropColumnIfExists('StockInHistories', 'Material_Specs');
CALL DropColumnIfExists('StockInHistories', 'Material_Unit');
CALL DropColumnIfExists('StockInHistories', 'StockPlace_AreaCode');
CALL DropColumnIfExists('StockInHistories', 'StockPlace_AreaName');
CALL DropColumnIfExists('StockInHistories', 'StockPlace_BoxCode');
CALL DropColumnIfExists('StockInHistories', 'StockPlace_BoxName');
CALL DropColumnIfExists('StockInHistories', 'StockPlace_CellCode');
CALL DropColumnIfExists('StockInHistories', 'StockPlace_CellName');
CALL DropColumnIfExists('StockInHistories', 'StockPlace_HouseCode');
CALL DropColumnIfExists('StockInHistories', 'StockPlace_HouseName');
CALL DropColumnIfExists('StockInHistories', 'Supplier_Code');
CALL DropColumnIfExists('StockInHistories', 'Supplier_Name');

-- 2. 添加新的直接属性列
CALL AddColumnIfNotExists('StockInHistories', 'AreaCode', 'VARCHAR(20)');
CALL AddColumnIfNotExists('StockInHistories', 'AreaName', 'VARCHAR(50)');
CALL AddColumnIfNotExists('StockInHistories', 'BatchNo', 'VARCHAR(30)');
CALL AddColumnIfNotExists('StockInHistories', 'BoxCode', 'VARCHAR(20)');
CALL AddColumnIfNotExists('StockInHistories', 'BoxName', 'VARCHAR(50)');
CALL AddColumnIfNotExists('StockInHistories', 'CellCode', 'VARCHAR(20)');
CALL AddColumnIfNotExists('StockInHistories', 'CellName', 'VARCHAR(50)');
CALL AddColumnIfNotExists('StockInHistories', 'MaterialCode', 'VARCHAR(20)');
CALL AddColumnIfNotExists('StockInHistories', 'MaterialName', 'VARCHAR(120)');
CALL AddColumnIfNotExists('StockInHistories', 'MaterialSpecs', 'VARCHAR(120)');
CALL AddColumnIfNotExists('StockInHistories', 'MaterialUnit', 'VARCHAR(10)');
CALL AddColumnIfNotExists('StockInHistories', 'WarehouseCode', 'VARCHAR(20)');
CALL AddColumnIfNotExists('StockInHistories', 'WarehouseName', 'VARCHAR(50)');

-- 删除临时存储过程
DROP PROCEDURE IF EXISTS DropColumnIfExists;
DROP PROCEDURE IF EXISTS AddColumnIfNotExists;

-- 3. 更新迁移历史表（如果存在）
INSERT IGNORE INTO __EFMigrationsHistory (MigrationId, ProductVersion) 
VALUES ('20260520100000_SimplifyStockInHistory', '8.0.6');

SELECT 'StockInHistory表结构更新完成' AS Result;