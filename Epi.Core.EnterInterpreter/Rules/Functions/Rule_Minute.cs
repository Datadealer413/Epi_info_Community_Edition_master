using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    public partial class Rule_Minute : Rule_DatePart
    {
        private List<EnterRule> ParameterList = new List<EnterRule>();

        public Rule_Minute(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext, pToken, FunctionUtils.DateInterval.Year)
        {
            this.ParameterList = EnterRule.GetFunctionParameters(pContext, pToken);
            AddCommandVariableCheckValue(ParameterList, "minute");
            //if (ParameterList.Count > 0)
            //{
            //    foreach (var item in ParameterList)
            //    {
            //        if (item is Rule_Value)
            //        {
            //            var id = ((Epi.Core.EnterInterpreter.Rules.Rule_Value)(item)).Id;
            //            if (!this.Context.CommandVariableCheck.ContainsKey(id.ToLowerInvariant()))
            //            {
            //                this.Context.CommandVariableCheck.Add(id.ToLowerInvariant(), "minute");
            //            }
            //        }
            //    }
            //}
        }

        /// <summary>
        /// Executes the reduction.
        /// </summary>
        /// <returns>Returns the date difference in years between two dates.</returns>
        public override object Execute()
        {
            object result = null;

            object p1 = this.ParameterList[0].Execute();

            if (p1 is DateTime)
            {

                DateTime param1 = (DateTime)p1;
                result = param1.Minute;
            }

            return result;
        }
    }
}
