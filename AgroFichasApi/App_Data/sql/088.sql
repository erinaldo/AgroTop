use agrofichas
GO

update SYS_Version set CurrentVersion = 88
GO

alter table PrecioIngreso 
	add BonoUnidad   decimal(13,5) not null default(0),
	    BonoCantidad int not null default(0),
		BonoTotal    decimal(10, 2) not null default(0)
GO


ALTER procedure [dbo].[ConveniosParaCierreInicial]
(
	@IdContrato int,
	@IdSucursal int
)
as

declare @t1 table (
	IdConvenioPrecioItemTabla int primary key, --
	IdConvenioPrecio int, --
	Cantidad int not null, --
	PrecioUnidad decimal(13,5) not null, --
	DisponibleItemTabla int,
    DisponibleConvenio int,
	Disponible int,
	IdSucursal int --
)

	insert into @t1 (IdConvenioPrecioItemTabla, IdConvenioPrecio, Cantidad, PrecioUnidad, IdSucursal)
		select b.IdConvenioPrecioItemTabla, b.IdConvenioPrecio, b.Cantidad, b.PrecioUnidad, b.IdSucursal
		  from ConvenioPrecio          a  inner join
		       ConvenioPrecioItemTabla b on a.IdConvenioPrecio = b.IdConvenioPrecio
		 where a.IdContrato = @IdContrato
		   and b.IdSucursal = isnull(@IdSucursal, b.IdSucursal)


	update @t1
	   set DisponibleItemTabla = a.Cantidad - (select isnull(sum(x.Cantidad), 0)
	                                             from PrecioIngreso  x inner join  
								                      ProcesoIngreso y on x.IdProcesoIngreso = y.IdProcesoIngreso
								                where x.IdConvenioPrecio = a.IdConvenioPrecio
								                  and y.IdSucursal = a.IdSucursal
								          	      and x.PrecioUnidad = a.PrecioUnidad)
	  from @t1 a

	update @t1
	   set DisponibleConvenio  = (select x.Cantidad from ConvenioPrecio x where x.IdConvenioPrecio = a.IdConvenioPrecio) - (select isnull(sum(x.Cantidad), 0)
	                               from PrecioIngreso  x inner join  
								        ProcesoIngreso y on x.IdProcesoIngreso = y.IdProcesoIngreso
								  where x.IdConvenioPrecio = a.IdConvenioPrecio)
	  from @t1 a


	update @t1
	   set Disponible = case when DisponibleItemTabla <= DisponibleConvenio then DisponibleItemTabla else DisponibleConvenio end


	--select * from  @t1
	--Select final
	select b.Prioridad, b.IdConvenioPrecio, b.IdMoneda,
	       f.PrecioUnidad, b.PrecioUnidad as PrecioUnidadBase, f.PrecioUnidad - b.PrecioUnidad as BonoUnidad,
	       
	       b.Comentarios, e.Formato as FormatoMoneda, e.Simbolo as SimboloMoneda,
		   f.Cantidad as CantidadConvenio,  b.EsPiso,
		   g.Disponible as CantidadDisponible, f.IdConvenioPrecioItemTabla, b.IdContrato, f.IdSucursal

	  from ConvenioPrecio          b inner join
		   Moneda                  e on b.IdMoneda = e.IdMoneda inner join
		   ConvenioPrecioItemTabla f on b.IdConvenioPrecio = f.IdConvenioPrecio inner join
		   @t1                     g on f.IdConvenioPrecioItemTabla = g.IdConvenioPrecioItemTabla

     where b.IdContrato = @IdContrato
	   and f.IdSucursal = isnull(@IdSucursal, f.IdSucursal)
	   and g.Disponible > 0
  order by b.Prioridad
 
GO


ALTER procedure [dbo].[TotalesPreciosIngreso]
(
	@IdProcesoIngreso int
)
as
	
	select a.IdMoneda, b.Formato, b.Nombre, b.Simbolo,
	       round(sum(round(a.PrecioUnidad * a.Cantidad + a.SobrePrecioTotal - a.DescuentoTotal + a.BonoTotal, 5)), 5) as Valor
	  from PrecioIngreso a inner join
	       Moneda        b on a.IdMoneda = b.IdMoneda
	 where a.IdProcesoIngreso = @IdProcesoIngreso
  group by a.IdMoneda, b.Formato, b.Nombre, b.Simbolo
  order by a.IdMoneda

GO

ALTER procedure [dbo].[PreciosParaLiquidacion]
(
	@IdProcesoIngreso int
)
as
	select a.IdPrecioIngreso, a.IdConvenioPrecio, a.PrecioUnidad, a.IdMoneda,
	       b.Simbolo as SimboloMoneda, b.Formato as FormatoMoneda, e.Rut, e.Nombre,
		   d.IdContrato, d.NumeroContrato, a.TasaCambio, a.SobrePrecioPor, a.DescuentoPor, a.IdLiquidacion,
		   a.Cantidad, --Kg del ingreso asignado a este convenio
		   f.Cantidad as CantidadConvenio,
		   f.Cantidad - (select isnull(sum(x.Cantidad), 0) from PrecioIngreso x where x.IdConvenioPrecio = f.IdConvenioPrecio) as CantidadDisponible, --Kg no asignados
		   d.Comentarios as ComentariosContrato, isnull(f.EsPiso, 0) as EsPiso,
		   cast(case when (select count(*) from ConvenioCambioMoneda x where x.IdContrato = d.IdContrato and x.Habilitado = 1) > 0 then 1 else 0 end as bit) as TieneConvenioMoneda,
		   a.BonoUnidad, a.BonoCantidad, g.PesoNormal, g.PesoBruto
	  from PrecioIngreso  a inner join
	       Moneda         b on a.IdMoneda = b.IdMoneda inner join
		   ItemContrato   c on a.IdItemContrato = c.IdItemContrato inner join
		   Contrato       d on c.IdContrato = d.IdContrato inner join
		   Agricultor     e on d.IdAgricultor = e.IdAgricultor inner join
		   ProcesoIngreso g on a.IdProcesoIngreso = g.IdProcesoIngreso left join
		   ConvenioPrecio f on a.IdConvenioPrecio = f.IdConvenioPrecio
	 where a.IdProcesoIngreso = @IdProcesoIngreso
	   --and a.IdLiquidacion is null
  order by a.IdPrecioIngreso

GO


	select a.Cantidad, a.PrecioUnidad, a.TasaCambio, a.SobrePrecioTotal, a.DescuentoTotal,
	       a.TotalNeto, (a.Cantidad * a.PrecioUnidad + a.SobrePrecioTotal - a.DescuentoTotal + a.BonoTotal ) * a.TasaCambio as Neto2, *
      from PrecioIngreso a
	 where 
		abs(a.TotalNeto  - (a.Cantidad * a.PrecioUnidad + a.SobrePrecioTotal - a.DescuentoTotal + a.BonoTotal) * a.TasaCambio) >= 1
		--a.TotalNeto  <> (a.Cantidad * a.PrecioUnidad + a.SobrePrecioTotal - a.DescuentoTotal ) * a.TasaCambio

select * from ConvenioPrecioItemTabla		

select * from ConvenioPrecioAjuste

select * from PrecioIngreso order by IdPrecioIngreso desc