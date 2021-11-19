use agrofichas
GO

update SYS_Version set CurrentVersion = 85
GO


ALTER  procedure [dbo].[rpt_ConvenioPreciosPorSemana]
(
	@IdTemporada int,
	@IdCultivo   int

)
as

	set DATEFIRST 1 --Monday is the first day of the week

		select isnull(a.IdSucursal, 0) as IdSucursal, a.IdComuna, d.IdProvincia, a.IdAgricultor, datepart(week, b.FechaHoraIns) as Semana,
		       datepart(month, dbo.FirstDateOfWeekForDate(b.FechaHoraIns)) as Mes, year(dbo.FirstDateOfWeekForDate(b.FechaHoraIns)) as Ano,
		       sum(b.Cantidad) as PrecioQty,
		       sum(case when b.IdMoneda = 1 then b.Cantidad else 0 end) as PrecioQtyCLP,
			   sum(case when b.IdMoneda = 1 then b.Cantidad * b.PrecioUnidad else 0 end) as PrecioNetoCLP,
		       sum(case when b.IdMoneda = 2 then b.Cantidad else 0 end) as PrecioQtyUSD,
			   sum(case when b.IdMoneda = 2 then b.Cantidad * b.PrecioUnidad else 0 end) as PrecioNetoUSD
		  from Contrato       a inner join
	    	   ConvenioPrecio b on a.IdContrato = b.IdContrato inner join
			   Comuna         d on a.IdComuna = d.IdComuna
		 where a.Habilitado = 1
		    and a.IdTemporada = @IdTemporada 
			and b.Habilitado = 1
    		and (select count(*)
  				from ItemContrato    x inner join
  					CultivoContrato y on x.IdCultivoContrato = y.IdCultivoContrato
  				where x.IdContrato = a.IdContrato
  				and y.IdCultivo = @IdCultivo) > 0
    group by a.IdSucursal, a.IdComuna, a.IdAgricultor, d.IdProvincia,
	         datepart(week, b.FechaHoraIns), datepart(month, dbo.FirstDateOfWeekForDate(b.FechaHoraIns)), year(dbo.FirstDateOfWeekForDate(b.FechaHoraIns))

GO


create  function [dbo].PrecioConvenioDefault
(
	@IdConvenioPrecio int,
    @IdMoneda int,
    @Valor    varchar(20)
)
returns decimal
begin

	declare @Result decimal(20,5)


	declare @Cantidad decimal
	declare @Neto     decimal(20, 5)
		
		select @Cantidad = isnull(sum(x.Cantidad), 0),
			   @Neto     = isnull(sum(x.Cantidad * x.PrecioUnidad), 0)
		  from ConvenioPrecioItemTabla x inner join
		       ConvenioPrecio          y on x.IdConvenioPrecio = y.IdConvenioPrecio inner join
		       Contrato                z on y.IdContrato = z.IdContrato and x.IdSucursal = z.IdSucursal
		 where y.IdConvenioPrecio = @IdConvenioPrecio
		   and y.IdMoneda = @IdMoneda
		   --and x.IdSucursal = z.IdSucursal
		   
	if @Valor = 'Cantidad'
		set @Result = @Cantidad
	
	if @Valor = 'Neto'
		set @Result = @Neto

	return @Result
end

GO

alter  procedure [dbo].[rpt_IntencionContratoPrecioIngresos]
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
	PrecioQtyCLP int,
	PrecioNetoCLP decimal(30,5),
	PrecioQtyUSD  int,
	PrecioNetoUSD decimal(30,5),
	IngresoQty   int,
	IngresoPesoLiquidado int,
	IngresoNetoLiquidado decimal(20, 2)
)	   

	--Intención de Siembra
	insert into @Data (IdSucursal, IdComuna, IdAgricultor, IntencionQty, IntencionSup)
		select null as IdSucursal, a.IdComuna, a.IdAgricultor, sum(a.Cantidad) as IntencionQty, sum(a.Superficie) as IntencionSup
		  from IntencionSiembra a inner join
			   Comuna           b on a.IdComuna = b.IdComuna
		 where a.IdTemporada = @IdTemporada
		   and a.IdCultivo = @IdCultivo
	  group by b.IdProvincia, a.IdComuna, a.IdAgricultor

    --Contratado
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

    --Convenios de Precios
	insert into @Data (IdSucursal, IdComuna, IdAgricultor, PrecioQty, PrecioQtyCLP, PrecioNetoCLP, PrecioQtyUSD, PrecioNetoUSD)

		 /* select c.IdSucursal, a.IdComuna, a.IdAgricultor, sum(c.Cantidad) as PrecioQty,
		         sum(case when b.IdMoneda = 1 then c.Cantidad else 0 end) as PrecioQtyCLP,
				 sum(case when b.IdMoneda = 1 then c.Cantidad * c.PrecioUnidad else 0 end) as PrecioNetoCLP,
		         sum(case when b.IdMoneda = 2 then c.Cantidad else 0 end) as PrecioQtyUSD,
				 sum(case when b.IdMoneda = 2 then c.Cantidad * c.PrecioUnidad else 0 end) as PrecioNetoUSD
		    from Contrato a inner join
			     ConvenioPrecio b on a.IdContrato = b.IdContrato inner join
				 ConvenioPrecioItemTabla c on b.IdConvenioPrecio = c.IdConvenioPrecio
		   where a.Habilitado = 1
		     and a.IdTemporada = @IdTemporada 
			 and b.Habilitado = 1
    		 and (select count(*)
  				  from ItemContrato    x inner join
  					   CultivoContrato y on x.IdCultivoContrato = y.IdCultivoContrato
  				 where x.IdContrato = a.IdContrato
  				   and y.IdCultivo = @IdCultivo) > 0
        group by c.IdSucursal, a.IdComuna, a.IdAgricultor*/

		/* ****************************************************************************************************************************************** */
		/* El precio unitario deberia salir de suponer que estamos entregado en la sucursal del contrato y aplicando lo bonos / decuentos necesarios  */
		/* ****************************************************************************************************************************************** */
		
		  select a.IdSucursal as IdSucursal, a.IdComuna, A.IdAgricultor, sum(e.Cantidad) as PrecioQty,
				 sum(cast(round(dbo.PrecioConvenioDefault(e.IdConvenioPrecio, 1, 'Cantidad'), 0) as int)) as PrecioQtyCLP,
				 sum(dbo.PrecioConvenioDefault(e.IdConvenioPrecio, 1, 'Neto')) as PrecioNetoCLP,	
				 sum(cast(round(dbo.PrecioConvenioDefault(e.IdConvenioPrecio, 2, 'Cantidad'), 0) as int)) as PrecioQtyUSD,
				 sum(dbo.PrecioConvenioDefault(e.IdConvenioPrecio, 2, 'Neto')) as PrecioNetoUSD
			from Contrato        a inner join
				 Comuna          c on a.IdComuna = c.IdComuna inner join
				 ConvenioPrecio  e on a.IdContrato = e.IdContrato
		 where a.Habilitado = 1
		   and a.IdTemporada = @IdTemporada
		   and e.Habilitado = 1
  		   and (select count(*)
				  from ItemContrato    x inner join
					   CultivoContrato y on x.IdCultivoContrato = y.IdCultivoContrato
				 where x.IdContrato = a.IdContrato
				   and y.IdCultivo = @IdCultivo) > 0
	  group by a.IdSucursal, c.IdProvincia, a.IdComuna, a.IdAgricultor

    --Ingresos
	insert into @Data (IdSucursal, IdComuna, IdAgricultor, IngresoQty, IngresoPesoLiquidado, IngresoNetoLiquidado)
		select a.IdSucursal, 
		       [dbo].ComunaOrigenIngreso(a.IdComunaOrigen, a.IdAgricultor, a.IdTemporada, b.IdCultivo) as IdComuna, 
			   a.IdAgricultor, sum(a.PesoNormal) as IngresoQty, 
		       sum(a.PesoLiquidado), sum(a.TotalNetoLiquidacion)
		  from ProcesoIngreso  a inner join
			   CultivoContrato b on a.IdCultivoContrato = b.IdCultivoContrato inner join
			   Comuna          c on [dbo].ComunaOrigenIngreso(a.IdComunaOrigen, a.IdAgricultor, a.IdTemporada, b.IdCultivo) = c.IdComuna 

		 where a.IdTemporada = @IdTemporada
		   and b.IdCultivo = @IdCultivo
		   and a.Nulo = 0
		   and a.PesoFinal is not null
	  group by a.IdSucursal, c.IdProvincia, IdComuna, a.IdAgricultor, a.IdTemporada, b.IdCultivo, a.IdComunaOrigen


	select a.IdSucursal, a.IdComuna, b.IdProvincia, a.IdAgricultor,
           isnull(m.Nombre, '(Sucursal Desconocida)') as Sucursal,
	       b.Nombre as Comuna, d.Nombre as Provincia, c.Rut, c.Nombre as Agricultor,
		   sum(a.IntencionQty) as IntencionQty, sum(a.IntencionSup) as IntencionSup,
		   sum(a.ContratoQty) as ContratoQty, sum(a.ContratoSup) as ContratoSup,
		   sum(a.PrecioQty) as PrecioQty, 
		   sum(a.PrecioQtyCLP) as PrecioQtyCLP, sum(a.PrecioNetoCLP) as PrecioNetoCLP,
		   sum(a.PrecioQtyUSD) as PrecioQtyUSD, sum(a.PrecioNetoUSD) as PrecioNetoUSD,
		   sum(a.IngresoQty) as IngresoQty,
		   sum(a.IngresoPesoLiquidado) as PesoIngresos, sum(a.IngresoNetoLiquidado) as NetoIngresos

	  from @Data   a inner join 
	       Comuna  b on a.IdComuna = b.IdComuna inner join
		   Agricultor c on a.IdAgricultor = c.IdAgricultor inner join
		   Provincia  d on b.IdProvincia = d.IdProvincia inner join
		   Region     e on d.IdRegion = e.IdRegion left join
		   Sucursal   m on a.IdSucursal = m.IdSucursal
	 --where a.IdAgricultor = 3245
  group by a.IdSucursal, a.IdComuna, b.IdProvincia, a.IdAgricultor, isnull(m.Nombre, '(Sucursal Desconocida)'),
	       b.Nombre, d.Nombre, c.Rut, c.Nombre, e.Orden, e.IdRegion
  order by Sucursal, a.IdSucursal,
           e.Orden, e.IdRegion,
		   d.Nombre, b.IdProvincia,
		   b.Nombre, a.IdComuna,
		   c.Nombre, a.IdAgricultor



GO
