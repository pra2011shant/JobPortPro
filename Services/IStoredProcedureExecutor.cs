using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace JobPortPro.Services
{
    /// <summary>
    /// Contract for executing SQL Server Stored Procedures with parameters and strongly typed entity mapping.
    /// </summary>
    public interface IStoredProcedureExecutor
    {
        Task<List<T>> ExecuteStoredProcedureListAsync<T>(string procedureName, IEnumerable<SqlParameter>? parameters, Func<DbDataReader, T> mapFunction);
        
        Task<T?> ExecuteStoredProcedureSingleAsync<T>(string procedureName, IEnumerable<SqlParameter>? parameters, Func<DbDataReader, T> mapFunction);
        
        Task<int> ExecuteStoredProcedureNonQueryAsync(string procedureName, IEnumerable<SqlParameter>? parameters);

        Task<(List<T> Results, int OutputTotalCount)> ExecuteStoredProcedurePagedListAsync<T>(
            string procedureName, 
            IEnumerable<SqlParameter> inputParameters, 
            string outputCountParamName, 
            Func<DbDataReader, T> mapFunction);
    }
}
