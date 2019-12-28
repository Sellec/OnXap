CREATE TABLE [dbo].[Role] (
    [IdRole]       INT            IDENTITY (20, 1) NOT NULL,
    [NameRole]     NVARCHAR (200) CONSTRAINT [DF_Role_NameRole] DEFAULT (N'') NOT NULL,
    [IsHidden]     BIT            CONSTRAINT [DF_Role_IsHidden] DEFAULT ((0)) NOT NULL,
    [IdUserCreate] INT            CONSTRAINT [DF_Role_IdUserCreate] DEFAULT ((0)) NOT NULL,
    [DateCreate]   INT            CONSTRAINT [DF_Role_DateCreate] DEFAULT ((0)) NOT NULL,
    [IdUserChange] INT            CONSTRAINT [DF_Role_IdUserChange] DEFAULT ((0)) NOT NULL,
    [DateChange]   INT            CONSTRAINT [DF_Role_DateChange] DEFAULT ((0)) NOT NULL,
    [UniqueKey]    NVARCHAR (100) NULL,
    CONSTRAINT [PK_role_IdRole] PRIMARY KEY CLUSTERED ([IdRole] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [UniqueKey]
    ON [dbo].[Role]([UniqueKey] ASC) WHERE ([UniqueKey] IS NOT NULL);

