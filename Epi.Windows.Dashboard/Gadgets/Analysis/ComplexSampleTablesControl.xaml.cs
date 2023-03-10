using System;
using System.Data;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows;
using System.Windows.Controls;
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
using Epi.WPF.Dashboard;
using Epi.WPF.Dashboard.Rules;

namespace Epi.WPF.Dashboard.Gadgets.Analysis
{
    /// <summary>
    /// Interaction logic for ComplexSampleTablesControl.xaml
    /// </summary>
    public partial class ComplexSampleTablesControl : GadgetBase
    {
        #region Private Members
        /// <summary>
        /// A custom heading to use for this gadget's output
        /// </summary>
        private string customOutputHeading;

        /// <summary>
        /// A custom description to use for this gadget's output
        /// </summary>
        private string customOutputDescription;

        /// <summary>
        /// A custom caption to use for this gadget's table/image output, if applicable
        /// </summary>
        private string customOutputCaption;

        /// <summary>
        /// Whether the main variable is a drop-down list field
        /// </summary>
        private bool isDropDownList = false;

        /// <summary>
        /// Whether the main variable is a comment legal field
        /// </summary>
        private bool isCommentLegal = false;

        /// <summary>
        /// Whether the main variable is an option field
        /// </summary>
        private bool isOptionField = false;

        /// <summary>
        /// Whether the main variable is a recoded variable
        /// </summary>
        private bool isRecoded = false;
                
        /// <summary>
        /// A data structure used for storing the 95% confidence intervals
        /// </summary>
        private struct ConfLimit
        {
            public string Value;
            public double Upper;
            public double Lower;
        }

        #endregion // Private Members

        #region Delegates
        private delegate void CreateComplexSampleTableGridDelegate(StatisticsRepository.ComplexSampleTables.CSTablesResults results);
        #endregion // Delegates

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public ComplexSampleTablesControl()
        {
            InitializeComponent();
            Construct();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dashboardHelper">The dashboard helper object to attach</param>
        public ComplexSampleTablesControl(DashboardHelper dashboardHelper)
        {
            InitializeComponent();
            this.dashboardHelper = dashboardHelper;
            Construct();
            FillComboboxes();
        }

        #endregion // Constructors

        #region Private Methods
        /// <summary>
        /// Copies a grid's output to the clipboard
        /// </summary>
        protected override void CopyToClipboard()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Grid grid in this.strataGridList)
            {
                string gridName = grid.Tag.ToString();
                if (strataGridList.Count > 1)
                {
                    sb.AppendLine(grid.Tag.ToString());
                }

                sb.AppendLine(GadgetOptions.MainVariableName + " * " + GadgetOptions.CrosstabVariableName);

                sb.Append("\t");
                for (int j = 1; j < grid.ColumnDefinitions.Count - 1; j++)
                {
                    IEnumerable<UIElement> elements = grid.Children.Cast<UIElement>().Where(x => Grid.GetRow(x) == 1 && Grid.GetColumn(x) == j);
                    TextBlock txt = null;
                    foreach (UIElement element in elements)
                    {
                        if (element is TextBlock)
                        {
                            txt = element as TextBlock;
                        }
                    }

                    string value = string.Empty;

                    if (txt != null)
                    {
                        value = txt.Text;
                    }

                    sb.Append(value + "\t");
                }

                sb.AppendLine();

                for (int i = 2; i < grid.RowDefinitions.Count; i++)
                {
                    for (int j = 0; j < grid.ColumnDefinitions.Count; j++)
                    {
                        IEnumerable<UIElement> elements = grid.Children.Cast<UIElement>().Where(x => Grid.GetRow(x) == i && Grid.GetColumn(x) == j);
                        TextBlock txt = null;
                        foreach (UIElement element in elements)
                        {
                            if (element is TextBlock)
                            {
                                txt = element as TextBlock;
                            }
                        }

                        string value = string.Empty;

                        if (txt != null)
                        {
                            value = txt.Text;
                        }

                        sb.Append(value + "\t");

                        if (j >= grid.ColumnDefinitions.Count - 1)
                        {
                            sb.AppendLine();
                        }
                    }
                }

                sb.AppendLine();
            }
            Clipboard.Clear();
            Clipboard.SetText(sb.ToString());
        }

        /// <summary>
        /// Handles the filling of the gadget's combo boxes
        /// </summary>
        private void FillComboboxes(bool update = false)
        {
            LoadingCombos = true;

            string prevField = string.Empty;
            string prevWeightField = string.Empty;
            string prevStrataField = string.Empty;
            string prevPSUField = string.Empty;
            string prevOutcomeField = string.Empty;

            if (update)
            {
                if (cbxField.SelectedIndex >= 0)
                {
                    prevField = cbxField.SelectedItem.ToString();
                }
                if (cbxFieldWeight.SelectedIndex >= 0)
                {
                    prevWeightField = cbxFieldWeight.SelectedItem.ToString();
                }
                if (cbxFieldStrata.SelectedIndex >= 0)
                {
                    prevStrataField = cbxFieldStrata.SelectedItem.ToString();
                }

                if (cbxFieldPSU.SelectedIndex >= 0)
                {
                    prevPSUField = cbxFieldPSU.SelectedItem.ToString();
                }
                if (cbxFieldOutcome.SelectedIndex >= 0)
                {
                    prevOutcomeField = cbxFieldOutcome.SelectedItem.ToString();
                }
            }

            cbxField.ItemsSource = null;
            cbxField.Items.Clear();

            cbxFieldWeight.ItemsSource = null;
            cbxFieldWeight.Items.Clear();

            cbxFieldStrata.ItemsSource = null;
            cbxFieldStrata.Items.Clear();

            cbxFieldPSU.ItemsSource = null;
            cbxFieldPSU.Items.Clear();

            cbxFieldOutcome.ItemsSource = null;
            cbxFieldOutcome.Items.Clear();

            List<string> fieldNames = new List<string>();
            List<string> weightFieldNames = new List<string>();
            List<string> strataFieldNames = new List<string>();

            weightFieldNames.Add(string.Empty);
            strataFieldNames.Add(string.Empty);

            ColumnDataType columnDataType = ColumnDataType.Boolean | ColumnDataType.Numeric | ColumnDataType.Text | ColumnDataType.UserDefined;
            fieldNames = dashboardHelper.GetFieldsAsList(columnDataType);

            columnDataType = ColumnDataType.Numeric | ColumnDataType.UserDefined;
            weightFieldNames.AddRange(dashboardHelper.GetFieldsAsList(columnDataType));

            columnDataType = ColumnDataType.Numeric | ColumnDataType.Boolean | ColumnDataType.Text | ColumnDataType.UserDefined;
            strataFieldNames.AddRange(dashboardHelper.GetFieldsAsList(columnDataType));

            fieldNames.Sort();
            weightFieldNames.Sort();
            strataFieldNames.Sort();

            if (fieldNames.Contains("SYSTEMDATE"))
            {
                fieldNames.Remove("SYSTEMDATE");
            }

            if (DashboardHelper.IsUsingEpiProject)
            {
                if (fieldNames.Contains("RecStatus")) fieldNames.Remove("RecStatus");
                if (weightFieldNames.Contains("RecStatus")) weightFieldNames.Remove("RecStatus");

                if (strataFieldNames.Contains("RecStatus")) strataFieldNames.Remove("RecStatus");
                if (strataFieldNames.Contains("FKEY")) strataFieldNames.Remove("FKEY");
                if (strataFieldNames.Contains("GlobalRecordId")) strataFieldNames.Remove("GlobalRecordId");
            }

            cbxField.ItemsSource = fieldNames;
            cbxFieldOutcome.ItemsSource = strataFieldNames;
            cbxFieldWeight.ItemsSource = weightFieldNames;
            cbxFieldStrata.ItemsSource = strataFieldNames;
            cbxFieldPSU.ItemsSource = strataFieldNames;

            if (cbxField.Items.Count > 0)
            {
                cbxField.SelectedIndex = -1;
            }
            if (cbxFieldWeight.Items.Count > 0)
            {
                cbxFieldWeight.SelectedIndex = -1;
            }
            if (cbxFieldStrata.Items.Count > 0)
            {
                cbxFieldStrata.SelectedIndex = -1;
            }
            if (cbxFieldOutcome.Items.Count > 0)
            {
                cbxFieldOutcome.SelectedIndex = -1;
            }

            if (update)
            {
                cbxField.SelectedItem = prevField;
                cbxFieldWeight.SelectedItem = prevWeightField;
                cbxFieldStrata.SelectedItem = prevStrataField;
                cbxFieldPSU.SelectedItem = prevPSUField;
                cbxFieldOutcome.SelectedItem = prevOutcomeField;
            }

            LoadingCombos = false;
        }

        /// <summary>
        /// Sets the gadget's state to 'finished' mode
        /// </summary>
        private void RenderFinish()
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed; //waitCursor.Visibility = Visibility.Hidden;

            foreach (Grid freqGrid in strataGridList)
            {
                freqGrid.Visibility = Visibility.Visible;
            }

            messagePanel.MessagePanelType = Controls.MessagePanelType.StatusPanel;
            messagePanel.Text = string.Empty;
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;

            CheckAndSetPosition();
        }

        /// <summary>
        /// Sets the gadget's state to 'finished with warning' mode
        /// </summary>
        /// <remarks>
        /// Common scenario for the usage of this method is when the distinct list of frequency values
        /// exceeds some built-in row limit. The output is limited to prevent the UI from locking up,
        /// and we want to let the user know that the output is limited while still showing them something.
        /// Thus we finish the rendering, but still show a message.
        /// </remarks>
        /// <param name="errorMessage">The warning message to display</param>
        private void RenderFinishWithWarning(string errorMessage)
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed; //waitCursor.Visibility = Visibility.Hidden;

            foreach (Grid freqGrid in strataGridList)
            {
                freqGrid.Visibility = Visibility.Visible;
            }

            messagePanel.MessagePanelType = Controls.MessagePanelType.WarningPanel;
            messagePanel.Text = errorMessage;
            messagePanel.Visibility = System.Windows.Visibility.Visible;

            HideConfigPanel();
            CheckAndSetPosition();
        }

        /// <summary>
        /// Sets the gadget's state to 'finished with error' mode
        /// </summary>
        /// <param name="errorMessage">The error message to display</param>
        private void RenderFinishWithError(string errorMessage)
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed;

            messagePanel.MessagePanelType = Controls.MessagePanelType.ErrorPanel;
            messagePanel.Text = errorMessage;
            messagePanel.Visibility = System.Windows.Visibility.Visible;

            panelMain.Children.Clear();

            HideConfigPanel();
            CheckAndSetPosition();
        }

        /// <summary>
        /// Used to generate the list of variables and options for the GadgetParameters object
        /// </summary> 
        private void CreateInputVariableList()
        {
            Dictionary<string, string> inputVariableList = new Dictionary<string, string>();

            GadgetOptions.MainVariableName = string.Empty;
            GadgetOptions.WeightVariableName = string.Empty;
            GadgetOptions.StrataVariableNames = new List<string>();
            GadgetOptions.CrosstabVariableName = string.Empty;
            GadgetOptions.PSUVariableName = string.Empty;
            GadgetOptions.ColumnNames = new List<string>();

            if (cbxField.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxField.SelectedItem.ToString())
                &&
                cbxFieldPSU.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxFieldPSU.SelectedItem.ToString())
                &&
                cbxFieldOutcome.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxFieldOutcome.SelectedItem.ToString())
                )
            {
                inputVariableList.Add("EXPOSURE_VARIABLE", cbxField.SelectedItem.ToString());
                GadgetOptions.MainVariableName = cbxField.SelectedItem.ToString();
                inputVariableList.Add("PSUVar", cbxFieldPSU.SelectedItem.ToString());
                GadgetOptions.PSUVariableName = cbxFieldPSU.SelectedItem.ToString();
                inputVariableList.Add("OUTCOME_VARIABLE", cbxFieldOutcome.SelectedItem.ToString());
                GadgetOptions.CrosstabVariableName = cbxFieldOutcome.SelectedItem.ToString();
            }
            else
            {
                return;
            }
            if (cbxFieldWeight.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxFieldWeight.SelectedItem.ToString()))
            {
                inputVariableList.Add("WeightVar", cbxFieldWeight.SelectedItem.ToString());
                GadgetOptions.WeightVariableName = cbxFieldWeight.SelectedItem.ToString();
            }
            if (cbxFieldStrata.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxFieldStrata.SelectedItem.ToString()))
            {
                inputVariableList.Add("StratvarList", cbxFieldStrata.SelectedItem.ToString());
                GadgetOptions.StrataVariableNames = new List<string>();
                GadgetOptions.StrataVariableNames.Add(cbxFieldStrata.SelectedItem.ToString());
            }
            GadgetOptions.ShouldIncludeFullSummaryStatistics = false;
            GadgetOptions.InputVariableList = inputVariableList;
        }

        /// <summary>
        /// Used to construct the gadget and assign events
        /// </summary>        
        protected override void Construct()
        {
            if (!string.IsNullOrEmpty(CustomOutputHeading) && !CustomOutputHeading.Equals("(none)"))
            {
                headerPanel.Text = CustomOutputHeading;
            }

            strataGridList = new List<Grid>();
            strataExpanderList = new List<Expander>();            

            mnuCopy.Click += new RoutedEventHandler(mnuCopy_Click);
            mnuSendDataToHTML.Click += new RoutedEventHandler(mnuSendDataToHTML_Click);

#if LINUX_BUILD
            mnuSendDataToExcel.Visibility = Visibility.Collapsed;
#else
            mnuSendDataToExcel.Visibility = Visibility.Visible;
            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.ClassesRoot; Microsoft.Win32.RegistryKey excelKey = key.OpenSubKey("Excel.Application"); bool excelInstalled = excelKey == null ? false : true; key = Microsoft.Win32.Registry.ClassesRoot;
            excelKey = key.OpenSubKey("Excel.Application");
            excelInstalled = excelKey == null ? false : true;

            if (!excelInstalled)
            {
                mnuSendDataToExcel.Visibility = Visibility.Collapsed;
            }
            else
            {
                mnuSendDataToExcel.Click += new RoutedEventHandler(mnuSendDataToExcel_Click);
            }
#endif

            mnuSendToBack.Click += new RoutedEventHandler(mnuSendToBack_Click);
            mnuClose.Click += new RoutedEventHandler(mnuClose_Click);

            this.IsProcessing = false;

            this.GadgetStatusUpdate += new GadgetStatusUpdateHandler(RequestUpdateStatusMessage);
            this.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(IsCancelled);

            base.Construct();
        }

        /// <summary>
        /// Checks to see whether a valid numeric digit was pressed for numeric-only conditions
        /// </summary>
        /// <param name="keyChar">The key that was pressed</param>
        /// <returns>Whether the input was a valid number character</returns>
        private bool ValidNumberChar(string keyChar)
        {
            System.Globalization.NumberFormatInfo numberFormatInfo = System.Globalization.CultureInfo.CurrentCulture.NumberFormat;

            for (int i = 0; i < keyChar.Length; i++)
            {
                char ch = keyChar[i];
                if (!Char.IsDigit(ch))
                {
                    return false;
                }
            }

            return true;
        }

        public override void CollapseOutput()
        {
            foreach (Expander expander in this.strataExpanderList)
            {
                expander.Visibility = System.Windows.Visibility.Collapsed;
            }

            foreach (Grid grid in this.strataGridList)
            {
                grid.Visibility = System.Windows.Visibility.Collapsed;
                Border border = new Border();
                if (grid.Parent is Border)
                {
                    border = (grid.Parent) as Border;
                    border.Visibility = System.Windows.Visibility.Collapsed;
                }
            }

            foreach (UIElement element in panelMain.Children)
            {
                if (element is Grid && (element as Grid).Tag.ToString() == "2x2 grid")
                {
                    Grid statGrid = element as Grid;
                    statGrid.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                }
            }

            if (!string.IsNullOrEmpty(this.txtFilterString.Text))
            {
                this.txtFilterString.Visibility = System.Windows.Visibility.Collapsed;
            }

            this.messagePanel.Visibility = System.Windows.Visibility.Collapsed;
            this.txtFilterString.Visibility = System.Windows.Visibility.Collapsed;
            descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;
        }

        public override void ExpandOutput()
        {
            foreach (Expander expander in this.strataExpanderList)
            {
                expander.Visibility = System.Windows.Visibility.Visible;
            }

            foreach (Grid grid in this.strataGridList)
            {
                grid.Visibility = System.Windows.Visibility.Visible;
                Border border = new Border();
                if (grid.Parent is Border)
                {
                    border = (grid.Parent) as Border;
                    border.Visibility = System.Windows.Visibility.Visible;
                }
            }

            foreach (UIElement element in panelMain.Children)
            {
                if (element is Grid && (element as Grid).Tag.ToString() == "2x2 grid")
                {
                    Grid statGrid = element as Grid;
                    statGrid.Visibility = System.Windows.Visibility.Visible;
                    break;
                }
            }

            if (this.messagePanel.MessagePanelType != Controls.MessagePanelType.StatusPanel)
            {
                this.messagePanel.Visibility = System.Windows.Visibility.Visible;
            }

            if (!string.IsNullOrEmpty(this.txtFilterString.Text))
            {
                this.txtFilterString.Visibility = System.Windows.Visibility.Visible;
            }
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

            for (int i = 0; i < strataGridList.Count; i++)
            {
                strataGridList[i].Children.Clear();
            }
            for (int i = 0; i < strataExpanderList.Count; i++)
            {
                strataExpanderList[i].Content = null;
            }
            this.strataExpanderList.Clear();
            this.strataGridList.Clear();
            this.panelMain.Children.Clear();

            base.CloseGadget();

            GadgetOptions = null;
        }

        /// <summary>
        /// Clears the gadget's output
        /// </summary>
        private void ClearResults()
        {
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;
            messagePanel.Text = string.Empty;
            descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;

            foreach (Grid grid in strataGridList)
            {
                grid.Children.Clear();
                grid.RowDefinitions.Clear();
                if (grid.Parent is Border)
                {
                    Border border = (grid.Parent) as Border;                    
                    panelMain.Children.Remove(border);
                }                
            }

            foreach (Expander expander in strataExpanderList)
            {
                if (panelMain.Children.Contains(expander))
                {
                    panelMain.Children.Remove(expander);
                }
            }

            panelMain.Children.Clear();

            strataGridList.Clear();
            strataExpanderList.Clear();
        }

        /// <summary>
        /// Checks the selected variables and enables/disables checkboxes as appropriate
        /// </summary>
        private void CheckVariables()
        {
            isDropDownList = false;
            isCommentLegal = false;
            isOptionField = false;
            isRecoded = false;

            if (cbxField.SelectedItem != null && !string.IsNullOrEmpty(cbxField.SelectedItem.ToString()))
            {
                foreach (DataRow fieldRow in dashboardHelper.FieldTable.Rows)
                {
                    if (fieldRow["columnname"].Equals(cbxField.SelectedItem.ToString()))
                    {
                        if (fieldRow["epifieldtype"] is TableBasedDropDownField || fieldRow["epifieldtype"] is YesNoField || fieldRow["epifieldtype"] is CheckBoxField)
                        {
                            isDropDownList = true;
                            if (fieldRow["epifieldtype"] is DDLFieldOfCommentLegal)
                            {
                                isCommentLegal = true;
                            }
                        }
                        else if (fieldRow["epifieldtype"] is OptionField)
                        {
                            isOptionField = true;
                        }
                        break;
                    }
                }

                if (dashboardHelper.IsUserDefinedColumn(cbxField.SelectedItem.ToString()))
                {
                    List<IDashboardRule> associatedRules = dashboardHelper.Rules.GetRules(cbxField.SelectedItem.ToString());
                    foreach (IDashboardRule rule in associatedRules)
                    {
                        if (rule is Rule_Recode)
                        {
                            isRecoded = true;
                        }
                    }
                }
            }
        }

        #endregion // Private Methods        

        #region Public Methods
        /// <summary>
        /// Returns the gadget's description as a string.
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            return "Complex Sample Tables Gadget";
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handles the click event for the Run button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            RefreshResults();
        }

        private void AddComplexSampleTableGrid(StatisticsRepository.ComplexSampleTables.CSTablesResults results) 
        {
            Grid grid = new Grid();
            grid.Tag = GadgetOptions.MainVariableName;
            grid.Style = this.Resources["genericOutputGrid"] as Style;
            grid.Margin = (Thickness)this.Resources["genericElementMargin"];
            grid.Visibility = System.Windows.Visibility.Collapsed;
            strataGridList.Add(grid);

            // Setup grid header
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition()); // value

            for (int i = 0; i < results.Rows[0].Cells.Count; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            grid.ColumnDefinitions.Add(new ColumnDefinition()); // total

            for (int y = 0; y < grid.ColumnDefinitions.Count; y++)
            {
                Rectangle rctHeader = new Rectangle();
                rctHeader.Style = this.Resources["gridHeaderCellRectangle"] as Style;
                Grid.SetRow(rctHeader, 0);
                Grid.SetRowSpan(rctHeader, 2);
                Grid.SetColumn(rctHeader, y);
                grid.Children.Add(rctHeader);
            }

            for (int y = 0; y < grid.ColumnDefinitions.Count; y++)
            {
                if (y < grid.ColumnDefinitions.Count - 2)
                {
                    TextBlock txtColumnHeading = new TextBlock();
                    txtColumnHeading.Text = results.Rows[0].Cells[y].Value;
                    txtColumnHeading.Style = this.Resources["columnHeadingText"] as Style;
                    txtColumnHeading.Margin = (Thickness)this.Resources["genericTextMarginAlt"];
                    Grid.SetRow(txtColumnHeading, 1);
                    Grid.SetColumn(txtColumnHeading, y + 1);
                    grid.Children.Add(txtColumnHeading);
                }
            }

            TextBlock txtHeader = new TextBlock();
            txtHeader.Text = GadgetOptions.CrosstabVariableName;
            Field field = DashboardHelper.GetAssociatedField(GadgetOptions.CrosstabVariableName);

            if (field != null && field is IDataField)
            {
                txtHeader.Text = ((IDataField)field).PromptText;
            }

            txtHeader.Style = this.Resources["columnHeadingText"] as Style;
            txtHeader.Margin = (Thickness)this.Resources["genericTextMarginAlt"];
            txtHeader.FontSize = txtHeader.FontSize + 2;
            Grid.SetRow(txtHeader, 0);
            Grid.SetColumn(txtHeader, 1);
            Grid.SetColumnSpan(txtHeader, grid.ColumnDefinitions.Count - 2);
            grid.Children.Add(txtHeader);

            TextBlock txtValHeader = new TextBlock();
            txtValHeader.Text = GadgetOptions.MainVariableName;
            txtValHeader.Style = this.Resources["columnHeadingText"] as Style;
            Grid.SetRow(txtValHeader, 0);
            Grid.SetRowSpan(txtValHeader, 2);
            Grid.SetColumn(txtValHeader, 0);            
            grid.Children.Add(txtValHeader);

            TextBlock txtTotalHeader = new TextBlock();
            txtTotalHeader.Text = SharedStrings.TOTAL;
            txtTotalHeader.Style = this.Resources["columnHeadingText"] as Style;
            Grid.SetRow(txtTotalHeader, 0);
            Grid.SetRowSpan(txtTotalHeader, 2);
            Grid.SetColumn(txtTotalHeader, grid.ColumnDefinitions.Count - 1);            
            grid.Children.Add(txtTotalHeader);
            
            double grandTotal = 0;

            for (int i = 0; i < results.Rows.Count - 1; i++)
            {
                StatisticsRepository.ComplexSampleTables.TablesRow tRow = results.Rows[i];
                foreach (StatisticsRepository.ComplexSampleTables.CSRow fRow in tRow.Cells)
                {
                    grandTotal = grandTotal + fRow.Count;
                }
            }

            foreach (StatisticsRepository.ComplexSampleTables.TablesRow tRow in results.Rows)
            {
                grid.RowDefinitions.Add(new RowDefinition());
                TextBlock txtValueLabel = new TextBlock();
                txtValueLabel.Text = tRow.Cells[0].Domain;
                txtValueLabel.FontWeight = FontWeights.Bold;
                txtValueLabel.Margin = (Thickness)this.Resources["genericTextMarginAlt"];
                Grid.SetRow(txtValueLabel, grid.RowDefinitions.Count - 1);
                Grid.SetColumn(txtValueLabel, 0);
                grid.Children.Add(txtValueLabel);

                grid.RowDefinitions.Add(new RowDefinition());
                TextBlock txtRowPercentLabel = new TextBlock();
                txtRowPercentLabel.Text = "Row %";
                txtRowPercentLabel.Margin = (Thickness)this.Resources["genericTextMarginAlt"];
                Grid.SetRow(txtRowPercentLabel, grid.RowDefinitions.Count - 1);
                Grid.SetColumn(txtRowPercentLabel, 0);
                grid.Children.Add(txtRowPercentLabel);

                grid.RowDefinitions.Add(new RowDefinition());
                TextBlock txtColPercentLabel = new TextBlock();
                txtColPercentLabel.Text = "Col %";
                txtColPercentLabel.Margin = (Thickness)this.Resources["genericTextMarginAlt"];
                Grid.SetRow(txtColPercentLabel, grid.RowDefinitions.Count - 1);
                Grid.SetColumn(txtColPercentLabel, 0);
                grid.Children.Add(txtColPercentLabel);

                grid.RowDefinitions.Add(new RowDefinition());
                TextBlock txtSEPercentLabel = new TextBlock();
                txtSEPercentLabel.Text = "SE %";
                txtSEPercentLabel.Margin = (Thickness)this.Resources["genericTextMarginAlt"];
                Grid.SetRow(txtSEPercentLabel, grid.RowDefinitions.Count - 1);
                Grid.SetColumn(txtSEPercentLabel, 0);
                grid.Children.Add(txtSEPercentLabel);

                grid.RowDefinitions.Add(new RowDefinition());
                TextBlock txtLCLPercentLabel = new TextBlock();
                txtLCLPercentLabel.Text = "LCL %";
                txtLCLPercentLabel.Margin = (Thickness)this.Resources["genericTextMarginAlt"];
                Grid.SetRow(txtLCLPercentLabel, grid.RowDefinitions.Count - 1);
                Grid.SetColumn(txtLCLPercentLabel, 0);
                grid.Children.Add(txtLCLPercentLabel);

                grid.RowDefinitions.Add(new RowDefinition());
                TextBlock txtUCLPercentLabel = new TextBlock();
                txtUCLPercentLabel.Text = "UCL %";
                txtUCLPercentLabel.Margin = (Thickness)this.Resources["genericTextMarginAlt"];
                Grid.SetRow(txtUCLPercentLabel, grid.RowDefinitions.Count - 1);
                Grid.SetColumn(txtUCLPercentLabel, 0);
                grid.Children.Add(txtUCLPercentLabel);

                grid.RowDefinitions.Add(new RowDefinition());
                TextBlock txtDesginEffectLabel = new TextBlock();
                txtDesginEffectLabel.Text = "Design Effect";
                txtDesginEffectLabel.Margin = (Thickness)this.Resources["genericTextMarginAlt"];
                Grid.SetRow(txtDesginEffectLabel, grid.RowDefinitions.Count - 1);
                Grid.SetColumn(txtDesginEffectLabel, 0);
                grid.Children.Add(txtDesginEffectLabel);

                double runningRowTotal = 0;                
                double runningColPercentTotal = 0;
                string unknown = "----";

                int column = 1;

                // Grid rows
                foreach (StatisticsRepository.ComplexSampleTables.CSRow fRow in tRow.Cells)
                {
                    runningRowTotal = runningRowTotal + fRow.Count;                    
                    bool zeroCount = false;
                    if (fRow.Count == 0) zeroCount = true;

                    runningColPercentTotal = zeroCount == true ? runningColPercentTotal : runningColPercentTotal + fRow.ColPercent;                    

                    // Value row
                    TextBlock txtValue = new TextBlock();
                    txtValue.Text = fRow.Count.ToString();
                    txtValue.Margin = (Thickness)this.Resources["genericTextMarginAlt"];
                    txtValue.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                    Grid.SetRow(txtValue, grid.RowDefinitions.Count - 7);
                    Grid.SetColumn(txtValue, column);
                    grid.Children.Add(txtValue);

                    // Row % row
                    TextBlock txtRowPercent = new TextBlock();
                    txtRowPercent.Text = zeroCount == true ? unknown : fRow.RowPercent.ToString("F3");
                    txtRowPercent.Margin = (Thickness)this.Resources["genericTextMarginAlt"];
                    txtRowPercent.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                    Grid.SetRow(txtRowPercent, grid.RowDefinitions.Count - 6);
                    Grid.SetColumn(txtRowPercent, column);
                    grid.Children.Add(txtRowPercent);

                    // Col % row
                    TextBlock txtColPercent = new TextBlock();
                    txtColPercent.Text = zeroCount == true ? unknown : fRow.ColPercent.ToString("F3");
                    txtColPercent.Margin = (Thickness)this.Resources["genericTextMarginAlt"];
                    txtColPercent.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                    Grid.SetRow(txtColPercent, grid.RowDefinitions.Count - 5);
                    Grid.SetColumn(txtColPercent, column);
                    grid.Children.Add(txtColPercent);

                    // SE % row
                    TextBlock txtSEPercent = new TextBlock();
                    txtSEPercent.Text = fRow.SE.ToString("F3");
                    txtSEPercent.Margin = (Thickness)this.Resources["genericTextMarginAlt"];
                    txtSEPercent.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                    Grid.SetRow(txtSEPercent, grid.RowDefinitions.Count - 4);
                    Grid.SetColumn(txtSEPercent, column);
                    grid.Children.Add(txtSEPercent);

                    // LCL % row
                    TextBlock txtLCLPercent = new TextBlock();
                    txtLCLPercent.Text = zeroCount == true ? unknown : fRow.LCL.ToString("F3");
                    txtLCLPercent.Margin = (Thickness)this.Resources["genericTextMarginAlt"];
                    txtLCLPercent.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                    Grid.SetRow(txtLCLPercent, grid.RowDefinitions.Count - 3);
                    Grid.SetColumn(txtLCLPercent, column);
                    grid.Children.Add(txtLCLPercent);

                    // UCL % row
                    TextBlock txtUCLPercent = new TextBlock();
                    txtUCLPercent.Text = zeroCount == true ? unknown : fRow.UCL.ToString("F3");
                    txtUCLPercent.Margin = (Thickness)this.Resources["genericTextMarginAlt"];
                    txtUCLPercent.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                    Grid.SetRow(txtUCLPercent, grid.RowDefinitions.Count - 2);
                    Grid.SetColumn(txtUCLPercent, column);
                    grid.Children.Add(txtUCLPercent);

                    // DE row
                    TextBlock txtDesignEffect = new TextBlock();
                    txtDesignEffect.Text = fRow.DesignEffect.ToString("F3");
                    txtDesignEffect.Margin = (Thickness)this.Resources["genericTextMarginAlt"];
                    txtDesignEffect.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                    Grid.SetRow(txtDesignEffect, grid.RowDefinitions.Count - 1);
                    Grid.SetColumn(txtDesignEffect, column);
                    grid.Children.Add(txtDesignEffect);

                    //runningTotal = runningTotal + fRow.Count;
                    column++;
                }

                // Total Value row
                TextBlock txtRowTotalValue = new TextBlock();
                txtRowTotalValue.Text = runningRowTotal.ToString();
                txtRowTotalValue.Margin = (Thickness)this.Resources["genericTextMarginAlt"];
                txtRowTotalValue.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                Grid.SetRow(txtRowTotalValue, grid.RowDefinitions.Count - 7);
                Grid.SetColumn(txtRowTotalValue, grid.ColumnDefinitions.Count - 1);
                grid.Children.Add(txtRowTotalValue);

                // Total Row % row
                TextBlock txtRowPercentTotalValue = new TextBlock();
                txtRowPercentTotalValue.Text = (1).ToString("P");
                txtRowPercentTotalValue.Margin = (Thickness)this.Resources["genericTextMarginAlt"];
                txtRowPercentTotalValue.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                Grid.SetRow(txtRowPercentTotalValue, grid.RowDefinitions.Count - 6);
                Grid.SetColumn(txtRowPercentTotalValue, grid.ColumnDefinitions.Count - 1);
                grid.Children.Add(txtRowPercentTotalValue);

                // Total Col % row
                TextBlock txtColPercentTotalValue = new TextBlock();
                txtColPercentTotalValue.Text = (tRow.RowColPercent.Value / 100).ToString("P"); //(runningRowTotal / grandTotal).ToString("P");
                txtColPercentTotalValue.Margin = (Thickness)this.Resources["genericTextMarginAlt"];
                txtColPercentTotalValue.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                Grid.SetRow(txtColPercentTotalValue, grid.RowDefinitions.Count - 5);
                Grid.SetColumn(txtColPercentTotalValue, grid.ColumnDefinitions.Count - 1);
                grid.Children.Add(txtColPercentTotalValue);
            }

            // Draw grid borders
            int rdcount = 0;
            foreach (RowDefinition rd in grid.RowDefinitions)
            {
                int cdcount = 0;
                foreach (ColumnDefinition cd in grid.ColumnDefinitions)
                {
                    Border border = new Border();
                    border.Style = this.Resources["gridCellBorder"] as Style;

                    if (rdcount == 0 && cdcount > 0)
                    {
                        border.BorderThickness = new Thickness(border.BorderThickness.Left, border.BorderThickness.Bottom, border.BorderThickness.Right, border.BorderThickness.Bottom);
                    }
                    if (cdcount == 0 && rdcount > 0)
                    {
                        border.BorderThickness = new Thickness(border.BorderThickness.Right, border.BorderThickness.Top, border.BorderThickness.Right, border.BorderThickness.Bottom);
                    }

                    if (rdcount == 0 && cdcount > 0 && cdcount < grid.ColumnDefinitions.Count - 2)
                    {
                        border.BorderThickness = new Thickness(0, border.BorderThickness.Bottom, 0, border.BorderThickness.Bottom);
                    }
                    
                    if (rdcount == 0 && cdcount == 0)
                    {
                        border.BorderThickness = new Thickness(1, 1, 1, 0);
                    }
                    else if (rdcount == 0 && cdcount == grid.ColumnDefinitions.Count - 1)
                    {
                        border.BorderThickness = new Thickness(border.BorderThickness.Left, border.BorderThickness.Top, border.BorderThickness.Right, 0);
                    }

                    Grid.SetRow(border, rdcount);
                    Grid.SetColumn(border, cdcount);
                    grid.Children.Add(border);
                    cdcount++;
                }
                rdcount++;
            }

            panelMain.Children.Add(grid);

            if (results.OddsRatio.HasValue)
            {
                Grid statGrid = new Grid();
                statGrid.Margin = (Thickness)this.Resources["genericElementMargin"];
                statGrid.Margin = new Thickness(statGrid.Margin.Left, statGrid.Margin.Top + 6, statGrid.Margin.Right, statGrid.Margin.Bottom);

                statGrid.ColumnDefinitions.Add(new ColumnDefinition());
                statGrid.ColumnDefinitions.Add(new ColumnDefinition());

                statGrid.RowDefinitions.Add(new RowDefinition());
                statGrid.RowDefinitions.Add(new RowDefinition());
                statGrid.RowDefinitions.Add(new RowDefinition());
                statGrid.RowDefinitions.Add(new RowDefinition());
                statGrid.RowDefinitions.Add(new RowDefinition());
                statGrid.RowDefinitions.Add(new RowDefinition());
                statGrid.RowDefinitions.Add(new RowDefinition());
                statGrid.RowDefinitions.Add(new RowDefinition());
                statGrid.RowDefinitions.Add(new RowDefinition());
                statGrid.RowDefinitions.Add(new RowDefinition());
                statGrid.RowDefinitions.Add(new RowDefinition());
                statGrid.RowDefinitions.Add(new RowDefinition());
                statGrid.RowDefinitions.Add(new RowDefinition());

                panelMain.Children.Add(statGrid);
                statGrid.Tag = "2x2 grid";

                TextBlock txtHead = new TextBlock();
                txtHead.Text = "Complex Sample Design" + Environment.NewLine + "Analysis of 2x2 Table";
                txtHead.Margin = (Thickness)this.Resources["genericTextMarginAlt"];
                txtHead.FontSize = txtHead.FontSize + 1;
                txtHead.FontWeight = FontWeights.Bold;
                Grid.SetRow(txtHead, 0);
                Grid.SetColumn(txtHead, 0);
                Grid.SetColumnSpan(txtHead, 2);
                statGrid.Children.Add(txtHead);

                TextBlock txt1 = new TextBlock();
                txt1.Text = "Odds Ratio (OR)";
                txt1.Margin = (Thickness)this.Resources["genericTextMarginAlt"];
                Grid.SetRow(txt1, 1);
                Grid.SetColumn(txt1, 0);
                statGrid.Children.Add(txt1);                

                TextBlock txt1a = new TextBlock();
                txt1a.Text = results.OddsRatio.Value.ToString("F4");
                txt1a.Margin = (Thickness)this.Resources["genericTextMarginAlt"];
                txt1a.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                Grid.SetRow(txt1a, 1);
                Grid.SetColumn(txt1a, 1);
                statGrid.Children.Add(txt1a);

                // *****************************************************************

                TextBlock txt2 = new TextBlock();
                txt2.Text = "Standard Error (SE)";
                txt2.Margin = (Thickness)this.Resources["genericTextMarginAlt"];
                Grid.SetRow(txt2, 2);
                Grid.SetColumn(txt2, 0);
                statGrid.Children.Add(txt2);

                TextBlock txt2a = new TextBlock();
                txt2a.Text = results.StandardErrorOR.Value.ToString("F4");
                txt2a.Margin = (Thickness)this.Resources["genericTextMarginAlt"];
                txt2a.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                Grid.SetRow(txt2a, 2);
                Grid.SetColumn(txt2a, 1);
                statGrid.Children.Add(txt2a);

                // *****************************************************************

                TextBlock txt3 = new TextBlock();
                txt3.Text = "95% Conf. Limits ";
                txt3.Margin = (Thickness)this.Resources["genericTextMarginAlt"];
                Grid.SetRow(txt3, 3);
                Grid.SetColumn(txt3, 0);
                statGrid.Children.Add(txt3);

                TextBlock txt3a = new TextBlock();
                txt3a.Text = "(" + results.LCLOR.Value.ToString("F3") + ", " + results.UCLOR.Value.ToString("F3") + ")";
                txt3a.Margin = (Thickness)this.Resources["genericTextMarginAlt"];
                txt3a.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                Grid.SetRow(txt3a, 3);
                Grid.SetColumn(txt3a, 1);
                statGrid.Children.Add(txt3a);

                // *****************************************************************

                TextBlock txt4 = new TextBlock();
                txt4.Text = " ";
                txt4.Margin = (Thickness)this.Resources["genericTextMarginAlt"];
                Grid.SetRow(txt4, 4);
                Grid.SetColumn(txt4, 0);
                statGrid.Children.Add(txt4);

                // *****************************************************************

                TextBlock txt5 = new TextBlock();
                txt5.Text = "Risk Ratio (RR)";
                txt5.Margin = (Thickness)this.Resources["genericTextMarginAlt"];
                Grid.SetRow(txt5, 5);
                Grid.SetColumn(txt5, 0);
                statGrid.Children.Add(txt5);

                TextBlock txt5a = new TextBlock();
                txt5a.Text = results.RiskRatio.Value.ToString("F4");
                txt5a.Margin = (Thickness)this.Resources["genericTextMarginAlt"];
                txt5a.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                Grid.SetRow(txt5a, 5);
                Grid.SetColumn(txt5a, 1);
                statGrid.Children.Add(txt5a);

                // *****************************************************************

                TextBlock txt6 = new TextBlock();
                txt6.Text = "Standard Error (SE) ";
                txt6.Margin = (Thickness)this.Resources["genericTextMarginAlt"];
                Grid.SetRow(txt6, 6);
                Grid.SetColumn(txt6, 0);
                statGrid.Children.Add(txt6);

                TextBlock txt6a = new TextBlock();
                txt6a.Text = results.StandardErrorRR.Value.ToString("F4");
                txt6a.Margin = (Thickness)this.Resources["genericTextMarginAlt"];
                txt6a.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                Grid.SetRow(txt6a, 6);
                Grid.SetColumn(txt6a, 1);
                statGrid.Children.Add(txt6a);

                // *****************************************************************

                TextBlock txt7 = new TextBlock();
                txt7.Text = "95% Conf. Limits ";
                txt7.Margin = (Thickness)this.Resources["genericTextMarginAlt"];
                Grid.SetRow(txt7, 7);
                Grid.SetColumn(txt7, 0);
                statGrid.Children.Add(txt7);

                TextBlock txt7a = new TextBlock();
                txt7a.Text = "(" + results.LCLRR.Value.ToString("F3") + ", " + results.UCLRR.Value.ToString("F3") + ")";
                txt7a.Margin = (Thickness)this.Resources["genericTextMarginAlt"];
                txt7a.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                Grid.SetRow(txt7a, 7);
                Grid.SetColumn(txt7a, 1);
                statGrid.Children.Add(txt7a);

                // *****************************************************************

                TextBlock txt8 = new TextBlock();
                txt8.Text = " ";
                txt8.Margin = (Thickness)this.Resources["genericTextMarginAlt"];
                Grid.SetRow(txt8, 8);
                Grid.SetColumn(txt8, 0);
                statGrid.Children.Add(txt8);

                TextBlock txt9 = new TextBlock();
                txt9.Text = "Risk Difference (RD%)";
                txt9.Margin = (Thickness)this.Resources["genericTextMarginAlt"];
                Grid.SetRow(txt9, 9);
                Grid.SetColumn(txt9, 0);
                statGrid.Children.Add(txt9);

                TextBlock txt9a = new TextBlock();
                txt9a.Text = results.RiskDifference.Value.ToString("F4");
                txt9a.Margin = (Thickness)this.Resources["genericTextMarginAlt"];
                txt9a.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                Grid.SetRow(txt9a, 9);
                Grid.SetColumn(txt9a, 1);
                statGrid.Children.Add(txt9a);

                // *****************************************************************

                TextBlock txt10 = new TextBlock();
                txt10.Text = "Standard Error (SE)";
                txt10.Margin = (Thickness)this.Resources["genericTextMarginAlt"];
                Grid.SetRow(txt10, 10);
                Grid.SetColumn(txt10, 0);
                statGrid.Children.Add(txt10);

                TextBlock txt10a = new TextBlock();
                txt10a.Text = results.StandardErrorRD.Value.ToString("F4");
                txt10a.Margin = (Thickness)this.Resources["genericTextMarginAlt"];
                txt10a.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                Grid.SetRow(txt10a, 10);
                Grid.SetColumn(txt10a, 1);
                statGrid.Children.Add(txt10a);

                // *****************************************************************

                TextBlock txt11 = new TextBlock();
                txt11.Text = "95% Conf. Limits ";
                txt11.Margin = (Thickness)this.Resources["genericTextMarginAlt"];
                Grid.SetRow(txt11, 11);
                Grid.SetColumn(txt11, 0);
                statGrid.Children.Add(txt11);

                TextBlock txt11a = new TextBlock();
                txt11a.Text = "(" + results.LCLRD.Value.ToString("F3") + ", " + results.UCLRD.Value.ToString("F3") + ")";
                txt11a.Margin = (Thickness)this.Resources["genericTextMarginAlt"];
                txt11a.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                Grid.SetRow(txt11a, 11);
                Grid.SetColumn(txt11a, 1);
                statGrid.Children.Add(txt11a);

                // *****************************************************************

                TextBlock txt12 = new TextBlock();
                txt12.Text = " ";
                txt12.Margin = (Thickness)this.Resources["genericTextMarginAlt"];
                Grid.SetRow(txt12, 12);
                Grid.SetColumn(txt12, 0);
                statGrid.Children.Add(txt12);
            }
        }

        /// <summary>
        /// Handles the DoWorker event for the worker
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        protected override void worker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            lock (syncLock)
            {
                Dictionary<string, string> inputVariableList = ((GadgetParameters)e.Argument).InputVariableList;
            
                this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToProcessingState));                
                this.Dispatcher.BeginInvoke(new SimpleCallback(ClearResults));
                CreateComplexSampleTableGridDelegate createGrid = new CreateComplexSampleTableGridDelegate(AddComplexSampleTableGrid);

                string freqVar = GadgetOptions.MainVariableName;
                string weightVar = GadgetOptions.WeightVariableName;
                string strataVar = string.Empty;                
                bool includeMissing = GadgetOptions.ShouldIncludeMissing;
                
                if (inputVariableList.ContainsKey("stratavar"))
                {
                    strataVar = inputVariableList["stratavar"];
                }
                
                List<string> stratas = new List<string>();
                if (!string.IsNullOrEmpty(strataVar))
                {
                    stratas.Add(strataVar);
                }

                try
                {
                    Configuration config = dashboardHelper.Config;
                    string yesValue = config.Settings.RepresentationOfYes;
                    string noValue = config.Settings.RepresentationOfNo;

                    RequestUpdateStatusDelegate requestUpdateStatus = new RequestUpdateStatusDelegate(RequestUpdateStatusMessage);
                    CheckForCancellationDelegate checkForCancellation = new CheckForCancellationDelegate(IsCancelled);

                    GadgetOptions.GadgetStatusUpdate += new GadgetStatusUpdateHandler(requestUpdateStatus);
                    GadgetOptions.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(checkForCancellation);

                    if (this.DataFilters != null && this.DataFilters.Count > 0)
                    {
                        GadgetOptions.CustomFilter = this.DataFilters.GenerateDataFilterString(false);
                    }
                    else
                    {
                        GadgetOptions.CustomFilter = string.Empty;
                    }

                    List<string> columnNames = new List<string>();
                    columnNames.Add(GadgetOptions.MainVariableName);
                    columnNames.Add(GadgetOptions.CrosstabVariableName);
                    columnNames.Add(GadgetOptions.PSUVariableName);
                    if(GadgetOptions.StrataVariableNames != null && GadgetOptions.StrataVariableNames.Count == 1 && !string.IsNullOrEmpty(GadgetOptions.StrataVariableNames[0])) 
                    {
                        columnNames.Add(GadgetOptions.StrataVariableNames[0]);
                    }
                    if(!string.IsNullOrEmpty(GadgetOptions.WeightVariableName)) 
                    {
                        columnNames.Add(GadgetOptions.WeightVariableName);
                    }

                    DataTable csTable = dashboardHelper.GenerateTable(columnNames, GadgetOptions.CustomFilter);

                    if (csTable == null || csTable.Rows.Count == 0)
                    {
                        this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), SharedStrings.NO_RECORDS_SELECTED);
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
                        lock (staticSyncLock)
                        {
                            // Complex sample frequency actually uses the CS Tables class.
                            StatisticsRepository.ComplexSampleTables csTables = new StatisticsRepository.ComplexSampleTables();
                            StatisticsRepository.ComplexSampleTables.CSTablesResults results = csTables.ComplexSampleTables(GadgetOptions.InputVariableList, csTable);

                            if (!string.IsNullOrEmpty(results.ErrorMessage))
                            {
                                throw new ApplicationException(results.ErrorMessage);
                            }

                            if (results.Rows == null || results.Rows.Count == 0)
                            {
                                throw new ApplicationException("No data was returned from the statistics repository.");
                            }

                            StatisticsRepository.ComplexSampleTables.TablesRow totalRow = new StatisticsRepository.ComplexSampleTables.TablesRow();
                            totalRow.Cells = new List<StatisticsRepository.ComplexSampleTables.CSRow>();
                            this.Dispatcher.BeginInvoke(createGrid, results);
                        }                        
                    }
                    this.Dispatcher.BeginInvoke(new SimpleCallback(RenderFinish));
                    this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                }
                catch (Exception ex)
                {
                    this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), ex.Message);
                    this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                }
                finally
                {
                    //stopwatch.Stop();
                    //Debug.Print("CS Frequency gadget took " + stopwatch.Elapsed.ToString() + " seconds to complete with " + dashboardHelper.RecordCount.ToString() + " records and the following filters:");
                    //Debug.Print(dashboardHelper.DataFilters.GenerateDataFilterString());
                }
            }
        }
        #endregion // Event Handlers        
        
        #region Private Properties
        /// <summary>
        /// Gets whether or not the main variable is a drop-down list
        /// </summary>
        private bool IsDropDownList
        {
            get
            {
                return this.isDropDownList;
            }
        }

        /// <summary>
        /// Gets whether the main variable is a comment legal field
        /// </summary>
        private bool IsCommentLegal
        {
            get
            {
                return this.isCommentLegal;
            }
        }

        /// <summary>
        /// Gets whether the main variable is an option field
        /// </summary>
        private bool IsOptionField
        {
            get
            {
                return this.isOptionField;
            }
        }

        /// <summary>
        /// Gets whether the main variable is a recoded variable
        /// </summary>
        private bool IsRecoded
        {
            get
            {
                return this.isRecoded;
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
            this.cbxField.IsEnabled = false;
            this.cbxFieldStrata.IsEnabled = false;
            this.cbxFieldWeight.IsEnabled = false;
            this.cbxFieldPSU.IsEnabled = false;
            this.btnRun.IsEnabled = false;
        }

        /// <summary>
        /// Sets the gadget to its 'finished' state
        /// </summary>
        public override void SetGadgetToFinishedState()
        {
            this.IsProcessing = false;
            this.cbxField.IsEnabled = true;
            this.cbxFieldStrata.IsEnabled = true;
            this.cbxFieldWeight.IsEnabled = true;
            this.cbxFieldPSU.IsEnabled = true;
            this.btnRun.IsEnabled = true;

            base.SetGadgetToFinishedState();
        }

        /// <summary>
        /// Initiates a refresh of the gadget's output
        /// </summary>
        public override void RefreshResults()
        {
            if (!LoadingCombos && GadgetOptions != null && cbxField.SelectedIndex > -1 && cbxFieldPSU.SelectedIndex > -1 && cbxFieldOutcome.SelectedIndex > -1)
            {
                CreateInputVariableList();
                txtFilterString.Visibility = System.Windows.Visibility.Collapsed;
                waitPanel.Visibility = System.Windows.Visibility.Visible;
                messagePanel.MessagePanelType = Controls.MessagePanelType.StatusPanel;
                descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;
                baseWorker = new BackgroundWorker();
                baseWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(Execute);
                baseWorker.RunWorkerAsync();
                base.RefreshResults();
            }
            else if (!LoadingCombos && cbxField.SelectedIndex == -1)
            {
                ClearResults();                
                waitPanel.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Updates the variable names available in the gadget's properties
        /// </summary>
        public override void UpdateVariableNames()
        {
            FillComboboxes(true);
        }

        /// <summary>
        /// Generates Xml representation of this gadget
        /// </summary>
        /// <param name="doc">The Xml docment</param>
        /// <returns>XmlNode</returns>
        public override XmlNode Serialize(XmlDocument doc)
        {
            CreateInputVariableList();

            Dictionary<string, string> inputVariableList = GadgetOptions.InputVariableList;

            string freqVar = string.Empty;
            string strataVar = string.Empty;
            string weightVar = string.Empty;
            string sort = string.Empty;

            WordBuilder wb = new WordBuilder(",");

            if (GadgetOptions.StrataVariableNames != null && GadgetOptions.StrataVariableNames.Count == 1)
            {
                strataVar = GadgetOptions.StrataVariableNames[0];
            }

            CustomOutputHeading = headerPanel.Text;
            CustomOutputDescription = descriptionPanel.Text;

            string xmlString =
            "<mainVariable>" + GadgetOptions.MainVariableName + "</mainVariable>" +
            "<outcomeVariable>" + GadgetOptions.CrosstabVariableName + "</outcomeVariable>" +
            "<strataVariable>" + strataVar + "</strataVariable>" +
            "<weightVariable>" + GadgetOptions.WeightVariableName + "</weightVariable>" +
            "<psuVariable>" + GadgetOptions.PSUVariableName + "</psuVariable>" +
            "<customHeading>" + CustomOutputHeading.Replace("<", "&lt;") + "</customHeading>" +
            "<customDescription>" + CustomOutputDescription.Replace("<", "&lt;") + "</customDescription>" +
            "<customCaption>" + CustomOutputCaption + "</customCaption>";

            System.Xml.XmlElement element = doc.CreateElement("complexSampleTablesGadget");
            element.InnerXml = xmlString;
            element.AppendChild(SerializeFilters(doc));

            System.Xml.XmlAttribute locationY = doc.CreateAttribute("top");
            System.Xml.XmlAttribute locationX = doc.CreateAttribute("left");
            System.Xml.XmlAttribute collapsed = doc.CreateAttribute("collapsed");
            System.Xml.XmlAttribute type = doc.CreateAttribute("gadgetType");

            locationY.Value = Canvas.GetTop(this).ToString("F0");
            locationX.Value = Canvas.GetLeft(this).ToString("F0");
            collapsed.Value = "false"; // currently no way to collapse the gadget, so leave this 'false' for now
            type.Value = "Epi.WPF.Dashboard.Gadgets.Analysis.ComplexSampleTablesControl";

            element.Attributes.Append(locationY);
            element.Attributes.Append(locationX);
            element.Attributes.Append(collapsed);
            element.Attributes.Append(type);

            return element;
        }

        /// <summary>
        /// Creates the frequency gadget from an Xml element
        /// </summary>
        /// <param name="element">The element from which to create the gadget</param>
        public override void CreateFromXml(XmlElement element)
        {
            this.LoadingCombos = true;
            HideConfigPanel();
            txtFilterString.Visibility = System.Windows.Visibility.Collapsed;
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;

            foreach (XmlElement child in element.ChildNodes)
            {
                switch (child.Name.ToLower())
                {
                    case "mainvariable":
                        cbxField.Text = child.InnerText.Replace("&lt;", "<");
                        break;
                    case "outcomevariable":
                        cbxFieldOutcome.Text = child.InnerText.Replace("&lt;", "<");
                        break;
                    case "stratavariable":
                        cbxFieldStrata.Text = child.InnerText.Replace("&lt;", "<");
                        break;
                    case "weightvariable":
                        cbxFieldWeight.Text = child.InnerText.Replace("&lt;", "<");
                        break;
                    case "psuvariable":
                        cbxFieldPSU.Text = child.InnerText.Replace("&lt;", "<");
                        break;
                    case "customheading":
                        if (!string.IsNullOrEmpty(child.InnerText) && !child.InnerText.Equals("(none)"))
                        {
                            this.CustomOutputHeading = child.InnerText.Replace("&lt;", "<"); ;
                        }
                        break;
                    case "customdescription":
                        if (!string.IsNullOrEmpty(child.InnerText) && !child.InnerText.Equals("(none)"))
                        {
                            this.CustomOutputDescription = child.InnerText.Replace("&lt;", "<");

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
                    case "customcaption":
                        this.CustomOutputCaption = child.InnerText;
                        break;
                    case "datafilters":
                        this.dataFilters = new DataFilters(this.dashboardHelper);
                        this.DataFilters.CreateFromXml(child);
                        break; 
                }
            }
            SetPositionFromXml(element);
            this.LoadingCombos = false;
            CheckVariables();
            RefreshResults();
            HideConfigPanel();
        }

        /// <summary>
        /// Converts the gadget's output to Html
        /// </summary>
        /// <returns></returns>
        public override string ToHTML(string htmlFileName = "", int count = 0)
        {
            StringBuilder htmlBuilder = new StringBuilder();

            CustomOutputHeading = headerPanel.Text;
            CustomOutputDescription = descriptionPanel.Text;

            if (CustomOutputHeading == null || (string.IsNullOrEmpty(CustomOutputHeading) && !CustomOutputHeading.Equals("(none)")))
            {
                htmlBuilder.AppendLine("<h2 class=\"gadgetHeading\">Complex Sample Tables</h2>");
            }
            else if(CustomOutputHeading != "(none)")
            {
                htmlBuilder.AppendLine("<h2 class=\"gadgetHeading\">" + CustomOutputHeading + "</h2>");
            }

            htmlBuilder.AppendLine("<p class=\"gadgetOptions\"><small>");
            htmlBuilder.AppendLine("<em>Exposure variable:</em> <strong>" + cbxField.Text + "</strong>");
            htmlBuilder.AppendLine("<br />");
            htmlBuilder.AppendLine("<em>Outcome variable:</em> <strong>" + cbxFieldOutcome.Text + "</strong>");
            htmlBuilder.AppendLine("<br />");
            htmlBuilder.AppendLine("<em>PSU variable:</em> <strong>" + cbxFieldPSU.Text + "</strong>");
            htmlBuilder.AppendLine("<br />");

            if (cbxFieldWeight.SelectedIndex >= 0)
            {
                htmlBuilder.AppendLine("<em>Weight variable:</em> <strong>" + cbxFieldWeight.Text + "</strong>");
                htmlBuilder.AppendLine("<br />");
            }
            if (cbxFieldStrata.SelectedIndex >= 0)
            {
                htmlBuilder.AppendLine("<em>Strata variable:</em> <strong>" + cbxFieldStrata.Text + "</strong>");
                htmlBuilder.AppendLine("<br />");
            }
            //htmlBuilder.AppendLine("<em>Include missing:</em> <strong>" + checkboxIncludeMissing.IsChecked.ToString() + "</strong>");
            //htmlBuilder.AppendLine("<br />");
            htmlBuilder.AppendLine("</small></p>");

            if (!string.IsNullOrEmpty(CustomOutputDescription))
            {
                htmlBuilder.AppendLine("<p class=\"gadgetsummary\">" + CustomOutputDescription + "</p>");
            }

            if (!string.IsNullOrEmpty(messagePanel.Text) && messagePanel.Visibility == Visibility.Visible)
            {
                htmlBuilder.AppendLine("<p><small><strong>" + messagePanel.Text + "</strong></small></p>");
            }

            if (!string.IsNullOrEmpty(txtFilterString.Text) && txtFilterString.Visibility == Visibility.Visible)
            {
                htmlBuilder.AppendLine("<p><small><strong>" + txtFilterString.Text + "</strong></small></p>");
            }

            foreach (Grid grid in this.strataGridList)
            {
                string gridName = grid.Tag.ToString();               

                htmlBuilder.AppendLine("<div style=\"height: 7px;\"></div>");
                htmlBuilder.AppendLine("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\">");
                
                htmlBuilder.AppendLine("<tr>");

                htmlBuilder.AppendLine("    <th rowspan=\"2\">");
                htmlBuilder.AppendLine(GadgetOptions.MainVariableName);
                htmlBuilder.AppendLine("    </th>");

                htmlBuilder.AppendLine("    <th style=\"text-align: center;\" colspan=\"" + (grid.ColumnDefinitions.Count - 2).ToString() + "\">");
                htmlBuilder.AppendLine(GadgetOptions.CrosstabVariableName);
                htmlBuilder.AppendLine("    </th>");

                htmlBuilder.AppendLine("    <th rowspan=\"2\">");
                htmlBuilder.AppendLine(SharedStrings.TOTAL);
                htmlBuilder.AppendLine("    </th>");

                htmlBuilder.AppendLine("</tr>");

                htmlBuilder.AppendLine("<tr>");

                for (int j = 1; j < grid.ColumnDefinitions.Count - 1; j++)
                {
                    IEnumerable<UIElement> elements = grid.Children.Cast<UIElement>().Where(x => Grid.GetRow(x) == 1 && Grid.GetColumn(x) == j);
                    TextBlock txt = null;
                    foreach (UIElement element in elements)
                    {
                        if (element is TextBlock)
                        {
                            txt = element as TextBlock;
                        }
                    }

                    string value = "&nbsp;";

                    if (txt != null)
                    {
                        value = txt.Text;
                    }

                    htmlBuilder.AppendLine("<th>" + value + "</th>");
                }

                htmlBuilder.AppendLine("</tr>");


                for(int i = 2; i < grid.RowDefinitions.Count; i++) 
                {
                    for (int j = 0; j < grid.ColumnDefinitions.Count; j++)
                    {
                        string tableDataTagOpen = "<td>";
                        string tableDataTagClose = "</td>";

                        if (j == 0)
                        {
                            htmlBuilder.AppendLine("<tr>");
                        }
                        if (j == 0 && i > 0)
                        {
                            tableDataTagOpen = "<td class=\"value\">";
                        }

                        IEnumerable<UIElement> elements = grid.Children.Cast<UIElement>().Where(x => Grid.GetRow(x) == i && Grid.GetColumn(x) == j);
                        TextBlock txt = null;
                        foreach (UIElement element in elements)
                        {
                            if (element is TextBlock)
                            {
                                txt = element as TextBlock;
                            }
                        }

                        string value = "&nbsp;";

                        if (txt != null)
                        {
                            value = txt.Text;                            
                        }

                        htmlBuilder.AppendLine(tableDataTagOpen + value + tableDataTagClose);

                        if (j >= grid.ColumnDefinitions.Count - 1)
                        {
                            htmlBuilder.AppendLine("</tr>");
                        }
                    }
                }
                htmlBuilder.AppendLine("</table>");                
            }

            foreach (UIElement element in panelMain.Children)
            {
                if (element is Grid && (element as Grid).Tag.ToString() == "2x2 grid")
                {
                    Grid statGrid = element as Grid;
                    htmlBuilder.AppendLine("<h3>");
                    htmlBuilder.AppendLine("CTABLES COMPLEX SAMPLE DESIGN ANALYSIS OF 2 X 2 TABLE");
                    htmlBuilder.AppendLine("</h3>");
                    htmlBuilder.AppendLine("<div style=\"height: 7px;\"></div>");
                    htmlBuilder.AppendLine("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\">");

                    for (int i = 1; i < statGrid.RowDefinitions.Count; i++)
                    {
                        for (int j = 0; j < statGrid.ColumnDefinitions.Count; j++)
                        {
                            string tableDataTagOpen = "<td>";
                            string tableDataTagClose = "</td>";

                            if (j == 0)
                            {
                                htmlBuilder.AppendLine("<tr>");
                            }
                            if (j == 0 && i > 0)
                            {
                                tableDataTagOpen = "<td class=\"value\">";
                            }

                            IEnumerable<UIElement> elements = statGrid.Children.Cast<UIElement>().Where(x => Grid.GetRow(x) == i && Grid.GetColumn(x) == j);
                            TextBlock txt = null;
                            foreach (UIElement e in elements)
                            {
                                if (e is TextBlock)
                                {
                                    txt = e as TextBlock;
                                }
                            }

                            string value = "&nbsp;";

                            if (txt != null)
                            {
                                value = txt.Text;
                            }

                            htmlBuilder.AppendLine(tableDataTagOpen + value + tableDataTagClose);

                            if (j >= statGrid.ColumnDefinitions.Count - 1)
                            {
                                htmlBuilder.AppendLine("</tr>");
                            }
                        }
                    }
                    htmlBuilder.AppendLine("</table>");  
                }
            }

            return htmlBuilder.ToString();
        }

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
        #endregion  
    }
}
