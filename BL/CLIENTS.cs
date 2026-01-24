using Ecoinv.Common;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Ecoinv.BL
{
    public partial class CLIENTS : TableBaseClass
    {
        // --- ADATMEZŐK (A KÉPED ALAPJÁN PONTOSÍTVA) ---

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int __id;
        public int ID
        {
            get => __id;
            set => SetPropertyValue(nameof(ID), ref __id, value);
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string __name;
        public string NAME
        {
            get => __name;
            set => SetPropertyValue(nameof(NAME), ref __name, value);
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string __tax_number;
        public string TAX_NUMBER
        {
            get => __tax_number;
            set => SetPropertyValue(nameof(TAX_NUMBER), ref __tax_number, value);
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string __phone;
        public string PHONE
        {
            get => __phone;
            set => SetPropertyValue(nameof(PHONE), ref __phone, value);
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string __email;
        public string EMAIL
        {
            get => __email;
            set => SetPropertyValue(nameof(EMAIL), ref __email, value);
        }

        // ÚJ: A képen látható Bankszámlaszám
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string __bankaccount;
        public string BANKACCOUNT
        {
            get => __bankaccount;
            set => SetPropertyValue(nameof(BANKACCOUNT), ref __bankaccount, value);
        }

        // ÚJ: A képen látható Közösségi adószám
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string __comtax_number;
        public string COMTAX_NUMBER
        {
            get => __comtax_number;
            set => SetPropertyValue(nameof(COMTAX_NUMBER), ref __comtax_number, value);
        }

        // Apa kérése: Aktív státusz (CACTIVE)
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string __cactive = "1";
        public string CACTIVE
        {
            get => __cactive;
            set => SetPropertyValue(nameof(CACTIVE), ref __cactive, value);
        }

        public object PrimaryKeyValue => ID;
    }

    public partial class CLIENTSTable
    {
        // SQL: Minden mezőt felsorolunk, ami a képen van
        private readonly string selectSQL = "SELECT ID, NAME, TAX_NUMBER, PHONE, EMAIL, BANKACCOUNT, CACTIVE, COMTAX_NUMBER FROM CLIENTS ORDER BY NAME";

        private readonly string insSQL =
            "INSERT INTO CLIENTS (ID, NAME, TAX_NUMBER, PHONE, EMAIL, BANKACCOUNT, CACTIVE, COMTAX_NUMBER) " +
            "VALUES ({0}, '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')";

        private readonly string updSQL =
            "UPDATE CLIENTS SET NAME='{1}', TAX_NUMBER='{2}', PHONE='{3}', EMAIL='{4}', BANKACCOUNT='{5}', CACTIVE='{6}', COMTAX_NUMBER='{7}' WHERE ID={0}";

        private readonly string delSQL = "DELETE FROM CLIENTS WHERE ID={0}";

        private readonly string selGenSQL = "SELECT GEN_ID(GEN_CLIENTS_ID, 1) FROM RDB$DATABASE";

        private ObservableCollection<CLIENTS> __innerList;

        public ObservableCollection<CLIENTS> GetList(FBConnectX conn)
        {
            if (__innerList != null) return __innerList;
            __innerList = TableBaseClass.GetListBase<CLIENTS>(selectSQL, conn);
            return __innerList;
        }

        private int GetGenerator(FBConnectX conn) => DBFunc.Get_Generator(selGenSQL, conn);

        // EZ A METÓDUS VÉGZI A MENTÉST (Insert és Update egyben)
        public void Save(CLIENTS item, FBConnectX conn)
        {
            string active = string.IsNullOrEmpty(item.CACTIVE) ? "1" : item.CACTIVE;

            // BIZTONSÁG: Aposztrófok kezelése (hogy az O'Neil ne okozzon hibát)
            string safeName = item.NAME?.Replace("'", "''") ?? "";
            string safeTax = item.TAX_NUMBER?.Replace("'", "''") ?? "";
            string safePhone = item.PHONE?.Replace("'", "''") ?? "";
            string safeEmail = item.EMAIL?.Replace("'", "''") ?? "";
            string safeBank = item.BANKACCOUNT?.Replace("'", "''") ?? "";
            string safeComTax = item.COMTAX_NUMBER?.Replace("'", "''") ?? "";

            if (item.ID <= 0) // Új felvétel
            {
                item.ID = GetGenerator(conn);
                conn.InsertSQL(string.Format(insSQL,
                    item.ID, safeName, safeTax, safePhone, safeEmail, safeBank, active, safeComTax));

                if (__innerList != null && !__innerList.Contains(item))
                    __innerList.Add(item);
            }
            else // Módosítás
            {
                conn.UpdateSQL(string.Format(updSQL,
                    item.ID, safeName, safeTax, safePhone, safeEmail, safeBank, active, safeComTax));
            }
        }

        public void Delete(CLIENTS item, FBConnectX conn)
        {
            conn.DeleteSQL(string.Format(delSQL, item.ID));
            if (__innerList != null) __innerList.Remove(item);
        }
    }
}