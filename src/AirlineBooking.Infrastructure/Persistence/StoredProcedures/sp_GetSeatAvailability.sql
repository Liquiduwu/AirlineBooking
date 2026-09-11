-- Reference copy of the stored procedure created by migration AddStoredProcedures.
-- This file is not executed directly by the app or by `dotnet ef database update` - the
-- migration's Up() method is the actual source of truth (so schema history stays reproducible
-- from `dotnet ef database update` alone, without a second out-of-band script step). This copy
-- exists so the SQL is reviewable as SQL in a PR diff, not buried inside a C# string literal.
--
-- Returns the full seat map for one flight with live availability: a seat counts as available
-- unless it has a Held or Confirmed booking on *this* flight (the same physical seat can be
-- free on one flight and taken on another, since seats belong to the aircraft, not the flight).
CREATE OR ALTER PROCEDURE dbo.sp_GetSeatAvailability
    @FlightId INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @AircraftId INT;
    SELECT @AircraftId = AircraftId FROM dbo.Flights WHERE Id = @FlightId;

    IF @AircraftId IS NULL
    BEGIN
        RAISERROR('Flight %d not found.', 16, 1, @FlightId);
        RETURN;
    END

    SELECT
        s.Id AS SeatId,
        s.SeatNumber,
        s.CabinClass,
        s.IsExitRow,
        CASE WHEN b.Id IS NULL THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsAvailable
    FROM dbo.Seats s
    LEFT JOIN dbo.Bookings b
        ON b.SeatId = s.Id
        AND b.FlightId = @FlightId
        AND b.Status IN ('Held', 'Confirmed')
    WHERE s.AircraftId = @AircraftId
    ORDER BY s.SeatNumber;
END
