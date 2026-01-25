using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Ecoinv.Common;
using System.Globalization;
using FirebirdSql.Data.FirebirdClient; // Erre szükség lehet a kivétel kezeléséhez

namespace Ecoinv.BL
{
    public partial class INVOICE_DETAILS : TableBaseClass
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private int __id;
        public int ID { get => __id; set { SetPropertyValue(nameof(ID), ref __id, value); } }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private int __invoiceheaders_id;
        public int INVOICEHEADERS_ID { get => __invoiceheaders_id; set { SetPropertyValue(nameof(INVOICEHEADERS_ID), ref __invoiceheaders_id, value); } }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private int __services_id;
        public int SERVICES_ID { get => __services_id; set { SetPropertyValue(nameof(SERVICES_ID), ref __services_id, value); } }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private int __vatrate_id;
        public int VATRATE_ID { get => __vatrate_id; set { SetPropertyValue(nameof(VATRATE_ID), ref __vatrate_id, value); } }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private string __service_name;
        public string SERVICE_NAME { get => __service_name; set => SetPropertyValue(nameof(SERVICE_NAME), ref __service_name, value); }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private decimal __qty = 1;
        public decimal QTY { get => __qty; set { SetPropertyValue(nameof(QTY), ref __qty, value); Recalculate(); } }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private decimal __net_unit_price;
        public decimal NET_UNIT_PRICE { get => __net_unit_price; set { SetPropertyValue(nameof(NET_UNIT_PRICE), ref __net_unit_price, value); Recalculate(); } }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private decimal __vat_percent;
        public decimal VAT_PERCENT { get => __vat_percent; set { SetPropertyValue(nameof(VAT_PERCENT), ref __vat_percent, value); Recalculate(); } }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private decimal __line_total_net;
        public decimal LINE_TOTAL_NET { get => __line_total_net; set => SetPropertyValue(nameof(LINE_TOTAL_NET), ref __line_total_net, value); }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private decimal __vat_amount;
        public decimal VAT_AMOUNT { get => __vat_amount; set => SetPropertyValue(nameof(VAT_AMOUNT), ref __vat_amount, value); }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private decimal __line_total_gross;
        public decimal LINE_TOTAL_GROSS { get => __line_total_gross; set => SetPropertyValue(nameof(LINE_TOTAL_GROSS), ref __line_total_gross, value); }

        private void Recalculate()
        {
            LINE_TOTAL_NET = QTY * NET_UNIT_PRICE;
            VAT_AMOUNT = LINE_TOTAL_NET * VAT_PERCENT / 100m;
            LINE_TOTAL_GROSS = LINE_TOTAL_NET + VAT_AMOUNT;
        }

        public object PrimaryKeyValue => ID;
    }

    public partial class INVOICE_DETAILSTable
    {
        // SQL parancsok
        private readonly string selectSQL = "SELECT ID, INVOICEHEADERS_ID, SERVICES_ID, VATRATE_ID, QTY, NET_UNIT_PRICE, VAT_PERCENT, LINE_TOTAL_NET, VAT_AMOUNT, LINE_TOTAL_GROSS, SERVICE_NAME FROM INVOICE_DETAILS";

        // INSERT parancs a SERVICE_NAME mezővel
        private readonly string insSQL = "INSERT INTO INVOICE_DETAILS (ID, INVOICEHEADERS_ID, SERVICES_ID, VATRATE_ID, QTY, NET_UNIT_PRICE, VAT_PERCENT, LINE_TOTAL_NET, VAT_AMOUNT, LINE_TOTAL_GROSS, SERVICE_NAME) VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, '{10}')";

        private readonly string delSQL = "DELETE FROM INVOICE_DETAILS WHERE ID = {0}";
        private readonly string selGenSQL = "SELECT GEN_ID(GEN_INVOICE_DETAILS_ID, 1) FROM RDB$DATABASE";

        // Ez a parancs hozza létre az oszlopot, ha hiányzik
        private readonly string addColSQL = "ALTER TABLE INVOICE_DETAILS ADD SERVICE_NAME VARCHAR(255)";

        private ObservableCollection<INVOICE_DETAILS> __innerList;

        public ObservableCollection<INVOICE_DETAILS> GetList(FBConnectX conn)
        {
            if (__innerList != null) return __innerList;

            try
            {
                __innerList = TableBaseClass.GetListBase<INVOICE_DETAILS>(selectSQL, conn);
            }
            catch (Exception ex)
            {
                // Ha lekérdezéskor hiányzik az oszlop, akkor is létre kell hozni, 
                // különben a lista betöltése sem fog működni a régi számláknál.
                if (ex.Message.Contains("Column unknown") || ex.Message.Contains("SERVICE_NAME"))
                {
                    conn.UpdateSQL(addColSQL); // Oszlop létrehozása
                    // Újrapróbáljuk a lekérdezést
                    __innerList = TableBaseClass.GetListBase<INVOICE_DETAILS>(selectSQL, conn);
                }
                else
                {
                    throw; // Ha más hiba van, dobjuk tovább
                }
            }
            return __innerList;
        }

        private int GetGenerator(FBConnectX conn) => DBFunc.Get_Generator(selGenSQL, conn);

        public void Insert(INVOICE_DETAILS src, FBConnectX conn)
        {
            var id = GetGenerator(conn);

            // Az SQL parancs összeállítása
            var sql = string.Format(CultureInfo.InvariantCulture, insSQL,
                id,
                src.INVOICEHEADERS_ID,
                src.SERVICES_ID,
                src.VATRATE_ID,
                src.QTY,
                src.NET_UNIT_PRICE,
                src.VAT_PERCENT,
                src.LINE_TOTAL_NET,
                src.VAT_AMOUNT,
                src.LINE_TOTAL_GROSS,
                src.SERVICE_NAME ?? "" // Ha üres, ne legyen null
            );

            try
            {
                conn?.InsertSQL(sql);
            }
            catch (Exception ex)
            {
                // --- ITT AZ ÖNGYÓGYÍTÁS! ---
                // Ha a hiba oka, hogy hiányzik az oszlop:
                if (ex.Message.Contains("Column unknown") || ex.Message.Contains("SERVICE_NAME"))
                {
                    // 1. Létrehozzuk az oszlopot
                    conn.UpdateSQL(addColSQL);

                    // 2. Újra megpróbáljuk a beszúrást (most már mennie kell)
                    conn.InsertSQL(sql);
                }
                else
                {
                    throw; // Ha más a baj, szóljon
                }
            }

            src.ID = id;
        }

        public void Delete(int id, FBConnectX conn)
        {
            var rec = __innerList?.FirstOrDefault(x => x.ID == id);
            if (rec != null) __innerList.Remove(rec);
            conn?.DeleteSQL(string.Format(delSQL, id));
        }

        public void DeleteByHeaderId(int headerId, FBConnectX conn)
        {
            string sql = string.Format("DELETE FROM INVOICE_DETAILS WHERE INVOICEHEADERS_ID = {0}", headerId);
            conn?.DeleteSQL(sql);
        }

        public void CopyItems(int originalInvoiceId, int newInvoiceId, FBConnectX conn)
        {
            var allItems = GetList(conn);
            var itemsToCopy = allItems.Where(x => x.INVOICEHEADERS_ID == originalInvoiceId).ToList();

            foreach (var item in itemsToCopy)
            {
                var newItem = new INVOICE_DETAILS
                {
                    INVOICEHEADERS_ID = newInvoiceId,
                    SERVICES_ID = item.SERVICES_ID,
                    VATRATE_ID = item.VATRATE_ID,
                    QTY = item.QTY,
                    NET_UNIT_PRICE = item.NET_UNIT_PRICE,
                    VAT_PERCENT = item.VAT_PERCENT,
                    LINE_TOTAL_NET = item.LINE_TOTAL_NET,
                    VAT_AMOUNT = item.VAT_AMOUNT,
                    LINE_TOTAL_GROSS = item.LINE_TOTAL_GROSS,
                    SERVICE_NAME = item.SERVICE_NAME
                };
                Insert(newItem, conn);
            }
        }
    }
}