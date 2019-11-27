CREATE TYPE [dbo].[TVP_Sessions] AS TABLE (
    [SessionId]   VARCHAR (24)    DEFAULT ('') NOT NULL,
    [Created]     SMALLDATETIME   DEFAULT (getdate()) NOT NULL,
    [Expires]     SMALLDATETIME   DEFAULT (getdate()) NOT NULL,
    [LockDate]    SMALLDATETIME   DEFAULT (getdate()) NOT NULL,
    [LockId]      INT             DEFAULT ((0)) NOT NULL,
    [Locked]      BIT             DEFAULT ((0)) NOT NULL,
    [ItemContent] VARBINARY (MAX) NULL,
    [IdUser]      INT             DEFAULT ((0)) NOT NULL);



