use agrofichas
GO

update SYS_Version set CurrentVersion = 79
GO

alter table Contrato
	add IdSucursal int null
GO

alter table Contrato
	add constraint [FK_Contrato_Sucursal] foreign key (IdSucursal) references Sucursal
GO

ALTER  procedure [dbo].[rpt_IntencionContratoPrecioIngresos]
(
	@IdTemporada int,
	@IdCultivo   int
)
as

declare @Data table (
	IdSucursal   int,
	IdComuna     int not null, 
	IdAgricultor int not null,
	IntencionQty int,
	IntencionSup int,
	ContratoQty  int,
	ContratoSup  int,
	PrecioQty    int,
	IngresoQty   int,
	IngresoPesoLiquidado int,
	IngresoNetoLiquidado decimal(20, 2)
)	   

	  
insert into @Data (IdSucursal, IdComuna, IdAgricultor, IntencionQty, IntencionSup)
		select null as IdSucursal, a.IdComuna, a.IdAgricultor, sum(a.Cantidad) as IntencionQty, sum(a.Superficie) as IntencionSup
		  from IntencionSiembra a inner join
			   Comuna           b on a.IdComuna = b.IdComuna
		 where a.IdTemporada = @IdTemporada
		   and a.IdCultivo = @IdCultivo
	  group by b.IdProvincia, a.IdComuna, a.IdAgricultor


	insert into @Data (IdSucursal, IdComuna, IdAgricultor, ContratoQty, ContratoSup)
		select a.IdSucursal, a.IdComuna, a.IdAgricultor, sum(b.Cantidad) ContratoQty, sum(b.Superficie) as ContratoSup
		  from Contrato        a inner join
  			   ItemContrato    b on a.IdContrato = b.IdContrato inner join
  			   Comuna          c on a.IdComuna = c.IdComuna inner join
  			   CultivoContrato d on b.IdCultivoContrato = d.IdCultivoContrato
		 where a.Habilitado = 1
		   and a.IdTemporada = @IdTemporada
  		   and d.IdCultivo = @IdCultivo
	  group by a.IdSucursal, c.IdProvincia, a.IdComuna, a.IdAgricultor


	insert into @Data (IdSucursal, IdComuna, IdAgricultor, PrecioQty)
		  select null as IdSucursal, a.IdComuna, A.IdAgricultor, sum(e.Cantidad) as PrecioQty 
			from Contrato        a inner join
				 Comuna          c on a.IdComuna = c.IdComuna inner join
				 ConvenioPrecio e on a.IdContrato = e.IdContrato
		 where a.Habilitado = 1
		   and a.IdTemporada = @IdTemporada
		   and e.Habilitado = 1
  		   and (select count(*)
				  from ItemContrato    x inner join
					   CultivoContrato y on x.IdCultivoContrato = y.IdCultivoContrato
				 where x.IdContrato = a.IdContrato
				   and y.IdCultivo = @IdCultivo) > 0
	  group by c.IdProvincia, a.IdComuna, a.IdAgricultor


	insert into @Data (IdSucursal, IdComuna, IdAgricultor, IngresoQty, IngresoPesoLiquidado, IngresoNetoLiquidado)
		select a.IdSucursal, a.IdComunaOrigen, a.IdAgricultor, sum(a.PesoNormal) as IngresoQty, 
		       sum(a.PesoLiquidado), sum(a.TotalNetoLiquidacion)
		  from ProcesoIngreso  a inner join
			   CultivoContrato b on a.IdCultivoContrato = b.IdCultivoContrato inner join
			   Comuna          c on a.IdComunaOrigen = c.IdComuna 

		 where a.IdTemporada = @IdTemporada
		   and b.IdCultivo = @IdCultivo
		   and a.Nulo = 0
		   and a.PesoFinal is not null
	  group by a.IdSucursal, c.IdProvincia, a.IdComunaOrigen, a.IdAgricultor


	select a.IdSucursal, a.IdComuna, b.IdProvincia, a.IdAgricultor,
           isnull(m.Nombre, '(Sucursal Desconocida)') as Sucursal,
	       b.Nombre as Comuna, d.Nombre as Provincia, c.Rut, c.Nombre as Agricultor,
		   sum(a.IntencionQty) as IntencionQty, sum(a.IntencionSup) as IntencionSup,
		   sum(a.ContratoQty) as ContratoQty, sum(a.ContratoSup) as ContratoSup,
		   sum(a.PrecioQty) as PrecioQty, sum(a.IngresoQty) as IngresoQty,
		   sum(a.IngresoPesoLiquidado) as PesoIngresos, sum(a.IngresoNetoLiquidado) as NetoIngresos

	  from @Data   a inner join 
	       Comuna  b on a.IdComuna = b.IdComuna inner join
		   Agricultor c on a.IdAgricultor = c.IdAgricultor inner join
		   Provincia  d on b.IdProvincia = d.IdProvincia inner join
		   Region     e on d.IdRegion = e.IdRegion left join
		   Sucursal   m on a.IdSucursal = m.IdSucursal
  group by a.IdSucursal, a.IdComuna, b.IdProvincia, a.IdAgricultor, isnull(m.Nombre, '(Sucursal Desconocida)'),
	       b.Nombre, d.Nombre, c.Rut, c.Nombre, e.Orden, e.IdRegion
  order by Sucursal, a.IdSucursal,
           e.Orden, e.IdRegion,
		   d.Nombre, b.IdProvincia,
		   b.Nombre, a.IdComuna,
		   c.Nombre, a.IdAgricultor
GO

alter table PrecioSpot
	add IdSucursal int not null default(1)
GO

alter table PrecioSpot
	add constraint [FK_PrecioSpot_Sucursal] foreign key (IdSucursal) references Sucursal
GO


ALTER TABLE [dbo].[PrecioSpot] DROP CONSTRAINT [UI_PrecioSpot_CultivoMonedaFecha]
GO

ALTER TABLE [dbo].[PrecioSpot] ADD  CONSTRAINT [UI_PrecioSpot_CultivoMonedaFechaSucursal] UNIQUE NONCLUSTERED 
(
	[IdCultivo] ASC,
	[IdMoneda] ASC,
	IdSucursal ASC,
	[Fecha] ASC
)
GO



insert into PrecioSpot (IdCultivo, IdMoneda, Fecha, Valor, UserIns, FechaHoraIns, IpIns, UserUpd, FechaHoraUpd, IpUpd, IdSucursal)
	select a.IdCultivo, a.IdMoneda, a.Fecha, a.Valor, 'cdonoso', getdate(), '127.0.0.1', null, null, null, b.IdSucursal
	  from PrecioSpot a cross join
	       Sucursal   b
     where b.IdSucursal <> 1
GO


ALTER view [dbo].[vwPrecioSpot]
as

	select a.IdCultivo, a.Fecha, a.IdSucursal, b.Nombre as Sucursal,
	       (select x.Valor from PrecioSpot x where x.IdMoneda = 1 and x.IdCultivo = a.IdCultivo and x.Fecha = a.Fecha and x.IdSucursal = a.IdSucursal) as ValorCLP,
		   (select x.Valor from PrecioSpot x where x.IdMoneda = 2 and x.IdCultivo = a.IdCultivo and x.Fecha = a.Fecha and x.IdSucursal = a.IdSucursal) as ValorUSD
	  from PrecioSpot a inner join
	       Sucursal   b on a.IdSucursal = b.IdSucursal
	group by a.IdCultivo, a.Fecha, a.IdSucursal, b.Nombre 
	

GO


create table ConvenioPrecioItemTabla
(
	IdConvenioPrecioItemTabla int identity(1, 1) not null,
	IdConvenioPrecio int not null,
	IdSucursal int not null,
	Cantidad int not null,
	PrecioUnidad decimal(13, 5) not null,

	Autorizado bit null,
	Autorizador varchar(50) null,

	constraint [PK_ConvenioPrecioItemTabla] primary key (IdConvenioPrecioItemTabla),
	constraint [FK_ConvenioPrecioItemTabla_ConvenioPrecio] foreign key (IdConvenioPrecio) references ConvenioPrecio on delete cascade,
	constraint [FK_ConvenioPrecioItemTabla_Sucursal] foreign key (IdSucursal) references Sucursal,
)
GO

alter table ConvenioPrecioItemTabla
	add constraint [UI_ConvenioPrecioItemTabla_SucursalPrecio] unique (IdConvenioPrecio, IdSucursal, PrecioUnidad)
GO


ALTER procedure [dbo].[ReceptoresNotificacionAutorizacionPrecio]
(
	@IdConvenioPrecioAutorizacion int
)
as
	declare @IdNivelAutorizacion int
	declare @IdPermiso int

	set @IdNivelAutorizacion = (select IdNivelAutorizacion from ConvenioPrecioAutorizacion where IdConvenioPrecioAutorizacion = @IdConvenioPrecioAutorizacion)
	set @IdPermiso = (case @IdNivelAutorizacion when 1 then 1023 when 2 then 1024 else 0 end)


	select a.UserID, a.UserName, a.Email, a.FullName, cast(1 as bit) as PuedeAutorizar
	  from SYS_User           a inner join
	       SYS_PermisoUsuario b on a.UserName = b.UserName and b.IdPermiso = @IdPermiso
	 where a.Disabled = 0
GO

delete from ConvenioPrecioAutorizacion
GO


alter table ConvenioPrecioAutorizacion
	add TablaPrecios text not null 
GO


insert into SYS_Permiso values(1025, 'Ver historia de autorizaciones de precios', 4083, 'cdonoso', getdate(), '127.0.0.1', 4)
GO


select * from SYS_Permiso order by IdModulo, Orden

select * from ConvenioPrecioItemTabla

select * from NivelRangoPrecio

select * from ConvenioPrecioAutorizacion