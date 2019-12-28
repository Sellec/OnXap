CREATE TABLE [dbo].[menus] (
    [id]   INT            IDENTITY (8, 1) NOT NULL,
    [name] NVARCHAR (MAX) NOT NULL,
    [code] NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_menus_id] PRIMARY KEY CLUSTERED ([id] ASC)
);
