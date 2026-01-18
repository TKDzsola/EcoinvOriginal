using System;
using System.Data;
using FirebirdSql.Data.FirebirdClient;

namespace Ecoinv.Common
{
    public class DBFunc
    {
        public DBFunc() { }

        public static int Get_Generator(string sql, FBConnectX conn)
        {
            // Ha kaptunk kapcsolatot, azt használjuk, ha nem, csinálunk egyet a művelet idejére
            if (conn != null)
            {
                return ExecuteGenerator(sql, conn);
            }
            else
            {
                using (FBConnectX localConn = new FBConnectX())
                {
                    localConn.GetConnectionX();
                    localConn.FBConnOpenX();
                    return ExecuteGenerator(sql, localConn);
                }
            }
        }

        private static int ExecuteGenerator(string sql, FBConnectX conn)
        {
            if (conn.GetConStateX() != ConnectionState.Open) conn.FBConnOpenX();

            using (var fbtr = conn.FBConnBeginTransactionX())
            using (var cmd = new FbCommand(sql, conn.FBCConnectionX, fbtr))
            using (var fbdr = cmd.ExecuteReader())
            {
                int genvalue = -1;
                if (fbdr.Read())
                {
                    genvalue = Convert.ToInt32(fbdr[0]);
                }
                fbtr.Commit();
                return genvalue;
            }
        }
    }
}