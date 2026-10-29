using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using AutoService.Wpf.Models;

namespace AutoService.Wpf.Data
{
    public sealed class LookupOption
    {
        public object Id { get; }
        public string Label { get; }
        public LookupOption(object id, string label) { Id = id; Label = label; }
    }

    public sealed class SqlRepository
    {
        private readonly string _connectionString;

        public SqlRepository()
        {
            var settings = ConfigurationManager.ConnectionStrings["AutoService"];
            if (settings == null || string.IsNullOrWhiteSpace(settings.ConnectionString))
                throw new ConfigurationErrorsException("В App.config не найдена строка подключения AutoService.");
            _connectionString = settings.ConnectionString;
        }

        public DataTable Query(string query)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            using (var adapter = new SqlDataAdapter(command))
            {
                var table = new DataTable();
                adapter.Fill(table);
                return table;
            }
        }

        public List<LookupOption> GetOptions(FieldDefinition field)
        {
            var options = new List<LookupOption>();
            if (!field.Required) options.Add(new LookupOption(null, "(не выбрано)"));
            foreach (DataRow row in Query(field.LookupSql).Rows)
                options.Add(new LookupOption(row["id"], Convert.ToString(row["label"])));
            return options;
        }

        public void Save(EntitySchema schema, int? id, IDictionary<string, object> values)
        {
            var names = schema.Fields.Select(f => "[" + f.Column + "]").ToArray();
            var parameters = schema.Fields.Select((f, i) => "@p" + i).ToArray();
            string sql;
            if (id.HasValue)
            {
                var set = names.Select((name, i) => name + "=" + parameters[i]);
                sql = "UPDATE [dbo].[" + schema.Table + "] SET " + string.Join(",", set) + " WHERE [id]=@id";
            }
            else
            {
                sql = "INSERT INTO [dbo].[" + schema.Table + "] (" + string.Join(",", names) + ") VALUES (" +
                      string.Join(",", parameters) + ")";
            }

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                for (int i = 0; i < schema.Fields.Length; i++)
                {
                    var field = schema.Fields[i];
                    command.Parameters.AddWithValue(parameters[i], values[field.Column] ?? (object)DBNull.Value);
                }
                if (id.HasValue) command.Parameters.Add("@id", SqlDbType.Int).Value = id.Value;
                connection.Open();
                int affected = command.ExecuteNonQuery();
                if (affected != 1) throw new DataException("Ожидалось изменение одной записи; получено: " + affected);
            }
        }

        public void Delete(EntitySchema schema, int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand("DELETE FROM [dbo].[" + schema.Table + "] WHERE [id]=@id", connection))
            {
                command.Parameters.Add("@id", SqlDbType.Int).Value = id;
                connection.Open();
                if (command.ExecuteNonQuery() != 1)
                    throw new DataException("Запись не найдена или уже удалена.");
            }
        }
    }
}
