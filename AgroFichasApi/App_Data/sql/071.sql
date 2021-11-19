use agrofichas
GO

update SYS_Version set CurrentVersion = 71
GO

create table TipoFichaCultivo
(
	IdTipoFichaCultivo int identity(1,1) not null,
	IdTipoFicha int not null,
	IdCultivo int not null,

	constraint [PK_TipoFichaCultivo] primary key (IdTipoFichaCultivo),
	constraint [AK_TipoFichaCultivo_1] unique (IdTipoFicha, IdCultivo),
	constraint [FK_TipoFichaCultivo_Cultivo] foreign key (IdCultivo) references Cultivo on delete cascade,
	constraint [FK_TipoFichaCultivo_TipoFicha] foreign key (IdTipoFicha) references TipoFicha on delete cascade
)
GO

--Raps, trigo y avena
insert into TipoFichaCultivo (IdTipoFicha, IdCultivo)
	select IdTipoFicha, IdCultivo
	  from TipoFicha a inner join
	       Cultivo   b on 1 = 1
     where b.IdCultivo in(1,2, 3)

--Lupino
insert into TipoFicha  values(8, 'Siembra – Emergencia'                     , 101, 'cdonoso', getdate(), '127.0.0.1', null, null, null)
insert into TipoFicha  values(9, 'Desarrollo de hojas – elongación de tallo', 102, 'cdonoso', getdate(), '127.0.0.1', null, null, null)
insert into TipoFicha  values(10, 'Floración – maduración de vainas'        , 103, 'cdonoso', getdate(), '127.0.0.1', null, null, null)
GO

insert into TipoFichaCultivo values(8, 4)
insert into TipoFichaCultivo values(9, 4)
insert into TipoFichaCultivo values(10, 4)
insert into TipoFichaCultivo values(7, 4)
GO

--Linaza
insert into TipoFicha  values(11, 'Primer par de hojas – elongación de tallo', 202, 'cdonoso', getdate(), '127.0.0.1', null, null, null)
insert into TipoFicha  values(12, 'Floración – llenado de cápsulas'         , 203, 'cdonoso', getdate(), '127.0.0.1', null, null, null)
GO

insert into TipoFichaCultivo values(8, 16)
insert into TipoFichaCultivo values(11, 16)
insert into TipoFichaCultivo values(12, 16)
insert into TipoFichaCultivo values(7, 16)
GO

CREATE TABLE [dbo].[FichaPreSiembra](
	[IdFichaPreSiembra] [int] IDENTITY(1,1) NOT NULL,
	[IdPredio] [int] NOT NULL,
	[IdTemporada] [int] NOT NULL,
	[Fecha] [datetime] NOT NULL,
	[Observaciones] [varchar](1000) NOT NULL,
	[MobileTag] [varchar](50) NOT NULL,
	[UserIns] [varchar](50) NOT NULL,
	[FechaHoraIns] [datetime] NOT NULL,
	[IpIns] [varchar](50) NOT NULL,
	[UserUpd] [varchar](50) NULL,
	[FechaHoraUpd] [datetime] NULL,
	[IpUpd] [varchar](50) NULL,
 CONSTRAINT [PK_FichaPreSiembra] PRIMARY KEY CLUSTERED 
(
	[IdFichaPreSiembra] ASC
)
)

GO

ALTER TABLE [dbo].[FichaPreSiembra]  WITH CHECK ADD  CONSTRAINT [FK_FichaPreSiembra_Predio] FOREIGN KEY([IdPredio])
REFERENCES [dbo].[Predio] ([IdPredio])
GO

ALTER TABLE [dbo].FichaPreSiembra CHECK CONSTRAINT [FK_FichaPreSiembra_Predio]
GO

ALTER TABLE [dbo].[FichaPreSiembra]  WITH CHECK ADD  CONSTRAINT [FK_FichaPreSiembra_Temporada] FOREIGN KEY([IdTemporada])
REFERENCES [dbo].[Temporada] ([IdTemporada])
GO

ALTER TABLE [dbo].[FichaPreSiembra] CHECK CONSTRAINT [FK_FichaPreSiembra_Temporada]
GO




CREATE TABLE [dbo].[FichaPreSiembraPotrero](
	[IdFichaPreSiembraPotrero] [int] IDENTITY(1,1) NOT NULL,
	[IdPotrero] [int] NOT NULL,
	[IdFichaPreSiembra] [int] NOT NULL,
	[MobileTag] [varchar](50) NOT NULL,
	[UserIns] [varchar](50) NOT NULL,
	[FechaHoraIns] [datetime] NOT NULL,
	[IpIns] [varchar](50) NOT NULL,
	[UserUpd] [varchar](50) NULL,
	[FechaHoraUpd] [datetime] NULL,
	[IpUpd] [varchar](50) NULL,
 CONSTRAINT [PK_FichaPreSiembraPotrero] PRIMARY KEY CLUSTERED 
(
	[IdFichaPreSiembraPotrero] ASC
),
 CONSTRAINT [UI_FichaPreSiembraPotrero_PotreroFichaPreSiembra] UNIQUE NONCLUSTERED 
(
	[IdPotrero] ASC,
	[IdFichaPreSiembra] ASC
)
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[FichaPreSiembraPotrero]  WITH CHECK ADD  CONSTRAINT [FK_FichaPreSiembraPotrero_FichaPreSiembra] FOREIGN KEY([IdFichaPreSiembra])
REFERENCES [dbo].[FichaPreSiembra] ([IdFichaPreSiembra])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[FichaPreSiembraPotrero] CHECK CONSTRAINT [FK_FichaPreSiembraPotrero_FichaPreSiembra]
GO

ALTER TABLE [dbo].[FichaPreSiembraPotrero]  WITH CHECK ADD  CONSTRAINT [FK_FichaPreSiembraPotrero_Potrero] FOREIGN KEY([IdPotrero])
REFERENCES [dbo].[Potrero] ([IdPotrero])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[FichaPreSiembraPotrero] CHECK CONSTRAINT [FK_FichaPreSiembraPotrero_Potrero]
GO




CREATE TABLE [dbo].[FotoFichaPreSiembra](
	[IdFotoFichaPreSiembra] [int] IDENTITY(1,1) NOT NULL,
	[IdFichaPreSiembra] [int] NOT NULL,
	[FileName] [varchar](100) NOT NULL,
	[MobileTag] [varchar](50) NOT NULL,
	[UserIns] [varchar](50) NOT NULL,
	[FechaHoraIns] [datetime] NOT NULL,
	[IpIns] [varchar](50) NOT NULL,
	[UserUpd] [varchar](50) NULL,
	[FechaHoraUpd] [datetime] NULL,
	[IpUpd] [varchar](50) NULL,
	[Observaciones] [varchar](500) NOT NULL,
 CONSTRAINT [PK_FotoFichaPreSiembra] PRIMARY KEY CLUSTERED 
(
	[IdFotoFichaPreSiembra] ASC
)
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[FotoFichaPreSiembra] ADD  DEFAULT ('') FOR [Observaciones]
GO

ALTER TABLE [dbo].[FotoFichaPreSiembra]  WITH CHECK ADD  CONSTRAINT [FK_FotoFichaPreSiembra_FichaPreSiembra] FOREIGN KEY([IdFichaPreSiembra])
REFERENCES [dbo].[FichaPreSiembra] ([IdFichaPreSiembra])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[FotoFichaPreSiembra] CHECK CONSTRAINT [FK_FotoFichaPreSiembra_FichaPreSiembra]
GO



CREATE TABLE [dbo].[RecomendacionPreSiembra](
	[IdRecomendacionPreSiembra] [int] IDENTITY(1,1) NOT NULL,
	[IdFichaPreSiembra] [int] NOT NULL,
	[IdQuimico] [int] NOT NULL,
	[Dosis] [decimal](10, 4) NOT NULL,
	[IdUM] [int] NOT NULL,
	[FechaAplicacion] [datetime] NULL,
	[FerN] [decimal](10, 4) NOT NULL,
	[FerP2O5] [decimal](10, 4) NOT NULL,
	[FerKO2] [decimal](10, 4) NOT NULL,
	[FerMgO] [decimal](10, 4) NOT NULL,
	[FerS] [decimal](10, 4) NOT NULL,
	[FerB] [decimal](10, 4) NOT NULL,
	[FerZn] [decimal](10, 4) NOT NULL,
	[FerCaO] [decimal](10, 4) NOT NULL,
	[MobileTag] [varchar](50) NOT NULL,
	[UserIns] [varchar](50) NOT NULL,
	[FechaHoraIns] [datetime] NOT NULL,
	[IpIns] [varchar](50) NOT NULL,
	[UserUpd] [varchar](50) NULL,
	[FechaHoraUpd] [datetime] NULL,
	[IpUpd] [varchar](50) NULL,
 CONSTRAINT [PK_RecomendacionPreSiembra] PRIMARY KEY CLUSTERED 
(
	[IdRecomendacionPreSiembra] ASC
)
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[RecomendacionPreSiembra]  WITH CHECK ADD  CONSTRAINT [FK_RecomendacionPreSiembra_UM] FOREIGN KEY([IdUM])
REFERENCES [dbo].[UM] ([IdUM])
GO

ALTER TABLE [dbo].[RecomendacionPreSiembra] CHECK CONSTRAINT [FK_RecomendacionPreSiembra_UM]
GO

ALTER TABLE [dbo].[RecomendacionPreSiembra]  WITH CHECK ADD  CONSTRAINT [FK_RecomendacionPreSiembra_FichaPreSiembra] FOREIGN KEY([IdFichaPreSiembra])
REFERENCES [dbo].[FichaPreSiembra] ([IdFichaPreSiembra])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[RecomendacionPreSiembra] CHECK CONSTRAINT [FK_RecomendacionPreSiembra_FichaPreSiembra]
GO

ALTER TABLE [dbo].[RecomendacionPreSiembra]  WITH CHECK ADD  CONSTRAINT [FK_RecomendacionPreSiembra_Quimico] FOREIGN KEY([IdQuimico])
REFERENCES [dbo].[Quimico] ([IdQuimico])
GO

ALTER TABLE [dbo].[RecomendacionPreSiembra] CHECK CONSTRAINT [FK_RecomendacionPreSiembra_Quimico]
GO

ALTER TABLE [dbo].[RecomendacionPreSiembra]  WITH CHECK ADD  CONSTRAINT [CK_TipoRecomendacionPreSiembra] CHECK  (([Dosis]<>(0) AND [FerN]=(0) AND [FerP2O5]=(0) AND [FerKO2]=(0) AND [FerMgO]=(0) AND [FerS]=(0) AND [FerB]=(0) AND [FerZn]=(0) AND [FerCaO]=(0) OR [Dosis]=(0) AND ((((((([FerN]+[FerP2O5])+[FerKO2])+[FerMgO])+[FerS])+[FerB])+[FerZn])+[FerCaO])<>(0)))
GO

ALTER TABLE [dbo].[RecomendacionPreSiembra] CHECK CONSTRAINT [CK_TipoRecomendacionPreSiembra]
GO



create procedure [dbo].[GetDestinatariosFichaPreSiembra]
(
	@IdFichaPreSiembra int
)
as

	select a.Email
	  from SYS_User           a inner join
	       SYS_PermisoUsuario b on a.UserName = b.UserName
     where b.IdPermiso = 8

union

    select b.Email
	  from FichaPreSiembra a inner join
	       SYS_User b on a.UserIns = b.UserName
	 where a.IdFichaPreSiembra =  @IdFichaPreSiembra

union 
	 
    select e.Email
	  from FichaPreSiembra   a inner join
		   Predio            b on a.IdPredio = b.IdPredio inner join
		   Agricultor        c on b.IdAgricultor = c.IdAgricultor inner join
		   UsuarioAgricultor d on c.IdAgricultor = d.IdAgricultor inner join
	       SYS_User          e on d.UserID = e.UserID
	 where a.IdFichaPreSiembra =  @IdFichaPreSiembra

GO


CREATE TABLE [dbo].[LecturaMailFichaPreSiembra](
	[IdLecturaMailFichaPreSiembra] [int] IDENTITY(1,1) NOT NULL,
	[IdFichaPreSiembra] [int] NOT NULL,
	[UserIns] [varchar](50) NOT NULL,
	[FechaHoraIns] [datetime] NOT NULL,
	[IpIns] [varchar](50) NOT NULL,
	[UserUpd] [varchar](50) NULL,
	[FechaHoraUpd] [datetime] NULL,
	[IpUpd] [varchar](50) NULL,
 CONSTRAINT [PK_LecturaMailFichaPreSiembra] PRIMARY KEY CLUSTERED 
(
	[IdLecturaMailFichaPreSiembra] ASC
)
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[LecturaMailFichaPreSiembra]  WITH CHECK ADD  CONSTRAINT [FK_LectutaMailFichaPreSiembra_FichaPreSiembra] FOREIGN KEY([IdFichaPreSiembra])
REFERENCES [dbo].[FichaPreSiembra] ([IdFichaPreSiembra])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[LecturaMailFichaPreSiembra] CHECK CONSTRAINT [FK_LectutaMailFichaPreSiembra_FichaPreSiembra]
GO




alter VIEW [dbo].[vw_FichasInbox]
as
	SELECT 'Ficha' AS Tipo, a.IdFicha AS ID, 'Ficha ' + b.Nombre AS Descripcion,
	        CASE WHEN a.FechaHoraUpd IS NOT NULL THEN a.UserUpd ELSE a.UserIns END AS Usuario,
			ISNULL(a.FechaHoraUpd, a.FechaHoraIns) AS FechaHora, c.Nombre AS Predio, d.Nombre AS Agricultor,
			d.IdAgricultor, c.IdPredio,
			(select count(*) from LecturaMailFicha x where x.IdFicha = a.IdFicha) as Descargas,
			a.IdTemporada, e.Nombre as NombreTemporada
	  FROM dbo.Ficha      a INNER JOIN   
	       dbo.TipoFicha  b ON a.IdTipoFicha = b.IdTipoFicha INNER JOIN
           dbo.Predio     c ON a.IdPredio = c.IdPredio INNER JOIN
           dbo.Agricultor d ON c.IdAgricultor = d.IdAgricultor inner join 
		   Temporada      e on a.IdTemporada = e.IdTemporada


UNION ALL

    SELECT 'Siembra' AS Tipo, a.IdSiembra AS ID, 'Datos Siembra' AS Descripcion,
	        CASE WHEN a.FechaHoraUpd IS NOT NULL THEN a.UserUpd ELSE a.UserIns END AS Usuario,
			ISNULL(a.FechaHoraUpd, a.FechaHoraIns) AS FechaHora, c.Nombre AS Predio, d.Nombre AS Agricultor,
	        d.IdAgricultor, c.IdPredio, null as Descargas,
			a.IdTemporada, e.Nombre as NombreTemporada
	  FROM dbo.Siembra   a INNER JOIN
           dbo.Predio     c ON a.IdPredio = c.IdPredio INNER JOIN
           dbo.Agricultor d ON c.IdAgricultor = d.IdAgricultor inner join 
		   Temporada      e on a.IdTemporada = e.IdTemporada

UNION ALL

	SELECT 'Ficha Pre-Siembra' AS Tipo, a.IdFichaPreSiembra AS ID, 'Ficha Pre-Siembra' as Descripcion,
	        CASE WHEN a.FechaHoraUpd IS NOT NULL THEN a.UserUpd ELSE a.UserIns END AS Usuario,
			ISNULL(a.FechaHoraUpd, a.FechaHoraIns) AS FechaHora, c.Nombre AS Predio, d.Nombre AS Agricultor,
			d.IdAgricultor, c.IdPredio,
			(select count(*) from LecturaMailFichaPreSiembra x where x.IdFichaPreSiembra = a.IdFichaPreSiembra) as Descargas,
			a.IdTemporada, e.Nombre as NombreTemporada
	  FROM dbo.FichaPreSiembra a INNER JOIN   
           dbo.Predio          c ON a.IdPredio = c.IdPredio INNER JOIN
           dbo.Agricultor      d ON c.IdAgricultor = d.IdAgricultor inner join 
		   Temporada           e on a.IdTemporada = e.IdTemporada

GO
--

create table IntencionSiembra
(
	IdIntencionSiembra int identity(1,1) not null,
	IdAgricultor int not null,
	[IdTemporada] [int] NOT NULL,
	IdCultivo int not null,
	IdComuna int not null,
	PuntoEntrega varchar(50) not null,
	Cantidad int not null,
	Superficie int not null,
	[Observaciones] [varchar](1000) NOT NULL,
	[MobileTag] [varchar](50) NOT NULL,
	[UserIns] [varchar](50) NOT NULL,
	[FechaHoraIns] [datetime] NOT NULL,
	[IpIns] [varchar](50) NOT NULL,
	[UserUpd] [varchar](50) NULL,
	[FechaHoraUpd] [datetime] NULL,
	[IpUpd] [varchar](50) NULL,

	constraint [PK_IntencionSiembra] primary key (IdIntencionSiembra),
	constraint [FK_IntencionSiembra_Agricultor] foreign key (IdAgricultor) references Agricultor,
	constraint [FK_IntencionSiembra_Temporada] foreign key (IdTemporada) references Temporada,
	constraint [FK_IntencionSiembra_Cultivo] foreign key (IdCultivo) references Cultivo,
	constraint [FK_IntencionSiembra_Comuna] foreign key (IdComuna) references Comuna
)
GO

insert into SYS_Permiso values(1002, 'Ver intenciones de siembra', 4310, 'cdonosom', getdate(), '127.0.0.1', 4)
insert into SYS_Permiso values(1003, 'Crear intenciones de siembra', 4320, 'cdonosom', getdate(), '127.0.0.1', 4)
insert into SYS_Permiso values(1004, 'Editar intenciones de siembra', 4330, 'cdonosom', getdate(), '127.0.0.1', 4)
insert into SYS_Permiso values(1005, 'Eliminar intenciones de siembra', 4340, 'cdonosom', getdate(), '127.0.0.1', 4)

insert into SYS_Menu values (1001, 4, 1002, 'Intención de Siembra', '~/intencionsiembra', 4007)
GO


select * from SYS_Menu where IdModulo = 4 order by Orden
select * from SYS_Permiso order by IdModulo, Orden