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

    //#region ... FBConnX property ...

    //[DebuggerBrowsable(DebuggerBrowsableState.Never)]
    //private static FBConnectX __fbconnx;

    //public static FBConnectX FBConnX
    //{
    //  get => __fbconnx;
    //  set => __fbconnx = value;
    //}

    //#endregion ... end of FBConn property ...


    #region ... LogPassword property ...

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static string __logpassword = "*";

    public static string LogPassword
    {
      get => __logpassword;
      set => __logpassword = value;
    }

    #endregion ... end of LogPassword property ...

    //#region ... LoginCmd property ...

    //[DebuggerBrowsable(DebuggerBrowsableState.Never)]
    //private LoginCommand _loginCmd;

    //public LoginCommand LoginCmd => _loginCmd ??= new LoginCommand();

    //#endregion ... end of LoginCmd property ...

    //#region ... KilepCmd property ...

    //[DebuggerBrowsable(DebuggerBrowsableState.Never)]
    //private KilepCommand _kilepCmd;

    //public KilepCommand KilepCmd => _kilepCmd ??= new KilepCommand();

    //#endregion ... end of KilepCmd property ...


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
          MessageBox.Show("Hibás bejelentkezés!"+Environment.NewLine+"Inaktív felhasználkó, vagy hibás név/jelszó páros!");
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
        var logpsswMD5 = md5f.MD5Encode(LogPassword); //3389DAE361AF79B04C9C8E7057F60CC6 = csillag
                                                      //var dbpsswMD5 = FBConnX?.SelectSQL_FirstCol("select upssw from eusers where uname = '" + LoginUserName + "' and uactive='I'");
                                                      //todo ehelyett kell egy másik SQL, ami az upssw-t és az isadmin-t visszaadja.       
        var dbpsswMD5 = string.Empty;

        var alkres = FBConnX.SelectSQL_FirstRow("select upssw,isadmin from eusers where uname  = @uname and uactive='I'",
                                                new FbParameter("@uname", LoginUserName));
        if (alkres != null)
        {
          dbpsswMD5 = alkres[0]?.ToString();
          IsAdmin = alkres[1].ToString() == "I";
        }

        if (dbpsswMD5 == "")
          return false;

        if (logpsswMD5 == dbpsswMD5)
        {
          //todo itt kell az IsAdmin-t beállítani

          retvalue = true;
        }

        return retvalue;
      }
      catch (Exception Ex)
      {
        MessageBox.Show(Ex.Message); //new Exception(Ex.Message);
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
