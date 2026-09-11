-- Reference copy of the stored procedure created by migration AddStoredProcedures.
-- See sp_GetSeatAvailability.sql for why this exists alongside (not instead of) the migration.
--
-- Computes the price for a flight + fare class as base fare * fare class multiplier, plus a
-- load-factor surcharge that rises in tiers as the flight fills up - a simplified version of
-- the demand-based pricing real airline revenue-management systems use, and specifically the
-- kind of business rule that belongs in the database (single source of truth for "what does
-- this booking cost", computed from live occupancy at query time) rather than duplicated in
-- application code where it could drift out of sync with what was actually charged.
CREATE OR ALTER PROCEDURE dbo.sp_CalculateFare
    @FlightId INT,
    @FareClassId INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @BaseFare DECIMAL(10,2);
    DECLARE @AircraftId INT;
    DECLARE @Multiplier DECIMAL(5,2);
    DECLARE @TotalSeats INT;
    DECLARE @BookedSeats INT;
    DECLARE @OccupancyRate DECIMAL(9,4);
    DECLARE @SurchargeRate DECIMAL(5,4);

    SELECT @BaseFare = BaseFare, @AircraftId = AircraftId FROM dbo.Flights WHERE Id = @FlightId;
    IF @BaseFare IS NULL
    BEGIN
        RAISERROR('Flight %d not found.', 16, 1, @FlightId);
        RETURN;
    END

    SELECT @Multiplier = PriceMultiplier FROM dbo.FareClasses WHERE Id = @FareClassId;
    IF @Multiplier IS NULL
    BEGIN
        RAISERROR('Fare class %d not found.', 16, 1, @FareClassId);
        RETURN;
    END

    SELECT @TotalSeats = COUNT(*) FROM dbo.Seats WHERE AircraftId = @AircraftId;

    SELECT @BookedSeats = COUNT(*)
    FROM dbo.Bookings
    WHERE FlightId = @FlightId AND Status IN ('Held', 'Confirmed');

    SET @OccupancyRate = CASE WHEN @TotalSeats = 0 THEN 0 ELSE CAST(@BookedSeats AS DECIMAL(9,4)) / @TotalSeats END;

    SET @SurchargeRate =
        CASE
            WHEN @OccupancyRate >= 0.90 THEN 0.25
            WHEN @OccupancyRate >= 0.75 THEN 0.15
            WHEN @OccupancyRate >= 0.50 THEN 0.05
            ELSE 0
        END;

    DECLARE @SubTotal DECIMAL(10,2) = @BaseFare * @Multiplier;
    DECLARE @Surcharge DECIMAL(10,2) = ROUND(@SubTotal * @SurchargeRate, 2);

    SELECT
        @BaseFare AS BaseFare,
        @Multiplier AS PriceMultiplier,
        @Surcharge AS LoadFactorSurcharge,
        (@SubTotal + @Surcharge) AS TotalFare;
END
