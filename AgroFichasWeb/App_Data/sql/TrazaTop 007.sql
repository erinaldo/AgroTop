USE AgroFichas
GO

CREATE PROCEDURE TRZ_CrearContrato
	@IdSolicitudContrato INT,
	@UserIns [varchar](50),
	@FechaHoraIns [datetime],
	@IpIns [varchar](50)
AS
DECLARE @IdCultivo INT
DECLARE @IdTipoContrato INT
DECLARE @IdComuna INT
DECLARE @IdSucursal INT
DECLARE @IdTemporada INT
DECLARE @IdEmpresa INT
DECLARE @IdAgricultor INT

DECLARE @IdContrato INT

SELECT @IdCultivo = [IdCultivo]
      ,@IdTipoContrato = [IdTipoContrato]
      ,@IdComuna = [IdComunaOrigen]
      ,@IdSucursal = [IdSucursalEntrega]
      ,@IdTemporada = [IdTemporada]
      ,@IdEmpresa = [IdEmpresa]
      ,@IdAgricultor = [IdAgricultor]
  FROM [dbo].[SolicitudContrato]
 WHERE [IdSolicitudContrato] = @IdSolicitudContrato

INSERT INTO [dbo].[Contrato]
           ([IdEmpresa]
           ,[NumeroContrato]
           ,[IdAgricultor]
           ,[IdTemporada]
           ,[Habilitado]
           ,[Comentarios]
           ,[UserIns]
           ,[FechaHoraIns]
           ,[IpIns]
           ,[IdTipoContrato]
           ,[IdComuna]
           ,[IdSucursal])
     VALUES
           (@IdEmpresa
           ,CONCAT('ForceManager-', @IdSolicitudContrato)
           ,@IdAgricultor
           ,@IdTemporada
           ,1
           ,CONCAT('Solicitud de Contrato Nº ', @IdSolicitudContrato, ' desde ForceManager CRM')
           ,@UserIns
           ,@FechaHoraIns
           ,@IpIns
           ,@IdTipoContrato
           ,@IdComuna
           ,@IdSucursal)

SELECT TOP 1 @IdContrato = [IdContrato]
  FROM [dbo].[Contrato]
 ORDER BY [IdContrato] DESC

INSERT INTO [dbo].[ItemContrato]
           ([IdContrato]
           ,[IdCultivoContrato]
           ,[Cantidad]
           ,[Superficie]
           ,[UserIns]
           ,[FechaHoraIns]
           ,[IpIns])
SELECT @IdContrato
      ,CC.IdCultivoContrato
      ,CAST((SCV.[Toneladas]*1000) AS int)
      ,SCV.[Hectareas]
      ,@UserIns
      ,@FechaHoraIns
      ,@IpIns
  FROM [dbo].[SolicitudContratoVariedad] SCV
  JOIN [dbo].[CultivoContrato] CC ON SCV.IdVariedad = CC.IdForceManagerCRM
 WHERE [IdSolicitudContrato] = @IdSolicitudContrato

UPDATE [dbo].[SolicitudContrato] 
   SET [IdContrato] = @IdContrato,
       [ContratoCreado] = 1
 WHERE [IdSolicitudContrato] = @IdSolicitudContrato
GO