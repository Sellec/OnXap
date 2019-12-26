﻿CREATE TYPE [dbo].[TVP_MessageQueue] AS TABLE (
    [IdQueue]         INT            DEFAULT ((0)) NOT NULL,
    [IdMessageType]   INT            DEFAULT ((0)) NOT NULL,
    [Direction]       BIT            DEFAULT ((0)) NOT NULL,
    [DateCreate]      DATETIME       DEFAULT (getdate()) NOT NULL,
    [StateType]       TINYINT        DEFAULT ((0)) NOT NULL,
    [State]           NVARCHAR (200) NULL,
    [IdTypeComponent] INT            NULL,
    [DateChange]      DATETIME       NULL,
    [DateDelayed]     DATETIME       NULL,
    [MessageInfo]     NVARCHAR (MAX) NULL);




