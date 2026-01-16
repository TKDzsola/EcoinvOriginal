using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Ecoinv.BL;
using Ecoinv.Common;

namespace Ecoinv.BL
{
  public partial class EUSERS : TableBaseClass
  {
    private MD5Func md5f;
    public EUSERS()
    {
      md5f = new MD5Func();
    }

    #region ... ID property ...

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private int __id;

    public int ID
    {
      get => __id;
      set
      {
        OnIDChanging(value);
        SetPropertyValue(nameof(ID), ref __id, value);
        OnIDChanged();
      }
    }

    /*partial*/
    private void OnIDChanging(int value)
    {
    }

    /*partial*/
    private void OnIDChanged()
    {
    }

    #endregion ... end of ID property ...

    #region ... UNAME property ...

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string __uname;

    public string UNAME
    {
      get => __uname;
      set
      {
        OnLOGNEVChanging(value);
        SetPropertyValue(nameof(UNAME), ref __uname, value);
        OnLOGNEVChanged();
      }
    }

    /*partial*/
    private void OnLOGNEVChanging(string value)
    {
    }

    /*partial*/
    private void OnLOGNEVChanged()
    {
    }

    #endregion ... end of UNAME property ...

    #region ... UPSSW property ...

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string __upssw;

    public string UPSSW
    {
      get => __upssw;
      set
      {
        OnUPSSWChanging(value);
        SetPropertyValue(nameof(UPSSW), ref __upssw, value);
        OnUPSSWChanged();
      }
    }

    /*partial*/
    private void OnUPSSWChanging(string value)
    {
    }

    /*partial*/
    private void OnUPSSWChanged()
    {
    }

    #endregion ... end of FULNEV property ...

    #region ... UACTIVE property ...

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string __uactive;

    public string UACTIVE
    {
      get => __uactive;
      set
      {
        OnAKTIVEChanging(value);
        SetPropertyValue(nameof(UACTIVE), ref __uactive, value);
        OnAKTIVEChanged();
      }
    }

    /*partial*/
    private void OnAKTIVEChanging(string value)
    {
    }

    /*partial*/
    private void OnAKTIVEChanged()
    {
    }

    #endregion ... end of AKTIVE property ...

    #region ... ISADMIN property ...

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string __isadmin;
    public string ISADMIN
    {
      get => __isadmin;
      set
      {
        OnISADMINChanging(value);
        SetPropertyValue(nameof(ISADMIN), ref __isadmin, value);
        OnISADMINChanged();
      }
    }
    private void OnISADMINChanging(string value) { }
    private void OnISADMINChanged() { }

    #endregion ... end of ISADMIN property ...

    #region ... JELSZO_NO_MD5 property ...

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string __jelszo_no_md5;

    public string JELSZO_NO_MD5
    {
      get => __jelszo_no_md5;
      set
      {
        OnJELSZO_NO_MD5Changing(value);
        SetPropertyValue(nameof(JELSZO_NO_MD5), ref __jelszo_no_md5, value);
        OnJELSZO_NO_MD5Changed();
      }
    }

    /*partial*/
    private void OnJELSZO_NO_MD5Changing(string value)
    {
    }

    /*partial*/
    private void OnJELSZO_NO_MD5Changed()
    {
      UPSSW = md5f.MD5Encode(__jelszo_no_md5); //3389DAE361AF79B04C9C8E7057F60CC6 = csillag
    }

    #endregion ... end of JELSZO_NO_MD5 property ...

    #region ... fulname property ...

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string __fulname;
    public string FULNAME
    {
      get => __fulname;
      set
      {
        OnfulnameChanging(value);
        SetPropertyValue(nameof(FULNAME), ref __fulname, value);
        OnfulnameChanged();
      }
    }
    private void OnfulnameChanging(string value) { }
    private void OnfulnameChanged() { }

    #endregion ... end of fulname property ...
    public object PrimaryKeyValue => ID;

  } // EUSERS


  public partial class EUSERSTable
  {
    private readonly string selectSQL = "SELECT ID, UNAME, UPSSW, UACTIVE, ISADMIN, FULNAME, '' AS JELSZO_NO_MD5 FROM EUSERS";
    private readonly string insSQL = "INSERT INTO EUSERS (ID, UNAME, UPSSW, UACTIVE, ISADMIN, FULNAME) VALUES ({0}, '{1}', '{2}', '{3}', '{4}', '{5}')";
    private readonly string delSQL = "DELETE FROM EUSERS WHERE ID = {0}";
    private readonly string updSQL = "UPDATE EUSERS SET UNAME = '{0}', UPSSW = '{1}', UACTIVE = '{2}', ISADMIN = '{3}', FULNAME = '{4}' WHERE ID = {5}";
    private readonly string selGenSQL = "SELECT GEN_ID(GEN_EUSERS_ID, 1) FROM RDB$DATABASE";

    private ObservableCollection<EUSERS> __innerList;

    public ObservableCollection<EUSERS> GetList(FBConnectX conn)
    {
      if (__innerList != null)
        return __innerList;

      __innerList = TableBaseClass.GetListBase<EUSERS>(selectSQL, conn);
      return __innerList;
    }

    private int GetGenerator(FBConnectX conn) => DBFunc.Get_Generator(selGenSQL, conn);

    public EUSERS NewEUSERS_M()
    {
      var rec = new EUSERS
      {
        ID = -1,
        UNAME = "",
        UPSSW = "",
        JELSZO_NO_MD5 = "",
        UACTIVE = "I",
        ISADMIN = "N",
        FULNAME = ""
      };
      __innerList.Add(rec);
      return rec;
    }

    public void DelNewEUSERS_M()
    {
      var _actrec = __innerList.FirstOrDefault(r => r.ID == -1);
      if (_actrec != null)
        __innerList.Remove(_actrec);
    }

    public void ReUpdateEUSERS_M(EUSERS oldeusers)
    {
      var _actrec = __innerList.FirstOrDefault(r => r.ID == oldeusers.ID);
      if (_actrec == null)
        return;

      _actrec.UNAME = oldeusers.UNAME;
      _actrec.UPSSW = oldeusers.UPSSW;
      _actrec.UACTIVE = oldeusers.UACTIVE;
      _actrec.ISADMIN = oldeusers.ISADMIN;
      _actrec.FULNAME = oldeusers.FULNAME;
      _actrec.JELSZO_NO_MD5 = "";
    }

    public void Insert(EUSERS src, FBConnectX conn)
    {
      var _id = GetGenerator(conn);
      var _inssql = string.Format(insSQL, _id, src.UNAME, src.UPSSW, src.UACTIVE, src.ISADMIN, src.FULNAME);
      conn?.InsertSQL(_inssql);

      var _actrec = __innerList.FirstOrDefault(r => r.ID == -1);
      if (_actrec != null)
        src.ID = _id;
    }

    public void Update(EUSERS src, FBConnectX conn)
    {
      var _actrec = __innerList.FirstOrDefault(r => r.ID == src.ID);
      if (_actrec != null)
      {
        var _updsql = string.Format(updSQL, src.UNAME, src.UPSSW, src.UACTIVE, src.ISADMIN, src.FULNAME, src.ID);
        conn?.UpdateSQL(_updsql);
      }
    }

    public void Delete(int num, FBConnectX conn)
    {
      var _actrec = __innerList.FirstOrDefault(r => r.ID == num);
      if (_actrec != null)
      {
        __innerList.Remove(_actrec);
        var _delsql = string.Format(delSQL, num);
        conn?.DeleteSQL(_delsql);
      }
    }
  }
}

