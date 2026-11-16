-- Fuel distribution ("operativo de combustible"): open an operativo.
-- Idempotent: safe to run several times, on every database (Windows, Mac, production).

-- 0) LitersDispatched: liters dispatched (NOT NULL; DEFAULT 0 so existing rows and the "open" INSERT work).
IF COL_LENGTH('dbo.FuelDistribution', 'LitersDispatched') IS NULL
BEGIN
	ALTER TABLE dbo.FuelDistribution ADD LitersDispatched INT NOT NULL
		CONSTRAINT DF_FuelDistribution_LitersDispatched DEFAULT 0;
END
GO

-- 1) The columns filled later (daily load) get DEFAULT 0 so the "open" INSERT can omit them.
DECLARE @cols TABLE (ColumnName SYSNAME);
INSERT INTO @cols VALUES
	('GasStationId'), ('FuelTypeId'), ('LitersReceived'), ('LitersDispatched'), ('TrucksReceived'),
	('UrbanAttended'), ('SubUrbanAttended'), ('TaxiAttended'), ('MotoTaxiAttended');

DECLARE @col SYSNAME, @sql NVARCHAR(MAX);
DECLARE c CURSOR LOCAL FAST_FORWARD FOR SELECT ColumnName FROM @cols;
OPEN c;
FETCH NEXT FROM c INTO @col;
WHILE @@FETCH_STATUS = 0
BEGIN
	IF NOT EXISTS (
		SELECT 1
		FROM sys.default_constraints dc
		JOIN sys.columns col ON col.object_id = dc.parent_object_id AND col.column_id = dc.parent_column_id
		WHERE dc.parent_object_id = OBJECT_ID('dbo.FuelDistribution') AND col.name = @col)
	BEGIN
		SET @sql = N'ALTER TABLE dbo.FuelDistribution ADD CONSTRAINT DF_FuelDistribution_' + @col
			+ N' DEFAULT 0 FOR ' + QUOTENAME(@col);
		EXEC sp_executesql @sql;
	END
	FETCH NEXT FROM c INTO @col;
END
CLOSE c;
DEALLOCATE c;
GO

-- 2) Stored procedure: inserts (id = 0) or edits date/remarks of an operativo.
CREATE OR ALTER PROCEDURE dbo.SuperTransp_FuelDistributionAddOrEdit
	@FuelDistributionId INT,
	@StateId INT,
	@FuelDistributionStartDate SMALLDATETIME,
	@FuelDistributionRemarks VARCHAR(250)
AS
BEGIN
	SET NOCOUNT ON

	IF @FuelDistributionId = 0
	BEGIN
		INSERT INTO FuelDistribution (StateId, FuelDistributionStartDate, FuelDistributionRemarks)
		VALUES (@StateId, @FuelDistributionStartDate, @FuelDistributionRemarks)

		SELECT CAST(SCOPE_IDENTITY() AS INT)
	END
	ELSE
	BEGIN
		UPDATE FuelDistribution
		SET FuelDistributionStartDate = @FuelDistributionStartDate,
			FuelDistributionRemarks = @FuelDistributionRemarks
		WHERE FuelDistributionId = @FuelDistributionId

		SELECT @FuelDistributionId
	END
END
GO
