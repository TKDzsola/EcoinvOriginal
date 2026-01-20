using System;
using System.Diagnostics;
using System.Windows.Input;
using Ecoinv.Common;
using Ecoinv.Components;
using FirebirdSql.Data.FirebirdClient;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace Ecoinv.DataContext
{
    public class LoginFrmDataContext : DataContextBase
    {
        public DelegateCommand LoginCmd { get; } = new(
            execute: param => DoLogin(),
            canExecute: param => LoginUserName.Length > 0
        );

        public DelegateCommand KilepCmd { get; } = new(
            execute: param => CloseAppDB(),
            canExecute: param => true
        );

        #region ... LogPassword property ...
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static string __logpassword = "";
        public static string LogPassword
        {
            get => __logpassword;
            set => __logpassword = value;
        }
        #endregion

        private static void DoLogin()
        {
            using (FBConnectX conn = new FBConnectX())
            {
                conn.GetConnectionX();
                conn.FBConnOpenX();

                if (conn.FBCConnectionX != null)
                {
                    if (GetLogin(conn))
                    {
                        Logger.Log($"Sikeres bejelentkezés: {LoginUserName}");
                        var lgfloginform = Application.Current.Windows[0];
                        lgfloginform?.Hide();

                        var startfrm = new StartFrm();
                        startfrm.ShowDialog();
                    }
                    else
                    {
                        Logger.Log($"Sikertelen bejelentkezési kísérlet: {LoginUserName}", "WARNING");
                        MessageBox.Show("Hibás bejelentkezés!" + Environment.NewLine + "Inaktív felhasználó, vagy hibás név/jelszó páros!");
                    }
                }
                else
                {
                    MessageBox.Show("Adatbázis kapcsolódási hiba!");
                }
            }
        }

        private static bool GetLogin(FBConnectX conn)
        {
            if (LogPassword == null) return false;

            try
            {
                var md5f = new MD5Func();
                var logpsswMD5 = md5f.MD5Encode(LogPassword);

                var alkres = conn.SelectSQL_FirstRow(
                    "select upssw, isadmin from eusers where uname = @uname and uactive='I'",
                    new FbParameter("@uname", LoginUserName));

                if (alkres != null && alkres.Length >= 2)
                {
                    string dbpsswMD5 = alkres[0]?.ToString();
                    string dbIsAdmin = alkres[1]?.ToString();

                    if (logpsswMD5 == dbpsswMD5)
                    {
                        DataContextBase.IsAdmin = (dbIsAdmin == "I");
                        return true;
                    }
                }
                return false;
            }
            catch (Exception Ex)
            {
                // JAVÍTÁS: Naplózzuk a konkrét hibát a belépésnél
                Logger.LogError(Ex, $"Bejelentkezési adatbázis hiba. User: {LoginUserName}");
                MessageBox.Show(Ex.Message);
                return false;
            }
        }

        private static void CloseAppDB()
        {
            if (System.Windows.Application.Current.MainWindow != null)
                System.Windows.Application.Current.MainWindow.Close();
        }
    }
}