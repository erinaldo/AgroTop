use agrofichas
GO

update SYS_Version set CurrentVersion = 78
GO

create procedure [dbo].[ReceptoresNotificacionAutorizacionPrecio]
(
	@IdConvenioPrecioAutorizacion int
)
as
	declare @IdNivelAutorizacion int

	set @IdNivelAutorizacion = (select IdNivelAutorizacion from ConvenioPrecioAutorizacion where IdConvenioPrecioAutorizacion = @IdConvenioPrecioAutorizacion)

	select a.UserID, a.UserName, a.Email, a.FullName,
		   cast (case when c.IdPermiso is null then 0 else 1 end as bit) as PuedeAutorizar
	  from SYS_User a inner join
	       SYS_PermisoUsuario b on a.UserName = b.UserName and b.IdPermiso = 1023 left join
		   SYS_PermisoUsuario c on a.UserName = c.UserName and c.IdPermiso = 1024
	 where a.Disabled = 0
	   and ((@IdNivelAutorizacion = 1 and b.IdPermiso is not null or
	         @IdNivelAutorizacion = 2 and c.IdPermiso is not null))
GO

alter table Contrato
	add IdComuna int not null default(999999)
GO

alter table Contrato
	add constraint [FK_Contrato_Comuna] foreign key (IdComuna) references Comuna
GO

alter table ProcesoIngreso
	add IdComunaOrigen int not null default(999999)
GO

alter table ProcesoIngreso
	add constraint [FK_ProcesoIngreso_Comuna] foreign key (IdComunaOrigen) references Comuna(IdComuna)
GO

create procedure dbo.rpt_IntencionContratoPrecioIngresos
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
		select null as IdSucursal, a.IdComuna, a.IdAgricultor, sum(b.Cantidad) ContratoQty, sum(b.Superficie) as ContratoSup
		  from Contrato        a inner join
  			   ItemContrato    b on a.IdContrato = b.IdContrato inner join
  			   Comuna          c on a.IdComuna = c.IdComuna inner join
  			   CultivoContrato d on b.IdCultivoContrato = d.IdCultivoContrato
		 where a.Habilitado = 1
		   and a.IdTemporada = @IdTemporada
  		   and d.IdCultivo = @IdCultivo
	  group by c.IdProvincia, a.IdComuna, a.IdAgricultor


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

ALTER procedure [dbo].[ReceptoresNotificacionConvenioPrecio]
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

	select 'Administrativo' as Rol, a.FullName, a.Email
	  from SYS_User a inner join
	       SYS_PermisoUsuario b on a.UserName = b.UserName 
	 where a.Disabled = 0
	   and b.IdPermiso = 1011
GO

exec rpt_IntencionContratoPrecioIngresos 4, 1

select PesoLiquidado, PesoNormal, PesoNoLiquidado, * from ProcesoIngreso where Nulo = 0 and  PesoLiquidado + PesoNoLiquidado <> PesoNormal


select *

    from ProcesoIngreso
	where IdSucursal = 1 and IdAgricultor = 3485 and IdTemporada = 4 and Nulo = 0
	1 -> 122 -> 12201 -> 3485