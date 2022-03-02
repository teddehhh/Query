using Query.Helpers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Query.Managers
{
    internal class DataProvider
    {
        private static DataProvider instance;
        private SqlConnection conn;
        public static DataProvider Instance => instance ?? new DataProvider();

        public DataProvider()
        {
            try
            {
                conn = new SqlConnection($@"Server=ted\SQLEXPRESS;Database=Furniture;User Id=sa");
                conn.Open();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void ConnectionClose()
        {
            conn.Close();
        }
        public List<Helpers.Attribute> GetAttributes()
        {
            SqlCommand sqlCommand = new SqlCommand("SELECT * FROM INFORMATION_SCHEMA.COLUMNS INNER JOIN INFORMATION_SCHEMA.TABLES on TABLES.TABLE_NAME=COLUMNS.TABLE_NAME WHERE TABLE_TYPE='BASE TABLE' AND TABLES.TABLE_NAME!='sysdiagrams'") { Connection = conn };
            SqlDataReader reader = sqlCommand.ExecuteReader();
            List<Helpers.Attribute> attributes = new List<Helpers.Attribute>();
            while (reader.Read())
            {
                attributes.Add(new Helpers.Attribute()
                {
                    Name = reader["COLUMN_NAME"].ToString(),
                    TableName = reader["TABLE_NAME"].ToString(),
                    Type = reader["DATA_TYPE"].ToString()
                });
            }
            reader.Close();
            return attributes;
        }

        public List<ForeignKey> GetForeignKeys()
        {
            List<ForeignKey> foreignKeys = new List<ForeignKey>();
            SqlCommand sqlCommand = new SqlCommand(@"SELECT tab1.name AS [table],
                    col1.name AS[column],
                    tab2.name AS[referenced_table],
                    col2.name AS[referenced_column]
                    FROM sys.foreign_key_columns fkc
                    INNER JOIN sys.objects obj
                        ON obj.object_id = fkc.constraint_object_id
                    INNER JOIN sys.tables tab1
                        ON tab1.object_id = fkc.parent_object_id
                    INNER JOIN sys.schemas sch
                        ON tab1.schema_id = sch.schema_id
                    INNER JOIN sys.columns col1
                        ON col1.column_id = parent_column_id AND col1.object_id = tab1.object_id
                    INNER JOIN sys.tables tab2
                        ON tab2.object_id = fkc.referenced_object_id
                    INNER JOIN sys.columns col2
                    ON col2.column_id = referenced_column_id AND col2.object_id = tab2.object_id") { Connection = conn };

            SqlDataReader reader = sqlCommand.ExecuteReader();
            while (reader.Read())
            {
                foreignKeys.Add(new ForeignKey()
                {
                    TableFrom = reader["table"].ToString(),
                    AttributeFrom = reader["column"].ToString(),
                    TableTo = reader["referenced_table"].ToString(),
                    AttributeTo = reader["referenced_column"].ToString()
                });
            }
            reader.Close();
            return foreignKeys;
        }
        public List<List<string>> RunQuery(string query)
        {
            List<List<string>> table = new List<List<string>>();
            SqlCommand sqlCommand = new SqlCommand(query) { Connection = conn };
            SqlDataReader reader = sqlCommand.ExecuteReader();
            table.Add(new List<string>());
            for (int i = 0; i < reader.FieldCount; i++)
            {
                table.First().Add(reader.GetName(i));
            }
            while (reader.Read())
            {
                table.Add(new List<string>());
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    table.Last().Add(reader[i].ToString());
                }
            }
            reader.Close();
            return table;
        }
    }
}
