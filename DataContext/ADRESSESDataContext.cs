using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Ecoinv.BL;
using Ecoinv.BL.Enums;
using Ecoinv.Common;
using Ecoinv.Components;
using Ecoinv.Forms;

namespace Ecoinv.DataContext
{
  public class ADRESSESDataContext : DataContextBase
  {
    public ADRESSESDataContext()
    {
      alkTable = new ADRESSESTable();
      ADRESSESList = alkTable.GetList(FBConnX);
    }

    private readonly ADRESSESTable alkTable;

    #region ... ADRESSESList ObservableCollection<ADRESSES> property ...

    private ObservableCollection<ADRESSES> __ADRESSESList = new ObservableCollection<ADRESSES>();

    public ObservableCollection<ADRESSES> ADRESSESList
    {
      get => __ADRESSESList;
      set => SetPropertyValue(nameof(ADRESSESList), ref __ADRESSESList, value);
    }

    #endregion ... end of ADRESSESList ObservableCollection<ADRESSES> property ...

    #region ... SelectedADRESSES property ...

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ADRESSES __selectedADRESSES;

    public ADRESSES SelectedADRESSES
    {
      get => __selectedADRESSES;
      set
      {
        OnSelectedADRESSESChanging(value);
        SetPropertyValue(nameof(SelectedADRESSES), ref __selectedADRESSES, value);
        OnSelectedADRESSESChanged();
      }
    }

    /*partial*/
    private void OnSelectedADRESSESChanging(ADRESSES value)
    {
    }

    /*partial*/
    private void OnSelectedADRESSESChanged()
    {
      OLDADRESSES ??= new ADRESSES();

      if (SelectedADRESSES != null)
      {
        OLDADRESSES.ID = SelectedADRESSES.ID;
        OLDADRESSES.CLIENT_ID = SelectedADRESSES.CLIENT_ID;
        OLDADRESSES.POSTALCODE = SelectedADRESSES.POSTALCODE;
        OLDADRESSES.CITY = SelectedADRESSES.CITY;
        OLDADRESSES.ADDRESS = SelectedADRESSES.ADDRESS;
        OLDADRESSES.ATYPE = SelectedADRESSES.ATYPE;
        OLDADRESSES.AACTIVE = SelectedADRESSES.AACTIVE;
      }
    }

    #endregion ... end of SelectedADRESSES property ...

    #region ... OLDADRESSES property ...

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ADRESSES __oldADRESSES;

    public ADRESSES OLDADRESSES
    {
      get => __oldADRESSES;
      set => SetPropertyValue(nameof(OLDADRESSES), ref __oldADRESSES, value);
    }

    #endregion ... end of OLDADRESSES property ...

    #region ... CommandModifyCancel property ...

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ICommand __commandModifyCancel;

    public ICommand CommandModifyCancel => __commandModifyCancel ??= new DelegateCommand(ac => ModifyCancelExecute(), fc => ModifyCancelCanExecute());

    private bool ModifyCancelCanExecute() => ((SelectedADRESSES != null) && (IsAdmin));

    private void ModifyCancelExecute()
    {
      if (IsEditing)
      {
        if (IsNewRecord)
        {
          // C A N C E L - gombot nyomott az új rekod felvitele után
          alkTable.DelNewADRESSES_M();
          var _actrec = ADRESSESList.FirstOrDefault(r => r.ID == -1); // ezért -1, mert GetNewCIKKEK-be ezzel kerül bele
          if (ADRESSESList.IndexOf(_actrec) != -1)
            ADRESSESList.Remove(_actrec);

          IsNewRecord = false;
        }
        else
        {
          // C A N C E L - gombot nyomott a módosítás után
          if (SelectedADRESSES != null)
            alkTable.ReUpdateADRESSES_M(OLDADRESSES);
        }
      }
      else
      {
        // M O D I F Y - gombot megnyomta. Nincs teendő!
        if (SelectedADRESSES != null)
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
          return !string.IsNullOrEmpty(SelectedADRESSES.POSTALCODE.ToString()) &&
                 !string.IsNullOrEmpty(SelectedADRESSES.CITY) &&
                 !string.IsNullOrEmpty(SelectedADRESSES.ADDRESS) &&
                 !string.IsNullOrEmpty(SelectedADRESSES.ATYPE) &&
                 !string.IsNullOrEmpty(SelectedADRESSES.AACTIVE);

        }
        else
        {
          // U P D A T E
          return (SelectedADRESSES != null) &&
                 !string.IsNullOrEmpty(SelectedADRESSES.CLIENT_ID.ToString());
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

        SelectedADRESSES = alkTable.NewADRESSES_M();
      }
      else
      {
        // S A V E
        var md5f = new MD5Func();
        if (IsNewRecord)
        {
          // Mentés a new gomb megnyomása után
          if (SelectedADRESSES != null)
            alkTable.Insert(SelectedADRESSES, FBConnX);

          IsNewRecord = false;
        }
        else
        {
          // Mentés a modify gomb megnyomása után
          if (SelectedADRESSES != null)
            alkTable.Update(SelectedADRESSES, FBConnX);
        }

      }

      ShowDetailPanel();
    }

    #endregion ... end of CommandNewSave property ...

    #region ... CommandDelete property ...

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ICommand __commandDelete;


    public ICommand CommandDelete => __commandDelete ??= new DelegateCommand(ac => DeleteExecute(), fc => DeleteCanExecute());

    private bool DeleteCanExecute() => (IsAdmin) && (SelectedADRESSES != null) && (!IsEditing);

    private void DeleteExecute()
    {
      MessageBoxResult dres = MessageBox.Show("Biztos a törlésben?", "Törlés", MessageBoxButton.YesNo);
      if (MessageBoxResult.Yes == dres)
      {
        if (SelectedADRESSES != null)
          alkTable.Delete(SelectedADRESSES.ID, FBConnX);

        ShowDetailPanel();
      }
    }

    #endregion ... end of CommandDelete property ...
  }
}
