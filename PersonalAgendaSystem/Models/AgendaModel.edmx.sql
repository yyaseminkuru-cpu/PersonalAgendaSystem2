
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 04/28/2026 22:50:01
-- Generated from EDMX file: C:\Users\Acer\source\repos\PersonalAgendaSystem\PersonalAgendaSystem\Models\AgendaModel.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [PersonalAgendaDB];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_UsersAgendaItems]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[AgendaItems] DROP CONSTRAINT [FK_UsersAgendaItems];
GO
IF OBJECT_ID(N'[dbo].[FK_AgendaItemsCategories]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[AgendaItems] DROP CONSTRAINT [FK_AgendaItemsCategories];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[Users]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Users];
GO
IF OBJECT_ID(N'[dbo].[Categories]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Categories];
GO
IF OBJECT_ID(N'[dbo].[AgendaItems]', 'U') IS NOT NULL
    DROP TABLE [dbo].[AgendaItems];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Users'
CREATE TABLE [dbo].[Users] (
    [UserID] int IDENTITY(1,1) NOT NULL,
    [FullName] nvarchar(max)  NOT NULL,
    [UserName] nvarchar(max)  NOT NULL,
    [Password] nvarchar(max)  NOT NULL,
    [Email] nvarchar(max)  NOT NULL,
    [Role] nvarchar(max)  NOT NULL,
    [IsActive] bit  NOT NULL
);
GO

-- Creating table 'Categories'
CREATE TABLE [dbo].[Categories] (
    [CategoryID] int IDENTITY(1,1) NOT NULL,
    [CategoryName] nvarchar(max)  NOT NULL,
    [IsActive] bit  NOT NULL
);
GO

-- Creating table 'AgendaItems'
CREATE TABLE [dbo].[AgendaItems] (
    [AgendaItemId] int IDENTITY(1,1) NOT NULL,
    [Title] nvarchar(max)  NOT NULL,
    [Description] nvarchar(max)  NOT NULL,
    [StartDate] datetime  NOT NULL,
    [EndDate] datetime  NOT NULL,
    [Priority] nvarchar(max)  NOT NULL,
    [Status] nvarchar(max)  NOT NULL,
    [IsApproved] bit  NOT NULL,
    [IsActive] bit  NOT NULL,
    [CreatedDate] datetime  NOT NULL,
    [Users_UserID] int  NOT NULL,
    [Categories_CategoryID] int  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [UserID] in table 'Users'
ALTER TABLE [dbo].[Users]
ADD CONSTRAINT [PK_Users]
    PRIMARY KEY CLUSTERED ([UserID] ASC);
GO

-- Creating primary key on [CategoryID] in table 'Categories'
ALTER TABLE [dbo].[Categories]
ADD CONSTRAINT [PK_Categories]
    PRIMARY KEY CLUSTERED ([CategoryID] ASC);
GO

-- Creating primary key on [AgendaItemId] in table 'AgendaItems'
ALTER TABLE [dbo].[AgendaItems]
ADD CONSTRAINT [PK_AgendaItems]
    PRIMARY KEY CLUSTERED ([AgendaItemId] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [Users_UserID] in table 'AgendaItems'
ALTER TABLE [dbo].[AgendaItems]
ADD CONSTRAINT [FK_UsersAgendaItems]
    FOREIGN KEY ([Users_UserID])
    REFERENCES [dbo].[Users]
        ([UserID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_UsersAgendaItems'
CREATE INDEX [IX_FK_UsersAgendaItems]
ON [dbo].[AgendaItems]
    ([Users_UserID]);
GO

-- Creating foreign key on [Categories_CategoryID] in table 'AgendaItems'
ALTER TABLE [dbo].[AgendaItems]
ADD CONSTRAINT [FK_AgendaItemsCategories]
    FOREIGN KEY ([Categories_CategoryID])
    REFERENCES [dbo].[Categories]
        ([CategoryID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_AgendaItemsCategories'
CREATE INDEX [IX_FK_AgendaItemsCategories]
ON [dbo].[AgendaItems]
    ([Categories_CategoryID]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------