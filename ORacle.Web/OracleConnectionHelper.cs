using System.Web;

namespace Oracle.Web
{
    internal static class OracleConnectionHelper
    {
        private static object s_lock = new object();

        internal static OracleConnectionHolder GetConnection(
            string connectionString)
        {
            OracleConnectionHolder connectionHolder = new OracleConnectionHolder(connectionString);
            bool flag = true;
            try
            {
                try
                {
                    connectionHolder.Open((HttpContext)null, true);
                    flag = false;
                }
                finally
                {
                    if (flag)
                    {
                        connectionHolder.Close();
                        connectionHolder = (OracleConnectionHolder)null;
                    }
                }
            }
            catch
            {
                throw;
            }
            return connectionHolder;
        }
    }
}