-- Organization assembly date: the view exposes PublicTransportGroup.AssemblyDate so the edit screen can load it.
-- The column PublicTransportGroup.AssemblyDate and the @AssemblyDate parameter of
-- SuperTransp_PublicTransportGroupAddOrEdit must already exist. Idempotent.
CREATE OR ALTER VIEW dbo.SuperTransp_PublicTransportGroupDetail
AS
SELECT        dbo.PublicTransportGroup.PublicTransportGroupId, dbo.PublicTransportGroup.PublicTransportGroupRif, dbo.Designation.DesignationName,
                         dbo.Designation.DesignationName + '  ' + dbo.PublicTransportGroup.PublicTransportGroupName AS PTGCompleteName, dbo.PublicTransportGroup.PublicTransportGroupName, { fn UCASE(dbo.State.StateName) } AS StateName,
                         dbo.Municipality.MunicipalityName, dbo.Mode.ModeName, dbo.[Union].UnionName, dbo.PublicTransportGroup.ModeId, dbo.PublicTransportGroup.UnionId, dbo.PublicTransportGroup.PublicTransportGroupGUID,
                         dbo.PublicTransportGroup.MunicipalityId, dbo.PublicTransportGroup.RepresentativeIdentityDocument, dbo.PublicTransportGroup.RepresentativeName, dbo.PublicTransportGroup.RepresentativePhone,
                         ISNULL(dbo.PublicTransportGroup.RepresentativeEmail, 'N/D') AS RepresentativeEmail, dbo.PublicTransportGroup.PublicTransportGroupAddedDate, dbo.PublicTransportGroup.PublicTransportGroupIdModifiedDate,
                         dbo.State.StateId, dbo.PublicTransportGroup.DesignationId, dbo.PublicTransportGroup.Partners, ISNULL(dbo.SuperTransp_TotalPTGADrivers.TotalDrivers, '0') AS TotalDrivers,
                         ISNULL(dbo.SuperTransp_TotalPTGAndSupervisedDrivers.TotalSupervised, 0) AS TotalSupervisedDrivers, ISNULL(dbo.SupervisionSummary.SupervisionSummaryId, 0) AS SupervisionSummaryId,
                         ISNULL(dbo.SecurityUser.FullName, 'NO ASIGNADO') AS UserFullName, ISNULL(dbo.SecurityUser.SecurityUserId, 0) AS SecurityUserId, ISNULL(dbo.GasStation.GasStationName, 'N/D') AS GasStationName,
                         ISNULL(dbo.GasStation.GasStationAddress, 'N/D') AS GasStationAddress, ISNULL(dbo.PublicTransportGroup.GasStationId, 0) AS GasStationId,
                         dbo.PublicTransportGroup.AssemblyDate
FROM            dbo.PublicTransportGroup INNER JOIN
                         dbo.Mode ON dbo.PublicTransportGroup.ModeId = dbo.Mode.ModeId INNER JOIN
                         dbo.Municipality ON dbo.PublicTransportGroup.MunicipalityId = dbo.Municipality.MunicipalityId INNER JOIN
                         dbo.State ON dbo.Municipality.StateId = dbo.State.StateId INNER JOIN
                         dbo.Designation ON dbo.PublicTransportGroup.DesignationId = dbo.Designation.DesignationId INNER JOIN
                         dbo.[Union] ON dbo.PublicTransportGroup.UnionId = dbo.[Union].UnionId LEFT OUTER JOIN
                         dbo.GasStation ON dbo.PublicTransportGroup.GasStationId = dbo.GasStation.GasStationId LEFT OUTER JOIN
                         dbo.SecurityUser RIGHT OUTER JOIN
                         dbo.SuperTransp_SupervisionUsers ON dbo.SecurityUser.SecurityUserId = dbo.SuperTransp_SupervisionUsers.SecurityUserId ON
                         dbo.PublicTransportGroup.PublicTransportGroupId = dbo.SuperTransp_SupervisionUsers.PublicTransportGroupId LEFT OUTER JOIN
                         dbo.SupervisionSummary ON dbo.PublicTransportGroup.PublicTransportGroupId = dbo.SupervisionSummary.PublicTransportGroupId LEFT OUTER JOIN
                         dbo.SuperTransp_TotalPTGAndSupervisedDrivers ON dbo.PublicTransportGroup.PublicTransportGroupId = dbo.SuperTransp_TotalPTGAndSupervisedDrivers.PublicTransportGroupId LEFT OUTER JOIN
                         dbo.SuperTransp_TotalPTGADrivers ON dbo.PublicTransportGroup.PublicTransportGroupId = dbo.SuperTransp_TotalPTGADrivers.PublicTransportGroupId
GO
