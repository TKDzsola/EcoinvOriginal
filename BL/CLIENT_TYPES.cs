using Ecoinv.Common;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Ecoinv.BL
{
    public partial class CLIENT_TYPES : TableBaseClass
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int __id;
        public int ID
        {
            get => __id;
            set => SetPropertyValue(nameof(ID), ref __id, value);
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string __type_name;
        public string TYPE_NAME
        {
            get => __type_name;
            set => SetPropertyValue(nameof(TYPE_NAME), ref __type_name, value);
        }

        public object PrimaryKeyValue => ID;
    }

    public partial class CLIENT_TYPESTable
    {
        private readonly string selectSQL = "SELECT ID, TYPE_NAME FROM CLIENT_TYPES ORDER BY TYPE_NAME";

        public ObservableCollection<CLIENT_TYPES> GetList(FBConnectX conn)
        {
            return TableBaseClass.GetListBase<CLIENT_TYPES>(selectSQL, conn);
        }
    }
}