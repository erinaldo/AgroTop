USE agrofichas
GO

INSERT [SYS_Permiso] VALUES (1034,'Crear solicitud de contrato',4006,'jfersep',getdate(),'localhost',4)
INSERT [SYS_Permiso] VALUES (1035,'Editar solicitud de contrato',4007,'jfersep',getdate(),'localhost',4)
INSERT [SYS_Permiso] VALUES (1036,'Eliminar solicitud de contrato',4008,'jfersep',getdate(),'localhost',4)
GO

INSERT [SYS_Permiso] VALUES (1037,'Crear planilla de contrato',4012,'jfersep',getdate(),'localhost',4)
INSERT [SYS_Permiso] VALUES (1038,'Editar planilla de contrato',4013,'jfersep',getdate(),'localhost',4)
INSERT [SYS_Permiso] VALUES (1039,'Eliminar planilla de contrato',4014,'jfersep',getdate(),'localhost',4)
GO

CREATE TABLE PlanillaContrato
(
	IdPlanillaContrato INT IDENTITY(1,1) NOT NULL,
	Documento  [varchar](50) NOT NULL,
	DocumentoM NTEXT NOT NULL,
	Observacion [varchar](150) NOT NULL,
	[IdTemporada] [int] NOT NULL,
	[IdTipoContrato] [int] NULL,
	[IdCultivo] [int] NULL,
	UserIns varchar(50) NOT NULL,
	FechaHoraIns datetime NOT NULL,
	IpIns varchar(50) NOT NULL,
	UserUpd varchar(50) NULL,
	FechaHoraUpd datetime NULL,
	IpUpd varchar(50) NULL,
	CONSTRAINT [PK_PlanillaContrato] PRIMARY KEY (IdPlanillaContrato),
	CONSTRAINT [FK_PlanillaContrato_Temporada] FOREIGN KEY ([IdTemporada]) REFERENCES Temporada,
	CONSTRAINT [FK_PlanillaContrato_TipoContrato] FOREIGN KEY ([IdTipoContrato]) REFERENCES TipoContrato,
	CONSTRAINT [FK_PlanillaContrato_Cultivo] FOREIGN KEY ([IdCultivo]) REFERENCES Cultivo,
)
GO

ALTER TABLE Temporada ADD
	IdForceManagerCRM INT NULL
GO

INSERT INTO Temporada VALUES ('Temporada 2021-2022', 0, 'jfersep', GETDATE(), 'localhost', NULL, NULL, NULL, 0, NULL)
GO

UPDATE Temporada SET IdForceManagerCRM = 1 WHERE IdTemporada = 7
UPDATE Temporada SET IdForceManagerCRM = 2 WHERE IdTemporada = 8
UPDATE Temporada SET IdForceManagerCRM = 3 WHERE IdTemporada = 9
GO

ALTER TABLE SolicitudContrato ADD
	Variedad VARCHAR(1000) NULL
GO

ALTER TABLE Temporada ADD
	IdForceManagerCRM INT NULL
GO

ALTER TABLE Temporada ADD
	NombreCorto [varchar](50) NOT NULL DEFAULT('')
GO

UPDATE Temporada SET NombreCorto = '2013-14' WHERE IdTemporada = 1
UPDATE Temporada SET NombreCorto = '2014-15' WHERE IdTemporada = 2
UPDATE Temporada SET NombreCorto = '2015-16' WHERE IdTemporada = 3
UPDATE Temporada SET NombreCorto = '2016-17' WHERE IdTemporada = 4
UPDATE Temporada SET NombreCorto = '2017-18' WHERE IdTemporada = 5
UPDATE Temporada SET NombreCorto = '2018-19' WHERE IdTemporada = 6
UPDATE Temporada SET NombreCorto = '2019-20' WHERE IdTemporada = 7
UPDATE Temporada SET NombreCorto = '2020-21' WHERE IdTemporada = 8
UPDATE Temporada SET NombreCorto = '2021-22' WHERE IdTemporada = 9
GO

ALTER TABLE Empresa ADD
	RazonSocial [varchar](150) NOT NULL DEFAULT(''),
	Rut [varchar](10) NOT NULL DEFAULT('')
GO

UPDATE Empresa
   SET RazonSocial = 'AVENATOP S.A.',
       Rut = '76035224-1'
 WHERE IdEmpresa = 2
GO