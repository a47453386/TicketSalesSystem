create function funGetProgrammeID ()
	returns nchar(8)
	as
	begin
	 declare @DatePrefix nchar(6),@LastProID int,@NewProgrammeID nchar(8)

	 set @DatePrefix =convert(nvarchar(6),getdate(),112)

	 select @LastProID= isnull (max(cast(right(ProgrammeID,2) as int)),0) 
	 from Programme
	 where left(ProgrammeID,6)=@DatePrefix

	 set @LastProID+=1

	 set @NewProgrammeID=cast(@DatePrefix as nchar(6))+RIGHT('00'+ CAST(@LastProID as varchar(2)),2)

	 return @NewProgrammeID
	 end
	 go

	 print dbo.funGetProgrammeID();
	 go
--------------------------------------------------------------------------------------------------------
	create PROCEDURE CreateProgramme
            @Name nvarchar(50),      
            @Desc nvarchar(max),     
            @Limit int,             
            @ShelfTime datetime.... 
            @EmpID nchar(8),        
            @PlaceID nchar(8),      
            @StatusID nchar(8)      
                          
        AS
        BEGIN
            SET NOCOUNT ON;
            DECLARE @NewID nchar(8);
    
            -- 呼叫你寫好的自動編號函數
            SET @NewID = dbo.funGetProgrammeID(); 

            INSERT INTO Programme (
                ProgrammeID, ProgrammeName, ProgrammeDescription,CreatedTime, UpdatedAt,CoverImage,SeatImage,
                 LimitPerOrder,OnShelfTime,EmployeeID, PlaceID, ProgrammeStatusID
            )
            VALUES (
                @NewID, @Name, @Desc,GETDATE(),GETDATE(), '','',
                @Limit,@ShelfTime,@EmpID, @PlaceID, @StatusID
                );

            -- 回傳結果供 VS 接收
            SELECT * FROM Programme WHERE ProgrammeID = @NewID;
        END
go

 






ALTER TABLE Programme ALTER COLUMN CoverImage nvarchar(13) NULL;
ALTER TABLE Programme ALTER COLUMN SeatImage nvarchar(13) NULL;

