using Ecoinv.Common;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Ecoinv.BL
{
    // ========================================================================
    // MODEL - ADATBÁZIS KOMPATIBILIS
    // ========================================================================
    public partial class ADRESSES : TableBaseClass
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int __id;
        public int ID
        {
            get => __id;
            set => SetPropertyValue(nameof(ID), ref __id, value);
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int __client_id;
        public int CLIENT_ID
        {
            get => __client_id;
            set => SetPropertyValue(nameof(CLIENT_ID), ref __client_id, value);
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string __city;
        public string CITY
        {
            get => __city;
            set => SetPropertyValue(nameof(CITY), ref __city, value);
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string __address;
        public string ADDRESS
        {
            get => __address;
            set
            {
                SetPropertyValue(nameof(ADDRESS), ref __address, value);
                OnPropertyChanged(nameof(STREET));
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string __atype = "1";
        public string ATYPE
        {
            get => __atype;
            set => SetPropertyValue(nameof(ATYPE), ref __atype, value);
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string __aactive = "I";
        public string AACTIVE
        {
            get => __aactive;
            set => SetPropertyValue(nameof(AACTIVE), ref __aactive, value);
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int __postalcode;
        public int POSTALCODE
        {
            get => __postalcode;
            set
            {
                SetPropertyValue(nameof(POSTALCODE), ref __postalcode, value);
                OnPropertyChanged(nameof(ZIP));
            }
        }

        public string ZIP
        {
            get => POSTALCODE.ToString();
            set { if (int.TryParse(value, out int result)) POSTALCODE = result; }
        }

        public string STREET { get => ADDRESS; set => ADDRESS = value; }
        public string COUNTRY { get => ""; set { } }
        public string HOUSE_NUMBER { get => ""; set { } }
        public object PrimaryKeyValue => ID;
    }

    // ========================================================================
    // TABLE - METÓDUSOK
    // ========================================================================
    public partial class ADRESSESTable
    {
        private readonly string selectAllSQL = "SELECT ID, CLIENT_ID, CITY, ADDRESS, ATYPE, AACTIVE, POSTALCODE FROM ADRESSES";
        private readonly string selectByIdSQL = "SELECT ID, CLIENT_ID, CITY, ADDRESS, ATYPE, AACTIVE, POSTALCODE FROM ADRESSES WHERE CLIENT_ID = {0}";
        private readonly string insSQL = "INSERT INTO ADRESSES (ID, CLIENT_ID, CITY, ADDRESS, ATYPE, AACTIVE, POSTALCODE) VALUES ({0}, {1}, '{2}', '{3}', '{4}', '{5}', {6})";
        private readonly string updSQL = "UPDATE ADRESSES SET CITY='{2}', ADDRESS='{3}', ATYPE='{4}', AACTIVE='{5}', POSTALCODE={6} WHERE ID={0}";
        private readonly string delSQL = "DELETE FROM ADRESSES WHERE ID = {0}";
        private readonly string selGenSQL = "SELECT GEN_ID(GEN_ADRESSES_ID, 1) FROM RDB$DATABASE";

        private ObservableCollection<ADRESSES> __innerList;

        // JAVÍTVA: Paraméter nélküli GetList a DataContext híváshoz
        public ObservableCollection<ADRESSES> GetList(FBConnectX conn)
        {
            if (__innerList != null) return __innerList;
            __innerList = TableBaseClass.GetListBase<ADRESSES>(selectAllSQL, conn);
            return __innerList;
        }

        // Túlterhelt verzió a clientId-hoz
        public ObservableCollection<ADRESSES> GetList(FBConnectX conn, int clientId)
        {
            string sql = string.Format(selectByIdSQL, clientId);
            return TableBaseClass.GetListBase<ADRESSES>(sql, conn);
        }

        private int GetGenerator(FBConnectX conn) => DBFunc.Get_Generator(selGenSQL, conn);

        public void Save(ADRESSES item, FBConnectX conn)
        {
            string originalType = item.ATYPE;
            // Konverzió a Firebird 1 karakteres mezője miatt
            if (item.ATYPE == "Számlázási") item.ATYPE = "1";
            else if (item.ATYPE == "Levelezési") item.ATYPE = "2";
            else if (item.ATYPE == "Telephely") item.ATYPE = "3";

            // Konverzió az I/N státuszhoz
            if (item.AACTIVE == "1" || string.IsNullOrEmpty(item.AACTIVE)) item.AACTIVE = "I";
            else if (item.AACTIVE == "0") item.AACTIVE = "N";

            try
            {
                if (item.ID <= 0) Insert(item, conn);
                else Update(item, conn);
            }
            finally { item.ATYPE = originalType; }
        }

        // JAVÍTVA: Publikus metódusok a DataContext számára
        public void Insert(ADRESSES item, FBConnectX conn)
        {
            item.ID = GetGenerator(conn);
            conn.InsertSQL(string.Format(insSQL, item.ID, item.CLIENT_ID, item.CITY?.Replace("'", "''"), item.ADDRESS?.Replace("'", "''"), item.ATYPE, item.AACTIVE, item.POSTALCODE));
        }

        public void Update(ADRESSES item, FBConnectX conn)
        {
            conn.UpdateSQL(string.Format(updSQL, item.ID, item.CLIENT_ID, item.CITY?.Replace("'", "''"), item.ADDRESS?.Replace("'", "''"), item.ATYPE, item.AACTIVE, item.POSTALCODE));
        }

        public void Delete(ADRESSES item, FBConnectX conn)
        {
            conn.DeleteSQL(string.Format(delSQL, item.ID));
            if (__innerList != null) __innerList.Remove(item);
        }
    }
}