use agrofichas
GO

update SYS_Version set CurrentVersion = 87
GO


ALTER procedure [dbo].[ConveniosParaCierreInicial]
(
	@IdContrato int,
	@IdSucursal int
)
as

declare @t1 table (
	IdConvenioPrecioItemTabla int primary key,
	IdConvenioPrecio int, 
	Cantidad int not null,
	PrecioUnidad decimal(13,5) not null,
	DisponibleItemTabla int,
    DisponibleConvenio int,
	Disponible int,
	IdSucursal int
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
	select b.Prioridad, b.IdConvenioPrecio, f.PrecioUnidad, b.IdMoneda,
	       b.Comentarios, e.Formato as FormatoMoneda, e.Simbolo as SimboloMoneda,
		   f.Cantidad as CantidadConvenio,  b.EsPiso,
		   g.Disponible as CantidadDisponible, f.IdConvenioPrecioItemTabla, b.IdContrato, g.IdSucursal

	  from ConvenioPrecio          b inner join
		   Moneda                  e on b.IdMoneda = e.IdMoneda inner join
		   ConvenioPrecioItemTabla f on b.IdConvenioPrecio = f.IdConvenioPrecio inner join
		   @t1                     g on f.IdConvenioPrecioItemTabla = g.IdConvenioPrecioItemTabla

     where b.IdContrato = @IdContrato
	   and f.IdSucursal = isnull(@IdSucursal, f.IdSucursal)
	   and g.Disponible > 0
  order by b.Prioridad
 
GO

