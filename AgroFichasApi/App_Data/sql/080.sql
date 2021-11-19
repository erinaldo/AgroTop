use agrofichas
GO

update SYS_Version set CurrentVersion = 80
GO

alter procedure [dbo].[ReceptoresNotificacionConvenioPrecio]
(
	@IdConvenioPrecio int
)
as
	declare @IdAgricultor int

	set @IdAgricultor = (select b.IdAgricultor 
	                       from ConvenioPrecio a inner join
						        Contrato       b on a.IdContrato = b.IdContrato
						  where a.IdConvenioPrecio = @IdConvenioPrecio)


	select 'Agricultor' as Rol, a.Nombre, a.Email, cast(1 as bit) as Optional
	  from Agricultor a 
	 where a.IdAgricultor = @IdAgricultor

union all

	select 'Asesor' as Rol, a.FullName as Nombre, a.Email, cast(1 as bit) as Optional
	  from SYS_User a inner join
	       UsuarioAgricultor b on a.UserID = b.UserID
	 where a.Disabled = 0
	   and b.IdAgricultor = @IdAgricultor

union all

	select 'Administrativo' as Rol, a.FullName, a.Email, cast(0 as bit) as Optional
	  from SYS_User a inner join
	       SYS_PermisoUsuario b on a.UserName = b.UserName 
	 where a.Disabled = 0
	   and b.IdPermiso = 1015

GO

create  table SolicitudPrecio
(
	IdSolicitudPrecio int identity(1,1) not null,
	IdTemporada  int not null,
	IdAgricultor int not null,
	IdCultivo    int not null,
	IdMoneda int not null,
	Cantidad int not null,
	PrecioUnidad decimal(13,5) not null,
	Comentarios text not null,
	Procesado bit not null,
	
	IdConvenioPrecioAutorizacion int null,
	IdConvenioPrecio int null,

	[UserIns] [varchar](50) NOT NULL,
	[FechaHoraIns] [datetime] NOT NULL,
	[IpIns] [varchar](50) NOT NULL,
	[UserUpd] [varchar](50) NULL,
	[FechaHoraUpd] [datetime] NULL,
	[IpUpd] [varchar](50) NULL,
	[UserProc] [varchar](50) NULL,
	[FechaHoraProc] [datetime] NULL,
	[IpProc] [varchar](50) NULL,

	constraint [PK_SolicitudPrecio] primary key (IdSolicitudPrecio),
	constraint [FK_SolicitudPrecio_Moneda] foreign key (IdMoneda) references Moneda,
	constraint [FK_SolicitudPrecio_ConvenioPrecioAutorizacion] foreign key (IdConvenioPrecioAutorizacion) references ConvenioPrecioAutorizacion on delete set null,
	constraint [FK_SolicitudPrecio_ConvenioPrecio] foreign key (IdConvenioPrecio) references ConvenioPrecio on delete set null,
	constraint [FK_SolicitudPrecio_Temporada] foreign key (IdTemporada) references Temporada,
	constraint [FK_SolicitudPrecio_Agricultor] foreign key (IdAgricultor) references Agricultor,
	constraint [FK_SolicitudPrecio_Cultivo] foreign key (IdCultivo) references Cultivo
)
GO


create table SolicitudPrecioSucursal
(
	IdSolicitudPrecio int not null,
	IdSucursal int not null,

	[UserIns] [varchar](50) NOT NULL,
	[FechaHoraIns] [datetime] NOT NULL,
	[IpIns] [varchar](50) NOT NULL,
	[UserUpd] [varchar](50) NULL,
	[FechaHoraUpd] [datetime] NULL,
	[IpUpd] [varchar](50) NULL,

	constraint [PK_SolicutudPrecioSucursal] primary key (IdSolicitudPrecio, IdSucursal),
	constraint [FK_SolicutudPrecioSucursal_SolicitudPrecio] foreign key (IdSolicitudPrecio) references SolicitudPrecio on delete cascade,
	constraint [FK_SolicutudPrecioSucursal_Sucursal] foreign key (IdSucursal) references Sucursal
)
GO

update SYS_Permiso set Orden = Orden + 100 where IdModulo = 4 and Orden >= 4090
GO


insert into SYS_Permiso values(1026, 'Procesar solicitudes de precios', 4100, 'cdonosom', getdate(), '127.0.0.1', 4)
GO

insert into SYS_Permiso values(1027, 'Ver solicitudes de precios', 4095, 'cdonosom', getdate(), '127.0.0.1', 4)
GO

----
select * from Cultivo
select * from SolicitudPrecio
select * from SolicitudPrecioSucursal
select * from Sucursal

insert into SolicitudPrecio values(4, 1848, 1, 1, 30000, 75.45, 'Test', 0, null, null, 'donwoc', getdate(), '127.0.0.1', null, null, null, null, null, null)

insert into SolicitudPrecioSucursal values(6, 1,  'donwoc', getdate(), '127.0.0.1', null, null, null)

select * from SYS_Permiso order by idmodulo, orden

select * from Contrato a
where (select count(*) from ItemContrato b where b.IdContrato = a.IdContrato) = 0

update SolicitudPrecio set Procesado = 0
