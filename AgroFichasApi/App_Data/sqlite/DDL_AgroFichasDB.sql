CREATE TABLE "Proveedor"(
"Nombre" varchar(140) ,
"Email" varchar(140),
"Telefono1" varchar(140),
"ID" integer primary key not null ,
"LocalID" varchar(140) ,
"IsDirty" INTEGER,
"Habilitado" integer );
CREATE TABLE "Agricultor"(
"Rut" varchar(140) ,
"Nombre" varchar(140) ,
"Email" varchar(140),
"Fono1" varchar(140),
"Fono2" varchar(140),
"IdProveedor" INTEGER,
"ID" integer primary key not null ,
"LocalID" varchar(140) ,
"IsDirty" INTEGER);
CREATE TABLE "Comuna"(
"Nombre" varchar(140) ,
"Orden" integer ,
"ID" integer primary key not null ,
"LocalID" varchar(140) ,
"IsDirty" integer );
CREATE TABLE "Cultivo"(
"Nombre" varchar(140) ,
"ID" integer primary key not null ,
"LocalID" varchar(140) ,
"IsDirty" integer );
CREATE TABLE "EstadoSiembra"(
"Nombre" varchar(140) ,
"ID" integer primary key not null ,
"LocalID" varchar(140) ,
"IsDirty" integer );
CREATE TABLE "ImportanciaSeguimiento"(
"Nombre" varchar(140) ,
"ID" integer primary key not null ,
"LocalID" varchar(140) ,
"IsDirty" integer );
CREATE TABLE "Ficha"(
"IdPredio" integer ,
"IdTipoFicha" integer ,
"IdTemporada" integer ,
"Fecha" datetime ,
"Observaciones" varchar(140) ,
"IdEstadoSiembra" integer,
"IdImportanciaSeguimiento" integer,
"ID" integer primary key not null ,
"LocalID" varchar(140) ,
"IsDirty" integer );
CREATE TABLE "TipoFichaCultivo"(
"IdCultivo" integer ,
"IdTipoFicha" integer ,
"ID" integer primary key not null ,
"LocalID" varchar(140) ,
"IsDirty" integer );

CREATE TABLE "FichaPotrero"(
"IdPotrero" integer ,
"IdFicha" integer ,
"ID" integer primary key not null ,
"LocalID" varchar(140) ,
"IsDirty" integer );
CREATE TABLE "Potrero"(
"IdPredio" integer ,
"Nombre" varchar(140) ,
"Superficie" integer ,
"ID" integer primary key not null ,
"LocalID" varchar(140) ,
"IsDirty" integer );
CREATE TABLE "Predio"(
"IdAgricultor" integer ,
"Nombre" varchar(140) ,
"IdComuna" integer ,
"Lat" float,
"Lon" float,
"ID" integer primary key not null ,
"LocalID" varchar(140) ,
"IsDirty" integer );
CREATE TABLE "Quimico"(
"IdTipoRecomendacion" integer ,
"Nombre" varchar(140) ,
"Dosis" float ,
"IdUM" integer ,
"ID" integer primary key not null ,
"LocalID" varchar(140) ,
"IsDirty" INTEGER,
"Habilitado" integer );
CREATE TABLE "Recomendacion"(
"IdFicha" integer ,
"IdQuimico" integer ,
"FechaAplicacion" datetime ,
"Dosis" float ,
"IdUM" integer ,
"FerN" float ,
"FerP2O5" float ,
"FerKO2" float ,
"FerMgO" float ,
"FerS" float ,
"FerB" float ,
"FerZn" float ,
"FerCaO" float ,
"ID" integer primary key not null ,
"LocalID" varchar(140) ,
"IsDirty" integer );
CREATE TABLE "Siembra"(
"IdPredio" integer ,
"IdVariedad" integer ,
"FechaSiembra" datetime ,
"Dosis" float ,
"IdCultivoAnterior" integer ,
"IdTipoSiembra" integer ,
"IdTemporada" integer ,
"RendimientoEstimado" float,
"FechaCosechaEstimada" datetime,
"ID" integer primary key not null ,
"LocalID" varchar(140) ,
"IsDirty" integer );
CREATE TABLE "SiembraPotrero"(
"IdPotrero" integer ,
"IdTemporada" integer ,
"IdSiembra" integer ,
"ID" integer primary key not null ,
"LocalID" varchar(140) ,
"IsDirty" integer );
CREATE TABLE "Temporada"(
"Nombre" varchar(140) ,
"Activa" integer ,
"ID" integer primary key not null ,
"LocalID" varchar(140) ,
"IsDirty" integer );
CREATE TABLE "TipoFicha"(
"Nombre" varchar(140) ,
"Orden" integer ,
"ID" integer primary key not null ,
"LocalID" varchar(140) ,
"IsDirty" integer );
CREATE TABLE "TipoRecomendacion"(
"Nombre" varchar(140) ,
"ID" integer primary key not null ,
"LocalID" varchar(140) ,
"IsDirty" integer );
CREATE TABLE "TipoSiembra"(
"Nombre" varchar(140) ,
"ID" integer primary key not null ,
"LocalID" varchar(140) ,
"IsDirty" integer );
CREATE TABLE "UM"(
"Nombre" varchar(140) ,
"ID" integer primary key not null ,
"LocalID" varchar(140) ,
"IsDirty" integer );
CREATE TABLE "Variedad"(
"IdCultivo" integer ,
"Nombre" varchar(140) ,
"ID" integer primary key not null ,
"LocalID" varchar(140) ,
"IsDirty" integer,
"Habilitado" integer );
CREATE TABLE "IndexAgricultor"(
"Rut" varchar(140) ,
"ID" integer primary key not null ,
"LocalID" varchar(140) ,
"IsDirty" INTEGER);
CREATE TABLE FotoFicha(
"IdFicha" integer ,
"FileName" varchar(140) ,
"Observaciones" varchar(140) ,
"ID" integer primary key not null ,
"LocalID" varchar(140) ,
"IsDirty" integer );
CREATE TABLE "FichaPreSiembra"(
"IdPredio" integer ,
"IdTemporada" integer ,
"Fecha" datetime ,
"Observaciones" varchar(140) ,
"IdEstadoSiembra" integer,
"IdImportanciaSeguimiento" integer,
"ID" integer primary key not null ,
"LocalID" varchar(140) ,
"IsDirty" integer );
CREATE TABLE "FichaPreSiembraPotrero"(
"IdPotrero" integer ,
"IdFichaPreSiembra" integer ,
"ID" integer primary key not null ,
"LocalID" varchar(140) ,
"IsDirty" integer );
CREATE TABLE "RecomendacionPreSiembra"(
"IdFichaPreSiembra" integer ,
"IdQuimico" integer ,
"FechaAplicacion" datetime ,
"Dosis" float ,
"IdUM" integer ,
"FerN" float ,
"FerP2O5" float ,
"FerKO2" float ,
"FerMgO" float ,
"FerS" float ,
"FerB" float ,
"FerZn" float ,
"FerCaO" float ,
"ID" integer primary key not null ,
"LocalID" varchar(140) ,
"IsDirty" integer );
CREATE TABLE FotoFichaPreSiembra(
"IdFichaPreSiembra" integer ,
"FileName" varchar(140) ,
"Observaciones" varchar(140) ,
"ID" integer primary key not null ,
"LocalID" varchar(140) ,
"IsDirty" integer );
CREATE TABLE IntencionSiembra(
"IdAgricultor" integer ,
"IdTemporada" integer ,
"IdCultivo" integer ,
"IdComuna" integer ,
"PuntoEntrega" varchar(140) ,
"Cantidad" int,
"Superficie" int,
"Observaciones" varchar(140) ,
"ID" integer primary key not null ,
"LocalID" varchar(140) ,
"IsDirty" integer );
