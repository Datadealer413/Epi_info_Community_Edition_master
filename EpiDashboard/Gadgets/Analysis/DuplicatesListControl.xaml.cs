using System;
using System.Data;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
using EpiDashboard.Controls;

namespace EpiDashboard.Gadgets.Analysis
{
    /// <summary>
    /// Interaction logic for DuplicatesListControl.xaml
    /// </summary>
    /// /// <remarks>
    /// This gadget is used to generate a line listing for the fields in the current data source, to include
    /// any related data columns. Columns are not sorted by default but, if using an Epi Info 7 project, they
    /// may also be sorted by their tab order (also known as the order of entry).
    /// </remarks>
    public partial class DuplicatesListControl : GadgetBase
    {
        #region Private Members
        private List<string> columnOrder = new List<string>();

        /// <summary>
        /// Bool used to determine if the gadget is being called directly from the Enter module (e.g. 'Interactive Line List' option)
        /// </summary>
        private bool isHostedByEnter;

        /// <summary>
        /// Used for maintaining the width when collapsing output
        /// </summary>
        private double currentWidth;

        private RequestUpdateStatusDelegate requestUpdateStatus;
        private CheckForCancellationDelegate checkForCancellation;

        #endregion // Private Members
        
        #region Events
        public event Mapping.RecordSelectedHandler RecordSelected;
        #endregion // Events

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public DuplicatesListControl()
        {
            InitializeComponent();
            Construct();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dashboardHelper">The dashboard helper object to attach</param>
        public DuplicatesListControl(DashboardHelper dashboardHelper)
        {
            InitializeComponent();
            this.DashboardHelper = dashboardHelper;
            this.IsHostedByEnter = false;
            Construct();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dashboardHelper">The dashboard helper object to attach</param>
        /// <param name="hostedByEnter">Whether the control is hosted by Enter</param>
        public DuplicatesListControl(DashboardHelper dashboardHelper, bool hostedByEnter)
        {
            InitializeComponent();
            this.DashboardHelper = dashboardHelper;
            this.IsHostedByEnter = hostedByEnter;
            Construct();
        }
        #endregion // Constructors

        #region Public Methods
        /// <summary>
        /// Returns the gadget's description as a string.
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            return "Line List Gadget";
        }

        /// <summary>
        /// Auto-selects a given page number from the list of pages
        /// </summary>
        /// <param name="pageNumber">The page number</param>
        public void SelectPageNumber(int pageNumber)
        {
        }

        private DataGrid GetDataGrid()
        {
            DataGrid dg = null;
            foreach (UIElement element in panelMain.Children)
            {
                if (element is DataGrid)
                {
                    dg = element as DataGrid;
                }
            }

            return dg;
        }

        /// <summary>
        /// Copies a grid's output to the clipboard
        /// </summary>
        protected override void CopyToClipboard()
        {
            DataGrid dg = GetDataGrid();

            if (dg != null)
            {
                if (dg.ItemsSource is DataView)
                {
                    Common.CopyDataViewToClipboard(dg.ItemsSource as DataView);
                }
                else if (dg.ItemsSource is ListCollectionView)
                {
                    ListCollectionView lcv = dg.ItemsSource as ListCollectionView;
                    if (lcv.SourceCollection is DataView)
                    {
                        Common.CopyDataViewToClipboard(lcv.SourceCollection as DataView);
                    }
                }
            }
        }

        Controls.GadgetProperties.DuplicatesListProperties properties = null;
        public override void ShowHideConfigPanel()
        {
            Popup = new DashboardPopup();
            Popup.Parent = ((this.Parent as DragCanvas).Parent as ScrollViewer).Parent as Grid;
            properties = new Controls.GadgetProperties.DuplicatesListProperties(this.DashboardHelper, this, (DuplicatesListParameters)Parameters, StrataGridList, columnOrder);

            if (DashboardHelper.ResizedWidth != 0 & DashboardHelper.ResizedHeight != 0)
            {
                double i_StandardHeight = System.Windows.SystemParameters.PrimaryScreenHeight;//Developer Desktop Width Where the Form is Designed
                double i_StandardWidth = System.Windows.SystemParameters.PrimaryScreenWidth; ////Developer Desktop Height Where the Form is Designed
                float f_HeightRatio = new float();
                float f_WidthRatio = new float();
                f_HeightRatio = (float)((float)DashboardHelper.ResizedHeight / (float)i_StandardHeight);
                f_WidthRatio = (float)((float)DashboardHelper.ResizedWidth / (float)i_StandardWidth);

                properties.Height = (Convert.ToInt32(i_StandardHeight * f_HeightRatio)) / 1.07;
                properties.Width = (Convert.ToInt32(i_StandardWidth * f_WidthRatio)) / 1.07;

            }
            else
            {
                properties.Width = (System.Windows.SystemParameters.PrimaryScreenWidth / 1.07);
                properties.Height = (System.Windows.SystemParameters.PrimaryScreenHeight / 1.15);
            }

            properties.Cancelled += new EventHandler(properties_Cancelled);
            properties.ChangesAccepted += new EventHandler(properties_ChangesAccepted);
            Popup.Content = properties;
            Popup.Show();
        }

        public override void GadgetBase_SizeChanged(double width, double height)
        {
            double i_StandardHeight = System.Windows.SystemParameters.PrimaryScreenHeight;//Developer Desktop Width Where the Form is Designed
            double i_StandardWidth = System.Windows.SystemParameters.PrimaryScreenWidth; ////Developer Desktop Height Where the Form is Designed
            float f_HeightRatio = new float();
            float f_WidthRatio = new float();
            f_HeightRatio = (float)((float)height / (float)i_StandardHeight);
            f_WidthRatio = (float)((float)width / (float)i_StandardWidth);

            if (properties == null)
                properties = new Controls.GadgetProperties.DuplicatesListProperties(this.DashboardHelper, this, (DuplicatesListParameters)Parameters, StrataGridList, columnOrder);
            properties.Height = (Convert.ToInt32(i_StandardHeight * f_HeightRatio)) / 1.07;
            properties.Width = (Convert.ToInt32(i_StandardWidth * f_WidthRatio)) / 1.07;

        }


        private void properties_ChangesAccepted(object sender, EventArgs e)
        {
            Controls.GadgetProperties.DuplicatesListProperties properties = Popup.Content as Controls.GadgetProperties.DuplicatesListProperties;
            this.Parameters = properties.Parameters;
            this.DataFilters = properties.DataFilters;
            this.CustomOutputHeading = Parameters.GadgetTitle;
            this.CustomOutputDescription = Parameters.GadgetDescription;
            Popup.Close();
            if (properties.HasSelectedFields)
            {
                RefreshResults();
            }
        }

        private void properties_Cancelled(object sender, EventArgs e)
        {
            Popup.Close();
        }

        /// <summary>
        /// Clears the gadget's output
        /// </summary>
        public void ClearResults()
        {
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;
            messagePanel.Text = string.Empty;
            descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;

            panelMain.Children.Clear();
        }
        #endregion // Public Methods

        private void AddDataGrid(DataView dv, string strataValue)
        {
            DataGrid dg = new DataGrid();
            dg.Style = this.Resources["LineListDataGridStyle"] as Style;

            DuplicatesListParameters ListParameters = (this.Parameters) as DuplicatesListParameters;

            FrameworkElementFactory datagridRowsPresenter = new FrameworkElementFactory(typeof(DataGridRowsPresenter));
            ItemsPanelTemplate itemsPanelTemplate = new ItemsPanelTemplate();
            itemsPanelTemplate.VisualTree = datagridRowsPresenter;
            GroupStyle groupStyle = new GroupStyle();
            groupStyle.ContainerStyle = this.Resources["DefaultGroupItemStyle"] as Style;
            groupStyle.Panel = itemsPanelTemplate;
            dg.GroupStyle.Add(groupStyle);

            GroupStyle groupStyle2 = new GroupStyle();
            groupStyle2.HeaderTemplate = this.Resources["GroupDataTemplate"] as DataTemplate;
            //groupStyle.Panel = itemsPanelTemplate;
            dg.GroupStyle.Add(groupStyle2);

            string groupVar = String.Empty;

            if (!String.IsNullOrEmpty(ListParameters.PrimaryGroupField.Trim()))
            {
                groupVar = ListParameters.PrimaryGroupField.Trim();
                ListCollectionView lcv = new ListCollectionView(dv);
                lcv.GroupDescriptions.Add(new PropertyGroupDescription(groupVar));
                if (!String.IsNullOrEmpty(ListParameters.SecondaryGroupField.Trim()) && !ListParameters.SecondaryGroupField.Trim().Equals(groupVar))
                {
                    lcv.GroupDescriptions.Add(new PropertyGroupDescription(ListParameters.SecondaryGroupField.Trim())); // for second category
                }
                dg.ItemsSource = lcv;
            }
            else
            {
                dg.ItemsSource = dv;
            }

            if (Parameters.Height.HasValue)
            {
                dg.MaxHeight = Parameters.Height.Value;
            }
            else
            {
                dg.MaxHeight = 700;
            }

            if (Parameters.Width.HasValue)
            {
                dg.MaxWidth = Parameters.Width.Value;
            }
            else
            {
                dg.MaxWidth = 900;
            }

            if (ListParameters.ShowColumnHeadings == false)
            {
                dg.ColumnHeaderHeight = 0;
            }

            //dg.HeadersVisibility = DataGridHeadersVisibility.All;
            if (ListParameters.ShowLineColumn == true)
            {
                dg.RowHeaderWidth = 25;
            }
            else
            {
                dg.RowHeaderWidth = 0;
            }

            dg.AutoGeneratedColumns += new EventHandler(dg_AutoGeneratedColumns);
            dg.AutoGeneratingColumn += new EventHandler<DataGridAutoGeneratingColumnEventArgs>(dg_AutoGeneratingColumn);
            dg.LoadingRow += new EventHandler<DataGridRowEventArgs>(dg_LoadingRow);

            panelMain.Children.Add(dg);
        }

        void dg_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            e.Column.IsReadOnly = true;

            DataGridTextColumn dataGridTextColumn = e.Column as DataGridTextColumn;
            if (dataGridTextColumn != null)
            {
                if (e.PropertyType == typeof(DateTime))
                {
                    dataGridTextColumn.CellStyle = this.Resources["RightAlignDataGridCellStyle"] as Style;
                    Field field = DashboardHelper.GetAssociatedField(e.Column.Header.ToString());
                    if (field != null && field is DateField)
                    {
                        dataGridTextColumn.Binding.StringFormat = "{0:d}";
                    }
                    else if (field != null && field is TimeField)
                    {
                        dataGridTextColumn.Binding.StringFormat = "{0:t}";
                    }
                }
                else if (e.PropertyType == typeof(int) ||
                    e.PropertyType == typeof(byte) ||
                    e.PropertyType == typeof(decimal) ||
                    e.PropertyType == typeof(double) ||
                    e.PropertyType == typeof(float))
                {
                    dataGridTextColumn.CellStyle = this.Resources["RightAlignDataGridCellStyle"] as Style;
                }
            }
        }

        void dg_AutoGeneratedColumns(object sender, EventArgs e)
        {
            DataGrid dg = (sender as DataGrid);
            DataView dv = dg.ItemsSource as DataView;
            DuplicatesListParameters dlParameters = (DuplicatesListParameters)Parameters;

            int columnCount = 0;
            foreach (DataColumn column in dv.Table.Columns)
            {
                Field field = DashboardHelper.GetAssociatedField(column.ColumnName);
                if (field != null && field is RenderableField)
                {
                    if (dlParameters.UseFieldPrompts == true)
                    {
                        dg.Columns[columnCount].Header = (((RenderableField)field).PromptText);
                    }
                }
                columnCount++;
            }
        }

        void dg_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }


        #region Event Handlers

        /// <summary>
        /// Handles the WorkerCompleted event for the worker
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        protected override void worker_WorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            Debug.Print("Background worker thread for line list gadget was cancelled or ran to completion.");
            this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
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
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToProcessingState));
                this.Dispatcher.BeginInvoke(new SimpleCallback(ClearResults));
                AddDataGridDelegate addDataGrid = new AddDataGridDelegate(AddDataGrid);

                try
                {
                    Parameters.GadgetStatusUpdate += new GadgetStatusUpdateHandler(requestUpdateStatus);
                    Parameters.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(checkForCancellation);
                    if (this.DataFilters != null)
                    {
                        Parameters.CustomFilter = this.DataFilters.GenerateDataFilterString(false);
                    }
                    else
                    {
                        Parameters.CustomFilter = string.Empty;
                    }

                    List<DataTable> lineListTables = new List<DataTable>();
                    lineListTables.Add(DashboardHelper.GenerateDuplicatesTable(Parameters as DuplicatesListParameters));

                    if (lineListTables == null || lineListTables.Count == 0)
                    {
                        this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), SharedStrings.NO_RECORDS_SELECTED);
                        this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                        Debug.Print("Line list thread cancelled");
                        return;
                    }
                    else if (lineListTables.Count == 1 && lineListTables[0].Rows.Count == 0)
                    {
                        this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), SharedStrings.NO_RECORDS_SELECTED);
                        this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                        Debug.Print("Line list thread cancelled");
                        return;
                    }
                    else if (worker.CancellationPending)
                    {
                        this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), SharedStrings.DASHBOARD_GADGET_STATUS_OPERATION_CANCELLED);
                        this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                        Debug.Print("Line list thread cancelled");
                        return;
                    }
                    else
                    {
                        string formatString = string.Empty;

                        foreach (DataTable listTable in lineListTables)
                        {
                            string strataValue = listTable.TableName;
                            if (listTable.Rows.Count == 0)
                            {
                                continue;
                            }
                        }

                        SetGadgetStatusMessage(SharedStrings.DASHBOARD_GADGET_STATUS_DISPLAYING_OUTPUT);

                        foreach (DataTable listTable in lineListTables)
                        {
                            string strataValue = listTable.TableName;
                            if (listTable.Rows.Count == 0)
                            {
                                continue;
                            }
                            string tableHeading = listTable.TableName;
                            SetCustomColumnSort(listTable);

                            if (listTable.Columns.Count == 0)
                            {
                                throw new ApplicationException("There are no columns to display in this list. If specifying a group variable, ensure the group variable contains data fields.");
                            }

                            int[] totals = new int[listTable.Columns.Count - 1];

                            this.Dispatcher.BeginInvoke(addDataGrid, listTable.AsDataView(), listTable.TableName);
                        }

                        //for(int i = 0; i < lineListTables.Count; i++)
                        //{
                        //    lineListTables[i].Dispose();
                        //}
                        //lineListTables.Clear();
                    }

                    this.Dispatcher.BeginInvoke(new SimpleCallback(RenderFinish));
                }
                catch (Exception ex)
                {
                    this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), ex.Message);
                }
                finally
                {   
                    stopwatch.Stop();
                    Debug.Print("Line list gadget took " + stopwatch.Elapsed.ToString() + " seconds to complete with " + DashboardHelper.RecordCount.ToString() + " records and the following filters:");
                    Debug.Print(DashboardHelper.DataFilters.GenerateDataFilterString());
                }
            }
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Used to construct the gadget and assign events
        /// </summary>
        /// <param name="dashboardHelper">The dashboard helper to attach to this gadget</param>
        protected override void Construct()
        {
            currentWidth = borderAll.ActualWidth;
            this.Parameters = new DuplicatesListParameters();

            if (!string.IsNullOrEmpty(CustomOutputHeading))
            {
                headerPanel.Text = CustomOutputHeading;
            }

            requestUpdateStatus = new RequestUpdateStatusDelegate(RequestUpdateStatusMessage);
            checkForCancellation = new CheckForCancellationDelegate(IsCancelled);

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
            if (!IsHostedByEnter)
            {
                mnuSendToBack.Click += new RoutedEventHandler(mnuSendToBack_Click);
                mnuClose.Click += new RoutedEventHandler(mnuClose_Click);
                mnuCenter.Click += new RoutedEventHandler(mnuCenter_Click);
                mnuSendDataToHTML.Visibility = System.Windows.Visibility.Visible;
                mnuClose.Visibility = System.Windows.Visibility.Visible;
                //mnuCenter.Visibility = System.Windows.Visibility.Collapsed;//Visible; // TODO: Re-enable.
            }
            else
            {
                mnuSendDataToHTML.Visibility = System.Windows.Visibility.Collapsed;
                mnuClose.Visibility = System.Windows.Visibility.Collapsed;
                mnuCenter.Visibility = System.Windows.Visibility.Collapsed;
                borderAll.ContextMenu = null;
                headerPanel.IsCloseButtonAvailable = false;
                headerPanel.IsDescriptionButtonAvailable = false;
                headerPanel.IsFilterButtonAvailable = false;
                headerPanel.IsOutputCollapseButtonAvailable = false;
            }

            this.IsProcessing = false;

            this.GadgetStatusUpdate += new GadgetStatusUpdateHandler(RequestUpdateStatusMessage);
            this.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(IsCancelled);
            mnuRemoveSorts.Click += mnuRemoveSorts_Click;
            base.Construct();

            #region Translation
            mnuCenter.Header = DashboardSharedStrings.GADGET_CENTER;
            mnuClose.Header = LineListSharedStrings.CMENU_CLOSE;
            mnuCopy.Header = LineListSharedStrings.CMENU_COPY;
            mnuRemoveSorts.Header = LineListSharedStrings.CMENU_REMOVE_SORTING;
            mnuSendDataToExcel.Header = LineListSharedStrings.CMENU_EXCEL;
            mnuSendDataToHTML.Header = LineListSharedStrings.CMENU_HTML;
            mnuSendToBack.Header = LineListSharedStrings.CMENU_SEND_TO_BACK;

            //tblockListVariablesToDisplay.Text = LineListSharedStrings.LIST_VARIABLES_TO_DISPLAY;
            //tblockSortVariables.Text = LineListSharedStrings.SORT_VARIABLES;
            //tblockSortOrder.Text = LineListSharedStrings.SORT_ORDER;
            //tblockLineListProperties.Text = LineListSharedStrings.LIST_CONFIG_TITLE;
            //tblockGroupResultsBy.Text = LineListSharedStrings.GROUP_RESULTS_BY;
            //tblockMaxVarNameLength.Text = LineListSharedStrings.MAX_VAR_NAME_LENGTH;
            //tblockMaxRows.Text = LineListSharedStrings.MAX_ROWS_TO_DISPLAY;

            //tblockMaxWidth.Text = DashboardSharedStrings.GADGET_MAX_WIDTH;
            //tblockMaxHeight.Text = DashboardSharedStrings.GADGET_MAX_HEIGHT;

            //tblockInst1.Text = LineListSharedStrings.INSTRUCTIONS_1;
            //tblockInst2.Text = LineListSharedStrings.INSTRUCTIONS_2;

            //checkboxTabOrder.Content = LineListSharedStrings.SORT_VARS_TAB_ORDER;
            //checkboxUsePrompts.Content = LineListSharedStrings.USE_FIELD_PROMPTS;
            //checkboxListLabels.Content = DashboardSharedStrings.GADGET_LIST_LABELS;
            //checkboxAllowUpdates.Content = LineListSharedStrings.ALLOW_UPDATES;
            //checkboxLineColumn.Content = LineListSharedStrings.SHOW_LINE_COLUMN;
            //checkboxColumnHeaders.Content = LineListSharedStrings.SHOW_COLUMN_HEADINGS;
            //checkboxShowNulls.Content = LineListSharedStrings.SHOW_MISSING_REPRESENTATION;

            //btnRun.Content = LineListSharedStrings.GENERATE_LINE_LIST;
            #endregion // Translation
        }

        void mnuRemoveSorts_Click(object sender, RoutedEventArgs e)
        {
            if (properties != null)
            {
                Parameters.SortVariables.Clear();
                RefreshResults();
            }
        }

        void mnuCenter_Click(object sender, RoutedEventArgs e)
        {
            CenterGadget();
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

            if (Parameters != null)
            {
                Parameters.GadgetStatusUpdate -= new GadgetStatusUpdateHandler(requestUpdateStatus);
                Parameters.GadgetCheckForCancellation -= new GadgetCheckForCancellationHandler(checkForCancellation);
                Parameters = null;
            }

            this.GadgetStatusUpdate -= new GadgetStatusUpdateHandler(RequestUpdateStatusMessage);
            this.GadgetCheckForCancellation -= new GadgetCheckForCancellationHandler(IsCancelled);

            panelMain.Children.Clear();

            requestUpdateStatus = null;
            checkForCancellation = null;

            if (!IsHostedByEnter)
            {
                mnuSendToBack.Click -= new RoutedEventHandler(mnuSendToBack_Click);
                mnuClose.Click -= new RoutedEventHandler(mnuClose_Click);
            }

            base.CloseGadget();

            Parameters = null;
        }

        private void SetCustomColumnSort(DataTable table)
        {
            #region Input Validation
            if (table == null)
            {
                throw new ArgumentNullException("table");
            }
            #endregion // Input Validation

            if (columnOrder == null || columnOrder.Count == 0)
            {
                return;
            }

            if (columnOrder.Contains("Line"))
            {
                columnOrder.Remove("Line");
            }

            for (int i = 0; i < columnOrder.Count; i++)
            {
                string s = columnOrder[i];
                if (table.Columns.Contains(s))
                {
                    table.Columns[s].SetOrdinal(i);
                }
            }
        }

        /// <summary>
        /// Sets the gadget's state to 'finished' mode
        /// </summary>
        protected override void RenderFinish()
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed;

            messagePanel.MessagePanelType = Controls.MessagePanelType.StatusPanel;
            messagePanel.Text = string.Empty;
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;

            HideConfigPanel();
            CheckAndSetPosition();
        }

        protected override void RenderFinishWithWarning(string errorMessage)
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed;

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
        protected override void RenderFinishWithError(string errorMessage)
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed;

            messagePanel.MessagePanelType = Controls.MessagePanelType.ErrorPanel;
            messagePanel.Text = errorMessage;
            messagePanel.Visibility = System.Windows.Visibility.Visible;

            HideConfigPanel();
            CheckAndSetPosition();
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

        public override void CollapseOutput()
        {
            borderAll.MinWidth = currentWidth;
            panelMain.Visibility = System.Windows.Visibility.Collapsed;

            this.messagePanel.Visibility = System.Windows.Visibility.Collapsed;
            this.infoPanel.Visibility = System.Windows.Visibility.Collapsed;
            //descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed; //EI-24
            IsCollapsed = true;
        }

        public override void ExpandOutput()
        {
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

            panelMain.Visibility = System.Windows.Visibility.Visible;

            IsCollapsed = false;
        }

        protected override void ShowCustomFilterDialog()
        {
            base.ShowCustomFilterDialog();
        }

        protected override void SetCustomFilterDisplay()
        {
            if (this.DataFilters != null && this.DataFilters.Count > 0)
            {
                infoPanel.MaxWidth = this.ActualWidth;
                infoPanel.Text = String.Format(DashboardSharedStrings.GADGET_CUSTOM_FILTER, this.DataFilters.GenerateReadableDataFilterString());
                infoPanel.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                infoPanel.Text = string.Empty;
                infoPanel.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Initiates a refresh of the gadget's output
        /// </summary>
        public override void RefreshResults()
        {
            DuplicatesListParameters listParameters = (DuplicatesListParameters)Parameters;

            if (!LoadingCombos && Parameters != null && listParameters.KeyColumnNames.Count > 0)
            {
                if (IsHostedByEnter)
                {
                    HideConfigPanel(); 
                }
                waitPanel.Visibility = System.Windows.Visibility.Visible;

                messagePanel.MessagePanelType = Controls.MessagePanelType.StatusPanel;
                descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;

                baseWorker = new BackgroundWorker();
                baseWorker.WorkerSupportsCancellation = true;
                baseWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(Execute);
                baseWorker.RunWorkerAsync();

                base.RefreshResults();
            }
            else
            {
                return;
            }

            base.RefreshResults();
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
            DuplicatesListParameters listParameters = (DuplicatesListParameters)Parameters;

            System.Xml.XmlElement element = doc.CreateElement("duplicatesListGadget");
            element.AppendChild(SerializeFilters(doc));

            System.Xml.XmlAttribute id = doc.CreateAttribute("id");
            System.Xml.XmlAttribute locationY = doc.CreateAttribute("top");
            System.Xml.XmlAttribute locationX = doc.CreateAttribute("left");
            System.Xml.XmlAttribute collapsed = doc.CreateAttribute("collapsed");
            System.Xml.XmlAttribute type = doc.CreateAttribute("gadgetType");
            System.Xml.XmlAttribute actualHeight = doc.CreateAttribute("actualHeight");

            id.Value = this.UniqueIdentifier.ToString();
            locationY.Value = Canvas.GetTop(this).ToString("F0");
            locationX.Value = Canvas.GetLeft(this).ToString("F0");
            collapsed.Value = IsCollapsed.ToString();
            type.Value = "EpiDashboard.Gadgets.Analysis.DuplicatesListControl";
            actualHeight.Value = this.ActualHeight.ToString();

            element.Attributes.Append(locationY);
            element.Attributes.Append(locationX);
            element.Attributes.Append(collapsed);
            element.Attributes.Append(type);
            element.Attributes.Append(id);
            if (IsCollapsed == false)
            {
                element.Attributes.Append(actualHeight);
            }

            string groupVar1 = String.Empty;
            string groupVar2 = String.Empty;

            if (!String.IsNullOrEmpty(listParameters.PrimaryGroupField))
            {
                groupVar1 = listParameters.PrimaryGroupField;
            }
            if (!String.IsNullOrEmpty(listParameters.SecondaryGroupField))
            {
                groupVar2 = listParameters.SecondaryGroupField;
            }

            XmlElement groupElement = doc.CreateElement("groupVariable");
            groupElement.InnerText = groupVar1;
            element.AppendChild(groupElement);

            XmlElement groupElement2 = doc.CreateElement("groupVariableSecondary");
            groupElement2.InnerText = groupVar2;
            element.AppendChild(groupElement2);

            XmlElement tabOrderElement = doc.CreateElement("sortColumnsByTabOrder");
            tabOrderElement.InnerText = listParameters.SortColumnsByTabOrder.ToString();
            element.AppendChild(tabOrderElement);

            XmlElement usePromptsElement = doc.CreateElement("useFieldPrompts");
            usePromptsElement.InnerText = listParameters.UseFieldPrompts.ToString();
            element.AppendChild(usePromptsElement);

            XmlElement showListLabelsElement = doc.CreateElement("showListLabels");
            showListLabelsElement.InnerText = listParameters.ShowCommentLegalLabels.ToString();
            element.AppendChild(showListLabelsElement);

            XmlElement showLineColumnElement = doc.CreateElement("showLineColumn");
            showLineColumnElement.InnerText = listParameters.ShowLineColumn.ToString();
            element.AppendChild(showLineColumnElement);

            XmlElement showColumnHeadersElement = doc.CreateElement("showColumnHeadings");
            showColumnHeadersElement.InnerText = listParameters.ShowColumnHeadings.ToString();
            element.AppendChild(showColumnHeadersElement);

            XmlElement showNullLabelsElement = doc.CreateElement("showNullLabels");
            showNullLabelsElement.InnerText = listParameters.ShowNullLabels.ToString();
            element.AppendChild(showNullLabelsElement);

            XmlElement customHeadingElement = doc.CreateElement("customHeading");
            customHeadingElement.InnerText = listParameters.GadgetTitle; // CustomOutputHeading.Replace("<", "&lt;");
            element.AppendChild(customHeadingElement);

            XmlElement customDescElement = doc.CreateElement("customDescription");
            customDescElement.InnerText = listParameters.GadgetDescription; // CustomOutputDescription.Replace("<", "&lt;");
            element.AppendChild(customDescElement);

            SerializeAnchors(element);

            XmlElement listKeyItemElement = doc.CreateElement("listKeyFields");
              string xmlListKeyItemString = string.Empty;
            
            foreach (string columnName in listParameters.KeyColumnNames)
            {
                xmlListKeyItemString = xmlListKeyItemString + "<listField>" + columnName.Replace("<", "&lt;") + "</listField>";
            }

            listKeyItemElement.InnerXml = xmlListKeyItemString;

            if (!String.IsNullOrEmpty(xmlListKeyItemString))
            {
                element.AppendChild(listKeyItemElement);
            }

            XmlElement listDisplayItemElement = doc.CreateElement("listDisplayFields");
            string xmlListDisplayItemString = string.Empty;

            foreach (string columnName in listParameters.ColumnNames)
            {
                xmlListDisplayItemString = xmlListDisplayItemString + "<listField>" + columnName.Replace("<", "&lt;") + "</listField>";
            }

            listDisplayItemElement.InnerXml = xmlListDisplayItemString;

            if (!String.IsNullOrEmpty(xmlListDisplayItemString))
            {
                element.AppendChild(listDisplayItemElement);
            }

            XmlElement sortItemElement = doc.CreateElement("sortFields");

            string xmlSortItemString = string.Empty;

            foreach (KeyValuePair<string, SortOrder> kvp in Parameters.SortVariables)
            {
                if (kvp.Value == SortOrder.Descending)
                {
                    xmlSortItemString = xmlSortItemString + "<sortField order=\"DESC\">" + kvp.Key.Replace("<", "&lt;") + "</sortField>";
                }
                else
                {
                    xmlSortItemString = xmlSortItemString + "<sortField order=\"ASC\">" + kvp.Key.Replace("<", "&lt;") + "</sortField>";
                }
            }

            sortItemElement.InnerXml = xmlSortItemString;

            if (!String.IsNullOrEmpty(xmlSortItemString))
            {
                element.AppendChild(sortItemElement);
            }

            return element;
        }

        /// <summary>
        /// Creates the line list gadget from an Xml element
        /// </summary>
        /// <param name="element">The element from which to create the gadget</param>
        public override void CreateFromXml(XmlElement element)
        {
            this.LoadingCombos = true;
            this.Parameters = new DuplicatesListParameters();

            HideConfigPanel();

            infoPanel.Visibility = System.Windows.Visibility.Collapsed;
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;

            foreach (XmlElement child in element.ChildNodes)
            {
                switch (child.Name.ToLowerInvariant())
                {
                    case "groupvariable":
                    case "groupvariableprimary":
                        if (!String.IsNullOrEmpty(child.InnerText.Trim()))
                        {
                            ((DuplicatesListParameters)Parameters).PrimaryGroupField = child.InnerText.Trim();
                        }
                        break;
                    case "groupvariablesecondary":
                        if (!String.IsNullOrEmpty(child.InnerText.Trim()))
                        {
                            ((DuplicatesListParameters)Parameters).SecondaryGroupField = child.InnerText.Trim();
                        }
                        break;
                    case "sortcolumnsbytaborder":
                        bool sortByTabs = false;
                        bool.TryParse(child.InnerText, out sortByTabs);
                        ((DuplicatesListParameters)Parameters).SortColumnsByTabOrder = sortByTabs;
                        break;
                    case "usefieldprompts":
                        bool usePrompts = false;
                        bool.TryParse(child.InnerText, out usePrompts);
                        ((DuplicatesListParameters)Parameters).UseFieldPrompts = usePrompts;
                        break;
                    case "showlinecolumn":
                        bool showLineColumn = true;
                        bool.TryParse(child.InnerText, out showLineColumn);
                        ((DuplicatesListParameters)Parameters).ShowLineColumn = showLineColumn;
                        break;
                    case "showcolumnheadings":
                        bool showColumnHeadings = true;
                        bool.TryParse(child.InnerText, out showColumnHeadings);
                        ((DuplicatesListParameters)Parameters).ShowColumnHeadings = showColumnHeadings;
                        break;
                    case "showlistlabels":
                        bool showLabels = false;
                        bool.TryParse(child.InnerText, out showLabels);
                        Parameters.ShowCommentLegalLabels = showLabels;
                        break;
                    case "shownulllabels":
                        bool showNullLabels = true;
                        bool.TryParse(child.InnerText, out showNullLabels);
                        ((DuplicatesListParameters)Parameters).ShowNullLabels = showNullLabels;
                        break;
                    case "customheading":
                        if (!string.IsNullOrEmpty(child.InnerText))
                        {
                            this.CustomOutputHeading = child.InnerText.Replace("&lt;", "<");
                            Parameters.GadgetTitle = CustomOutputHeading;
                        }
                        break;
                    case "customdescription":
                        if (!string.IsNullOrEmpty(child.InnerText))
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
                    case "customcaption":
                        this.CustomOutputCaption = child.InnerText;
                        break;
                    case "listkeyfields":
                        foreach (XmlElement field in child.ChildNodes)
                        {
                            List<string> fields = new List<string>();
                            if (field.Name.ToLowerInvariant().Equals("listfield"))
                            {
                                //GadgetOptions.InputVariableList.Add(field.InnerText.Replace("&lt;", "<"), "listfield");
                                ((DuplicatesListParameters)Parameters).KeyColumnNames.Add(field.InnerText.Replace("&lt;", "<"));
                            }
                        }
                        break;
                    case "listdisplayfields":
                        foreach (XmlElement field in child.ChildNodes)
                        {
                            List<string> fields = new List<string>();
                            if (field.Name.ToLowerInvariant().Equals("listfield"))
                            {
                                //GadgetOptions.InputVariableList.Add(field.InnerText.Replace("&lt;", "<"), "listfield");
                                Parameters.ColumnNames.Add(field.InnerText.Replace("&lt;", "<"));
                            }
                        }
                        break;
                    case "sortfields":
                        foreach (XmlElement field in child.ChildNodes)
                        {
                            List<string> fields = new List<string>();
                            
                            SortOrder order = SortOrder.Ascending;

                            if (field.Attributes.Count >= 1 && field.Attributes["order"].Value == "DESC")
                            {
                                order = SortOrder.Descending;
                            }

                            if (field.Name.ToLowerInvariant().Equals("sortfield"))
                            {
                                Parameters.SortVariables.Add(field.InnerText.Replace("&lt;", "<"), order);
                            }
                        }
                        break;
                    case "customusercolumnsort":
                        string[] cols = child.InnerText.Split('^');
                        columnOrder = cols.ToList();
                        break;
                    case "datafilters":
                        this.DataFilters = new DataFilters(this.DashboardHelper);
                        this.DataFilters.CreateFromXml(child);
                        break;
                }
            }

            base.CreateFromXml(element);

            this.LoadingCombos = false;

            RefreshResults();
            HideConfigPanel();
        }

        #endregion

        /// <summary>
        /// Sets the gadget to its 'processing' state
        /// </summary>
        public override void SetGadgetToProcessingState()
        {
            this.IsProcessing = true;
            base.SetGadgetToProcessingState();
        }

        /// <summary>
        /// Sets the gadget to its 'finished' state
        /// </summary>
        public override void SetGadgetToFinishedState()
        {
            this.IsProcessing = false;
            currentWidth = borderAll.ActualWidth;
            base.SetGadgetToFinishedState();
        }

        /// <summary>
        /// Converts the gadget's output to Html
        /// </summary>
        /// <returns></returns>
        public override string ToHTML(string htmlFileName = "", int count = 0, bool useAlternatingColors = false)
        {
            if (IsCollapsed) return string.Empty;

            StringBuilder htmlBuilder = new StringBuilder();
            CustomOutputHeading = headerPanel.Text;
            CustomOutputDescription = descriptionPanel.Text;

            if (CustomOutputHeading == null || (string.IsNullOrEmpty(CustomOutputHeading) && !CustomOutputHeading.Equals("(none)")))
            {
                htmlBuilder.AppendLine("<h2 class=\"gadgetHeading\">Line List</h2>");
            }
            else if (CustomOutputHeading != "(none)")
            {
                htmlBuilder.AppendLine("<h2 class=\"gadgetHeading\">" + CustomOutputHeading + "</h2>");
            }

            if (!string.IsNullOrEmpty(CustomOutputDescription) && CustomOutputDescription != "(none)")
            {
                htmlBuilder.AppendLine("<p class=\"gadgetsummary\">" + CustomOutputDescription + "</p>");
            }

            if (!string.IsNullOrEmpty(messagePanel.Text) && messagePanel.Visibility == Visibility.Visible)
            {
                htmlBuilder.AppendLine("<p><small><strong>" + messagePanel.Text + "</strong></small></p>");
            }

            DataGrid dg = GetDataGrid();
            if (dg != null && dg.ItemsSource != null)
            {
                htmlBuilder.AppendLine("<div style=\"height: 7px;\"></div>");
                htmlBuilder.AppendLine("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\">");
                if (string.IsNullOrEmpty(CustomOutputCaption) && this.StrataGridList.Count > 1)
                {
                    //htmlBuilder.AppendLine("<caption>" + grid.Tag + "</caption>");
                }
                else if (!string.IsNullOrEmpty(CustomOutputCaption))
                {
                    htmlBuilder.AppendLine("<caption>" + CustomOutputCaption + "</caption>");
                }

                if (dg.ItemsSource is DataView)
                {
                    htmlBuilder.AppendLine(Common.ConvertDataViewToHtmlString(dg.ItemsSource as DataView, useAlternatingColors));
                }
                else if (dg.ItemsSource is ListCollectionView)
                {
                    ListCollectionView lcv = dg.ItemsSource as ListCollectionView;
                    if (lcv.SourceCollection is DataView)
                    {
                        htmlBuilder.AppendLine(Common.ConvertDataViewToHtmlString(lcv.SourceCollection as DataView, useAlternatingColors));
                    }
                }

                htmlBuilder.AppendLine("</table>");
            }

            return htmlBuilder.ToString();
        }

        private string customOutputHeading;
        private string customOutputDescription;
        private string customOutputCaption;

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

        public override string CustomOutputDescription
        {
            get
            {
                return this.customOutputDescription;
            }
            set
            {
                this.customOutputDescription = value;
                //txtOutputDescription.Text = CustomOutputDescription;
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

        public bool IsHostedByEnter
        {
            get
            {
                return isHostedByEnter;
            }
            set
            {
                isHostedByEnter = value;
                // REVISIT THIS
                //if (value)
                //{
                //    imgClose.Visibility = System.Windows.Visibility.Collapsed;
                //}
                //else
                //{
                //    imgClose.Visibility = System.Windows.Visibility.Visible;
                //}
            }
        }
      
    }
}
