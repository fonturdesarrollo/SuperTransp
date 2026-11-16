-- Fuel distribution ("operativo de combustible"): close the open operativo of a state.
-- Run AFTER FuelDistribution_open.sql / FuelDistribution_load.sql. Idempotent.

-- Sets FuelDistributionStatus = 'Cerrado' on every row of the open operativo of the state
-- (the opening/first record and all the loaded records). Returns the number of rows closed
-- (0 = the state had no open operativo).
CREATE OR ALTER PROCEDURE dbo.SuperTransp_FuelDistributionClose
	@StateId INT
AS
BEGIN
	SET NOCOUNT ON

	UPDATE FuelDistribution
	SET FuelDistributionStatus = 'Cerrado'
	WHERE StateId = @StateId
		AND FuelDistributionStatus = 'Abierto'

	SELECT @@ROWCOUNT
END
GO
