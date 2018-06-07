﻿using NBi.Core.Calculation.Predication;
using NBi.Extensibility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBi.Core.Calculation.Grouping.CaseBased
{
    class CaseGrouping : IGroupBy
    {
        protected IEnumerable<IPredication> Cases { get; }

        public CaseGrouping(IEnumerable<IPredication> cases)
        {
            Cases = cases;
        }

        public IDictionary<object, DataTable> Execute(ResultSet.ResultSet resultSet)
        {
            var stopWatch = new Stopwatch();
            var dico = new Dictionary<object, DataTable>();
            
            stopWatch.Start();
            foreach (DataRow row in resultSet.Rows)
            {
                var index = Cases.Select((p, i) => new { Predication = p, Index =  i })
                                .FirstOrDefault(x => x.Predication.Execute(row))
                                ?.Index ?? -1;
                if (!dico.ContainsKey(index))
                    dico.Add(index, row.Table.Clone());
                dico[index].ImportRow(row);
            }
            Trace.WriteLineIf(NBiTraceSwitch.TraceInfo, $"Building rows' groups by cases: {dico.Count} [{stopWatch.Elapsed.ToString(@"d\d\.hh\h\:mm\m\:ss\s\ \+fff\m\s")}");

            return dico;
        }
    }
}