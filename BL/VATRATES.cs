using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Ecoinv.Common;

namespace Ecoinv.BL
{
  public partial class VATRATES : TableBaseClass
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
    private void OnIDChanging(int value) { }
    private void OnIDChanged() { }

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


    #region ... RATES property ...

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private int __rates;
    public int RATES
    {
      get => __rates;
      set
      {
        OnRATESChanging(value);
        SetPropertyValue(nameof(RATES), ref __rates, value);
        OnRATESChanged();
      }
    }
    private void OnRATESChanging(int value) { }
    private void OnRATESChanged() { }

    #endregion ... end of RATES property ...

    #region ... VACTIVE property ...

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string __vactive;

    public string VACTIVE
    {
      get => __vactive;
      set
      {
        OnVACTIVEChanging(value);
        SetPropertyValue(nameof(VACTIVE), ref __vactive, value);
        OnVACTIVEChanged();
      }
    }

    /*partial*/
    private void OnVACTIVEChanging(string value)
    {
    }

    /*partial*/
    private void OnVACTIVEChanged()
    {
    }

    #endregion ... end of VACTIVE property ...

    public object PrimaryKeyValue => ID;
  }

  public partial class VATRATESTable
  {
    private readonly string selectSQL = "SELECT ID, NAME, RATES, VACTIVE FROM VATRATES";
    private readonly string insSQL = "INSERT INTO VATRATES (ID, NAME, RATES, VACTIVE) VALUES ({0}, '{1}', {2}, '{3}')";
    private readonly string delSQL = "DELETE FROM VATRATES WHERE ID = {0}";
    private readonly string updSQL = "UPDATE VATRATES SET NAME='{0}', RATES={1}, VACTIVE='{2}' WHERE ID={3}";
    private readonly string selGenSQL = "SELECT GEN_ID(GEN_VATRATES_ID, 1) FROM RDB$DATABASE";

    private ObservableCollection<VATRATES> __innerList;

    public ObservableCollection<VATRATES> GetList(FBConnectX conn)
    {
      if (__innerList != null)
        return __innerList;

      __innerList = TableBaseClass.GetListBase<VATRATES>(selectSQL, conn);

      return __innerList;
    }

    private int GetGenerator(FBConnectX conn) => DBFunc.Get_Generator(selGenSQL, conn);

    public VATRATES NewVATRATES_M()
    {
      var rec = new VATRATES
      {
        ID = -1,
        NAME = "",
        RATES = -1,
        VACTIVE = "I"
      };
      __innerList.Add(rec);
      return rec;
    }

    public void DelNewVATRATES_M()
    {
      var _actrec = __innerList.FirstOrDefault(r => r.ID == -1); // ezért -1, mert GetNewHOTALK-ba ezzel kerül bele
      if (__innerList.IndexOf(_actrec) != -1)
        __innerList.Remove(_actrec);
    }

    public void ReUpdateVATRATES_M(VATRATES oldvatrates)
    {
      // Visszaírás CANCEL gomb megnyomása után
      var _actrec = __innerList.FirstOrDefault(r => r.ID == oldvatrates.ID); // ezért -1, mert GetNewHOTALK-ba ezzel kerül bele
      if (_actrec == null)
        return;

      _actrec.NAME = oldvatrates.NAME;
      _actrec.RATES = oldvatrates.RATES;
    }

    public void Insert(VATRATES src, FBConnectX conn)
    {
      var _id = GetGenerator(conn);
      var _inssql = string.Format(insSQL, _id, src.NAME, src.RATES, src.VACTIVE);
      conn?.InsertSQL(_inssql);
      var _actrec = __innerList.FirstOrDefault(r => r.ID == -1);  // ezért -1, mert GetNewHOTALK-ba ezzel kerül bele
      if (__innerList.IndexOf(_actrec) != -1)
        src.ID = _id;
    }

    public void Update(VATRATES src, FBConnectX conn)
    {
      var _actrec = __innerList.FirstOrDefault(r => r.ID == src.ID);
      if (__innerList.IndexOf(_actrec) != -1)
      {
        var _updsql = string.Format(updSQL, src.ID, src.NAME, src.RATES);
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
