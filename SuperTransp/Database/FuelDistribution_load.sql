-- Fuel distribution ("operativo de combustible"): load records.
-- Run AFTER FuelDistribution_open.sql. Idempotent: safe to run several times, on every database.
--
-- Model: the row created when the operativo is opened (GasStationId = 0, status 'Abierto') only holds
-- StateId, FuelDistributionStartDate and FuelDistributionRemarks. The FIRST loaded record completes
-- that same row (station, fuel, totals...). Every following record is a new row that inherits StateId,
-- StartDate and Remarks of the opening. ProccessDate is the date of each record.
-- Deleting the only record left of an open operativo resets its row to the "just opened" state
-- (instead of deleting it), so the operativo is not lost.

-- 1) ProccessDate: date of each added record (filled by the database).
IF COL_LENGTH('dbo.FuelDistribution', 'ProccessDate') IS NULL
BEGIN
	ALTER TABLE dbo.FuelDistribution
		ADD ProccessDate SMALLDATETIME NOT NULL
		CONSTRAINT DF_FuelDistribution_ProccessDate DEFAULT (GETDATE());
END
GO

-- 2) Query view: loaded records only (the header row has GasStationId = 0).
CREATE OR ALTER VIEW dbo.SuperTransp_FuelDistributionLoadDetail
AS
SELECT
	fd.FuelDistributionId,
	fd.StateId,
	fd.FuelDistributionStartDate,
	fd.FuelDistributionStatus,
	fd.ProccessDate,
	fd.GasStationId,
	gs.GasStationName,
	m.MunicipalityName,
	ISNULL(d.DistrictName, 'N/D') AS DistrictName,
	fd.FuelTypeId,
	ft.FuelTypeName,
	fd.TrucksReceived,
	ISNULL(fd.TrucksRemarks, '') AS TrucksRemarks,
	fd.LitersReceived,
	fd.LitersDispatched,
	fd.UrbanAttended,
	fd.SubUrbanAttended,
	fd.TaxiAttended,
	fd.MotoTaxiAttended,
	ISNULL(fd.DayRemarks, '') AS DayRemarks,
	gs.MunicipalityId,
	ISNULL(gs.DistrictId, 0) AS DistrictId
FROM dbo.FuelDistribution fd
	INNER JOIN dbo.GasStation gs ON gs.GasStationId = fd.GasStationId
	INNER JOIN dbo.Municipality m ON m.MunicipalityId = gs.MunicipalityId
	LEFT JOIN dbo.District d ON d.DistrictId = gs.DistrictId
	INNER JOIN dbo.FuelType ft ON ft.FuelTypeId = fd.FuelTypeId
WHERE fd.GasStationId > 0
GO

-- 3) Stored procedure: adds one loaded record to the open operativo of the state.
--    First record: completes the opening row (GasStationId = 0). Next ones: new rows that inherit
--    StateId, StartDate and Remarks of the opening.
--    Returns the FuelDistributionId, or 0 when the state has no open operativo.
CREATE OR ALTER PROCEDURE dbo.SuperTransp_FuelDistributionLoadAdd
	@StateId INT,
	@GasStationId INT,
	@FuelTypeId INT,
	@TrucksReceived INT,
	@TrucksRemarks VARCHAR(250),
	@LitersReceived INT,
	@LitersDispatched INT,
	@UrbanAttended INT,
	@SubUrbanAttended INT,
	@TaxiAttended INT,
	@MotoTaxiAttended INT,
	@DayRemarks VARCHAR(500)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @StartDate SMALLDATETIME, @Remarks VARCHAR(250), @OpeningRowId INT

	-- Opening row that has not received its first record yet
	SELECT TOP 1 @OpeningRowId = FuelDistributionId
	FROM FuelDistribution
	WHERE StateId = @StateId AND GasStationId = 0 AND FuelDistributionStatus = 'Abierto'
	ORDER BY FuelDistributionId

	IF @OpeningRowId IS NOT NULL
	BEGIN
		UPDATE FuelDistribution
		SET GasStationId = @GasStationId,
			FuelTypeId = @FuelTypeId,
			LitersReceived = @LitersReceived,
		LitersDispatched = @LitersDispatched,
			TrucksReceived = @TrucksReceived,
			TrucksRemarks = @TrucksRemarks,
			UrbanAttended = @UrbanAttended,
			SubUrbanAttended = @SubUrbanAttended,
			TaxiAttended = @TaxiAttended,
			MotoTaxiAttended = @MotoTaxiAttended,
			DayRemarks = @DayRemarks,
			ProccessDate = GETDATE()
		WHERE FuelDistributionId = @OpeningRowId

		SELECT @OpeningRowId
		RETURN
	END

	-- Following records inherit the data of the opening (the oldest row of the open operativo)
	SELECT TOP 1 @StartDate = FuelDistributionStartDate, @Remarks = FuelDistributionRemarks
	FROM FuelDistribution
	WHERE StateId = @StateId AND FuelDistributionStatus = 'Abierto'
	ORDER BY FuelDistributionId

	IF @StartDate IS NULL
	BEGIN
		SELECT 0
		RETURN
	END

	INSERT INTO FuelDistribution
		(StateId, GasStationId, FuelDistributionStartDate, FuelDistributionRemarks, FuelTypeId,
		 LitersReceived, LitersDispatched, TrucksReceived, TrucksRemarks, UrbanAttended, SubUrbanAttended,
		 TaxiAttended, MotoTaxiAttended, DayRemarks)
	VALUES
		(@StateId, @GasStationId, @StartDate, @Remarks, @FuelTypeId,
		 @LitersReceived, @LitersDispatched, @TrucksReceived, @TrucksRemarks, @UrbanAttended, @SubUrbanAttended,
		 @TaxiAttended, @MotoTaxiAttended, @DayRemarks)

	SELECT CAST(SCOPE_IDENTITY() AS INT)
END
GO

-- 4) Stored procedure: edits a loaded record. Only records of the open operativo of @CurrentStateId;
--    the record keeps its StateId, so @StateId (selected state) must be the current one.
--    Returns the number of rows updated (0 = not found / not editable).
CREATE OR ALTER PROCEDURE dbo.SuperTransp_FuelDistributionLoadEdit
	@FuelDistributionId INT,
	@CurrentStateId INT,
	@StateId INT,
	@GasStationId INT,
	@FuelTypeId INT,
	@TrucksReceived INT,
	@TrucksRemarks VARCHAR(250),
	@LitersReceived INT,
	@LitersDispatched INT,
	@UrbanAttended INT,
	@SubUrbanAttended INT,
	@TaxiAttended INT,
	@MotoTaxiAttended INT,
	@DayRemarks VARCHAR(500)
AS
BEGIN
	SET NOCOUNT ON

	UPDATE FuelDistribution
	SET GasStationId = @GasStationId,
		FuelTypeId = @FuelTypeId,
		TrucksReceived = @TrucksReceived,
		TrucksRemarks = @TrucksRemarks,
		LitersReceived = @LitersReceived,
		UrbanAttended = @UrbanAttended,
		SubUrbanAttended = @SubUrbanAttended,
		TaxiAttended = @TaxiAttended,
		MotoTaxiAttended = @MotoTaxiAttended,
		DayRemarks = @DayRemarks
	WHERE FuelDistributionId = @FuelDistributionId
		AND StateId = @CurrentStateId
		AND @StateId = @CurrentStateId
		AND GasStationId > 0
		AND FuelDistributionStatus = 'Abierto'

	SELECT @@ROWCOUNT
END
GO

-- 5) Stored procedure: deletes a loaded record (never an unloaded opening row, never a closed operativo).
--    If it is the only row left of the open operativo, the row is reset to the "just opened" state
--    (GasStationId = 0, totals in 0) instead of deleted, so the operativo stays open.
--    Returns the number of rows deleted/reset.
CREATE OR ALTER PROCEDURE dbo.SuperTransp_FuelDistributionLoadDelete
	@FuelDistributionId INT,
	@StateId INT
AS
BEGIN
	SET NOCOUNT ON

	IF (SELECT COUNT(*) FROM FuelDistribution WHERE StateId = @StateId AND FuelDistributionStatus = 'Abierto') = 1
	BEGIN
		UPDATE FuelDistribution
		SET GasStationId = 0,
			FuelTypeId = 0,
			LitersReceived = 0,
			TrucksReceived = 0,
			TrucksRemarks = NULL,
			UrbanAttended = 0,
			SubUrbanAttended = 0,
			TaxiAttended = 0,
			MotoTaxiAttended = 0,
			DayRemarks = NULL
		WHERE FuelDistributionId = @FuelDistributionId
			AND StateId = @StateId
			AND GasStationId > 0
			AND FuelDistributionStatus = 'Abierto'

		SELECT @@ROWCOUNT
		RETURN
	END

	DELETE FROM FuelDistribution
	WHERE FuelDistributionId = @FuelDistributionId
		AND StateId = @StateId
		AND GasStationId > 0
		AND FuelDistributionStatus = 'Abierto'

	SELECT @@ROWCOUNT
END
GO
