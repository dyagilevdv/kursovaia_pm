using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using AutoService.Wpf.Data;
using AutoService.Wpf.Models;

namespace AutoService.Wpf
{
    public partial class MainWindow : Window
    {
        private readonly SqlRepository _repository = new SqlRepository();
        private EntitySchema _current;
        private DataView _view;

        public MainWindow()
        {
            InitializeComponent();
            OpenSection("orders");
        }

        private void Navigate_Click(object sender, RoutedEventArgs e)
        {
            var key = Convert.ToString(((Button)sender).Tag);
            OpenSection(key);
        }

        private void OpenSection(string key)
        {
            _current = EntityCatalog.Get(key);
            SectionTitle.Text = _current.Title;
            SectionHint.Text = _current.Description;
            SearchBox.Clear();
            LoadRecords();
        }

        private void LoadRecords()
        {
            if (_current == null) return;
            try
            {
                var table = _repository.Query(_current.ListSql);
                _view = table.DefaultView;
                EntriesGrid.ItemsSource = _view;
                EntriesGrid.Columns.Clear();
                foreach (var col in _current.Columns)
                {
                    EntriesGrid.Columns.Add(new DataGridTextColumn
                    {
                        Header = col.Caption,
                        Binding = new Binding(col.Name),
                        Width = new DataGridLength(col.Width, DataGridLengthUnitType.Star)
                    });
                }
                ApplySearch();
                StatusText.Text = "Записей: " + _view.Count + ". Раздел: " + _current.Title;
            }
            catch (Exception ex)
            {
                EntriesGrid.ItemsSource = null;
                EntriesGrid.Columns.Clear();
                _view = null;
                StatusText.Text = "Нет соединения с базой данных";
                MessageBox.Show("Не удалось загрузить данные. Проверьте SQL Server, базу db_autoservice и App.config.\n\n" +
                                ex.GetBaseException().Message, "Ошибка подключения", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            UpdateButtons();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplySearch();
        }

        private void ApplySearch()
        {
            if (_current == null || _view == null || SearchBox == null) return;
            string value = SearchBox.Text.Trim();
            if (string.IsNullOrEmpty(value))
                _view.RowFilter = "";
            else
            {
                // Экранирование специальных символов выражения DataView.RowFilter.
                value = value.Replace("'", "''").Replace("[", "[[]").Replace("%", "[%]").Replace("*", "[*]");
                _view.RowFilter = string.Join(" OR ", _current.SearchColumns.Select(c =>
                    "CONVERT([" + c + "], 'System.String') LIKE '%" + value + "%'"));
            }
            StatusText.Text = "Найдено записей: " + _view.Count;
        }

        private DataRowView SelectedRow => EntriesGrid.SelectedItem as DataRowView;

        private void UpdateButtons()
        {
            if (EditButton == null || DeleteButton == null) return;
            bool selected = SelectedRow != null;
            EditButton.IsEnabled = selected;
            DeleteButton.IsEnabled = selected;
        }

        private void EntriesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateButtons();

        private void Add_Click(object sender, RoutedEventArgs e) => OpenEditor(null);

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedRow != null) OpenEditor(SelectedRow);
        }

        private void EntriesGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SelectedRow != null) OpenEditor(SelectedRow);
        }

        private void OpenEditor(DataRowView selected)
        {
            try
            {
                var editor = new EditDialog(_current, _repository, selected) { Owner = this };
                if (editor.ShowDialog() == true) LoadRecords();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось открыть форму:\n" + ex.GetBaseException().Message,
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedRow == null) return;
            int id = Convert.ToInt32(SelectedRow["id"]);
            if (MessageBox.Show("Удалить запись №" + id + " из раздела «" + _current.Title + "»?",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;
            try
            {
                _repository.Delete(_current, id);
                LoadRecords();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось удалить запись. Возможно, с ней связаны другие записи.\n\n" +
                                ex.GetBaseException().Message, "Ошибка удаления",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e) => LoadRecords();
    }
}
