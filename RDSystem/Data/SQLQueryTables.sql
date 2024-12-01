USE [RDS_DB]
GO

/****** Object:  Table [dbo].[FlyObjects]    Script Date: 2024-12-01 20:05:52 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('dbo.Aircraft', 'U') IS NOT NULL 
DROP TABLE [dbo].[Aircraft];

-- Create the Aircraft table
CREATE TABLE [dbo].[Aircraft] (
    ICAO24 NVARCHAR(200) PRIMARY KEY,  -- Unique ICAO24 identifier for the aircraft
    Callsign NVARCHAR(200) NULL,      -- Callsign of the aircraft
    Latitude FLOAT NULL,             -- Latitude of the aircraft
    Longitude FLOAT NULL,            -- Longitude of the aircraft
    Altitude FLOAT NULL,             -- Altitude of the aircraft
    Velocity FLOAT NULL,             -- Velocity of the aircraft
    Heading FLOAT NULL               -- Heading of the aircraft
);



GO

IF OBJECT_ID('dbo.Positions', 'U') IS NOT NULL 
DROP TABLE [dbo].[Positions];

-- Create the Positions table with a foreign key reference to Aircraft
CREATE TABLE [dbo].[Positions] (
    PositionID INT IDENTITY(1,1) PRIMARY KEY,   -- Unique identifier for each position
    ICAO24 NVARCHAR(200),                         -- Foreign key to Aircraft
    Latitude FLOAT,                              -- Latitude of the aircraft at the recorded position
    Longitude FLOAT,                             -- Longitude of the aircraft at the recorded position
    Altitude FLOAT,                              -- Altitude of the aircraft at the recorded position
    Velocity FLOAT,                              -- Velocity of the aircraft at the recorded position
    Heading FLOAT,                               -- Heading of the aircraft at the recorded position
    Timestamp DATETIME DEFAULT GETDATE(),        -- Timestamp when the position is recorded
    FOREIGN KEY (ICAO24) REFERENCES [dbo].[Aircraft](ICAO24) -- Foreign key to Aircraft
);


