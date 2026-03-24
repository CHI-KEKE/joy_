```sql


-- WebStoreDB
use WebStoreDB
DROP TABLE IF EXISTS #StateCityZipCodeSummary;

SELECT
    AdministrativeRegion_State,
    AdministrativeRegion_City,
    MIN(AdministrativeRegion_Postcode) AS DefaultZipCode,
    COUNT(*) AS ZipCodeCount
INTO #StateCityZipCodeSummary
FROM dbo.AdministrativeRegion
WHERE AdministrativeRegion_ShippingAreaAliasCode = 'malaysia'
GROUP BY
    AdministrativeRegion_State,
    AdministrativeRegion_City;

select *
from #StateCityZipCodeSummary



--把 Area + City join 起來（InfoDB 內部）
-- InfoDB
DROP TABLE IF EXISTS #AreaWithCity;

SELECT
    a.Area_Id,
    a.Area_Title,
    a.Area_Zipcode,
    a.Area_CityId,
    c.City_Title
INTO #AreaWithCity
FROM InfoDB.dbo.Area a
JOIN InfoDB.dbo.City c
    ON a.Area_CityId = c.City_Id
WHERE
    a.Area_Zipcode IS NULL;  -- 只處理還沒補的

select *
from #AreaWithCity


--再去對 AdministrativeRegion（⚠️ 關鍵 JOIN）
DROP TABLE IF EXISTS #AreaZipCodeCandidate;

SELECT
    awc.Area_Id,
    awc.Area_Title,
    awc.City_Title,
    s.AdministrativeRegion_City,
    s.DefaultZipCode,
    s.ZipCodeCount
INTO #AreaZipCodeCandidate
FROM #AreaWithCity awc
JOIN #StateCityZipCodeSummary s
    ON awc.City_Title COLLATE SQL_Latin1_General_CP1_CI_AS
     = s.AdministrativeRegion_State COLLATE SQL_Latin1_General_CP1_CI_AS
    AND awc.Area_Title COLLATE SQL_Latin1_General_CP1_CI_AS
     = s.AdministrativeRegion_City  COLLATE SQL_Latin1_General_CP1_CI_AS;

select *
from #AreaZipCodeCandidate

--use InfoDB
--select *
--from Area(nolock)
--where Area_ValidFlag = 1
--and Area_Id not in (
--select Area_Id
--from #AreaZipCodeCandidate
--)

--use WebStoreDB
--select AdministrativeRegion_State,AdministrativeRegion_City,*
--from AdministrativeRegion(nolock)
--where AdministrativeRegion_ValidFlag = 1
--and AdministrativeRegion_City = 'Ayer Baloi'

--use InfoDB
--select *
--from City(nolock)
--where City_ValidFlag = 1
--and City_Id = 23



--處理「一個 Area 對多個 ZipCode」👉 去重 同一個 Area_Id，只取 ZipCodeCount 最大的那一筆
DROP TABLE IF EXISTS #AreaZipCodeToUpdate;

WITH RankedZipCode AS (
    SELECT
        Area_Id,
        DefaultZipCode AS NewZipCode,
        ZipCodeCount,
        ROW_NUMBER() OVER (
            PARTITION BY Area_Id
            ORDER BY ZipCodeCount DESC, DefaultZipCode
        ) AS rn
    FROM #AreaZipCodeCandidate
)
SELECT
    Area_Id,
    NewZipCode,
    ZipCodeCount
INTO #AreaZipCodeToUpdate
FROM RankedZipCode
WHERE rn = 1;

SELECT Area_Id, COUNT(*)
FROM #AreaZipCodeToUpdate
GROUP BY Area_Id
HAVING COUNT(*) > 1;

--進 staging（正式更新前的保險）
-- InfoDB
DROP TABLE IF EXISTS dbo.AreaZipCode_Staging;

CREATE TABLE dbo.AreaZipCode_Staging
(
    Area_Id BIGINT PRIMARY KEY,
    NewZipCode NVARCHAR(20),
    ZipCodeCount INT,
    Source NVARCHAR(50),
    CreatedDateTime DATETIME2 DEFAULT SYSDATETIME()
);

INSERT INTO dbo.AreaZipCode_Staging
(
    Area_Id,
    NewZipCode,
    ZipCodeCount,
    Source
)
SELECT
    Area_Id,
    NewZipCode,
    ZipCodeCount,
    'WebStoreDB.AdministrativeRegion(State+City)'
FROM #AreaZipCodeToUpdate;


--UPDATE（一定包 transaction）
BEGIN TRAN;

UPDATE Area
SET Area_Zipcode = NewZipCode
FROM InfoDB.dbo.Area
JOIN dbo.AreaZipCode_Staging
    ON Area.Area_Id = AreaZipCode_Staging.Area_Id
WHERE Area.Area_Zipcode IS NULL;
SELECT @@ROWCOUNT AS UpdatedCount;

-- 確認 OK
COMMIT;
-- 有問題
-- ROLLBACK;


use InfoDB
select Area_Zipcode,*
from Area(nolock)
```