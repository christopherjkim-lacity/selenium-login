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
                DataSource = "ladotbsgsqlsrv.westus.cloudapp.azure.com",
                InitialCatalog = "eWork",
                UserID = "cxKim",
                Password = "415294!Lacity",
                Encrypt = true,
                TrustServerCertificate = true,
                ConnectTimeout = 30
            }.ConnectionString;

            // Parameterized SELECT query to prevent SQL Injection
            string selectSql = @"SELECT w.[Sign_WO_Id], a.[File_Path]
                    FROM [eWork].[dbo].[Sign_WO] w LEFT JOIN [eWork].[dbo].[Attachment] a
                    ON  w.[Sign_WO_Id] = a.[WO_Id]
                    WHERE (w.[Primary_St] = @PrimaryStreet AND w.[Cross_St] = @CrossStreet)
                       OR (w.[Primary_St] = @CrossStreet AND w.[Cross_St] = @PrimaryStreet)";

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
                                command.Parameters.AddWithValue("@WO_ID", workOrderId ?? (object)DBNull.Value);
                                string filePath = reader["File_Path"].ToString();
                                Console.WriteLine($"File Path For WO ID: {filePath}");
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