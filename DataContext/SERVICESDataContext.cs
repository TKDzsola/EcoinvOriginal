using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Ecoinv.BL;
using Ecoinv.Common;
using Ecoinv.Components;
using Ecoinv.Forms;

namespace Ecoinv.DataContext
{
  public class SERVICESDataContext : DataContextBase
  {
    public SERVICESDataContext()
    {
      alkTable = new SERVICESTable();
      SERVICESList = alkTable.GetList(FBConnX);
    }

    private readonly SERVICESTable alkTable;

    #region ... SERVICESList ObservableCollection<SERVICES> property ...

    private ObservableCollection<SERVICES> __SERVICESList = new ObservableCollection<SERVICES>();

    public ObservableCollection<SERVICES> SERVICESList
    {
      get => __SERVICESList;
      set => SetPropertyValue(nameof(SERVICESList), ref __SERVICESList, value);
    }

    #endregion ... end of SERVICESList ObservableCollection<SERVICES> property ...

    #region ... SelectedSERVICES property ...

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private SERVICES __selectedSERVICES;

    public SERVICES SelectedSERVICES
    {
      get => __selectedSERVICES;
      set
      {
        OnSelectedSERVICESChanging(value);
        SetPropertyValue(nameof(SelectedSERVICES), ref __selectedSERVICES, value);
        OnSelectedSERVICESChanged();
      }
    }

    /*partial*/
    private void OnSelectedSERVICESChanging(SERVICES value)
    {
    }

    /*partial*/
    private void OnSelectedSERVICESChanged()
    {
      OLDSERVICES ??= new SERVICES();

      if (SelectedSERVICES != null)
      {
        OLDSERVICES.ID = SelectedSERVICES.ID;
        OLDSERVICES.NAME = SelectedSERVICES.NAME;
        OLDSERVICES.DESCRIPTION = SelectedSERVICES.DESCRIPTION;
        OLDSERVICES.NETPRICE = SelectedSERVICES.NETPRICE;        
        OLDSERVICES.GROSSPRICE = SelectedSERVICES.GROSSPRICE;
        OLDSERVICES.VATRATE_ID = SelectedSERVICES.VATRATE_ID;
      }
    }

    #endregion ... end of SelectedSERVICES property ...

    #region ... OLDSERVICES property ...

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private SERVICES __oldSERVICES;

    public SERVICES OLDSERVICES
    {
      get => __oldSERVICES;
      set => SetPropertyValue(nameof(OLDSERVICES), ref __oldSERVICES, value);
    }

    #endregion ... end of OLDSERVICES property ...



    #region ... CommandModifyCancel property ...

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ICommand __commandModifyCancel;

    public ICommand CommandModifyCancel => __commandModifyCancel ??= new DelegateCommand(ac => ModifyCancelExecute(), fc => ModifyCancelCanExecute());

    private bool ModifyCancelCanExecute() => ((SelectedSERVICES != null) && (IsAdmin));

    private void ModifyCancelExecute()
    {
      if (IsEditing)
      {
        if (IsNewRecord)
        {
          // C A N C E L - gombot nyomott az új rekod felvitele után
          alkTable.DelNewservices_M();
          var _actrec = SERVICESList.FirstOrDefault(r => r.ID == -1); // ezért -1, mert GetNewCIKKEK-be ezzel kerül bele
          if (SERVICESList.IndexOf(_actrec) != -1)
            SERVICESList.Remove(_actrec);

          IsNewRecord = false;
        }
        else
        {
          // C A N C E L - gombot nyomott a módosítás után
          if (SelectedSERVICES != null)
            alkTable.ReUpdateservices_M(OLDSERVICES);
        }
      }
      else
      {
        // M O D I F Y - gombot megnyomta. Nincs teendő!
        if (SelectedSERVICES != null)
        {
        }
      }

      IsEditing = !IsEditing;
      ShowDetailPanel();
    }

    #endregion ... end of CommandModifyCancel property ...

    #region ... CommandNewSave property ...

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ICommand __commandNewSave;

    public ICommand CommandNewSave => __commandNewSave ??= new DelegateCommand(ac => NewSaveExecute(), fc => NewSaveCanExecute());

    private bool NewSaveCanExecute()
    {
      if (IsEditing)
      {
        if (IsNewRecord)
        {
          // I N S E R T
          return !string.IsNullOrEmpty(SelectedSERVICES.NAME) &&
                 !string.IsNullOrEmpty(SelectedSERVICES.DESCRIPTION) &&
                 !string.IsNullOrEmpty(SelectedSERVICES.NETPRICE.ToString()) &&
                 !string.IsNullOrEmpty(SelectedSERVICES.GROSSPRICE.ToString()) &&
                 !string.IsNullOrEmpty(SelectedSERVICES.VATRATE_ID.ToString());
        }
        else
        {
          // U P D A T E
          return (SelectedSERVICES != null) &&
                 !string.IsNullOrEmpty(SelectedSERVICES.NAME) &&
                 !string.IsNullOrEmpty(SelectedSERVICES.DESCRIPTION) &&
                 !string.IsNullOrEmpty(SelectedSERVICES.NETPRICE.ToString()) &&
                 !string.IsNullOrEmpty(SelectedSERVICES.GROSSPRICE.ToString()) &&
                 !string.IsNullOrEmpty(SelectedSERVICES.VATRATE_ID.ToString());
        }
      }
      else
      {
        return IsAdmin;
      }
    }

    private void NewSaveExecute()
    {
      IsEditing = !IsEditing;
      if (IsEditing)
      {
        // N E W
        IsNewRecord = true;

        SelectedSERVICES = alkTable.Newservices_M();
      }
      else
      {
        // S A V E
        var md5f = new MD5Func();
        if (IsNewRecord)
        {
          // Mentés a new gomb megnyomása után
          if (SelectedSERVICES != null)
            alkTable.Insert(SelectedSERVICES, FBConnX);

          IsNewRecord = false;
        }
        else
        {
          // Mentés a modify gomb megnyomása után
          if (SelectedSERVICES != null)
            alkTable.Update(SelectedSERVICES, FBConnX);
        }

      }

      ShowDetailPanel();
    }

    #endregion ... end of CommandNewSave property ...

    #region ... CommandDelete property ...

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ICommand __commandDelete;


    public ICommand CommandDelete => __commandDelete ??= new DelegateCommand(ac => DeleteExecute(), fc => DeleteCanExecute());

    private bool DeleteCanExecute() => (IsAdmin) && (SelectedSERVICES != null) && (!IsEditing);

    private void DeleteExecute()
    {
      MessageBoxResult dres = MessageBox.Show("Biztos a törlésben?", "Törlés", MessageBoxButton.YesNo);
      if (MessageBoxResult.Yes == dres)
      {
        if (SelectedSERVICES != null)
          alkTable.Delete(SelectedSERVICES.ID, FBConnX);

        ShowDetailPanel();
      }
    }

    #endregion ... end of CommandDelete property ...


  }
}
