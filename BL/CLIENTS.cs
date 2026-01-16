using Ecoinv.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecoinv.BL
{
  public partial class CLIENTS : TableBaseClass
  {
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

    #region ... NAME property ...

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string __name;
    public string NAME
    {
      get => __name;
      set
      {
        OnNAMEChanging(value);
        SetPropertyValue(nameof(NAME), ref __name, value);
        OnNAMEChanged();
      }
    }
    private void OnNAMEChanging(string value) { }
    private void OnNAMEChanged() { }

    #endregion ... end of NAME property ...

    #region ... TAX_NUMBER property ...

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string __tax_number;
    public string TAX_NUMBER
    {
      get => __tax_number;
      set
      {
        OnTAX_NUMBERChanging(value);
        SetPropertyValue(nameof(TAX_NUMBER), ref __tax_number, value);
        OnTAX_NUMBERChanged();
      }
    }
    private void OnTAX_NUMBERChanging(string value) { }
    private void OnTAX_NUMBERChanged() { }

    #endregion ... end of TAX_NUMBER property ...


    #region ... PHONE property ...

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string __phone;
    public string PHONE
    {
      get => __phone;
      set
      {
        OnPHONEChanging(value);
        SetPropertyValue(nameof(PHONE), ref __phone, value);
        OnPHONEChanged();
      }
    }
    private void OnPHONEChanging(string value) { }
    private void OnPHONEChanged() { }

    #endregion ... end of PHONE property ...

    #region ... EMAIL property ...

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string __email;
    public string EMAIL
    {
      get => __email;
      set
      {
        OnEMAILChanging(value);
        SetPropertyValue(nameof(EMAIL), ref __email, value);
        OnEMAILChanged();
      }
    }
    private void OnEMAILChanging(string value) { }
    private void OnEMAILChanged() { }

    #endregion ... end of EMAIL property ...

    #region ... BANKACCOUNT property ...

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string __bankaccount;
    public string BANKACCOUNT
    {
      get => __bankaccount;
      set
      {
        OnBANKACCOUNTChanging(value);
        SetPropertyValue(nameof(BANKACCOUNT), ref __bankaccount, value);
        OnBANKACCOUNTChanged();
      }
    }
    private void OnBANKACCOUNTChanging(string value) { }
    private void OnBANKACCOUNTChanged() { }

    #endregion ... end of BANKACCOUNT property ...

    #region ... CACTIVE property ...

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string __cactive;
    public string CACTIVE
    {
      get => __cactive;
      set
      {
        OnCACTIVEChanging(value);
        SetPropertyValue(nameof(CACTIVE), ref __cactive, value);
        OnCACTIVEChanged();
      }
    }
    private void OnCACTIVEChanging(string value) { }
    private void OnCACTIVEChanged() { }

    #endregion ... end of CACTIVE property ...

    #region ... COMTAX_NUMBER property ...

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string __comtax_number;
    public string COMTAX_NUMBER
    {
      get => __comtax_number;
      set
      {
        OnCOMTAX_NUMBERChanging(value);
        SetPropertyValue(nameof(COMTAX_NUMBER), ref __comtax_number, value);
        OnCOMTAX_NUMBERChanged();
      }
    }
    private void OnCOMTAX_NUMBERChanging(string value) { }
    private void OnCOMTAX_NUMBERChanged() { }

    #endregion ... end of COMTAX_NUMBER property ...
  }

  public partial class CLIENTSTable
  {
    private readonly string selectSQL = "select id, name, tax_number, comtax_number, phone, email, bankaccount, cactive from clients";

    private readonly string insSQL = "insert into clients (name, tax_number, comtax_number, phone, email, bankaccount, cactive) values ({0},'{1}','{2}','{3}','{4}', '{5}','{6}', '{7}')";
    private readonly string delSQL = "delete from clients where id = {0}";
    private readonly string updSQL = "update clients set fulnev='{0}', logpsw='{1}', aktive='{2}' where id = {3}";
    private readonly string selGenSQL = "select gen_id(GEN_CLIENTS_ID,1) from rdb$database";

    private ObservableCollection<CLIENTS> __innerList;

    public ObservableCollection<CLIENTS> GetList(FBConnectX conn)
    {
      if (__innerList != null)
        return __innerList;

      __innerList = TableBaseClass.GetListBase<CLIENTS>(selectSQL, conn);

      return __innerList;
    }

    private int GetGenerator(FBConnectX conn) => DBFunc.Get_Generator(selGenSQL, conn);

    public CLIENTS NewCLIENTS_M()
    {
      var rec = new CLIENTS
      {
        ID = -1,
        NAME = "",
        TAX_NUMBER = "",
        PHONE = "",
        EMAIL = "",
        BANKACCOUNT = "",
        CACTIVE = "I",
        COMTAX_NUMBER = ""
      };
      __innerList.Add(rec);
      return rec;
    }

    public void DelNewCLIENTS_M()
    {
      var _actrec = __innerList.FirstOrDefault(r => r.ID == -1); // ezért -1, mert GetNewHOTALK-ba ezzel kerül bele
      if (__innerList.IndexOf(_actrec) != -1)
        __innerList.Remove(_actrec);
    }

    public void ReUpdateCLIENTS_M(CLIENTS oldclients)
    {
      // Visszaírás CANCEL gomb megnyomása után
      var _actrec = __innerList.FirstOrDefault(r => r.ID == oldclients.ID); // ezért -1, mert GetNewHOTALK-ba ezzel kerül bele
      if (_actrec == null)
        return;

      _actrec.NAME = oldclients.NAME;
      _actrec.TAX_NUMBER = oldclients.TAX_NUMBER;
      _actrec.PHONE = oldclients.PHONE;
      _actrec.EMAIL = oldclients.EMAIL;
      _actrec.BANKACCOUNT = oldclients.BANKACCOUNT;
      _actrec.CACTIVE = oldclients.CACTIVE;
      _actrec.COMTAX_NUMBER = oldclients.COMTAX_NUMBER;
    }

    public void Insert(CLIENTS src, FBConnectX conn)
    {
      var _id = GetGenerator(conn);
      var _inssql = string.Format(insSQL, _id, src.NAME, src.TAX_NUMBER, src.PHONE, src.EMAIL, src.BANKACCOUNT, src.CACTIVE, src.COMTAX_NUMBER);
      conn?.InsertSQL(_inssql);
      var _actrec = __innerList.FirstOrDefault(r => r.ID == -1);  // ezért -1, mert GetNewHOTALK-ba ezzel kerül bele
      if (__innerList.IndexOf(_actrec) != -1)
        src.ID = _id;
    }

    public void Update(CLIENTS src, FBConnectX conn)
    {
      var _actrec = __innerList.FirstOrDefault(r => r.ID == src.ID);
      if (__innerList.IndexOf(_actrec) != -1)
      {
        var _updsql = string.Format(updSQL, src.NAME, src.TAX_NUMBER, src.PHONE, src.EMAIL, src.BANKACCOUNT, src.CACTIVE, src.COMTAX_NUMBER);
        conn?.UpdateSQL(_updsql);
      }
    }

    public void Delete(int num, FBConnectX conn)
    {
      var _actrec = __innerList.FirstOrDefault(r => r.ID == num);
      if (__innerList.IndexOf(_actrec) != -1)
      {
        __innerList.Remove(_actrec);
        var _delsql = string.Format(delSQL, num);
        conn?.DeleteSQL(_delsql);
      }
    }

  }
}
