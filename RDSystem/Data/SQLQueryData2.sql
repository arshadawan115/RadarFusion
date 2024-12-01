-- Delete existing data from Aircraft and Positions tables
--DELETE FROM [dbo].[Positions];
--DELETE FROM [dbo].[Aircraft];

-- Insert Aircraft into the Aircraft table
DECLARE @ICAO24 NVARCHAR(200) = 'StockholmSelect786';  -- ICAO24 of the aircraft
DECLARE @Callsign NVARCHAR(200) = 'StockholmSelectCallSign';
DECLARE @Latitude0 FLOAT = 59.3293; -- Central Latitude (Stockholm)
DECLARE @Longitude0 FLOAT = 18.0686; -- Central Longitude (Stockholm)
DECLARE @Altitude FLOAT = 125;  -- Altitude can be NULL initially
DECLARE @Velocity FLOAT = 25;  -- Velocity can be NULL initially
DECLARE @Heading FLOAT = 12;   -- Heading can be NULL initially

-- Insert aircraft record
INSERT INTO [dbo].[Aircraft] (ICAO24, Callsign, Latitude, Longitude, Altitude, Velocity, Heading)
VALUES (@ICAO24, @Callsign, @Latitude0, @Longitude0, @Altitude, @Velocity, @Heading);

-- Declare parameters for position generation
DECLARE @Latitude FLOAT = 59.3293;  -- Central Latitude (Stockholm)
DECLARE @Longitude FLOAT = 18.0686; -- Central Longitude (Stockholm)
DECLARE @Radius FLOAT = 35;          -- Radius in kilometers
DECLARE @AircraftICAO24 NVARCHAR(200) = 'StockholmSelect786'; -- ICAO24 of the aircraft

DECLARE @RADIUS_EARTH FLOAT = 6371;  -- Radius of the Earth in kilometers
DECLARE @DegToRad FLOAT = 3.14159265358979 / 180; -- Degrees to Radians conversion

-- Loop through 360 degrees to insert positions
DECLARE @i INT = 0;

WHILE @i < 360
BEGIN
    -- Calculate the bearing in radians
    DECLARE @Bearing FLOAT = @i * @DegToRad;

    -- Calculate the new latitude and longitude using the formula for a circle
    DECLARE @NewLatitude FLOAT = @Latitude + (@Radius / @RADIUS_EARTH) * COS(@Bearing) * (180 / PI());
    DECLARE @NewLongitude FLOAT = @Longitude + (@Radius / @RADIUS_EARTH) * SIN(@Bearing) * (180 / PI()) / COS(@Latitude * @DegToRad);

    -- Insert position into Positions table
    INSERT INTO [dbo].[Positions] (ICAO24, Latitude, Longitude, Altitude, Velocity, Heading, Timestamp)
    VALUES
    (@AircraftICAO24, @NewLatitude, @NewLongitude, NULL, NULL, NULL, GETDATE());

    -- Increment degree by 5
    SET @i = @i + 10;
END
