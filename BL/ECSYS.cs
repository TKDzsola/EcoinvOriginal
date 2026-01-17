using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Ecoinv.Common;

namespace Ecoinv.BL
{
    public partial class ECSYS : TableBaseClass
    {
        public ECSYS()
        {
        }

        #region ... ID property ...
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int __id;

        public int ID
        {
            get => __id;
            set
            {
                OnIDChanging(value);
                SetPropertyValue(nameof(ID), ref __id, value);
                OnIDChanged();
            }
        }
        private void OnIDChanging(int value) { }
        private void OnIDChanged() { }
        #endregion

        #region ... SZKNEV property ...
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string __szknev;

        public string SZKNEV
        {
            get => __szknev;
            set
            {
                OnSZKNEVChanging(value);
                SetPropertyValue(nameof(SZKNEV), ref __szknev, value);
                OnSZKNEVChanged();
            }
        }
        private void OnSZKNEVChanging(string value) { }
        private void OnSZKNEVChanged() { }
        #endregion

        #region ... SZKCIM property ...
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string __szkcim;

        public string SZKCIM
        {
            get => __szkcim;
            set
            {
                OnSZKCIMChanging(value);
                SetPropertyValue(nameof(SZKCIM), ref __szkcim, value);
                OnSZKCIMChanged();
            }
        }
        private void OnSZKCIMChanging(string value) { }
        private void OnSZKCIMChanged() { }
        #endregion

        #region ... SZKACTIVE property ...
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string __szkactive;

        public string SZKACTIVE
        {
            get => __szkactive;
            set
            {
                OnSZKACTIVEChanging(value);
                SetPropertyValue(nameof(SZKACTIVE), ref __szkactive, value);
                OnSZKACTIVEChanged();
            }
        }
        private void OnSZKACTIVEChanging(string value) { }
        private void OnSZKACTIVEChanged() { }
        #endregion

        #region ... SZKTAX property (Adószám) ...
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string __szktax;

        public string SZKTAX
        {
            get => __szktax;
            set
            {
                OnSZKTAXChanging(value);
                SetPropertyValue(nameof(SZKTAX), ref __szktax, value);
                OnSZKTAXChanged();
            }
        }
        private void OnSZKTAXChanging(string value) { }
        private void OnSZKTAXChanged() { }
        #endregion

        #region ... SZKCOMTAX property (Közösségi adószám) ...
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string __szkcomtax;

        public string SZKCOMTAX
        {
            get => __szkcomtax;
            set
            {
                OnSZKCOMTAXChanging(value);
                SetPropertyValue(nameof(SZKCOMTAX), ref __szkcomtax, value);
                OnSZKCOMTAXChanged();
            }
        }
        private void OnSZKCOMTAXChanging(string value) { }
        private void OnSZKCOMTAXChanged() { }
        #endregion

        #region ... SZKBANKACCOUNT property (Bankszámlaszám) ...
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string __szkbankaccount;

        public string SZKBANKACCOUNT
        {
            get => __szkbankaccount;
            set
            {
                OnSZKBANKACCOUNTChanging(value);
                SetPropertyValue(nameof(SZKBANKACCOUNT), ref __szkbankaccount, value);
                OnSZKBANKACCOUNTChanged();
            }
        }
        private void OnSZKBANKACCOUNTChanging(string value) { }
        private void OnSZKBANKACCOUNTChanged() { }
        #endregion

        public object PrimaryKeyValue => ID;

    } // ECSYS class vége


    public partial class ECSYSTable
    {
        // SQL lekérdezések frissítve a 3 új oszloppal
        private readonly string selectSQL = "SELECT ID, SZKNEV, SZKCIM, SZKACTIVE, SZKTAX, SZKCOMTAX, SZKBANKACCOUNT FROM ECSYS";

        private readonly string insSQL = "INSERT INTO ECSYS (ID, SZKNEV, SZKCIM, SZKACTIVE, SZKTAX, SZKCOMTAX, SZKBANKACCOUNT) VALUES ({0}, '{1}', '{2}', '{3}', '{4}', '{5}', '{6}')";

        private readonly string delSQL = "DELETE FROM ECSYS WHERE ID = {0}";

        // Paraméterek sorrendje: 0:NEV, 1:CIM, 2:ACTIVE, 3:TAX, 4:COMTAX, 5:BANK, 6:ID
        private readonly string updSQL = "UPDATE ECSYS SET SZKNEV = '{0}', SZKCIM = '{1}', SZKACTIVE = '{2}', SZKTAX = '{3}', SZKCOMTAX = '{4}', SZKBANKACCOUNT = '{5}' WHERE ID = {6}";

        private readonly string selGenSQL = "SELECT GEN_ID(GEN_ECSYS_ID, 1) FROM RDB$DATABASE";

        private ObservableCollection<ECSYS> __innerList;

        public ObservableCollection<ECSYS> GetList(FBConnectX conn)
        {
            if (__innerList != null)
                return __innerList;

            __innerList = TableBaseClass.GetListBase<ECSYS>(selectSQL, conn);
            return __innerList;
        }

        private int GetGenerator(FBConnectX conn) => DBFunc.Get_Generator(selGenSQL, conn);

        public ECSYS NewECSYS_M()
        {
            var rec = new ECSYS
            {
                ID = -1,
                SZKNEV = "",
                SZKCIM = "",
                SZKACTIVE = "I",
                SZKTAX = "",
                SZKCOMTAX = "",
                SZKBANKACCOUNT = ""
            };
            __innerList.Add(rec);
            return rec;
        }

        public void DelNewECSYS_M()
        {
            var _actrec = __innerList.FirstOrDefault(r => r.ID == -1);
            if (_actrec != null)
                __innerList.Remove(_actrec);
        }

        public void ReUpdateECSYS_M(ECSYS oldecsys)
        {
            var _actrec = __innerList.FirstOrDefault(r => r.ID == oldecsys.ID);
            if (_actrec == null)
                return;

            _actrec.SZKNEV = oldecsys.SZKNEV;
            _actrec.SZKCIM = oldecsys.SZKCIM;
            _actrec.SZKACTIVE = oldecsys.SZKACTIVE;
            _actrec.SZKTAX = oldecsys.SZKTAX;
            _actrec.SZKCOMTAX = oldecsys.SZKCOMTAX;
            _actrec.SZKBANKACCOUNT = oldecsys.SZKBANKACCOUNT;
        }

        public void Insert(ECSYS src, FBConnectX conn)
        {
            var _id = GetGenerator(conn);
            // Paraméterek: ID, SZKNEV, SZKCIM, SZKACTIVE, SZKTAX, SZKCOMTAX, SZKBANKACCOUNT
            var _inssql = string.Format(insSQL, _id, src.SZKNEV, src.SZKCIM, src.SZKACTIVE, src.SZKTAX, src.SZKCOMTAX, src.SZKBANKACCOUNT);
            conn?.InsertSQL(_inssql);

            var _actrec = __innerList.FirstOrDefault(r => r.ID == -1);
            if (_actrec != null)
                src.ID = _id;
        }

        public void Update(ECSYS src, FBConnectX conn)
        {
            var _actrec = __innerList.FirstOrDefault(r => r.ID == src.ID);
            if (_actrec != null)
            {
                // Paraméterek: SZKNEV, SZKCIM, SZKACTIVE, SZKTAX, SZKCOMTAX, SZKBANKACCOUNT, ID
                var _updsql = string.Format(updSQL, src.SZKNEV, src.SZKCIM, src.SZKACTIVE, src.SZKTAX, src.SZKCOMTAX, src.SZKBANKACCOUNT, src.ID);
                conn?.UpdateSQL(_updsql);
            }
        }

        public void Delete(int num, FBConnectX conn)
        {
            var _actrec = __innerList.FirstOrDefault(r => r.ID == num);
            if (_actrec != null)
            {
                __innerList.Remove(_actrec);
                var _delsql = string.Format(delSQL, num);
                conn?.DeleteSQL(_delsql);
            }
        }
    }
}