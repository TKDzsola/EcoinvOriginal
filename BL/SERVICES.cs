using Ecoinv.Common;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Ecoinv.BL
{
    public partial class SERVICES : TableBaseClass
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private int __id;
        public int ID { get => __id; set => SetPropertyValue(nameof(ID), ref __id, value); }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private string __name;
        public string NAME { get => __name; set => SetPropertyValue(nameof(NAME), ref __name, value); }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private string __description;
        public string DESCRIPTION { get => __description; set => SetPropertyValue(nameof(DESCRIPTION), ref __description, value); }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private int __netprice;
        public int NETPRICE { get => __netprice; set => SetPropertyValue(nameof(NETPRICE), ref __netprice, value); }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private int __grossprice;
        public int GROSSPRICE { get => __grossprice; set => SetPropertyValue(nameof(GROSSPRICE), ref __grossprice, value); }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private int __vatrate_id;
        public int VATRATE_ID { get => __vatrate_id; set => SetPropertyValue(nameof(VATRATE_ID), ref __vatrate_id, value); }

        // --- AKTÍV ("I" = Igen) ---
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private string __sactive = "I";
        public string SACTIVE
        {
            get => __sactive;
            set
            {
                if (SetPropertyValue(nameof(SACTIVE), ref __sactive, value))
                    OnPropertyChanged(nameof(IsActive));
            }
        }

        // Segédmező a XAML-nek (Nincs konverter!)
        public bool IsActive
        {
            get => SACTIVE == "I";
            set => SACTIVE = value ? "I" : "N";
        }

        public object PrimaryKeyValue => ID;
    }

    public partial class SERVICESTable
    {
        private readonly string selectSQL = "SELECT id, name, description, netprice, grossprice, vatrate_id, sactive FROM services ORDER BY name";
        private readonly string insSQL = "INSERT INTO services (id, name, description, netprice, grossprice, vatrate_id, sactive) VALUES ({0}, '{1}', '{2}', {3}, {4}, {5}, '{6}')";
        private readonly string updSQL = "UPDATE services SET name='{1}', description='{2}', netprice={3}, grossprice={4}, vatrate_id={5}, sactive='{6}' WHERE id={0}";
        private readonly string delSQL = "DELETE FROM services WHERE id={0}";
        private readonly string selGenSQL = "SELECT GEN_ID(GEN_SERVICES_ID, 1) FROM RDB$DATABASE";

        private ObservableCollection<SERVICES> __innerList;

        public void InvalidateCache() => __innerList = null;

        public ObservableCollection<SERVICES> GetList(FBConnectX conn)
        {
            if (__innerList != null) return __innerList;
            __innerList = TableBaseClass.GetListBase<SERVICES>(selectSQL, conn);
            return __innerList;
        }

        private int GetGenerator(FBConnectX conn) => DBFunc.Get_Generator(selGenSQL, conn);

        public void Save(SERVICES item, FBConnectX conn)
        {
            string active = string.IsNullOrEmpty(item.SACTIVE) ? "I" : item.SACTIVE;
            string safeName = item.NAME?.Replace("'", "''") ?? "";
            string safeDesc = item.DESCRIPTION?.Replace("'", "''") ?? "";

            if (item.ID <= 0) // INSERT
            {
                item.ID = GetGenerator(conn);
                conn.InsertSQL(string.Format(insSQL, item.ID, safeName, safeDesc, item.NETPRICE, item.GROSSPRICE, item.VATRATE_ID, active));
                if (__innerList != null && !__innerList.Contains(item)) __innerList.Add(item);
            }
            else // UPDATE
            {
                conn.UpdateSQL(string.Format(updSQL, item.ID, safeName, safeDesc, item.NETPRICE, item.GROSSPRICE, item.VATRATE_ID, active));
            }
        }

        public void Delete(SERVICES item, FBConnectX conn)
        {
            conn.DeleteSQL(string.Format(delSQL, item.ID));
            if (__innerList != null) __innerList.Remove(item);
        }
    }
}