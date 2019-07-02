using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace Oracle.Web
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Configuration.Provider;
    using System.Data;
    using System.Data.Common;
    using System.IO;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.SessionState;

    namespace Oracle.Web.SessionState
    {
        public class OracleSessionStateStore : SessionStateStoreProviderBase
        {
            private static int ID_LENGTH = SessionIDManager.SessionIDMaxLength + 36;
            private static string ora_aspnet_Sessn_InsStateItem_CommandText = "INSERT into ora_aspnet_Sessions (SessionId, SessionItemShort, SessionItemLong, Timeout, Expires, Locked, LockDate, LockDateLocal, LockCookie) VALUES (:id, :itemShort, :itemLong, :timeout, SYS_EXTRACT_UTC(SYSTIMESTAMP)+(:timeout/1440), 0, SYS_EXTRACT_UTC(SYSTIMESTAMP), SYSDATE, 1)";
            private static string ora_aspnet_Sessn_UpdStateItem_CommandText = "UPDATE ora_aspnet_Sessions SET Expires = (SYS_EXTRACT_UTC(SYSTIMESTAMP)+(:timeout/1440)), SessionItemShort = :itemShort, SessionItemLong = :itemLong, Timeout = :timeout, Locked = 0 WHERE SessionId = :id AND LockCookie = :lockCookie";
            private string m_OracleConnectionString;
            private string pApplicationName;
            private int m_CommandTimeout;
            private string m_appID;
            private const int MAX_LENGTH_ITEM_SHORT = 2000;
            private const int APPID_MAX = 280;
            private const int APPID_LENGTH = 36;

            public int CommandTimeout
            {
                get
                {
                    return this.m_CommandTimeout;
                }
            }

            [AspNetHostingPermission(SecurityAction.Assert, Unrestricted = true)]
            public override void Initialize(string name, NameValueCollection config)
            {
                if (config == null)
                    throw new ArgumentNullException(nameof(config));
                if (string.IsNullOrEmpty(name))
                    name = "Oracle.Web.SessionState.OracleSessionStateStoreProvider";
                Util.HandleDescriptionAttribute(config, ErrRes.SESSIONSTATE_PROVIDER_DESCRIPTION);
                base.Initialize(name, config);
                this.m_CommandTimeout = Convert.ToInt32(Util.GetConfigValue(config["commandTimeout"], "30"));
                if (this.m_CommandTimeout < 0 || this.m_CommandTimeout > int.MaxValue)
                    throw new ProviderException(MsgManager.GetMsg(ErrRes.PROVIDER_INVALID_COMMANDTIMEOUT_VALUE));
                this.m_OracleConnectionString = Util.ReadConnectionString(config).Trim();
                config.Remove("connectionStringName");
                config.Remove("commandTimeout");
                Util.CheckForUnrecognizedAttribute(config);
                this.pApplicationName = HttpRuntime.AppDomainAppId;
                OracleConnectionHolder connectionHolder = (OracleConnectionHolder)null;
                OracleCommand oracleCommand = (OracleCommand)null;
                try
                {
                    connectionHolder = OracleConnectionHelper.GetConnection(this.m_OracleConnectionString);
                    oracleCommand = new OracleCommand("ora_aspnet_SessnApp_GetAppID", connectionHolder.Connection);
                    ((DbCommand)oracleCommand).CommandTimeout = this.m_CommandTimeout;
                    ((DbCommand)oracleCommand).CommandType = CommandType.StoredProcedure;
                    oracleCommand.Parameters.Add(new OracleParameter("OutResult", (OracleDbType)112, ParameterDirection.ReturnValue));
                    ((DbParameter)oracleCommand.Parameters[0]).DbType = DbType.Int32;
                    oracleCommand.Parameters.Add(new OracleParameter("appName_", (OracleDbType)119, 280));
                    ((DbParameter)oracleCommand.Parameters[1]).Value = (object)this.pApplicationName;
                    oracleCommand.Parameters.Add(new OracleParameter("appId_", (OracleDbType)120, ParameterDirection.Output));
                    ((DbParameter)oracleCommand.Parameters[2]).Size = 16;
                    ((DbParameter)oracleCommand.Parameters[2]).DbType = DbType.Binary;
                    ((DbCommand)oracleCommand).ExecuteNonQuery();
                    this.m_appID = new Guid((byte[])((DbParameter)oracleCommand.Parameters[2]).Value).ToString();
                }
                catch
                {
                    throw;
                }
                finally
                {
                    ((Component)oracleCommand)?.Dispose();
                    connectionHolder?.Close();
                }
            }

            public override SessionStateStoreData CreateNewStoreData(
              HttpContext context,
              int timeout)
            {
                return new SessionStateStoreData((ISessionStateItemCollection)new SessionStateItemCollection(), SessionStateUtility.GetSessionStaticObjects(context), timeout);
            }

            public override void CreateUninitializedItem(HttpContext context, string id, int timeout)
            {
                OracleConnectionHolder connectionHolder = (OracleConnectionHolder)null;
                OracleCommand oracleCommand = (OracleCommand)null;
                try
                {
                    connectionHolder = OracleConnectionHelper.GetConnection(this.m_OracleConnectionString);
                    byte[] buffer;
                    int length;
                    OracleSessionStateStore.SerializeSessionStateStoreData(this.CreateNewStoreData(context, timeout), 2000, out buffer, out length);
                    oracleCommand = new OracleCommand("ora_aspnet_Sessn_InsUninitItem", connectionHolder.Connection);
                    ((DbCommand)oracleCommand).CommandTimeout = this.m_CommandTimeout;
                    ((DbCommand)oracleCommand).CommandType = CommandType.StoredProcedure;
                    oracleCommand.Parameters.Add(new OracleParameter("OutResult", (OracleDbType)112, ParameterDirection.ReturnValue));
                    ((DbParameter)oracleCommand.Parameters[0]).DbType = DbType.Int32;
                    oracleCommand.Parameters.Add(new OracleParameter(nameof(id), (OracleDbType)119, OracleSessionStateStore.ID_LENGTH));
                    ((DbParameter)oracleCommand.Parameters[1]).Value = (object)(id + this.m_appID);
                    oracleCommand.Parameters.Add(new OracleParameter("itemShort", (OracleDbType)120, 2000));
                    ((DbParameter)oracleCommand.Parameters[2]).Size = length;
                    ((DbParameter)oracleCommand.Parameters[2]).Value = (object)buffer;
                    oracleCommand.Parameters.Add(new OracleParameter(nameof(timeout), (OracleDbType)112));
                    ((DbParameter)oracleCommand.Parameters[3]).Value = (object)timeout;
                    ((DbCommand)oracleCommand).ExecuteNonQuery();
                }
                catch
                {
                    throw;
                }
                finally
                {
                    ((Component)oracleCommand)?.Dispose();
                    connectionHolder?.Close();
                }
            }

            public override void Dispose()
            {
            }

            public override void EndRequest(HttpContext context)
            {
            }

            public override SessionStateStoreData GetItem(
              HttpContext context,
              string id,
              out bool locked,
              out TimeSpan lockAge,
              out object lockId,
              out SessionStateActions actions)
            {
                return this.GetSessionStoreItem(context, id, false, out locked, out lockAge, out lockId, out actions);
            }

            public override SessionStateStoreData GetItemExclusive(
              HttpContext context,
              string id,
              out bool locked,
              out TimeSpan lockAge,
              out object lockId,
              out SessionStateActions actions)
            {
                return this.GetSessionStoreItem(context, id, true, out locked, out lockAge, out lockId, out actions);
            }

            public override void InitializeRequest(HttpContext context)
            {
            }

            public override void ReleaseItemExclusive(HttpContext context, string id, object lockId)
            {
                int num = (int)lockId;
                OracleConnectionHolder connectionHolder = (OracleConnectionHolder)null;
                OracleCommand oracleCommand = (OracleCommand)null;
                try
                {
                    connectionHolder = OracleConnectionHelper.GetConnection(this.m_OracleConnectionString);
                    oracleCommand = new OracleCommand("ora_aspnet_Sessn_RelStateItmEx", connectionHolder.Connection);
                    ((DbCommand)oracleCommand).CommandTimeout = this.m_CommandTimeout;
                    ((DbCommand)oracleCommand).CommandType = CommandType.StoredProcedure;
                    oracleCommand.Parameters.Add(new OracleParameter("OutResult", (OracleDbType)112, ParameterDirection.ReturnValue));
                    ((DbParameter)oracleCommand.Parameters[0]).DbType = DbType.Int32;
                    oracleCommand.Parameters.Add(new OracleParameter(nameof(id), (OracleDbType)119, OracleSessionStateStore.ID_LENGTH));
                    ((DbParameter)oracleCommand.Parameters[1]).Value = (object)(id + this.m_appID);
                    oracleCommand.Parameters.Add(new OracleParameter("lockCookie", (OracleDbType)112));
                    ((DbParameter)oracleCommand.Parameters[2]).Value = (object)num;
                    ((DbCommand)oracleCommand).ExecuteNonQuery();
                }
                catch
                {
                    throw;
                }
                finally
                {
                    ((Component)oracleCommand)?.Dispose();
                    connectionHolder?.Close();
                }
            }

            public override void RemoveItem(
              HttpContext context,
              string id,
              object lockId,
              SessionStateStoreData item)
            {
                int num = (int)lockId;
                OracleCommand oracleCommand = (OracleCommand)null;
                try
                {
                    oracleCommand = new OracleCommand("ora_aspnet_Sessn_RmStateItem", OracleConnectionHelper.GetConnection(this.m_OracleConnectionString).Connection);
                    ((DbCommand)oracleCommand).CommandTimeout = this.m_CommandTimeout;
                    ((DbCommand)oracleCommand).CommandType = CommandType.StoredProcedure;
                    oracleCommand.Parameters.Add(new OracleParameter("OutResult", (OracleDbType)112, ParameterDirection.ReturnValue));
                    ((DbParameter)oracleCommand.Parameters[0]).DbType = DbType.Int32;
                    oracleCommand.Parameters.Add(new OracleParameter(nameof(id), (OracleDbType)119, OracleSessionStateStore.ID_LENGTH));
                    ((DbParameter)oracleCommand.Parameters[1]).Value = (object)(id + this.m_appID);
                    oracleCommand.Parameters.Add(new OracleParameter("lockCookie", (OracleDbType)112));
                    ((DbParameter)oracleCommand.Parameters[2]).Value = (object)num;
                    ((DbCommand)oracleCommand).ExecuteNonQuery();
                }
                catch
                {
                    throw;
                }
                finally
                {
                    ((Component)oracleCommand)?.Dispose();
                }
            }

            public override void ResetItemTimeout(HttpContext context, string id)
            {
                OracleConnectionHolder connectionHolder = (OracleConnectionHolder)null;
                OracleCommand oracleCommand = (OracleCommand)null;
                try
                {
                    connectionHolder = OracleConnectionHelper.GetConnection(this.m_OracleConnectionString);
                    oracleCommand = new OracleCommand("ora_aspnet_Sessn_ResetTimeout", connectionHolder.Connection);
                    ((DbCommand)oracleCommand).CommandTimeout = this.m_CommandTimeout;
                    ((DbCommand)oracleCommand).CommandType = CommandType.StoredProcedure;
                    oracleCommand.Parameters.Add(new OracleParameter("OutResult", (OracleDbType)112, ParameterDirection.ReturnValue));
                    ((DbParameter)oracleCommand.Parameters[0]).DbType = DbType.Int32;
                    oracleCommand.Parameters.Add(new OracleParameter(nameof(id), (OracleDbType)119, OracleSessionStateStore.ID_LENGTH));
                    ((DbParameter)oracleCommand.Parameters[1]).Value = (object)(id + this.m_appID);
                    ((DbCommand)oracleCommand).ExecuteNonQuery();
                }
                catch
                {
                    throw;
                }
                finally
                {
                    ((Component)oracleCommand)?.Dispose();
                    connectionHolder?.Close();
                }
            }

            public override void SetAndReleaseItemExclusive(
              HttpContext context,
              string id,
              SessionStateStoreData item,
              object lockId,
              bool newItem)
            {
                OracleConnectionHolder connectionHolder = (OracleConnectionHolder)null;
                OracleCommand oracleCommand = (OracleCommand)null;
                try
                {
                    connectionHolder = OracleConnectionHelper.GetConnection(this.m_OracleConnectionString);
                    byte[] buffer;
                    int length;
                    try
                    {
                        OracleSessionStateStore.SerializeSessionStateStoreData(item, 2000, out buffer, out length);
                    }
                    catch
                    {
                        if (!newItem)
                            this.ReleaseItemExclusive(context, id, lockId);
                        throw;
                    }
                    int num = lockId != null ? (int)lockId : 0;
                    if (!newItem)
                    {
                        if (length <= 2000)
                        {
                            oracleCommand = new OracleCommand(OracleSessionStateStore.ora_aspnet_Sessn_UpdStateItem_CommandText, connectionHolder.Connection);
                            ((DbCommand)oracleCommand).CommandTimeout = this.m_CommandTimeout;
                            ((DbCommand)oracleCommand).CommandType = CommandType.Text;
                            oracleCommand.BindByName=true;
                            oracleCommand.Parameters.Add(new OracleParameter("itemShort", (OracleDbType)120, 2000));
                            ((DbParameter)oracleCommand.Parameters[0]).Size = length;
                            ((DbParameter)oracleCommand.Parameters[0]).Value = (object)buffer;
                            oracleCommand.Parameters.Add(new OracleParameter("itemLong", (OracleDbType)110));
                            ((DbParameter)oracleCommand.Parameters[1]).Value = (object)DBNull.Value;
                            oracleCommand.Parameters.Add(new OracleParameter("timeout", (OracleDbType)112));
                            ((DbParameter)oracleCommand.Parameters[2]).Value = (object)item.Timeout;
                            oracleCommand.Parameters.Add(new OracleParameter(nameof(id), (OracleDbType)119, OracleSessionStateStore.ID_LENGTH));
                            ((DbParameter)oracleCommand.Parameters[3]).Value = (object)(id + this.m_appID);
                            oracleCommand.Parameters.Add(new OracleParameter("lockCookie", (OracleDbType)112));
                            ((DbParameter)oracleCommand.Parameters[4]).Value = (object)num;
                            ((DbCommand)oracleCommand).ExecuteNonQuery();
                        }
                        else
                        {
                            oracleCommand = new OracleCommand(OracleSessionStateStore.ora_aspnet_Sessn_UpdStateItem_CommandText, connectionHolder.Connection);
                            ((DbCommand)oracleCommand).CommandTimeout = this.m_CommandTimeout;
                            ((DbCommand)oracleCommand).CommandType = CommandType.Text;
                            oracleCommand.BindByName=true;
                            oracleCommand.Parameters.Add(new OracleParameter("itemShort", (OracleDbType)120, 2000));
                            ((DbParameter)oracleCommand.Parameters[0]).Value = (object)DBNull.Value;
                            if (length <= int.MaxValue)
                            {
                                oracleCommand.Parameters.Add(new OracleParameter("itemLong", (OracleDbType)110));
                                ((DbParameter)oracleCommand.Parameters[1]).Size = length;
                                ((DbParameter)oracleCommand.Parameters[1]).Value = (object)buffer;
                            }
                            else
                            {
                                oracleCommand.Parameters.Add(new OracleParameter("itemLong", (OracleDbType)102));
                                OracleBlob oracleBlob = new OracleBlob(oracleCommand.Connection, true);
                                ((Stream)oracleBlob).Write(buffer, 0, length);
                                ((DbParameter)oracleCommand.Parameters[1]).Value = (object)oracleBlob;
                            }
                            oracleCommand.Parameters.Add(new OracleParameter("timeout", (OracleDbType)112));
                            ((DbParameter)oracleCommand.Parameters[2]).Value = (object)item.Timeout;
                            oracleCommand.Parameters.Add(new OracleParameter(nameof(id), (OracleDbType)119, OracleSessionStateStore.ID_LENGTH));
                            ((DbParameter)oracleCommand.Parameters[3]).Value = (object)(id + this.m_appID);
                            oracleCommand.Parameters.Add(new OracleParameter("lockCookie", (OracleDbType)112));
                            ((DbParameter)oracleCommand.Parameters[4]).Value = (object)num;
                            ((DbCommand)oracleCommand).ExecuteNonQuery();
                        }
                    }
                    else if (length <= 2000)
                    {
                        oracleCommand = new OracleCommand(OracleSessionStateStore.ora_aspnet_Sessn_InsStateItem_CommandText, connectionHolder.Connection);
                        ((DbCommand)oracleCommand).CommandTimeout = this.m_CommandTimeout;
                        ((DbCommand)oracleCommand).CommandType = CommandType.Text;
                        oracleCommand.BindByName=true;
                        oracleCommand.Parameters.Add(new OracleParameter(nameof(id), (OracleDbType)119, OracleSessionStateStore.ID_LENGTH));
                        ((DbParameter)oracleCommand.Parameters[0]).Value = (object)(id + this.m_appID);
                        oracleCommand.Parameters.Add(new OracleParameter("itemShort", (OracleDbType)120, 2000));
                        ((DbParameter)oracleCommand.Parameters[1]).Size = length;
                        ((DbParameter)oracleCommand.Parameters[1]).Value = (object)buffer;
                        oracleCommand.Parameters.Add(new OracleParameter("itemLong", (OracleDbType)102));
                        ((DbParameter)oracleCommand.Parameters[2]).Value = (object)DBNull.Value;
                        oracleCommand.Parameters.Add(new OracleParameter("timeout", (OracleDbType)112));
                        ((DbParameter)oracleCommand.Parameters[3]).Value = (object)item.Timeout;
                        ((DbCommand)oracleCommand).ExecuteNonQuery();
                    }
                    else
                    {
                        oracleCommand = new OracleCommand(OracleSessionStateStore.ora_aspnet_Sessn_InsStateItem_CommandText, connectionHolder.Connection);
                        ((DbCommand)oracleCommand).CommandTimeout = this.m_CommandTimeout;
                        ((DbCommand)oracleCommand).CommandType = CommandType.Text;
                        oracleCommand.BindByName=true;
                        oracleCommand.Parameters.Add(new OracleParameter(nameof(id), (OracleDbType)119, OracleSessionStateStore.ID_LENGTH));
                        ((DbParameter)oracleCommand.Parameters[0]).Value = (object)(id + this.m_appID);
                        oracleCommand.Parameters.Add(new OracleParameter("itemShort", (OracleDbType)120, 2000));
                        ((DbParameter)oracleCommand.Parameters[1]).Value = (object)DBNull.Value;
                        if (length <= int.MaxValue)
                        {
                            oracleCommand.Parameters.Add(new OracleParameter("itemLong", (OracleDbType)110));
                            ((DbParameter)oracleCommand.Parameters[2]).Size = length;
                            ((DbParameter)oracleCommand.Parameters[2]).Value = (object)buffer;
                        }
                        else
                        {
                            oracleCommand.Parameters.Add(new OracleParameter("itemLong", (OracleDbType)102));
                            OracleBlob oracleBlob = new OracleBlob(oracleCommand.Connection, true);
                            ((Stream)oracleBlob).Write(buffer, 0, length);
                            ((DbParameter)oracleCommand.Parameters[2]).Value = (object)oracleBlob;
                        }
                        oracleCommand.Parameters.Add(new OracleParameter("timeout", (OracleDbType)112));
                        ((DbParameter)oracleCommand.Parameters[3]).Value = (object)item.Timeout;
                        ((DbCommand)oracleCommand).ExecuteNonQuery();
                    }
                }
                catch (OracleException ex)
                {
                    if (newItem && ex.Number == 1)
                        return;
                    throw;
                }
                catch
                {
                    throw;
                }
                finally
                {
                    ((Component)oracleCommand)?.Dispose();
                    connectionHolder?.Close();
                }
            }

            public override bool SetItemExpireCallback(SessionStateItemExpireCallback expireCallback)
            {
                return false;
            }

            private static void SerializeSessionStateStoreData(
              SessionStateStoreData item,
              int initialStreamSize,
              out byte[] buffer,
              out int length)
            {
                MemoryStream memoryStream = (MemoryStream)null;
                try
                {
                    memoryStream = new MemoryStream(initialStreamSize);
                    OracleSessionStateStore.Serialize(item, (Stream)memoryStream);
                    buffer = memoryStream.GetBuffer();
                    length = (int)memoryStream.Length;
                }
                finally
                {
                    memoryStream?.Dispose();
                }
            }

            private static void Serialize(SessionStateStoreData item, Stream stream)
            {
                bool flag1 = true;
                bool flag2 = true;
                BinaryWriter writer = new BinaryWriter(stream);
                writer.Write(item.Timeout);
                if (item.Items == null || item.Items.Count == 0)
                    flag1 = false;
                writer.Write(flag1);
                if (item.StaticObjects == null || item.StaticObjects.NeverAccessed)
                    flag2 = false;
                writer.Write(flag2);
                if (flag1)
                    ((SessionStateItemCollection)item.Items).Serialize(writer);
                if (flag2)
                    item.StaticObjects.Serialize(writer);
                writer.Write(byte.MaxValue);
            }

            private static SessionStateStoreData Deserialize(
              HttpContext context,
              Stream stream)
            {
                int timeout;
                SessionStateItemCollection stateItemCollection;
                HttpStaticObjectsCollection staticObjects;
                try
                {
                    BinaryReader reader = new BinaryReader(stream);
                    timeout = reader.ReadInt32();
                    bool flag = reader.ReadBoolean();
                    int num = reader.ReadBoolean() ? 1 : 0;
                    stateItemCollection = !flag ? new SessionStateItemCollection() : SessionStateItemCollection.Deserialize(reader);
                    staticObjects = num == 0 ? SessionStateUtility.GetSessionStaticObjects(context) : HttpStaticObjectsCollection.Deserialize(reader);
                    if (reader.ReadByte() != byte.MaxValue)
                        throw new ProviderException(MsgManager.GetMsg(ErrRes.INVALID_SESSION_STATE));
                }
                catch (EndOfStreamException ex)
                {
                    throw new ProviderException(MsgManager.GetMsg(ErrRes.INVALID_SESSION_STATE));
                }
                return new SessionStateStoreData((ISessionStateItemCollection)stateItemCollection, staticObjects, timeout);
            }

            private SessionStateStoreData GetSessionStoreItem(
              HttpContext context,
              string id,
              bool getExclusive,
              out bool locked,
              out TimeSpan lockAge,
              out object lockId,
              out SessionStateActions actionFlags)
            {
                MemoryStream memoryStream = (MemoryStream)null;
                OracleConnectionHolder connectionHolder = (OracleConnectionHolder)null;
                OracleCommand oracleCommand = (OracleCommand)null;
                locked = false;
                lockId = (object)null;
                lockAge = TimeSpan.Zero;
                actionFlags = SessionStateActions.None;
                try
                {
                    connectionHolder = OracleConnectionHelper.GetConnection(this.m_OracleConnectionString);
                    oracleCommand = !getExclusive ? new OracleCommand("ora_aspnet_Sessn_GetStateItem", connectionHolder.Connection) : new OracleCommand("ora_aspnet_Sessn_GetStateItmEx", connectionHolder.Connection);
                    ((DbCommand)oracleCommand).CommandTimeout = this.m_CommandTimeout;
                    ((DbCommand)oracleCommand).CommandType = CommandType.StoredProcedure;
                    ((DbParameter)oracleCommand.Parameters.Add("OutResult", (OracleDbType)112, ParameterDirection.ReturnValue)).DbType = DbType.Int32;
                    ((DbParameter)oracleCommand.Parameters.Add(nameof(id), (OracleDbType)119, OracleSessionStateStore.ID_LENGTH)).Value = (object)(id + this.m_appID);
                    OracleParameter oracleParameter = oracleCommand.Parameters.Add("itemShort", (OracleDbType)120, ParameterDirection.Output);
                    ((DbParameter)oracleParameter).Size = 2000;
                    ((DbParameter)oracleParameter).DbType = DbType.Binary;
                    ((DbParameter)oracleCommand.Parameters.Add(nameof(locked), (OracleDbType)112, ParameterDirection.Output)).DbType = DbType.Int32;
                    ((DbParameter)oracleCommand.Parameters.Add(nameof(lockAge), (OracleDbType)112, ParameterDirection.Output)).DbType = DbType.Int32;
                    ((DbParameter)oracleCommand.Parameters.Add("lockCookie", (OracleDbType)112, ParameterDirection.Output)).DbType = DbType.Int32;
                    ((DbParameter)oracleCommand.Parameters.Add(nameof(actionFlags), (OracleDbType)112, ParameterDirection.Output)).DbType = DbType.Int32;
                    oracleCommand.Parameters.Add("itemLong", (OracleDbType)102, ParameterDirection.Output);
                    ((DbCommand)oracleCommand).ExecuteNonQuery();
                    if (Convert.IsDBNull(((DbParameter)oracleCommand.Parameters[3]).Value))
                        return (SessionStateStoreData)null;
                    locked = (int)((DbParameter)oracleCommand.Parameters[3]).Value != 0;
                    lockId = (object)(int)((DbParameter)oracleCommand.Parameters[5]).Value;
                    actionFlags = (SessionStateActions)((DbParameter)oracleCommand.Parameters[6]).Value;
                    if (locked)
                    {
                        lockAge = new TimeSpan(0, 0, (int)((DbParameter)oracleCommand.Parameters[4]).Value);
                        if (lockAge > new TimeSpan(0, 0, 30758400))
                            lockAge = TimeSpan.Zero;
                        return (SessionStateStoreData)null;
                    }
                    byte[] buffer = !Convert.IsDBNull(((DbParameter)oracleCommand.Parameters[2]).Value) ? (byte[])((DbParameter)oracleCommand.Parameters[2]).Value : ((OracleBlob)((DbParameter)oracleCommand.Parameters[7]).Value).Value;
                    SessionStateStoreData sessionStateStoreData;
                    try
                    {
                        memoryStream = new MemoryStream(buffer);
                        sessionStateStoreData = OracleSessionStateStore.Deserialize(context, (Stream)memoryStream);
                    }
                    finally
                    {
                        memoryStream?.Close();
                    }
                    return sessionStateStoreData;
                }
                catch
                {
                    throw;
                }
                finally
                {
                    ((Component)oracleCommand)?.Dispose();
                    connectionHolder?.Close();
                }
            }
        }
    }

}
