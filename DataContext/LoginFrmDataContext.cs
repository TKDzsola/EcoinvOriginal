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
            execute: param =>
            {
                CloseFBConn();
                CloseAppDB();
            },
            canExecute: param => true
        );

        #region ... LogPassword property ...

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static string __logpassword = "*";

        public static string LogPassword
        {
            get => __logpassword;
            set => __logpassword = value;
        }

        #endregion ... end of LogPassword property ...


        private static void DoLogin()
        {
            FBConnX = new FBConnectX();
            FBConnX?.GetConnectionX();

            if (FBConnX?.FBCConnectionX != null)
            {
                if (GetLogin())
                {
                    var lgfloginform = Application.Current.Windows[0];
                    lgfloginform?.Hide();

                    FBConnX?.FBConnCloseX();

                    var startfrm = new StartFrm();
                    startfrm.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Hibás bejelentkezés!" + Environment.NewLine + "Inaktív felhasználó, vagy hibás név/jelszó páros!");
                }
            }
            else
            {
                MessageBox.Show("Adatbázis kapcsolódási hiba!");
            }
        }

        private static bool GetLogin()
        {
            var retvalue = false;
            if (LogPassword == null)
                return false;

            try
            {
                var md5f = new MD5Func();
                var logpsswMD5 = md5f.MD5Encode(LogPassword);

                var dbpsswMD5 = string.Empty;
                var dbIsAdmin = "N"; // Alapértelmezetten NEM admin

                // SQL: Jelszó és Admin jog lekérése
                var alkres = FBConnX.SelectSQL_FirstRow("select upssw, isadmin from eusers where uname = @uname and uactive='I'",
                                                        new FbParameter("@uname", LoginUserName));

                if (alkres != null && alkres.Length >= 2)
                {
                    dbpsswMD5 = alkres[0]?.ToString(); // Jelszó hash
                    dbIsAdmin = alkres[1]?.ToString(); // "I" vagy "N"
                }

                if (string.IsNullOrEmpty(dbpsswMD5))
                    return false;

                // Jelszó ellenőrzés
                if (logpsswMD5 == dbpsswMD5)
                {
                    // --- JOGOSULTSÁG BEÁLLÍTÁSA ---
                    // Itt kötjük össze a globális változóval, amit a számlázó figyel
                    if (dbIsAdmin == "I")
                    {
                        DataContextBase.IsAdmin = true;
                    }
                    else
                    {
                        DataContextBase.IsAdmin = false;
                    }
                    // ------------------------------

                    retvalue = true;
                }

                return retvalue;
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message);
                throw new Exception(Ex.Message);
            }
        }

        private static void CloseFBConn()
        {
            FBConnX?.FBConnCloseX();
        }

        private static void CloseAppDB()
        {
            if (System.Windows.Application.Current.MainWindow != null)
                System.Windows.Application.Current.MainWindow.Close();
        }
    }
}