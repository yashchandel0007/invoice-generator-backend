﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Couchbase.Setup
{
    public static class Constants{
        public static class Couchbase
        {
            public const string url = "http://localhost:8091/";
            public const string url2 = "couchbase://localhost";
            public const string username = "Administrator";
            public const string password = "Administrator";
            public const string bucketType = "couchbase";
            public const int ramQuota = 100;
            public const int replicaNumber = 0;
            public const string durability = "none";
        }

    }
    

}
