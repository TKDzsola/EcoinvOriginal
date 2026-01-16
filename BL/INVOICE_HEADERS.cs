using Ecoinv.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ecoinv.BL
{
    // ========================================================================
    // ENUMOK – KOMPATIBILITÁS MIATT
    // ========================================================================
    public enum InvoiceStatus
    {
        InProgress = 0,
        Normal = 1,
        Storno = 2
    }

    public enum PaymentStatus
    {
        Unpaid = 0,
        Paid = 1
    }

    // ========================================================================
    // MODEL
    // ========================================================================
    public partial class INVOICE_HEADERS : TableBaseClass
    {
        private int __id;
        public int ID
        {
            get => __id;
            set => SetPropertyValue(nameof(ID), ref __id, value);
        }

        private int __client_id;
        public int CLIENT_ID
        {
            get => __client_id;
            set => SetPropertyValue(nameof(CLIENT_ID), ref __client_id, value);
        }

        private string __invoice_number;
        public string INVOICE_NUMBER
        {
            get => __invoice_number;
            set => SetPropertyValue(nameof(INVOICE_NUMBER), ref __invoice_number, value);
        }

        private DateTime? __issue_date;
        public DateTime? ISSUE_DATE
        {
            get => __issue_date;
            set => SetPropertyValue(nameof(ISSUE_DATE), ref __issue_date, value);
        }

        private DateTime? __due_date;
        public DateTime? DUE_DATE
        {
            get => __due_date;
            set => SetPropertyValue(nameof(DUE_DATE), ref __due_date, value);
        }

        private string __payment_method;
        public string PAYMENT_METHOD
        {
            get => __payment_method;
            set => SetPropertyValue(nameof(PAYMENT_METHOD), ref __payment_method, value);
        }

        private DateTime? __created;
        public DateTime? CREATED
        {
            get => __created;
            set => SetPropertyValue(nameof(CREATED), ref __created, value);
        }

        private string __szlastat;
        public string SZLASTAT
        {
            get => __szlastat;
            set => SetPropertyValue(nameof(SZLASTAT), ref __szlastat, value);
        }

        private string __fizstat;
        public string FIZSTAT
        {
            get => __fizstat;
            set => SetPropertyValue(nameof(FIZSTAT), ref __fizstat, value);
        }

        private string __storno_id;
        public string STORNO_ID
        {
            get => __storno_id;
            set => SetPropertyValue(nameof(STORNO_ID), ref __storno_id, value);
        }

        private string __client_name;
        public string CLIENT_NAME
        {
            get => __client_name;
            set => SetPropertyValue(nameof(CLIENT_NAME), ref __client_name, value);
        }

        public object PrimaryKeyValue => ID;
    }

    // ========================================================================
    // TABLE ACCESS
    // ========================================================================
    public partial class INVOICE_HEADERSTable
    {
        private readonly string selectSQL =
            "SELECT h.ID, h.CLIENT_ID, h.INVOICE_NUMBER, h.ISSUE_DATE, h.DUE_DATE, " +
            "h.PAYMENT_METHOD, h.CREATED, h.SZLASTAT, h.FIZSTAT, h.STORNO_ID, " +
            "c.NAME AS CLIENT_NAME " +
            "FROM INVOICE_HEADERS h " +
            "LEFT JOIN CLIENTS c ON c.ID = h.CLIENT_ID";

        private readonly string genSQL =
            "SELECT GEN_ID(GEN_INVOICE_HEADERS_ID, 1) FROM RDB$DATABASE";

        private ObservableCollection<INVOICE_HEADERS> __innerList;

        // --------------------------------------------------------------------
        public ObservableCollection<INVOICE_HEADERS> GetList(FBConnectX conn)
        {
            if (__innerList == null)
                __innerList = TableBaseClass.GetListBase<INVOICE_HEADERS>(selectSQL, conn);

            return __innerList;
        }

        // --------------------------------------------------------------------
        public void Insert(INVOICE_HEADERS src, FBConnectX conn)
        {
            if (src == null)
                throw new ArgumentNullException(nameof(src));

            int newId = DBFunc.Get_Generator(genSQL, conn);

            string sql =
                "INSERT INTO INVOICE_HEADERS (" +
                "ID, CLIENT_ID, INVOICE_NUMBER, ISSUE_DATE, DUE_DATE, PAYMENT_METHOD, CREATED, " +
                "SZLASTAT, FIZSTAT, STORNO_ID) VALUES (" +
                $"{newId}, " +
                $"{src.CLIENT_ID}, " +
                $"'{src.INVOICE_NUMBER}', " +
                $"{(src.ISSUE_DATE.HasValue ? $"'{src.ISSUE_DATE:yyyy-MM-dd}'" : "NULL")}, " +
                $"{(src.DUE_DATE.HasValue ? $"'{src.DUE_DATE:yyyy-MM-dd}'" : "NULL")}, " +
                $"'{src.PAYMENT_METHOD}', " +
                $"'{src.CREATED:yyyy-MM-dd HH:mm:ss}', " +
                $"'{src.SZLASTAT}', " +
                $"'{src.FIZSTAT}', " +
                $"'{src.STORNO_ID}')";

            conn.InsertSQL(sql);

            src.ID = newId;
            __innerList?.Add(src);
        }

        // --------------------------------------------------------------------
        public void SetStorno(int id, FBConnectX conn)
        {
            conn.UpdateSQL(
                $"UPDATE INVOICE_HEADERS SET SZLASTAT = '2', STORNO_ID = '{id}' WHERE ID = {id}");

            var rec = __innerList?.FirstOrDefault(x => x.ID == id);
            if (rec != null)
            {
                rec.SZLASTAT = "2";
                rec.STORNO_ID = id.ToString();
            }
        }

        // --------------------------------------------------------------------
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
                sql += $" AND h.ISSUE_DATE <= '{toDate:yyyy-MM-dd}'";

            if (!string.IsNullOrWhiteSpace(statusCode))
                sql += $" AND h.SZLASTAT = '{statusCode}'";

            sql += " ORDER BY h.ISSUE_DATE DESC";

            return TableBaseClass.GetListBase<INVOICE_HEADERS>(sql, conn).ToList();
        }
    }
}
