using Ecoinv.Common;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Ecoinv.BL
{
    public partial class CLIENTS : TableBaseClass
    {
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

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string __bankaccount;
        public string BANKACCOUNT
        {
            get => __bankaccount;
            set => SetPropertyValue(nameof(BANKACCOUNT), ref __bankaccount, value);
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string __comtax_number;
        public string COMTAX_NUMBER
        {
            get => __comtax_number;
            set => SetPropertyValue(nameof(COMTAX_NUMBER), ref __comtax_number, value);
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string __cactive = "I";
        public string CACTIVE
        {
            get => __cactive;
            set => SetPropertyValue(nameof(CACTIVE), ref __cactive, value);
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string __client_type;
        public string CLIENT_TYPE
        {
            get => __client_type;
            set => SetPropertyValue(nameof(CLIENT_TYPE), ref __client_type, value);
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string __internal_note;
        public string INTERNAL_NOTE
        {
            get => __internal_note;
            set => SetPropertyValue(nameof(INTERNAL_NOTE), ref __internal_note, value);
        }

        public object PrimaryKeyValue => ID;
    }

    public partial class CLIENTSTable
    {
        private readonly string selectSQL = "SELECT ID, NAME, TAX_NUMBER, PHONE, EMAIL, BANKACCOUNT, CACTIVE, COMTAX_NUMBER, CLIENT_TYPE, INTERNAL_NOTE FROM CLIENTS ORDER BY NAME";

        private readonly string insSQL =
            "INSERT INTO CLIENTS (ID, NAME, TAX_NUMBER, PHONE, EMAIL, BANKACCOUNT, CACTIVE, COMTAX_NUMBER, CLIENT_TYPE, INTERNAL_NOTE) " +
            "VALUES ({0}, '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}')";

        private readonly string updSQL =
            "UPDATE CLIENTS SET NAME='{1}', TAX_NUMBER='{2}', PHONE='{3}', EMAIL='{4}', BANKACCOUNT='{5}', CACTIVE='{6}', COMTAX_NUMBER='{7}', CLIENT_TYPE='{8}', INTERNAL_NOTE='{9}' WHERE ID={0}";

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

        public void Save(CLIENTS item, FBConnectX conn)
        {
            string active = string.IsNullOrEmpty(item.CACTIVE) ? "I" : item.CACTIVE;

            string safeName = item.NAME?.Replace("'", "''") ?? "";
            string safeTax = item.TAX_NUMBER?.Replace("'", "''") ?? "";
            string safePhone = item.PHONE?.Replace("'", "''") ?? "";
            string safeEmail = item.EMAIL?.Replace("'", "''") ?? "";
            string safeBank = item.BANKACCOUNT?.Replace("'", "''") ?? "";
            string safeComTax = item.COMTAX_NUMBER?.Replace("'", "''") ?? "";
            string safeInternalNote = item.INTERNAL_NOTE?.Replace("'", "''") ?? "";

            string safeType = item.CLIENT_TYPE?.Replace("'", "''") ?? "";

            if (item.ID <= 0)
            {
                item.ID = GetGenerator(conn);
                conn.InsertSQL(string.Format(insSQL,
                    item.ID, safeName, safeTax, safePhone, safeEmail, safeBank, active, safeComTax, safeType, safeInternalNote));

                if (__innerList != null && !__innerList.Any(x => x.ID == item.ID))
                    __innerList.Add(item);
            }
            else
            {
                conn.UpdateSQL(string.Format(updSQL,
                    item.ID, safeName, safeTax, safePhone, safeEmail, safeBank, active, safeComTax, safeType, safeInternalNote));
            }
        }

        public void Delete(CLIENTS item, FBConnectX conn)
        {
            if (item == null) return;
            conn.DeleteSQL(string.Format(delSQL, item.ID));
            if (__innerList != null) __innerList.Remove(item);
        }
    }
}