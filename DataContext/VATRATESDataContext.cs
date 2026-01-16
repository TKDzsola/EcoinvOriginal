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
  public class VATRATESDataContext : DataContextBase
  {
    public VATRATESDataContext()
    {
      alkTable = new VATRATESTable();
      VATRATESList = alkTable.GetList(FBConnX);
    }

    private readonly VATRATESTable alkTable;

    #region ... VATRATESList ObservableCollection<VATRATES> property ...

    private ObservableCollection<VATRATES> __vatratesList = new ObservableCollection<VATRATES>();

    public ObservableCollection<VATRATES> VATRATESList
    {
      get => __vatratesList;
      set => SetPropertyValue(nameof(VATRATESList), ref __vatratesList, value);
    }

    #endregion ... end of VATRATESList ObservableCollection<VATRATES> property ...

    #region ... SelectedVATRATES property ...

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private VATRATES __selectedVATRATES;

    public VATRATES SelectedVATRATES
    {
      get => __selectedVATRATES;
      set
      {
        OnSelectedVATRATESChanging(value);
        SetPropertyValue(nameof(SelectedVATRATES), ref __selectedVATRATES, value);
        OnSelectedVATRATESChanged();
      }
    }

    /*partial*/
    private void OnSelectedVATRATESChanging(VATRATES value)
    {
    }

    /*partial*/
    private void OnSelectedVATRATESChanged()
    {
      OLDVATRATES ??= new VATRATES();

      if (SelectedVATRATES != null)
      {
        OLDVATRATES.ID = SelectedVATRATES.ID;
        OLDVATRATES.NAME = SelectedVATRATES.NAME;
        OLDVATRATES.RATES = SelectedVATRATES.RATES;
        OLDVATRATES.VACTIVE = SelectedVATRATES.VACTIVE;
      }
    }

    #endregion ... end of SelectedVATRATES property ...

    #region ... OLDVATRATES property ...

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private VATRATES __oldvatrates;

    public VATRATES OLDVATRATES
    {
      get => __oldvatrates;
      set => SetPropertyValue(nameof(OLDVATRATES), ref __oldvatrates, value);
    }

    #endregion ... end of OLDVATRATES property ...



    #region ... CommandModifyCancel property ...

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ICommand __commandModifyCancel;

    public ICommand CommandModifyCancel => __commandModifyCancel ??= new DelegateCommand(ac => ModifyCancelExecute(), fc => ModifyCancelCanExecute());

    private bool ModifyCancelCanExecute() => ((SelectedVATRATES != null) && (IsAdmin));

    private void ModifyCancelExecute()
    {
      if (IsEditing)
      {
        if (IsNewRecord)
        {
          // C A N C E L - gombot nyomott az új rekod felvitele után
          alkTable.DelNewVATRATES_M();
          var _actrec = VATRATESList.FirstOrDefault(r => r.ID == -1); // ezért -1, mert GetNewCIKKEK-be ezzel kerül bele
          if (VATRATESList.IndexOf(_actrec) != -1)
            VATRATESList.Remove(_actrec);

          IsNewRecord = false;
        }
        else
        {
          // C A N C E L - gombot nyomott a módosítás után
          if (SelectedVATRATES != null)
            alkTable.ReUpdateVATRATES_M(OLDVATRATES);
        }
      }
      else
      {
        // M O D I F Y - gombot megnyomta. Nincs teendő!
        if (SelectedVATRATES != null)
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
          return !string.IsNullOrEmpty(SelectedVATRATES.NAME) &&
                 !string.IsNullOrEmpty(SelectedVATRATES.RATES.ToString());

        }
        else
        {
          // U P D A T E
          return (SelectedVATRATES != null) &&
                 !string.IsNullOrEmpty(SelectedVATRATES.NAME) &&
                 !string.IsNullOrEmpty(SelectedVATRATES.RATES.ToString());
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

        SelectedVATRATES = alkTable.NewVATRATES_M();
      }
      else
      {
        // S A V E
        var md5f = new MD5Func();
        if (IsNewRecord)
        {
          // Mentés a new gomb megnyomása után
          if (SelectedVATRATES != null)
            alkTable.Insert(SelectedVATRATES, FBConnX);

          IsNewRecord = false;
        }
        else
        {
          // Mentés a modify gomb megnyomása után
          if (SelectedVATRATES != null)
            alkTable.Update(SelectedVATRATES, FBConnX);
        }

      }

      ShowDetailPanel();
    }

    #endregion ... end of CommandNewSave property ...

    #region ... CommandDelete property ...

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ICommand __commandDelete;


    public ICommand CommandDelete => __commandDelete ??= new DelegateCommand(ac => DeleteExecute(), fc => DeleteCanExecute());

    private bool DeleteCanExecute() => (IsAdmin) && (SelectedVATRATES != null) && (!IsEditing);

    private void DeleteExecute()
    {
      MessageBoxResult dres = MessageBox.Show("Biztos a törlésben?", "Törlés", MessageBoxButton.YesNo);
      if (MessageBoxResult.Yes == dres)
      {
        if (SelectedVATRATES != null)
          alkTable.Delete(SelectedVATRATES.ID, FBConnX);

        ShowDetailPanel();
      }
    }

    #endregion ... end of CommandDelete property ...


  }
}
