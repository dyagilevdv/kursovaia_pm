/* НЕОБЯЗАТЕЛЬНЫЕ тестовые данные для пустой учебной базы.
   Не запускать на рабочей базе предприятия.
   Все контакты и персоналии ниже вымышленные.
   Колонка Users.password наследована от исходной схемы и НЕ используется для входа. */
USE [db_autoservice];
GO
IF NOT EXISTS(SELECT 1 FROM dbo.Towns WHERE id=N'01')
  INSERT INTO dbo.Towns(id,town) VALUES(N'01',N'Учебный город');
IF NOT EXISTS(SELECT 1 FROM dbo.Models WHERE brand=N'Lada' AND model=N'Vesta')
  INSERT INTO dbo.Models(brand,model) VALUES(N'Lada',N'Vesta');
IF NOT EXISTS(SELECT 1 FROM dbo.Colors WHERE color=N'Белый')
  INSERT INTO dbo.Colors(color) VALUES(N'Белый');
IF NOT EXISTS(SELECT 1 FROM dbo.Fuel_types WHERE fuel_type=N'Бензин')
  INSERT INTO dbo.Fuel_types(fuel_type) VALUES(N'Бензин');
IF NOT EXISTS(SELECT 1 FROM dbo.Transmissions WHERE transmission=N'Механическая')
  INSERT INTO dbo.Transmissions(transmission) VALUES(N'Механическая');
IF NOT EXISTS(SELECT 1 FROM dbo.Status WHERE statusname=N'Принят')
  INSERT INTO dbo.Status(statusname) VALUES(N'Принят');
IF NOT EXISTS(SELECT 1 FROM dbo.Status WHERE statusname=N'В работе')
  INSERT INTO dbo.Status(statusname) VALUES(N'В работе');
IF NOT EXISTS(SELECT 1 FROM dbo.Status WHERE statusname=N'Выполнен')
  INSERT INTO dbo.Status(statusname) VALUES(N'Выполнен');
IF NOT EXISTS(SELECT 1 FROM dbo.Reasons WHERE reason=N'Плановое ТО')
  INSERT INTO dbo.Reasons(reason) VALUES(N'Плановое ТО');
IF NOT EXISTS(SELECT 1 FROM dbo.Work_price WHERE name_of_work=N'Замена моторного масла')
  INSERT INTO dbo.Work_price(name_of_work,price) VALUES(N'Замена моторного масла',1800);
IF NOT EXISTS(SELECT 1 FROM dbo.Zapchasti WHERE partname=N'Масляный фильтр' AND manufacturer=N'Учебный')
  INSERT INTO dbo.Zapchasti(partname,manufacturer,country,[year]) VALUES(N'Масляный фильтр',N'Учебный',N'Россия',2025);
IF NOT EXISTS(SELECT 1 FROM dbo.Users WHERE login=N'demo_operator')
  INSERT INTO dbo.Users(surname,name,patronymic,login,[password])
  VALUES(N'Учебный',N'Оператор',N'Системы',N'demo_operator',N'NOT_USED_FOR_LOGIN');
IF NOT EXISTS(SELECT 1 FROM dbo.Owners WHERE surname=N'Примеров' AND telephone=N'000-000-0000')
  INSERT INTO dbo.Owners(surname,name,patronymic,id_town,telephone)
  VALUES(N'Примеров',N'Алексей',N'Учебный',N'01',N'000-000-0000');

DECLARE @owner int = (SELECT TOP 1 id FROM dbo.Owners WHERE surname=N'Примеров' AND telephone=N'000-000-0000' ORDER BY id);
DECLARE @model int = (SELECT TOP 1 id FROM dbo.Models WHERE brand=N'Lada' AND model=N'Vesta' ORDER BY id);
DECLARE @color int = (SELECT TOP 1 id FROM dbo.Colors WHERE color=N'Белый' ORDER BY id);
DECLARE @fuel int = (SELECT TOP 1 id FROM dbo.Fuel_types WHERE fuel_type=N'Бензин' ORDER BY id);
DECLARE @transmission int = (SELECT TOP 1 id FROM dbo.Transmissions WHERE transmission=N'Механическая' ORDER BY id);
IF NOT EXISTS(SELECT 1 FROM dbo.Automobiles WHERE VIN=N'DEMO-VIN-ONLY-00001')
  INSERT INTO dbo.Automobiles(gosnumber,id_owner,VIN,id_model,id_color,id_fuel,id_transmission)
  VALUES(N'А000АА',@owner,N'DEMO-VIN-ONLY-00001',@model,@color,@fuel,@transmission);

DECLARE @auto int = (SELECT TOP 1 id FROM dbo.Automobiles WHERE VIN=N'DEMO-VIN-ONLY-00001' ORDER BY id);
DECLARE @status int = (SELECT TOP 1 id FROM dbo.Status WHERE statusname=N'Принят' ORDER BY id);
DECLARE @reason int = (SELECT TOP 1 id FROM dbo.Reasons WHERE reason=N'Плановое ТО' ORDER BY id);
DECLARE @user int = (SELECT TOP 1 id FROM dbo.Users WHERE login=N'demo_operator' ORDER BY id);
DECLARE @work int = (SELECT TOP 1 id FROM dbo.Work_price WHERE name_of_work=N'Замена моторного масла' ORDER BY id);
DECLARE @part int = (SELECT TOP 1 id FROM dbo.Zapchasti WHERE partname=N'Масляный фильтр' AND manufacturer=N'Учебный' ORDER BY id);
IF NOT EXISTS(SELECT 1 FROM dbo.Zakaz WHERE nomer_zakaza=1001)
  INSERT INTO dbo.Zakaz(nomer_zakaza,id_automobile,date_remont,id_status,id_reason,probeg,id_user,zapchasti_zakaza,uslugi_zakaza,kolichestvo)
  VALUES(1001,@auto,GETDATE(),@status,@reason,N'75000',@user,@part,@work,1);
GO
PRINT N'Учебные справочники и пример заказа готовы.';
