CREATE TABLE [dbo].[RolePermission] (
    [IdRole]       INT            CONSTRAINT [DF_RolePermission_IdRole] DEFAULT ((0)) NOT NULL,
    [IdModule]     INT            CONSTRAINT [DF_RolePermission_IdModule] DEFAULT ((0)) NOT NULL,
    [Permission]   NVARCHAR (200) CONSTRAINT [DF_RolePermission_Permission] DEFAULT (N'') NOT NULL,
    [IdUserChange] INT            CONSTRAINT [DF_RolePermission_IdUserChange] DEFAULT ((0)) NOT NULL,
    [DateChange]   INT            CONSTRAINT [DF_RolePermission_DateChange] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [rolepermission$IdRole] UNIQUE CLUSTERED ([IdRole] ASC, [IdModule] ASC, [Permission] ASC)
);
