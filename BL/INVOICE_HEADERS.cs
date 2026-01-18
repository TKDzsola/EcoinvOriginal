using Ecoinv.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Ecoinv.BL
{
    public enum InvoiceStatus
    {
        InProgress = 0,
        Normal = 1,
        Storno = 2
    }

    public partial class INVOICE_HEADERS : TableBaseClass
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private int __id;
        public int ID { get => __id; set => SetPropertyValue(nameof(ID), ref __id, value); }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private int __client_id;
        public int CLIENT_ID { get => __client_id; set => SetPropertyValue(nameof(CLIENT_ID), ref __client_id, value); }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private string __invoice_number;
        public string INVOICE_NUMBER { get => __invoice_number; set => SetPropertyValue(nameof(INVOICE_NUMBER), ref __invoice_number, value); }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private DateTime? __issue_date;
        public DateTime? ISSUE_DATE { get => __issue_date; set => SetPropertyValue(nameof(ISSUE_DATE), ref __issue_date, value); }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private DateTime? __due_date;
        public DateTime? DUE_DATE { get => __due_date; set => SetPropertyValue(nameof(DUE_DATE), ref __due_date, value); }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private DateTime? __created;
        public DateTime? CREATED { get => __created; set => SetPropertyValue(nameof(CREATED), ref __created, value); }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private string __payment_method;
        public string PAYMENT_METHOD { get => __payment_method; set => SetPropertyValue(nameof(PAYMENT_METHOD), ref __payment_method, value); }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private string __szlastat;
        public string SZLASTAT { get => __szlastat; set => SetPropertyValue(nameof(SZLASTAT), ref __szlastat, value); }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private string __fizstat;
        public string FIZSTAT { get => __fizstat; set => SetPropertyValue(nameof(FIZSTAT), ref __fizstat, value); }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private string __storno_id;
        public string STORNO_ID { get => __storno_id; set => SetPropertyValue(nameof(STORNO_ID), ref __storno_id, value); }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private string __client_name;
        public string CLIENT_NAME { get => __client_name; set => SetPropertyValue(nameof(CLIENT_NAME), ref __client_name, value); }

        public object PrimaryKeyValue => ID;
    }

    public partial class INVOICE_HEADERSTable
    {
        private readonly string selectSQL = "SELECT h.*, c.NAME as CLIENT_NAME FROM INVOICE_HEADERS h LEFT JOIN CLIENTS c ON h.CLIENT_ID = c.ID";

        private readonly string insSQL = "INSERT INTO INVOICE_HEADERS (ID, CLIENT_ID, INVOICE_NUMBER, ISSUE_DATE, DUE_DATE, CREATED, PAYMENT_METHOD, SZLASTAT, FIZSTAT, STORNO_ID) VALUES ({0}, {1}, '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}')";

        // Ez már csak arra kell, hogy az EREDETI számlát megjelöljük, hogy "sztornózva lett"
        private readonly string updStornoSQL = "UPDATE INVOICE_HEADERS SET SZLASTAT = '2' WHERE ID = {0}";

        private readonly string selGenSQL = "SELECT GEN_ID(GEN_INVOICE_HEADERS_ID, 1) FROM RDB$DATABASE";

        private ObservableCollection<INVOICE_HEADERS> __innerList;

        public ObservableCollection<INVOICE_HEADERS> GetList(FBConnectX conn)
        {
            if (__innerList != null) return __innerList;
            __innerList = TableBaseClass.GetListBase<INVOICE_HEADERS>(selectSQL, conn);
            return __innerList;
        }

        private int GetGenerator(FBConnectX conn) => DBFunc.Get_Generator(selGenSQL, conn);

        public void Insert(INVOICE_HEADERS src, FBConnectX conn)
        {
            var id = GetGenerator(conn);
            string issue = src.ISSUE_DATE.HasValue ? src.ISSUE_DATE.Value.ToString("yyyy-MM-dd") : "NULL";
            string due = src.DUE_DATE.HasValue ? src.DUE_DATE.Value.ToString("yyyy-MM-dd") : "NULL";
            string created = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string payMethod = string.IsNullOrEmpty(src.PAYMENT_METHOD) ? "Transfer" : src.PAYMENT_METHOD;

            var sql = string.Format(insSQL, id, src.CLIENT_ID, src.INVOICE_NUMBER, issue, due, created, payMethod, src.SZLASTAT ?? "0", src.FIZSTAT ?? "0", src.STORNO_ID ?? "0");
            conn?.InsertSQL(sql);
            src.ID = id;
        }

        public void SetStornoStatus(int id, FBConnectX conn)
        {
            conn.UpdateSQL(string.Format(updStornoSQL, id));
        }

        // --- ÚJ: SZTORNÓ SZÁMLA LÉTREHOZÁSA (A régi alapján) ---
        public int InsertStorno(INVOICE_HEADERS original, FBConnectX conn)
        {
            var newId = GetGenerator(conn);

            // Új számlaszám: ST-EREDETISZÁM
            string newInvoiceNumber = "ST-" + original.INVOICE_NUMBER;
            if (newInvoiceNumber.Length > 20) newInvoiceNumber = newInvoiceNumber.Substring(0, 20); // Ha túl hosszú lenne

            // Sztornó számla dátumai: MAI NAP
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            string created = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // Státusz: 2 (Sztornó), STORNO_ID: Az eredeti ID-ja
            var sql = string.Format(insSQL,
                newId,
                original.CLIENT_ID,
                newInvoiceNumber,
                today, // Kelt: Ma
                today, // Fiz.hat: Ma
                created,
                original.PAYMENT_METHOD,
                "2", // SZLASTAT: Sztornó
                "1", // FIZSTAT: Fizetettnek tekintjük (technikai)
                original.ID // STORNO_ID: Hivatkozás az eredetire
            );

            conn?.InsertSQL(sql);
            return newId;
        }

        public List<INVOICE_HEADERS> SearchInvoices(FBConnectX conn, string clientName, string invoiceNumber, DateTime? fromDate, DateTime? toDate, string statusCode)
        {
            string sql = selectSQL + " WHERE 1=1 ";
            if (!string.IsNullOrWhiteSpace(clientName)) sql += $" AND c.NAME CONTAINING '{clientName}'";
            if (!string.IsNullOrWhiteSpace(invoiceNumber)) sql += $" AND h.INVOICE_NUMBER CONTAINING '{invoiceNumber}'";
            if (fromDate.HasValue) sql += $" AND h.ISSUE_DATE >= '{fromDate:yyyy-MM-dd}'";
            if (toDate.HasValue) sql += $" AND h.ISSUE_DATE <= '{toDate:yyyy-MM-dd 23:59:59}'";
            if (!string.IsNullOrWhiteSpace(statusCode)) sql += $" AND h.SZLASTAT = '{statusCode}'";
            sql += " ORDER BY h.ID DESC";
            return TableBaseClass.GetListBase<INVOICE_HEADERS>(sql, conn).ToList();
        }
    }
}