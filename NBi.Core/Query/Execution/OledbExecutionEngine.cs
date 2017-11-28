﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;

namespace NBi.Core.Query.Execution
{
    /// <summary>
    /// Engine wrapping the System.Data.OleDb namespace for execution of NBi tests
    /// <remarks>Instances of this class are built by the means of the <see>QueryEngineFactory</see></remarks>
    /// </summary>
    internal class OleDbExecutionEngine : DbCommandExecutionEngine
    {
        protected internal OleDbExecutionEngine(OleDbCommand command)
            : base(command)
        { }
        

        protected override void OpenConnection(IDbConnection connection)
        {
            var connectionString = command.Connection.ConnectionString;
            try
            { connection.ConnectionString = connectionString; }
            catch (ArgumentException ex)
            { throw new ConnectionException(ex, connectionString); }

            try
            { connection.Open(); }
            catch (OleDbException ex)
            { throw new ConnectionException(ex, connectionString); }

            command.Connection = connection;
        }

        protected override void HandleException(Exception ex, IDbCommand command)
        {
            if (ex is OleDbException && ex.Message == "Query timeout expired")
                throw new CommandTimeoutException(ex, command);
            throw ex;
        }

        protected override IDbConnection NewConnection() => new OleDbConnection();
        protected override IDataAdapter NewDataAdapter(IDbCommand command) => new OleDbDataAdapter((OleDbCommand)command);
    }
}