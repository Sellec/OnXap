CREATE TYPE [dbo].[TVP_ItemLink] AS TABLE (
    [ItemIdType] INT              DEFAULT ((0)) NOT NULL,
    [ItemId]     INT              DEFAULT ((0)) NOT NULL,
    [ItemKey]    NVARCHAR (200)   DEFAULT ('') NOT NULL,
    [LinkId]     UNIQUEIDENTIFIER DEFAULT (NULL) NOT NULL,
    [IdUser]     INT              NULL,
    [DateCreate] DATETIME         DEFAULT (getdate()) NOT NULL);

