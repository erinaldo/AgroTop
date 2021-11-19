USE AgroFichas
GO

ALTER TABLE [Temporada] ADD
	Inicio INT NULL,
	Termino INT NULL
GO

UPDATE [Temporada]
   SET Inicio = 2013, Termino = 2014
 WHERE [IdTemporada] = 1
GO

UPDATE [Temporada]
   SET Inicio = 2014, Termino = 2015
 WHERE [IdTemporada] = 2
GO

UPDATE [Temporada]
   SET Inicio = 2015, Termino = 2016
 WHERE [IdTemporada] = 3
GO

UPDATE [Temporada]
   SET Inicio = 2016, Termino = 2017
 WHERE [IdTemporada] = 4
GO

UPDATE [Temporada]
   SET Inicio = 2017, Termino = 2018
 WHERE [IdTemporada] = 5
GO

UPDATE [Temporada]
   SET Inicio = 2018, Termino = 2019
 WHERE [IdTemporada] = 6
GO

UPDATE [Temporada]
   SET Inicio = 2019, Termino = 2020
 WHERE [IdTemporada] = 7
GO

UPDATE [Temporada]
   SET Inicio = 2020, Termino = 2021
 WHERE [IdTemporada] = 8
GO

UPDATE [Temporada]
   SET Inicio = 2021, Termino = 2022
 WHERE [IdTemporada] = 9
GO