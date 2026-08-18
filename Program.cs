using System;
using Microsoft.Data.SqlClient;

namespace LoginTest
{
    public class StreetInputs
    {
        public static void Main(string[] args)
        {
            // Prompt for user inputs
            Console.Write("Enter Primary Street: ");
            string primaryStreet = Console.ReadLine();

            Console.Write("Enter Cross Street: ");
            string crossStreet = Console.ReadLine();

            // Connection string built for Microsoft Azure SQL Database
            string connectionString = new SqlConnectionStringBuilder
            {
                DataSource = "ladotbsgsqlsrv.westus.cloudapp.azure.com.database.windows.net,1433",
                InitialCatalog = "eWork",
                UserID = "cxKim",
                Password = "415294!Lacity",
                Encrypt = true,
                TrustServerCertificate = false,
                ConnectTimeout = 30
            }.ConnectionString;

            // Parameterized SELECT query to prevent SQL Injection
            string selectSql = @"SELECT [Sign_WO_Id] 
                                FROM [eWork].[dbo].[Sign_WO] 
                                WHERE [Primary_St] = @PrimaryStreet AND [Cross_St] = @CrossStreet";

            try
            {
                // 'using' statements handle automatic connection closing and disposal
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(selectSql, connection))
                    {
                        // Safely bind user parameters
                        command.Parameters.AddWithValue("@PrimaryStreet", primaryStreet ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@CrossStreet", crossStreet ?? (object)DBNull.Value);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            // Read and display results
                            while (reader.Read())
                            {
                                // Fetches column 0 (Sign_WO_Id)
                                string workOrderId = reader["Sign_WO_Id"].ToString();
                                Console.WriteLine($"Sign Work Order ID: {workOrderId}");
                            }
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine($"Database Error: {e.Message}");
            }
        }
    }
}