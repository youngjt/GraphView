﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace GraphView
{
    internal class GremlinSampleOp: GremlinTranslationOperator
    {
        public long AmountToSample { get; set; }

        public GremlinSampleOp(long amountToSample)
        {
            AmountToSample = amountToSample;
        }

        internal override GremlinToSqlContext GetContext()
        {
            GremlinToSqlContext inputContext = GetInputContext();
            if (inputContext.PivotVariable == null)
            {
                throw new QueryCompilationException("The PivotVariable can't be null.");
            }

            throw new NotImplementedException();

        }
    }
}
