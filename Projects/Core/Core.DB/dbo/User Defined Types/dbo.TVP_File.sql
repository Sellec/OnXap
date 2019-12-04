CREATE TYPE [dbo].[TVP_File] AS TABLE (
    [IdFile]       INT              DEFAULT ((0)) NOT NULL,
    [IdModule]     INT              DEFAULT ((0)) NOT NULL,
    [NameFile]     NVARCHAR (260)   DEFAULT ('') NOT NULL,
    [PathFile]     NVARCHAR (260)   DEFAULT ('') NOT NULL,
    [UniqueKey]    UNIQUEIDENTIFIER NULL,
    [CountUsage]   INT              DEFAULT ((0)) NOT NULL,
    [TypeCommon]   INT              DEFAULT ((0)) NOT NULL,
    [TypeConcrete] NVARCHAR (50)    NULL,
    [DateChange]   INT              DEFAULT ((0)) NOT NULL,
    [DateExpire]   DATETIME         NULL,
    [IdUserChange] INT              DEFAULT ((0)) NOT NULL,
    [IsRemoving]   BIT              DEFAULT ((0)) NOT NULL,
    [IsRemoved]    BIT              DEFAULT ((0)) NOT NULL);







