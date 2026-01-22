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

        // --- ÚJ MEZŐK ---

        #region ... IBAN property ...
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string __iban;

        public string IBAN
        {
            get => __iban;
            set
            {
                OnIBANChanging(value);
                SetPropertyValue(nameof(IBAN), ref __iban, value);
                OnIBANChanged();
            }
        }
        private void OnIBANChanging(string value) { }
        private void OnIBANChanged() { }
        #endregion

        #region ... BIC property ...
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string __bic;

        public string BIC
        {
            get => __bic;
            set
            {
                OnBICChanging(value);
                SetPropertyValue(nameof(BIC), ref __bic, value);
                OnBICChanged();
            }
        }
        private void OnBICChanging(string value) { }
        private void OnBICChanged() { }
        #endregion

        public object PrimaryKeyValue => ID;

    } // ECSYS class vége


    public partial class ECSYSTable
    {
        // SQL lekérdezések frissítve az IBAN és BIC mezőkkel
        private readonly string selectSQL = "SELECT ID, SZKNEV, SZKCIM, SZKACTIVE, SZKTAX, SZKCOMTAX, SZKBANKACCOUNT, IBAN, BIC FROM ECSYS";

        private readonly string insSQL = "INSERT INTO ECSYS (ID, SZKNEV, SZKCIM, SZKACTIVE, SZKTAX, SZKCOMTAX, SZKBANKACCOUNT, IBAN, BIC) VALUES ({0}, '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}')";

        private readonly string delSQL = "DELETE FROM ECSYS WHERE ID = {0}";

        // Paraméterek sorrendje frissítve:
        // 0:SZKNEV, 1:SZKCIM, 2:SZKACTIVE, 3:SZKTAX, 4:SZKCOMTAX, 5:SZKBANKACCOUNT, 6:IBAN, 7:BIC, 8:ID
        private readonly string updSQL = "UPDATE ECSYS SET SZKNEV = '{0}', SZKCIM = '{1}', SZKACTIVE = '{2}', SZKTAX = '{3}', SZKCOMTAX = '{4}', SZKBANKACCOUNT = '{5}', IBAN = '{6}', BIC = '{7}' WHERE ID = {8}";

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
                SZKBANKACCOUNT = "",
                IBAN = "",
                BIC = ""
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
            _actrec.IBAN = oldecsys.IBAN;
            _actrec.BIC = oldecsys.BIC;
        }

        public void Insert(ECSYS src, FBConnectX conn)
        {
            var _id = GetGenerator(conn);
            // Paraméterek bővítve: IBAN ({7}), BIC ({8})
            var _inssql = string.Format(insSQL, _id, src.SZKNEV, src.SZKCIM, src.SZKACTIVE, src.SZKTAX, src.SZKCOMTAX, src.SZKBANKACCOUNT, src.IBAN, src.BIC);
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
                // Paraméterek bővítve: IBAN ({6}), BIC ({7}), ID eltolódott ({8}-ra)
                var _updsql = string.Format(updSQL, src.SZKNEV, src.SZKCIM, src.SZKACTIVE, src.SZKTAX, src.SZKCOMTAX, src.SZKBANKACCOUNT, src.IBAN, src.BIC, src.ID);
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