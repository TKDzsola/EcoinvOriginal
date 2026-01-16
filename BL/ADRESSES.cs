using Ecoinv.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecoinv.BL.Enums
{
  public partial class ADRESSES : TableBaseClass
  {
    public ADRESSES() 
    {
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

    #region ... CLIENT_ID property ...

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private int __client_id;
    public int CLIENT_ID
    {
      get => __client_id;
      set
      {
        OnCLIENT_IDChanging(value);
        SetPropertyValue(nameof(CLIENT_ID), ref __client_id, value);
        OnCLIENT_IDChanged();
      }
    }
    private void OnCLIENT_IDChanging(int value) { }
    private void OnCLIENT_IDChanged() { }

    #endregion ... end of CLIENT_ID property ...

    #region ... CITY property ...

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string __city;
    public string CITY
    {
      get => __city;
      set
      {
        OnCITYChanging(value);
        SetPropertyValue(nameof(CITY), ref __city, value);
        OnCITYChanged();
      }
    }
    private void OnCITYChanging(string value) { }
    private void OnCITYChanged() { }

    #endregion ... end of CITY property ...

    #region ... ADDRESSES property ...

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string __address;
    public string ADDRESS
    {
      get => __address;
      set
      {
        OnADDRESSESChanging(value);
        SetPropertyValue(nameof(ADDRESS), ref __address, value);
        OnADDRESSESChanged();
      }
    }
    private void OnADDRESSESChanging(string value) { }
    private void OnADDRESSESChanged() { }

    #endregion ... end of ADDRESSES property ...

    #region ... ATYPE property ...

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string __atype;
    public string ATYPE
    {
      get => __atype;
      set
      {
        OnATYPEChanging(value);
        SetPropertyValue(nameof(ATYPE), ref __atype, value);
        OnATYPEChanged();
      }
    }
    private void OnATYPEChanging(string value) { }
    private void OnATYPEChanged() { }

    #endregion ... end of ATYPE property ...

    #region ... AACTIVE property ...

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string __aactive;
    public string AACTIVE
    {
      get => __aactive;
      set
      {
        OnAACTIVEChanging(value);
        SetPropertyValue(nameof(AACTIVE), ref __aactive, value);
        OnAACTIVEChanged();
      }
    }
    private void OnAACTIVEChanging(string value) { }
    private void OnAACTIVEChanged() { }

    #endregion ... end of AACTIVE property ...

    #region ... POSTALCODE property ...

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private int __postalcode;
    public int POSTALCODE
    {
      get => __postalcode;
      set
      {
        OnPOSTALCODEChanging(value);
        SetPropertyValue(nameof(POSTALCODE), ref __postalcode, value);
        OnPOSTALCODEChanged();
      }
    }
    private void OnPOSTALCODEChanging(int value) { }
    private void OnPOSTALCODEChanged() { }

    #endregion ... end of POSTALCODE property ...

    public object PrimaryKeyValue => ID;
  }
  public partial class ADRESSESTable
  {
    private readonly string selectSQL = "select id, client_id, city, address, atype, aactive, postalcode from adresses";
    private readonly string selectClntAdrSQL = "select id, client_id, city, address, atype, aactive, postalcode from adresses where client_id={0} and atype='L'";
    private readonly string insSQL = "insert into adresses (city, address, atype, aactive, postalcode) values ({0},'{1}','{2}','{3}','{4}')";
    private readonly string delSQL = "delete from adresses where id = {0}";
    private readonly string updSQL = "update adresses set city='{0}', address='{1}', atype='{2}', aactive={3}  where id = {3}";
    private readonly string selGenSQL = "select gen_id(GEN_adresses_ID,1) from rdb$database";

    private ObservableCollection<ADRESSES> __innerList;

    public ObservableCollection<ADRESSES> GetList(FBConnectX conn)
    {
      if (__innerList != null)
        return __innerList;

      __innerList = TableBaseClass.GetListBase<ADRESSES>(selectSQL, conn);

      return __innerList;
    }

    public ObservableCollection<ADRESSES> GetList(FBConnectX conn, int ClntId)
    {
      //if (__innerList != null)
        //return __innerList;

      __innerList = TableBaseClass.GetListBase<ADRESSES>(string.Format(selectClntAdrSQL, ClntId), conn);

      return __innerList;
    }



    private int GetGenerator(FBConnectX conn) => DBFunc.Get_Generator(selGenSQL, conn);

    public ADRESSES NewADRESSES_M()
    {
      var rec = new ADRESSES
      {
        ID = -1,
        CLIENT_ID = -1,
        CITY = "",
        ADDRESS = "",
        ATYPE = "",
        AACTIVE = "",
        POSTALCODE = -1,
      };
      __innerList.Add(rec);
      return rec;
    }

    public void DelNewADRESSES_M()
    {
      var _actrec = __innerList.FirstOrDefault(r => r.ID == -1); // ezért -1, mert GetNewHOTALK-ba ezzel kerül bele
      if (__innerList.IndexOf(_actrec) != -1)
        __innerList.Remove(_actrec);
    }

    public void ReUpdateADRESSES_M(ADRESSES oldadresses)
    {
      // Visszaírás CANCEL gomb megnyomása után
      var _actrec = __innerList.FirstOrDefault(r => r.ID == oldadresses.ID); // ezért -1, mert GetNewHOTALK-ba ezzel kerül bele
      if (_actrec == null)
        return;

      _actrec.ID = oldadresses.ID;
      _actrec.CLIENT_ID = oldadresses.CLIENT_ID;
      _actrec.CITY = oldadresses.CITY;
      _actrec.ADDRESS = oldadresses.ADDRESS;
      _actrec.ATYPE = oldadresses.ATYPE;
      _actrec.AACTIVE = oldadresses.AACTIVE;
      _actrec.POSTALCODE = oldadresses.POSTALCODE;
    }

    public void Insert(ADRESSES src, FBConnectX conn)
    {
      var _id = GetGenerator(conn);
      var _inssql = string.Format(insSQL, _id, src.ID, src.CLIENT_ID, src.CITY, src.ADDRESS, src.ATYPE, src.AACTIVE, src.POSTALCODE);
      conn?.InsertSQL(_inssql);
      var _actrec = __innerList.FirstOrDefault(r => r.ID == -1);  // ezért -1, mert GetNewHOTALK-ba ezzel kerül bele
      if (__innerList.IndexOf(_actrec) != -1)
        src.ID = _id;
    }

    public void Update(ADRESSES src, FBConnectX conn)
    {
      var _actrec = __innerList.FirstOrDefault(r => r.ID == src.ID);
      if (__innerList.IndexOf(_actrec) != -1)
      {
        var _updsql = string.Format(updSQL, src.ID, src.CLIENT_ID, src.CITY, src.ADDRESS, src.ATYPE, src.AACTIVE, src.POSTALCODE);
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
