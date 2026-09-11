using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirlineBooking.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStoredProcedures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // See Persistence/StoredProcedures/sp_GetSeatAvailability.sql for the commented,
            // reviewable copy of this SQL - this migration is what actually ships it.
            migrationBuilder.Sql(@"
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
");

            // See Persistence/StoredProcedures/sp_CalculateFare.sql for the commented,
            // reviewable copy of this SQL.
            migrationBuilder.Sql(@"
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
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.sp_GetSeatAvailability;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.sp_CalculateFare;");
        }
    }
}
