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
        protected TableBaseClass()
        {

        }

        public static ObservableCollection<T> BaseReadList<T>(FbDataReader dataReader)
        {
            ObservableCollection<T> list = new ObservableCollection<T>();
            T obj = default(T);

            if (dataReader != null)
            {
                while (dataReader.Read())
                {
                    obj = Activator.CreateInstance<T>();
                    foreach (PropertyInfo prop in obj.GetType().GetProperties())
                    {
                        if (prop.Name == "PrimaryKeyValue") continue;

                        // Ellenőrizzük, hogy létezik-e az oszlop az eredményhalmazban
                        // (Ez megakadályozza a hibát, ha a Modelben van olyan property, ami nincs a Select-ben)
                        bool columnExists = false;
                        try
                        {
                            // Gyors ellenőrzés
                            int ordinal = dataReader.GetOrdinal(prop.Name);
                            columnExists = true;
                        }
                        catch { columnExists = false; }

                        if (columnExists && !object.Equals(dataReader[prop.Name], DBNull.Value))
                        {
                            // --- JAVÍTÁS KEZDETE: Nullable típusok kezelése ---
                            var targetType = prop.PropertyType;

                            // Ha a cél típus Nullable (pl. DateTime?), akkor kivesszük az alaptípust (DateTime)
                            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
                            {
                                targetType = Nullable.GetUnderlyingType(targetType);
                            }

                            // Konverzió az alaptípusra
                            object value = Convert.ChangeType(dataReader[prop.Name], targetType);
                            prop.SetValue(obj, value, null);
                            // --- JAVÍTÁS VÉGE ---
                        }
                    }

                    list.Add(obj);
                }
            }
            return list;
        }


        public static ObservableCollection<T> GetListBase<T>(string selectSQL, FBConnectX conn)
        {
            if (conn == null)
                conn = new FBConnectX(); // Javítva: ha null volt, példányosítjuk, de nem csak a GetConnectionX-et hívjuk

            conn.GetConnectionX(); // Biztosítjuk, hogy legyen connection object

            if (conn.GetConStateX() != ConnectionState.Open)
                conn.FBConnOpenX();

            var fbtr = conn.FBConnBeginTransactionX(); //IsolationLevel.RepeatableRead
            try
            {
                var cmd = new FbCommand(selectSQL, conn.FBCConnectionX, fbtr);
                var fbdr = cmd.ExecuteReader();

                var tmpRecordList = BaseReadList<T>(fbdr);

                fbtr.Commit();
                // cmd.Connection.Close(); // NE zárjuk le itt a kapcsolatot, mert a tranzakciót már kommitoltuk, a connectiont a hívó kezeli vagy a finally
                fbdr.Close();
                fbtr.Dispose();

                // conn.FBConnCloseX(); // Ezt is inkább a hívóra vagy a finally-re bízzuk a Managerben, de itt maradhat, ha így szoktad.

                return tmpRecordList;
            }
            catch (Exception Ex)
            {
                fbtr?.Rollback();
                fbtr?.Dispose();
                conn.FBConnCloseX();

                throw new Exception(Ex.Message);
            }
        }
    }
}