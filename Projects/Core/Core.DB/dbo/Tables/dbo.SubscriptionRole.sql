CREATE TABLE [dbo].[SubscriptionRole] (
    [IdSubscription] INT CONSTRAINT [DF__subscript__IdSub__04659998] DEFAULT ((0)) NOT NULL,
    [IdRole]         INT CONSTRAINT [DF__subscript__IdRol__0559BDD1] DEFAULT ((0)) NOT NULL,
    [IdUserChange]   INT CONSTRAINT [DF__subscript__IdUse__064DE20A] DEFAULT ((0)) NOT NULL,
    [DateChange]     INT CONSTRAINT [DF__subscript__DateC__07420643] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [subscriptionrole$IdSubscription] UNIQUE CLUSTERED ([IdSubscription] ASC, [IdRole] ASC)
);
