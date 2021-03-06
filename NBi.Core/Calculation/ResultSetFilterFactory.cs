﻿using NBi.Core.Calculation.Grouping;
using NBi.Core.Calculation.Predicate;
using NBi.Core.Calculation.Predicate.Combination;
using NBi.Core.Calculation.Ranking;
using NBi.Core.Evaluate;
using NBi.Core.ResultSet;
using NBi.Core.Variable;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBi.Core.Calculation
{
    public class ResultSetFilterFactory
    {
        private readonly IDictionary<string, ITestVariable> variables;

        public ResultSetFilterFactory(IDictionary<string, ITestVariable> variables)
        {
            this.variables = variables;
        }

        public IResultSetFilter Instantiate(IEnumerable<IColumnAlias> aliases, IEnumerable<IColumnExpression> expressions, IPredicateInfo predicateInfo)
        {
            if (predicateInfo.Operand == null)
                throw new ArgumentException("You must specify an operand for a predicate. The operand is the column or alias or expression on which the predicate will be evaluated.");

            var factory = new PredicateFactory();
            var predicate = factory.Instantiate(predicateInfo, variables);

            var pf = new SinglePredicateFilter(aliases, expressions, predicateInfo.Operand, predicate.Execute, predicate.ToString);

            return pf;
        }

        public IResultSetFilter Instantiate(IEnumerable<IColumnAlias> aliases, IEnumerable<IColumnExpression> expressions, CombinationOperator combinationOperator, IEnumerable<IPredicateInfo> predicateInfos)
        {
            var predications = new List<Predication>();

            var factory = new PredicateFactory();
            foreach (var predicateInfo in predicateInfos)
            {
                if (predicateInfo.Operand == null)
                    throw new ArgumentException("You must specify an operand for a predicate. The operand is the column or alias or expression on which the predicate will be evaluated.");

                var predicate = factory.Instantiate(predicateInfo, variables);
                predications.Add(new Predication(predicate, predicateInfo.Operand));
            }

            switch (combinationOperator)
            {
                case CombinationOperator.Or:
                    return new OrCombinationPredicateFilter(aliases, expressions, predications);
                case CombinationOperator.XOr:
                    return new XOrCombinationPredicateFilter(aliases, expressions, predications);
                case CombinationOperator.And:
                    return new AndCombinationPredicateFilter(aliases, expressions, predications);
                default:
                    throw new ArgumentOutOfRangeException(nameof(combinationOperator));
            }
        }


        public IResultSetFilter Instantiate(IRankingInfo rankingInfo, IEnumerable<IColumnDefinitionLight> columns)
        {
            var groupingFactory = new ByColumnGroupingFactory();
            var grouping = groupingFactory.Instantiate(columns);

            var rankingFactory = new RankingFactory();
            var ranking = rankingFactory.Instantiate(rankingInfo);

            return new FilterGroupByFilter(ranking, grouping);
        }
    }
}
