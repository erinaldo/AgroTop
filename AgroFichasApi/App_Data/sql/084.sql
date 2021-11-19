use agrofichas
GO

update SYS_Version set CurrentVersion = 84
GO

insert into SYS_Permiso values(1028, 'Eliminar autorizaciones de precio en revisión', 4084, 'cdonoso', getdate(), '127.0.0.1', 4)
GO

--1028
select * from SYS_Permiso order by IdModulo, Orden