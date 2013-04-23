﻿using System;
using System.Data;
using Microsoft.AnalysisServices.AdomdClient;

namespace NBi.Core.Query
{
    /// <summary>
    /// Engine wrapping the Microsoft.AnalysisServices.AdomdClient namespace for execution of NBi tests
    /// <remarks>Instances of this class are built by the means of the <see>QueryEngineFactory</see></remarks>
    /// </summary>
    internal class QueryAdomdEngine: IQueryEnginable, IQueryExecutor, IQueryParser, IQueryPerformance
    {
        /// <summary>
        /// The query to execute
        /// </summary>
        protected readonly AdomdCommand command;


        protected internal QueryAdomdEngine(AdomdCommand command)
        {
            this.command = command;
        }

        /// <summary>
        /// Method exposed by the interface IQueryPerformance to execute a test of performance
        /// </summary>
        /// <returns></returns>
        public PerformanceResult CheckPerformance()
        {
            return CheckPerformance(0);
        }

        public PerformanceResult CheckPerformance(int timeout)
        {
            bool isTimeout = false;
            DateTime tsStart, tsStop = DateTime.Now;

            if (command.Connection.State == ConnectionState.Closed)
                command.Connection.Open();

            tsStart = DateTime.Now;
            try
            {
                command.ExecuteNonQuery();
                tsStop = DateTime.Now;
            }
            catch (AdomdException e)
            {
                if (!e.Message.StartsWith("Timeout expired."))
                    throw;
                isTimeout = true;
            }

            if (command.Connection.State == ConnectionState.Open)
                command.Connection.Close();

            if (isTimeout)
                return PerformanceResult.Timeout(timeout);
            else
                return new PerformanceResult(tsStop.Subtract(tsStart));
        }

        /// <summary>
        /// Method exposed by the interface IQueryExecutor to execute a test of execution and get the result of the query executed
        /// </summary>
        /// <returns>The result of  execution of the query</returns>
        public virtual DataSet Execute()
        {
            int i;
            return Execute(out i);
        }

        /// <summary>
        /// Method exposed by the interface IQueryExecutor to execute a test of execution and get the result of the query executed and also the time needed to retrieve this result
        /// </summary>
        /// <returns>The result of  execution of the query</returns>
        public virtual DataSet Execute(out int elapsedSec)
        {
            // Open the connection
            using (var connection = new AdomdConnection())
            {
                var connectionString = command.Connection.ConnectionString;
                try
                    { connection.ConnectionString = connectionString; }
                catch (ArgumentException ex)
                { throw new ConnectionException(ex, connectionString); }
                //TODO
                //try
                //    {connection.Open();}
                //catch (AdomdException ex)
                //    {throw new ConnectionException(ex);}

                // capture time before execution
                long ticksBefore = DateTime.Now.Ticks;
                var adapter = new AdomdDataAdapter(command.CommandText, connection);
                var ds = new DataSet();
                
                adapter.SelectCommand.CommandTimeout = 0;
                try
                {
                    adapter.Fill(ds);
                }
                catch (AdomdConnectionException ex)
                {
                    throw new ConnectionException(ex, connectionString);
                }

                // capture time after execution
                long ticksAfter = DateTime.Now.Ticks;

                // setting query runtime
                elapsedSec = Convert.ToInt32((ticksAfter - ticksBefore) / 1000 / 1000);

                return ds;
            }
        }

        /// <summary>
        /// Method exposed by the interface IQueryParser to execute a test of syntax on a MDX or SQL statement
        /// </summary>
        /// <remarks>This method makes usage the scommand behaviour named 'SchemaOnly' to not effectively execute the query but check the validity of this query</remarks>
        /// <returns>The result of parsing test</returns>
        public virtual ParserResult Parse()
        {
            ParserResult res = null;

            using (var connection = new AdomdConnection())
            {
                var connectionString = command.Connection.ConnectionString;
                try
                {
                    connection.ConnectionString = connectionString;
                    connection.Open();
                }
                catch (ArgumentException ex)
                { throw new ConnectionException(ex, connectionString); }
                
                using (AdomdCommand cmdIn = new AdomdCommand(command.CommandText, connection))
                {
                    try
                    {
                        cmdIn.ExecuteReader(CommandBehavior.SchemaOnly);
                        res = ParserResult.NoParsingError();
                    }
                    catch (AdomdException ex)
                    {
                        res = new ParserResult(ex.Message.Split(new string[] { "\r\n" }, System.StringSplitOptions.RemoveEmptyEntries));
                    }

                }

                if (connection.State != System.Data.ConnectionState.Closed)
                    connection.Close();
            }

            return res;
        }

        /// <summary>
        /// Method exposed by the interface IQueryPerformance of engine but useless in the case of an SSAS cube
        /// </summary>
        public void CleanCache()
        {
            throw new NotImplementedException("Hé man what's the goal to clean the cache for an MDX query?");
        }
    }
}
