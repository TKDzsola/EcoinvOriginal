using System.Data;

namespace Ecoinv.Common
{
    public static class DatabaseHelper
    {
        public static FBConnectX CreateConnection()
        {
            var conn = new FBConnectX();

            // ⚠️ A projektben EZ hozza létre a belső FbConnection-t
            conn.GetConnectionX();

            // ⚠️ Megnyitás a wrapperen keresztül
            if (conn.GetConStateX() != ConnectionState.Open)
            {
                conn.FBConnOpenX();
            }

            return conn;
        }
    }
}
