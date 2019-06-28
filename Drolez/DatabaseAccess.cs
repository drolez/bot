namespace Drolez
{
    using System;
    using System.Data.SqlClient;

    /// <summary>
    /// Database access class
    /// </summary>
    public static class DatabaseAccess
    {
        /// <summary>
        /// Connection event handler
        /// </summary>
        /// <param name="server">Web socket server</param>
        /// <param name="exception">Raised exception</param>
        public delegate void OnExceptionHandler(WebSocketsServer server, Exception exception);

        /// <summary>
        /// On Exception event
        /// </summary>
        public static event Extensions.OnExceptionHandler OnException;

        /// <summary>
        /// Gets instance of the SQL database connection
        /// </summary>
        public static SqlConnection Database { get; private set; }

        /// <summary>
        /// Close database connection
        /// </summary>
        /// <returns>True on success</returns>
        public static bool Close()
        {
            try
            {
                DatabaseAccess.Database.Close();
            }
            catch (Exception ex)
            {
                DatabaseAccess.OnException?.Invoke(DatabaseAccess.Database, ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Connect to SQL database
        /// </summary>
        /// <param name="settings">Connection settings</param>
        /// <returns>True on success</returns>
        public static bool Connect(DatabaseConnectionSettings settings)
        {
            try
            {
                string connection =
                    "Server=" + settings.Server + "; " +
                    "Database=" + settings.DatabaseName + "; " +
                    "User ID=" + settings.User + "; " +
                    "Password=" + settings.Password + ";";

                DatabaseAccess.Database = new SqlConnection(connection);
                DatabaseAccess.Database.Open();
            }
            catch (Exception ex)
            {
                DatabaseAccess.OnException?.Invoke(DatabaseAccess.Database, ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Read from database
        /// </summary>
        /// <param name="sql">SQL command</param>
        /// <returns>Data or NULL on error</returns>
        public static SqlDataReader Read(string sql)
        {
            try
            {
                SqlCommand comand = new SqlCommand(sql, DatabaseAccess.Database);
                return comand.ExecuteReader();
            }
            catch (Exception ex)
            {
                DatabaseAccess.OnException?.Invoke(DatabaseAccess.Database, ex);
            }

            return null;
        }

        /// <summary>
        /// Holds connection settings
        /// </summary>
        public class DatabaseConnectionSettings
        {
            /// <summary>
            /// Gets or sets name of the database
            /// </summary>
            public string DatabaseName { get; set; }

            /// <summary>
            /// Gets or sets password
            /// </summary>
            public string Password { get; set; }

            /// <summary>
            /// Gets or sets server address
            /// </summary>
            public string Server { get; set; }

            /// <summary>
            /// Gets or sets user name
            /// </summary>
            public string User { get; set; }
        }
    }
}