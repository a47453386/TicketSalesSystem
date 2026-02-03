alter function funGetOrderID ()
	returns nvarchar(14)
	as
	begin
	 declare @DatePrefix nchar(8),@LastOID int,@NewOrderID nchar(14)

	 set @DatePrefix =convert(nvarchar(8),getdate(),112)

	 select @LastOID= isnull (max(cast(right(OrderID,6) as int)),0) 
	 from [Order]
	 where left(OrderID,8)=@DatePrefix

	 set @LastOID+=1

	 set @NewOrderID=cast(@DatePrefix as nchar(8))+RIGHT('000000'+ CAST(@LastOID as varchar(6)),6)

	 return @NewOrderID
	 end
	 go

	 SELECT dbo.funGetOrderID();



-- 第一步：先刪除所有關聯到這筆訂單的票券
DELETE FROM Tickets WHERE OrderID like '20260203%'; 
go
-- 第二步：再刪除訂單本身
DELETE FROM [Order] WHERE OrderID like '20260203%';
go