using Ecoinv.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics; // Fontos a DebuggerBrowsable miatt
using System.Linq;

namespace Ecoinv.BL
{
    // ========================================================================
    // ENUMOK (A hibaüzenet hiányolta)
    // ========================================================================
    public enum InvoiceStatus
    {
        InProgress = 0,
        Normal = 1,
        Storno = 2
    }

    // ========================================================================
    // MODEL OSZTÁLY
    // ========================================================================
    public partial class INVOICE_HEADERS : TableBaseClass
    {
        // --- ID ---
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int __id;
        public int ID
        {
            get => __id;
            set => SetPropertyValue(nameof(ID), ref __id, value);
        }

        // --- CLIENT_ID ---
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int __client_id;
        public int CLIENT_ID
        {
            get => __client_id;
            set => SetPropertyValue(nameof(CLIENT_ID), ref __client_id, value);
        }

        // --- INVOICE_NUMBER ---
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string __invoice_number;
        public string INVOICE_NUMBER
        {
            get => __invoice_number;
            set => SetPropertyValue(nameof(INVOICE_NUMBER), ref __invoice_number, value);
        }

        // --- ISSUE_DATE (Kelt) ---
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private DateTime? __issue_date;
        public DateTime? ISSUE_DATE
        {
            get => __issue_date;
            set => SetPropertyValue(nameof(ISSUE_DATE), ref __issue_date, value);
        }

        // --- DUE_DATE (Fizetési határidő) ---
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private DateTime? __due_date;
        public DateTime? DUE_DATE
        {
            get => __due_date;
            set => SetPropertyValue(nameof(DUE_DATE), ref __due_date, value);
        }

        // --- CREATED (Létrehozva - HIÁNYZOTT!) ---
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private DateTime? __created;
        public DateTime? CREATED
        {
            get => __created;
            set => SetPropertyValue(nameof(CREATED), ref __created, value);
        }

        // --- PAYMENT_METHOD (Fiz. mód - HIÁNYZOTT!) ---
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string __payment_method;
        public string PAYMENT_METHOD
        {
            get => __payment_method;
            set => SetPropertyValue(nameof(PAYMENT_METHOD), ref __payment_method, value);
        }

        // --- SZLASTAT (Számla státusz) ---
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string __szlastat;
        public string SZLASTAT
        {
            get => __szlastat;
            set => SetPropertyValue(nameof(SZLASTAT), ref __szlastat, value);
        }

        // --- FIZSTAT (Fizetve státusz - HIÁNYZOTT!) ---
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string __fizstat;
        public string FIZSTAT
        {
            get => __fizstat;
            set => SetPropertyValue(nameof(FIZSTAT), ref __fizstat, value);
        }

        // --- STORNO_ID ---
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string __storno_id;
        public string STORNO_ID
        {
            get => __storno_id;
            set => SetPropertyValue(nameof(STORNO_ID), ref __storno_id, value);
        }

        // --- CLIENT_NAME (Csak kereséshez/JOIN-hoz) ---
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string __client_name;
        public string CLIENT_NAME
        {
            get => __client_name;
            set => SetPropertyValue(nameof(CLIENT_NAME), ref __client_name, value);
        }

        public object PrimaryKeyValue => ID;
    }

    // ========================================================================
    // TÁBLA KEZELŐ OSZTÁLY
    // ========================================================================
    public partial class INVOICE_HEADERSTable
    {
        // LEFT JOIN a CLIENTS táblával a névkereséshez
        private readonly string selectSQL =
            "SELECT h.*, c.NAME as CLIENT_NAME " +
            "FROM INVOICE_HEADERS h " +
            "LEFT JOIN CLIENTS c ON h.CLIENT_ID = c.ID";

        // Paraméterek: ID, CLIENT_ID, INVOICE_NUMBER, ISSUE_DATE, DUE_DATE, CREATED, PAYMENT_METHOD, SZLASTAT, FIZSTAT, STORNO_ID
        private readonly string insSQL =
            "INSERT INTO INVOICE_HEADERS (ID, CLIENT_ID, INVOICE_NUMBER, ISSUE_DATE, DUE_DATE, CREATED, PAYMENT_METHOD, SZLASTAT, FIZSTAT, STORNO_ID) " +
            "VALUES ({0}, {1}, '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}')";

        private readonly string updStornoSQL =
            "UPDATE INVOICE_HEADERS SET SZLASTAT = '2', STORNO_ID = '{0}' WHERE ID = {0}";

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

            // Dátum formátumok (Firebird)
            string issue = src.ISSUE_DATE.HasValue ? src.ISSUE_DATE.Value.ToString("yyyy-MM-dd") : "NULL";
            string due = src.DUE_DATE.HasValue ? src.DUE_DATE.Value.ToString("yyyy-MM-dd") : "NULL";
            string created = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // Ha esetleg null lenne a fizetési mód
            string payMethod = string.IsNullOrEmpty(src.PAYMENT_METHOD) ? "Transfer" : src.PAYMENT_METHOD;

            var sql = string.Format(insSQL,
                id,
                src.CLIENT_ID,
                src.INVOICE_NUMBER,
                issue,
                due,
                created,
                payMethod,
                src.SZLASTAT ?? "0",
                src.FIZSTAT ?? "0",
                src.STORNO_ID ?? "0");

            conn?.InsertSQL(sql);
            src.ID = id;
        }

        public void SetStorno(int id, FBConnectX conn)
        {
            conn.UpdateSQL(string.Format(updStornoSQL, id));
        }

        // KERESÉS
        public List<INVOICE_HEADERS> SearchInvoices(
            FBConnectX conn,
            string clientName,
            string invoiceNumber,
            DateTime? fromDate,
            DateTime? toDate,
            string statusCode)
        {
            string sql = selectSQL + " WHERE 1=1 ";

            if (!string.IsNullOrWhiteSpace(clientName))
                sql += $" AND c.NAME CONTAINING '{clientName}'";

            if (!string.IsNullOrWhiteSpace(invoiceNumber))
                sql += $" AND h.INVOICE_NUMBER CONTAINING '{invoiceNumber}'";

            if (fromDate.HasValue)
                sql += $" AND h.ISSUE_DATE >= '{fromDate:yyyy-MM-dd}'";

            if (toDate.HasValue)
                sql += $" AND h.ISSUE_DATE <= '{toDate:yyyy-MM-dd 23:59:59}'";

            if (!string.IsNullOrWhiteSpace(statusCode))
                sql += $" AND h.SZLASTAT = '{statusCode}'";

            sql += " ORDER BY h.ID DESC";

            var rawList = TableBaseClass.GetListBase<INVOICE_HEADERS>(sql, conn);
            return rawList.ToList();
        }
    }
}