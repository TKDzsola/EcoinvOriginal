using System;
using System.Data;
using FirebirdSql.Data.FirebirdClient;


namespace Ecoinv.Common
{
  public class DBFunc
  {
    public DBFunc()
    {
    }


    public static int Get_Generator(string SERVICES, FBConnectX conn)
    {
      var genvalue = -1;
      
      if (conn == null) {
        conn = new FBConnectX();
        conn.GetConnectionX();
      }
      conn.FBConnOpenX();

      var fbtr = conn.FBConnBeginTransactionX(); //IsolationLevel.RepeatableRead
      try
      {
        var cmd = new FbCommand(SERVICES, conn?.FBCConnectionX, fbtr);
        var fbdr = cmd.ExecuteReader();

        if (fbdr != null)
        {
          fbdr.Read();
          genvalue = Convert.ToInt32(fbdr[0].ToString());
        }

        fbtr?.Commit();
        cmd.Connection.Close();
        fbdr?.Close();
        fbtr?.Dispose();
        
        conn.FBConnCloseX();
      }
      catch (Exception Ex)
      {
        fbtr?.Rollback();
        fbtr?.Dispose();
        conn.FBConnCloseX();

        throw new Exception(Ex.Message);
      }

      return genvalue;
    }

  }
}
