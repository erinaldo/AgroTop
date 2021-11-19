USE agrofichas
GO

INSERT INTO [dbo].[TipoContrato]
           ([IdTipoContrato]
           ,[Descripcion])
     VALUES
           (4
           ,'Cierre de Precio')
GO

SELECT DISTINCT 
       C.IdCultivo,
       C.Nombre
  FROM CultivoContrato CC
  JOIN Cultivo C ON CC.IdCultivo = C.IdCultivo
 ORDER BY C.IdCultivo
GO

CREATE TABLE [dbo].[EstadoSolicitudContrato](
	[IdEstado] [int] NOT NULL,
	[Nombre] [varchar](40) NOT NULL,
	[Color] [varchar](20) NOT NULL,
 CONSTRAINT [PK_EstadoSolicitudContrato] PRIMARY KEY CLUSTERED 
(
	[IdEstado] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

INSERT INTO EstadoSolicitudContrato VALUES (1, 'No Verificado', '#ff8e73')
INSERT INTO EstadoSolicitudContrato VALUES (2, 'Verificado', '#83f03c')
INSERT INTO EstadoSolicitudContrato VALUES (3, 'Notificado', '#FFE640')
GO

ALTER TABLE SolicitudContrato ADD
	IdEstado INT NOT NULL DEFAULT(1),
	CONSTRAINT [FK_SolicitudContrato_EstadoSolicitudContrato] FOREIGN KEY (IdEstado) REFERENCES EstadoSolicitudContrato
GO

CREATE TABLE [dbo].[CRM_AsesorAgricola](
	[ID] [int] NOT NULL,
	[Nombre] [varchar](50) NOT NULL,
	[Apellido] [varchar](50) NOT NULL,
	[Telefono] [varchar](50) NOT NULL,
	[Email] [varchar](150) NOT NULL,
	[FechaHoraIns] [datetime] NOT NULL,
	[FechaHoraUpd] [datetime] NULL,
	[FechaHoraDel] [datetime] NULL,
 CONSTRAINT [PK_CRM_AsesorAgricola] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE CRM_Informacion DROP CONSTRAINT
	[FK_CRM_Informacion_CRM_Usuario1]
ALTER TABLE CRM_Informacion DROP CONSTRAINT
	[FK_CRM_Informacion_CRM_Usuario2]
ALTER TABLE CRM_Informacion DROP CONSTRAINT
	[FK_CRM_Informacion_CRM_Usuario3]
ALTER TABLE CRM_Informacion DROP CONSTRAINT
	[FK_CRM_Informacion_CRM_Usuario4]
ALTER TABLE CRM_Informacion DROP CONSTRAINT
	[FK_CRM_Informacion_CRM_Usuario5]

ALTER TABLE CRM_Informacion DROP COLUMN
	IdUsuario1
ALTER TABLE CRM_Informacion DROP COLUMN
	IdUsuario2
ALTER TABLE CRM_Informacion DROP COLUMN
	IdUsuario3
ALTER TABLE CRM_Informacion DROP COLUMN
	IdUsuario4
ALTER TABLE CRM_Informacion DROP COLUMN
	IdUsuario5

ALTER TABLE CRM_Informacion ADD
	IdAsesorAgricola1 INT NULL,
	IdAsesorAgricola2 INT NULL,
	IdAsesorAgricola3 INT NULL,
	IdAsesorAgricola4 INT NULL,
	IdAsesorAgricola5 INT NULL,
	CONSTRAINT [FK_CRM_Informacion_CRM_AsesorAgricola_01] FOREIGN KEY (IdAsesorAgricola1) REFERENCES CRM_AsesorAgricola,
	CONSTRAINT [FK_CRM_Informacion_CRM_AsesorAgricola_02] FOREIGN KEY (IdAsesorAgricola2) REFERENCES CRM_AsesorAgricola,
	CONSTRAINT [FK_CRM_Informacion_CRM_AsesorAgricola_03] FOREIGN KEY (IdAsesorAgricola3) REFERENCES CRM_AsesorAgricola,
	CONSTRAINT [FK_CRM_Informacion_CRM_AsesorAgricola_04] FOREIGN KEY (IdAsesorAgricola4) REFERENCES CRM_AsesorAgricola,
	CONSTRAINT [FK_CRM_Informacion_CRM_AsesorAgricola_05] FOREIGN KEY (IdAsesorAgricola5) REFERENCES CRM_AsesorAgricola
GO

DROP TABLE CRM_Usuario
GO