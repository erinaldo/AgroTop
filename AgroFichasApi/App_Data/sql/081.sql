use agrofichas
GO

update SYS_Version set CurrentVersion = 81
GO

insert into Comuna values(9122, 91, 'Labranza', 347)
GO


select * from Comuna where Nombre like '%futrono%'

select * from Comuna Order by Orden
