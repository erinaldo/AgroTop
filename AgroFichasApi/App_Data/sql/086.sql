use agrofichas
GO

update SYS_Version set CurrentVersion = 86
GO



ALTER  procedure [dbo].[rpt_IngresosPorSemana]
(
	@IdTemporada int,
	@IdCultivo   int

)
as

	set DATEFIRST 1 --Monday is the first day of the week

		select a.IdSucursal, 
		       [dbo].ComunaOrigenIngreso(a.IdComunaOrigen, a.IdAgricultor, a.IdTemporada, b.IdCultivo) as IdComuna, 
			   c.IdProvincia, a.IdAgricultor, sum(a.PesoNormal) as IngresoQty, 
			   datepart(week, a.FechaHoraLlegada) as Semana,
		       datepart(month, dbo.FirstDateOfWeekForDate(a.FechaHoraLlegada)) as Mes, 
			   year(dbo.FirstDateOfWeekForDate(a.FechaHoraLlegada)) as Ano,
		       sum(a.PesoLiquidado) as IngresoPesoLiquidado, sum(a.TotalNetoLiquidacion) as IngresoNetoLiquidado

		  from ProcesoIngreso  a inner join
			   CultivoContrato b on a.IdCultivoContrato = b.IdCultivoContrato inner join
			   Comuna          c on [dbo].ComunaOrigenIngreso(a.IdComunaOrigen, a.IdAgricultor, a.IdTemporada, b.IdCultivo)  = c.IdComuna 

		 where a.IdTemporada = @IdTemporada
		   and b.IdCultivo = @IdCultivo
		   and a.Nulo = 0
		   and a.PesoFinal is not null
	  group by a.IdSucursal, c.IdProvincia, IdComuna, a.IdAgricultor, a.IdTemporada, b.IdCultivo, a.IdComunaOrigen,
	           datepart(week, a.FechaHoraLlegada), datepart(month, dbo.FirstDateOfWeekForDate(a.FechaHoraLlegada)), year(dbo.FirstDateOfWeekForDate(a.FechaHoraLlegada))
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
				 sum(cast(round(dbo.PrecioConvenioDefault(b.IdConvenioPrecio, 1, 'Cantidad'), 0) as int)) as PrecioQtyCLP,
				 sum(dbo.PrecioConvenioDefault(b.IdConvenioPrecio, 1, 'Neto')) as PrecioNetoCLP,	
				 sum(cast(round(dbo.PrecioConvenioDefault(b.IdConvenioPrecio, 2, 'Cantidad'), 0) as int)) as PrecioQtyUSD,
				 sum(dbo.PrecioConvenioDefault(b.IdConvenioPrecio, 2, 'Neto')) as PrecioNetoUSD
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