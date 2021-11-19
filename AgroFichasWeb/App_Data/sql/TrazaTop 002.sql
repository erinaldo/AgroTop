USE agrofichas
GO

ALTER TABLE SolicitudContrato ADD
	IdContrato INT NULL,
	IdEmpresa INT NULL,
	IdAgricultor INT NULL,
	IdTemporada INT NULL,
	CONSTRAINT [FK_SolicitudContrato_Contrato] FOREIGN KEY (IdContrato) REFERENCES Contrato,
	CONSTRAINT [FK_SolicitudContrato_Empresa] FOREIGN KEY (IdEmpresa) REFERENCES Empresa,
 	CONSTRAINT [FK_SolicitudContrato_Agricultor] FOREIGN KEY (IdAgricultor) REFERENCES Agricultor,
	CONSTRAINT [FK_SolicitudContrato_Temporada] FOREIGN KEY (IdTemporada) REFERENCES Temporada
GO

ALTER TABLE SolicitudContrato ADD
	VerificadoCRM BIT NOT NULL DEFAULT(0),
	VerificadoFichas BIT NOT NULL DEFAULT(0)
GO

ALTER TABLE SolicitudContrato DROP COLUMN
	Verificado

ALTER TABLE SolicitudContrato ADD
	Verificado AS CAST(CASE WHEN VerificadoCRM = 1 AND VerificadoFichas = 1 THEN 1 ELSE 0 END AS BIT)
GO