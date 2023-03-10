using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using ComponentArt.Win.DataVisualization;
using ComponentArt.Win.DataVisualization.Common;
using ComponentArt.Win.DataVisualization.Charting;

namespace EpiDashboard
{
    public class ColumnChartParametersBase : ChartParametersBase
    {
        #region Events

        #endregion //Events

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public ColumnChartParametersBase() : base()
        {
            BarSpace = BarSpacing.Default;
            BarType = BarKind.Rectangle;
            YAxisFrom = 0;
            YAxisTo = 0;
            YAxisStep = 0;
            XAxisStart = string.Empty;
            XAxisEnd = string.Empty;
            SortStringValues = false;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        public ColumnChartParametersBase(ColumnChartParametersBase parameters)
            : base(parameters)
        {
            BarSpace = parameters.BarSpace;
            BarType = parameters.BarType;
            YAxisFrom = parameters.YAxisFrom;
            YAxisTo = parameters.YAxisTo;
            YAxisStep = parameters.YAxisStep;
            XAxisStart = parameters.XAxisStart;
            XAxisEnd = parameters.XAxisEnd;
            SortStringValues = parameters.SortStringValues;
        }
        
        #endregion     // Constructors

        #region Properties
        //IChartGadgetParameters Properties
        
        public BarSpacing BarSpace { get; set; }
        public BarKind BarType { get; set; }
        //EI-418
        public double YAxisFrom { get; set; }
        public double YAxisTo { get; set; }
        public double YAxisStep { get; set; }
        public string XAxisStart { get; set; }
        public string XAxisEnd { get; set; }
        public object XAxisStep { get; set; }
        public bool SortStringValues { get; set; }

        #endregion  // Properties

    }
}
