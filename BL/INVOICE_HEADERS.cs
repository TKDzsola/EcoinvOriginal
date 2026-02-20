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

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private decimal __paid_amount;
        public decimal PAID_AMOUNT
        {
            get => __paid_amount;
            set
            {
                decimal roundedValue = Math.Round(value, 2);
                if (SetPropertyValue(nameof(PAID_AMOUNT), ref __paid_amount, roundedValue))
                    OnPropertyChanged(nameof(DEBT_AMOUNT));
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private decimal __total_gross;
        public decimal TOTAL_GROSS
        {
            get => __total_gross;
            set
            {
                decimal roundedValue = Math.Round(value, 2);
                if (SetPropertyValue(nameof(TOTAL_GROSS), ref __total_gross, roundedValue))
                    OnPropertyChanged(nameof(DEBT_AMOUNT));
            }
        }

        public decimal DEBT_AMOUNT => Math.Round(TOTAL_GROSS - PAID_AMOUNT, 2);

        public object PrimaryKeyValue => ID;
    }

    public partial class INVOICE_HEADERSTable
    {
        private readonly string selectSQL = "SELECT h.*, c.NAME as CLIENT_NAME FROM INVOICE_HEADERS h LEFT JOIN CLIENTS c ON h.CLIENT_ID = c.ID";
        private readonly string insSQL = "INSERT INTO INVOICE_HEADERS (ID, CLIENT_ID, INVOICE_NUMBER, ISSUE_DATE, DUE_DATE, CREATED, PAYMENT_METHOD, SZLASTAT, FIZSTAT, STORNO_ID, PAID_AMOUNT, TOTAL_GROSS) VALUES ({0}, {1}, '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', {10}, {11})";
        private readonly string updPaidSQL = "UPDATE INVOICE_HEADERS SET PAID_AMOUNT = {0} WHERE ID = {1}";
        private readonly string updStornoSQL = "UPDATE INVOICE_HEADERS SET SZLASTAT = '2' WHERE ID = {0}";
        private readonly string selGenSQL = "SELECT GEN_ID(GEN_INVOICE_HEADERS_ID, 1) FROM RDB$DATABASE";

        public ObservableCollection<INVOICE_HEADERS> GetList(FBConnectX conn)
        {
            return TableBaseClass.GetListBase<INVOICE_HEADERS>(selectSQL, conn);
        }

        public void UpdatePaidAmount(int id, decimal paidAmount, FBConnectX conn)
        {
            string sql = string.Format(System.Globalization.CultureInfo.InvariantCulture, updPaidSQL, Math.Round(paidAmount, 2), id);
            conn?.UpdateSQL(sql);
        }

        public void Insert(INVOICE_HEADERS src, FBConnectX conn)
        {
            var id = DBFunc.Get_Generator(selGenSQL, conn);
            string issue = src.ISSUE_DATE.HasValue ? src.ISSUE_DATE.Value.ToString("yyyy-MM-dd") : "NULL";
            string due = src.DUE_DATE.HasValue ? src.DUE_DATE.Value.ToString("yyyy-MM-dd") : "NULL";

            var sql = string.Format(System.Globalization.CultureInfo.InvariantCulture, insSQL,
                id, src.CLIENT_ID, src.INVOICE_NUMBER, issue, due, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                src.PAYMENT_METHOD ?? "Transfer", src.SZLASTAT ?? "1", src.FIZSTAT ?? "0", src.STORNO_ID ?? "0", Math.Round(src.PAID_AMOUNT, 2), Math.Round(src.TOTAL_GROSS, 2));

            conn?.InsertSQL(sql);
            src.ID = id;
        }

        public void SetStorno(int id, FBConnectX conn) => conn.UpdateSQL(string.Format(updStornoSQL, id));

        public int InsertStorno(INVOICE_HEADERS original, FBConnectX conn)
        {
            var newId = DBFunc.Get_Generator(selGenSQL, conn);
            string newNum = ("ST-" + original.INVOICE_NUMBER);
            if (newNum.Length > 32) newNum = newNum.Substring(0, 32);

            var sql = string.Format(System.Globalization.CultureInfo.InvariantCulture, insSQL,
                newId, original.CLIENT_ID, newNum, DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("yyyy-MM-dd"),
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), original.PAYMENT_METHOD, "2", "1", original.ID, 0, Math.Round(original.TOTAL_GROSS * -1, 2));

            conn?.InsertSQL(sql);
            return newId;
        }

        public void Delete(int id, FBConnectX conn)
        {
            new INVOICE_DETAILSTable().DeleteByHeaderId(id, conn);
            conn?.DeleteSQL(string.Format("DELETE FROM INVOICE_HEADERS WHERE ID = {0}", id));
        }

        public List<INVOICE_HEADERS> SearchInvoices(FBConnectX conn, string clientName, string invoiceNumber, DateTime? fromDate, DateTime? toDate, string statusCode)
        {
            string sql = selectSQL + " WHERE 1=1 ";
            List<FbParameter> parameters = new List<FbParameter>();

            if (!string.IsNullOrWhiteSpace(clientName)) { sql += " AND c.NAME CONTAINING @cname"; parameters.Add(new FbParameter("@cname", clientName)); }
            if (!string.IsNullOrWhiteSpace(invoiceNumber)) { sql += " AND h.INVOICE_NUMBER CONTAINING @inum"; parameters.Add(new FbParameter("@inum", invoiceNumber)); }
            if (fromDate.HasValue) { sql += " AND h.ISSUE_DATE >= @fdate"; parameters.Add(new FbParameter("@fdate", fromDate.Value.Date)); }
            if (toDate.HasValue) { sql += " AND h.ISSUE_DATE <= @tdate"; parameters.Add(new FbParameter("@tdate", toDate.Value.Date.AddDays(1).AddSeconds(-1))); }
            if (!string.IsNullOrWhiteSpace(statusCode)) { sql += " AND h.SZLASTAT = @stat"; parameters.Add(new FbParameter("@stat", statusCode)); }

            sql += " ORDER BY h.ID DESC";
            return TableBaseClass.GetListBase<INVOICE_HEADERS>(sql, conn, parameters.ToArray()).ToList();
        }
    }
}