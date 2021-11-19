use agrofichas
GO

update SYS_Version set CurrentVersion = 72
GO


insert into SYS_Menu values (1002, 1, 1, 'Sistema AgroFichas', '~/fichasclient', 5)
GO

update Region set DoSync = 1
GO

update SYS_Menu 
   set Orden = 15
 where IdMenu = 1002
GO


select * from SYS_Menu where IdModulo = 1 order by Orden

--select * from SYS_Modulo
--select * from SYS_Menu where IdModulo = 1 order by Orden
--select * from SYS_Permiso order by IdModulo, Orden

select * from Agricultor

select * from Temporada

select * from Comuna

SELECt * from predio