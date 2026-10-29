/* Схема на основе исходного db_autoservice.sql.
   Создаёт структуру без привязки к путям MDF/LDF другого компьютера.
   Запускать в SSMS в режиме SQLCMD не требуется. */
USE [master];
GO
IF DB_ID(N'db_autoservice') IS NULL
    CREATE DATABASE [db_autoservice];
GO
USE [db_autoservice];
GO

IF OBJECT_ID(N'dbo.Towns',N'U') IS NULL
CREATE TABLE dbo.Towns (
  id nvarchar(50) NOT NULL CONSTRAINT PK_Towns PRIMARY KEY,
  town nvarchar(50) NULL
);
GO
IF OBJECT_ID(N'dbo.Colors',N'U') IS NULL
CREATE TABLE dbo.Colors (
  id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Colors PRIMARY KEY,
  color nvarchar(50) NULL
);
GO
IF OBJECT_ID(N'dbo.Fuel_types',N'U') IS NULL
CREATE TABLE dbo.Fuel_types (
  id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Fuel_types PRIMARY KEY,
  fuel_type nvarchar(50) NULL
);
GO
IF OBJECT_ID(N'dbo.Models',N'U') IS NULL
CREATE TABLE dbo.Models (
  id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Models PRIMARY KEY,
  brand nvarchar(50) NULL,
  model nvarchar(50) NULL
);
GO
IF OBJECT_ID(N'dbo.Transmissions',N'U') IS NULL
CREATE TABLE dbo.Transmissions (
  id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Transmissions PRIMARY KEY,
  transmission nvarchar(50) NULL
);
GO
IF OBJECT_ID(N'dbo.Owners',N'U') IS NULL
CREATE TABLE dbo.Owners (
  id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Owners PRIMARY KEY,
  surname nvarchar(50) NULL,
  name nvarchar(50) NULL,
  patronymic nvarchar(50) NULL,
  id_town nvarchar(50) NULL,
  telephone nvarchar(50) NULL,
  CONSTRAINT FK_Owners_Towns FOREIGN KEY(id_town) REFERENCES dbo.Towns(id)
);
GO
IF OBJECT_ID(N'dbo.Automobiles',N'U') IS NULL
CREATE TABLE dbo.Automobiles (
  id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Automobiles PRIMARY KEY,
  gosnumber nvarchar(50) NULL,
  id_owner int NULL,
  VIN nvarchar(50) NULL,
  id_model int NULL,
  id_color int NULL,
  id_fuel int NULL,
  id_transmission int NULL,
  CONSTRAINT FK_Automobiles_Owners FOREIGN KEY(id_owner) REFERENCES dbo.Owners(id),
  CONSTRAINT FK_Automobiles_Models FOREIGN KEY(id_model) REFERENCES dbo.Models(id),
  CONSTRAINT FK_Automobiles_Colors FOREIGN KEY(id_color) REFERENCES dbo.Colors(id),
  CONSTRAINT FK_Automobiles_Fuel FOREIGN KEY(id_fuel) REFERENCES dbo.Fuel_types(id),
  CONSTRAINT FK_Automobiles_Transmission FOREIGN KEY(id_transmission) REFERENCES dbo.Transmissions(id)
);
GO
IF OBJECT_ID(N'dbo.Reasons',N'U') IS NULL
CREATE TABLE dbo.Reasons (
  id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Reasons PRIMARY KEY,
  reason nvarchar(50) NULL
);
GO
IF OBJECT_ID(N'dbo.Status',N'U') IS NULL
CREATE TABLE dbo.Status (
  id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Status PRIMARY KEY,
  statusname nvarchar(50) NULL
);
GO
IF OBJECT_ID(N'dbo.Roles',N'U') IS NULL
CREATE TABLE dbo.Roles (
  id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Roles PRIMARY KEY,
  role_name nvarchar(50) NOT NULL
);
GO
IF OBJECT_ID(N'dbo.Users',N'U') IS NULL
CREATE TABLE dbo.Users (
  id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
  surname nvarchar(50) NOT NULL,
  name nvarchar(50) NOT NULL,
  patronymic nvarchar(50) NOT NULL,
  login nvarchar(50) NOT NULL,
  [password] nvarchar(50) NOT NULL
);
GO
IF OBJECT_ID(N'dbo.Roleuser',N'U') IS NULL
CREATE TABLE dbo.Roleuser (
  role_id int NOT NULL,
  user_id int NOT NULL,
  CONSTRAINT PK_Roleuser PRIMARY KEY(role_id,user_id),
  CONSTRAINT FK_Roleuser_Roles FOREIGN KEY(role_id) REFERENCES dbo.Roles(id),
  CONSTRAINT FK_Roleuser_Users FOREIGN KEY(user_id) REFERENCES dbo.Users(id)
);
GO
IF OBJECT_ID(N'dbo.Work_price',N'U') IS NULL
CREATE TABLE dbo.Work_price (
  id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Work_price PRIMARY KEY,
  name_of_work nvarchar(50) NULL,
  price decimal(6,0) NULL
);
GO
IF OBJECT_ID(N'dbo.Zapchasti',N'U') IS NULL
CREATE TABLE dbo.Zapchasti (
  id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Zapchasti PRIMARY KEY,
  partname nvarchar(50) NOT NULL,
  manufacturer nvarchar(50) NOT NULL,
  country nvarchar(50) NOT NULL,
  [year] int NOT NULL
);
GO
IF OBJECT_ID(N'dbo.Zakaz',N'U') IS NULL
CREATE TABLE dbo.Zakaz (
  id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Zakaz PRIMARY KEY,
  nomer_zakaza int NOT NULL,
  id_automobile int NOT NULL,
  date_remont datetime NOT NULL,
  id_status int NOT NULL,
  id_reason int NOT NULL,
  probeg nvarchar(50) NOT NULL,
  id_user int NOT NULL,
  zapchasti_zakaza int NULL,
  uslugi_zakaza int NOT NULL,
  kolichestvo int NOT NULL,
  CONSTRAINT FK_Zakaz_Automobiles FOREIGN KEY(id_automobile) REFERENCES dbo.Automobiles(id),
  CONSTRAINT FK_Zakaz_Status FOREIGN KEY(id_status) REFERENCES dbo.Status(id),
  CONSTRAINT FK_Zakaz_Reasons FOREIGN KEY(id_reason) REFERENCES dbo.Reasons(id),
  CONSTRAINT FK_Zakaz_Users FOREIGN KEY(id_user) REFERENCES dbo.Users(id),
  CONSTRAINT FK_Zakaz_Parts FOREIGN KEY(zapchasti_zakaza) REFERENCES dbo.Zapchasti(id),
  CONSTRAINT FK_Zakaz_Services FOREIGN KEY(uslugi_zakaza) REFERENCES dbo.Work_price(id)
);
GO

PRINT N'Структура db_autoservice подготовлена. Данные не изменены.';
GO
