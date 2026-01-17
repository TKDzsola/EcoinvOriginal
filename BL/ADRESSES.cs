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

        // ADDRESS mező az adatbázisban (Utca + Házszám)
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string __address;
        public string ADDRESS
        {
            get => __address;
            set
            {
                SetPropertyValue(nameof(ADDRESS), ref __address, value);
                OnPropertyChanged(nameof(STREET)); // PDF miatt
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
        private string __aactive = "1";
        public string AACTIVE
        {
            get => __aactive;
            set => SetPropertyValue(nameof(AACTIVE), ref __aactive, value);
        }

        // POSTALCODE mező az adatbázisban (INTEGER!)
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int __postalcode;
        public int POSTALCODE
        {
            get => __postalcode;
            set
            {
                SetPropertyValue(nameof(POSTALCODE), ref __postalcode, value);
                OnPropertyChanged(nameof(ZIP)); // PDF miatt
            }
        }

        // --- PDF / ÚJ KÓD KOMPATIBILITÁS ---

        public string ZIP
        {
            get => POSTALCODE.ToString();
            set
            {
                if (int.TryParse(value, out int result)) POSTALCODE = result;
            }
        }

        public string STREET
        {
            get => ADDRESS;
            set => ADDRESS = value;
        }

        // Dummy mezők, hogy a PDF generáló ne szálljon el
        public string COUNTRY { get => ""; set { } }
        public string HOUSE_NUMBER { get => ""; set { } }

        public object PrimaryKeyValue => ID;
    }

    // ========================================================================
    // TABLE - METÓDUSOK
    // ========================================================================
    public partial class ADRESSESTable
    {
        private readonly string selectAllSQL =
            "SELECT ID, CLIENT_ID, CITY, ADDRESS, ATYPE, AACTIVE, POSTALCODE FROM ADRESSES";

        private readonly string selectByIdSQL =
            "SELECT ID, CLIENT_ID, CITY, ADDRESS, ATYPE, AACTIVE, POSTALCODE FROM ADRESSES WHERE CLIENT_ID = {0}";

        private readonly string insSQL =
            "INSERT INTO ADRESSES (ID, CLIENT_ID, CITY, ADDRESS, ATYPE, AACTIVE, POSTALCODE) " +
            "VALUES ({0}, {1}, '{2}', '{3}', '{4}', '{5}', {6})";

        private readonly string updSQL =
            "UPDATE ADRESSES SET CITY='{2}', ADDRESS='{3}', ATYPE='{4}', AACTIVE='{5}', POSTALCODE={6} " +
            "WHERE ID={0}";

        private readonly string delSQL = "DELETE FROM ADRESSES WHERE ID = {0}";

        private readonly string selGenSQL = "SELECT GEN_ID(GEN_ADRESSES_ID, 1) FROM RDB$DATABASE";

        private ObservableCollection<ADRESSES> __innerList;

        // --- LEKÉRDEZÉSEK ---

        public ObservableCollection<ADRESSES> GetList(FBConnectX conn, int clientId)
        {
            string sql = string.Format(selectByIdSQL, clientId);
            return TableBaseClass.GetListBase<ADRESSES>(sql, conn);
        }

        public ObservableCollection<ADRESSES> GetList(FBConnectX conn)
        {
            if (__innerList != null) return __innerList;
            __innerList = TableBaseClass.GetListBase<ADRESSES>(selectAllSQL, conn);
            return __innerList;
        }

        private int GetGenerator(FBConnectX conn) => DBFunc.Get_Generator(selGenSQL, conn);

        // --- MENTÉS LOGIKA ---

        public void Save(ADRESSES item, FBConnectX conn)
        {
            if (item.ID <= 0) Insert(item, conn);
            else Update(item, conn);
        }

        public void Insert(ADRESSES item, FBConnectX conn)
        {
            item.ID = GetGenerator(conn);
            // Null értékek kezelése
            string atype = string.IsNullOrEmpty(item.ATYPE) ? "1" : item.ATYPE;
            string aactive = string.IsNullOrEmpty(item.AACTIVE) ? "1" : item.AACTIVE;
            string city = item.CITY ?? "";
            string address = item.ADDRESS ?? "";

            string sql = string.Format(insSQL,
                item.ID, item.CLIENT_ID, city, address, atype, aactive, item.POSTALCODE);

            conn.InsertSQL(sql);
        }

        public void Update(ADRESSES item, FBConnectX conn)
        {
            string atype = string.IsNullOrEmpty(item.ATYPE) ? "1" : item.ATYPE;
            string aactive = string.IsNullOrEmpty(item.AACTIVE) ? "1" : item.AACTIVE;
            string city = item.CITY ?? "";
            string address = item.ADDRESS ?? "";

            string sql = string.Format(updSQL,
                item.ID, item.CLIENT_ID, city, address, atype, aactive, item.POSTALCODE);

            conn.UpdateSQL(sql);
        }

        public void Delete(ADRESSES item, FBConnectX conn)
        {
            conn.DeleteSQL(string.Format(delSQL, item.ID));
            if (__innerList != null) __innerList.Remove(item);
        }

        public void Delete(int id, FBConnectX conn)
        {
            conn.DeleteSQL(string.Format(delSQL, id));
        }

        // ====================================================================
        // FONTOS: RÉGI KÓD TÁMOGATÁSA (LEGACY METHODS)
        // EZ A RÉSZ HIÁNYZOTT VAGY NEM LÁTSZÓDOTT!
        // ====================================================================

        public void NewADRESSES_M(ADRESSES item, FBConnectX conn)
        {
            Insert(item, conn);
        }

        public void ReUpdateADRESSES_M(ADRESSES item, FBConnectX conn)
        {
            Update(item, conn);
        }

        public void DelNewADRESSES_M(ADRESSES item, FBConnectX conn)
        {
            Delete(item, conn);
        }
    }
}