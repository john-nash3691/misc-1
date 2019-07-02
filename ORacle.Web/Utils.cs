using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Configuration.Provider;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Web.Configuration;
using System.Web.Hosting;

namespace Oracle.Web
{
    internal static class Util
    {
        internal static string GetConfigValue(string configValue, string defaultValue)
        {
            if (string.IsNullOrEmpty(configValue))
                return defaultValue;
            return configValue;
        }

        internal static bool IsParameterOK(
          ref string param,
          bool checkForNull,
          bool checkIfEmpty,
          bool checkForCommas,
          int maxSize)
        {
            if (param == null)
                return !checkForNull;
            param = param.Trim();
            return (!checkIfEmpty || param.Length >= 1) && (maxSize <= 0 || param.Length <= maxSize) && (!checkForCommas || !param.Contains(","));
        }

        internal static void CheckParameter(
          ref string param,
          bool checkForNull,
          bool checkIfEmpty,
          bool checkForCommas,
          int maxSize,
          string paramName)
        {
            if (param == null)
            {
                if (checkForNull)
                    throw new ArgumentNullException(paramName);
            }
            else
            {
                if (checkIfEmpty && param.Length < 1)
                    throw new ArgumentException(MsgManager.GetMsg(ErrRes.PROVIDER_PARAMETER_CANNOT_BE_EMPTY, paramName));
                if (maxSize > 0 && param.Length > maxSize)
                    throw new ArgumentException(MsgManager.GetMsg(ErrRes.PROVIDER_PARAMETER_TOO_LONG, paramName, maxSize.ToString((IFormatProvider)CultureInfo.InvariantCulture)));
                if (checkForCommas && param.Contains(","))
                    throw new ArgumentException(MsgManager.GetMsg(ErrRes.PROVIDER_PARAMETER_CANNOT_CONTAIN_COMMA, paramName));
            }
        }

        internal static void CheckArrayParameter(
          ref string[] param,
          bool checkForNull,
          bool checkIfEmpty,
          bool checkForCommas,
          int maxSize,
          string paramName)
        {
            if (param == null)
                throw new ArgumentNullException(paramName);
            if (param.Length < 1)
                throw new ArgumentException(MsgManager.GetMsg(ErrRes.PROVIDER_PARAMETER_ARRAY_EMPTY, paramName));
            Hashtable hashtable = new Hashtable(param.Length);
            for (int index = param.Length - 1; index >= 0; --index)
            {
                Util.CheckParameter(ref param[index], checkForNull, checkIfEmpty, checkForCommas, maxSize, paramName + "[ " + index.ToString((IFormatProvider)CultureInfo.InvariantCulture) + " ]");
                if (hashtable.Contains((object)param[index]))
                    throw new ArgumentException(MsgManager.GetMsg(ErrRes.PROVIDER_PARAMETER_DUPLICATE_ARRAY_ELEMENT, paramName));
                hashtable.Add((object)param[index], (object)param[index]);
            }
        }

        internal static string GetDefaultAppName()
        {
            try
            {
                string str = HostingEnvironment.ApplicationVirtualPath;
                if (string.IsNullOrEmpty(str))
                {
                    str = Process.GetCurrentProcess().MainModule.ModuleName;
                    int startIndex = str.IndexOf('.');
                    if (startIndex != -1)
                        str = str.Remove(startIndex);
                }
                if (string.IsNullOrEmpty(str))
                    return "/";
                return str;
            }
            catch
            {
                return "/";
            }
        }

        internal static string ReadConnectionString(NameValueCollection config)
        {
            string index = config["connectionStringName"];
            if (string.IsNullOrEmpty(index))
                throw new ProviderException(MsgManager.GetMsg(ErrRes.PROVIDER_CONNECTION_NAME_NOT_SPECIFIED));
            ConnectionStringSettings connectionString = (WebConfigurationManager.GetSection("connectionStrings") as ConnectionStringsSection).ConnectionStrings[index];
            string str = string.Empty;
            if (connectionString != null)
                str = connectionString.ConnectionString;
            if (string.IsNullOrEmpty(str))
                throw new ProviderException(MsgManager.GetMsg(ErrRes.PROVIDER_CONNECTION_STRING_NOT_FOUND, index));
            return str;
        }

        internal static void HandleDescriptionAttribute(
          NameValueCollection config,
          string defDescription)
        {
            if (!string.IsNullOrEmpty(config["description"]))
                return;
            config.Remove("description");
            config.Add("description", defDescription);
        }

        internal static string ReadAndVerifyApplicationName(NameValueCollection config)
        {
            string defaultAppName = config["applicationName"];
            if (string.IsNullOrEmpty(defaultAppName))
                defaultAppName = Util.GetDefaultAppName();
            if (defaultAppName.Length > 256)
                throw new ProviderException(MsgManager.GetMsg(ErrRes.PROVIDER_APPLICATION_NAME_TOO_LONG));
            return defaultAppName;
        }

        internal static void CheckForUnrecognizedAttribute(NameValueCollection config)
        {
            if (config.Count <= 0)
                return;
            string key = config.GetKey(0);
            if (!string.IsNullOrEmpty(key))
                throw new ProviderException(MsgManager.GetMsg(ErrRes.PROVIDER_UNRECOGNIZED_ATTRIBUTE, key));
        }

        internal static void CleanUpConnectionResources(ref OracleConnection con, ref OracleCommand cmd)
        {
            if (cmd != null)
            {
                foreach (OracleParameter parameter in (DbParameterCollection)cmd.Parameters)
                    parameter?.Dispose();
                ((Component)cmd).Dispose();
                cmd = (OracleCommand)null;
            }
            if (con == null || ((DbConnection)con).State != ConnectionState.Open)
                return;
            ((DbConnection)con).Close();
            ((Component)con).Dispose();
            con = (OracleConnection)null;
        }

        internal static void CleanUpConnectionResources(
          ref OracleCommand cmd,
          ref OracleConnectionHolder holder)
        {
            if (cmd != null)
            {
                foreach (OracleParameter parameter in (DbParameterCollection)cmd.Parameters)
                    parameter?.Dispose();
                ((Component)cmd).Dispose();
                cmd = (OracleCommand)null;
            }
            if (holder == null)
                return;
            holder.Close();
            holder = (OracleConnectionHolder)null;
        }
    }
}
