CREATE TABLE [dbo].[Subscription] (
    [id]             INT            IDENTITY (752, 1) NOT NULL,
    [name]           NVARCHAR (200) DEFAULT (N'') NOT NULL,
    [description]    NVARCHAR (MAX) NOT NULL,
    [status]         SMALLINT       DEFAULT ((0)) NOT NULL,
    [AllowSubscribe] TINYINT        DEFAULT ((1)) NOT NULL,
    [UniqueKey]      NVARCHAR (100) DEFAULT (NULL) NULL,
    CONSTRAINT [PK_subscription_id] PRIMARY KEY CLUSTERED ([id] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [subscription$UniqueKey]
    ON [dbo].[Subscription]([UniqueKey] ASC) WHERE ([UniqueKey] IS NOT NULL);
