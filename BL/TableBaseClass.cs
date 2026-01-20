using System;
using System.Collections.Generic;
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
                    catch { /* Itt nem logolunk, mert ez gyakran csak hiányzó oszlop */ }
                }
                list.Add(obj);
            }
            return list;
        }

        // Meglévő metódus a kompatibilitás miatt
        public static ObservableCollection<T> GetListBase<T>(string selectSQL, FBConnectX conn)
        {
            return GetListBase<T>(selectSQL, conn, null);
        }

        // ÚJ METÓDUS: Ez kezeli a paraméterezett lekérdezéseket a dátumhiba ellen
        public static ObservableCollection<T> GetListBase<T>(string selectSQL, FBConnectX conn, FbParameter[] parameters)
        {
            if (conn != null)
            {
                return InternalGetList<T>(selectSQL, conn, parameters);
            }
            else
            {
                using (FBConnectX localConn = new FBConnectX())
                {
                    localConn.GetConnectionX();
                    localConn.FBConnOpenX();
                    return InternalGetList<T>(selectSQL, localConn, parameters);
                }
            }
        }

        // MÓDOSÍTOTT BELSŐ METÓDUS: Átadja a paramétereket az FbCommand-nak
        private static ObservableCollection<T> InternalGetList<T>(string sql, FBConnectX conn, FbParameter[] parameters = null)
        {
            if (conn.GetConStateX() != ConnectionState.Open) conn.FBConnOpenX();

            FbTransaction fbtr = null;
            try
            {
                fbtr = conn.FBConnBeginTransactionX();
                using (var cmd = new FbCommand(sql, conn.FBCConnectionX, fbtr))
                {
                    // Ha vannak paraméterek (pl. dátumok), hozzáadjuk őket a parancshoz
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }

                    using (var fbdr = cmd.ExecuteReader())
                    {
                        var list = BaseReadList<T>(fbdr);
                        fbtr.Commit();
                        return list;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"TableBaseClass.InternalGetList hiba. SQL: {sql}");
                fbtr?.Rollback();
                throw;
            }
            finally
            {
                fbtr?.Dispose();
            }
        }
    }
}