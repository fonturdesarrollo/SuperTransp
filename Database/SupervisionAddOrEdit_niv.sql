-- Supervision: adds the vehicle NIV (VIN). The procedure now receives @Niv and stores it in Supervision.Niv
-- (the column already exists). @Niv has a default so callers that do not send it keep working. Idempotent.
CREATE OR ALTER PROCEDURE [dbo].[SuperTransp_SupervisionAddOrEdit]
	@SupervisionId INT,
	@DriverId INT,
	@DriverWithVehicle INT,
	@WorkingVehicle BIT,
	@InPerson BIT,
	@Plate VARCHAR(20),
	@VehicleDataId INT,
	@Passengers INT,
	@RimId INT,
	@Wheels INT,
	@MotorOilId INT,
	@Liters INT,
	@FuelTypeId INT,
	@TankCapacity INT,
	@BatteryId INT,
	@NumberOfBatteries INT,
	@BodySerialNumber VARCHAR(50),
	@EngineSerialNumber VARCHAR(50),
	@VehicleImageUrl VARCHAR(1000),
	@FailureTypeId INT,
	@FingerprintTrouble BIT,
	@Remarks VARCHAR(250),
	@SecurityUserId INT,
	@SupervisionStatus BIT,
	@SupervisionRoundId INT,
	@SupervisionRoundStartDate SMALLDATETIME,
	@SupervisionRoundStartDescription VARCHAR (50),
	@SupervisionRoundEndDate SMALLDATETIME,
	@SupervisionRoundEndDescription VARCHAR (50),
	@SupervisionRoundStatus BIT,
	@Niv VARCHAR(50) = ''
AS
BEGIN
	SET NOCOUNT ON

	BEGIN
		DELETE FROM Supervision WHERE SupervisionId = @SupervisionId

		INSERT INTO Supervision(DriverId, DriverWithVehicle,WorkingVehicle,InPerson,Plate,VehicleDataId,Passengers,RimId,Wheels,MotorOilId,Liters,FuelTypeId,TankCapacity,BatteryId,NumberOfBatteries,VehicleImageUrl,FailureTypeId,FingerprintTrouble,Remarks,SecurityUserId,SupervisionRoundId, SupervisionRoundStartDate, SupervisionRoundStartDescription, SupervisionRoundEndDate, SupervisionRoundEndDescription, SupervisionRoundStatus, BodySerialNumber, EngineSerialNumber, Niv)
		VALUES(@DriverId, @DriverWithVehicle, @WorkingVehicle, @InPerson, @Plate, @VehicleDataId,@Passengers,@RimId,@Wheels,@MotorOilId,@Liters,@FuelTypeId,@TankCapacity,@BatteryId,@NumberOfBatteries,@VehicleImageUrl,@FailureTypeId,@FingerprintTrouble,@Remarks,@SecurityUserId,@SupervisionRoundId, @SupervisionRoundStartDate, @SupervisionRoundStartDescription, @SupervisionRoundEndDate, @SupervisionRoundEndDescription, @SupervisionRoundStatus, @BodySerialNumber, @EngineSerialNumber, @Niv)

		SELECT @@IDENTITY
	END
END
GO
