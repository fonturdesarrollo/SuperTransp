-- GasStation: parroquia (District) obligatoria al cargar estaciones.
-- Script idempotente: se puede correr varias veces en cualquier BD (Windows, Mac, produccion).
-- NOTA: la tabla District se debe poblar con las parroquias reales (no incluye datos).
-- GasStation.MunicipalityId se mantiene por retrocompatibilidad.

-- 1. Tabla District
IF OBJECT_ID('dbo.District', 'U') IS NULL
BEGIN
	CREATE TABLE dbo.District
	(
		DistrictId     INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_District PRIMARY KEY,
		MunicipalityId INT NOT NULL,
		DistrictName   VARCHAR(50) NOT NULL
	);
END
GO

-- 1b. PK sobre District.DistrictId (si la tabla se creo sin PK)
IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE parent_object_id = OBJECT_ID('dbo.District') AND type = 'PK')
   AND COL_LENGTH('dbo.District', 'DistrictId') IS NOT NULL
   AND NOT EXISTS (SELECT DistrictId FROM dbo.District GROUP BY DistrictId HAVING COUNT(*) > 1)
BEGIN
	ALTER TABLE dbo.District ALTER COLUMN DistrictId INT NOT NULL;
	ALTER TABLE dbo.District ADD CONSTRAINT PK_District PRIMARY KEY (DistrictId);
END
GO

-- 2. GasStation.DistrictId
IF COL_LENGTH('dbo.GasStation', 'DistrictId') IS NULL
BEGIN
	ALTER TABLE dbo.GasStation ADD DistrictId INT NULL;
END
GO

-- 3. FK GasStation -> District (solo si la tabla District tiene PK sobre DistrictId)
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_GasStation_District')
   AND EXISTS (SELECT 1 FROM sys.indexes i
               JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
               JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
               WHERE i.object_id = OBJECT_ID('dbo.District') AND i.is_unique = 1 AND c.name = 'DistrictId')
BEGIN
	ALTER TABLE dbo.GasStation WITH NOCHECK
		ADD CONSTRAINT FK_GasStation_District FOREIGN KEY (DistrictId) REFERENCES dbo.District (DistrictId);
END
GO

-- 4. SP de escritura
CREATE OR ALTER PROCEDURE dbo.SuperTransp_GasStationAddOrEdit
	@GasStationId INT,
	@MunicipalityId INT,
	@GasStationName VARCHAR(150),
	@GasStationAddress VARCHAR(150),
	@DistrictId INT = NULL
AS
BEGIN
	SET NOCOUNT ON

	IF @GasStationId = 0
	BEGIN
		INSERT INTO GasStation(MunicipalityId, DistrictId, GasStationName, GasStationAddress)
		VALUES(@MunicipalityId, @DistrictId, @GasStationName, @GasStationAddress)
		SELECT @@IDENTITY
	END

	ELSE
	BEGIN
		UPDATE GasStation
		SET
			MunicipalityId = @MunicipalityId,
			DistrictId = @DistrictId,
			GasStationName = @GasStationName,
			GasStationAddress = @GasStationAddress

		WHERE GasStationId  = @GasStationId
	END
END
GO

-- 5. Vista de consulta: se agregan DistrictId y DistrictName al final (las demas columnas no cambian)
CREATE OR ALTER VIEW dbo.SuperTransp_GasStationsDetail
AS
SELECT        TOP (100) PERCENT dbo.State.StateName, dbo.Municipality.MunicipalityName, dbo.State.StateId, dbo.Municipality.MunicipalityId, ISNULL(dbo.SuperTransp_GasStationsTotalByMunicipality.TotalByMunicipality, 0)
                         AS TotalByMunicipality, ISNULL(dbo.SuperTransp_GasStationsTotalByPTG.TotalByPTG, 0) AS TotalByPTG, ISNULL(dbo.GasStation.GasStationName, 'N/D') AS GasStationName, ISNULL(dbo.GasStation.GasStationAddress, 'N/D')
                          AS GasStationAddress, ISNULL(dbo.GasStation.GasStationId, 0) AS GasStationId,
                         ISNULL(dbo.GasStation.DistrictId, 0) AS DistrictId, ISNULL(dbo.District.DistrictName, 'N/D') AS DistrictName
FROM            dbo.Municipality LEFT OUTER JOIN
                         dbo.SuperTransp_GasStationsTotalByPTG ON dbo.Municipality.MunicipalityId = dbo.SuperTransp_GasStationsTotalByPTG.MunicipalityId LEFT OUTER JOIN
                         dbo.SuperTransp_GasStationsTotalByMunicipality ON dbo.Municipality.MunicipalityId = dbo.SuperTransp_GasStationsTotalByMunicipality.MunicipalityId LEFT OUTER JOIN
                         dbo.State ON dbo.Municipality.StateId = dbo.State.StateId LEFT OUTER JOIN
                         dbo.GasStation ON dbo.Municipality.MunicipalityId = dbo.GasStation.MunicipalityId LEFT OUTER JOIN
                         dbo.District ON dbo.GasStation.DistrictId = dbo.District.DistrictId
WHERE        (ISNULL(dbo.GasStation.GasStationName, 'N/D') <> 'N/D')
ORDER BY dbo.Municipality.MunicipalityName, dbo.State.StateName
GO

-- 6. Vista de parroquias por municipio (combo anidado)
CREATE OR ALTER VIEW dbo.Geography_GetDistrictByMunicipalityId
AS
SELECT        TOP (100) PERCENT DistrictId, MunicipalityId, DistrictName
FROM            dbo.District
ORDER BY DistrictName
GO
