CREATE TABLE [dbo].[CustomFieldsScheme] (
    [IdScheme]   INT            IDENTITY (1, 1) NOT NULL,
    [IdModule]   INT            NOT NULL,
    [NameScheme] NVARCHAR (200) NOT NULL,
    [UniqueKey]  NVARCHAR (500) NULL,
    CONSTRAINT [PK_CustomFieldsScheme] PRIMARY KEY CLUSTERED ([IdScheme] ASC)
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [UniqueKey]
    ON [dbo].[CustomFieldsScheme]([UniqueKey] ASC) WHERE ([UniqueKey] IS NOT NULL);

