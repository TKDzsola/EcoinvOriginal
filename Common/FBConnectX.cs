using System;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using FirebirdSql.Data.FirebirdClient;

namespace Ecoinv.Common
{
  public class FBConnectX
  {
    public FBConnectX()
    {

    }

    //#region ... FBConnStateX property ...

    //[DebuggerBrowsable(DebuggerBrowsableState.Never)]
    //private ConnectionState __fbconnstatex;

    //public ConnectionState FBConnStateX
    //{
    //  get => __fbconnstatex;
    //  set => __fbconnstatex = value;
    //}

    //#endregion ... end of FBConnStateX property ...

    #region ... FBCConnectionX property ...

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private FbConnection __fbcconnectionx;

    public FbConnection FBCConnectionX
    {
      get => __fbcconnectionx;
      set => __fbcconnectionx = value;
    }

    #endregion ... end of FBCConnectionX property ...

    public void FBConnOpenX()
    {
      if (GetConStateX() != ConnectionState.Open)
        FBCConnectionX?.Open();
    }

    public FbTransaction FBConnBeginTransactionX() => FBCConnectionX?.BeginTransaction();

    public FbTransaction FBConnBeginTransactionROX() => FBCConnectionX?.BeginTransaction(IsolationLevel.ReadCommitted);

    //public FbCommand FBConnFbCommand(string cmdText, FbConnection connection, FbTransaction transaction) => new FbCommand(cmdText, connection, transaction);


    public void GetConnectionX()
    {
      var appset = new AppSettingsReader();
      var csb = new FbConnectionStringBuilder
      {
        // App.config -ban deffiniálva
        //UserID = "HOSTWARE",
        //Password = "none",
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

      FBCConnectionX ??= new FbConnection(csb.ToString());
    }

    public ConnectionState GetConStateX() => FBCConnectionX.State;

    public void FBConnCloseX()
    {
      if (FBCConnectionX != null)
        if (GetConStateX() != ConnectionState.Closed)
          FBCConnectionX.Close();
    }

    public string SelectSQL_FirstCol(string sqlstr)
    {
      var retstr = string.Empty;
      FbTransaction fbtr = null;
      try
      {
        GetConnectionX();
        FBConnOpenX();
        fbtr = FBCConnectionX?.BeginTransaction(); //IsolationLevel.RepeatableRead
        var cmd = new FbCommand(sqlstr, FBCConnectionX, fbtr);
        var fbdr = cmd.ExecuteReader();
        if (fbdr.Read())
          if (!fbdr.IsDBNull(0))
            retstr = fbdr.GetString(0);

        fbtr?.Commit();
        cmd.Connection.Close();
        fbdr.Close();
        fbtr?.Dispose();
        
        FBConnCloseX();
      }
      catch (Exception Ex)
      {
        fbtr?.Rollback();
        fbtr?.Dispose();
        FBConnCloseX();

        throw new Exception(Ex.Message);
      }

      return retstr;
    }



    public object[] SelectSQL_FirstRow(string sqlstr, params FbParameter[] parameters)
    {
      FbTransaction fbtr = null;
      try
      {
        GetConnectionX();
        FBConnOpenX();

        fbtr = FBCConnectionX?.BeginTransaction(IsolationLevel.ReadCommitted);

        using (var cmd = new FbCommand(sqlstr, FBCConnectionX, fbtr))
        {
          if (parameters != null)
            cmd.Parameters.AddRange(parameters);

          using (var reader = cmd.ExecuteReader())
          {
            if (reader.Read())
            {
              var values = new object[reader.FieldCount];
              reader.GetValues(values); // kitölti az array-t
              fbtr?.Commit();
              return values; // object[] (Array-ként visszaadva)
            }
          }
        }

        fbtr?.Commit();
        return null;
      }
      catch
      {
        // Hiba esetén rollback, majd továbbdobjuk a kivételt
        try
        {
          fbtr?.Rollback();
        }
        catch
        {
          /* ignoráljuk rollback hibákat */
        }

        throw;
      }
      finally
      {
        // Kapcsolat és tranzakció takarítása
        try
        {
          FBCConnectionX?.Close();
        }
        catch
        {
        }

        try
        {
          fbtr?.Dispose();
        }
        catch
        {
        }
      }
    }



    public string InsertSQL(string sqlstr)
    {
      var retstr = string.Empty;
      FbTransaction fbtr = null;
      GetConnectionX();
      FBConnOpenX();
      
      try
      {
        fbtr = FBCConnectionX?.BeginTransaction(); //IsolationLevel.RepeatableRead
        var cmd = new FbCommand(sqlstr, FBCConnectionX, fbtr);
        cmd.ExecuteNonQuery();
        fbtr?.Commit();
        cmd.Connection.Close();
        fbtr?.Dispose();
      }
      catch (Exception Ex)
      {
        fbtr?.Rollback();
        fbtr?.Dispose();
        FBConnCloseX();

        throw new Exception(Ex.Message);
      }

      return retstr;
    }

    public string DeleteSQL(string sqlstr)
    {
      var retstr = string.Empty;
      FbTransaction fbtr = null;
      GetConnectionX();
      FBConnOpenX();

      try
      {
        fbtr = FBCConnectionX?.BeginTransaction(); //IsolationLevel.RepeatableRead
        var cmd = new FbCommand(sqlstr, FBCConnectionX, fbtr);
        cmd.ExecuteNonQuery();
        fbtr?.Commit();
        cmd.Connection.Close();
        fbtr?.Dispose();
      }
      catch (Exception Ex)
      {
        fbtr?.Rollback();
        fbtr?.Dispose();
        FBConnCloseX();

        throw new Exception(Ex.Message);
      }

      return retstr;
    }

    public string UpdateSQL(string sqlstr)
    {
      var retstr = string.Empty;
      FbTransaction fbtr = null;
      GetConnectionX();
      FBConnOpenX();
      try
      {
        fbtr = FBCConnectionX?.BeginTransaction(); //IsolationLevel.RepeatableRead
        var cmd = new FbCommand(sqlstr, FBCConnectionX, fbtr);
        cmd.ExecuteNonQuery();
        fbtr?.Commit();
        cmd.Connection.Close();
        fbtr?.Dispose();
      }
      catch (Exception Ex)
      {
        fbtr?.Rollback();
        fbtr?.Dispose();
        FBConnCloseX();

        throw new Exception(Ex.Message);
      }

      return retstr;
    }


  }
}