using System;

namespace Invoice.Generator.Api
{
    public static class Constants
    {
        public static class Couchbase
        {
            public const string url = "http://localhost:8091/";
            public const string url2 = "couchbase://localhost";
            public const string username = "Administrator";
            public const string password = "Administrator";
            public const string bucketType = "couchbase";
            public const int ramQuota = 100;
        }

        public static class DocumentType
        {
            public const string User = "_user";
        }
    }
}
