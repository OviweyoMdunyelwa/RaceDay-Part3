/* =========================================================
   RACEDAY DATABASE - PART 1 ONLY
   Database: RaceDay_PoE_Final
   Portfolio of Evidence
   
   PART 1: Database Creation, Tables, Constraints, 
           Relationships & Sample Data
   ========================================================= */

/* ---------------------------------------------------------
   1. DROP AND CREATE DATABASE
   --------------------------------------------------------- */
IF DB_ID('RaceDay_PoE_Final') IS NOT NULL
BEGIN
    ALTER DATABASE RaceDay_PoE_Final
    SET SINGLE_USER
    WITH ROLLBACK IMMEDIATE;

    DROP DATABASE RaceDay_PoE_Final;
END;
GO

CREATE DATABASE RaceDay_PoE_Final;
GO

USE RaceDay_PoE_Final;
GO


/* ---------------------------------------------------------
   2. CREATE TABLE: RaceDayUsers
   --------------------------------------------------------- */
CREATE TABLE dbo.RaceDayUsers
(
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Role VARCHAR(20) NOT NULL,
    PhoneNumber VARCHAR(20) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT CK_RaceDayUsers_Role
        CHECK (Role IN ('Organiser', 'Participant'))
);
GO


/* ---------------------------------------------------------
   3. CREATE TABLE: Events
   --------------------------------------------------------- */
CREATE TABLE dbo.Events
(
    EventID INT IDENTITY(1,1) PRIMARY KEY,
    OrganiserID INT NOT NULL,
    EventName VARCHAR(100) NOT NULL,
    Description VARCHAR(500) NULL,
    EventDate DATETIME NOT NULL,
    Location VARCHAR(150) NOT NULL,
    RegistrationDeadline DATETIME NOT NULL,
    Status VARCHAR(20) NOT NULL DEFAULT 'Upcoming',
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Events_Organiser
        FOREIGN KEY (OrganiserID)
        REFERENCES dbo.RaceDayUsers(UserID),

    CONSTRAINT CK_Events_Status
        CHECK (Status IN ('Upcoming', 'Open', 'Closed', 'Completed', 'Cancelled'))
);
GO


/* ---------------------------------------------------------
   4. CREATE TABLE: Categories
   --------------------------------------------------------- */
CREATE TABLE dbo.Categories
(
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    EventID INT NOT NULL,
    CategoryName VARCHAR(100) NOT NULL,
    DistanceKM DECIMAL(5,2) NOT NULL,
    EntryFee DECIMAL(10,2) NOT NULL,
    AgeGroup VARCHAR(50) NULL,
    Description VARCHAR(300) NULL,

    CONSTRAINT FK_Categories_Event
        FOREIGN KEY (EventID)
        REFERENCES dbo.Events(EventID),

    CONSTRAINT CK_Categories_Distance
        CHECK (DistanceKM > 0),

    CONSTRAINT CK_Categories_EntryFee
        CHECK (EntryFee >= 0),

    CONSTRAINT UQ_Categories_Event_Name
        UNIQUE (EventID, CategoryName)
);
GO


/* ---------------------------------------------------------
   5. CREATE TABLE: Enrolments
   --------------------------------------------------------- */
CREATE TABLE dbo.Enrolments
(
    EnrolmentID INT IDENTITY(1,1) PRIMARY KEY,
    EventID INT NOT NULL,
    CategoryID INT NOT NULL,
    ParticipantID INT NOT NULL,
    EnrolmentDate DATETIME NOT NULL DEFAULT GETDATE(),
    RaceNumber VARCHAR(20) NOT NULL,
    PaymentStatus VARCHAR(20) NOT NULL DEFAULT 'Pending',

    CONSTRAINT CK_Enrolments_PaymentStatus
        CHECK (PaymentStatus IN ('Pending', 'Paid', 'Refunded', 'Cancelled')),

    CONSTRAINT UQ_Enrolments_RaceNumber
        UNIQUE (EventID, RaceNumber)
);
GO


/* ---------------------------------------------------------
   6. CREATE TABLE: Results
   --------------------------------------------------------- */
CREATE TABLE dbo.Results
(
    ResultID INT IDENTITY(1,1) PRIMARY KEY,
    EnrolmentID INT NOT NULL,
    FinishTime TIME NULL,
    ChipTime TIME NULL,
    PositionOverall INT NULL,
    PositionCategory INT NULL,
    Pace DECIMAL(5,2) NULL,
    Status VARCHAR(20) NOT NULL DEFAULT 'Finished',

    CONSTRAINT UQ_Results_Enrolment
        UNIQUE (EnrolmentID),

    CONSTRAINT CK_Results_PositionOverall
        CHECK (PositionOverall IS NULL OR PositionOverall > 0),

    CONSTRAINT CK_Results_PositionCategory
        CHECK (PositionCategory IS NULL OR PositionCategory > 0),

    CONSTRAINT CK_Results_Pace
        CHECK (Pace IS NULL OR Pace > 0),

    CONSTRAINT CK_Results_Status
        CHECK (Status IN ('Finished', 'DNF', 'DNS', 'Disqualified'))
);
GO


/* ---------------------------------------------------------
   7. CREATE TABLE: Routes
   --------------------------------------------------------- */
CREATE TABLE dbo.Routes
(
    RouteID INT IDENTITY(1,1) PRIMARY KEY,
    EventID INT NOT NULL,
    RouteName VARCHAR(100) NOT NULL,
    DistanceKM DECIMAL(5,2) NOT NULL,
    ElevationGain DECIMAL(7,2) NOT NULL,
    Description VARCHAR(500) NULL,
    MapURL VARCHAR(500) NULL,

    CONSTRAINT CK_Routes_Distance
        CHECK (DistanceKM > 0),

    CONSTRAINT CK_Routes_Elevation
        CHECK (ElevationGain >= 0)
);
GO


/* ---------------------------------------------------------
   8. CREATE TABLE: Weather
   --------------------------------------------------------- */
CREATE TABLE dbo.Weather
(
    WeatherID INT IDENTITY(1,1) PRIMARY KEY,
    EventID INT NOT NULL,
    ForecastDate DATE NOT NULL,
    Temperature DECIMAL(5,2) NOT NULL,
    Conditions VARCHAR(100) NOT NULL,
    WindSpeed DECIMAL(5,2) NOT NULL,
    Humidity DECIMAL(5,2) NOT NULL,

    CONSTRAINT CK_Weather_WindSpeed
        CHECK (WindSpeed >= 0),

    CONSTRAINT CK_Weather_Humidity
        CHECK (Humidity >= 0 AND Humidity <= 100)
);
GO


/* ---------------------------------------------------------
   9. ADD FOREIGN KEYS (Enrolments, Results, Routes, Weather)
   --------------------------------------------------------- */
ALTER TABLE dbo.Enrolments
ADD CONSTRAINT FK_Enrolments_Event
FOREIGN KEY (EventID)
REFERENCES dbo.Events(EventID);
GO

ALTER TABLE dbo.Enrolments
ADD CONSTRAINT FK_Enrolments_Category
FOREIGN KEY (CategoryID)
REFERENCES dbo.Categories(CategoryID);
GO

ALTER TABLE dbo.Enrolments
ADD CONSTRAINT FK_Enrolments_Participant
FOREIGN KEY (ParticipantID)
REFERENCES dbo.RaceDayUsers(UserID);
GO

ALTER TABLE dbo.Results
ADD CONSTRAINT FK_Results_Enrolment
FOREIGN KEY (EnrolmentID)
REFERENCES dbo.Enrolments(EnrolmentID);
GO

ALTER TABLE dbo.Routes
ADD CONSTRAINT FK_Routes_Event
FOREIGN KEY (EventID)
REFERENCES dbo.Events(EventID);
GO

ALTER TABLE dbo.Weather
ADD CONSTRAINT FK_Weather_Event
FOREIGN KEY (EventID)
REFERENCES dbo.Events(EventID);
GO


/* ---------------------------------------------------------
   10. INSERT DATA: Users
   --------------------------------------------------------- */
INSERT INTO dbo.RaceDayUsers
(
    FirstName,
    LastName,
    Email,
    PasswordHash,
    Role,
    PhoneNumber,
    CreatedAt
)
VALUES
(
    'Thabo',
    'Mokoena',
    'thabo.mokoena@raceday.co.za',
    'HASH_ORGANISER_001',
    'Organiser',
    '0825551001',
    GETDATE()
),
(
    'Lerato',
    'Naidoo',
    'lerato.naidoo@raceday.co.za',
    'HASH_ORGANISER_002',
    'Organiser',
    '0835551002',
    GETDATE()
),
(
    'Sipho',
    'Dlamini',
    'sipho.dlamini@email.co.za',
    'HASH_PARTICIPANT_001',
    'Participant',
    '0845551003',
    GETDATE()
),
(
    'Ayesha',
    'Pillay',
    'ayesha.pillay@email.co.za',
    'HASH_PARTICIPANT_002',
    'Participant',
    '0855551004',
    GETDATE()
);
GO


/* ---------------------------------------------------------
   11. INSERT DATA: Events
   --------------------------------------------------------- */
INSERT INTO dbo.Events
(
    OrganiserID,
    EventName,
    Description,
    EventDate,
    Location,
    RegistrationDeadline,
    Status,
    CreatedAt
)
VALUES
(
    1,
    'Johannesburg City Run',
    'A road running event through central Johannesburg.',
    '2026-10-10 06:00:00',
    'Johannesburg, Gauteng',
    '2026-10-01 23:59:59',
    'Open',
    GETDATE()
),
(
    2,
    'Durban Coastal Cycle',
    'A scenic cycling event along the Durban coastline.',
    '2026-11-07 06:30:00',
    'Durban, KwaZulu-Natal',
    '2026-10-28 23:59:59',
    'Open',
    GETDATE()
),
(
    1,
    'Cape Town Community Run',
    'A community road running and walking event.',
    '2026-12-05 07:00:00',
    'Cape Town, Western Cape',
    '2026-11-25 23:59:59',
    'Upcoming',
    GETDATE()
);
GO


/* ---------------------------------------------------------
   12. INSERT DATA: Categories
   --------------------------------------------------------- */
INSERT INTO dbo.Categories
(
    EventID,
    CategoryName,
    DistanceKM,
    EntryFee,
    AgeGroup,
    Description
)
VALUES
(
    1,
    '10 KM Run',
    10.00,
    180.00,
    '16+',
    'Competitive 10 kilometre road race.'
),
(
    1,
    '5 KM Fun Run',
    5.00,
    100.00,
    'All Ages',
    'Short community fun run.'
),
(
    2,
    '50 KM Cycle',
    50.00,
    350.00,
    '18+',
    'Long-distance coastal cycling event.'
),
(
    2,
    '20 KM Cycle',
    20.00,
    220.00,
    '16+',
    'Shorter cycling category.'
),
(
    3,
    '21 KM Half Marathon',
    21.10,
    250.00,
    '18+',
    'Half marathon road race.'
),
(
    3,
    '10 KM Run',
    10.00,
    150.00,
    '16+',
    'Community 10 kilometre run.'
),
(
    3,
    '5 KM Walk',
    5.00,
    80.00,
    'All Ages',
    'Non-competitive community walk.'
);
GO


/* ---------------------------------------------------------
   13. INSERT DATA: Enrolments
   --------------------------------------------------------- */
INSERT INTO dbo.Enrolments
(
    EventID,
    CategoryID,
    ParticipantID,
    EnrolmentDate,
    RaceNumber,
    PaymentStatus
)
VALUES
(
    1,
    1,
    3,
    GETDATE(),
    'JHB001',
    'Paid'
),
(
    1,
    2,
    4,
    GETDATE(),
    'JHB002',
    'Paid'
),
(
    2,
    3,
    3,
    GETDATE(),
    'DBN001',
    'Paid'
),
(
    3,
    5,
    4,
    GETDATE(),
    'CPT001',
    'Pending'
),
(
    3,
    6,
    3,
    GETDATE(),
    'CPT002',
    'Paid'
);
GO


/* ---------------------------------------------------------
   14. INSERT DATA: Results
   --------------------------------------------------------- */
INSERT INTO dbo.Results
(
    EnrolmentID,
    FinishTime,
    ChipTime,
    PositionOverall,
    PositionCategory,
    Pace,
    Status
)
VALUES
(
    1,
    '00:52:30',
    '00:52:10',
    14,
    8,
    5.22,
    'Finished'
),
(
    2,
    '00:31:45',
    '00:31:20',
    22,
    10,
    6.28,
    'Finished'
),
(
    3,
    '01:48:20',
    '01:47:55',
    9,
    6,
    2.16,
    'Finished'
);
GO


/* ---------------------------------------------------------
   15. INSERT DATA: Routes
   --------------------------------------------------------- */
INSERT INTO dbo.Routes
(
    EventID,
    RouteName,
    DistanceKM,
    ElevationGain,
    Description,
    MapURL
)
VALUES
(
    1,
    'Johannesburg City Loop',
    10.00,
    120.00,
    'Urban loop through central Johannesburg.',
    'https://maps.example.com/johannesburg-city-loop'
),
(
    2,
    'Durban Coastal Route',
    50.00,
    280.00,
    'Coastal cycling route with moderate elevation.',
    'https://maps.example.com/durban-coastal-route'
),
(
    3,
    'Cape Town Community Route',
    21.10,
    190.00,
    'Road route through selected Cape Town communities.',
    'https://maps.example.com/cape-town-community-route'
);
GO


/* ---------------------------------------------------------
   16. INSERT DATA: Weather
   --------------------------------------------------------- */
INSERT INTO dbo.Weather
(
    EventID,
    ForecastDate,
    Temperature,
    Conditions,
    WindSpeed,
    Humidity
)
VALUES
(
    1,
    '2026-10-10',
    18.50,
    'Clear',
    12.00,
    55.00
),
(
    2,
    '2026-11-07',
    21.00,
    'Partly Cloudy',
    18.00,
    62.00
),
(
    3,
    '2026-12-05',
    19.50,
    'Sunny',
    10.00,
    58.00
);
GO


/* ---------------------------------------------------------
   17. VERIFICATION QUERIES
   --------------------------------------------------------- */

-- List all tables
SELECT 
    TABLE_SCHEMA,
    TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE';
GO

-- Check Users
SELECT * FROM dbo.RaceDayUsers;
GO

-- Check Events
SELECT * FROM dbo.Events;
GO

-- Check Categories
SELECT * FROM dbo.Categories;
GO

-- Check Enrolments
SELECT * FROM dbo.Enrolments;
GO

-- Check Results
SELECT * FROM dbo.Results;
GO

-- Check Routes
SELECT * FROM dbo.Routes;
GO

-- Check Weather
SELECT * FROM dbo.Weather;
GO

-- Sample join query (Participants with Event & Category details)
SELECT
    u.FirstName + ' ' + u.LastName AS Participant,
    e.EventName,
    c.CategoryName,
    en.RaceNumber,
    en.PaymentStatus
FROM dbo.Enrolments en
INNER JOIN dbo.RaceDayUsers u
    ON en.ParticipantID = u.UserID
INNER JOIN dbo.Events e
    ON en.EventID = e.EventID
INNER JOIN dbo.Categories c
    ON en.CategoryID = c.CategoryID
ORDER BY e.EventName, u.LastName;
GO
