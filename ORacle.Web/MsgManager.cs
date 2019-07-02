using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Security.Permissions;
using System.Threading;

namespace Oracle.Web
{
    internal static class MsgManager
    {
        private static readonly string DFEAULT_RESOURCE_NAME = string.Empty;
        private static string DEFAULT_MESSAGE_NUMBER = Convert.ToString(-10);
        private static Dictionary<string, ResourceSet> m_CultureToResourceStringMap = (Dictionary<string, ResourceSet>)null;
        private const string RESOURCE_NAME_PREFIX = "Oracle.Web.src.resources.Exception";
        private const string RESOURCE_NAME_PREFIX_WITH_DOT = "Oracle.Web.src.resources.Exception.";
        private const string RESOURCE_NAME_SUFFIX = "resources";
        private const string RESOURCE_NAME_SUFFIX_WITH_DOT = ".resources";
        private const char DOT = '.';

        static MsgManager()
        {
            MsgManager.m_CultureToResourceStringMap = ((IEnumerable<string>)Assembly.GetExecutingAssembly().GetManifestResourceNames()).Where<string>((Func<string, bool>)(resourceName =>
            {
                if (resourceName.StartsWith("Oracle.Web.src.resources.Exception."))
                    return resourceName.EndsWith(".resources");
                return false;
            })).ToDictionary<string, string, ResourceSet>((Func<string, string>)(resourceName => resourceName.Substring("Oracle.Web.src.resources.Exception".Length, resourceName.Length - ("Oracle.Web.src.resources.Exception".Length + "resources".Length)).Trim('.')), (Func<string, ResourceSet>)(resourceName => (ResourceSet)null));
        }

        internal static string GetString(string key, CultureInfo culture)
        {
            try
            {
                foreach (string cultureName in culture.NextMatchingNonInvariantCulture().Where<string>((Func<string, bool>)(cultureName => MsgManager.m_CultureToResourceStringMap.ContainsKey(cultureName))))
                {
                    string stringForCultureName = MsgManager.GetResourceStringForCultureName(key, cultureName);
                    if (stringForCultureName != null)
                        return stringForCultureName;
                }
                return MsgManager.GetResourceStringForCultureName(key, MsgManager.DFEAULT_RESOURCE_NAME);
            }
            catch
            {
                return (string)null;
            }
        }

        private static string GetResourceStringForCultureName(string key, string cultureName)
        {
            if (!MsgManager.m_CultureToResourceStringMap.ContainsKey(cultureName))
                return (string)null;
            if (MsgManager.m_CultureToResourceStringMap[cultureName] == null)
                MsgManager.ExtractEmbeddedResourceStringsForCultureName(cultureName);
            return MsgManager.m_CultureToResourceStringMap[cultureName]?.GetString(key);
        }

        [ReflectionPermission(SecurityAction.Assert, Unrestricted = true)]
        private static void ExtractEmbeddedResourceStringsForCultureName(string name)
        {
            if (!MsgManager.m_CultureToResourceStringMap.ContainsKey(name) || MsgManager.m_CultureToResourceStringMap[name] != null)
                return;
            Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Oracle.Web.src.resources.Exception." + (string.IsNullOrEmpty(name) || name.Trim().Length == 0 ? "resources" : name.Trim() + ".resources"));
            if (manifestResourceStream == null)
                return;
            MsgManager.m_CultureToResourceStringMap[name] = new ResourceSet(manifestResourceStream);
        }

        private static IEnumerable<string> NextMatchingNonInvariantCulture(
          this CultureInfo culture)
        {
            CultureInfo ci;
            for (ci = culture; ci != null && ci != CultureInfo.InvariantCulture; ci = ci.Parent)
                yield return ci.Name;
            ci = (CultureInfo)null;
        }

        internal static string GetMsg(int errorcode, params string[] args)
        {
            string str = (string)null;
            string format1 = MsgManager.GetString(Convert.ToString(errorcode), Thread.CurrentThread.CurrentCulture);
            if (format1 != null)
            {
                str = string.Format(format1, (object[])args);
            }
            else
            {
                string format2;
                if ((format2 = MsgManager.GetString(MsgManager.DEFAULT_MESSAGE_NUMBER, Thread.CurrentThread.CurrentCulture)) != null)
                    str = string.Format(format2, (object)errorcode);
            }
            return str;
        }
    }
}
