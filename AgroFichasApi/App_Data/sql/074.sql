use agrofichas
GO

update SYS_Version set CurrentVersion = 74
GO

create view dbo.vw_VisitaPorSemanaCultivoAgente
as
	select 'Ficha' as Origen, b.IdAgricultor, c.IdTemporada, 
		   datepart(week, c.Fecha) as Semana, datepart(month, dbo.FirstDateOfWeekForDate(c.Fecha)) as Mes, year(dbo.FirstDateOfWeekForDate(c.Fecha)) as Ano, 
		   dbo.CultivoPotrero(d.IdPotrero, c.IdTemporada) as IdCultivo,
		   c.UserIns,
		   count(distinct dbo.StripTime(c.Fecha)) as Visitas 
	  from Predio b inner join
		   Ficha  c on b.IdPredio = c.IdPredio inner join
		   FichaPotrero d on c.IdFicha = d.IdFicha
     
   group by b.IdAgricultor, c.IdTemporada, c.UserIns, datepart(week, c.Fecha), datepart(month, dbo.FirstDateOfWeekForDate(c.Fecha)),  year(dbo.FirstDateOfWeekForDate(c.Fecha)), dbo.CultivoPotrero(d.IdPotrero, c.IdTemporada)

union all   

   	select 'FichaPreSiembra' as Origen, b.IdAgricultor, c.IdTemporada, 
	       datepart(week, c.Fecha) as Semana, datepart(month, dbo.FirstDateOfWeekForDate(c.Fecha)) as Mes, year(dbo.FirstDateOfWeekForDate(c.Fecha)) as Ano, 
		   dbo.CultivoPotrero(d.IdPotrero, c.IdTemporada) as IdCultivo,
		   c.UserIns,
		   count(distinct dbo.StripTime(c.Fecha)) as Visitas 
	  from Predio                 b inner join
		   FichaPreSiembra        c on b.IdPredio = c.IdPredio inner join
		   FichaPreSiembraPotrero d on c.IdFichaPreSiembra = d.IdFichaPreSiembra

   group by b.IdAgricultor, c.IdTemporada, c.UserIns, datepart(week, c.Fecha), datepart(month, dbo.FirstDateOfWeekForDate(c.Fecha)),  year(dbo.FirstDateOfWeekForDate(c.Fecha)), dbo.CultivoPotrero(d.IdPotrero, c.IdTemporada)

GO

create procedure rpt_VisitaPorSemanaCultivoAgente_Data
(
	@IdTemporada int
)
as
      select a.IdCultivo, a.Semana, a.Mes, a.Ano, a.UserIns, sum(a.Visitas) as Visitas,
	         isnull((select x.FullName from SYS_User x where x.UserName = a.UserIns), a.UserIns) as Agente
		from vw_VisitaPorSemanaCultivoAgente a 
	   where a.IdTemporada = @IdTemporada
	group by a.IdCultivo, a.Semana, a.Mes, a.Ano, a.UserIns

GO

exec rpt_VisitaPorSemanaCultivoAgente_Data 5

select * from SYS_Permiso