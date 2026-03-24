



```sql
USE NMQV2DB
GO

---- 請協助加到各環境的人，根據環境需求調整，下列兩個參數
DECLARE @Region VARCHAR(2) = 'MY', -- TW/HK/PX/MY
@Env VARCHAR(4) = 'Prod' -- QA/Prod

---- 建立 Job的人，請填寫下方參數
DECLARE @JobName VARCHAR(50) = 'PayNowRazerRefundRequestFinish',
 @JobDesc NVARCHAR(500) = N'Payment Middleware 退款完成',
 @ClassName VARCHAR(200) = 'NineYi.SCM.Frontend.NMQV2.ThirdPartyPayments.PaymentMiddleWareRefundRequestFinishProcess',
 @User VARCHAR(50) = 'VSTS569269',
 @JobGroupId INT = 1; -- TW Prod請確實根據需求設定 JobGroup位置，但其他環境目前會強制設定為 1

---- 這些參數不用管
DECLARE @Today DATETIME = GETDATE(),
 @JobId BIGINT,
 @AssemblyRoot VARCHAR(500),
 @AssemblyPath VARCHAR(500),
 @AssemblyName VARCHAR(500);

IF LEFT(UPPER(@ClassName), 19) = 'NINEYI.SCM.FRONTEND'
BEGIN
    SET @AssemblyName = 'NineYi.SCM.Frontend.NMQV2'
END
ELSE IF LEFT(UPPER(@ClassName), 19) = 'NINEYI.ERP.BACKEND.'
BEGIN
    SET @AssemblyName = 'NineYi.ERP.Backend.NMQV2'
END

IF @Env = 'Prod' 
BEGIN
    IF @Region = 'PX' 
    BEGIN
        SET @AssemblyRoot = 'D:\Prod\NineYi';

        IF @AssemblyName = 'NineYi.SCM.Frontend.NMQV2'
        BEGIN      
            SET @AssemblyPath = 'SCM.NMQV2'
        END
        ELSE IF @AssemblyName = 'NineYi.ERP.Backend.NMQV2'
        BEGIN      
            SET @AssemblyPath = 'ERP.NMQV2'
        END
    END
    ELSE IF @Region = 'HK' OR @Region = 'MY'
    BEGIN
        SET @AssemblyRoot = 'D:\Prod\NineYi';

        IF @AssemblyName = 'NineYi.SCM.Frontend.NMQV2'
        BEGIN      
            SET @AssemblyPath = 'SCM.NMQV2'
        END
        ELSE IF @AssemblyName = 'NineYi.ERP.Backend.NMQV2'
        BEGIN      
            SET @AssemblyPath = 'ERP.NMQV2'
        END
    END
    ELSE IF @Region = 'TW'
    BEGIN
        SET @AssemblyRoot = 'D:\Prod\NineYi\NMQV2\Library';

        IF @AssemblyName = 'NineYi.SCM.Frontend.NMQV2'
        BEGIN      
            SET @AssemblyPath = 'NineYi.SCM.Frontend'
        END
        ELSE IF @AssemblyName = 'NineYi.ERP.Backend.NMQV2'
        BEGIN      
            SET @AssemblyPath = 'NineYi.ERP.Backend'
        END
    END
END
ELSE IF @Env = 'QA' 
BEGIN
    IF @Region = 'PX' 
    BEGIN
        SET @AssemblyRoot = 'D:\QA\NineYi';

        IF @AssemblyName = 'NineYi.SCM.Frontend.NMQV2'
        BEGIN      
            SET @AssemblyPath = 'SCM.NMQV2'
        END
        ELSE IF @AssemblyName = 'NineYi.ERP.Backend.NMQV2'
        BEGIN      
            SET @AssemblyPath = 'ERP.NMQV2'
        END
    END
    ELSE IF @Region = 'HK' OR @Region = 'MY'
    BEGIN
        SET @AssemblyRoot = 'D:\QA\NineYi\NMQV2\Library';

        IF @AssemblyName = 'NineYi.SCM.Frontend.NMQV2'
        BEGIN      
            SET @AssemblyPath = 'NineYi.SCM.Frontend'
        END
        ELSE IF @AssemblyName = 'NineYi.ERP.Backend.NMQV2'
        BEGIN      
            SET @AssemblyPath = 'NineYi.ERP.Backend'
        END
    END
    ELSE IF @Region = 'TW'
    BEGIN
        SET @AssemblyRoot = 'D:\QA\NineYi';
       
        IF @AssemblyName = 'NineYi.SCM.Frontend.NMQV2'
        BEGIN      
            SET @AssemblyPath = 'SCM.NMQV2'
        END
        ELSE IF @AssemblyName = 'NineYi.ERP.Backend.NMQV2'
        BEGIN      
            SET @AssemblyPath = 'ERP.NMQV2'
        END
    END
END

IF @Region = 'TW' AND @Env = 'Prod'
BEGIN
    SET @JobGroupId = @JobGroupId;
END
ELSE
BEGIN
	SET @JobGroupId = 1;
END

INSERT INTO [dbo].[Job] ([Job_Name]
, [Job_Description]
, [Job_ClassName]
, [Job_AssemblyRoot]
, [Job_AssemblyPath]
, [Job_AssemblyName]
, [Job_RetryCount]
, [Job_Priority]
, [Job_Enable]
, [Job_Timeout]
, [Job_CreatedDateTime]
, [Job_CreatedUser]
, [Job_UpdatedTimes]
, [Job_UpdatedDateTime]
, [Job_UpdatedUser]
, [Job_ValidFlag]
, [Job_ErrorDelay]
, [Job_PendingDelay])
VALUES (@JobName, @JobDesc, @ClassName, @AssemblyRoot, @AssemblyPath, @AssemblyName, 0, 3, 1, 1200, @Today, @User, 0, @Today, @User, 1, 0, 0);

SELECT @JobId = Job_Id
FROM dbo.Job WITH (NOLOCK)
WHERE Job_Name = @JobName;

INSERT INTO [dbo].[JobGroupMapping] ([JobGroupMapping_JobId]
, [JobGroupMapping_JobGroupId]
, [JobGroupMapping_CreatedUser]
, [JobGroupMapping_CreatedDateTime]
, [JobGroupMapping_UpdatedTimes]
, [JobGroupMapping_UpdatedDateTime]
, [JobGroupMapping_UpdatedUser]
, [JobGroupMapping_ValidFlag])
VALUES (@JobId, @JobGroupId, @User, @Today, 0, @Today, @User, 1);


-- NMQv2 轉 NMQv3
INSERT INTO NMQV2DB.dbo.JobSwitch
(
    JobSwitch_JobId, JobSwitch_Priority, JobSwitch_ValidFlag, JobSwitch_CreatedDateTime, JobSwitch_CreatedUser,
    JobSwitch_UpdatedDateTime, JobSwitch_UpdatedUser, JobSwitch_UpdatedTimes
)
VALUES
(
    @JobId, 2, 1, @Today, @User,
    @Today, @User, 0
);

MERGE NMQV2DB.dbo.JobGroupMapping AS TARGET
USING (SELECT @JobId AS Job_Id) AS SOURCE
    ON JobGroupMapping_JobId = Job_Id
    AND JobGroupMapping_ValidFlag = 1
WHEN MATCHED THEN UPDATE SET
    JobGroupMapping_ValidFlag = 0,
    JobGroupMapping_UpdatedUser = @User,
    JobGroupMapping_UpdatedDateTime = @Today,
    JobGroupMapping_UpdatedTimes = JobGroupMapping_UpdatedTimes % 255 + 1;
```