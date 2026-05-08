SET QUOTED_IDENTIFIER OFF;
GO

IF DB_ID(N'PersonalAgendaDB') IS NULL
    CREATE DATABASE [PersonalAgendaDB];
GO

USE [PersonalAgendaDB];
GO

IF SCHEMA_ID(N'dbo') IS NULL
    EXECUTE(N'CREATE SCHEMA [dbo]');
GO

IF OBJECT_ID(N'[dbo].[FK_UsersAgendaItems]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[AgendaItems] DROP CONSTRAINT [FK_UsersAgendaItems];
GO

IF OBJECT_ID(N'[dbo].[FK_AgendaItemsCategories]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[AgendaItems] DROP CONSTRAINT [FK_AgendaItemsCategories];
GO

IF OBJECT_ID(N'[dbo].[AgendaItems]', 'U') IS NOT NULL
    DROP TABLE [dbo].[AgendaItems];
GO

IF OBJECT_ID(N'[dbo].[Users]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Users];
GO

IF OBJECT_ID(N'[dbo].[Categories]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Categories];
GO

CREATE TABLE [dbo].[Users] (
    [UserID] int IDENTITY(1,1) NOT NULL,
    [FullName] nvarchar(max) NOT NULL,
    [UserName] nvarchar(max) NOT NULL,
    [Password] nvarchar(max) NOT NULL,
    [Email] nvarchar(max) NOT NULL,
    [Role] nvarchar(max) NOT NULL,
    [IsActive] bit NOT NULL
);
GO

CREATE TABLE [dbo].[Categories] (
    [CategoryID] int IDENTITY(1,1) NOT NULL,
    [CategoryName] nvarchar(max) NOT NULL,
    [IsActive] bit NOT NULL
);
GO

CREATE TABLE [dbo].[AgendaItems] (
    [AgendaItemId] int IDENTITY(1,1) NOT NULL,
    [Title] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [StartDate] datetime NOT NULL,
    [EndDate] datetime NOT NULL,
    [Priority] nvarchar(max) NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [IsApproved] bit NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedDate] datetime NOT NULL,
    [Users_UserID] int NOT NULL,
    [Categories_CategoryID] int NOT NULL
);
GO

ALTER TABLE [dbo].[Users]
ADD CONSTRAINT [PK_Users]
PRIMARY KEY CLUSTERED ([UserID] ASC);
GO

ALTER TABLE [dbo].[Categories]
ADD CONSTRAINT [PK_Categories]
PRIMARY KEY CLUSTERED ([CategoryID] ASC);
GO

ALTER TABLE [dbo].[AgendaItems]
ADD CONSTRAINT [PK_AgendaItems]
PRIMARY KEY CLUSTERED ([AgendaItemId] ASC);
GO

ALTER TABLE [dbo].[AgendaItems]
ADD CONSTRAINT [FK_UsersAgendaItems]
FOREIGN KEY ([Users_UserID])
REFERENCES [dbo].[Users] ([UserID])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

CREATE INDEX [IX_FK_UsersAgendaItems]
ON [dbo].[AgendaItems] ([Users_UserID]);
GO

ALTER TABLE [dbo].[AgendaItems]
ADD CONSTRAINT [FK_AgendaItemsCategories]
FOREIGN KEY ([Categories_CategoryID])
REFERENCES [dbo].[Categories] ([CategoryID])
ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

CREATE INDEX [IX_FK_AgendaItemsCategories]
ON [dbo].[AgendaItems] ([Categories_CategoryID]);
GO

INSERT INTO [dbo].[Categories] ([CategoryName], [IsActive])
VALUES
(N'Ders', 1),
(N'Odev', 1),
(N'Sinav', 1),
(N'Kisisel', 1),
(N'Is', 1),
(N'Etkinlik', 1),
(N'Bulusma', 1),
(N'Diger', 1);
GO

INSERT INTO [dbo].[Users] ([FullName], [UserName], [Password], [Email], [Role], [IsActive])
VALUES
(N'Admin Kullanici', N'admin', N'5109', N'admin5109@gmail.com', N'Admin', 1),
(N'Normal Kullanici', N'user', N'1234', N'user1234@gmail.com', N'Kullanici', 1);
GO