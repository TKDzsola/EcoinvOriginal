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
        private string __email;
        public string EMAIL
        {
            get => __email;
            set => SetPropertyValue(nameof(EMAIL), ref __email, value);
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string __phone;
        public string PHONE
        {
            get => __phone;
            set => SetPropertyValue(nameof(PHONE), ref __phone, value);
        }

        // --- EZ HIÁNYZOTT AZ AKTÍV STÁTUSZHOZ ---
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
        // SQL BŐVÍTÉSE A CACTIVE MEZŐVEL
        private readonly string selectSQL = "SELECT ID, NAME, TAX_NUMBER, EMAIL, PHONE, CACTIVE FROM CLIENTS ORDER BY NAME";

        private readonly string insSQL =
            "INSERT INTO CLIENTS (ID, NAME, TAX_NUMBER, EMAIL, PHONE, CACTIVE) VALUES ({0}, '{1}', '{2}', '{3}', '{4}', '{5}')";

        private readonly string updSQL =
            "UPDATE CLIENTS SET NAME = '{1}', TAX_NUMBER = '{2}', EMAIL = '{3}', PHONE = '{4}', CACTIVE = '{5}' WHERE ID = {0}";

        private readonly string delSQL = "DELETE FROM CLIENTS WHERE ID = {0}";

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
            // Null értékek kezelése az aktív mezőnél
            string active = string.IsNullOrEmpty(item.CACTIVE) ? "1" : item.CACTIVE;

            if (item.ID <= 0) // INSERT
            {
                item.ID = GetGenerator(conn);
                conn.InsertSQL(string.Format(insSQL, item.ID, item.NAME, item.TAX_NUMBER, item.EMAIL, item.PHONE, active));

                if (__innerList != null && !__innerList.Contains(item))
                    __innerList.Add(item);
            }
            else // UPDATE
            {
                conn.UpdateSQL(string.Format(updSQL, item.ID, item.NAME, item.TAX_NUMBER, item.EMAIL, item.PHONE, active));
            }
        }

        public void Delete(CLIENTS item, FBConnectX conn)
        {
            conn.DeleteSQL(string.Format(delSQL, item.ID));
            if (__innerList != null) __innerList.Remove(item);
        }
    }
}