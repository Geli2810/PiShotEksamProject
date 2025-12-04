-- DROP TABLES IF EXIST (Clean Slate)
IF OBJECT_ID('Scores', 'U') IS NOT NULL DROP TABLE Scores;
IF OBJECT_ID('ShotAttempts', 'U') IS NOT NULL DROP TABLE ShotAttempts;
IF OBJECT_ID('CurrentGame', 'U') IS NOT NULL DROP TABLE CurrentGame;
IF OBJECT_ID('GameResults', 'U') IS NOT NULL DROP TABLE GameResults;
IF OBJECT_ID('Profiles', 'U') IS NOT NULL DROP TABLE Profiles;

-- 1. PROFILES
CREATE TABLE Profiles (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL,
    ProfileImage NVARCHAR(MAX), -- Base64 string
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- 2. CURRENT GAME STATE (Includes Tiebreak columns)
CREATE TABLE CurrentGame (
    Id INT PRIMARY KEY DEFAULT 1,
    Player1Id INT,
    Player2Id INT,
    IsActive BIT DEFAULT 0,
    StartTime DATETIME NULL,
    IsTiebreak BIT DEFAULT 0,      -- 0 = Normal, 1 = Overtime
    TiebreakOffsetP1 INT DEFAULT 0, -- Score to subtract for visual display
    TiebreakOffsetP2 INT DEFAULT 0
);
INSERT INTO CurrentGame (Id, Player1Id, Player2Id, IsActive) VALUES (1, 0, 0, 0);

-- 3. STATS LOGS
CREATE TABLE Scores (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProfileId INT NOT NULL, 
    ScoredAt DATETIME DEFAULT GETDATE()
);

CREATE TABLE ShotAttempts (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProfileId INT NOT NULL,
    AttemptedAt DATETIME DEFAULT GETDATE()
);

-- 4. HISTORY
CREATE TABLE GameResults (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    WinnerId INT NOT NULL,
    LoserId INT NOT NULL,
    PlayedAt DATETIME DEFAULT GETDATE()
);