using Ecoinv.Common;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq; // Ez kell a Remove-hoz

namespace Ecoinv.BL
{
    public partial class VATRATES : TableBaseClass
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private int __id;
        public int ID { get => __id; set => SetPropertyValue(nameof(ID), ref __id, value); }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private string __name;
        public string NAME { get => __name; set => SetPropertyValue(nameof(NAME), ref __name, value); }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private int __rates;
        public int RATES { get => __rates; set => SetPropertyValue(nameof(RATES), ref __rates, value); }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private string __vactive = "I";
        public string VACTIVE
        {
            get => __vactive;
            set
            {
                if (SetPropertyValue(nameof(VACTIVE), ref __vactive, value))
                    OnPropertyChanged(nameof(IsActive));
            }
        }

        public bool IsActive
        {
            get => VACTIVE == "I";
            set => VACTIVE = value ? "I" : "N";
        }

        public object PrimaryKeyValue => ID;
    }

    public partial class VATRATESTable
    {
        private readonly string selectSQL = "SELECT ID, NAME, RATES, VACTIVE FROM VATRATES ORDER BY NAME";
        private readonly string insSQL = "INSERT INTO VATRATES (ID, NAME, RATES, VACTIVE) VALUES ({0}, '{1}', {2}, '{3}')";
        private readonly string updSQL = "UPDATE VATRATES SET NAME='{1}', RATES={2}, VACTIVE='{3}' WHERE ID={0}";
        private readonly string delSQL = "DELETE FROM VATRATES WHERE ID={0}";
        private readonly string selGenSQL = "SELECT GEN_ID(GEN_VATRATES_ID, 1) FROM RDB$DATABASE";

        private ObservableCollection<VATRATES> __innerList;

        public void InvalidateCache() => __innerList = null;

        public ObservableCollection<VATRATES> GetList(FBConnectX conn)
        {
            if (__innerList != null) return __innerList;
            __innerList = TableBaseClass.GetListBase<VATRATES>(selectSQL, conn);
            return __innerList;
        }

        private int GetGenerator(FBConnectX conn) => DBFunc.Get_Generator(selGenSQL, conn);

        public void Save(VATRATES item, FBConnectX conn)
        {
            string active = string.IsNullOrEmpty(item.VACTIVE) ? "I" : item.VACTIVE;
            string safeName = item.NAME?.Replace("'", "''") ?? "";

            if (item.ID <= 0)
            {
                item.ID = GetGenerator(conn);
                conn.InsertSQL(string.Format(insSQL, item.ID, safeName, item.RATES, active));
                if (__innerList != null && !__innerList.Contains(item)) __innerList.Add(item);
            }
            else
            {
                conn.UpdateSQL(string.Format(updSQL, item.ID, safeName, item.RATES, active));
            }
        }

        // JAVÍTVA: Itt "VATRATES item"-et várunk, nem int-et!
        public void Delete(VATRATES item, FBConnectX conn)
        {
            conn.DeleteSQL(string.Format(delSQL, item.ID));
            if (__innerList != null) __innerList.Remove(item);
        }
    }
}