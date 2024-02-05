
using System.Data.SqlClient;

public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService()
    {
        _connectionString = "Server=EASYSOLUTIONS7;Database=Alimentation;Trusted_Connection=True;";
    }
    
        public async Task<List<Dictionary<string, object>>> ExecuteQueryAsync(string query)
    {
        List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        Dictionary<string, object> row = new Dictionary<string, object>();

                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            string columnName = reader.GetName(i);
                            object columnValue = reader.GetValue(i);
                            row[columnName] = columnValue;
                        }

                        result.Add(row);
                    }
                }
            }

            connection.Close();
        }

        return result;
    }
    public async Task<List<Dictionary<string, object>>> ExecuteQueryAsync(string query, object parameters = null)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (SqlCommand command = new SqlCommand(query, connection))
            {

                if (parameters != null)
                {
                    SetParameters(command, parameters);
                }

                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

                    while (await reader.ReadAsync())
                    {
                        Dictionary<string, object> row = new Dictionary<string, object>();

                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            string columnName = reader.GetName(i);
                            object columnValue = reader.GetValue(i);
                            row[columnName] = columnValue;
                        }

                        result.Add(row);
                    }

                    return result;
                }
            }
        }
    }
    public async Task<T> ExecuteScalarAsync<T>(string query, object parameters = null)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                if (parameters != null)
                {
                    SetParameters(command, parameters);
                }

                object result = await command.ExecuteScalarAsync();
                return (T)Convert.ChangeType(result, typeof(T));
            }
        }
    }
    public async Task<int> ExecuteNonQueryAsync(string query, object parameters = null)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                if (parameters != null)
                {
                    SetParameters(command, parameters);
                }

                return await command.ExecuteNonQueryAsync();
            }
        }
    }



    private void SetParameters(SqlCommand command, object parameters)
    {
        if (parameters != null)
        {
            foreach (var prop in parameters.GetType().GetProperties())
            {
                object? value = prop.GetValue(parameters);
                Console.WriteLine($"Setting parameter: {prop.Name} = {value}");

                try
                {
                    command.Parameters.AddWithValue($"@{prop.Name}", value ?? DBNull.Value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error setting parameter: {ex}");
                }
            }
        }
    }




}