CREATE TABLE [dbo].[Journal] (
    [IdJournalData]     INT            IDENTITY (1, 1) NOT NULL,
    [IdJournal]         INT            NOT NULL,
    [EventType]         TINYINT        CONSTRAINT [DF_Journal_EventType] DEFAULT ((1)) NOT NULL,
    [EventInfo]         NVARCHAR (300) NOT NULL,
    [EventInfoDetailed] NVARCHAR (MAX) NULL,
	[EventCode]	        INT            NULL,
    [ExceptionDetailed] NVARCHAR (MAX) NULL,
    [DateEvent]         DATETIME       NOT NULL,
    [IdUser] INT NULL , 
    [ItemLinkId] UNIQUEIDENTIFIER NULL, 
    CONSTRAINT [PK_Journal] PRIMARY KEY CLUSTERED ([IdJournalData] ASC), 
    CONSTRAINT [FK_Journal_UserBase] FOREIGN KEY ([IdUser]) REFERENCES [dbo].[UserBase] ([IdUser]) ON DELETE SET NULL, 
    CONSTRAINT [FK_Journal_ItemLink] FOREIGN KEY ([ItemLinkId]) REFERENCES [dbo].[ItemLink] ([LinkId]) ON DELETE CASCADE
);


GO

CREATE INDEX [IX_Journal_ItemLinkId] ON [dbo].[Journal] ([ItemLinkId]) WHERE ([ItemLinkId] IS NOT NULL)
