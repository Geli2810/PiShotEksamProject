-- =============================================
-- 1. DROP TABLES (Order is specific due to Foreign Keys)
-- =============================================

-- Drop "Child" tables first (tables that point to others)
DROP TABLE IF EXISTS [dbo].[Scores];
DROP TABLE IF EXISTS [dbo].[ShotAttempts];
DROP TABLE IF EXISTS [dbo].[CurrentGame];

-- Drop "Parent" tables next
DROP TABLE IF EXISTS [dbo].[GameResults];
DROP TABLE IF EXISTS [dbo].[Profiles];


-- =============================================
-- 2. RECREATE TABLES (Parent tables first)
-- =============================================

-- 1. Profiles (Root table, referenced by everyone)
CREATE TABLE [dbo].[Profiles] (
    [Id]           INT            IDENTITY (1, 1) NOT NULL,
    [Name]         NVARCHAR (50)  NOT NULL,
    [ProfileImage] NVARCHAR (MAX) NULL,
    [CreatedAt]    DATETIME       DEFAULT (getdate()) NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

-- 2. GameResults (References Profiles)
CREATE TABLE [dbo].[GameResults] (
    [Id]        INT           IDENTITY (1, 1) NOT NULL,
    [WinnerId]  INT           NOT NULL,
    [LoserId]   INT           NOT NULL,
    [PlayedOn]  DATETIME      DEFAULT (getdate()) NULL,
    [GameScore] NVARCHAR (10) NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_GameResults_Winner] FOREIGN KEY ([WinnerId]) REFERENCES [dbo].[Profiles] ([Id]),
    CONSTRAINT [FK_GameResults_Loser] FOREIGN KEY ([LoserId]) REFERENCES [dbo].[Profiles] ([Id])
);

-- 3. CurrentGame (References Profiles)
CREATE TABLE [dbo].[CurrentGame] (
    [Id]               INT      DEFAULT ((1)) NOT NULL,
    [Player1Id]        INT      NULL,
    [Player2Id]        INT      NULL,
    [IsActive]         BIT      DEFAULT ((0)) NULL,
    [StartTime]        DATETIME NULL,
    [IsTiebreak]       BIT      DEFAULT ((0)) NULL,
    [TiebreakOffsetP1] INT      DEFAULT ((0)) NULL,
    [TiebreakOffsetP2] INT      DEFAULT ((0)) NULL,
    [CurrentWinnerId]  INT      NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_CurrentGame_Player1] FOREIGN KEY ([Player1Id]) REFERENCES [dbo].[Profiles] ([Id]),
    CONSTRAINT [FK_CurrentGame_Player2] FOREIGN KEY ([Player2Id]) REFERENCES [dbo].[Profiles] ([Id]),
    CONSTRAINT [FK_CurrentGame_Winner] FOREIGN KEY ([CurrentWinnerId]) REFERENCES [dbo].[Profiles] ([Id])
);

-- 4. Scores (References Profiles and GameResults)
CREATE TABLE [dbo].[Scores] (
    [Id]           INT      IDENTITY (1, 1) NOT NULL,
    [ProfileId]    INT      NOT NULL,
    [ScoredAt]     DATETIME DEFAULT (getdate()) NULL,
    [GameResultId] INT      NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Scores_GameResult] FOREIGN KEY ([GameResultId]) REFERENCES [dbo].[GameResults] ([Id]),
    CONSTRAINT [FK_Scores_Profile] FOREIGN KEY ([ProfileId]) REFERENCES [dbo].[Profiles] ([Id])
);

-- 5. ShotAttempts (References Profiles and GameResults)
CREATE TABLE [dbo].[ShotAttempts] (
    [Id]           INT      IDENTITY (1, 1) NOT NULL,
    [ProfileId]    INT      NOT NULL,
    [AttemptedAt]  DATETIME DEFAULT (getdate()) NULL,
    [GameResultId] INT      NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ShotAttempts_GameResult] FOREIGN KEY ([GameResultId]) REFERENCES [dbo].[GameResults] ([Id]),
    CONSTRAINT [FK_ShotAttempts_Profile] FOREIGN KEY ([ProfileId]) REFERENCES [dbo].[Profiles] ([Id])
);


-- =============================================
-- 3. INITIALIZE DEFAULT STATE
-- =============================================

-- We must have one row in CurrentGame with ID=1.
-- Since the Foreign Keys (Player1Id, etc.) are NULLable, we can insert NULLs here without error.
INSERT INTO [dbo].[CurrentGame] 
(Id, Player1Id, Player2Id, IsActive, StartTime, IsTiebreak, TiebreakOffsetP1, TiebreakOffsetP2, CurrentWinnerId)
VALUES 
(1, NULL, NULL, 0, NULL, 0, 0, 0, NULL);

-- Verification
SELECT * FROM CurrentGame;