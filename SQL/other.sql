Select * 
from TicketsArea with (UPDLOCK,ROWLOCK) 
where SessionID  and TicketsAreaID