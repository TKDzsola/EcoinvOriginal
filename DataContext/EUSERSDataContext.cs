using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Ecoinv.BL;
using Ecoinv.Common;
using Ecoinv.Components;

namespace Ecoinv.DataContext
{
    public class EUSERSDataContext : DataContextBase
    {
        private readonly EUSERSTable alkTable;

        public EUSERSDataContext()
        {
            alkTable = new EUSERSTable();
            EUSERSList = new ObservableCollection<EUSERS>();

            try
            {
                RefreshData();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Users init hiba");
            }
        }

        private void RefreshData()
        {
            try
            {
                alkTable.InvalidateCache();
                DatabaseHelper.Execute(conn =>
                {
                    EUSERSList.Clear();
                    var list = alkTable.GetList(conn);
                    foreach (var item in list) EUSERSList.Add(item);
                }, "Felhasználók betöltése sikertelen");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Felhasználók betöltése sikertelen");
            }
        }

        public ObservableCollection<EUSERS> EUSERSList { get; private set; }

        private EUSERS _selectedEUSERS;
        public EUSERS SelectedEUSERS
        {
            get => _selectedEUSERS;
            set
            {
                if (SetPropertyValue(nameof(SelectedEUSERS), ref _selectedEUSERS, value))
                {
                    IsEditing = false;
                }
            }
        }

        // --- PARANCSOK (cache-elve ??= operátorral) ---

        private ICommand _commandNew;
        public ICommand CommandNew => _commandNew ??= new DelegateCommand(_ => DoNew(), _ => IsAdmin);

        private ICommand _commandSave;
        public ICommand CommandSave => _commandSave ??= new DelegateCommand(_ => DoSave(), _ => IsEditing && IsAdmin);

        private ICommand _commandDelete;
        public ICommand CommandDelete => _commandDelete ??= new DelegateCommand(_ => DoDelete(), _ => SelectedEUSERS != null && IsAdmin && !IsEditing);

        private ICommand _commandModify;
        public ICommand CommandModify => _commandModify ??= new DelegateCommand(_ => IsEditing = true, _ => SelectedEUSERS != null && IsAdmin);

        private void DoNew()
        {
            SelectedEUSERS = new EUSERS { UACTIVE = "I", ISADMIN = "N" };
            IsEditing = true;
        }

        private void DoSave()
        {
            if (string.IsNullOrWhiteSpace(SelectedEUSERS.UNAME))
            {
                MessageBox.Show("Felhasználónév kötelező!");
                return;
            }
            if (SelectedEUSERS.ID <= 0 && string.IsNullOrEmpty(SelectedEUSERS.UPSSW))
            {
                MessageBox.Show("Új felhasználónál a jelszó kötelező!");
                return;
            }

            try
            {
                DatabaseHelper.Execute(conn =>
                {
                    alkTable.Save(SelectedEUSERS, conn);
                }, "User mentési hiba");

                MessageBox.Show("Sikeres mentés!");
                IsEditing = false;
                RefreshData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hiba: " + ex.Message);
            }
        }

        private void DoDelete()
        {
            if (MessageBox.Show("Biztosan törölni szeretnéd?", "Törlés", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    DatabaseHelper.Execute(conn =>
                    {
                        alkTable.Delete(SelectedEUSERS, conn);
                    }, "User törlési hiba");

                    RefreshData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Hiba: " + ex.Message);
                }
            }
        }
    }
}