use agrofichas
GO

update SYS_Version set CurrentVersion = 76
GO


create function [dbo].[CultivoFicha]
(
	@IdFicha int
)
returns int
begin
	declare @IdCultivo int

	set @IdCultivo = (
	  select top 1 d.IdCultivo
		from FichaPotrero   m inner join
		     Potrero        a on m.IdPotrero = a.IdPotrero inner join
			 SiembraPotrero b on a.IdPotrero = b.IdPotrero inner join
			 Siembra        c on b.IdSiembra = c.IdSiembra inner join
			 Variedad       d on c.IdVariedad = d.IdVariedad
	   where m.IdFicha = @IdFicha)

	return @IdCultivo
end
GO

select [dbo].[CultivoFicha] (6066)
GO

create function [dbo].[CultivoFichaPreSiembra]
(
	@IdFicha int
)
returns int
begin
	declare @IdCultivo int

	set @IdCultivo = (
	  select top 1 d.IdCultivo
		from FichaPreSiembraPotrero m inner join
		     Potrero                a on m.IdPotrero = a.IdPotrero inner join
			 SiembraPotrero         b on a.IdPotrero = b.IdPotrero inner join
			 Siembra                c on b.IdSiembra = c.IdSiembra inner join
			 Variedad               d on c.IdVariedad = d.IdVariedad
	   where m.IdFichaPreSiembra = @IdFicha)

	return @IdCultivo
end
GO

select [dbo].[CultivoFichaPreSiembra]� (4)
GO



ALTER procedure [dbo].[rpt_Recomendaciones]
(
	@IdTemporada int,
	@IdCultivo   int
)
as

	select 'Ficha' as Origen, a.IdFicha, a.IdPredio, a.Fecha, a.UserIns, c.Nombre as Etapa, 
		   b.FechaAplicacion, i.Nombre as Quimico, b.Dosis, j.Nombre as UM,
		   b.FerB, b.FerCaO, b.FerKO2, b.FerMgO, b.FerN, b.FerP2O5, b.FerS, b.FerZn,
		   d.Nombre as Predio, e.Nombre as Comuna, f.Nombre as Provincia,
		   g.Nombre as Region,  h.Nombre as Agricultor,
		   isnull((select x.FullName from SYS_User x where x.UserName = a.UserIns), a.UserIns) as Agente,
		   m.IdCultivo, m.Nombre as Cultivo
		  
	  from Ficha a inner join 
	       Recomendacion b on a.IdFicha = b.IdFicha inner join
		   TipoFicha     c on a.IdTipoFicha = c.IdTipoFicha inner join
		   Predio        d on a.IdPredio = d.IdPredio inner join
		   Comuna        e on d.IdComuna = e.IdComuna inner join 
		   Provincia     f on e.IdProvincia = f.IdProvincia inner join
		   Region        g on f.IdRegion = g.IdRegion inner join 
		   Agricultor    h on d.IdAgricultor = h.IdAgricultor inner join
		   Quimico       i on b.IdQuimico = i.IdQuimico inner join
		   UM            j on b.IdUM = j.IdUM left join
		   Cultivo       m on dbo.CultivoFicha(a.IdFicha) = m.IdCultivo

	 where a.IdTemporada = @IdTemporada
	   and  (dbo.CultivoFicha(a.IdFicha) = @IdCultivo or @IdCultivo is null)

union all

	select 'FichaPreSiembra' as Origen, a.IdFichaPreSiembra, a.IdPredio, a.Fecha, a.UserIns, 'Pre-Siembra' as Etapa, 
		  b.FechaAplicacion, i.Nombre as Quimico, b.Dosis, j.Nombre as UM,
		   b.FerB, b.FerCaO, b.FerKO2, b.FerMgO, b.FerN, b.FerP2O5, b.FerS, b.FerZn,
		   d.Nombre as Predio, e.Nombre as Comuna, f.Nombre as Provincia,
		   g.Nombre as Region,  h.Nombre as Agricultor,
		   isnull((select x.FullName from SYS_User x where x.UserName = a.UserIns), a.UserIns) as Agente,
		   m.IdCultivo, m.Nombre as Cultivo

	  from FichaPreSiembra         a inner join 
	       RecomendacionPreSiembra b on a.IdFichaPreSiembra = b.IdFichaPreSiembra inner join
		   Predio                  d on a.IdPredio = d.IdPredio inner join
		   Comuna                  e on d.IdComuna = e.IdComuna inner join 
		   Provincia               f on e.IdProvincia = f.IdProvincia inner join
		   Region                  g on f.IdRegion = g.IdRegion inner join 
		   Agricultor              h on d.IdAgricultor = h.IdAgricultor inner join
		   Quimico                 i on b.IdQuimico = i.IdQuimico inner join
		   UM                      j on b.IdUM = j.IdUM left join
		   Cultivo                 m on dbo.CultivoFichaPreSiembra(a.IdFichaPreSiembra) = m.IdCultivo

	 where a.IdTemporada = @IdTemporada
	  and  (dbo.CultivoFichaPreSiembra(a.IdFichaPreSiembra) = @IdCultivo or @IdCultivo is null)
order by Fecha

