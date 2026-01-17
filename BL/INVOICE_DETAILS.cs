using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Ecoinv.Common;
using System.Globalization;

namespace Ecoinv.BL
{
    // ======================================================================
    // 1. OSZTÁLY: AZ ADATMODELL (INVOICE_DETAILS)
    // ======================================================================
    public partial class INVOICE_DETAILS : TableBaseClass
    {
        // --- ID (PK) ---
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

        // --- INVOICEHEADERS_ID (FK) ---
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int __invoiceheaders_id;
        public int INVOICEHEADERS_ID
        {
            get => __invoiceheaders_id;
            set
            {
                OnINVOICEHEADERS_IDChanging(value);
                SetPropertyValue(nameof(INVOICEHEADERS_ID), ref __invoiceheaders_id, value);
                OnINVOICEHEADERS_IDChanged();
            }
        }
        private void OnINVOICEHEADERS_IDChanging(int value) { }
        private void OnINVOICEHEADERS_IDChanged() { }

        // --- SERVICES_ID (FK) ---
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int __services_id;
        public int SERVICES_ID
        {
            get => __services_id;
            set
            {
                OnSERVICES_IDChanging(value);
                SetPropertyValue(nameof(SERVICES_ID), ref __services_id, value);
                OnSERVICES_IDChanged();
            }
        }
        private void OnSERVICES_IDChanging(int value) { }
        private void OnSERVICES_IDChanged() { }

        // --- VATRATE_ID (FK) ---
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

        // ==========================================================
        // ÚJ MEZŐK (MENNYISÉG, ÁR, ÖSSZEGEK)
        // ==========================================================

        // Szolgáltatás neve (Nem tárolt adat, csak megjelenítés)
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string __service_name;
        public string SERVICE_NAME
        {
            get => __service_name;
            set => SetPropertyValue(nameof(SERVICE_NAME), ref __service_name, value);
        }

        // QTY (Mennyiség)
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private decimal __qty = 1;
        public decimal QTY
        {
            get => __qty;
            set
            {
                SetPropertyValue(nameof(QTY), ref __qty, value);
                Recalculate();
            }
        }

        // NET_UNIT_PRICE (Nettó egységár)
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private decimal __net_unit_price;
        public decimal NET_UNIT_PRICE
        {
            get => __net_unit_price;
            set
            {
                SetPropertyValue(nameof(NET_UNIT_PRICE), ref __net_unit_price, value);
                Recalculate();
            }
        }

        // VAT_PERCENT (ÁFA %)
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private decimal __vat_percent;
        public decimal VAT_PERCENT
        {
            get => __vat_percent;
            set
            {
                SetPropertyValue(nameof(VAT_PERCENT), ref __vat_percent, value);
                Recalculate();
            }
        }

        // LINE_TOTAL_NET (Nettó érték)
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private decimal __line_total_net;
        public decimal LINE_TOTAL_NET
        {
            get => __line_total_net;
            set => SetPropertyValue(nameof(LINE_TOTAL_NET), ref __line_total_net, value);
        }

        // VAT_AMOUNT (ÁFA érték)
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private decimal __vat_amount;
        public decimal VAT_AMOUNT
        {
            get => __vat_amount;
            set => SetPropertyValue(nameof(VAT_AMOUNT), ref __vat_amount, value);
        }

        // LINE_TOTAL_GROSS (Bruttó érték)
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private decimal __line_total_gross;
        public decimal LINE_TOTAL_GROSS
        {
            get => __line_total_gross;
            set => SetPropertyValue(nameof(LINE_TOTAL_GROSS), ref __line_total_gross, value);
        }

        // SZÁMOLÁS METÓDUS (Ezt hiányolta a fordító)
        private void Recalculate()
        {
            LINE_TOTAL_NET = QTY * NET_UNIT_PRICE;
            VAT_AMOUNT = LINE_TOTAL_NET * VAT_PERCENT / 100m;
            LINE_TOTAL_GROSS = LINE_TOTAL_NET + VAT_AMOUNT;
        }

        public object PrimaryKeyValue => ID;

    } // <--- ITT ÉR VÉGET AZ INVOICE_DETAILS OSZTÁLY!


    // ======================================================================
    // 2. OSZTÁLY: AZ ADATBÁZIS KEZELŐ (INVOICE_DETAILSTable)
    // ======================================================================
    public partial class INVOICE_DETAILSTable
    {
        private readonly string selectSQL =
            "SELECT ID, INVOICEHEADERS_ID, SERVICES_ID, VATRATE_ID, " +
            "QTY, NET_UNIT_PRICE, VAT_PERCENT, LINE_TOTAL_NET, VAT_AMOUNT, LINE_TOTAL_GROSS " +
            "FROM INVOICE_DETAILS";

        private readonly string insSQL =
            "INSERT INTO INVOICE_DETAILS (" +
            "ID, INVOICEHEADERS_ID, SERVICES_ID, VATRATE_ID, " +
            "QTY, NET_UNIT_PRICE, VAT_PERCENT, LINE_TOTAL_NET, VAT_AMOUNT, LINE_TOTAL_GROSS) " +
            "VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9})";

        private readonly string delSQL = "DELETE FROM INVOICE_DETAILS WHERE ID = {0}";

        private readonly string selGenSQL = "SELECT GEN_ID(GEN_INVOICE_DETAILS_ID, 1) FROM RDB$DATABASE";

        private ObservableCollection<INVOICE_DETAILS> __innerList;

        public ObservableCollection<INVOICE_DETAILS> GetList(FBConnectX conn)
        {
            if (__innerList != null)
                return __innerList;

            __innerList = TableBaseClass.GetListBase<INVOICE_DETAILS>(selectSQL, conn);
            return __innerList;
        }

        private int GetGenerator(FBConnectX conn) => DBFunc.Get_Generator(selGenSQL, conn);

        public void Insert(INVOICE_DETAILS src, FBConnectX conn)
        {
            var id = GetGenerator(conn);

            // InvariantCulture a tizedespontok miatt
            var sql = string.Format(CultureInfo.InvariantCulture,
                insSQL,
                id,
                src.INVOICEHEADERS_ID,
                src.SERVICES_ID,
                src.VATRATE_ID,
                src.QTY,
                src.NET_UNIT_PRICE,
                src.VAT_PERCENT,
                src.LINE_TOTAL_NET,
                src.VAT_AMOUNT,
                src.LINE_TOTAL_GROSS
            );

            conn?.InsertSQL(sql);
            src.ID = id;
        }

        public void Delete(int id, FBConnectX conn)
        {
            var rec = __innerList?.FirstOrDefault(x => x.ID == id);
            if (rec != null)
                __innerList.Remove(rec);

            conn?.DeleteSQL(string.Format(delSQL, id));
        }

    } // <--- ITT ÉR VÉGET AZ INVOICE_DETAILSTable OSZTÁLY!

} // <--- ITT ÉR VÉGET A NAMESPACE!