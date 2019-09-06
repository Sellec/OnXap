﻿CREATE TYPE [dbo].[TVP_Page] AS TABLE (
    [id]             INT           DEFAULT ((0)) NOT NULL,
    [category]       INT           DEFAULT ((-1)) NULL,
    [subs_id]        VARCHAR (MAX) DEFAULT ('') NOT NULL,
    [subs_order]     VARCHAR (MAX) DEFAULT ('') NOT NULL,
    [status]         SMALLINT      DEFAULT ((0)) NOT NULL,
    [language]       VARCHAR (20)  DEFAULT (N'') NOT NULL,
    [name]           VARCHAR (MAX) DEFAULT ('') NOT NULL,
    [urlname]        VARCHAR (MAX) DEFAULT ('') NOT NULL,
    [body]           VARCHAR (MAX) DEFAULT ('') NOT NULL,
    [parent]         SMALLINT      DEFAULT ((-1)) NOT NULL,
    [order]          INT           DEFAULT ((0)) NOT NULL,
    [photo]          VARCHAR (MAX) DEFAULT ('') NOT NULL,
    [count_views]    INT           DEFAULT ((0)) NOT NULL,
    [comments_count] INT           DEFAULT ((0)) NOT NULL,
    [pages_gallery]  INT           DEFAULT ((-1)) NOT NULL,
    [news_id]        INT           DEFAULT ((0)) NOT NULL,
    [seo_title]      VARCHAR (255) DEFAULT (N'') NOT NULL,
    [seo_descr]      VARCHAR (MAX) DEFAULT ('') NOT NULL,
    [seo_kw]         VARCHAR (MAX) DEFAULT ('') NOT NULL,
    [ajax_name]      VARCHAR (255) DEFAULT (N'') NOT NULL);

