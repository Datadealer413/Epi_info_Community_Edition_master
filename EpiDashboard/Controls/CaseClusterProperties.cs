using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Epi;
using ESRI.ArcGIS.Client.Symbols;
using EpiDashboard.Mapping;

namespace EpiDashboard.Controls
{
    /// <summary>
    /// Interaction logic for DashboardProperties.xaml
    /// </summary>


    public partial class CaseClusterProperties : UserControl, ILayerProperties
    {

        private ESRI.ArcGIS.Client.Map myMap;
        private DashboardHelper dashboardHelper;
        public event EventHandler MapGenerated;
        public event EventHandler FilterRequested;
        public event EventHandler EditRequested;

        private EpiDashboard.Mapping.ClusterLayerProvider provider;
        private SimpleMarkerSymbol.SimpleMarkerStyle style;
        private EpiDashboard.Mapping.StandaloneMapControl mapControl;
        private IMapControl imapcontrol;
        public ClusterLayerProperties layerprop;
        private Brush colorselected;
        private RowFilterControl rowfiltercontrol;
        private DataFilters datafilters;

        public CaseClusterProperties(EpiDashboard.Mapping.StandaloneMapControl mapControl, ESRI.ArcGIS.Client.Map myMap, ClusterLayerProperties clusterprop)
        {
            InitializeComponent();

            this.myMap = myMap;
            this.mapControl = mapControl;

            mapControl.TimeVariableSet += new TimeVariableSetHandler(mapControl_TimeVariableSet);
            mapControl.MapDataChanged += new EventHandler(mapControl_MapDataChanged);

            provider = clusterprop.provider;
            layerprop = clusterprop;
            mapControl.SizeChanged += mapControl_SizeChanged;
            rctSelectColor.Fill = new SolidColorBrush(Color.FromArgb(120, 0, 0, 255));
            colorselected = new SolidColorBrush(Color.FromArgb(120, 0, 0, 255));

            #region Translation
            lblConfigExpandedTitle.Content = DashboardSharedStrings.GADGET_CONFIG_TITLE_CLUSTER;
            tbtnDataSource.Title = DashboardSharedStrings.GADGET_MAP_TABBUTTON_DATA_SOURCE;
            tbtnDataSource.Description = DashboardSharedStrings.GADGET_MAP_TABDESC_DATASOURCE;
            tbtnVariables.Title = DashboardSharedStrings.GADGET_MAP_TABBUTTON_VARIABLES;
            tbtnVariables.Description = DashboardSharedStrings.GADGET_MAP_TABDESC_VARIABLES;
            tbtnDisplay.Title = DashboardSharedStrings.GADGET_MAP_TABBUTTON_DISPLAY;
            tbtnDisplay.Description = DashboardSharedStrings.GADGET_MAP_TABDESC_DISPLAY;
            tbtnFilters.Title = DashboardSharedStrings.GADGET_MAP_TABBUTTON_FILTERS;
            tbtnFilters.Description = DashboardSharedStrings.GADGET_MAP_TABDESC_FILTERS;

            //Data Source Panel
            tblockPanelDataSource.Content = DashboardSharedStrings.GADGET_DATA_SOURCE;
            tblockDataSource.Content = DashboardSharedStrings.GADGET_DATA_SOURCE;
            btnBrowse.Content = DashboardSharedStrings.BUTTON_BROWSE;
            tblockConnectionString.Content = DashboardSharedStrings.GADGET_CONNECTION_STRING;
            tblockSQLQuery.Content = DashboardSharedStrings.GADGET_SQL_QUERY;
            tblockLoadingData.Text = DashboardSharedStrings.GADGET_LOADING_DATA;

            //Variables Panel
            tblockPanelVariables.Content = DashboardSharedStrings.GADGET_TABBUTTON_VARIABLES;
            tblockSelectVarData.Content = DashboardSharedStrings.GADGET_POF_COORD_VARIABLES;
            lblLatitude.Content = DashboardSharedStrings.GADGET_LATITUDE_FIELD;
            lblLongitude.Content = DashboardSharedStrings.GADGET_LONGITUDE_FIELD;

            //Display Panel
            tblockPanelDisplay.Content = DashboardSharedStrings.GADGET_TABBUTTON_DISPLAY;
            tblockTitleNDescSubheader.Content = DashboardSharedStrings.GADGET_LABEL_DESCRIPTION;
            tblockMapDesc.Content = DashboardSharedStrings.GADGET_LEGEND_DESCRIPTION;

            //Filters Panel
            tblockPanelDataFilter.Content = DashboardSharedStrings.GADGET_PANELHEADER_DATA_FILTER;
            tblockSetDataFilter.Content = DashboardSharedStrings.GADGET_TABDESC_FILTERS;
            tblockAnyFilterGadgetOnly.Content = DashboardSharedStrings.GADGET_FILTER_GADGET_ONLY;

            btnOK.Content = DashboardSharedStrings.BUTTON_OK;
            btnCancel.Content = DashboardSharedStrings.BUTTON_CANCEL;

            #endregion
        }

        public void mapControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            mapControl.ResizedWidth = e.NewSize.Width;
            mapControl.ResizedHeight = e.NewSize.Height;
            if (mapControl.ResizedWidth != 0 & mapControl.ResizedHeight != 0)
            {
                double i_StandardHeight = System.Windows.SystemParameters.PrimaryScreenHeight;//Developer Desktop Width Where the Form is Designed
                double i_StandardWidth = System.Windows.SystemParameters.PrimaryScreenWidth; ////Developer Desktop Height Where the Form is Designed
                float f_HeightRatio = new float();
                float f_WidthRatio = new float();
                f_HeightRatio = (float)((float)mapControl.ResizedHeight / (float)i_StandardHeight);
                f_WidthRatio = (float)((float)mapControl.ResizedWidth / (float)i_StandardWidth);

                this.Height = (Convert.ToInt32(i_StandardHeight * f_HeightRatio)) / 1.16;
                this.Width = (Convert.ToInt32(i_StandardWidth * f_WidthRatio)) / 1.13;

            }
            else
            {
                this.Width = (System.Windows.SystemParameters.PrimaryScreenWidth / 1.2);
                this.Height = (System.Windows.SystemParameters.PrimaryScreenHeight / 1.2);
            }
        }

        public event EventHandler Cancelled;
        public event EventHandler ChangesAccepted;


        public Brush ColorSelected
        {
            set
            {
                colorselected = value;
            }
        }

        public FileInfo ProjectFileInfo
        {
            get
            {
                FileInfo fi = new FileInfo(txtProjectPath.Text);
                return fi;
            }
            set
            {
                txtProjectPath.Text = value.FullName;
                panelDataSourceProject.Visibility = Visibility.Visible;
                panelDataSourceOther.Visibility = Visibility.Collapsed;
                panelDataSourceAdvanced.Visibility = Visibility.Collapsed;
            }
        }

        public string ConnectionString
        {
            get
            {
                return txtDataSource.Text;
            }
            set
            {
                txtDataSource.Text = value;
                panelDataSourceProject.Visibility = Visibility.Collapsed;
                panelDataSourceOther.Visibility = Visibility.Visible;
                panelDataSourceAdvanced.Visibility = Visibility.Collapsed;
            }
        }

        public string SqlQuery
        {
            get
            {
                return txtSQLQuery.Text;
            }
            set
            {
                txtSQLQuery.Text = value;
                panelDataSourceProject.Visibility = Visibility.Collapsed;
                panelDataSourceOther.Visibility = Visibility.Collapsed;
                panelDataSourceAdvanced.Visibility = Visibility.Visible;
            }
        }

        private void tbtnInfo_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as ToggleButton);
            panelDataSource.Visibility = System.Windows.Visibility.Collapsed;
            panelVariables.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplay.Visibility = System.Windows.Visibility.Collapsed;
            panelInfo.Visibility = System.Windows.Visibility.Visible;
            panelFilter.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tbtnVariables_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as ToggleButton);
            panelDataSource.Visibility = System.Windows.Visibility.Collapsed;
            panelVariables.Visibility = System.Windows.Visibility.Visible;
            panelDisplay.Visibility = System.Windows.Visibility.Collapsed;
            panelInfo.Visibility = System.Windows.Visibility.Collapsed;
            panelFilter.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tbtnDisplay_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as ToggleButton);
            panelDataSource.Visibility = System.Windows.Visibility.Collapsed;
            panelVariables.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplay.Visibility = System.Windows.Visibility.Visible;
            panelInfo.Visibility = System.Windows.Visibility.Collapsed;
            panelFilter.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tbtnFilter_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as ToggleButton);
            panelDataSource.Visibility = System.Windows.Visibility.Collapsed;
            panelVariables.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplay.Visibility = System.Windows.Visibility.Collapsed;
            panelFilter.Visibility = System.Windows.Visibility.Visible;
            panelInfo.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tbtnDataSource_Checked(object sender, RoutedEventArgs e)
        {
            if (panelDataSource == null) return;
            CheckButtonStates(sender as ToggleButton);
            panelDataSource.Visibility = System.Windows.Visibility.Visible;
            panelVariables.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplay.Visibility = System.Windows.Visibility.Collapsed;
            panelInfo.Visibility = System.Windows.Visibility.Collapsed;
            panelFilter.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void CheckButtonStates(ToggleButton sender)
        {
            foreach (UIElement element in panelSidebar.Children)
            {
                if (element is ToggleButton)
                {
                    ToggleButton tbtn = element as ToggleButton;
                    if (tbtn != sender)
                    {
                        tbtn.IsChecked = false;
                    }
                }
            }
        }

        private void txtProjectPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.PropertyChanged_EnableDisable();
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            waitCursor.Visibility = Visibility.Visible;
            tblockLoadingData.Visibility = Visibility.Visible;
            dashboardHelper = mapControl.GetNewDashboardHelper();
            // this.DashboardHelper = dashboardHelper;
            layerprop.SetdashboardHelper(dashboardHelper);
            waitCursor.Visibility = Visibility.Collapsed;
            tblockLoadingData.Visibility = Visibility.Collapsed;
            if (dashboardHelper != null)
            {
                txtProjectPath.Text = mapControl.ProjectFilepath;
                FillComboBoxes();
                SetFilter();
            }
        }

        #region ILayerProperties Members

        public void CloseLayer()
        {
            provider.CloseLayer();
        }

        public Color FontColor
        {
            set
            {
                SolidColorBrush brush = new SolidColorBrush(value);
                lblLatitude.Foreground = brush;
                lblLongitude.Foreground = brush;
            }
        }

        public void MakeReadOnly()
        {

        }

        public System.Xml.XmlNode Serialize(System.Xml.XmlDocument doc)
        {
            string connectionString = string.Empty;
            string tableName = string.Empty;
            string projectPath = string.Empty;
            string viewName = string.Empty;
            if (dashboardHelper.View == null)
            {
                connectionString = dashboardHelper.Database.ConnectionString;
                tableName = dashboardHelper.TableName;
            }
            else
            {
                projectPath = dashboardHelper.View.Project.FilePath;
                viewName = dashboardHelper.View.Name;
            }
            string latitude = cmbLatitude.SelectedItem.ToString();
            string longitude = cmbLongitude.SelectedItem.ToString();
            SolidColorBrush color = (SolidColorBrush)rctSelectColor.Fill;
            string description = txtDescription.Text;
            string xmlString = "<description>" + description + "</description><color>" + color.Color.ToString() + "</color><style>" + style + "</style><latitude>" + latitude + "</latitude><longitude>" + longitude + "</longitude>";
            System.Xml.XmlElement element = doc.CreateElement("dataLayer");
            element.InnerXml = xmlString;
            element.AppendChild(dashboardHelper.Serialize(doc));

            System.Xml.XmlAttribute type = doc.CreateAttribute("layerType");
            type.Value = "EpiDashboard.Mapping.PointLayerProperties";
            element.Attributes.Append(type);

            return element;
        }

        public void CreateFromXml(System.Xml.XmlElement element)
        {
            foreach (System.Xml.XmlElement child in element.ChildNodes)
            {
                if (child.Name.Equals("latitude"))
                {
                    cmbLatitude.SelectedItem = child.InnerText;
                }
                if (child.Name.Equals("longitude"))
                {
                    cmbLongitude.SelectedItem = child.InnerText;
                }
                if (child.Name.Equals("color"))
                {
                    rctSelectColor.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(child.InnerText));
                }
                if (child.Name.Equals("description"))
                {
                    txtDescription.Text = child.InnerText;
                }
            }
            RenderMap();
        }

        #endregion

        #region ILayerProperties Members


        public StackPanel LegendStackPanel
        {
            get { return provider.LegendStackPanel; }
        }

        #endregion

        public void MoveUp()
        {
            provider.MoveUp();
        }

        public void MoveDown()
        {
            provider.MoveDown();
        }


        void coord_SelectionChanged(object sender, EventArgs e)
        {
            //RenderMap();
        }

        public DashboardHelper GetDashboardHelper()
        {
            return this.dashboardHelper;
        }

        public void SetDashboardHelper(DashboardHelper dash)
        {
            this.dashboardHelper = dash;
        }

        void provider_RecordSelected(int id)
        {
            mapControl.OnRecordSelected(id);
        }

        void mapControl_MapDataChanged(object sender, EventArgs e)
        {
            provider.Refresh();
        }

        void mapControl_TimeVariableSet(string timeVariable)
        {
            provider.TimeVar = timeVariable;
        }

        void provider_DateRangeDefined(DateTime start, DateTime end, List<KeyValuePair<DateTime, int>> intervalCounts)
        {
            mapControl.OnDateRangeDefined(start, end, intervalCounts);
        }
        public void SetFilter()
        {
            //--for filters
            rowfiltercontrol = new RowFilterControl(dashboardHelper, Dialogs.FilterDialogMode.ConditionalMode, dashboardHelper.DataFilters, true);
            rowfiltercontrol.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            rowfiltercontrol.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            if (HasFilter())
            {
                RemoveFilter();

            }
            panelFilter.Children.Add(rowfiltercontrol);
            tblockAnyFilterGadgetOnly.Visibility = Visibility.Collapsed;
        }
        private void RemoveFilter()
        {
            int ChildIndex = 0;
            foreach (var child in panelFilter.Children)
            {

                if (child.GetType().FullName.Contains("EpiDashboard.RowFilterControl"))
                {

                    panelFilter.Children.RemoveAt(ChildIndex);
                    break;
                }
                ChildIndex++;
            }
        }

        private bool HasFilter()
        {
            bool HasFilter = false;
            foreach (var child in panelFilter.Children)
            {
                if (child.GetType().FullName.Contains("EpiDashboard.RowFilterControl"))
                {

                    HasFilter = true;
                }

            }
            return HasFilter;
        }
        public void FillComboBoxes()
        {
            cmbLatitude.Items.Clear();
            cmbLongitude.Items.Clear();
            ColumnDataType columnDataType = ColumnDataType.Numeric;
            List<string> fields = dashboardHelper.GetFieldsAsList(columnDataType); //dashboardHelper.GetNumericFormFields();
            foreach (string f in fields)
            {
                if (!(f.ToUpperInvariant() == "RECSTATUS" || f.ToUpperInvariant() == "FKEY" || f.ToUpperInvariant() == "GLOBALRECORDID" || f.ToUpperInvariant() == "UNIQUEKEY"))
                {
                    cmbLatitude.Items.Add(f);
                    cmbLongitude.Items.Add(f);
                }
            }
            if (cmbLatitude.Items.Count > 0)
            {
                cmbLatitude.SelectedIndex = -1;
                cmbLongitude.SelectedIndex = -1;
            }

        }
        public void ReFillLongitudeComboBoxes()
        {

            cmbLongitude.Items.Clear();
            ColumnDataType columnDataType = ColumnDataType.Numeric;
            List<string> fields = dashboardHelper.GetFieldsAsList(columnDataType); //dashboardHelper.GetNumericFormFields();
            foreach (string f in fields)
            {
                if (!(f.ToUpperInvariant() == "RECSTATUS" || f.ToUpperInvariant() == "FKEY" || f.ToUpperInvariant() == "GLOBALRECORDID" || f.ToUpperInvariant() == "UNIQUEKEY"))
                {

                    cmbLongitude.Items.Add(f);
                }
            }


        }
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (cmbLongitude.SelectedIndex > -1 && cmbLongitude.SelectedIndex > -1)
            {
                if (txtProjectPath.Text.Length > 0 && provider != null)
                {
                    RenderMap();
                    layerprop.SetValues(txtDescription.Text, cmbLatitude.Text, cmbLongitude.Text, colorselected);
                }

                if (ChangesAccepted != null)
                {
                    ChangesAccepted(this, new EventArgs());
                }
            }
            else
            {
                MessageBoxResult result = System.Windows.MessageBox.Show(DashboardSharedStrings.GADGET_MAP_ADD_VARIABLES, DashboardSharedStrings.ALERT, MessageBoxButton.OK);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (Cancelled != null)
            {
                Cancelled(this, new EventArgs());
            }
        }


        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (Cancelled != null)
            {
                Cancelled(this, new EventArgs());
            }
        }
        private void RenderMap()
        {

            if (cmbLatitude.SelectedIndex != -1 && cmbLongitude.SelectedIndex != -1)
            {
                provider.RenderClusterMap(dashboardHelper, cmbLatitude.SelectedItem.ToString(), cmbLongitude.SelectedItem.ToString(), colorselected, null, txtDescription.Text);

                if (MapGenerated != null)
                {
                    MapGenerated(layerprop, new EventArgs());
                }
            }
        }

        private void rctSelectColor_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctSelectColor.Fill = new SolidColorBrush(Color.FromArgb(240, dialog.Color.R, dialog.Color.G, dialog.Color.B));
                colorselected = new SolidColorBrush(Color.FromArgb(240, dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
        }

        private void rctAddFilter_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SetFilter();
        }

        private void PropertyChanged_EnableDisable()
        {
            if (btnOK == null) return;

            if (!string.IsNullOrEmpty(txtProjectPath.Text))
            {
                if (tbtnVariables.IsEnabled == false)
                {
                    tbtnVariables.IsEnabled = true;
                    tbtnVariables_Checked(this, new RoutedEventArgs());
                }
                if (cmbLatitude.SelectedIndex != -1 && cmbLongitude.SelectedIndex != -1)
                {
                    if (tbtnDisplay.IsEnabled == false)
                    {
                        tbtnDisplay.IsEnabled = true;
                        tbtnDisplay_Checked(this, new RoutedEventArgs());
                    }
                    tbtnFilters.IsEnabled = true;
                    tbtnFilters.Visibility = Visibility.Visible;
                    btnOK.IsEnabled = true;
                    return;
                }
            }
         
        }

        public void cmbLatitude_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbLatitude.SelectedItem != null)
            {
                cmbLongitude.IsEnabled = true;
                PropertyChanged_EnableDisable();
                ReFillLongitudeComboBoxes();
                if (cmbLongitude.Items.Contains(cmbLatitude.SelectedItem))
                {

                    cmbLongitude.Items.Remove(cmbLatitude.SelectedItem);
                    PropertyChanged_EnableDisable();
                }
            }

        }

        public void cmbLongitude_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PropertyChanged_EnableDisable();

        }

    }
}
