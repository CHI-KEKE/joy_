

```sql
SELECT TOP (1000) [AppSetting_Id]
      ,[AppSetting_Key]
      ,[AppSetting_Value]
      ,[AppSetting_CreatedDateTime]
      ,[AppSetting_CreatedUser]
      ,[AppSetting_UpdatedTimes]
      ,[AppSetting_UpdatedDateTime]
      ,[AppSetting_UpdatedUser]
      ,[AppSetting_ValidFlag]
      ,[AppSetting_Rowversion]
      ,[AppSetting_Description]
  FROM [ConfigDB].[dbo].[AppSetting]
  where AppSetting_Key = 'CSP.Payment.PayTypes'
```

```sql
USE ConfigDB


DECLARE @id int,
		@now DATETIME = GETDATE(),
		@user VARCHAR(20) = 'VSTS569269';

SELECT @id = AppSetting_Id
FROM dbo.AppSetting WITH (NOLOCK)
WHERE AppSetting_ValidFlag = 1 AND AppSetting_Key = 'CSP.Payment.PayTypes';

-- SELECT
SELECT AppSetting_Value
FROM dbo.AppSetting WITH(NOLOCK)
WHERE AppSetting_Id = @id;

-- BACKUP
SELECT *
INTO MATempDB.dbo.tmpConfigDB_AppSetting_VSTS573550_1
FROM dbo.AppSetting WITH(NOLOCK)
WHERE AppSetting_Id = @id;

-- UPDATE 
UPDATE dbo.AppSetting
SET AppSetting_Value = AppSetting_Value + N',PayNow_Razer',
    AppSetting_UpdatedTimes = AppSetting_UpdatedTimes % 255 + 1,
    AppSetting_UpdatedDateTime = @now,
    AppSetting_UpdatedUser = @user
WHERE AppSetting_Id = @id;

-- VERIFY
SELECT AppSetting_Value
FROM dbo.AppSetting WITH(NOLOCK)
WHERE AppSetting_Id = @id;
```