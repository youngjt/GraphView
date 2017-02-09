﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphView
{
    internal class GremlinOptionalVariable : GremlinTableVariable
    {
        public static GremlinOptionalVariable Create(GremlinVariable inputVariable, GremlinToSqlContext context)
        {
            if (inputVariable.GetVariableType() == context.PivotVariable.GetVariableType())
            {
                switch (context.PivotVariable.GetVariableType())
                {
                    case GremlinVariableType.Vertex:
                        return new GremlinOptionalVertexVariable(context, inputVariable);
                    case GremlinVariableType.Edge:
                        return new GremlinOptionalEdgeVariable(context, inputVariable);
                    case GremlinVariableType.Scalar:
                        return new GremlinOptionalScalarVariable(context, inputVariable);
                }
            }
            return new GremlinOptionalVariable(context, inputVariable);
        }

        public GremlinToSqlContext OptionalContext { get; set; }
        public GremlinVariable InputVariable { get; set; }

        public GremlinOptionalVariable(GremlinToSqlContext context,
                                       GremlinVariable inputVariable,
                                       GremlinVariableType variableType = GremlinVariableType.Table)
            : base(variableType)
        {
            OptionalContext = context;
            InputVariable = inputVariable;
        }

        internal override GremlinVariableProperty GetPath()
        {
            return new GremlinVariableProperty(this, GremlinKeyword.Path);
        }

        internal override void Populate(string property)
        {
            if (ProjectedProperties.Contains(property)) return;

            base.Populate(property);
            InputVariable.Populate(property);
            OptionalContext.Populate(property);
        }

        internal override List<GremlinVariable> PopulateAllTaggedVariable(string label)
        {
            return OptionalContext.SelectCurrentAndChildVariable(label);
        }

        internal override List<GremlinVariable> FetchAllVariablesInCurrAndChildContext()
        {
            return OptionalContext.FetchAllVariablesInCurrAndChildContext();
        }

        internal override void PopulateGremlinPath()
        {
            OptionalContext.PopulateGremlinPath();
        }


        internal override bool ContainsLabel(string label)
        {
            if (base.ContainsLabel(label)) return true;
            foreach (var variable in OptionalContext.VariableList)
            {
                if (variable.ContainsLabel(label))
                {
                    return true;
                }
            }
            return false;
        }

        public override WTableReference ToTableReference()
        {
            WSelectQueryBlock firstQueryExpr = new WSelectQueryBlock();
            WSelectQueryBlock secondQueryExpr = OptionalContext.ToSelectQueryBlock();
            secondQueryExpr.SelectElements.Clear();
            firstQueryExpr.SelectElements.Add(SqlUtil.GetSelectScalarExpr(InputVariable.DefaultProjection().ToScalarExpression(), GremlinKeyword.TableDefaultColumnName));
            secondQueryExpr.SelectElements.Add(SqlUtil.GetSelectScalarExpr(OptionalContext.PivotVariable.DefaultProjection().ToScalarExpression(), GremlinKeyword.TableDefaultColumnName));
            foreach (var projectProperty in ProjectedProperties)
            {
                if (InputVariable.ContainsProperties(projectProperty))
                {
                    firstQueryExpr.SelectElements.Add(
                        SqlUtil.GetSelectScalarExpr(
                            InputVariable.GetVariableProperty(projectProperty).ToScalarExpression(), projectProperty));
                }
                else
                {
                    firstQueryExpr.SelectElements.Add(
                        SqlUtil.GetSelectScalarExpr(SqlUtil.GetValueExpr(null), projectProperty));
                }
                if (OptionalContext.PivotVariable.ContainsProperties(projectProperty))
                {
                    secondQueryExpr.SelectElements.Add(
                        SqlUtil.GetSelectScalarExpr(
                            OptionalContext.PivotVariable.GetVariableProperty(projectProperty).ToScalarExpression(), projectProperty));
                }
                else
                {
                    secondQueryExpr.SelectElements.Add(
                        SqlUtil.GetSelectScalarExpr(SqlUtil.GetValueExpr(null), projectProperty));
                }
            }

            var WBinaryQueryExpression = SqlUtil.GetBinaryQueryExpr(firstQueryExpr, secondQueryExpr);

            List<WScalarExpression> parameters = new List<WScalarExpression>();
            parameters.Add(SqlUtil.GetScalarSubquery(WBinaryQueryExpression));
            var secondTableRef = SqlUtil.GetFunctionTableReference(GremlinKeyword.func.Optional, parameters, this, VariableName);
            return SqlUtil.GetCrossApplyTableReference(null, secondTableRef);
        }
    }

    internal class GremlinOptionalVertexVariable : GremlinOptionalVariable
    {
        public GremlinOptionalVertexVariable(GremlinToSqlContext context, GremlinVariable inputVariable)
            : base(context, inputVariable, GremlinVariableType.Vertex)
        {
        }

        internal override void Both(GremlinToSqlContext currentContext, List<string> edgeLabels)
        {
            currentContext.Both(this, edgeLabels);
        }

        internal override void BothE(GremlinToSqlContext currentContext, List<string> edgeLabels)
        {
            currentContext.BothE(this, edgeLabels);
        }

        internal override void BothV(GremlinToSqlContext currentContext)
        {
            currentContext.BothV(this);
        }

        internal override void In(GremlinToSqlContext currentContext, List<string> edgeLabels)
        {
            currentContext.In(this, edgeLabels);
        }

        internal override void InE(GremlinToSqlContext currentContext, List<string> edgeLabels)
        {
            currentContext.InE(this, edgeLabels);
        }

        internal override void Out(GremlinToSqlContext currentContext, List<string> edgeLabels)
        {
            currentContext.Out(this, edgeLabels);
        }

        internal override void OutE(GremlinToSqlContext currentContext, List<string> edgeLabels)
        {
            currentContext.OutE(this, edgeLabels);
        }
    }

    internal class GremlinOptionalEdgeVariable : GremlinOptionalVariable
    {
        public GremlinOptionalEdgeVariable(GremlinToSqlContext context, GremlinVariable inputVariable)
            : base(context, inputVariable, GremlinVariableType.Edge)
        {
        }

        internal override void InV(GremlinToSqlContext currentContext)
        {
            currentContext.InV(this);
        }

        internal override void OutV(GremlinToSqlContext currentContext)
        {
            currentContext.OutV(this);
        }

        internal override void OtherV(GremlinToSqlContext currentContext)
        {
            currentContext.OtherV(this);
        }
    }

    internal class GremlinOptionalScalarVariable : GremlinOptionalVariable
    {
        public GremlinOptionalScalarVariable(GremlinToSqlContext context, GremlinVariable inputVariable)
            : base(context, inputVariable, GremlinVariableType.Scalar)
        {
        }
    }
}
