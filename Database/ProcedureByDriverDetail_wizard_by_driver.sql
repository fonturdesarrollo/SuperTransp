-- Wizard "Solicitar tramite" por socio + vehiculo: la vista de detalle debe exponer
-- organizacion, vehiculo y fecha del tramite guardados en ProcedureByDriver. Idempotente.
CREATE OR ALTER VIEW dbo.SuperTransp_ProcedureByDriverDetail
AS
SELECT        dbo.ProcedureByDriver.ProcedureByDriverId, dbo.ProcedureByDriver.ProcedureId, dbo.[Procedure].ProcedureConcept, dbo.ProcedureCategory.ProcedureCategoryName, dbo.Driver.DriverId, dbo.Driver.DriverIdentityDocument,
                         dbo.Driver.DriverFullName, dbo.Driver.DriverPhone, dbo.ProcedureByDriver.DriverPTGRif, dbo.ProcedureByDriver.DriverPTGName, dbo.ProcedureByDriver.DriverVehiclePlate,
                         dbo.ProcedureByDriver.DriverVehicleYear, dbo.ProcedureByDriver.DriverVehicleMake, dbo.ProcedureByDriver.DriverVehicleModel,
                         dbo.SuperTransp_ProcedureBankAccountDetail.AccounTypeName AS ReceiverBankAccountTypeName,
                         dbo.SuperTransp_ProcedureBankAccountDetail.BankName AS ReceiverBankName, dbo.SuperTransp_ProcedureBankAccountDetail.AccountNumber AS ReceiverBankAccountNumber,
                         dbo.SuperTransp_ProcedureBankAccountDetail.CellPhoneNumber AS ReceiverBankCellPhone, dbo.SuperTransp_ProcedureBankAccountDetail.IdNumber AS ReceiverBankIdNumber, dbo.Bank.BankName AS PayerBankName,
                         dbo.ProcedureByDriver.PayerCellPhoneNumber, dbo.ProcedureByDriver.PayerIdNumber, dbo.ProcedureByDriver.PayerAccountNumber, dbo.ProcedureByDriver.PayerReferenceNumber, dbo.ProcedureByDriver.BCVPaid,
                         dbo.ProcedureStatus.ProcedureStatusName, dbo.ProcedureStatus.ProcedureStatusId, dbo.ProcedureByDriver.Tariff, dbo.ProcedureByDriver.Rate, dbo.ProcedureByDriver.AccounTypeId,
                         dbo.ProcedureByDriver.ProcedureDate
FROM            dbo.Driver INNER JOIN
                         dbo.ProcedureByDriver ON dbo.Driver.DriverId = dbo.ProcedureByDriver.DriverId INNER JOIN
                         dbo.AccounType ON dbo.ProcedureByDriver.AccounTypeId = dbo.AccounType.AccounTypeId INNER JOIN
                         dbo.Bank ON dbo.ProcedureByDriver.BankId = dbo.Bank.BankId INNER JOIN
                         dbo.SuperTransp_ProcedureBankAccountDetail ON dbo.ProcedureByDriver.ProcedureBankAccountId = dbo.SuperTransp_ProcedureBankAccountDetail.ProcedureBankAccountId INNER JOIN
                         dbo.ProcedureStatus ON dbo.ProcedureByDriver.ProcedureStatusId = dbo.ProcedureStatus.ProcedureStatusId INNER JOIN
                         dbo.[Procedure] ON dbo.ProcedureByDriver.ProcedureId = dbo.[Procedure].ProcedureId INNER JOIN
                         dbo.ProcedureCategory ON dbo.[Procedure].ProcedureCategoryId = dbo.ProcedureCategory.ProcedureCategoryId
GO
