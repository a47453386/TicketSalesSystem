Select * 
from TicketsArea with (UPDLOCK,ROWLOCK) 
where SessionID  and TicketsAreaID


INSERT INTO __EFMigrationsHistory
(MigrationId, ProductVersion)
VALUES
('20260202123515_InitialCreate', '9.0.12');


Select *
from [Order]
Where OrderID like '20260210%'


Select *
from Tickets
Where OrderID like '20260210%'

SELECT CHARACTER_MAXIMUM_LENGTH 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'DescriptionImage' AND COLUMN_NAME = 'DescriptionImageName';


SELECT TOP 10
    dest.text AS [SQL Statement],
    deqs.last_execution_time AS [執行時間],
    deqs.execution_count AS [執行次數]
FROM sys.dm_exec_query_stats AS deqs
CROSS APPLY sys.dm_exec_sql_text(deqs.sql_handle) AS dest
WHERE dest.text LIKE '%DELETE%' -- 關鍵字過濾
ORDER BY deqs.last_execution_time DESC;

SELECT name, parent_class_desc, create_date 
FROM sys.triggers 
WHERE is_disabled = 0;

SELECT 
    t.name AS [資料表名稱],
    tr.name AS [觸發器名稱],
    tr.is_disabled AS [是否停用],
    OBJECT_DEFINITION(tr.object_id) AS [觸發器內容]
FROM sys.triggers tr
JOIN sys.tables t ON tr.parent_id = t.object_id
WHERE t.name = 'Order' OR t.name = 'Tickets';