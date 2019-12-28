CREATE TABLE [dbo].[passwords_remember] (
    [id]         INT           IDENTITY (180, 1) NOT NULL,
    [user_id]    INT           DEFAULT ((0)) NOT NULL,
    [code]       NVARCHAR (32) DEFAULT (N'') NOT NULL,
    [DateCreate] DATETIME      CONSTRAINT [DF_passwords_remember_DateCreate] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_passwords_remember_id] PRIMARY KEY CLUSTERED ([id] ASC)
);

GO
CREATE NONCLUSTERED INDEX [user_id]
    ON [dbo].[passwords_remember]([user_id] ASC);
