CREATE TABLE [dbo].[SubscriptionHistory] (
    [id]        INT            IDENTITY (2737, 1) NOT NULL,
    [subscr_id] INT            DEFAULT ((0)) NOT NULL,
    [email]     NVARCHAR (200) DEFAULT (N'') NOT NULL,
    CONSTRAINT [PK_subscriptionhistory_id] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [subscriptionhistory$subscr_id] UNIQUE NONCLUSTERED ([subscr_id] ASC, [email] ASC)
);
