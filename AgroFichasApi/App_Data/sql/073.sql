use agrofichas
GO

update SYS_Version set CurrentVersion = 73
GO


create table EstadoSiembra
(
	IdEstadoSiembra int not null,
	Nombre varchar(50) not null,

	constraint [PK_EstadoSiembra] primary key (IdEstadoSiembra)
)
GO

insert into EstadoSiembra values (5, '(Desconocido)')
insert into EstadoSiembra values (10, 'Bueno')
insert into EstadoSiembra values (20, 'Regular')
insert into EstadoSiembra values (30, 'Malo')
GO

create table ImportanciaSeguimiento
(
	IdImportanciaSeguimiento int not null,
	Nombre varchar(50) not null,

	constraint [PK_ImportanciaSeguimiento] primary key (IdImportanciaSeguimiento)
)
GO

insert into ImportanciaSeguimiento values (10, 'Alta')
insert into ImportanciaSeguimiento values (20, 'Normal')
insert into ImportanciaSeguimiento values (30, 'Baja')
GO

alter table Ficha
	add IdEstadoSiembra int not null default(5),
	    IdImportanciaSeguimiento int not null default(20)
GO

alter table Ficha add constraint [FK_Ficha_EstadoSiembra] foreign key (IdEstadoSiembra) references EstadoSiembra
GO

alter table Ficha add constraint [FK_Ficha_ImportanciaSeguimiento] foreign key (IdImportanciaSeguimiento) references ImportanciaSeguimiento
GO


alter table FichaPreSiembra
	add IdEstadoSiembra int not null default(5),
	    IdImportanciaSeguimiento int not null default(20)
GO

alter table FichaPreSiembra add constraint [FK_FichaPreSiembra_EstadoSiembra] foreign key (IdEstadoSiembra) references EstadoSiembra
GO

alter table FichaPreSiembra add constraint [FK_FichaPreSiembra_ImportanciaSeguimiento] foreign key (IdImportanciaSeguimiento) references ImportanciaSeguimiento
GO

