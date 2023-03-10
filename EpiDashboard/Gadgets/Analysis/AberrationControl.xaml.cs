using System;
using System.Data;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.DataVisualization.Charting;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Epi;
using Epi.Data;
using Epi.Fields;
using EpiDashboard.Rules;

namespace EpiDashboard
{
    /// <summary>
    /// Interaction logic for AberrationControl.xaml
    /// </summary>
    public partial class AberrationControl : GadgetBase
    {
        #region Classes
        public class SimpleDataValue
        {
            public double DependentValue { get; set; }
            public DateTime IndependentValue { get; set; }
        }

        public class DataGridRow
        {
            public DateTime Date { get; set; }
            public double Frequency { get; set; }
            public double RunningAverage { get; set; }
            public double StandardDeviation { get; set; }
            public double Delta
            {
                get
                {
                    return (Frequency - RunningAverage) / StandardDeviation;
                }
            }
        }
        #endregion // Classes

        #region Private Variables

        private const int DEFAULT_DEVIATIONS = 3;
        private const int DEFAULT_LAG_TIME = 7;
        private const int DEFAULT_TIME_PERIOD = 365;

        #endregion

        #region Delegates

        private delegate void SetGraphDelegate(string strataValue, List<SimpleDataValue> actualValues, List<SimpleDataValue> trendValues, List<SimpleDataValue> aberrationValues, List<DataGridRow> aberrationDetails);

        #endregion

        #region Constructors

        public AberrationControl()
        {
            InitializeComponent();
            Construct();
        }

        public AberrationControl(DashboardHelper dashboardHelper)
        {
            InitializeComponent();
            this.DashboardHelper = dashboardHelper;
            Construct();
        }

        protected override void Construct()
        {
            if (!string.IsNullOrEmpty(CustomOutputHeading) && !CustomOutputHeading.Equals("(none)"))
            {
                headerPanel.Text = CustomOutputHeading;
            }

            this.IsProcessing = false;

            this.IsProcessing = false;

            this.GadgetStatusUpdate += new GadgetStatusUpdateHandler(RequestUpdateStatusMessage);
            this.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(IsCancelled);

            base.Construct();
        }

        #endregion
    
        protected override void worker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            lock (syncLock)
            {
                AberrationDetectionChartParameters aberrationParameters = (AberrationDetectionChartParameters)Parameters;

                this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToProcessingState));
                this.Dispatcher.BeginInvoke(new SimpleCallback(ClearResults));

                SetGraphDelegate setGraph = new SetGraphDelegate(SetGraph);
                System.Collections.Generic.Dictionary<string, System.Data.DataTable> Freq_ListSet = new Dictionary<string, System.Data.DataTable>();

                string freqVar = string.Empty;
                string weightVar = string.Empty;
                string strataVar = string.Empty;
                bool includeMissing = false;
                int lagTime = DEFAULT_LAG_TIME;
                int timePeriod = DEFAULT_TIME_PERIOD;
                double deviations = DEFAULT_DEVIATIONS;

                if (!String.IsNullOrEmpty(aberrationParameters.ColumnNames[0]))
                    freqVar = aberrationParameters.ColumnNames[0];

                if (!String.IsNullOrEmpty(aberrationParameters.WeightVariableName))
                    weightVar = aberrationParameters.WeightVariableName;

                List<string> stratas = new List<string>();
                if (aberrationParameters.StrataVariableNames.Count > 0)
                {
                    stratas = aberrationParameters.StrataVariableNames;
                }

                if (!String.IsNullOrEmpty(aberrationParameters.LagTime))
                {
                    int.TryParse(aberrationParameters.LagTime, out lagTime);
                }

                if (!String.IsNullOrEmpty(aberrationParameters.TimePeriod))
                {
                    int.TryParse(aberrationParameters.TimePeriod, out timePeriod);
                }

                if (!String.IsNullOrEmpty(aberrationParameters.Deviations))
                {
                    double.TryParse(aberrationParameters.Deviations, out deviations);
                }

                includeMissing = aberrationParameters.IncludeMissing;


                deviations = deviations - 0.001;

                if (!string.IsNullOrEmpty(strataVar))
                {
                    stratas.Add(strataVar);
                }

                try
                {
                    RequestUpdateStatusDelegate requestUpdateStatus = new RequestUpdateStatusDelegate(RequestUpdateStatusMessage);
                    CheckForCancellationDelegate checkForCancellation = new CheckForCancellationDelegate(IsCancelled);

                    aberrationParameters.GadgetStatusUpdate += new GadgetStatusUpdateHandler(requestUpdateStatus);
                    aberrationParameters.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(checkForCancellation);

                    if (this.DataFilters != null && this.DataFilters.Count > 0)
                    {
                        aberrationParameters.CustomFilter = this.DataFilters.GenerateDataFilterString(false);
                    }
                    else
                    {
                        aberrationParameters.CustomFilter = string.Empty;
                    }

                    Dictionary<DataTable, List<DescriptiveStatistics>> stratifiedFrequencyTables = DashboardHelper.GenerateFrequencyTable(aberrationParameters);

                    if (stratifiedFrequencyTables == null || stratifiedFrequencyTables.Count == 0)
                    {
                        this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), DashboardSharedStrings.GADGET_MSG_NO_DATA);
                        this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                        return;
                    }
                    else if (worker.CancellationPending)
                    {
                        this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), SharedStrings.DASHBOARD_GADGET_STATUS_OPERATION_CANCELLED);
                        this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                        return;
                    }
                    else
                    {
                        string formatString = string.Empty;

                        foreach (KeyValuePair<DataTable, List<DescriptiveStatistics>> tableKvp in stratifiedFrequencyTables)
                        {
                            AddFillerRows(tableKvp.Key);

                            if (tableKvp.Key.Rows.Count > 366)
                            {
                                string exMessage = string.Format(DashboardSharedStrings.ERROR_TOO_MANY_VALUES, freqVar, tableKvp.Key.Rows.Count, 366);
                                throw new ApplicationException(exMessage);
                            }

                            string strataValue = tableKvp.Key.TableName;

                            double count = 0;
                            foreach (DescriptiveStatistics ds in tableKvp.Value)
                            {
                                count = count + ds.observations;
                            }

                            if (count == 0 && stratifiedFrequencyTables.Count == 1)
                            {
                                // this is the only table and there are no records, so let the user know
                                this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), SharedStrings.NO_RECORDS_SELECTED);
                                this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                                return;
                            }
                            DataTable frequencies = tableKvp.Key;

                            if (frequencies.Rows.Count == 0)
                            {
                                continue;
                            }
                        }

                        foreach (KeyValuePair<DataTable, List<DescriptiveStatistics>> tableKvp in stratifiedFrequencyTables)
                        {
                            string strataValue = tableKvp.Key.TableName;

                            double count = 0;
                            foreach (DescriptiveStatistics ds in tableKvp.Value)
                            {
                                count = count + ds.observations;
                            }

                            if (count == 0)
                            {
                                continue;
                            }
                            DataTable frequencies = tableKvp.Key;

                            if (frequencies.Rows.Count == 0)
                            {
                                continue;
                            }

                            string tableHeading = tableKvp.Key.TableName;

                            if (stratifiedFrequencyTables.Count > 1)
                            {
                                tableHeading = freqVar;
                            }

                            double lastAvg = double.NegativeInfinity;
                            double lastStdDev = double.NegativeInfinity;
                            Queue<double> frame = new Queue<double>();
                            List<SimpleDataValue> actualValues = new List<SimpleDataValue>();
                            List<SimpleDataValue> trendValues = new List<SimpleDataValue>();
                            List<SimpleDataValue> aberrationValues = new List<SimpleDataValue>();
                            List<DataGridRow> aberrationDetails = new List<DataGridRow>();
                            int rowCount = 1;
                            foreach (System.Data.DataRow row in frequencies.Rows)
                            {
                                if (!row[freqVar].Equals(DBNull.Value) || (row[freqVar].Equals(DBNull.Value) && includeMissing == true))
                                {
                                    DateTime displayValue = DateTime.Parse(row[freqVar].ToString());

                                    frame.Enqueue((double)row["freq"]);
                                    SimpleDataValue actualValue = new SimpleDataValue();
                                    actualValue.IndependentValue = displayValue;
                                    actualValue.DependentValue = (double)row["freq"];
                                    actualValues.Add(actualValue);
                                    if (frame.Count > lagTime - 1 /*6*/)
                                    {
                                        double[] frameArray = frame.ToArray();
                                        double frameAvg = frameArray.Average();
                                        frame.Dequeue();
                                        double stdDev = CalculateStdDev(frameArray);
                                        if (lastAvg != double.NegativeInfinity)
                                        {
                                            SimpleDataValue trendValue = new SimpleDataValue();
                                            trendValue.IndependentValue = displayValue;
                                            trendValue.DependentValue = lastAvg;
                                            trendValues.Add(trendValue);
                                            if ((double)row["freq"] > lastAvg + (/*2.99*/deviations * lastStdDev))
                                            {
                                                SimpleDataValue aberrationValue = new SimpleDataValue();
                                                aberrationValue.IndependentValue = displayValue;
                                                aberrationValue.DependentValue = (double)row["freq"];
                                                aberrationValues.Add(aberrationValue);
                                                DataGridRow aberrationDetail = new DataGridRow()
                                                {
                                                    Date = displayValue,
                                                    Frequency = (double)row["freq"],
                                                    RunningAverage = lastAvg,
                                                    StandardDeviation = lastStdDev
                                                };
                                                aberrationDetails.Add(aberrationDetail);
                                            }
                                        }
                                        lastAvg = frameAvg;
                                        lastStdDev = stdDev;
                                    }

                                    rowCount++;
                                }
                            }

                            this.Dispatcher.BeginInvoke(setGraph, strataValue, actualValues, trendValues, aberrationValues, aberrationDetails);
                            System.Threading.Thread.Sleep(1000);
                        }
                    }
                    this.Dispatcher.BeginInvoke(new SimpleCallback(RenderFinish));
                    System.Threading.Thread.Sleep(3000);
                    this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                }
                catch (Exception ex)
                {
                    this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), ex.Message);
                    this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                }
            }
        }


        #region Private Methods

        private void AddFillerRows(DataTable table)
        {
            bool recreateTable = false;
            string columnType = table.Columns[0].DataType.ToString();
            if (columnType == "System.DateTime")
            {
                DateTime minDate = ((DateTime)table.Rows[0][0]);
                DateTime maxDate = ((DateTime)table.Rows[table.Rows.Count - 1][0]);
                DataColumn[] pkColumns = new DataColumn[1];
                pkColumns[0] = table.Columns[0];
                table.PrimaryKey = pkColumns;
                DateTime curDate = minDate;

                while (curDate < maxDate)
                {
                    curDate = curDate.AddDays(1);
                    DataRow row = table.Rows.Find(curDate);
                    if (row == null)
                    {
                        table.Rows.Add(curDate, 0);
                        recreateTable = true;
                    }
                }
            }

            if (recreateTable)
            {
                DataRow[] rows = table.Select(string.Empty, "[" + table.Columns[0].ColumnName + "]", DataViewRowState.CurrentRows);
                table = rows.CopyToDataTable();
            }
        }

        private double CalculateStdDev(IEnumerable<double> values)
        {
            double ret = 0;
            if (values.Count() > 0)
            {
                double avg = values.Average();
                double sum = values.Sum(d => Math.Pow(d - avg, 2));
                ret = Math.Sqrt((sum) / (values.Count() - 1));
            }
            return ret;
        }

        private void NumberBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //e.ControlText
            e.Handled = !Util.IsWholeNumber(e.Text);
            base.OnPreviewTextInput(e);
        }



        /// <summary>
        /// Clears the gadget's output
        /// </summary>
        private void ClearResults()
        {
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;
            messagePanel.Text = string.Empty;
            descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;

            panelMain.Children.Clear();

            StrataPanelList.Clear();
            StrataGridList.Clear();
            StrataExpanderList.Clear();
        }

        private void SetGraph(string strataValue, List<SimpleDataValue> actualValues, List<SimpleDataValue> trendValues, List<SimpleDataValue> aberrationValues, List<DataGridRow> aberrationDetails)
        {
            StackPanel chartPanel = new StackPanel();
            chartPanel.Margin = (Thickness)this.Resources["genericElementMargin"];
            chartPanel.Visibility = System.Windows.Visibility.Collapsed;
            StrataPanelList.Add(chartPanel);

            if (GadgetOptions.StrataVariableNames.Count == 0)
            {
                panelMain.Children.Add(chartPanel);
            }
            else
            {
                TextBlock txtExpanderHeader = new TextBlock();
                txtExpanderHeader.Text = strataValue;
                txtExpanderHeader.Style = this.Resources["genericOutputExpanderText"] as Style;

                Expander expander = new Expander();
                expander.Margin = (Thickness)this.Resources["expanderMargin"]; //new Thickness(6, 2, 6, 6);
                expander.IsExpanded = true;
                expander.Header = txtExpanderHeader;
                expander.Visibility = System.Windows.Visibility.Collapsed;

                expander.Content = chartPanel;
                chartPanel.Tag = txtExpanderHeader.Text;
                panelMain.Children.Add(expander);
                StrataExpanderList.Add(expander);
            }

            Chart chart = new Chart();

            LinearAxis dependentAxis = new LinearAxis();
            dependentAxis.Orientation = AxisOrientation.Y;
            dependentAxis.Minimum = 0;
            dependentAxis.ShowGridLines = true;

            CategoryAxis independentAxis = new CategoryAxis();
            independentAxis.Orientation = AxisOrientation.X;
            independentAxis.SortOrder = CategorySortOrder.Ascending;
            independentAxis.AxisLabelStyle = Resources["RotateAxisAberrationStyle"] as Style;

            chart.PlotAreaStyle = new Style();
            chart.PlotAreaStyle.Setters.Add(new Setter(Chart.BackgroundProperty, Brushes.White));

            dependentAxis.GridLineStyle = new Style();
            dependentAxis.GridLineStyle.Setters.Add(new Setter(Line.StrokeProperty, Brushes.LightGray));

            //System.Windows.Controls.DataVisualization.Charting.Chart.BackgroundProperty

            try
            {
                independentAxis.AxisLabelStyle.Setters.Add(new Setter(AxisLabel.StringFormatProperty, "{0:d}"));
            }
            catch (Exception)
            {
                //already added
            }

            LineSeries series1 = new LineSeries();
            series1.IndependentValuePath = "IndependentValue";
            series1.DependentValuePath = "DependentValue";
            series1.ItemsSource = actualValues;
            series1.Title = DashboardSharedStrings.GADGET_ABERRATION_ACTUAL;
            series1.DependentRangeAxis = dependentAxis;
            series1.IndependentAxis = independentAxis;

            LineSeries series2 = new LineSeries();
            series2.IndependentValuePath = "IndependentValue";
            series2.DependentValuePath = "DependentValue";
            series2.ItemsSource = trendValues;
            series2.Title = DashboardSharedStrings.GADGET_ABERRATION_EXPECTED;
            series2.DependentRangeAxis = dependentAxis;
            series2.IndependentAxis = independentAxis;
            series2.PolylineStyle = Resources["GooglePolylineAberrationStyle"] as Style;
            series2.DataPointStyle = Resources["GoogleDataPointAberrationStyle"] as Style;

            ScatterSeries series3 = new ScatterSeries();
            series3.IndependentValuePath = "IndependentValue";
            series3.DependentValuePath = "DependentValue";
            series3.ItemsSource = aberrationValues;
            series3.Title = DashboardSharedStrings.GADGET_ABERRATION;
            series3.DependentRangeAxis = dependentAxis;
            series3.IndependentAxis = independentAxis;
            series3.DataPointStyle = Resources["DataPointAberrationStyle"] as Style;

            chart.Series.Add(series1);
            chart.Series.Add(series3);
            chart.Series.Add(series2);
            chart.Height = 400;


            //if (actualValues.Count > 37)
            //{
            //    chart.Width = (actualValues.Count * (871.0 / 37.0)) + 129;
            //}
            //else
            //{
            //    chart.Width = 1000;
            //}
            chart.BorderThickness = new Thickness(0);
            chart.Margin = new Thickness(6, -20, 6, 6);
            //chart.Width = (actualValues.Count * (871.0 / 37.0)) + 539;
            //Label title = new Label();
            //title.Content = strataValue;
            //title.Margin = new Thickness(10);
            //title.FontWeight = FontWeights.Bold;

            //panelMain.Children.Add(title);
            //panelMain.Children.Add(chart);
            ScrollViewer sv = new ScrollViewer();
            sv.MaxHeight = 400;
            sv.MaxWidth = System.Windows.SystemParameters.PrimaryScreenWidth - 125;
            //if (actualValues.Count > 37)
            //{
            chart.Width = (actualValues.Count * (871.0 / 37.0)) + 229;
            //}
            //else
            //{
            //    sv.Width = 900;
            //}
            sv.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            //sv.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            sv.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left;
            sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            sv.Content = chart;

            chartPanel.Children.Add(sv);

            if (aberrationDetails.Count == 0)
            {
                Label noAbberration = new Label();
                noAbberration.Content = DashboardSharedStrings.GADGET_NO_ABERRATIONS_FOUND;
                noAbberration.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                noAbberration.Margin = new Thickness(0, 0, 0, 0);
                noAbberration.FontWeight = FontWeights.Bold;
                noAbberration.Foreground = Brushes.Red;
                //panelMain.Children.Add(noAbberration);
                chartPanel.Children.Add(noAbberration);
            }
            else
            {
                Label abberrationFound = new Label();
                abberrationFound.Content = DashboardSharedStrings.GADGET_ABERRATIONS_FOUND;
                abberrationFound.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                abberrationFound.Margin = new Thickness(0, 0, 0, 5);
                abberrationFound.FontWeight = FontWeights.Bold;
                abberrationFound.Foreground = Brushes.Red;
                //panelMain.Children.Add(abberrationFound);
                chartPanel.Children.Add(abberrationFound);

                Grid grid = new Grid();
                grid.SnapsToDevicePixels = true;
                grid.HorizontalAlignment = HorizontalAlignment.Center;
                grid.Margin = new Thickness(0, 0, 0, 0);

                ColumnDefinition column1 = new ColumnDefinition();
                ColumnDefinition column2 = new ColumnDefinition();
                ColumnDefinition column3 = new ColumnDefinition();
                ColumnDefinition column4 = new ColumnDefinition();

                column1.Width = GridLength.Auto;
                column2.Width = GridLength.Auto;
                column3.Width = GridLength.Auto;
                column4.Width = GridLength.Auto;

                grid.ColumnDefinitions.Add(column1);
                grid.ColumnDefinitions.Add(column2);
                grid.ColumnDefinitions.Add(column3);
                grid.ColumnDefinitions.Add(column4);

                RowDefinition rowDefHeader = new RowDefinition();
                rowDefHeader.Height = new GridLength(25);
                grid.RowDefinitions.Add(rowDefHeader); //grdFreq.RowDefinitions.Add(rowDefHeader);

                for (int y = 0; y < /*grdFreq*/grid.ColumnDefinitions.Count; y++)
                {
                    Rectangle rctHeader = new Rectangle();
                    rctHeader.Style = this.Resources["gridHeaderCellRectangle"] as Style;
                    Grid.SetRow(rctHeader, 0);
                    Grid.SetColumn(rctHeader, y);
                    grid.Children.Add(rctHeader); //grdFreq.Children.Add(rctHeader);
                }

                TextBlock txtValHeader = new TextBlock();
                txtValHeader.Text = DashboardSharedStrings.GADGET_ABERRATION_DATE;
                txtValHeader.Style = this.Resources["columnHeadingText"] as Style;
                Grid.SetRow(txtValHeader, 0);
                Grid.SetColumn(txtValHeader, 0);
                grid.Children.Add(txtValHeader); //grdFreq.Children.Add(txtValHeader);

                TextBlock txtFreqHeader = new TextBlock();
                txtFreqHeader.Text = DashboardSharedStrings.COUNT;
                txtFreqHeader.Style = this.Resources["columnHeadingText"] as Style;
                Grid.SetRow(txtFreqHeader, 0);
                Grid.SetColumn(txtFreqHeader, 1);
                grid.Children.Add(txtFreqHeader); //grdFreq.Children.Add(txtFreqHeader);

                TextBlock txtPctHeader = new TextBlock();
                txtPctHeader.Text = DashboardSharedStrings.GADGET_ABERRATION_EXPECTED;
                txtPctHeader.Style = this.Resources["columnHeadingText"] as Style;
                Grid.SetRow(txtPctHeader, 0);
                Grid.SetColumn(txtPctHeader, 2);
                grid.Children.Add(txtPctHeader); //grdFreq.Children.Add(txtPctHeader);

                TextBlock txtAccuHeader = new TextBlock();
                txtAccuHeader.Text = DashboardSharedStrings.GADGET_ABERRATION_DIFF;
                txtAccuHeader.Style = this.Resources["columnHeadingText"] as Style;
                Grid.SetRow(txtAccuHeader, 0);
                Grid.SetColumn(txtAccuHeader, 3);
                grid.Children.Add(txtAccuHeader);

                //panelMain.Children.Add(grid);
                chartPanel.Children.Add(grid);

                int rowcount = 1;
                foreach (DataGridRow aberrationDetail in aberrationDetails)
                {
                    RowDefinition rowDef = new RowDefinition();
                    grid.RowDefinitions.Add(rowDef);

                    TextBlock txtDate = new TextBlock();
                    txtDate.Text = aberrationDetail.Date.ToShortDateString();
                    txtDate.Margin = new Thickness(2);
                    txtDate.VerticalAlignment = VerticalAlignment.Center;
                    txtDate.HorizontalAlignment = HorizontalAlignment.Center;
                    Grid.SetRow(txtDate, rowcount);
                    Grid.SetColumn(txtDate, 0);
                    grid.Children.Add(txtDate);

                    TextBlock txtFreq = new TextBlock();
                    txtFreq.Text = aberrationDetail.Frequency.ToString();
                    txtFreq.Margin = new Thickness(2);
                    txtFreq.VerticalAlignment = VerticalAlignment.Center;
                    txtFreq.HorizontalAlignment = HorizontalAlignment.Center;
                    Grid.SetRow(txtFreq, rowcount);
                    Grid.SetColumn(txtFreq, 1);
                    grid.Children.Add(txtFreq);

                    TextBlock txtAvg = new TextBlock();
                    txtAvg.Text = aberrationDetail.RunningAverage.ToString("N2");
                    txtAvg.Margin = new Thickness(2);
                    txtAvg.VerticalAlignment = VerticalAlignment.Center;
                    txtAvg.HorizontalAlignment = HorizontalAlignment.Center;
                    Grid.SetRow(txtAvg, rowcount);
                    Grid.SetColumn(txtAvg, 2);
                    grid.Children.Add(txtAvg);

                    TextBlock txtDelta = new TextBlock();
                    txtDelta.Text = "  " + aberrationDetail.Delta.ToString("N2") + " standard deviations  ";

                    StackPanel pnl = new StackPanel();
                    pnl.Orientation = Orientation.Horizontal;
                    pnl.VerticalAlignment = VerticalAlignment.Center;
                    pnl.HorizontalAlignment = HorizontalAlignment.Center;
                    pnl.Children.Add(txtDelta);

                    Grid.SetRow(pnl, rowcount);
                    Grid.SetColumn(pnl, 3);
                    grid.Children.Add(pnl);

                    rowcount++;
                }

                int rdcount = 0;
                foreach (RowDefinition rd in grid.RowDefinitions)
                {
                    int cdcount = 0;
                    foreach (ColumnDefinition cd in grid.ColumnDefinitions)
                    {
                        //Rectangle rctBorder = new Rectangle();
                        //rctBorder.Stroke = Brushes.Black;
                        //Grid.SetRow(rctBorder, rdcount);
                        //Grid.SetColumn(rctBorder, cdcount);
                        //grid.Children.Add(rctBorder);
                        //cdcount++;
                        Border border = new Border();
                        border.Style = this.Resources["gridCellBorder"] as Style;

                        if (rdcount == 0)
                        {
                            border.BorderThickness = new Thickness(border.BorderThickness.Left, border.BorderThickness.Bottom, border.BorderThickness.Right, border.BorderThickness.Bottom);
                        }
                        if (cdcount == 0)
                        {
                            border.BorderThickness = new Thickness(border.BorderThickness.Right, border.BorderThickness.Top, border.BorderThickness.Right, border.BorderThickness.Bottom);
                        }

                        Grid.SetRow(border, rdcount);
                        Grid.SetColumn(border, cdcount);
                        grid.Children.Add(border);
                        cdcount++;
                    }
                    rdcount++;
                }
            }
        }

        protected override void RenderFinish()
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed;
            panelMain.Visibility = System.Windows.Visibility.Visible;

            foreach (Expander expander in this.StrataExpanderList)
            {
                expander.Visibility = System.Windows.Visibility.Visible;
            }

            foreach (StackPanel panel in this.StrataPanelList)
            {
                panel.Visibility = System.Windows.Visibility.Visible;
            }

            messagePanel.MessagePanelType = Controls.MessagePanelType.StatusPanel;
            messagePanel.Text = string.Empty;
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;

            HideConfigPanel();
            CheckAndSetPosition();
        }

        protected override void RenderFinishWithWarning(string errorMessage)
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed;
            panelMain.Visibility = System.Windows.Visibility.Visible;

            foreach (Expander expander in this.StrataExpanderList)
            {
                expander.Visibility = System.Windows.Visibility.Visible;
            }

            foreach (StackPanel panel in this.StrataPanelList)
            {
                panel.Visibility = System.Windows.Visibility.Visible;
            }

            messagePanel.MessagePanelType = Controls.MessagePanelType.WarningPanel;
            messagePanel.Text = errorMessage;
            messagePanel.Visibility = System.Windows.Visibility.Visible;

            HideConfigPanel();
            CheckAndSetPosition();
        }

        protected override void RenderFinishWithError(string errorMessage)
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed;

            messagePanel.MessagePanelType = Controls.MessagePanelType.ErrorPanel;
            messagePanel.Text = errorMessage;
            messagePanel.Visibility = System.Windows.Visibility.Visible;

            panelMain.Children.Clear();

            HideConfigPanel();
            CheckAndSetPosition();
        }

        public override void CollapseOutput()
        {
            panelMain.Visibility = Visibility.Collapsed;

            if (!string.IsNullOrEmpty(this.infoPanel.Text))
            {
                this.infoPanel.Visibility = System.Windows.Visibility.Collapsed;
            }

            this.messagePanel.Visibility = System.Windows.Visibility.Collapsed;
            this.infoPanel.Visibility = System.Windows.Visibility.Collapsed;
            IsCollapsed = true;
        }

        public override void ExpandOutput()
        {
            panelMain.Visibility = Visibility.Visible;

            if (this.messagePanel.MessagePanelType != Controls.MessagePanelType.StatusPanel)
            {
                this.messagePanel.Visibility = System.Windows.Visibility.Visible;
            }

            if (!string.IsNullOrEmpty(this.infoPanel.Text))
            {
                this.infoPanel.Visibility = System.Windows.Visibility.Visible;
            }

            if (!string.IsNullOrEmpty(this.descriptionPanel.Text))
            {
                descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.DisplayMode;
            }
            IsCollapsed = false;
        }

        /// <summary>
        /// Closes the gadget
        /// </summary>
        protected override void CloseGadget()
        {
            if (worker != null && worker.WorkerSupportsCancellation)
            {
                worker.CancelAsync();
            }

            if (worker != null)
            {
                worker.DoWork -= new System.ComponentModel.DoWorkEventHandler(worker_DoWork);
                worker.RunWorkerCompleted -= new System.ComponentModel.RunWorkerCompletedEventHandler(worker_WorkerCompleted);
            }
            if (baseWorker != null)
            {
                baseWorker.DoWork -= new System.ComponentModel.DoWorkEventHandler(Execute);
            }

            this.GadgetStatusUpdate -= new GadgetStatusUpdateHandler(RequestUpdateStatusMessage);
            this.GadgetCheckForCancellation -= new GadgetCheckForCancellationHandler(IsCancelled);

            for (int i = 0; i < StrataExpanderList.Count; i++)
            {
                StrataExpanderList[i].Content = null;
            }
            this.StrataPanelList.Clear();
            this.StrataExpanderList.Clear();
            this.panelMain.Children.Clear();

            base.CloseGadget();

            GadgetOptions = null;
        }

        #endregion

        #region Private Properties
        private View View
        {
            get
            {
                return this.DashboardHelper.View;
            }
        }

        private IDbDriver Database
        {
            get
            {
                return this.DashboardHelper.Database;
            }
        }
        #endregion // Private Properties

        #region IGadget Members

        /// <summary>
        /// Sets the gadget to its 'processing' state
        /// </summary>
        public override void SetGadgetToProcessingState()
        {
            this.IsProcessing = true;
        }

        /// <summary>
        /// Sets the gadget to its 'finished' state
        /// </summary>
        public override void SetGadgetToFinishedState()
        {
            this.IsProcessing = false;
            base.SetGadgetToFinishedState();
        }

        public override void RefreshResults()
        {
            AberrationDetectionChartParameters aberrationParameters = (AberrationDetectionChartParameters)Parameters;
            if(Parameters != null && aberrationParameters.ColumnNames.Count > 0)
            {
                if (!String.IsNullOrEmpty(aberrationParameters.ColumnNames[0]) && (!String.IsNullOrEmpty(aberrationParameters.CrosstabVariableName)))
                {
                    infoPanel.Visibility = System.Windows.Visibility.Collapsed;
                    waitPanel.Visibility = System.Windows.Visibility.Visible;
                    messagePanel.MessagePanelType = Controls.MessagePanelType.StatusPanel;
                    descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;

                    baseWorker = new BackgroundWorker();
                    baseWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(Execute);
                    baseWorker.RunWorkerAsync();

                    base.RefreshResults();
                }
            }
        }

        /// <summary>
        /// Updates the variable names available in the gadget's properties
        /// </summary>
        public override void UpdateVariableNames()
        {
            //FillComboboxes(true);
        }

        /// <summary>
        /// Generates Xml representation of this gadget
        /// </summary>
        /// <param name="doc">The Xml docment</param>
        /// <returns>XmlNode</returns>
        public override XmlNode Serialize(XmlDocument doc)
        {
            AberrationDetectionChartParameters aberrationParameters = (AberrationDetectionChartParameters)Parameters;

            System.Xml.XmlElement element = doc.CreateElement("aberrationDetectionGadget"); //Note: 7.1.3.0 has this as "frequencyGadget"
            string xmlString = string.Empty;
            element.InnerXml = xmlString;
            element.AppendChild(SerializeFilters(doc));

            System.Xml.XmlAttribute id = doc.CreateAttribute("id");
            System.Xml.XmlAttribute locationY = doc.CreateAttribute("top");
            System.Xml.XmlAttribute locationX = doc.CreateAttribute("left");
            System.Xml.XmlAttribute collapsed = doc.CreateAttribute("collapsed");
            System.Xml.XmlAttribute type = doc.CreateAttribute("gadgetType");

            id.Value = this.UniqueIdentifier.ToString();
            locationY.Value = Canvas.GetTop(this).ToString("F0");
            locationX.Value = Canvas.GetLeft(this).ToString("F0");
            collapsed.Value = IsCollapsed.ToString();
            type.Value = "EpiDashboard.AberrationControl";

            element.Attributes.Append(locationY);
            element.Attributes.Append(locationX);
            element.Attributes.Append(collapsed);
            element.Attributes.Append(type);
            element.Attributes.Append(id);

            CustomOutputHeading = aberrationParameters.GadgetTitle;
            CustomOutputDescription = aberrationParameters.GadgetDescription;

            XmlElement mainVarElement = doc.CreateElement("mainVariable");
            if (aberrationParameters.ColumnNames.Count > 0)
            {
                if (!String.IsNullOrEmpty(aberrationParameters.ColumnNames[0].ToString()))
                {
                    mainVarElement.InnerText = aberrationParameters.ColumnNames[0].ToString();
                    element.AppendChild(mainVarElement);
                }
            }

            XmlElement StrataVariableNameElement = doc.CreateElement("strataVariable");
            XmlElement StrataVariableNamesElement = doc.CreateElement("strataVariables");
            if (aberrationParameters.StrataVariableNames.Count == 1)
            {
                StrataVariableNameElement.InnerText = aberrationParameters.StrataVariableNames[0].ToString();
                element.AppendChild(StrataVariableNameElement);
            }
            else if (aberrationParameters.StrataVariableNames.Count > 1)
            {
                foreach (string strataColumn in aberrationParameters.StrataVariableNames)
                {
                    XmlElement strataElement = doc.CreateElement("strataVariable");
                    strataElement.InnerText = strataColumn.Replace("<", "&lt;");
                    StrataVariableNamesElement.AppendChild(strataElement);
                }

                element.AppendChild(StrataVariableNamesElement);
            }

            XmlElement weightVarElement = doc.CreateElement("weightVar");
            if (!String.IsNullOrEmpty(aberrationParameters.WeightVariableName.ToString()))
            {
                weightVarElement.InnerText = aberrationParameters.WeightVariableName.ToString();
                element.AppendChild(weightVarElement);
            }

            //if (GadgetOptions.InputVariableList.ContainsKey("sort"))
            //{
            //    sort = GadgetOptions.InputVariableList["sort"];
            //}

            XmlElement lagtimeElement = doc.CreateElement("lagtime");
            if (!String.IsNullOrEmpty(aberrationParameters.LagTime.ToString()))
            {
                lagtimeElement.InnerText = aberrationParameters.LagTime.ToString();
                element.AppendChild(lagtimeElement);
            }

            XmlElement deviationsVarElement = doc.CreateElement("deviations");
            if (!String.IsNullOrEmpty(aberrationParameters.Deviations.ToString()))
            {
                deviationsVarElement.InnerText = aberrationParameters.Deviations.ToString();
                element.AppendChild(deviationsVarElement);
            }

            XmlElement timeperiodVarElement = doc.CreateElement("timeperiod");
            if (!String.IsNullOrEmpty(aberrationParameters.TimePeriod.ToString()))
            {
                timeperiodVarElement.InnerText = aberrationParameters.TimePeriod.ToString();
                element.AppendChild(timeperiodVarElement);
            }

            SerializeAnchors(element);
            return element;
        }

        /// <summary>
        /// Creates the frequency gadget from an Xml element
        /// </summary>
        /// <param name="element">The element from which to create the gadget</param>
        public override void CreateFromXml(XmlElement element)
        {
            this.LoadingCombos = true;
            this.Parameters = new AberrationDetectionChartParameters();

            HideConfigPanel();
            infoPanel.Visibility = System.Windows.Visibility.Collapsed;
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;

            foreach (XmlElement child in element.ChildNodes)
            {
                switch (child.Name.ToLowerInvariant())
                {
                    case "mainvariable":
                        ((AberrationDetectionChartParameters)Parameters).ColumnNames.Add(child.InnerText.Replace("&lt;", "<"));
                        break;
                    case "stratavariable":
                        if (!string.IsNullOrEmpty(child.InnerText))
                        {
                            if (((AberrationDetectionChartParameters)Parameters).StrataVariableNames.Count == 0)
                            {
                                ((AberrationDetectionChartParameters)Parameters).StrataVariableNames.Add(child.InnerText.Replace("&lt;", "<"));
                            }
                            else
                            {
                                ((AberrationDetectionChartParameters)Parameters).StrataVariableNames[0] = child.InnerText.Replace("&lt;", "<");
                            }
                        }                       
                        break;
                    case "stratavariables":
                        foreach (XmlElement field in child.ChildNodes)
                        {
                            List<string> fields = new List<string>();
                            if (field.Name.ToLowerInvariant().Equals("stratavariable"))
                            {
                                ((AberrationDetectionChartParameters)Parameters).StrataVariableNames.Add(field.InnerText.Replace("&lt;", "<"));
                            }
                        }
                        break;
                    case "weightvariable":
                        ((AberrationDetectionChartParameters)Parameters).WeightVariableName = child.InnerText.Replace("&lt;", "<");
                        break;
                    case "lagtime":
                        ((AberrationDetectionChartParameters)Parameters).LagTime = child.InnerText.Replace("&lt;", "<");
                        break;
                    case "deviations":
                        ((AberrationDetectionChartParameters)Parameters).Deviations = child.InnerText.Replace("&lt;", "<");
                        break;
                    case "timeperiod":
                        ((AberrationDetectionChartParameters)Parameters).TimePeriod = child.InnerText.Replace("&lt;", "<");
                        break;
                    case "datafilters":
                        this.DataFilters = new DataFilters(this.DashboardHelper);
                        this.DataFilters.CreateFromXml(child);
                        break;
                    case "customheading":
                        if (!string.IsNullOrEmpty(child.InnerText) && !child.InnerText.Equals("(none)"))
                        {
                            this.CustomOutputHeading = child.InnerText.Replace("&lt;", "<"); ;
                            Parameters.GadgetTitle = CustomOutputHeading;
                        }
                        break;
                    case "customdescription":
                        if (!string.IsNullOrEmpty(child.InnerText) && !child.InnerText.Equals("(none)"))
                        {
                            this.CustomOutputDescription = child.InnerText.Replace("&lt;", "<");
                            Parameters.GadgetDescription = CustomOutputDescription;
                            if (!string.IsNullOrEmpty(CustomOutputDescription) && !CustomOutputHeading.Equals("(none)"))
                            {
                                descriptionPanel.Text = CustomOutputDescription;
                                descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.DisplayMode;
                            }
                            else
                            {
                                descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;
                            }
                        }
                        break;
                }
            }

            base.CreateFromXml(element);

            this.LoadingCombos = false;

            RefreshResults();
        }

        /// <summary>
        /// Converts the gadget's output to Html
        /// </summary>
        /// <returns></returns>
        public override string ToHTML(string htmlFileName = "", int count = 0, bool useAlternatingColors = false)
        {
            if (IsCollapsed) return string.Empty;

            StringBuilder htmlBuilder = new StringBuilder();

            if (CustomOutputHeading == null || (string.IsNullOrEmpty(CustomOutputHeading) && !CustomOutputHeading.Equals("(none)")))
            {
                htmlBuilder.AppendLine("<h2 class=\"gadgetHeading\">EARS Aberration</h2>");
            }
            else if (CustomOutputHeading != "(none)")
            {
                htmlBuilder.AppendLine("<h2 class=\"gadgetHeading\">" + CustomOutputHeading + "</h2>");
            }

            int aberrationChartCount = 0;

            string imageFileName = string.Empty;

            if (htmlFileName.EndsWith(".html"))
            {
                imageFileName = htmlFileName.Remove(htmlFileName.Length - 5, 5);
            }
            else if (htmlFileName.EndsWith(".htm"))
            {
                imageFileName = htmlFileName.Remove(htmlFileName.Length - 4, 4);
            }

            if (!string.IsNullOrEmpty(CustomOutputDescription))
            {
                htmlBuilder.AppendLine("<p class=\"gadgetsummary\">" + CustomOutputDescription + "</p>");
            }

            if (!string.IsNullOrEmpty(messagePanel.Text) && messagePanel.Visibility == Visibility.Visible)
            {
                htmlBuilder.AppendLine("<p><small><strong>" + messagePanel.Text + "</strong></small></p>");
            }

            if (!string.IsNullOrEmpty(infoPanel.Text) && infoPanel.Visibility == Visibility.Visible)
            {
                htmlBuilder.AppendLine("<p><small><strong>" + infoPanel.Text + "</strong></small></p>");
            }

            foreach (StackPanel strataPanel in this.StrataPanelList)
            {
                if (strataPanel.Tag != null && strataPanel.Tag is string)
                {
                    string header = strataPanel.Tag.ToString();
                    htmlBuilder.AppendLine("<h3>" + header + "</h3>");
                }

                foreach (UIElement control in strataPanel.Children)
                {
                    if (control is Label)
                    {
                        Label label = control as Label;
                        if (label.Content.ToString().ToLowerInvariant().Contains("aberrations found"))
                        {
                            htmlBuilder.AppendLine("<p><span class=\"warning\">" + label.Content + "</span></p>");
                        }
                        else
                        {
                            htmlBuilder.AppendLine("<p><span class=\"bold\">" + label.Content + "</span></p>");
                        }
                    }
                    else if (control is ScrollViewer && (control as ScrollViewer).Content is Chart)
                    {
                        imageFileName = string.Empty;

                        if (htmlFileName.EndsWith(".html"))
                        {
                            imageFileName = htmlFileName.Remove(htmlFileName.Length - 5, 5);
                        }
                        else if (htmlFileName.EndsWith(".htm"))
                        {
                            imageFileName = htmlFileName.Remove(htmlFileName.Length - 4, 4);
                        }

                        imageFileName = imageFileName + "_" + count.ToString() + "_" + aberrationChartCount.ToString() + ".png";

                        Chart chart = (control as ScrollViewer).Content as Chart;
                        BitmapSource img = (BitmapSource)Common.ToImageSource(chart);
                        FileStream stream = new FileStream(imageFileName, FileMode.Create);
                        PngBitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(img));
                        encoder.Save(stream);
                        stream.Close();

                        htmlBuilder.AppendLine("<img src=\"" + imageFileName + "\" alt=\"aberration graph\" />");

                        aberrationChartCount++;
                    }
                    else if (control is Grid)
                    {
                        Grid grid = control as Grid;

                        htmlBuilder.AppendLine("<div style=\"height: 7px;\"></div>");
                        htmlBuilder.AppendLine("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\">");

                        foreach (UIElement element in grid.Children)
                        {
                            if (element is TextBlock)
                            {
                                int rowNumber = Grid.GetRow(element);
                                int columnNumber = Grid.GetColumn(element);

                                string tableDataTagOpen = "<td>";
                                string tableDataTagClose = "</td>";

                                if (rowNumber == 0)
                                {
                                    tableDataTagOpen = "<th>";
                                    tableDataTagClose = "</th>";
                                }

                                if (columnNumber == 0)
                                {
                                    htmlBuilder.AppendLine("<tr>");
                                }
                                if (columnNumber == 0 && rowNumber > 0)
                                {
                                    tableDataTagOpen = "<td class=\"value\">";
                                }

                                string value = ((TextBlock)element).Text;
                                string formattedValue = value;

                                htmlBuilder.AppendLine(tableDataTagOpen + formattedValue + tableDataTagClose);

                                if (columnNumber >= grid.ColumnDefinitions.Count - 1)
                                {
                                    htmlBuilder.AppendLine("</tr>");
                                }
                            }

                            else if (element is StackPanel)
                            {
                                int rowNumber = Grid.GetRow(element);
                                int columnNumber = Grid.GetColumn(element);

                                string tableDataTagOpen = "<td>";
                                string tableDataTagClose = "</td>";

                                if (rowNumber == 0)
                                {
                                    tableDataTagOpen = "<th>";
                                    tableDataTagClose = "</th>";
                                }

                                if (columnNumber == 0)
                                {
                                    htmlBuilder.AppendLine("<tr>");
                                }
                                if (columnNumber == 0 && rowNumber > 0)
                                {
                                    tableDataTagOpen = "<td class=\"value\">";
                                }

                                StackPanel panel = element as StackPanel;

                                string value = string.Empty;

                                foreach (UIElement panelElement in panel.Children)
                                {
                                    if (panelElement is TextBlock)
                                    {
                                        value = value + ((TextBlock)panelElement).Text;
                                    }
                                }

                                string formattedValue = value;

                                htmlBuilder.AppendLine(tableDataTagOpen + formattedValue + tableDataTagClose);

                                if (columnNumber >= grid.ColumnDefinitions.Count - 1)
                                {
                                    htmlBuilder.AppendLine("</tr>");
                                }
                            }
                        }

                        htmlBuilder.AppendLine("</table>");
                        htmlBuilder.AppendLine("<div style=\"height: 17px;\"></div>");
                    }
                }
            }

            return htmlBuilder.ToString();
        }

        private string customOutputHeading;
        private string customOutputDescription;
        private string customOutputCaption;

        /// <summary>
        /// Gets/sets the gadget's custom output heading
        /// </summary>
        public override string CustomOutputHeading
        {
            get
            {
                return this.customOutputHeading;
            }
            set
            {
                this.customOutputHeading = value;
                headerPanel.Text = CustomOutputHeading;
            }
        }

        /// <summary>
        /// Gets/sets the gadget's custom output description
        /// </summary>
        public override string CustomOutputDescription
        {
            get
            {
                return this.customOutputDescription;
            }
            set
            {
                this.customOutputDescription = value;
                descriptionPanel.Text = CustomOutputDescription;
            }
        }

        public override string CustomOutputCaption
        {
            get
            {
                return this.customOutputCaption;
            }
            set
            {
                this.customOutputCaption = value;
            }
        }

        #endregion

        /// <summary>
        /// Returns the gadget's description as a string.
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            return "Aberration Detection Gadget";
        }

        public ImageSource ToImageSource(FrameworkElement obj)
        {
            // Save current canvas transform
            Transform transform = obj.LayoutTransform;

            // fix margin offset as well
            Thickness margin = obj.Margin;
            obj.Margin = new Thickness(0, 0,
                 margin.Right - margin.Left, margin.Bottom - margin.Top);

            // Get the size of canvas
            Size size = new Size(obj.ActualWidth, obj.ActualHeight);

            // force control to Update
            obj.Measure(size);
            obj.Arrange(new Rect(size));

            RenderTargetBitmap bmp = new RenderTargetBitmap(
                (int)obj.ActualWidth, (int)obj.ActualHeight, 96, 96, PixelFormats.Pbgra32);

            bmp.Render(obj);

            // return values as they were before
            obj.LayoutTransform = transform;
            obj.Margin = margin;
            return bmp;
        }
    }
}
