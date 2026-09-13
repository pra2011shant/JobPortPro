using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using JobPortPro.Data;

namespace JobPortPro.Services
{
    /// <summary>
    /// Lightweight, high-performance executor for SQL Server Stored Procedures.
    /// Eliminates direct table queries while maintaining 100% parameter safety.
    /// </summary>
    public class StoredProcedureExecutor : IStoredProcedureExecutor
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StoredProcedureExecutor> _logger;

        public StoredProcedureExecutor(ApplicationDbContext context, ILogger<StoredProcedureExecutor> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<T>> ExecuteStoredProcedureListAsync<T>(
            string procedureName, 
            IEnumerable<SqlParameter>? parameters, 
            Func<DbDataReader, T> mapFunction)
        {
            var results = new List<T>();
            var connection = _context.Database.GetDbConnection();

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                await using var command = connection.CreateCommand();
                command.CommandText = procedureName;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 30;

                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.Add(param);
                    }
                }

                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    results.Add(mapFunction(reader));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing Stored Procedure {ProcedureName}", procedureName);
                throw;
            }

            return results;
        }

        public async Task<T?> ExecuteStoredProcedureSingleAsync<T>(
            string procedureName, 
            IEnumerable<SqlParameter>? parameters, 
            Func<DbDataReader, T> mapFunction)
        {
            var connection = _context.Database.GetDbConnection();

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                await using var command = connection.CreateCommand();
                command.CommandText = procedureName;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 30;

                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.Add(param);
                    }
                }

                await using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return mapFunction(reader);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing single result Stored Procedure {ProcedureName}", procedureName);
                throw;
            }

            return default;
        }

        public async Task<int> ExecuteStoredProcedureNonQueryAsync(
            string procedureName, 
            IEnumerable<SqlParameter>? parameters)
        {
            var connection = _context.Database.GetDbConnection();

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                await using var command = connection.CreateCommand();
                command.CommandText = procedureName;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 30;

                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.Add(param);
                    }
                }

                return await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing non-query Stored Procedure {ProcedureName}", procedureName);
                throw;
            }
        }

        public async Task<(List<T> Results, int OutputTotalCount)> ExecuteStoredProcedurePagedListAsync<T>(
            string procedureName, 
            IEnumerable<SqlParameter> inputParameters, 
            string outputCountParamName, 
            Func<DbDataReader, T> mapFunction)
        {
            var results = new List<T>();
            int totalCount = 0;
            var connection = _context.Database.GetDbConnection();

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                await using var command = connection.CreateCommand();
                command.CommandText = procedureName;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 30;

                var outputParam = new SqlParameter(outputCountParamName, SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };

                foreach (var param in inputParameters)
                {
                    command.Parameters.Add(param);
                }
                command.Parameters.Add(outputParam);

                await using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        results.Add(mapFunction(reader));
                    }
                }

                if (outputParam.Value != DBNull.Value && outputParam.Value != null)
                {
                    totalCount = Convert.ToInt32(outputParam.Value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing paginated Stored Procedure {ProcedureName}", procedureName);
                throw;
            }

            return (results, totalCount);
        }
    }
}
