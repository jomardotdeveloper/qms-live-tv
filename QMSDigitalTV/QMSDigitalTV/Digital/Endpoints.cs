using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMSDigitalTV.Digital
{
    static class Endpoints
    {
        public static string BaseUrl = "http://127.0.0.1:8000/api/";

        public static string BranchUrl = "qms-rest-api/branch/product_key";

        // REQUIRED PARAMETER(BRANCH ID) GET
        public static string TokenUrl = "/api/transactions/get_live/";
        public static string WindowsUrl = "qms-rest-api/transaction/get_all_windows";
        public static string BranchIDUrl = "server/get_branch_id/";
    }
}
