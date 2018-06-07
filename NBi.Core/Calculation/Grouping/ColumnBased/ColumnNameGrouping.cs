﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NBi.Core.ResultSet;

namespace NBi.Core.Calculation.Grouping.ColumnBased
{
    class ColumnNameGrouping : ColumnGrouping
    {
        protected new SettingsNameResultSet Settings { get => base.Settings as SettingsNameResultSet; }

        public ColumnNameGrouping(SettingsNameResultSet settings)
            : base(settings) { }

        protected override DataRowKeysComparer BuildDataRowsKeyComparer(DataTable x)
            => new DataRowKeysComparerByName(Settings);
    }
}