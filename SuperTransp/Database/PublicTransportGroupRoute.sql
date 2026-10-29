-- Organization routes: each PublicTransportGroup can have several routes (start, end and the full path as an encoded polyline, precision 5).
-- Idempotent: it can be run several times (local and production), also over the previous single-route version of the table.

IF OBJECT_ID('dbo.PublicTransportGroupRoute', 'U') IS NULL
BEGIN
	CREATE TABLE dbo.PublicTransportGroupRoute
	(
		PublicTransportGroupId INT NOT NULL,
		RouteNumber INT NOT NULL CONSTRAINT DF_PublicTransportGroupRoute_RouteNumber DEFAULT (1),
		RouteStartName NVARCHAR(250) NOT NULL,
		RouteStartLat DECIMAL(9, 6) NOT NULL,
		RouteStartLng DECIMAL(9, 6) NOT NULL,
		RouteEndName NVARCHAR(250) NOT NULL,
		RouteEndLat DECIMAL(9, 6) NOT NULL,
		RouteEndLng DECIMAL(9, 6) NOT NULL,
		RouteGeometry NVARCHAR(MAX) NOT NULL,
		RouteDistanceMeters INT NOT NULL,
		RouteDurationSeconds INT NOT NULL,
		RouteModifiedDate SMALLDATETIME NOT NULL CONSTRAINT DF_PublicTransportGroupRoute_ModifiedDate DEFAULT (GETDATE()),
		CONSTRAINT PK_PublicTransportGroupRoute PRIMARY KEY (PublicTransportGroupId, RouteNumber),
		CONSTRAINT FK_PublicTransportGroupRoute_PublicTransportGroup FOREIGN KEY (PublicTransportGroupId)
			REFERENCES dbo.PublicTransportGroup (PublicTransportGroupId)
	)
END
GO

-- Previous version had one route per organization (primary key on PublicTransportGroupId only): existing rows become route 1
IF COL_LENGTH('dbo.PublicTransportGroupRoute', 'RouteNumber') IS NULL
BEGIN
	ALTER TABLE dbo.PublicTransportGroupRoute DROP CONSTRAINT PK_PublicTransportGroupRoute
	ALTER TABLE dbo.PublicTransportGroupRoute ADD RouteNumber INT NOT NULL CONSTRAINT DF_PublicTransportGroupRoute_RouteNumber DEFAULT (1)
	ALTER TABLE dbo.PublicTransportGroupRoute ADD CONSTRAINT PK_PublicTransportGroupRoute PRIMARY KEY (PublicTransportGroupId, RouteNumber)
END
GO

-- Descriptive intermediate points (free text, optional): they do not take part in the route calculation
IF COL_LENGTH('dbo.PublicTransportGroupRoute', 'RouteMidPoint1') IS NULL
	ALTER TABLE dbo.PublicTransportGroupRoute ADD RouteMidPoint1 NVARCHAR(250) NULL
GO

IF COL_LENGTH('dbo.PublicTransportGroupRoute', 'RouteMidPoint2') IS NULL
	ALTER TABLE dbo.PublicTransportGroupRoute ADD RouteMidPoint2 NVARCHAR(250) NULL
GO

CREATE OR ALTER PROCEDURE dbo.SuperTransp_PublicTransportGroupRouteDelete
	@PublicTransportGroupId INT
AS
BEGIN
	SET NOCOUNT ON

	DELETE FROM dbo.PublicTransportGroupRoute WHERE PublicTransportGroupId = @PublicTransportGroupId
END
GO

CREATE OR ALTER PROCEDURE dbo.SuperTransp_PublicTransportGroupRouteAddOrEdit
	@PublicTransportGroupId INT,
	@RouteNumber INT,
	@RouteStartName NVARCHAR(250),
	@RouteStartLat DECIMAL(9, 6),
	@RouteStartLng DECIMAL(9, 6),
	@RouteEndName NVARCHAR(250),
	@RouteEndLat DECIMAL(9, 6),
	@RouteEndLng DECIMAL(9, 6),
	@RouteGeometry NVARCHAR(MAX),
	@RouteDistanceMeters INT,
	@RouteDurationSeconds INT,
	@RouteMidPoint1 NVARCHAR(250) = NULL,
	@RouteMidPoint2 NVARCHAR(250) = NULL
AS
BEGIN
	SET NOCOUNT ON

	IF EXISTS (SELECT 1 FROM dbo.PublicTransportGroupRoute WHERE PublicTransportGroupId = @PublicTransportGroupId AND RouteNumber = @RouteNumber)
	BEGIN
		UPDATE dbo.PublicTransportGroupRoute
		SET
			RouteStartName = @RouteStartName,
			RouteStartLat = @RouteStartLat,
			RouteStartLng = @RouteStartLng,
			RouteEndName = @RouteEndName,
			RouteEndLat = @RouteEndLat,
			RouteEndLng = @RouteEndLng,
			RouteGeometry = @RouteGeometry,
			RouteDistanceMeters = @RouteDistanceMeters,
			RouteDurationSeconds = @RouteDurationSeconds,
			RouteMidPoint1 = @RouteMidPoint1,
			RouteMidPoint2 = @RouteMidPoint2,
			RouteModifiedDate = GETDATE()
		WHERE PublicTransportGroupId = @PublicTransportGroupId AND RouteNumber = @RouteNumber
	END
	ELSE
	BEGIN
		INSERT INTO dbo.PublicTransportGroupRoute(PublicTransportGroupId, RouteNumber, RouteStartName, RouteStartLat, RouteStartLng, RouteEndName, RouteEndLat, RouteEndLng, RouteGeometry, RouteDistanceMeters, RouteDurationSeconds, RouteMidPoint1, RouteMidPoint2)
		VALUES(@PublicTransportGroupId, @RouteNumber, @RouteStartName, @RouteStartLat, @RouteStartLng, @RouteEndName, @RouteEndLat, @RouteEndLng, @RouteGeometry, @RouteDistanceMeters, @RouteDurationSeconds, @RouteMidPoint1, @RouteMidPoint2)
	END
END
GO
