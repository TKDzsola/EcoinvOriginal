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

        /*partial*/
        private void OnIDChanging(int value) { }
        /*partial*/
        private void OnIDChanged() { }

        #endregion ... end of ID property ...

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

        /*partial*/
        private void OnSZKNEVChanging(string value) { }
        /*partial*/
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

        /*partial*/
        private void OnSZKCIMChanging(string value) { }
        /*partial*/
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

        /*partial*/
        private void OnSZKACTIVEChanging(string value) { }
        /*partial*/
        private void OnSZKACTIVEChanged() { }

        #endregion

        #region ... SZKBANKACCOUNT property ...

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

        /*partial*/
        private void OnSZKBANKACCOUNTChanging(string value) { }
        /*partial*/
        private void OnSZKBANKACCOUNTChanged() { }

        #endregion

        #region ... SZKCOMTAX property ...

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

        /*partial*/
        private void OnSZKCOMTAXChanging(string value) { }
        /*partial*/
        private void OnSZKCOMTAXChanged() { }

        #endregion

        #region ... SZKTAX property ...

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

        /*partial*/
        private void OnSZKTAXChanging(string value) { }
        /*partial*/
        private void OnSZKTAXChanged() { }

        #endregion

        public object PrimaryKeyValue => ID;

    } // ECSYS


    public partial class ECSYSTable
    {
        // SQL lekérdezések a képek alapján
        private readonly string selectSQL = "SELECT ID, SZKNEV, SZKCIM, SZKACTIVE, SZKBANKACCOUNT, SZKCOMTAX, SZKTAX FROM ECSYS";
        private readonly string insSQL = "INSERT INTO ECSYS (ID, SZKNEV, SZKCIM, SZKACTIVE, SZKBANKACCOUNT, SZKCOMTAX, SZKTAX) VALUES ({0}, '{1}', '{2}', '{3}', '{4}', '{5}', '{6}')";
        private readonly string delSQL = "DELETE FROM ECSYS WHERE ID = {0}";
        // Figyelem: az update sorrendje fontos a string.Format paraméterek miatt!
        private readonly string updSQL = "UPDATE ECSYS SET SZKNEV = '{0}', SZKCIM = '{1}', SZKACTIVE = '{2}', SZKBANKACCOUNT = '{3}', SZKCOMTAX = '{4}', SZKTAX = '{5}' WHERE ID = {6}";

        // Feltételezzük, hogy létezik egy GEN_ECSYS_ID generátor. Ha más a neve, itt át kell írni!
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
                SZKACTIVE = "I", // Alapértelmezett érték (I = Igen?)
                SZKBANKACCOUNT = "",
                SZKCOMTAX = "",
                SZKTAX = ""
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
            _actrec.SZKBANKACCOUNT = oldecsys.SZKBANKACCOUNT;
            _actrec.SZKCOMTAX = oldecsys.SZKCOMTAX;
            _actrec.SZKTAX = oldecsys.SZKTAX;
        }

        public void Insert(ECSYS src, FBConnectX conn)
        {
            var _id = GetGenerator(conn);
            // Paraméterek sorrendje: ID, SZKNEV, SZKCIM, SZKACTIVE, SZKBANKACCOUNT, SZKCOMTAX, SZKTAX
            var _inssql = string.Format(insSQL, _id, src.SZKNEV, src.SZKCIM, src.SZKACTIVE, src.SZKBANKACCOUNT, src.SZKCOMTAX, src.SZKTAX);
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
                // Paraméterek sorrendje: SZKNEV, SZKCIM, SZKACTIVE, SZKBANKACCOUNT, SZKCOMTAX, SZKTAX, ID
                var _updsql = string.Format(updSQL, src.SZKNEV, src.SZKCIM, src.SZKACTIVE, src.SZKBANKACCOUNT, src.SZKCOMTAX, src.SZKTAX, src.ID);
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