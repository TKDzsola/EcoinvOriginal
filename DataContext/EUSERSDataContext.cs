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
    public class EUSERSDataContext : DataContextBase
    {
        public EUSERSDataContext()
        {
            alkTable = new EUSERSTable();
            EUSERSList = alkTable.GetList(FBConnX);
        }

        private readonly EUSERSTable alkTable;

        #region ... EUSERSList ObservableCollection<EUSERS> property ...

        private ObservableCollection<EUSERS> __eusersList = new ObservableCollection<EUSERS>();

        public ObservableCollection<EUSERS> EUSERSList
        {
            get => __eusersList;
            set => SetPropertyValue(nameof(EUSERSList), ref __eusersList, value);
        }

        #endregion ... end of EUSERSList ObservableCollection<EUSERS> property ...

        #region ... SelectedEUSERS property ...

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private EUSERS __selectedEUSERS;

        public EUSERS SelectedEUSERS
        {
            get => __selectedEUSERS;
            set
            {
                OnSelectedEUSERSChanging(value);
                SetPropertyValue(nameof(SelectedEUSERS), ref __selectedEUSERS, value);
                OnSelectedEUSERSChanged();
            }
        }

        /*partial*/
        private void OnSelectedEUSERSChanging(EUSERS value)
        {
        }

        /*partial*/
        private void OnSelectedEUSERSChanged()
        {
            OLDEUSERS ??= new EUSERS();

            if (SelectedEUSERS != null)
            {
                OLDEUSERS.ID = SelectedEUSERS.ID;
                OLDEUSERS.UNAME = SelectedEUSERS.UNAME;
                OLDEUSERS.UPSSW = SelectedEUSERS.UPSSW;
                OLDEUSERS.FULNAME = SelectedEUSERS.FULNAME;
                // ez kalkulált: OLDHOTALK.LOGPSW = SelectedHOTALK.LOGPSW;
                OLDEUSERS.JELSZO_NO_MD5 = SelectedEUSERS.JELSZO_NO_MD5;
                OLDEUSERS.UACTIVE = SelectedEUSERS.UACTIVE;
            }
        }

        #endregion ... end of SelectedEUSERS property ...

        #region ... OLDEUSERS property ...

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private EUSERS __oldeusers;

        public EUSERS OLDEUSERS
        {
            get => __oldeusers;
            set => SetPropertyValue(nameof(OLDEUSERS), ref __oldeusers, value);
        }

        #endregion ... end of OLDEUSERS property ...

        #region ... CommandModifyCancel property ...

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ICommand __commandModifyCancel;

        public ICommand CommandModifyCancel => __commandModifyCancel ??= new DelegateCommand(ac => ModifyCancelExecute(), fc => ModifyCancelCanExecute());

        private bool ModifyCancelCanExecute() => ((SelectedEUSERS != null) && (IsAdmin || SelectedEUSERS.UNAME == LoginUserName));

        private void ModifyCancelExecute()
        {
            if (IsEditing)
            {
                if (IsNewRecord)
                {
                    // C A N C E L - gombot nyomott az új rekod felvitele után
                    alkTable.DelNewEUSERS_M();
                    var _actrec = EUSERSList.FirstOrDefault(r => r.ID == -1); // ezért -1, mert GetNewCIKKEK-be ezzel kerül bele
                    if (EUSERSList.IndexOf(_actrec) != -1)
                        EUSERSList.Remove(_actrec);

                    IsNewRecord = false;
                }
                else
                {
                    // C A N C E L - gombot nyomott a módosítás után
                    if (SelectedEUSERS != null)
                        alkTable.ReUpdateEUSERS_M(OLDEUSERS);
                }
            }
            else
            {
                // M O D I F Y - gombot megnyomta. Nincs teendő!
                if (SelectedEUSERS != null)
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
                    return !string.IsNullOrEmpty(SelectedEUSERS.UNAME) &&
                           !string.IsNullOrEmpty(SelectedEUSERS.UPSSW) &&
                           !string.IsNullOrEmpty(SelectedEUSERS.JELSZO_NO_MD5);
                }
                else
                {
                    // U P D A T E
                    return (SelectedEUSERS != null) &&
                           !string.IsNullOrEmpty(SelectedEUSERS.UPSSW) &&
                           !string.IsNullOrEmpty(SelectedEUSERS.JELSZO_NO_MD5);                         
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

                SelectedEUSERS = alkTable.NewEUSERS_M();
            }
            else
            {
                // S A V E
                var md5f = new MD5Func();
                if (IsNewRecord)
                {
                    // Mentés a new gomb megnyomása után
                    if (SelectedEUSERS != null)
                        alkTable.Insert(SelectedEUSERS, FBConnX);

                    IsNewRecord = false;
                }
                else
                {
                    // Mentés a modify gomb megnyomása után
                    if (SelectedEUSERS != null)
                        alkTable.Update(SelectedEUSERS, FBConnX);
                }

            }

            ShowDetailPanel();
        }

        #endregion ... end of CommandNewSave property ...

        #region ... CommandDelete property ...

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ICommand __commandDelete;


        public ICommand CommandDelete => __commandDelete ??= new DelegateCommand(ac => DeleteExecute(), fc => DeleteCanExecute());

        private bool DeleteCanExecute() => (IsAdmin) && (SelectedEUSERS != null) && (!IsEditing);

        private void DeleteExecute()
        {
            MessageBoxResult dres = MessageBox.Show("Biztos a törlésben?", "Törlés", MessageBoxButton.YesNo);
            if (MessageBoxResult.Yes == dres)
            {
                if (SelectedEUSERS != null)
                    alkTable.Delete(SelectedEUSERS.ID, FBConnX);

                ShowDetailPanel();
            }
        }

        #endregion ... end of CommandDelete property ...
    }
}
