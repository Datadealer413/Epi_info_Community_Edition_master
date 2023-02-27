﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.ComponentModel;
using System.Data.Common;
using System.Drawing.Design;
using System.Reflection;

namespace Epi.Data.EpiWeb
{
    public class EpiWebConnectionStringBuilder : DbConnectionStringBuilder
    {

        private string connString;
        private string certFile;

        public EpiWebConnectionStringBuilder() { }
        public EpiWebConnectionStringBuilder(string connString) { this.connString = connString; }

        public string Verbosity { get; set; }
        public string PoolIdleTimeout { get; set; }
        public string PoolMaxSize { get; set; }
        public string PoolMinSize { get; set; }
        public string PoolWaitTime { get; set; }
        public string Port { get; set; }
        public string PseudoColumns { get; set; }
        public bool QueryPassthrough { get; set; }
        public bool Readonly { get; set; }
        public string ReadPreference { get; set; }
        public string ReplicaSet { get; set; }
        public string RowScanDepth { get; set; }
        public string RTK { get; set; }
        public string Views { get; set; }
        public string Server
        {
            get
            {
                return certFile;
            }
            set
            {
                connString = "";
                certFile = value;
            }
        }
        public string Password { get; set; }
        public string SSLClientCertPassword { get; set; }
        public string SSLClientCertSubject { get; set; }
        public string SSLClientCertType { get; set; }
        public string SSLServerCert { get; set; }
        public bool SupportEnhancedSQL { get; set; }
        public string Tables { get; set; }
        public string Timeout { get; set; }
        public string TypeDetectionScheme { get; set; }
        public bool UseConnectionPooling { get; set; }
        public string User { get; set; }
        public bool UseSSL { get; set; }
        public bool SlaveOK { get; set; }
        public string SSLClientCert { get; set; }
        public string Other { get; set; }
        public bool NoCursorTimeout { get; set; }
        public string ConnectionString { get; set; }
        public string AuthDatabase { get; set; }
        public string AuthMechanism { get; set; }
        public bool AutoCache { get; set; }
        public string CacheConnection { get; set; }
        public string CacheLocation { get; set; }
        public bool CacheMetadata { get; set; }
        public string CacheProvider { get; set; }
        public bool CacheQueryResult { get; set; }
        public bool Offline { get; set; }
        public string Database { get; set; }
        public string ConnectionLifeTime { get; set; }
        public string FirewallPort { get; set; }
        public string MaxRows { get; set; }
        public string MaxLogFileSize { get; set; }
        public string FirewallPassword { get; set; }
        public string Location { get; set; }
        public string GenerateSchemaFiles { get; set; }
        public string Logfile { get; set; }
        public string FlattenArrays { get; set; }
        public string FirewallUser { get; set; }
        public string FirewallType { get; set; }
        public string FirewallServer { get; set; }
        public bool FlattenObjects { get; set; }
        public override ICollection Keys { get; }
        public override void Clear() { }
        public override bool ContainsKey(string keyword) { return false; }
        public override bool Remove(string key) { return false; }
        public override bool TryGetValue(string key, out object value)
        {
            value = null;
            return false;
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(connString))
                connString = "epiweb://" + certFile;
            return connString;
        }
    }
}