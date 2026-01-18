using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Reflection;
using FirebirdSql.Data.FirebirdClient;
using Ecoinv.Common;

namespace Ecoinv.BL
{
    public class TableBaseClass : SimpleModel
    {
        protected TableBaseClass() { }

        public static ObservableCollection<T> BaseReadList<T>(FbDataReader dataReader)
        {
            ObservableCollection<T> list = new ObservableCollection<T>();
            if (dataReader == null) return list;

            while (dataReader.Read())
            {
                T obj = Activator.CreateInstance<T>();
                foreach (PropertyInfo prop in obj.GetType().GetProperties())
                {
                    if (prop.Name == "PrimaryKeyValue") continue;
                    try
                    {
                        int ordinal = dataReader.GetOrdinal(prop.Name);
                        if (!dataReader.IsDBNull(ordinal))
                        {
                            var targetType = prop.PropertyType;
                            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
                                targetType = Nullable.GetUnderlyingType(targetType);

                            object value = Convert.ChangeType(dataReader[prop.Name], targetType);
                            prop.SetValue(obj, value, null);
                        }
                    }
                    catch { }
                }
                list.Add(obj);
            }
            return list;
        }

        public static ObservableCollection<T> GetListBase<T>(string selectSQL, FBConnectX conn)
        {
            if (conn != null)
            {
                return InternalGetList<T>(selectSQL, conn);
            }
            else
            {
                using (FBConnectX localConn = new FBConnectX())
                {
                    localConn.GetConnectionX();
                    localConn.FBConnOpenX();
                    return InternalGetList<T>(selectSQL, localConn);
                }
            }
        }

        private static ObservableCollection<T> InternalGetList<T>(string sql, FBConnectX conn)
        {
            if (conn.GetConStateX() != ConnectionState.Open) conn.FBConnOpenX();

            using (var fbtr = conn.FBConnBeginTransactionX())
            using (var cmd = new FbCommand(sql, conn.FBCConnectionX, fbtr))
            using (var fbdr = cmd.ExecuteReader())
            {
                var list = BaseReadList<T>(fbdr);
                fbtr.Commit();
                return list;
            }
        }
    }
}