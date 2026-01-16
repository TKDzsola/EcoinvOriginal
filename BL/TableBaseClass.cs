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

            if (!object.Equals(dataReader[prop.Name], DBNull.Value))
            {
              prop.SetValue(obj, Convert.ChangeType(dataReader[prop.Name], prop.PropertyType), null);
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
        conn.GetConnectionX();

      if (conn.GetConStateX() != ConnectionState.Open)
        conn.FBConnOpenX();


      var fbtr = conn.FBConnBeginTransactionX(); //IsolationLevel.RepeatableRead
      try
      {
        var cmd = new FbCommand(selectSQL, conn.FBCConnectionX, fbtr);
        var fbdr = cmd.ExecuteReader();

        var tmpRecordList = BaseReadList<T>(fbdr);

        fbtr.Commit();
        cmd.Connection.Close();
        fbdr.Close();
        fbtr.Dispose();
        conn.FBConnCloseX();

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