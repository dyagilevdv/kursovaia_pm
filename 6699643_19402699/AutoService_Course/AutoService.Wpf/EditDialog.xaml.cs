using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AutoService.Wpf.Data;
using AutoService.Wpf.Models;

namespace AutoService.Wpf
{
    public partial class EditDialog : Window
    {
        private readonly EntitySchema _schema;
        private readonly SqlRepository _repository;
        private readonly int? _id;
        private readonly Dictionary<string, Control> _editors = new Dictionary<string, Control>();

        public EditDialog(EntitySchema schema, SqlRepository repository, DataRowView existing)
        {
            InitializeComponent();
            _schema = schema;
            _repository = repository;
            _id = existing == null ? (int?)null : Convert.ToInt32(existing["id"]);
            Heading.Text = (_id.HasValue ? "Изменить: " : "Добавить: ") + schema.Title.ToLowerInvariant();
            Title = Heading.Text;

            foreach (var field in schema.Fields)
            {
                object initial = existing == null ? null : existing[field.Column];
                if (initial == DBNull.Value) initial = null;
                var title = new TextBlock
                {
                    Text = field.Caption + (field.Required ? " *" : ""),
                    Margin = new Thickness(0, 0, 0, 5),
                    Foreground = new SolidColorBrush(Color.FromRgb(37, 58, 74)),
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold
                };
                FieldsPanel.Children.Add(title);

                Control editor;
                if (field.Kind == InputKind.Lookup)
                {
                    var options = repository.GetOptions(field);
                    var box = new ComboBox
                    {
                        ItemsSource = options,
                        DisplayMemberPath = "Label",
                        SelectedValuePath = "Id",
                        MinHeight = 36
                    };
                    if (initial != null)
                        box.SelectedItem = options.FirstOrDefault(o => Convert.ToString(o.Id) == Convert.ToString(initial));
                    else if (!field.Required && options.Count > 0)
                        box.SelectedIndex = 0;
                    editor = box;
                }
                else if (field.Kind == InputKind.Date)
                {
                    editor = new DatePicker
                    {
                        SelectedDate = initial == null ? DateTime.Today : (DateTime)initial,
                        MinHeight = 36
                    };
                }
                else
                {
                    editor = new TextBox
                    {
                        Text = initial == null ? "" : Convert.ToString(initial, CultureInfo.CurrentCulture),
                        MinHeight = 36
                    };
                }
                editor.Margin = new Thickness(0, 0, 0, 14);
                _editors[field.Column] = editor;
                FieldsPanel.Children.Add(editor);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var values = new Dictionary<string, object>();
            foreach (var field in _schema.Fields)
            {
                object value;
                string error;
                if (!ReadValue(field, _editors[field.Column], out value, out error))
                {
                    MessageBox.Show(error, "Проверьте данные", MessageBoxButton.OK, MessageBoxImage.Warning);
                    _editors[field.Column].Focus();
                    return;
                }
                values[field.Column] = value;
            }

            try
            {
                _repository.Save(_schema, _id, values);
                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось сохранить запись:\n" + ex.GetBaseException().Message,
                                "Ошибка сохранения", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static bool ReadValue(FieldDefinition field, Control editor, out object value, out string error)
        {
            value = null;
            error = "";
            if (field.Kind == InputKind.Lookup)
            {
                var option = ((ComboBox)editor).SelectedItem as LookupOption;
                value = option == null ? null : option.Id;
                if (field.Required && value == null)
                    error = "Выберите значение поля «" + field.Caption + "».";
            }
            else if (field.Kind == InputKind.Date)
            {
                value = ((DatePicker)editor).SelectedDate;
                if (field.Required && value == null)
                    error = "Выберите дату для поля «" + field.Caption + "».";
            }
            else
            {
                string text = ((TextBox)editor).Text.Trim();
                if (string.IsNullOrEmpty(text))
                {
                    if (field.Required) error = "Заполните поле «" + field.Caption + "».";
                    return error.Length == 0;
                }
                if (field.Kind == InputKind.Integer)
                {
                    int number;
                    if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentCulture, out number) || number < 0)
                        error = "Поле «" + field.Caption + "» должно содержать целое неотрицательное число.";
                    else if ((field.Column == "kolichestvo" || field.Column == "nomer_zakaza") && number == 0)
                        error = "Поле «" + field.Caption + "» должно быть больше нуля.";
                    else if (field.Column == "year" && (number < 1900 || number > DateTime.Today.Year + 1))
                        error = "Укажите год выпуска в допустимом диапазоне.";
                    else value = number;
                }
                else if (field.Kind == InputKind.Money)
                {
                    decimal amount;
                    if (!decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out amount) &&
                        !decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out amount))
                        error = "Поле «" + field.Caption + "» должно содержать число.";
                    else if (amount < 0 || amount > 999999 || amount != decimal.Truncate(amount))
                        error = "Цена должна быть целым числом от 0 до 999999 (тип DECIMAL(6,0) в исходной БД).";
                    else value = amount;
                }
                else if (text.Length > field.MaxLength)
                    error = "Поле «" + field.Caption + "» длиннее " + field.MaxLength + " символов.";
                else value = text;
            }
            return error.Length == 0;
        }
    }
}
