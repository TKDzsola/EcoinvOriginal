using Ecoinv.Common;
using FirebirdSql.Data.FirebirdClient;
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

        public void SetStorno(int id, FBConnectX conn)
        {
            conn.UpdateSQL(string.Format(updStornoSQL, id));
        }

        public int InsertStorno(INVOICE_HEADERS original, FBConnectX conn)
        {
            var newId = GetGenerator(conn);

            string newInvoiceNumber = "ST-" + original.INVOICE_NUMBER;
            if (newInvoiceNumber.Length > 20) newInvoiceNumber = newInvoiceNumber.Substring(0, 20);

            string today = DateTime.Now.ToString("yyyy-MM-dd");
            string created = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            var sql = string.Format(insSQL,
                newId,
                original.CLIENT_ID,
                newInvoiceNumber,
                today,
                today,
                created,
                original.PAYMENT_METHOD,
                "2",
                "1",
                original.ID
            );

            conn?.InsertSQL(sql);
            return newId;
        }

        // JAVÍTOTT METÓDUS: Paraméterezett lekérdezés a dátumhiba ellen
        public List<INVOICE_HEADERS> SearchInvoices(FBConnectX conn, string clientName, string invoiceNumber, DateTime? fromDate, DateTime? toDate, string statusCode)
        {
            string sql = selectSQL + " WHERE 1=1 ";
            List<FbParameter> parameters = new List<FbParameter>();

            if (!string.IsNullOrWhiteSpace(clientName))
            {
                sql += " AND c.NAME CONTAINING @clientName";
                parameters.Add(new FbParameter("@clientName", clientName));
            }

            if (!string.IsNullOrWhiteSpace(invoiceNumber))
            {
                sql += " AND h.INVOICE_NUMBER CONTAINING @invoiceNumber";
                parameters.Add(new FbParameter("@invoiceNumber", invoiceNumber));
            }

            if (fromDate.HasValue)
            {
                sql += " AND h.ISSUE_DATE >= @fromDate";
                parameters.Add(new FbParameter("@fromDate", fromDate.Value.Date));
            }

            if (toDate.HasValue)
            {
                sql += " AND h.ISSUE_DATE <= @toDate";
                // A nap végét pontosan számoljuk ki: 23:59:59
                DateTime endOfDay = toDate.Value.Date.AddDays(1).AddSeconds(-1);
                parameters.Add(new FbParameter("@toDate", endOfDay));
            }

            if (!string.IsNullOrWhiteSpace(statusCode))
            {
                sql += " AND h.SZLASTAT = @statusCode";
                parameters.Add(new FbParameter("@statusCode", statusCode));
            }

            sql += " ORDER BY h.ID DESC";

            // A TableBaseClass.GetListBase-nek átadjuk a paramétertömböt is
            var rawList = TableBaseClass.GetListBase<INVOICE_HEADERS>(sql, conn, parameters.ToArray());
            return rawList.ToList();
        }
    }
}