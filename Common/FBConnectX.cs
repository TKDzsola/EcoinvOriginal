using System;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using FirebirdSql.Data.FirebirdClient;
using System.Windows;
using Ecoinv.Common; // Biztosítjuk a Logger elérését

namespace Ecoinv.Common
{
    public class FBConnectX : IDisposable
    {
        public FBConnectX()
        {
        }

        #region ... FBCConnectionX property ...

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private FbConnection __fbcconnectionx;

        public FbConnection FBCConnectionX
        {
            get => __fbcconnectionx;
            set => __fbcconnectionx = value;
        }

        #endregion

        public void GetConnectionX()
        {
            if (FBCConnectionX != null) return;

            try
            {
                var appset = new AppSettingsReader();
                var csb = new FbConnectionStringBuilder
                {
                    UserID = appset.GetValue("UserID", typeof(string)).ToString(),
                    Password = appset.GetValue("Password", typeof(string)).ToString(),
                    Database = appset.GetValue("Database", typeof(string)).ToString(),
                    DataSource = appset.GetValue("Host", typeof(string)).ToString(),
                    Port = Convert.ToInt32(appset.GetValue("Port", typeof(string)).ToString()),
                    Charset = appset.GetValue("Charset", typeof(string)).ToString(),
                    Pooling = Convert.ToBoolean(appset.GetValue("Pooling", typeof(string)).ToString()),
                    ConnectionLifeTime = Convert.ToInt32(appset.GetValue("ConnectionLifeTime", typeof(string)).ToString()),
                    Dialect = Convert.ToByte(appset.GetValue("Dialect", typeof(string)).ToString()),
                    ServerType = FbServerType.Default
                };

                FBCConnectionX = new FbConnection(csb.ToString());
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "GetConnectionX - Connection String összeállítási hiba");
                MessageBox.Show("Hiba a Connection String összeállításakor:\n" + ex.Message);
            }
        }

        public void FBConnOpenX()
        {
            try
            {
                if (FBCConnectionX == null) GetConnectionX();

                if (GetConStateX() != ConnectionState.Open)
                {
                    FBCConnectionX?.Open();
                    Logger.Log("Adatbázis kapcsolat sikeresen megnyitva.");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "FBConnOpenX - Kapcsolat nyitási hiba");
                MessageBox.Show("Adatbázis kapcsolódási hiba:\n" + ex.Message);
            }
        }

        public void FBConnCloseX()
        {
            if (FBCConnectionX != null && GetConStateX() != ConnectionState.Closed)
            {
                FBCConnectionX.Close();
                Logger.Log("Adatbázis kapcsolat lezárva.");
            }
        }

        public ConnectionState GetConStateX()
        {
            return FBCConnectionX?.State ?? ConnectionState.Closed;
        }

        public FbTransaction FBConnBeginTransactionX() => FBCConnectionX?.BeginTransaction();

        public string InsertSQL(string sqlstr) => ExecuteSimpleSQL(sqlstr, "INSERT");
        public string UpdateSQL(string sqlstr) => ExecuteSimpleSQL(sqlstr, "UPDATE");
        public string DeleteSQL(string sqlstr) => ExecuteSimpleSQL(sqlstr, "DELETE");

        private string ExecuteSimpleSQL(string sqlstr, string type)
        {
            if (FBCConnectionX == null || GetConStateX() != ConnectionState.Open)
            {
                Logger.Log($"Sikertelen {type} művelet: Nincs nyitott kapcsolat.", "WARNING");
                return "Connection Error";
            }

            try
            {
                using (var cmd = new FbCommand(sqlstr, FBCConnectionX))
                {
                    cmd.ExecuteNonQuery();
                    Logger.Log($"{type} sikeres: {sqlstr}");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"ExecuteSimpleSQL ({type}) hiba. SQL: {sqlstr}");
                throw new Exception(ex.Message);
            }

            return string.Empty;
        }

        public string SelectSQL_FirstCol(string sqlstr)
        {
            var retstr = string.Empty;

            if (FBCConnectionX == null) GetConnectionX();
            if (GetConStateX() != ConnectionState.Open) FBConnOpenX();

            try
            {
                using (var cmd = new FbCommand(sqlstr, FBCConnectionX))
                using (var fbdr = cmd.ExecuteReader())
                {
                    if (fbdr.Read() && !fbdr.IsDBNull(0))
                        retstr = fbdr.GetString(0);
                }
                Logger.Log($"SelectSQL_FirstCol sikeres. SQL: {sqlstr}");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"SelectSQL_FirstCol hiba. SQL: {sqlstr}");
                throw new Exception(ex.Message);
            }
            return retstr;
        }

        public object[] SelectSQL_FirstRow(string sqlstr, params FbParameter[] parameters)
        {
            if (FBCConnectionX == null) GetConnectionX();
            if (GetConStateX() != ConnectionState.Open) FBConnOpenX();

            try
            {
                using (var cmd = new FbCommand(sqlstr, FBCConnectionX))
                {
                    if (parameters != null)
                        cmd.Parameters.AddRange(parameters);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var values = new object[reader.FieldCount];
                            reader.GetValues(values);
                            Logger.Log($"SelectSQL_FirstRow sikeres. SQL: {sqlstr}");
                            return values;
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"SelectSQL_FirstRow hiba. SQL: {sqlstr}");
                throw;
            }
        }

        public void Dispose()
        {
            FBConnCloseX();

            if (FBCConnectionX != null)
            {
                FBCConnectionX.Dispose();
                FBCConnectionX = null;
                Logger.Log("FBConnectX erőforrások felszabadítva (Dispose).");
            }

            GC.SuppressFinalize(this);
        }
    }
}