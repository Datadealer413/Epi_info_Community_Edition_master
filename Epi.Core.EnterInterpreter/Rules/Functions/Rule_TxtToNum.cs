using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    public partial class Rule_TxtToNum : EnterRule
    {
        private List<EnterRule> ParameterList = new List<EnterRule>();

        public Rule_TxtToNum(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            this.ParameterList = EnterRule.GetFunctionParameters(pContext, pToken);
            AddCommandVariableCheckValue(ParameterList, "txttonumber");
            //if (ParameterList.Count > 0)
            //{
            //    foreach (var item in ParameterList)
            //    {
            //        if (item is Rule_Value)
            //        {
            //            var id = ((Epi.Core.EnterInterpreter.Rules.Rule_Value)(item)).Id;
            //            if (!this.Context.CommandVariableCheck.ContainsKey(id.ToLowerInvariant()))
            //            {
            //                this.Context.CommandVariableCheck.Add(id.ToLowerInvariant(), "txttonumber");
            //            }
            //        }
            //    }
            //}
        }

        /// <summary>
        /// returns a number based on a text string
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = null;

            result = ParameterList[0].Execute();

            double doubleValue;
            if (!Util.IsEmpty(result))
            {
                if (result.ToString().ToLowerInvariant().Equals("true") || result.ToString().ToLowerInvariant().Equals("false"))
                {
                    result = (result.ToString().ToLowerInvariant() == "true" ? 1 : 0);
                }

                if (Double.TryParse(result.ToString(), out doubleValue))
                {
                    result = doubleValue;
                }
            }

            return result;
        }
    }
}
