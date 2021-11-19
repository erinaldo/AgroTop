USE agrofichas
GO

CREATE TABLE CRM_Usuario
(
	ID INT NOT NULL,
	Nombre [varchar](50) NOT NULL,
	Apellido [varchar](50) NOT NULL,
	Telefono [varchar](50) NOT NULL,
	Email [varchar](150) NOT NULL,
	FechaHoraIns [datetime] NOT NULL,
	FechaHoraUpd [datetime] NULL,
	FechaHoraDel [datetime] NULL,
	CONSTRAINT [PK_CRM_Usuario] PRIMARY KEY (ID)
)
GO

CREATE TABLE CRM_Solicitud
(
	ID INT IDENTITY(1,1) NOT NULL,
	Tipo varchar(50) NOT NULL,
	ApiEndpoint varchar(250) NOT NULL,
	Pagina INT NOT NULL,
	Respuesta ntext NOT NULL,
	UserIns varchar(50) NOT NULL,
	FechaHoraIns datetime NOT NULL,
	IpIns varchar(50) NOT NULL,
	CONSTRAINT [PK_CRM_Solicitud] PRIMARY KEY (ID)
)
GO

ALTER TABLE CRM_Informacion ADD
	IdUsuario1 INT NULL,
	IdUsuario2 INT NULL,
	IdUsuario3 INT NULL,
	IdUsuario4 INT NULL,
	IdUsuario5 INT NULL,
	CONSTRAINT [FK_CRM_Informacion_CRM_Usuario1] FOREIGN KEY (IdUsuario1) REFERENCES CRM_Usuario,
	CONSTRAINT [FK_CRM_Informacion_CRM_Usuario2] FOREIGN KEY (IdUsuario2) REFERENCES CRM_Usuario,
	CONSTRAINT [FK_CRM_Informacion_CRM_Usuario3] FOREIGN KEY (IdUsuario3) REFERENCES CRM_Usuario,
	CONSTRAINT [FK_CRM_Informacion_CRM_Usuario4] FOREIGN KEY (IdUsuario4) REFERENCES CRM_Usuario,
	CONSTRAINT [FK_CRM_Informacion_CRM_Usuario5] FOREIGN KEY (IdUsuario5) REFERENCES CRM_Usuario
GO

CREATE TABLE SolicitudContrato
(
	IdSolicitudContrato INT NOT NULL,
	Rut varchar(50) NOT NULL,
	NombreProveedor varchar(250) NOT NULL,
	Cultivo varchar(50) NOT NULL,
	PrecioCierre INT NOT NULL,
	ToneladasCierre INT NOT NULL,
	TipoContrato varchar(50) NOT NULL,
	ComunaOrigen varchar(50) NOT NULL,
	SucursalEntrega varchar(50) NOT NULL,
	Hectareas INT NOT NULL,
	ToneladasTotales INT NOT NULL,
	NombreAsesor varchar(250) NOT NULL,
	EmailAsesor varchar(250) NOT NULL,
	Predio varchar(50) NOT NULL,
	Verificado BIT NOT NULL,
	ContratoCreado BIT NOT NULL,
	CierreCreado BIT NOT NULL,
	UserIns varchar(50) NOT NULL,
	FechaHoraIns datetime NOT NULL,
	IpIns varchar(50) NOT NULL,
	UserUpd varchar(50) NULL,
	FechaHoraUpd datetime NULL,
	IpUpd varchar(50) NULL,
	CONSTRAINT [PK_SolicitudContrato] PRIMARY KEY (IdSolicitudContrato)
)
GO