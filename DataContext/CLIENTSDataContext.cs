using Ecoinv.BL;
using Ecoinv.BL.Enums;
using Ecoinv.Common;
using Ecoinv.Components;
using Ecoinv.Forms;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Ecoinv.DataContext
{
    public class CLIENTSDataContext : DataContextBase
    {
        public CLIENTSDataContext()
        {
            aktTable = new CLIENTSTable();
            CLIENTSList = aktTable.GetList(FBConnX);

            aktcaddrtbl = new ADRESSESTable();
            CADRESSES = aktcaddrtbl.GetList(FBConnX, -1);

            CliAdr = new ADRESSES();
        }

        private readonly CLIENTSTable aktTable;
        private readonly ADRESSESTable aktcaddrtbl;

        #region ... CLIENTSList ObservableCollection<CLIENTS> property ...

        private ObservableCollection<CLIENTS> __clientsList = new ObservableCollection<CLIENTS>();

        public ObservableCollection<CLIENTS> CLIENTSList
        {
            get => __clientsList;
            set => SetPropertyValue(nameof(CLIENTSList), ref __clientsList, value);
        }

        #endregion ... end of CLIENTSList ObservableCollection<CLIENTS> property ...

        #region ... SelectedCLIENTS property ...

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private CLIENTS __selectedCLIENTS;

        public CLIENTS SelectedCLIENTS
        {
            get => __selectedCLIENTS;
            set
            {
                SetPropertyValue(nameof(SelectedCLIENTS), ref __selectedCLIENTS, value);

                if (SelectedCLIENTS != null)
                {
                    CADRESSES = aktcaddrtbl.GetList(FBConnX, SelectedCLIENTS.ID);
                    CliAdr = CADRESSES.FirstOrDefault();
                }
            }
        }

        #endregion ... end of SelectedCLIENTS property ...

        #region ... CliAdr property ...

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ADRESSES __cliAdr;

        public ADRESSES CliAdr
        {
            get => __cliAdr;
            set => SetPropertyValue(nameof(CliAdr), ref __cliAdr, value);
        }

        #endregion ... end of CliAdr property ...

        #region ... CADRESSES ObservableCollection<ADRESSES> property ...

        private ObservableCollection<ADRESSES> __cadresses = new ObservableCollection<ADRESSES>();

        public ObservableCollection<ADRESSES> CADRESSES
        {
            get => __cadresses;
            set => SetPropertyValue(nameof(CADRESSES), ref __cadresses, value);
        }

        #endregion ... end of CADRESSES ObservableCollection ...

        // =====================================================================
        // ⭐ SZÁMLÁZÁSHOZ KIVÁLASZTÁS – EZ AZ ÚJ RÉSZ ⭐
        // =====================================================================

        #region ... CommandSelectClientToInvoice ...

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ICommand __commandSelectClientToInvoice;

        public ICommand CommandSelectClientToInvoice =>
            __commandSelectClientToInvoice ??=
                new DelegateCommand(
                    _ => SelectClientToInvoiceExecute(),
                    _ => SelectedCLIENTS != null
                );

        private void SelectClientToInvoiceExecute()
        {
            // 🔴 EZ A LÉNYEG
            DataContextBase.SelectedClientForInvoice = SelectedCLIENTS.ID;

            // opcionális: bezárjuk az ablakot
            Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(w => w is CLIENTSFrm)
                ?.Close();
        }

        #endregion

    }
}
