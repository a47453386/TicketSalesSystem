Select * 
from TicketsArea with (UPDLOCK,ROWLOCK) 
where SessionID  and TicketsAreaID


INSERT INTO __EFMigrationsHistory
(MigrationId, ProductVersion)
VALUES
('20260202123515_InitialCreate', '9.0.12');


Select *
from [Order]
Where OrderID like '20260204%'


Select *
from Tickets
Where OrderID like '20260204%'

SELECT CHARACTER_MAXIMUM_LENGTH 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'DescriptionImage' AND COLUMN_NAME = 'DescriptionImageName';