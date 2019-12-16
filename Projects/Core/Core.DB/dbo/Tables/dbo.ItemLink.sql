CREATE TABLE [dbo].[ItemLink]
(
    [ItemIdType] INT NOT NULL, 
	[ItemId] INT NOT NULL , 
    [ItemKey] NVARCHAR(200) NOT NULL, 
    [LinkId] UNIQUEIDENTIFIER NOT NULL, 
    [IdUser] INT NULL, 
    [DateCreate] DATETIME NOT NULL, 
    PRIMARY KEY ([ItemId], [ItemIdType], [ItemKey]), 
    CONSTRAINT [FK_ItemLink_User] FOREIGN KEY ([IdUser]) REFERENCES [dbo].[users] ([id])
)

GO

CREATE UNIQUE INDEX [IX_ItemLink_Column] ON [dbo].[ItemLink] ([LinkId])
