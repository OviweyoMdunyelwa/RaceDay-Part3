/* =========================================================
   RACEDAY DATABASE - PART 2 ONLY
   Database: RaceDay_PoE_Final
   Portfolio of Evidence
   
   PART 2: Verification & Inspection Queries
   ========================================================= */

USE RaceDay_PoE_Final;
GO


/* ---------------------------------------------------------
   Query 1: Verify RaceDay database tables
   --------------------------------------------------------- */
SELECT
    TABLE_SCHEMA,
    TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;
GO


/* ---------------------------------------------------------
   Query 2: Inspect the RaceDay database schema
   --------------------------------------------------------- */
SELECT
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME IN (
    'RaceDayUsers',
    'Events',
    'Categories',
    'Enrolments',
    'Results',
    'Routes',
    'Weather'
)
ORDER BY TABLE_NAME, ORDINAL_POSITION;
GO


/* ---------------------------------------------------------
   Query 3: Count records in each RaceDay table
   --------------------------------------------------------- */
SELECT 'RaceDayUsers' AS TableName, COUNT(*) AS TotalRecords FROM dbo.RaceDayUsers
UNION ALL
SELECT 'Events', COUNT(*) FROM dbo.Events
UNION ALL
SELECT 'Categories', COUNT(*) FROM dbo.Categories
UNION ALL
SELECT 'Enrolments', COUNT(*) FROM dbo.Enrolments
UNION ALL
SELECT 'Results', COUNT(*) FROM dbo.Results
UNION ALL
SELECT 'Routes', COUNT(*) FROM dbo.Routes
UNION ALL
SELECT 'Weather', COUNT(*) FROM dbo.Weather
ORDER BY TableName;
GO


/* ---------------------------------------------------------
   Query 4: Check for duplicate user emails
   --------------------------------------------------------- */
SELECT
    Email,
    COUNT(*) AS Occurrences
FROM dbo.RaceDayUsers
GROUP BY Email
HAVING COUNT(*) > 1;
GO


/* ---------------------------------------------------------
   Query 5: Inspect columns in the Enrolments table
   --------------------------------------------------------- */
SELECT
    COLUMN_NAME,
    DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Enrolments'
ORDER BY ORDINAL_POSITION;
GO


/* ---------------------------------------------------------
   Query 6: View categories and their events
   --------------------------------------------------------- */
SELECT
    c.*,
    e.*
FROM dbo.Categories AS c
INNER JOIN dbo.Events AS e
    ON c.EventID = e.EventID
ORDER BY e.EventID;
GO


/* ---------------------------------------------------------
   Query 7: View recorded race results
   --------------------------------------------------------- */
SELECT *
FROM dbo.Results;
GO


/* ---------------------------------------------------------
   Query 8: View RaceDay enrolments
   --------------------------------------------------------- */
SELECT *
FROM dbo.Enrolments;
GO
