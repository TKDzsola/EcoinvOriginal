using Ecoinv.Common;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Ecoinv.BL
{
  public partial class SERVICES : TableBaseClass
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

    #region ... DESCRIPTION property ...
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string __description;
    public string DESCRIPTION
    {
      get => __description;
      set
      {
        OnDESCRIPTIONChanging(value);
        SetPropertyValue(nameof(DESCRIPTION), ref __description, value);
        OnDESCRIPTIONChanged();
      }
    }
    private void OnDESCRIPTIONChanging(string value) { }
    private void OnDESCRIPTIONChanged() { }
    #endregion ... end of DESCRIPTION property ...

    #region ... NETPRICE property ...
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private int __netprice;
    public int NETPRICE
    {
      get => __netprice;
      set
      {
        OnNETPRICEChanging(value);
        SetPropertyValue(nameof(NETPRICE), ref __netprice, value);
        OnNETPRICEChanged();
      }
    }
    private void OnNETPRICEChanging(int value) { }
    private void OnNETPRICEChanged() { }
    #endregion ... end of NETPRICE property ...

    #region ... GROSSPRICE property ...
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private int __grossprice;
    public int GROSSPRICE
    {
      get => __grossprice;
      set
      {
        OnGROSSPRICEChanging(value);
        SetPropertyValue(nameof(GROSSPRICE), ref __grossprice, value);
        OnGROSSPRICEChanged();
      }
    }
    private void OnGROSSPRICEChanging(int value) { }
    private void OnGROSSPRICEChanged() { }
    #endregion ... end of GROSSPRICE property ...

    #region ... VATRATE_ID property ...
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private int __vatrate_id;
    public int VATRATE_ID
    {
      get => __vatrate_id;
      set
      {
        OnVATRATE_IDChanging(value);
        SetPropertyValue(nameof(VATRATE_ID), ref __vatrate_id, value);
        OnVATRATE_IDChanged();
      }
    }
    private void OnVATRATE_IDChanging(int value) { }
    private void OnVATRATE_IDChanged() { }
    #endregion ... end of VATRATE_ID property ...

    #region ... SACTIVE property ...
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string __sactive;
    public string SACTIVE
    {
      get => __sactive;
      set
      {
        OnSACTIVEChanging(value);
        SetPropertyValue(nameof(SACTIVE), ref __sactive, value);
        OnSACTIVEChanged();
      }
    }
    private void OnSACTIVEChanging(string value) { }
    private void OnSACTIVEChanged() { }
    #endregion ... end of SACTIVE property ...
  }

  // ---------------------------------------------------------------------------------
  //                              SERVICESTable rész
  // ---------------------------------------------------------------------------------
  public partial class SERVICESTable
  {
    private readonly string selectSQL =
      "SELECT id, name, description, netprice, grossprice, vatrate_id, sactive FROM services";

    private readonly string insSQL =
      "INSERT INTO services (id, name, description, netprice, grossprice, vatrate_id, sactive) " +
      "VALUES ({0}, '{1}', '{2}', {3}, {4}, {5}, '{6}')";

    private readonly string delSQL =
      "DELETE FROM services WHERE id = {0}";

    private readonly string updSQL =
      "UPDATE services SET name='{0}', description='{1}', netprice={2}, grossprice={3}, " +
      "vatrate_id={4}, sactive='{5}' WHERE id={6}";

    private readonly string selGenSQL =
      "SELECT GEN_ID(GEN_SERVICES_ID,1) FROM RDB$DATABASE";

    private ObservableCollection<SERVICES> __innerList;

    public ObservableCollection<SERVICES> GetList(FBConnectX conn)
    {
      if (__innerList != null)
        return __innerList;

      __innerList = TableBaseClass.GetListBase<SERVICES>(selectSQL, conn);
      return __innerList;
    }

    private int GetGenerator(FBConnectX conn) => DBFunc.Get_Generator(selGenSQL, conn);

    public SERVICES Newservices_M()
    {
      var rec = new SERVICES
      {
        ID = -1,
        NAME = "",
        DESCRIPTION = "",
        NETPRICE = 0,
        GROSSPRICE = 0,
        VATRATE_ID = 0,
        SACTIVE = "I" // Alapértelmezett: aktív
      };
      __innerList.Add(rec);
      return rec;
    }

    public void DelNewservices_M()
    {
      var _actrec = __innerList.FirstOrDefault(r => r.ID == -1);
      if (_actrec != null)
        __innerList.Remove(_actrec);
    }

    public void ReUpdateservices_M(SERVICES oldservices)
    {
      var _actrec = __innerList.FirstOrDefault(r => r.ID == oldservices.ID);
      if (_actrec == null) return;

      _actrec.NAME = oldservices.NAME;
      _actrec.DESCRIPTION = oldservices.DESCRIPTION;
      _actrec.NETPRICE = oldservices.NETPRICE;
      _actrec.GROSSPRICE = oldservices.GROSSPRICE;
      _actrec.VATRATE_ID = oldservices.VATRATE_ID;
      _actrec.SACTIVE = oldservices.SACTIVE;
    }

    public void Insert(SERVICES src, FBConnectX conn)
    {
      var _id = GetGenerator(conn);
      var _inssql = string.Format(insSQL,
          _id,
          src.NAME,
          src.DESCRIPTION,
          src.NETPRICE,
          src.GROSSPRICE,
          src.VATRATE_ID,
          src.SACTIVE);

      conn?.InsertSQL(_inssql);

      var _actrec = __innerList.FirstOrDefault(r => r.ID == -1);
      if (_actrec != null)
        src.ID = _id;
    }

    public void Update(SERVICES src, FBConnectX conn)
    {
      var _actrec = __innerList.FirstOrDefault(r => r.ID == src.ID);
      if (_actrec != null)
      {
        var _updsql = string.Format(updSQL,
            src.NAME,
            src.DESCRIPTION,
            src.NETPRICE,
            src.GROSSPRICE,
            src.VATRATE_ID,
            src.SACTIVE,
            src.ID);

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
