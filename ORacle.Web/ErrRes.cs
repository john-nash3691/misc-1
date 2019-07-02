namespace Oracle.Web
{
    internal class ErrRes
    {
        internal static string MEMBERSHIP_PROVIDER_DESCRIPTION = "Oracle Membership Provider for ASP.NET";
        internal static string ROLE_PROVIDER_DESCRIPTION = "Oracle Role Provider for ASP.NET";
        internal static string SESSIONSTATE_PROVIDER_DESCRIPTION = "Oracle SessionState Provider for ASP.NET";
        internal static string PROFILE_PROVIDER_DESCRIPTION = "Oracle Profile Provider for ASP.NET";
        internal static string PERSONALIZATION_PROVIDER_DESCRIPTION = "Oracle Personalization Provider for ASP.NET";
        internal static string SITEMAP_PROVIDER_DESCRIPTION = "Oracle Site Map Provider for ASP.NET";
        internal static string WEBEVENTS_PROVIDER_DESCRIPTION = "Oracle Web Events Provider for ASP.NET";
        internal static int MEMBERSHIP_CUSTOM_PWD_VALIDATION_FAILED = -1000;
        internal static int MEMBERSHIP_INVALID_USER_NAME = -1001;
        internal static int MEMBERSHIP_INVALID_PASSWORD = -1002;
        internal static int MEMBERSHIP_INVALID_QUESTION = -1003;
        internal static int MEMBERSHIP_INVALID_ANSWER = -1004;
        internal static int MEMBERSHIP_INVALID_EMAIL = -1005;
        internal static int MEMBERSHIP_DUPLICATED_USER_NAME = -1006;
        internal static int MEMBERSHIP_DUPLICATED_EMAIL = -1007;
        internal static int MEMBERSHIP_USER_REJECTED = -1008;
        internal static int MEMBERSHIP_INVALID_PROVIDER_USER_KEY = -1009;
        internal static int MEMBERSHIP_DUPLICATED_PROVIDER_USERKEY = -1010;
        internal static int MEMBERSHIP_PROVIDER_ERROR = -1011;
        internal static int MEMBERSHIP_USER_NOT_FOUND = -1012;
        internal static int MEMBERSHIP_WRONG_PASSWORD = -1013;
        internal static int MEMBERSHIP_WRONG_ANSWER = -1014;
        internal static int MEMBERSHIP_ACCOUNT_LOCKED_OUT = -1099;
        internal static int MEMBERSHIP_PWD_RETRIEVAL_NOT_SUPPORTED = -1016;
        internal static int MEMBERSHIP_MIN_REQUIRED_NON_ALPHANUMERIC_CHARS_INCORRECT = -1017;
        internal static int MEMBERSHIP_MORE_THAN_ONE_USER_WITH_EMAIL = -1018;
        internal static int MEMBERSHIP_PASSWORD_TOO_LONG = -1020;
        internal static int MEMBERSHIP_PASSWORD_TOO_SHORT = -1021;
        internal static int MEMBERSHIP_PASSWORD_DOES_NOT_MATCH_REGULAR_EXPRESSION = -1023;
        internal static int MEMBERSHIP_PASSWORD_CANNOT_BE_RESET = -1024;
        internal static int PROVIDER_ERROR = -2000;
        internal static int PROVIDER_NOT_FOUND = -2001;
        internal static int PROVIDER_SCHEMA_VERSION_NOT_MATCH = -2002;
        internal static int PROVIDER_APPLICATION_NAME_TOO_LONG = -2003;
        internal static int PROVIDER_BAD_PASSWORD_FORMAT = -2004;
        internal static int PROVIDER_CANNOT_CREATE_FILE_IN_THIS_TRUST_LEVEL = -2005;
        internal static int PROVIDER_CANNOT_DECODE_HASHED_PASSWORD = -2006;
        internal static int PROVIDER_CANNOT_RETRIEVE_HASHED_PASSWORD = -2007;
        internal static int PROVIDER_MISSING_ATTRIBUTE = -2008;
        internal static int PROVIDER_MUST_IMPLEMENT_THE_INTERFACE = -2009;
        internal static int PROVIDER_MUST_IMPLEMENT_TYPE = -2010;
        internal static int PROVIDER_NO_TYPE_NAME = -2011;
        internal static int PROVIDER_UNKNOWN_FAILURE = -2017;
        internal static int PROVIDER_UNRECOGNIZED_ATTRIBUTE = -2018;
        internal static int PROVIDER_USER_NOT_FOUND = -2019;
        internal static int PROVIDER_CONNECTION_NAME_NOT_SPECIFIED = -2020;
        internal static int PROVIDER_CONNECTION_STRING_NOT_FOUND = -2021;
        internal static int PROVIDER_CANNOT_USE_ENCRYPTED_PWD_WITH_AUTOGEN_KEYS = -2022;
        internal static int PROVIDER_DEFAULT_PROVIDER_NOT_FOUND = -2023;
        internal static int PROVIDER_DEFAULT_PROVIDER_NOT_SPECIFIED = -2024;
        internal static int PROVIDER_PAGEINDEX_CANNOT_BE_LESS_THAN_ZERO = -2025;
        internal static int PROVIDER_PAGESIZE_CANNOT_BE_LESS_THAN_ONE = -2026;
        internal static int PROVIDER_PAGE_UPPER_BOUND_EXCEED_INT_MAX = -2027;
        internal static int PROVIDER_FEATURE_NOT_SUPPORTED_AT_THIS_LEVEL = -2028;
        internal static int PROVIDER_PARAMETER_ARRAY_EMPTY = -2029;
        internal static int PROVIDER_PARAMETER_CANNOT_BE_EMPTY = -2030;
        internal static int PROVIDER_PARAMETER_CANNOT_CONTAIN_COMMA = -2031;
        internal static int PROVIDER_PARAMETER_DUPLICATE_ARRAY_ELEMENT = -2032;
        internal static int PROVIDER_PARAMETER_TOO_LONG = -2033;
        internal static int PROVIDER_PASSWORD_DOES_NOT_MATCH_REGULAR_EXPRESSION = -2034;
        internal static int PROVIDER_PASSWORD_NEED_MORE_NON_ALPHA_NUMERIC_CHARS = -2035;
        internal static int PROVIDER_PASSWORD_TOO_SHORT = -2036;
        internal static int PROVIDER_PARAMETER_COLLECTION_EMPTY = -2037;
        internal static int PROVIDER_INITIALIZATION_ERROR = -2038;
        internal static int PROVIDER_INVALID_COMMANDTIMEOUT_VALUE = -2039;
        internal static int PROVIDER_ODP_CONNECTION_STRING_ERROR = -2040;
        internal static int PROVIDER_PARAMETER_ARRAY_CONTAIN_NULLOREMPTY_ELEMENT = -2041;
        internal static int PROVIDER_PARAMETER_ARRAY_ELEMENT_EXCEED_SIZE = -2042;
        internal static int PROVIDER_ROLE_ALREADY_EXISTS = -3000;
        internal static int PROVIDER_ROLE_NOT_FOUND = -3001;
        internal static int PROVIDER_THIS_USER_ALREADY_IN_ROLE = -3002;
        internal static int PROVIDER_THIS_USER_ALREADY_NOT_IN_ROLE = -3003;
        internal static int PROVIDER_THIS_USER_NOT_FOUND = -3004;
        internal static int ROLE_IS_NOT_EMPTY = -3005;
        internal static int INVALID_SESSION_STATE = -4000;
        internal static int MULTIPLE_ROOT_NODES_FOUND = -5000;
        internal static int PARENT_NODE_NOT_FOUND = -5001;
        internal static int ROOT_NODE_NOT_FOUND = -5002;

        private ErrRes()
        {
        }
    }
}
