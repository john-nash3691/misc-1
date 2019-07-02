using Oracle.ManagedDataAccess.Client;
using System;
using System.ComponentModel;
using System.Data.Common;
using System.Web;
using System.Web.Hosting;

namespace Oracle.Web
{
    internal sealed class OracleConnectionHolder
    {
        internal OracleConnection m_Connection;
        private bool m_Opened;

        internal OracleConnection Connection
        {
            get
            {
                return this.m_Connection;
            }
        }

        internal OracleConnectionHolder(string connectionString)
        {
            try
            {
                this.m_Connection = new OracleConnection(connectionString);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException(MsgManager.GetMsg(ErrRes.PROVIDER_ODP_CONNECTION_STRING_ERROR), nameof(connectionString), (Exception)ex);
            }
        }

        internal void Open(HttpContext context, bool revertImpersonate)
        {
            if (this.m_Opened)
                return;
            if (revertImpersonate)
            {
                using (HostingEnvironment.Impersonate())
                    ((DbConnection)this.Connection).Open();
            }
            else
                ((DbConnection)this.Connection).Open();
            this.m_Opened = true;
        }

        internal void Close()
        {
            if (!this.m_Opened)
                return;
            ((Component)this.Connection).Dispose();
            this.m_Opened = false;
        }
    }
}