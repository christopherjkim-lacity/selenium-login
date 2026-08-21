using System;
using Microsoft.Data.SqlClient;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

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
            string selectSql = @"SELECT w.[Sign_WO_Id], a.[File_Path], a.[File_Name], a.[Att_Type]
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

                        var fullURLs = new List<string>();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            // Read and display results
                            while (reader.Read())
                            {
                                // Fetches column 0 (Sign_WO_Id)
                                string workOrderId = reader["Sign_WO_Id"].ToString();
                                Console.WriteLine($"Sign Work Order ID: {workOrderId}");
                                command.Parameters.AddWithValue("@WO_ID", workOrderId ?? (object)DBNull.Value);
                                string filePath = reader["File_Path"].ToString().Replace("~", "");
                                string fileName = reader["File_Name"].ToString();
                                string fileType = reader["Att_Type"].ToString();

                                if (!(string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(fileType)))
                                {
                                    string fullURL = $"http://myladot.lacity.org/eWork{filePath}{fileType}/{fileName}";
                                    Console.WriteLine($"Full URL For WO ID: {fullURL}");
                                    fullURLs.Add(fullURL);
                                }
                            }
                        }

                        eWorksSearch(fullURLs);
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine($"Database Error: {e.Message}");
            }
        }
        static void eWorksSearch(List<string> fullURLs)
        {
            var driver = new ChromeDriver();
            driver.Url = "http://myladot.lacity.org/eWork/Account/LogOn";

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            driver.Manage().Window.Maximize();

            // Enter credentials - lambda handles waiting for visibility natively
            var userNameInput = wait.Until(d => 
            {
                var element = d.FindElement(By.Id("UserName"));
                return element.Displayed ? element : null;
            });
            userNameInput.SendKeys("ckim");

            driver.FindElement(By.Id("Password")).SendKeys("18742361!Ladot");

            // Click Log On button
            driver.FindElement(By.XPath("//input[@value='Log On']")).Click();

            //Click on Search button
            wait.Until(d => d.FindElement(By.XPath("//a[@href='/eWork/Search']"))).Click();

            //Click on checkbox "cbALLWO"
            wait.Until(d => d.FindElement(By.Id("cbALLWO"))).Click();

            //Searches file paths if they exist in the eWorks database
            if (fullURLs.Count != 0){
                driver.SwitchTo().NewWindow(WindowType.Tab);
                bool firstUrl = true;
                foreach (var fullURL in fullURLs)
                {
                    if (string.IsNullOrWhiteSpace(fullURL))
                    {
                        continue;
                    }

                    if (!firstUrl)
                    {
                        driver.SwitchTo().NewWindow(WindowType.Tab);
                    }

                    driver.Navigate().GoToUrl(fullURL);
                    firstUrl = false;
                }
            }
            
        }
    }
}