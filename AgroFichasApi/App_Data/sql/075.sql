use agrofichas
GO

update SYS_Version set CurrentVersion = 75
GO



create procedure rpt_Recomendaciones
(
	@IdTemporada int
)
as

	select 'Ficha' as Origen, a.IdFicha, a.IdPredio, a.Fecha, a.UserIns, c.Nombre as Etapa, 
		   b.FechaAplicacion, i.Nombre as Quimico, b.Dosis, j.Nombre as UM,
		   b.FerB, b.FerCaO, b.FerKO2, b.FerMgO, b.FerN, b.FerP2O5, b.FerS, b.FerZn,
		   d.Nombre as Predio, e.Nombre as Comuna, f.Nombre as Provincia,
		   g.Nombre as Region,  h.Nombre as Agricultor,
		   isnull((select x.FullName from SYS_User x where x.UserName = a.UserIns), a.UserIns) as Agente
		  
	  from Ficha a inner join 
	       Recomendacion b on a.IdFicha = b.IdFicha inner join
		   TipoFicha     c on a.IdTipoFicha = c.IdTipoFicha inner join
		   Predio        d on a.IdPredio = d.IdPredio inner join
		   Comuna        e on d.IdComuna = e.IdComuna inner join 
		   Provincia     f on e.IdProvincia = f.IdProvincia inner join
		   Region        g on f.IdRegion = g.IdRegion inner join 
		   Agricultor    h on d.IdAgricultor = h.IdAgricultor inner join
		   Quimico       i on b.IdQuimico = i.IdQuimico inner join
		   UM            j on b.IdUM = j.IdUM

	 where a.IdTemporada = @IdTemporada

union all

	select 'FichaPreSiembra' as Origen, a.IdFichaPreSiembra, a.IdPredio, a.Fecha, a.UserIns, 'Pre-Siembra' as Etapa, 
		   b.FechaAplicacion, i.Nombre, b.Dosis, j.Nombre,
		   b.FerB, b.FerCaO, b.FerKO2, b.FerMgO, b.FerN, b.FerP2O5, b.FerS, b.FerZn,
		   d.Nombre as Predio, e.Nombre as Comuna, f.Nombre as Provincia,
		   g.Nombre as Region,  h.Nombre as Agricultor,
		   isnull((select x.FullName from SYS_User x where x.UserName = a.UserIns), a.UserIns) as Agente

	  from FichaPreSiembra         a inner join 
	       RecomendacionPreSiembra b on a.IdFichaPreSiembra = b.IdFichaPreSiembra inner join
		   Predio                  d on a.IdPredio = d.IdPredio inner join
		   Comuna                  e on d.IdComuna = e.IdComuna inner join 
		   Provincia               f on e.IdProvincia = f.IdProvincia inner join
		   Region                  g on f.IdRegion = g.IdRegion inner join 
		   Agricultor              h on d.IdAgricultor = h.IdAgricultor inner join
		   Quimico                 i on b.IdQuimico = i.IdQuimico inner join
		   UM                      j on b.IdUM = j.IdUM

	 where a.IdTemporada = @IdTemporada

order by Fecha

GO


exec rpt_Recomendaciones 5