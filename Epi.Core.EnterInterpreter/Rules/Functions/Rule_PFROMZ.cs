using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    /// <summary>
    /// Class for the Rule_PFROMZ reduction.
    /// </summary>
    public partial class Rule_PFROMZ : EnterRule
    {
        private List<EnterRule> ParameterList = new List<EnterRule>();

        public Rule_PFROMZ(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            this.ParameterList = EnterRule.GetFunctionParameters(pContext, pToken);
            AddCommandVariableCheckValue(ParameterList, "pfromz");
            //if (ParameterList.Count > 0)
            //{
            //    foreach (var item in ParameterList)
            //    {
            //        if (item is Rule_Value)
            //        {
            //            var id = ((Epi.Core.EnterInterpreter.Rules.Rule_Value)(item)).Id;
            //            if (!this.Context.CommandVariableCheck.ContainsKey(id.ToLowerInvariant()))
            //            {
            //                this.Context.CommandVariableCheck.Add(id.ToLowerInvariant(), "pfromz");
            //            }
            //        }
            //    }
            //}
        }

        /// <summary>
        /// Executes the PFROMZ Command
        /// </summary>
        /// <returns>Returns the P value from the Z-score.</returns>
        public override object Execute()
        {
            double result = 0.0;
            if (this.ParameterList[0].Execute() != null && Double.TryParse(this.ParameterList[0].Execute().ToString(), out result))
            {
                result = Math.Round(AnthStat.NutriDataCalc.GetPercentile(result), 2);
                if (result >= 99.9999)
                {
                    result = 99.99;
                }
                else if (result <= -99.9999)
                {
                    result = -99.99;
                }
                return result;
            }
            else
            {
                return null;
            }
        }
        
    }
}
