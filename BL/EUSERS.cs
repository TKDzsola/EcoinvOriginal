using Ecoinv.Common;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq; // Ez kell a Remove-hoz

namespace Ecoinv.BL
{
    public partial class EUSERS : TableBaseClass
    {
        private MD5Func md5f = new MD5Func();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private int __id;
        public int ID { get => __id; set => SetPropertyValue(nameof(ID), ref __id, value); }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private string __uname;
        public string UNAME { get => __uname; set => SetPropertyValue(nameof(UNAME), ref __uname, value); }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private string __upssw;
        public string UPSSW { get => __upssw; set => SetPropertyValue(nameof(UPSSW), ref __upssw, value); }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private string __fulname;
        public string FULNAME { get => __fulname; set => SetPropertyValue(nameof(FULNAME), ref __fulname, value); }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private string __isadmin = "N";
        public string ISADMIN { get => __isadmin; set => SetPropertyValue(nameof(ISADMIN), ref __isadmin, value); }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private string __uactive = "I";
        public string UACTIVE
        {
            get => __uactive;
            set
            {
                if (SetPropertyValue(nameof(UACTIVE), ref __uactive, value))
                    OnPropertyChanged(nameof(IsActive));
            }
        }

        public bool IsActive
        {
            get => UACTIVE == "I";
            set => UACTIVE = value ? "I" : "N";
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private string __jelszo_no_md5;
        public string JELSZO_NO_MD5
        {
            get => __jelszo_no_md5;
            set
            {
                SetPropertyValue(nameof(JELSZO_NO_MD5), ref __jelszo_no_md5, value);
                if (!string.IsNullOrEmpty(value)) UPSSW = md5f.MD5Encode(value);
            }
        }
        public object PrimaryKeyValue => ID;
    }

    public partial class EUSERSTable
    {
        private readonly string selectSQL = "SELECT ID, UNAME, UPSSW, UACTIVE, ISADMIN, FULNAME, '' AS JELSZO_NO_MD5 FROM EUSERS ORDER BY UNAME";
        private readonly string insSQL = "INSERT INTO EUSERS (ID, UNAME, UPSSW, UACTIVE, ISADMIN, FULNAME) VALUES ({0}, '{1}', '{2}', '{3}', '{4}', '{5}')";
        private readonly string updSQL = "UPDATE EUSERS SET UNAME='{1}', UPSSW='{2}', UACTIVE='{3}', ISADMIN='{4}', FULNAME='{5}' WHERE ID={0}";
        private readonly string delSQL = "DELETE FROM EUSERS WHERE ID={0}";
        private readonly string selGenSQL = "SELECT GEN_ID(GEN_EUSERS_ID, 1) FROM RDB$DATABASE";

        private ObservableCollection<EUSERS> __innerList;

        public ObservableCollection<EUSERS> GetList(FBConnectX conn)
        {
            if (__innerList != null) return __innerList;
            __innerList = TableBaseClass.GetListBase<EUSERS>(selectSQL, conn);
            return __innerList;
        }

        private int GetGenerator(FBConnectX conn) => DBFunc.Get_Generator(selGenSQL, conn);

        public void Save(EUSERS item, FBConnectX conn)
        {
            string active = string.IsNullOrEmpty(item.UACTIVE) ? "I" : item.UACTIVE;
            string admin = string.IsNullOrEmpty(item.ISADMIN) ? "N" : item.ISADMIN;
            string safeUname = item.UNAME?.Replace("'", "''") ?? "";
            string safeFullname = item.FULNAME?.Replace("'", "''") ?? "";

            if (item.ID <= 0)
            {
                item.ID = GetGenerator(conn);
                conn.InsertSQL(string.Format(insSQL, item.ID, safeUname, item.UPSSW, active, admin, safeFullname));
                if (__innerList != null && !__innerList.Contains(item)) __innerList.Add(item);
            }
            else
            {
                conn.UpdateSQL(string.Format(updSQL, item.ID, safeUname, item.UPSSW, active, admin, safeFullname));
            }
        }

        // JAVÍTVA: Itt "EUSERS item"-et várunk, nem int-et!
        public void Delete(EUSERS item, FBConnectX conn)
        {
            conn.DeleteSQL(string.Format(delSQL, item.ID));
            if (__innerList != null) __innerList.Remove(item);
        }
    }
}