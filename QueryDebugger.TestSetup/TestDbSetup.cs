using Dapper;
using Microsoft.Data.SqlClient;
using System;

namespace QueryDebugger.TestSetup
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionString = @"Server=(localdb)\mssqllocaldb;Database=master;Trusted_Connection=True;";
            var dbName = "QueryDebugDb";

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                
                try
                {
                    connection.Execute($"ALTER DATABASE {dbName} SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE {dbName};");
                    Console.WriteLine($"Dropped existing database {dbName}");
                }
                catch { }

                connection.Execute($"CREATE DATABASE {dbName};");
                Console.WriteLine($"Created database {dbName}");
            }

            connectionString = $@"Server=(localdb)\mssqllocaldb;Database={dbName};Trusted_Connection=True;";
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                connection.Execute(@"
                    CREATE TABLE TableAs (Id INT PRIMARY KEY, Name NVARCHAR(100));
                    CREATE TABLE TableBs (Id INT PRIMARY KEY, AId INT, Status NVARCHAR(50));
                    CREATE TABLE TableCs (Id INT PRIMARY KEY, AId INT, Created DATETIME);
                ");

                // 1. Match
                connection.Execute("INSERT INTO TableAs VALUES (1, 'Araba')");
                connection.Execute("INSERT INTO TableBs VALUES (1, 1, 'Active')");
                connection.Execute("INSERT INTO TableCs VALUES (1, 1, DATEADD(day, -5, GETUTCDATE()))");

                // 2. Fail Join B
                connection.Execute("INSERT INTO TableAs VALUES (2, 'Araba')");
                
                // 3. Fail Join C
                connection.Execute("INSERT INTO TableAs VALUES (3, 'Araba')");
                connection.Execute("INSERT INTO TableBs VALUES (3, 3, 'Active')");

                // 4. Fail Filter Name
                connection.Execute("INSERT INTO TableAs VALUES (4, 'Kamyon')");
                connection.Execute("INSERT INTO TableBs VALUES (4, 4, 'Active')");
                connection.Execute("INSERT INTO TableCs VALUES (4, 4, DATEADD(day, -5, GETUTCDATE()))");

                // 5. Fail Filter Status
                connection.Execute("INSERT INTO TableAs VALUES (5, 'Araba')");
                connection.Execute("INSERT INTO TableBs VALUES (5, 5, 'Inactive')");
                connection.Execute("INSERT INTO TableCs VALUES (5, 5, DATEADD(day, -5, GETUTCDATE()))");

                // 6. Fail Filter Date
                connection.Execute("INSERT INTO TableAs VALUES (6, 'Araba')");
                connection.Execute("INSERT INTO TableBs VALUES (6, 6, 'Active')");
                connection.Execute("INSERT INTO TableCs VALUES (6, 6, DATEADD(month, -2, GETUTCDATE()))");

                Console.WriteLine("Database setup complete.");
            }
        }
    }
}

