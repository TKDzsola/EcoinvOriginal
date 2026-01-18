using System;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using FirebirdSql.Data.FirebirdClient;
using System.Windows; // MessageBox miatt

namespace Ecoinv.Common
{
    // A ": IDisposable" jelzi, hogy az osztály támogatja a 'using' blokkot és a takarítást
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

        // -------------------------------------------------------------------
        // KAPCSOLAT LÉTREHOZÁSA (A te App.Config logikáddal)
        // -------------------------------------------------------------------
        public void GetConnectionX()
        {
            // Ha már létezik a kapcsolat objektum, nem hozzuk létre újra
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
                MessageBox.Show("Hiba a Connection String összeállításakor:\n" + ex.Message);
            }
        }

        // -------------------------------------------------------------------
        // KAPCSOLAT NYITÁSA / ZÁRÁSA
        // -------------------------------------------------------------------
        public void FBConnOpenX()
        {
            try
            {
                if (FBCConnectionX == null) GetConnectionX();

                if (GetConStateX() != ConnectionState.Open)
                    FBCConnectionX?.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Adatbázis kapcsolódási hiba:\n" + ex.Message);
            }
        }

        public void FBConnCloseX()
        {
            if (FBCConnectionX != null && GetConStateX() != ConnectionState.Closed)
            {
                FBCConnectionX.Close();
            }
        }

        public ConnectionState GetConStateX()
        {
            return FBCConnectionX?.State ?? ConnectionState.Closed;
        }

        public FbTransaction FBConnBeginTransactionX() => FBCConnectionX?.BeginTransaction();

        // -------------------------------------------------------------------
        // SQL VÉGREHAJTÓK (Egyszerűsítve az IDisposable mintához)
        // Mostantól a hívó fél (a using blokk) felel a kapcsolat nyitvatartásáért!
        // -------------------------------------------------------------------
        public string InsertSQL(string sqlstr) => ExecuteSimpleSQL(sqlstr);
        public string UpdateSQL(string sqlstr) => ExecuteSimpleSQL(sqlstr);
        public string DeleteSQL(string sqlstr) => ExecuteSimpleSQL(sqlstr);

        private string ExecuteSimpleSQL(string sqlstr)
        {
            if (FBCConnectionX == null || GetConStateX() != ConnectionState.Open) return "Connection Error";

            try
            {
                // Itt nem nyitunk/zárunk tranzakciót, hanem a nyitott kapcsolaton futtatjuk.
                // A 'using' itt a Command objektumot takarítja el futás után.
                using (var cmd = new FbCommand(sqlstr, FBCConnectionX))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                // Ha hiba van, eldobjuk, hogy a hívó (pl. a Manager) tudja kezelni
                throw new Exception(ex.Message);
            }

            return string.Empty;
        }

        // -------------------------------------------------------------------
        // LEKÉRDEZŐK (Refaktorálva a biztonságos működéshez)
        // -------------------------------------------------------------------
        public string SelectSQL_FirstCol(string sqlstr)
        {
            var retstr = string.Empty;

            // Biztosítjuk, hogy nyitva legyen (ha a using blokkon belül hívják)
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
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            // Nem zárjuk be a kapcsolatot, mert a using blokk fogja a végén!
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
                            return values;
                        }
                    }
                }
                return null;
            }
            catch
            {
                throw;
            }
        }

        // =================================================================
        // IDISPOSABLE IMPLEMENTÁCIÓ (A Lényeg!)
        // =================================================================
        public void Dispose()
        {
            // 1. Bezárjuk a kapcsolatot
            FBConnCloseX();

            // 2. Megszüntetjük az objektumot a memóriában
            if (FBCConnectionX != null)
            {
                FBCConnectionX.Dispose();
                FBCConnectionX = null;
            }

            // 3. Jelezzük a Garbage Collectornek, hogy végeztünk
            GC.SuppressFinalize(this);
        }
    }
}