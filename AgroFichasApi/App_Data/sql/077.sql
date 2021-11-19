use agrofichas
GO

update SYS_Version set CurrentVersion = 77
GO


create table ConvenioPrecioSucursal
(
	IdConvenioPrecio int not null,
	IdSucursal      int not null,

	[UserIns] [varchar](50) NOT NULL,
	[FechaHoraIns] [datetime] NOT NULL,
	[IpIns] [varchar](50) NOT NULL,
	[UserUpd] [varchar](50) NULL,
	[FechaHoraUpd] [datetime] NULL,
	[IpUpd] [varchar](50) NULL,

	constraint [PK_ConvenioPrecioSucursal] primary key (IdConvenioPrecio, IdSucursal),
	constraint [FK_ConvenioPrecioSucursal_ConvenioPrecio] foreign key (IdConvenioPrecio) references ConvenioPrecio on delete cascade,
	constraint [FK_ConvenioPrecioSucursal_Sucursal] foreign key (IdSucursal) references Sucursal,
)
GO

delete from ConvenioPrecioSucursal
GO

insert into ConvenioPrecioSucursal (IdConvenioPrecio, IdSucursal, UserIns, FechaHoraIns, IpIns)
	select b.IdConvenioPrecio, a.IdSucursal, 'cdonoso', getdate(), '127.0.0.1'
	  from Sucursal a inner join
	       ConvenioPrecio b on 1 = 1
GO


create table MotivoAjustePrecio
(
	IdMotivoAjustePrecio int not null,
	Nombre varchar(100) not null,

	PermiteBono bit not null,
	PermiteDescuento bit not null,

	[UserIns] [varchar](50) NOT NULL,
	[FechaHoraIns] [datetime] NOT NULL,
	[IpIns] [varchar](50) NOT NULL,
	[UserUpd] [varchar](50) NULL,
	[FechaHoraUpd] [datetime] NULL,
	[IpUpd] [varchar](50) NULL,

	constraint [PK_MotivoBonoPrecio] primary key (IdMotivoAjustePrecio)
)
GO

insert into MotivoAjustePrecio values(1, 'Flete', 1, 1, 'cdonoso', getdate(), '127.0.0.1', null, null, null)
insert into MotivoAjustePrecio values(2, 'Cambio punto de entrega', 1, 1, 'cdonoso', getdate(), '127.0.0.1', null, null, null)
insert into MotivoAjustePrecio values(99, 'Otro', 1, 1, 'cdonoso', getdate(), '127.0.0.1', null, null, null)
GO

create table ConvenioPrecioAjuste
(
	IdConvenioPrecioAjuste int identity(1,1) not null,
	IdConvenioPrecio int not null,
	IdMotivoAjustePrecio int not null,

	Cantidad int not null,
	PrecioUnidad decimal(13,5) not null,

	Comentarios text not null,

	[UserIns] [varchar](50) NOT NULL,
	[FechaHoraIns] [datetime] NOT NULL,
	[IpIns] [varchar](50) NOT NULL,
	[UserUpd] [varchar](50) NULL,
	[FechaHoraUpd] [datetime] NULL,
	[IpUpd] [varchar](50) NULL,

	constraint [PK_ConvenioPrecioAjuste] primary key (IdConvenioPrecioAjuste),
	constraint [FK_ConvenioPrecioAjuste_ConvenioPrecio] foreign key (IdConvenioPrecio) references ConvenioPrecio on delete cascade,
	constraint [FK_ConvenioPrecioAjuste_MotivoAjustePrecio] foreign key (IdMotivoAjustePrecio) references MotivoAjustePrecio
)
GO

create table ConvenioPrecioAjusteSucursal
(
	IdConvenioPrecioAjuste int not null,
	IdSucursal      int not null,

	[UserIns] [varchar](50) NOT NULL,
	[FechaHoraIns] [datetime] NOT NULL,
	[IpIns] [varchar](50) NOT NULL,
	[UserUpd] [varchar](50) NULL,
	[FechaHoraUpd] [datetime] NULL,
	[IpUpd] [varchar](50) NULL,

	constraint [PK_ConvenioPrecioAjusteSucursal] primary key (IdConvenioPrecioAjuste, IdSucursal),
	constraint [FK_ConvenioPrecioAjusteSucursal_ConvenioPrecio] foreign key (IdConvenioPrecioAjuste) references ConvenioPrecioAjuste on delete cascade,
	constraint [FK_ConvenioPrecioAjusteSucursal_Sucursal] foreign key (IdSucursal) references Sucursal,
)
GO


create procedure dbo.SelectSucursalesConvenioPrecio
(
	@IdConvenioPrecio int
)
as

	select a.IdSucursal, a.Nombre,
	       cast(case when b.IdConvenioPrecio is not null then 1 else 0 end as bit) as Selected
	  from Sucursal               a left join
	       ConvenioPrecioSucursal b on a.IdSucursal = b.IdSucursal and b.IdConvenioPrecio = @IdConvenioPrecio
     where a.Habilitada = 1 or b.IdConvenioPrecio is not null
  order by a.Nombre

GO

create procedure dbo.SelectSucursalesConvenioPrecioAjuste
(
	@IdConvenioPrecioAjuste int
)
as

	select a.IdSucursal, a.Nombre,
	       cast(case when b.IdConvenioPrecioAjuste is not null then 1 else 0 end as bit) as Selected
	  from Sucursal                     a left join
	       ConvenioPrecioAjusteSucursal b on a.IdSucursal = b.IdSucursal and b.IdConvenioPrecioAjuste = @IdConvenioPrecioAjuste
     where a.Habilitada = 1 or b.IdConvenioPrecioAjuste is not null
  order by a.Nombre

GO

insert into SYS_Permiso values(1011, 'Recibir email notificación actualización convenio de precios', 4085, 'cdonoso', getdate(), '127.0.0.1', 4)
GO

create procedure [dbo].[ReceptoresNotificacionConvenioPrecio]
(
	@IdConvenioPrecio int
)
as
	declare @IdAgricultor int

	set @IdAgricultor = (select b.IdAgricultor 
	                       from ConvenioPrecio a inner join
						        Contrato       b on a.IdContrato = b.IdContrato
						  where a.IdConvenioPrecio = @IdConvenioPrecio)


	select 'Agricultor' as Rol, a.Nombre, a.Email
	  from Agricultor a 
	 where a.IdAgricultor = @IdAgricultor

union all

	select 'Asesor' as Rol, a.FullName as Nombre, a.Email
	  from SYS_User a inner join
	       UsuarioAgricultor b on a.UserID = b.UserID
	 where a.Disabled = 0
	   and b.IdAgricultor = @IdAgricultor

union all

	select 'Administrativo' as Rol, a.Email, a.FullName
	  from SYS_User a inner join
	       SYS_PermisoUsuario b on a.UserName = b.UserName 
	 where a.Disabled = 0
	   and b.IdPermiso = 1011

GO

create table dbo.NivelRangoPrecio
(
	IdNivelRangoPrecio int not null,
	Nombre varchar(50) not null,

	constraint [PK_NivelRangoPrecio] primary key (IdNivelRangoPrecio)
)
GO

insert into NivelRangoPrecio values (1, 'Gerencia')
insert into NivelRangoPrecio values (2, 'Operaciones')
go

update NivelRangoPrecio set Nombre = 'Abastecimiento' where IdNivelRangoPrecio = 2
GO


alter table NivelRangoPrecio 
	add IdNivelRangoPrecioPadre int null
GO

update NivelRangoPrecio set IdNivelRangoPrecioPadre = 1 where IdNivelRangoPrecio = 2
GO


create table dbo.RangoPrecio
(
	IdSucursal int not null,
	IdCultivo int not null,
	IdNivelRangoPrecio int not null,
	IdMoneda int not null, 
	PrecioMin decimal(13, 5) null,
	PrecioMax decimal(13, 5) null,

	[UserIns] [varchar](50) NOT NULL,
	[FechaHoraIns] [datetime] NOT NULL,
	[IpIns] [varchar](50) NOT NULL,
	[UserUpd] [varchar](50) NULL,
	[FechaHoraUpd] [datetime] NULL,
	[IpUpd] [varchar](50) NULL,

	constraint [PK_RangoPrecio] primary key (IdSucursal, IdCultivo, IdNivelRangoPrecio, IdMoneda),
	constraint [FK_RangoPrecio_Sucursal] foreign key (IdSucursal) references Sucursal on delete cascade,
	constraint [FK_RangoPrecio_Cultivo] foreign key (IdCultivo) references Cultivo on delete cascade,
	constraint [FK_RangoPrecio_NivelRangoPrecio] foreign key (IdNivelRangoPrecio) references NivelRangoPrecio,
	constraint [Fk_RangoPrecio_Moneda] foreign key (IdMoneda) references Moneda
)
GO


create alter procedure GetRangosPrecio
(
	@IdNivelRangoPrecio int
)
as

	declare @IdNivelRangoPrecioPadre int 
	set @IdNivelRangoPrecioPadre = (select IdNivelRangoPrecioPadre from NivelRangoPrecio where IdNivelRangoPrecio = @IdNivelRangoPrecio)

	declare @Cultivo table (IdCultivo int not null primary key)

	insert into @Cultivo (IdCultivo)
		select distinct a.IdCultivo
		  from CultivoContrato a
	  union
		select distinct a.IdCultivo
		  from RangoPrecio a
		 where a.IdNivelRangoPrecio = @IdNivelRangoPrecio
	
	declare @Sucursal table (IdSucursal int not null primary key)

	insert into @Sucursal	
		select a.IdSucursal
		 from Sucursal a 
		where a.Habilitada = 1
	  union
	    select distinct a.IdSucursal
		  from RangoPrecio a 
		 where a.IdNivelRangoPrecio = @IdNivelRangoPrecio


	select a.IdCultivo, d.Nombre as NombreCultivo, b.IdSucursal, e.Nombre as NombreSucursal,
	       @IdNivelRangoPrecio as IdNivelRangoPrecio,
		   m.PrecioMin as PrecioMinCLP, m.PrecioMax as PrecioMaxCLP,
		   n.PrecioMin as PrecioMinUSD, n.PrecioMax as PrecioMaxUSD,
		   o.PrecioMin as PisoCLP, o.PrecioMax as TechoCLP,
		   p.PrecioMin as PisoUSD, p.PrecioMax as TechoUSD

	  from @Cultivo    a cross join
	       @Sucursal   b inner join
		   Cultivo     d on a.IdCultivo = d.IdCultivo inner join
		   Sucursal    e on b.IdSucursal = e.IdSucursal left join

		   --CLP
		   RangoPrecio m on a.IdCultivo = m.IdCultivo 
		                and b.IdSucursal = m.IdSucursal 
						and m.IdNivelRangoPrecio = @IdNivelRangoPrecio
						and m.IdMoneda = 1              left join
		   --USD
		   RangoPrecio n on a.IdCultivo = n.IdCultivo 
		                and b.IdSucursal = n.IdSucursal 
						and n.IdNivelRangoPrecio = @IdNivelRangoPrecio
						and n.IdMoneda = 2              left join

		   --CLP RangoPadre
		   RangoPrecio o on a.IdCultivo = o.IdCultivo
		                and b.IdSucursal = o.IdSucursal
						and o.IdNivelRangoPrecio = @IdNivelRangoPrecioPadre
						and o.IdMoneda = 1              left join

		   --USD RangoPadre
		   RangoPrecio p on a.IdCultivo = p.IdCultivo
		                and b.IdSucursal = p.IdSucursal
						and p.IdNivelRangoPrecio = @IdNivelRangoPrecioPadre
						and p.IdMoneda = 2

  order by a.IdCultivo, e.Nombre, e.IdSucursal
		 
GO  

--exec GetRangosPrecio 1

insert into SYS_Permiso values (1021, 'Administrar Rango de Precios Gerencia', 4087, 'cdonoso', getdate(), '127.0.0.1', 4)
insert into SYS_Permiso values (1022, 'Administrar Rango de Precios Operaciones', 4088, 'cdonoso', getdate(), '127.0.0.1', 4)
GO

update SYS_Permiso set Nombre = 'Administrar Rango de Precios Abastecimiento' where IdPermiso = 1022
GO

insert into SYS_Permiso values (1023, 'Autorizar Precios Gerencia', 4089, 'cdonoso', getdate(), '127.0.0.1', 4)
insert into SYS_Permiso values (1024, 'Autorizar Precios Operaciones', 4089, 'cdonoso', getdate(), '127.0.0.1', 4)
GO

create table ConvenioPrecioAutorizacion
(
	IdConvenioPrecioAutorizacion int identity(1,1) not null,
	IdContrato int not null,
	IdConvenioPrecio int null,
	IdNivelAutorizacion int not null,

	[Data] text not null,

	UserIns varchar(50) not null,
	FechaHoraIns datetime not null,
	IpIns varchar(50),
	Autorizada bit null,
	UserAut varchar(50) null,
	FechaHoraAut datetime null,
	IpAut varchar(50) null,

	constraint [PK_ConvenioPrecioAutorizacion] primary key (IdConvenioPrecioAutorizacion),
	constraint [FK_ConvenioPrecioAutorizacion_ConvenioPrecio] foreign key (IdConvenioPrecio) references ConvenioPrecio,
	constraint [FK_ConvenioPrecioAutorizacion_NivelRangoPrecio] foreign key (IdNivelAutorizacion) references NivelRangoPrecio (IdNivelRangoPrecio),
	constraint [FK_ConvenioPrecioAutorizacion_Contrato] foreign key (IdContrato) references Contrato on delete cascade
)
GO



select * from EstadoLiquidacion

--delete from ConvenioPrecioAutorizacion
select * from ConvenioPrecioAutorizacion

select * from NivelRangoPrecio

select * from RangoPrecio
select * from Moneda

select * from SYS_Permiso

		   
exec GetRangosPrecio 1
		  

		  select * from MOneda



select a.IdContrato, count(distinct b.IdCultivo)
  from ItemContrato a inner join
       CultivoContrato b on a.IdCultivoContrato = b.IdCultivoContrato
group by a.IdContrato
having count(distinct b.IdCultivo) > 1



