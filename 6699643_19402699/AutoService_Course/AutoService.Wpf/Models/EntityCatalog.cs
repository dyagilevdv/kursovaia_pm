using System;
using System.Collections.Generic;

namespace AutoService.Wpf.Models
{
    public enum InputKind { Text, Integer, Money, Date, Lookup }

    public sealed class GridColumn
    {
        public string Name { get; }
        public string Caption { get; }
        public double Width { get; }
        public GridColumn(string name, string caption, double width = 1)
        { Name = name; Caption = caption; Width = width; }
    }

    public sealed class FieldDefinition
    {
        public string Column { get; }
        public string Caption { get; }
        public InputKind Kind { get; }
        public bool Required { get; }
        public string LookupSql { get; }
        public int MaxLength { get; }
        public FieldDefinition(string column, string caption, InputKind kind = InputKind.Text,
                               bool required = false, string lookupSql = null, int maxLength = 50)
        { Column = column; Caption = caption; Kind = kind; Required = required;
          LookupSql = lookupSql; MaxLength = maxLength; }
    }

    public sealed class EntitySchema
    {
        public string Key { get; }
        public string Title { get; }
        public string Description { get; }
        public string Table { get; }
        public string ListSql { get; }
        public GridColumn[] Columns { get; }
        public FieldDefinition[] Fields { get; }
        public string[] SearchColumns { get; }

        public EntitySchema(string key, string title, string description, string table,
            string listSql, GridColumn[] columns, FieldDefinition[] fields, string[] searchColumns)
        { Key = key; Title = title; Description = description; Table = table;
          ListSql = listSql; Columns = columns; Fields = fields; SearchColumns = searchColumns; }
    }

    // SQL-шаблоны ниже заданы в коде и не составляются из пользовательского ввода.
    public static class EntityCatalog
    {
        private const string TownOptions = "SELECT id, town AS label FROM dbo.Towns ORDER BY town";
        private const string OwnerOptions =
            "SELECT id, LTRIM(RTRIM(COALESCE(surname,N'') + N' ' + COALESCE(name,N'') + N' ' + COALESCE(patronymic,N''))) AS label FROM dbo.Owners ORDER BY surname, name";
        private const string ModelOptions =
            "SELECT id, COALESCE(brand,N'') + N' ' + COALESCE(model,N'') AS label FROM dbo.Models ORDER BY brand, model";
        private const string ColorOptions = "SELECT id, color AS label FROM dbo.Colors ORDER BY color";
        private const string FuelOptions = "SELECT id, fuel_type AS label FROM dbo.Fuel_types ORDER BY fuel_type";
        private const string TransmissionOptions = "SELECT id, transmission AS label FROM dbo.Transmissions ORDER BY transmission";
        private const string CarOptions =
            "SELECT a.id, COALESCE(a.gosnumber,N'Без номера') + N' | ' + COALESCE(m.brand,N'') + N' ' + COALESCE(m.model,N'') AS label FROM dbo.Automobiles a LEFT JOIN dbo.Models m ON a.id_model=m.id ORDER BY a.gosnumber";
        private const string StatusOptions = "SELECT id, statusname AS label FROM dbo.Status ORDER BY id";
        private const string ReasonOptions = "SELECT id, reason AS label FROM dbo.Reasons ORDER BY reason";
        private const string UserOptions =
            "SELECT id, surname + N' ' + name AS label FROM dbo.Users ORDER BY surname, name";
        private const string WorkOptions = "SELECT id, name_of_work AS label FROM dbo.Work_price ORDER BY name_of_work";
        private const string PartOptions = "SELECT id, partname AS label FROM dbo.Zapchasti ORDER BY partname";

        private static FieldDefinition Txt(string key, string label, bool required = false) =>
            new FieldDefinition(key, label, InputKind.Text, required);
        private static FieldDefinition Num(string key, string label, bool required = false) =>
            new FieldDefinition(key, label, InputKind.Integer, required);
        private static FieldDefinition Ref(string key, string label, string sql, bool required = false) =>
            new FieldDefinition(key, label, InputKind.Lookup, required, sql);

        private static readonly Dictionary<string, EntitySchema> Items = new Dictionary<string, EntitySchema>
        {
            ["orders"] = new EntitySchema("orders", "Заказы на ремонт", "Создание заявок, учёт работ и контроль статусов", "Zakaz",
                @"SELECT z.*, a.gosnumber AS car, COALESCE(m.brand,N'') + N' ' + COALESCE(m.model,N'') AS model,
                         s.statusname AS status, r.reason AS reason, w.name_of_work AS service,
                         p.partname AS part, u.surname + N' ' + u.name AS employee
                  FROM dbo.Zakaz z
                  LEFT JOIN dbo.Automobiles a ON a.id=z.id_automobile
                  LEFT JOIN dbo.Models m ON m.id=a.id_model
                  LEFT JOIN dbo.Status s ON s.id=z.id_status
                  LEFT JOIN dbo.Reasons r ON r.id=z.id_reason
                  LEFT JOIN dbo.Work_price w ON w.id=z.uslugi_zakaza
                  LEFT JOIN dbo.Zapchasti p ON p.id=z.zapchasti_zakaza
                  LEFT JOIN dbo.Users u ON u.id=z.id_user
                  ORDER BY z.date_remont DESC, z.id DESC",
                new[] {new GridColumn("nomer_zakaza","№"),new GridColumn("date_remont","Дата",1.4),
                       new GridColumn("car","Госномер",1.4),new GridColumn("model","Модель",1.8),
                       new GridColumn("service","Услуга",2.2),new GridColumn("status","Статус",1.5),
                       new GridColumn("kolichestvo","Кол-во",1)},
                new[] {Num("nomer_zakaza","Номер заказа",true),Ref("id_automobile","Автомобиль",CarOptions,true),
                       new FieldDefinition("date_remont","Дата ремонта",InputKind.Date,true),
                       Ref("id_status","Статус",StatusOptions,true),Ref("id_reason","Причина обращения",ReasonOptions,true),
                       Txt("probeg","Пробег (км)",true),Ref("id_user","Сотрудник",UserOptions,true),
                       Ref("zapchasti_zakaza","Запчасть",PartOptions),Ref("uslugi_zakaza","Услуга",WorkOptions,true),
                       Num("kolichestvo","Количество",true)},
                new[] {"nomer_zakaza","car","model","service","status","reason","employee"}),

            ["cars"] = new EntitySchema("cars", "Автомобили", "Машины клиентов и их технические характеристики", "Automobiles",
                @"SELECT a.*, o.surname + N' ' + o.name AS owner_name,
                         COALESCE(m.brand,N'') + N' ' + COALESCE(m.model,N'') AS model_name,
                         c.color AS color_name, f.fuel_type AS fuel_name, t.transmission AS transmission_name
                  FROM dbo.Automobiles a
                  LEFT JOIN dbo.Owners o ON o.id=a.id_owner
                  LEFT JOIN dbo.Models m ON m.id=a.id_model
                  LEFT JOIN dbo.Colors c ON c.id=a.id_color
                  LEFT JOIN dbo.Fuel_types f ON f.id=a.id_fuel
                  LEFT JOIN dbo.Transmissions t ON t.id=a.id_transmission
                  ORDER BY a.id DESC",
                new[] {new GridColumn("gosnumber","Госномер",1.1),new GridColumn("model_name","Марка и модель",1.6),
                       new GridColumn("VIN","VIN",2),new GridColumn("owner_name","Владелец",1.6),
                       new GridColumn("fuel_name","Топливо"),new GridColumn("transmission_name","КПП",1.4)},
                new[] {Txt("gosnumber","Государственный номер",true),Txt("VIN","VIN",true),
                       Ref("id_owner","Владелец",OwnerOptions,true),Ref("id_model","Марка и модель",ModelOptions,true),
                       Ref("id_color","Цвет",ColorOptions),Ref("id_fuel","Тип топлива",FuelOptions),
                       Ref("id_transmission","Коробка передач",TransmissionOptions)},
                new[] {"gosnumber","VIN","owner_name","model_name","fuel_name"}),

            ["owners"] = new EntitySchema("owners", "Клиенты", "Контактные данные владельцев автомобилей", "Owners",
                @"SELECT o.*, t.town AS town_name FROM dbo.Owners o
                  LEFT JOIN dbo.Towns t ON t.id=o.id_town ORDER BY o.surname, o.name",
                new[] {new GridColumn("id","ID",0.6),new GridColumn("surname","Фамилия",1.4),
                       new GridColumn("name","Имя"),new GridColumn("patronymic","Отчество",1.4),
                       new GridColumn("town_name","Город",1.4),new GridColumn("telephone","Телефон",1.5)},
                new[] {Txt("surname","Фамилия",true),Txt("name","Имя",true),Txt("patronymic","Отчество"),
                       Ref("id_town","Город",TownOptions),Txt("telephone","Телефон",true)},
                new[] {"surname","name","patronymic","town_name","telephone"}),

            ["services"] = new EntitySchema("services", "Услуги", "Справочник выполняемых работ и их стоимости", "Work_price",
                "SELECT id, name_of_work, price FROM dbo.Work_price ORDER BY name_of_work",
                new[] {new GridColumn("id","ID",0.6),new GridColumn("name_of_work","Наименование услуги",3),
                       new GridColumn("price","Цена",1.2)},
                new[] {Txt("name_of_work","Наименование работы",true),
                       new FieldDefinition("price","Цена, руб.",InputKind.Money,true)},
                new[] {"name_of_work","price"}),

            ["parts"] = new EntitySchema("parts", "Запчасти", "Номенклатура применяемых деталей", "Zapchasti",
                "SELECT id, partname, manufacturer, country, year FROM dbo.Zapchasti ORDER BY partname",
                new[] {new GridColumn("id","ID",0.5),new GridColumn("partname","Деталь",2.2),
                       new GridColumn("manufacturer","Производитель",1.5),new GridColumn("country","Страна"),
                       new GridColumn("year","Год",0.7)},
                new[] {Txt("partname","Название детали",true),Txt("manufacturer","Производитель",true),
                       Txt("country","Страна",true),Num("year","Год выпуска",true)},
                new[] {"partname","manufacturer","country","year"})
        };

        public static EntitySchema Get(string key) => Items[key];
    }
}
