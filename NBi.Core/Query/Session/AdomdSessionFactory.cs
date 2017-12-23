﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBi.Core.Query.Session
{
    class AdomdSessionFactory : ISessionFactory
    {
        public bool CanHandle(string connectionString)
        {
            return !string.IsNullOrEmpty(ParseConnectionString(connectionString));
        }

        public ISession Instantiate(string connectionString)
        {
            if (!CanHandle(connectionString))
                throw new ArgumentException();

            return new AdomdSession(connectionString);
        }

        private string ParseConnectionString(string connectionString)
        {
            var providerName = ExtractProviderName(connectionString);
            if (string.IsNullOrEmpty(providerName))
                return string.Empty;

            providerName = TranslateProviderName(providerName);
            if (string.IsNullOrEmpty(providerName))
                return string.Empty;

            return providerName;
        }

        private string ExtractProviderName(string connectionString)
        {
            try
            {
                var csb = new DbConnectionStringBuilder() { ConnectionString = connectionString };

                if (csb.ContainsKey("Provider"))
                    return (csb["Provider"].ToString());
            }
            catch (Exception) { }

            return string.Empty;
        }

        private string TranslateProviderName(string providerName)
        {
            if (providerName.ToLowerInvariant().StartsWith("msolap")) return "Microsoft.AnalysisServices.AdomdClient";
            
            return null;
        }
    }
}
