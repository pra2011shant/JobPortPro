using System;
using JobPortPro.Models;

namespace JobPortPro.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // Ensure database schema exists
            context.Database.EnsureCreated();

            // Execute Stored Procedures Creation / Update from SQL Script
            ApplyStoredProcedures(context);
        }

        private static void ApplyStoredProcedures(ApplicationDbContext context)
        {
            try
            {
                var sqlScriptPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Scripts", "StoredProcedures.sql");
                if (!System.IO.File.Exists(sqlScriptPath))
                {
                    // Fallback to project root directory
                    sqlScriptPath = System.IO.Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Data", "Scripts", "StoredProcedures.sql");
                }

                if (System.IO.File.Exists(sqlScriptPath))
                {
                    var sql = System.IO.File.ReadAllText(sqlScriptPath);
                    var batches = System.Text.RegularExpressions.Regex.Split(sql, @"^\s*GO\s*$", System.Text.RegularExpressions.RegexOptions.Multiline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                    var connection = Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.GetDbConnection(context.Database);
                    if (connection.State != System.Data.ConnectionState.Open)
                    {
                        connection.Open();
                    }

                    foreach (var batch in batches)
                    {
                        var trimmed = batch.Trim();
                        if (!string.IsNullOrWhiteSpace(trimmed))
                        {
                            using var cmd = connection.CreateCommand();
                            cmd.CommandText = trimmed;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Silently continue - stored procedures can also be executed directly via SQL Server Management Studio
            }
        }
    }
}
