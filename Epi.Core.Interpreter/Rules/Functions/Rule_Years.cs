using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.AnalysisInterpreter.Rules
{

    /// <summary>
    /// Class for the Rule_Years reduction.
    /// </summary>
    public partial class Rule_Years : Rule_DateDiff
    {

        private List<AnalysisRule> ParameterList = new List<AnalysisRule>();

        public Rule_Years(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext, pToken, FunctionUtils.DateInterval.Year)
        {
            this.ParameterList = AnalysisRule.GetFunctionParameters(pContext, pToken);
        }

        /// <summary>
        /// Executes the reduction.
        /// </summary>
        /// <returns>Returns the date difference in years between two dates.</returns>
        public override object Execute()
        {
            object result = null;

            object p1 = this.ParameterList[0].Execute();
            object p2 = this.ParameterList[1].Execute();

            if (p1 is DateTime && p2 is DateTime)
            {

                DateTime param1 = (DateTime) p1;
                DateTime param2 = (DateTime) p2;

                int age = param2.Year - param1.Year;
                if
                (
                    param2.Month < param1.Month ||
                    (param2.Month == param1.Month && param2.Day < param1.Day)
                )
                {
                    age--;
                }

                result = age;
            }
             
            return result;
        }
    }

}
