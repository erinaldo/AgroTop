USE agrofichas
GO

ALTER TABLE [CRM_Solicitud] ALTER COLUMN
	[ApiEndpoint] [varchar](8000) NOT NULL
GO