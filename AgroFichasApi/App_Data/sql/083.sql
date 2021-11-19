use agrofichas
GO

update SYS_Version set CurrentVersion = 83
GO

create alter function [dbo].ComunaOrigenIngreso
(
    @IdComunaOrigen int,
	@IdAgricultor int,
	@IdTemporada  int, 
	@IdCultivo    int
)
returns int
begin

	declare @IdComuna int

	if @IdComunaOrigen <> 999999
	begin
		set @IdComuna = @IdComunaOrigen
	end
	else
	begin
		set @IdComuna = (
		  select top 1 a.IdComuna
			from Contrato        a inner join
				 ItemContrato    b on a.IdContrato = b.IdContrato inner join
				 CultivoContrato c on b.IdCultivoContrato = c.IdCultivoContrato
		   where a.IdAgricultor = @IdAgricultor 
			 and a.IdTemporada  = @IdTemporada
			 and c.IdCultivo = @IdCultivo
			 and a.IdComuna <> 999999)
	end

	return isnull(@IdComuna, 999999)
end
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
	PrecioQtyCLP int,
	PrecioNetoCLP decimal(20,5),
	PrecioQtyUSD  int,
	PrecioNetoUSD decimal(20,5),
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
		         sum(case when e.IdMoneda = 1 then e.Cantidad else 0 end) as PrecioQtyCLP,
				 sum(case when e.IdMoneda = 1 then e.Cantidad * e.PrecioUnidad else 0 end) as PrecioNetoCLP,
		         sum(case when e.IdMoneda = 2 then e.Cantidad else 0 end) as PrecioQtyUSD,
				 sum(case when e.IdMoneda = 2 then e.Cantidad * e.PrecioUnidad else 0 end) as PrecioNetoUSD
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

exec [dbo].[rpt_IntencionContratoPrecioIngresos] 5, 1
GO 

alter  procedure [dbo].[rpt_ConvenioPreciosPorSemana]
(
	@IdTemporada int,
	@IdCultivo   int

)
as

	set DATEFIRST 1 --Monday is the first day of the week

		select a.IdSucursal, a.IdComuna, d.IdProvincia, a.IdAgricultor, datepart(week, b.FechaHoraIns) as Semana,
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

create  procedure [dbo].[rpt_IngresosPorSemana]
(
	@IdTemporada int,
	@IdCultivo   int

)
as

	set DATEFIRST 1 --Monday is the first day of the week

		select a.IdSucursal, 
		       [dbo].ComunaOrigenIngreso(a.IdComunaOrigen, a.IdAgricultor, a.IdTemporada, b.IdCultivo) as IdComuna, 
			   c.IdProvincia, a.IdAgricultor, sum(a.PesoNormal) as IngresoQty, 
			   datepart(week, b.FechaHoraIns) as Semana,
		       datepart(month, dbo.FirstDateOfWeekForDate(b.FechaHoraIns)) as Mes, 
			   year(dbo.FirstDateOfWeekForDate(b.FechaHoraIns)) as Ano,
		       sum(a.PesoLiquidado) as IngresoPesoLiquidado, sum(a.TotalNetoLiquidacion) as IngresoNetoLiquidado

		  from ProcesoIngreso  a inner join
			   CultivoContrato b on a.IdCultivoContrato = b.IdCultivoContrato inner join
			   Comuna          c on [dbo].ComunaOrigenIngreso(a.IdComunaOrigen, a.IdAgricultor, a.IdTemporada, b.IdCultivo)  = c.IdComuna 

		 where a.IdTemporada = @IdTemporada
		   and b.IdCultivo = @IdCultivo
		   and a.Nulo = 0
		   and a.PesoFinal is not null
	  group by a.IdSucursal, c.IdProvincia, IdComuna, a.IdAgricultor, a.IdTemporada, b.IdCultivo, a.IdComunaOrigen,
	           datepart(week, b.FechaHoraIns), datepart(month, dbo.FirstDateOfWeekForDate(b.FechaHoraIns)), year(dbo.FirstDateOfWeekForDate(b.FechaHoraIns))
GO

ALTER procedure [dbo].[ItemsContratosParaCierreInicial]
(
	@IdProcesoIngreso int
)
as

	--Suponemos que no existe cierre anterior, es decir no hay registros en PrecioIngreso
	if (select count(*) from PrecioIngreso where IdProcesoIngreso = @IdProcesoIngreso) > 0
	begin
		raiserror('El Ingreso ya tiene in cierre asociado', 16, 1)
		return
	end

	--Priodidad Para Seleccion de Convenios de Precio:
	--1: Convenios del Rut
	--2: Convenios del Padre
	--3: Convenios de Hermanos
	--4: Convenios de los hijos

	declare @IdAgricultor      int
	declare @IdTemporada       int
	declare @IdCultivoContrato int
	declare @IdAgricultorPadre int
	declare @Hermanos          table (IdAgricultor int not null)
	declare @Hijos             table (IdAgricultor int not null)

	select @IdAgricultor = IdAgricultor, @IdTemporada = IdTemporada, @IdCultivoContrato = IdCultivoContrato
	  from ProcesoIngreso 
	 where IdProcesoIngreso = @IdProcesoIngreso

	set @IdAgricultorPadre = (select a.IdAgricultorPadre 
	                            from AgricultorRelacionado a
							   where a.IdAgricultorHijo  = @IdAgricultor
							     and a.IdAgricultorPadre <> @IdAgricultor) -- me excluyo en caso de que sea mi propio padre
	
	--
	declare @T1 table (
		Prioridad1     int not null,
		Prioridad2     int not null,
		IdItemContrato int not null
	)

	insert into @Hermanos (IdAgricultor)
		select a.IdAgricultorHijo
		  from AgricultorRelacionado a
		 where a.IdAgricultorPadre = @IdAgricultorPadre
		   and a.IdAgricultorHijo <> @IdAgricultorPadre
		   and a.IdAgricultorHijo <> @IdAgricultor

	insert into @Hijos (IdAgricultor)
		select a.IdAgricultorHijo
		  from AgricultorRelacionado a
		 where a.IdAgricultorPadre = @IdAgricultor
		   and (a.IdAgricultorHijo <> @IdAgricultor)


	--Contratos del Rut, del padre o de los hermanos e hijos
	insert into @T1 (Prioridad1, Prioridad2, IdItemContrato)
		select case a.IdAgricultor when  @IdAgricultor      then 1 
		                           when  @IdAgricultorPadre then 2 
			                       else                          3 end as Prioridad1, 
		       b.IdItemContrato as Prioridad2,
			   b.IdItemContrato
		  from Contrato       a inner join
			   ItemContrato   b on a.IdContrato = b.IdContrato

		 where (a.IdAgricultor in(@IdAgricultor, @IdAgricultorPadre)  or
		        a.IdAgricultor in(select IdAgricultor from @Hermanos) or
				a.IdAgricultor in(select IdAgricultor from @Hijos))
		   and a.IdTemporada       = @IdTemporada 
		   and b.IdCultivoContrato = @IdCultivoContrato
		   and a.Habilitado        = 1

	/*select * from Agricultor a 
	 where (a.IdAgricultor in(@IdAgricultor, @IdAgricultorPadre)  or
		    a.IdAgricultor in(select IdAgricultor from @Hermanos) or
			a.IdAgricultor in(select IdAgricultor from @Hijos)) and a.Habilitado = 1*/

	--Select final
	select a.Prioridad1, a.Prioridad2, d.IdAgricultor, d.Rut, d.Nombre, c.IdContrato, c.NumeroContrato, b.IdItemContrato,
		   b.IdCultivoContrato, e.Nombre as CultivoContrato, c.Comentarios as ComentariosContrato,
		   b.Cantidad as CantidadContrato, 
		   b.Cantidad - (select isnull(sum(x.Cantidad), 0) from PrecioIngreso x where x.IdItemContrato = b.IdItemContrato) as CantidadDisponible,
		   cast(case when (select count(*) from ConvenioCambioMoneda x where x.IdContrato = c.IdContrato and x.Habilitado = 1) > 0 then 1 else 0 end as bit) as TieneConvenioMoneda
	  from @T1             a inner join
		   ItemContrato    b on a.IdItemContrato = b.IdItemContrato inner join
		   Contrato        c on b.IdContrato = c.IdContrato  inner join
		   Agricultor      d on c.IdAgricultor = d.IdAgricultor inner join
		   CultivoContrato e on e.IdCultivoContrato = b.IdCultivoContrato
  order by a.Prioridad1, a.Prioridad2
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
	Disponible int
)

	insert into @t1 (IdConvenioPrecioItemTabla, IdConvenioPrecio, Cantidad, PrecioUnidad)
		select b.IdConvenioPrecioItemTabla, b.IdConvenioPrecio, b.Cantidad, b.PrecioUnidad
		  from ConvenioPrecio          a  inner join
		       ConvenioPrecioItemTabla b on a.IdConvenioPrecio = b.IdConvenioPrecio
		 where a.IdContrato = @IdContrato
		   and b.IdSucursal = @IdSucursal


	update @t1
	   set DisponibleItemTabla = a.Cantidad - (select isnull(sum(x.Cantidad), 0)
	                                             from PrecioIngreso  x inner join  
								                      ProcesoIngreso y on x.IdProcesoIngreso = y.IdProcesoIngreso
								                where x.IdConvenioPrecio = a.IdConvenioPrecio
								                  and y.IdSucursal = @IdSucursal
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
		   g.Disponible as CantidadDisponible, f.IdConvenioPrecioItemTabla

	  from ConvenioPrecio          b inner join
		   Moneda                  e on b.IdMoneda = e.IdMoneda inner join
		   ConvenioPrecioItemTabla f on b.IdConvenioPrecio = f.IdConvenioPrecio inner join
		   @t1                     g on f.IdConvenioPrecioItemTabla = g.IdConvenioPrecioItemTabla

     where b.IdContrato = @IdContrato
	   and f.IdSucursal = @IdSucursal
	   and g.Disponible > 0
  order by b.Prioridad
 
 GO

