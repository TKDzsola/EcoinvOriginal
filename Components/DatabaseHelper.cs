using System;
using System.Data;

namespace Ecoinv.Common
{
    public static class DatabaseHelper
    {
        public static FBConnectX CreateConnection()
        {
            var conn = new FBConnectX();
            conn.GetConnectionX();

            if (conn.GetConStateX() != ConnectionState.Open)
            {
                conn.FBConnOpenX();
            }

            return conn;
        }

        /// <summary>
        /// Megnyit egy kapcsolatot, végrehajtja a műveletet, majd lezárja.
        /// </summary>
        public static void Execute(Action<FBConnectX> action, string errorContext = "")
        {
            using (FBConnectX conn = CreateConnection())
            {
                try
                {
                    action(conn);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, errorContext);
                    throw;
                }
            }
        }

        /// <summary>
        /// Megnyit egy kapcsolatot, végrehajtja a műveletet és visszaad egy értéket.
        /// </summary>
        public static T Execute<T>(Func<FBConnectX, T> action, string errorContext = "")
        {
            using (FBConnectX conn = CreateConnection())
            {
                try
                {
                    return action(conn);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, errorContext);
                    throw;
                }
            }
        }
    }
}