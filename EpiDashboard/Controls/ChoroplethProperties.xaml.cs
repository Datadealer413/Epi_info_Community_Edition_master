using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Epi;
using EpiDashboard.Mapping;
using System.Windows.Forms;
using System.Net;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Toolkit;

namespace EpiDashboard.Controls
{
    public class UIControls
    {
        public Rectangle rectangle;
        public System.Windows.Controls.TextBox rampStarts;
        public System.Windows.Controls.TextBlock centerTexts;
        public System.Windows.Controls.TextBox rampEnds;
        public System.Windows.Controls.TextBox legedTexts;
    }

    /// <summary>
    /// Interaction logic for DashboardProperties.xaml
    /// </summary>
    public partial class ChoroplethProperties : System.Windows.Controls.UserControl
    {
        private EpiDashboard.Mapping.StandaloneMapControl _mapControl;
        private ESRI.ArcGIS.Client.Map _myMap;
        private DashboardHelper _dashboardHelper;
        private int _currentStratCount;
        private SolidColorBrush _currentColor_rampMissing;
        private SolidColorBrush _currentColor_rampStart;
        private SolidColorBrush _currentColor_rampEnd;
        private bool _initialRampCalc;

        ChoroplethLayerProvider _layerProvider;
        public ChoroplethLayerProvider LayerProvider
        {
            get { return _layerProvider; }
            set { _layerProvider = value; }
        }

        public ChoroplethKmlLayerProvider choroplethKmlLayerProvider;
        public ChoroplethServerLayerProvider choroplethServerLayerProvider;
        public ChoroplethShapeLayerProvider choroplethShapeLayerProvider;

        public ChoroplethShapeLayerProperties choroplethShapeLayerProperties;
        public ChoroplethServerLayerProperties choroplethServerLayerProperties;
        public ChoroplethKmlLayerProperties choroplethKmlLayerProperties;

        public RowFilterControl rowFilterControl { get; set; }
        public DataFilters datafilters { get; set; }

        private System.Xml.XmlElement currentElement;
        private string shapeFilePath;

        public ListLegendTextDictionary ListLegendText = new ListLegendTextDictionary();

        public IDictionary<string, object> shapeAttributes;
        private Dictionary<int, object> ClassAttribList = new Dictionary<int, object>();
        public Envelope mapOriginalExtent;
        public List<string> layerAddednew = new List<string>();
        public string ShapeKey { get; set; }
    
        string _shapeKey;
        string _dataKey;
        string _value;

        private struct classAttributes
        {
            public Brush rctColor;
            public string rampStart;
            public string rampEnd;
            public string legendText;
        }

        public ChoroplethProperties(EpiDashboard.Mapping.StandaloneMapControl mapControl, ESRI.ArcGIS.Client.Map myMap)
        {
            InitializeComponent();
            _mapControl = mapControl;
            _myMap = myMap;

            if(mapControl != null)
            { 
                mapControl.SizeChanged += mapControl_SizeChanged;
            }

            Epi.ApplicationIdentity appId = new Epi.ApplicationIdentity(typeof(Configuration).Assembly);
            tblockCurrentEpiVersion.Text = "Epi Info " + appId.Version;

            if (int.TryParse(cmbClasses.Text, out _currentStratCount) == false)
            {
                _currentStratCount = 4;
            }

            _currentColor_rampMissing = (SolidColorBrush)rctMissingColor.Fill;
            _currentColor_rampStart = (SolidColorBrush)rctLowColor.Fill;
            _currentColor_rampEnd = (SolidColorBrush)rctHighColor.Fill;
            _initialRampCalc = true;
            //mapOriginalExtent = myMap.Extent;

            #region Translation

            //Point of Interest Left Panel
            lblConfigExpandedTitle.Content = DashboardSharedStrings.GADGET_CONFIG_TITLE_CHOROPLETH;
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
            lblBoundaries.Content = DashboardSharedStrings.GADGET_BOUNDARIES;
            radShapeFile.Content = DashboardSharedStrings.GADGET_SHAPEFILE;
            btnBrowseShape.Content = DashboardSharedStrings.BUTTON_BROWSE;
            radMapServer.Content = DashboardSharedStrings.GADGET_MAPSERVER;
            lblURL.Content = DashboardSharedStrings.GADGET_URL;
            btnMapserverlocate.Content = DashboardSharedStrings.BUTTON_CONNECT;
            lblExampleMapServerURL.Text = DashboardSharedStrings.GADGET_EXAMPLE_MAPSERVER;
            lblSelectFeature.Content = DashboardSharedStrings.GADGET_SELECT_FEATURE;
            radKML.Content = DashboardSharedStrings.GADGET_KMLFILE;
            lblURLOfKMLFile.Content = DashboardSharedStrings.GADGET_KMLFILE_LOCATION;
            btnKMLFile.Content = DashboardSharedStrings.BUTTON_BROWSE;
            lblExampleKMLFile.Text = DashboardSharedStrings.GADGET_EXAMPLE_KMLFILE;

            tblockConnectionString.Content = DashboardSharedStrings.GADGET_CONNECTION_STRING;
            tblockSQLQuery.Content = DashboardSharedStrings.GADGET_SQL_QUERY;
            tblockLoadingData.Text = DashboardSharedStrings.GADGET_LOADING_DATA;

            //Variables Panel
            tblockPanelVariables.Content = DashboardSharedStrings.GADGET_TABBUTTON_VARIABLES;
            //tblockSelectVarData.Content = DashboardSharedStrings.GADGET_VARIABLES_CHORPLTH;
            lblFeature.Content = DashboardSharedStrings.GADGET_MAP_FEATURE;
            lblData.Content = DashboardSharedStrings.GADGET_MAP_DATA;
            lblValue.Content = DashboardSharedStrings.GADGET_MAP_VALUE;

            //Colors & Ranges Panel
            lblPanelHdrColorsAndStyles.Content = DashboardSharedStrings.GADGET_MAP_TABBUTTON_DISPLAY;
            tblockColorsSubheader.Content = DashboardSharedStrings.GADGET_PANELSUBHEADER_COLORS;
            //lblMissingExcluded.Content = DashboardSharedStrings.GADGET_MAP_COLOR_MISSING;
            //lblColorRamp.Content = DashboardSharedStrings.GADGET_PANELSUBHEADER_COLORS;
            lblColorStart.Content = DashboardSharedStrings.GADGET_MAP_START;
            lblColorEnd.Content = DashboardSharedStrings.GADGET_MAP_END;
            tblockOpacity.Text = DashboardSharedStrings.GADGET_MAP_OPACITY;
            tblockRangesSubheader.Content = DashboardSharedStrings.GADGET_MAP_RANGES;
            lblClassBreaks.Content = DashboardSharedStrings.GADGET_MAP_CLASSES;
            quintilesOption.Content = DashboardSharedStrings.GADGET_MAP_QUANTILES;
            tblockLegendTitleSubheader.Content = DashboardSharedStrings.GADGET_MAP_LEGEND_TITLE;
            tblockColorRamp.Text = DashboardSharedStrings.GADGET_PANELSUBHEADER_COLOR;
            tblockRange.Text = DashboardSharedStrings.GADGET_MAP_RANGE;
            tblockLegText.Text = DashboardSharedStrings.GADGET_MAP_LEGEND_TEXT;
            legendText0.Text = DashboardSharedStrings.GADGET_MAP_CHORPLTH_MISSEXCLUDED;

            //Filters Panel
            tblockPanelDataFilter.Content = DashboardSharedStrings.GADGET_PANELHEADER_DATA_FILTER;
            tblockSetDataFilter.Content = DashboardSharedStrings.GADGET_TABDESC_FILTERS_MAPS;

            //Info Panel
            //lblCanvasInfo.Content = DashboardSharedStrings.GADGET_CANVAS_INFO;
            //tblockRows.Text = dashboardHelper.DataSet.Tables[0].Rows.Count.ToString() + DashboardSharedStrings.GADGET_INFO_UNFILTERED_ROWS;


            btnOK.Content = DashboardSharedStrings.BUTTON_OK;
            btnCancel.Content = DashboardSharedStrings.BUTTON_CANCEL;

            tblockColorsSubheader.Content = DashboardSharedStrings.GADGET_PANELSUBHEADER_COLORS;

            #endregion // Translation
        }

        public void mapControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _mapControl.ResizedWidth = e.NewSize.Width;
            _mapControl.ResizedHeight = e.NewSize.Height;

            if (_mapControl.ResizedWidth != 0 & _mapControl.ResizedHeight != 0)
            {
                double i_StandardHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
                double i_StandardWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
                float f_HeightRatio = new float();
                float f_WidthRatio = new float();
                f_HeightRatio = (float)((float)_mapControl.ResizedHeight / (float)i_StandardHeight);
                f_WidthRatio = (float)((float)_mapControl.ResizedWidth / (float)i_StandardWidth);

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

        public DashboardHelper DashboardHelper { 
            get{ return _dashboardHelper; } 
            private set{ _dashboardHelper = value; }
        }

        public string DataSource { get; set; }

        private byte _opacity = 240;

        public byte Opacity
        {
            get { return _opacity; }
            set { _opacity = value; }
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

        public string MapServerName { get; set; }
        public int MapVisibleLayer { get; set; }
        public string KMLMapServerName { get; set; }
        public int KMLMapVisibleLayer { get; set; }

        private void tbtnInfo_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as ToggleButton);
            panelDataSource.Visibility = System.Windows.Visibility.Collapsed;
            panelVariables.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplay.Visibility = System.Windows.Visibility.Collapsed;
            panelInfo.Visibility = System.Windows.Visibility.Visible;
            panelFilters.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tbtnDisplay_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as ToggleButton);
            panelDataSource.Visibility = System.Windows.Visibility.Collapsed;
            panelVariables.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplay.Visibility = System.Windows.Visibility.Visible;
            panelInfo.Visibility = System.Windows.Visibility.Collapsed;
            panelFilters.Visibility = System.Windows.Visibility.Collapsed;
            SetProperties();
            PropertyChanged_EnableDisable();
        }

        public void SetProperties()
        {
            if (cmbShapeKey.SelectedItem != null && cmbDataKey.SelectedItem != null && cmbValue.SelectedItem != null)
            {
                if (LayerProvider != null && LayerProvider.Opacity > 0)
                {
                    Opacity = LayerProvider.Opacity;
                    sliderOpacity.Value = Opacity;
                }
                else if (choroplethKmlLayerProvider != null && choroplethKmlLayerProvider.Opacity > 0)
                {
                    Opacity = choroplethKmlLayerProvider.Opacity;
                    sliderOpacity.Value = Opacity;
                }
                else if (choroplethServerLayerProperties != null && choroplethServerLayerProperties.Opacity > 0)
                {
                    Opacity = choroplethServerLayerProperties.Opacity;
                    sliderOpacity.Value = Opacity;
                }
                else
                {
                    Opacity = Convert.ToByte(sliderOpacity.Value);
                }

                List<SolidColorBrush> brushList = new List<SolidColorBrush>()
                {
                    (SolidColorBrush) rctColor1.Fill,
                    (SolidColorBrush) rctColor2.Fill,
                    (SolidColorBrush) rctColor3.Fill,
                    (SolidColorBrush) rctColor4.Fill,
                    (SolidColorBrush) rctColor5.Fill,
                    (SolidColorBrush) rctColor6.Fill,
                    (SolidColorBrush) rctColor7.Fill,
                    (SolidColorBrush) rctColor8.Fill,
                    (SolidColorBrush) rctColor9.Fill,
                    (SolidColorBrush) rctColor10.Fill,
                    (SolidColorBrush) rctMissingColor.Fill
                };

                int classCount = 0;
                if (cmbClasses.SelectedItem != null)
                {
                    if (!int.TryParse(((ComboBoxItem)cmbClasses.SelectedItem).Content.ToString(), out classCount))
                    {
                        classCount = 4;
                    }

                    string validationMessage = string.Empty;

                    if (LayerProvider != null)
                    {
                        validationMessage = LayerProvider.PopulateRangeValues(_dashboardHelper,
                            cmbShapeKey.SelectedItem.ToString(),
                            cmbDataKey.SelectedItem.ToString(),
                            cmbValue.SelectedItem.ToString(),
                            brushList,
                            classCount,
                            legTitle.Text,
                            showPolyLabels.IsChecked);

                        if (LayerProvider.RangeCount != classCount)
                        {
                            classCount = LayerProvider.RangeCount;
                            LayerProvider._classCount = LayerProvider.RangeCount;
                            _currentStratCount = LayerProvider.RangeCount;
                            cmbClasses.Text = classCount.ToString();
                        }
                    }

                    SetRangeUISection();
                }

                UpdateClassRangesDictionary_FromControls();
            }
        }

        private void UpdateColorsCollection()
        {
            foreach (UIElement element in stratGrid.Children)
            {
                if (element is Rectangle)
                {
                    SolidColorBrush scBrush = ((Rectangle)element).Fill as SolidColorBrush;
                    Color color = scBrush.Color;
                    LayerProvider.CustomColorsDictionary.Add(((Rectangle)element).Name, color);
                }
            }
        }

        private void UpdateClassRangesDictionary_FromControls()
        {
            if (LayerProvider == null) return;

            foreach (UIElement element in stratGrid.Children)
            {
                if (element is System.Windows.Controls.TextBox)
                {
                    string elementName = ((System.Windows.Controls.TextBox)element).Name;

                    if (elementName.StartsWith("ramp"))
                    {
                        LayerProvider.ClassRangesDictionary.Add(((System.Windows.Controls.TextBox)element).Name, ((System.Windows.Controls.TextBox)element).Text);
                    }
                }
            }
        }


        private void tbtnVariables_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as ToggleButton);
            panelDataSource.Visibility = System.Windows.Visibility.Collapsed;
            panelVariables.Visibility = System.Windows.Visibility.Visible;
            panelDisplay.Visibility = System.Windows.Visibility.Collapsed;
            panelInfo.Visibility = System.Windows.Visibility.Collapsed;
            panelFilters.Visibility = System.Windows.Visibility.Collapsed;

            if (choroplethServerLayerProperties != null)
            {
                if (choroplethServerLayerProperties.Provider.FlagUpdateToGLFailed) 
                { 
                    ResetShapeCombo(); 
                }
            }
        }

        private void tbtnFilters_Checked(object sender, RoutedEventArgs e)
        {

            if (panelDataSource == null) return;
            CheckButtonStates(sender as ToggleButton);
            panelDataSource.Visibility = System.Windows.Visibility.Collapsed;
            panelVariables.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplay.Visibility = System.Windows.Visibility.Collapsed;
            panelInfo.Visibility = System.Windows.Visibility.Collapsed;
            panelFilters.Visibility = System.Windows.Visibility.Visible;
        }

        private void tbtnDataSource_Checked(object sender, RoutedEventArgs e)
        {
            if (panelDataSource == null) return;
            CheckButtonStates(sender as ToggleButton);
            panelDataSource.Visibility = System.Windows.Visibility.Visible;
            panelVariables.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplay.Visibility = System.Windows.Visibility.Collapsed;
            panelInfo.Visibility = System.Windows.Visibility.Collapsed;
            panelFilters.Visibility = System.Windows.Visibility.Collapsed;
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
            PropertyChanged_EnableDisable();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (tbtnDisplay.IsChecked == false)
                {
                    PropertyChanged_EnableDisable();
                    return;
                }
                
                if (cmbDataKey.SelectedIndex > -1 && cmbShapeKey.SelectedIndex > -1 && cmbValue.SelectedIndex > -1)
                {

                    RenderMap();
                    AddClassAttributes();

                    int numclasses = Convert.ToInt32(cmbClasses.Text);
                    bool flagquintiles = (bool)quintilesOption.IsChecked;

                    if (radShapeFile.IsChecked == true && LayerProvider != null)
                    {
                        choroplethShapeLayerProperties.SetdashboardHelper(this.DashboardHelper);
                        choroplethShapeLayerProperties.SetValues(
                            txtShapePath.Text,
                            cmbShapeKey.Text,
                            cmbDataKey.Text,
                            cmbValue.Text,
                            cmbClasses.Text,
                            rctHighColor.Fill,
                            rctLowColor.Fill,
                            rctMissingColor.Fill,
                            shapeAttributes,
                            ClassAttribList,
                            flagquintiles,
                            numclasses,
                            legTitle.Text,
                            showPolyLabels.IsChecked,
                            Opacity);
                    }
                    else if ((radMapServer.IsChecked == true && LayerProvider != null) || (LayerProvider is ChoroplethServerLayerProvider))
                    {
                        choroplethServerLayerProperties.SetdashboardHelper(this.DashboardHelper);
                        choroplethServerLayerProperties.SetValues(
                            cmbShapeKey.Text,
                            cmbDataKey.Text,
                            cmbValue.Text,
                            cmbClasses.Text,
                            rctHighColor.Fill,
                            rctLowColor.Fill,
                            rctMissingColor.Fill,
                            shapeAttributes,
                            ClassAttribList,
                            flagquintiles,
                            numclasses,
                            Opacity);
                        
                        choroplethServerLayerProperties.txtMapserverText = txtMapSeverpath.Text;
                        choroplethServerLayerProperties.cbxMapFeatureText = cbxmapfeature.Text;
                    }

                    else if (radKML.IsChecked == true && LayerProvider != null)
                    {
                        choroplethKmlLayerProperties.SetdashboardHelper(this.DashboardHelper);
                        choroplethKmlLayerProperties.SetValues(
                            cmbShapeKey.Text, 
                            cmbDataKey.Text, 
                            cmbValue.Text,
                            cmbClasses.Text, 
                            rctHighColor.Fill, 
                            rctLowColor.Fill, 
                            rctMissingColor.Fill, 
                            shapeAttributes,
                            ClassAttribList, 
                            flagquintiles,
                            numclasses,
                            Opacity);
                    }

                    if (ChangesAccepted != null)
                    {
                        ChangesAccepted(this, new EventArgs());
                    }

                    btnOK.IsEnabled = true;
                }
                else
                {
                    MessageBoxResult result = System.Windows.MessageBox.Show(DashboardSharedStrings.GADGET_MAP_ADD_VARIABLES, DashboardSharedStrings.ALERT, MessageBoxButton.OK);
                    btnOK.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (Cancelled != null)
            {
                Cancelled(this, new EventArgs());
            }

            foreach (string id in layerAddednew)
            {
                if (choroplethKmlLayerProperties != null)
                {
                    ESRI.ArcGIS.Client.Toolkit.DataSources.KmlLayer shapeLayer = _myMap.Layers[id] as ESRI.ArcGIS.Client.Toolkit.DataSources.KmlLayer;
                    if (shapeLayer != null)
                        _myMap.Layers.Remove(shapeLayer);
                }
                else
                {
                    ESRI.ArcGIS.Client.GraphicsLayer graphicsLayer = _myMap.Layers[id] as ESRI.ArcGIS.Client.GraphicsLayer;
                    if (graphicsLayer != null)
                       _myMap.Layers.Remove(graphicsLayer);
                }
            }
            if (choroplethServerLayerProperties != null && shapeAttributes != null & !string.IsNullOrEmpty(ShapeKey))
            {
                choroplethServerLayerProperties.curfeatureAttributes = shapeAttributes;
                    cmbShapeKey.Items.Clear();
                    choroplethServerLayerProperties.cbxShapeKey.Items.Clear();
                    foreach (string key in shapeAttributes.Keys)
                    {
                        cmbShapeKey.Items.Add(key);
                        choroplethServerLayerProperties.cbxShapeKey.Items.Add(key);                       
                    }
                    choroplethServerLayerProperties.cbxShapeKey.Text = ShapeKey;                                
            }
            
           _myMap.Extent = mapOriginalExtent;
           
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            waitCursor.Visibility = Visibility.Visible;
            tblockLoadingData.Visibility = Visibility.Visible;

            _dashboardHelper = _mapControl.GetNewDashboardHelper();
            waitCursor.Visibility = Visibility.Collapsed;
            tblockLoadingData.Visibility = Visibility.Collapsed;

            if (_dashboardHelper != null)
            {
                this.DashboardHelper = _dashboardHelper;
                txtProjectPath.Text = _mapControl.ProjectFilepath;
                FillComboBoxes();
                validationText.Text = string.Empty;
                panelBoundaries.IsEnabled = true;
                radShapeFile.IsChecked = true;
                this.datafilters = new DataFilters(_dashboardHelper);
                rowFilterControl = new RowFilterControl(_dashboardHelper, Dialogs.FilterDialogMode.ConditionalMode, datafilters, true);
                rowFilterControl.HorizontalAlignment = System.Windows.HorizontalAlignment.Left; rowFilterControl.FillSelectionComboboxes();

                if (HasFilter())
                {
                    RemoveFilter();
                }

                panelFilters.Children.Add(rowFilterControl);
            }
        }

        private void RemoveFilter()
        {
            int ChildIndex = 0;

            foreach (var child in panelFilters.Children)
            {
                if (child.GetType().FullName.Contains("EpiDashboard.RowFilterControl"))
                {
                    panelFilters.Children.RemoveAt(ChildIndex);
                    break;
                }

                ChildIndex++;
            }
        }

        private bool HasFilter()
        {
            bool HasFilter = false;
            foreach (var child in panelFilters.Children)
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
            cmbDataKey.Items.Clear();
            cmbValue.Items.Clear();
            List<string> fields = _dashboardHelper.GetFieldsAsList(); // dashboardHelper.GetFormFields();
            ColumnDataType columnDataType = ColumnDataType.Numeric;
            List<string> numericFields = _dashboardHelper.GetFieldsAsList(columnDataType); //dashboardHelper.GetNumericFormFields();
            foreach (string field in fields)
            {
                if (!(field.ToUpperInvariant() == "RECSTATUS" || field.ToUpperInvariant() == "FKEY" || field.ToUpperInvariant() == "GLOBALRECORDID" || field.ToUpperInvariant() == "UNIQUEKEY" || field.ToUpperInvariant() == "FIRSTSAVETIME" || field.ToUpperInvariant() == "LASTSAVETIME" || field.ToUpperInvariant() == "SYSTEMDATE"))
                { cmbDataKey.Items.Add(field); }
            }
            foreach (string field in numericFields)
            {
                if (!(field.ToUpperInvariant() == "RECSTATUS" || field.ToUpperInvariant() == "UNIQUEKEY"))
                { cmbValue.Items.Add(field); }
            }
            cmbValue.Items.Insert(0, "{Record Count}");
        }
        
        public void ReFillValueComboBoxes()
        {
            cmbValue.Items.Clear();
            ColumnDataType columnDataType = ColumnDataType.Numeric;
            List<string> numericFields = _dashboardHelper.GetFieldsAsList(columnDataType); //dashboardHelper.GetNumericFormFields();

            foreach (string field in numericFields)
            {
                if (!(field.ToUpperInvariant() == "RECSTATUS" || field.ToUpperInvariant() == "UNIQUEKEY"))
                { 
                    cmbValue.Items.Add(field); 
                }
            }

            cmbValue.Items.Insert(0, "{Record Count}");
        }
        
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (Cancelled != null)
            {
                Cancelled(this, new EventArgs());
            }
        }

        private void AddClassAttributes()
        {
            ClassAttribList.Clear();

            classAttributes ca1 = new classAttributes();
            ca1.rctColor = rctColor1.Fill;
            ca1.rampStart = rampStart01.Text;
            ca1.rampEnd = rampEnd01.Text;
            ca1.legendText = legendText1.Text;
            ClassAttribList.Add(1, ca1);

            classAttributes ca2 = new classAttributes();
            ca2.rctColor = rctColor2.Fill;
            ca2.rampStart = rampStart02.Text;
            ca2.rampEnd = rampEnd02.Text;
            ca2.legendText = legendText2.Text;
            ClassAttribList.Add(2, ca2);

            classAttributes ca3 = new classAttributes();
            ca3.rctColor = rctColor3.Fill;
            ca3.rampStart = rampStart03.Text;
            ca3.rampEnd = rampEnd03.Text;
            ca3.legendText = legendText3.Text;
            ClassAttribList.Add(3, ca3);

            classAttributes ca4 = new classAttributes();
            ca4.rctColor = rctColor4.Fill;
            ca4.rampStart = rampStart04.Text;
            ca4.rampEnd = rampEnd04.Text;
            ca4.legendText = legendText4.Text;
            ClassAttribList.Add(4, ca4);

            classAttributes ca5 = new classAttributes();
            ca5.rctColor = rctColor5.Fill;
            ca5.rampStart = rampStart05.Text;
            ca5.rampEnd = rampEnd05.Text;
            ca5.legendText = legendText5.Text;
            ClassAttribList.Add(5, ca5);

            classAttributes ca6 = new classAttributes();
            ca6.rctColor = rctColor6.Fill;
            ca6.rampStart = rampStart06.Text;
            ca6.rampEnd = rampEnd06.Text;
            ca6.legendText = legendText6.Text;
            ClassAttribList.Add(6, ca6);

            classAttributes ca7 = new classAttributes();
            ca7.rctColor = rctColor7.Fill;
            ca7.rampStart = rampStart07.Text;
            ca7.rampEnd = rampEnd07.Text;
            ca7.legendText = legendText7.Text;
            ClassAttribList.Add(7, ca7);

            classAttributes ca8 = new classAttributes();
            ca8.rctColor = rctColor8.Fill;
            ca8.rampStart = rampStart08.Text;
            ca8.rampEnd = rampEnd08.Text;
            ca8.legendText = legendText8.Text;
            ClassAttribList.Add(8, ca8);

            classAttributes ca9 = new classAttributes();
            ca9.rctColor = rctColor9.Fill;
            ca9.rampStart = rampStart09.Text;
            ca9.rampEnd = rampEnd09.Text;
            ca9.legendText = legendText9.Text;
            ClassAttribList.Add(9, ca9);

            classAttributes ca10 = new classAttributes();
            ca10.rctColor = rctColor10.Fill;
            ca10.rampStart = rampStart10.Text;
            ca10.rampEnd = rampEnd10.Text;
            ca10.legendText = legendText10.Text;
            ClassAttribList.Add(10, ca10);

            UpdateClassRangesDictionary_FromControls();
        }

        private bool ValidateInputValue(System.Windows.Controls.TextBox textBox)
        {
            double textboxValue, rangeStartValue, rangeEndValue;

            if (double.TryParse(textBox.Text, out textboxValue))
            {
                rangeStartValue = double.Parse(LayerProvider.RangeValues[0, 0].ToString());
                rangeEndValue = double.Parse(LayerProvider.RangeValues[LayerProvider.RangeCount - 1, 1].ToString());
                
                if (textboxValue >= rangeStartValue && textboxValue <= rangeEndValue)
                {
                    return true;
                }
            }
            return false;
        }

        private bool ValidateRangeInput()
        {
            int _rangeCount = LayerProvider.RangeCount;

            if (_rangeCount >= 2)
            {
                if (!(ValidateInputValue(rampStart01) && ValidateInputValue(rampEnd01)
                    && ValidateInputValue(rampStart02) && ValidateInputValue(rampEnd02)))
                {
                    System.Windows.MessageBox.Show(DashboardSharedStrings.DASHBOARD_MAP_INCORRECT_INPUT_VALUE);
                    return false;
                }
            }

            if (_rangeCount >= 3)
            {
                if (!(ValidateInputValue(rampStart03) && ValidateInputValue(rampEnd03)))
                {
                    System.Windows.MessageBox.Show(DashboardSharedStrings.DASHBOARD_MAP_INCORRECT_INPUT_VALUE);
                    return false;
                }
            }

            if (_rangeCount >= 4)
            {
                if (!(ValidateInputValue(rampStart04) && ValidateInputValue(rampEnd04)))
                {
                    System.Windows.MessageBox.Show(DashboardSharedStrings.DASHBOARD_MAP_INCORRECT_INPUT_VALUE);
                    return false;
                }
            }
            if (_rangeCount >= 5)
            {
                if (!(ValidateInputValue(rampStart05) && ValidateInputValue(rampEnd05)))
                {
                    System.Windows.MessageBox.Show(DashboardSharedStrings.DASHBOARD_MAP_INCORRECT_INPUT_VALUE);
                    return false;
                }
            }
            if (_rangeCount >= 6)
            {
                if (!(ValidateInputValue(rampStart06) && ValidateInputValue(rampEnd06)))
                {
                    System.Windows.MessageBox.Show(DashboardSharedStrings.DASHBOARD_MAP_INCORRECT_INPUT_VALUE);
                    return false;
                }
            }
            if (_rangeCount >= 7)
            {
                if (!(ValidateInputValue(rampStart07) && ValidateInputValue(rampEnd07)))
                {
                    System.Windows.MessageBox.Show(DashboardSharedStrings.DASHBOARD_MAP_INCORRECT_INPUT_VALUE);
                    return false;
                }
            }
            if (_rangeCount >= 8)
            {
                if (!(ValidateInputValue(rampStart08) && ValidateInputValue(rampEnd08)))
                {
                    System.Windows.MessageBox.Show(DashboardSharedStrings.DASHBOARD_MAP_INCORRECT_INPUT_VALUE);
                    return false;
                }
            }
            if (_rangeCount >= 9)
            {
                if (!(ValidateInputValue(rampStart09) && ValidateInputValue(rampEnd09)))
                {
                    System.Windows.MessageBox.Show(DashboardSharedStrings.DASHBOARD_MAP_INCORRECT_INPUT_VALUE);
                    return false;
                }
            }
            if (_rangeCount >= 10)
            {
                if (!(ValidateInputValue(rampStart10) && ValidateInputValue(rampEnd10)))
                {
                    System.Windows.MessageBox.Show(DashboardSharedStrings.DASHBOARD_MAP_INCORRECT_INPUT_VALUE);
                    return false;
                }
            }
            return true;
        }

        public void SetClassAttributes(Dictionary<int, object> classAttrib)
        {
            foreach (int key in classAttrib.Keys)
            {
                var item = classAttrib.ElementAt(key - 1);
                classAttributes itemvalue = (classAttributes)item.Value;

                if (key == 1)
                {
                    rctColor1.Fill = (Brush)itemvalue.rctColor;
                    rampStart01.Text = itemvalue.rampStart;
                    rampEnd01.Text = itemvalue.rampEnd;
                    legendText1.Text = itemvalue.legendText;
                }
                else if (key == 2)
                {
                    rctColor2.Fill = (Brush)itemvalue.rctColor;
                    rampStart02.Text = itemvalue.rampStart;
                    rampEnd02.Text = itemvalue.rampEnd;
                    legendText2.Text = itemvalue.legendText;
                }
                else if (key == 3)
                {
                    rctColor3.Fill = (Brush)itemvalue.rctColor;
                    rampStart03.Text = itemvalue.rampStart;
                    rampEnd03.Text = itemvalue.rampEnd;
                    legendText3.Text = itemvalue.legendText;
                }
                else if (key == 4)
                {
                    rctColor4.Fill = (Brush)itemvalue.rctColor;
                    rampStart04.Text = itemvalue.rampStart;
                    rampEnd04.Text = itemvalue.rampEnd;
                    legendText4.Text = itemvalue.legendText;
                }
                else if (key == 5)
                {
                    rctColor5.Fill = (Brush)itemvalue.rctColor;
                    rampStart05.Text = itemvalue.rampStart;
                    rampEnd05.Text = itemvalue.rampEnd;
                    legendText5.Text = itemvalue.legendText;

                }
                else if (key == 6)
                {
                    rctColor6.Fill = (Brush)itemvalue.rctColor;
                    rampStart06.Text = itemvalue.rampStart;
                    rampEnd06.Text = itemvalue.rampEnd;
                    legendText6.Text = itemvalue.legendText;

                }
                else if (key == 7)
                {
                    rctColor7.Fill = (Brush)itemvalue.rctColor;
                    rampStart07.Text = itemvalue.rampStart;
                    rampEnd07.Text = itemvalue.rampEnd;
                    legendText7.Text = itemvalue.legendText;
                }
                else if (key == 8)
                {
                    rctColor8.Fill = (Brush)itemvalue.rctColor;
                    rampStart08.Text = itemvalue.rampStart;
                    rampEnd08.Text = itemvalue.rampEnd;
                    legendText8.Text = itemvalue.legendText;

                }
                else if (key == 9)
                {
                    rctColor9.Fill = (Brush)itemvalue.rctColor;
                    rampStart09.Text = itemvalue.rampStart;
                    rampEnd09.Text = itemvalue.rampEnd;
                    legendText9.Text = itemvalue.legendText;

                }
                else if (key == 10)
                {
                    rctColor10.Fill = (Brush)itemvalue.rctColor;
                    rampStart10.Text = itemvalue.rampStart;
                    rampEnd10.Text = itemvalue.rampEnd;
                    legendText10.Text = itemvalue.legendText;
                }
            }
        }

        public void SetDashboardHelper(DashboardHelper dash)
        {
            _dashboardHelper = dash;
        }

        private HttpWebRequest CreateWebRequest(string endPoint)
        {
            var request = (HttpWebRequest)WebRequest.Create(endPoint);
            request.Method = "GET";
            request.ContentLength = 0;
            request.ContentType = "text/xml";
            return request;
        }

        public string GetMessage(string endPoint)
        {
            HttpWebRequest request = CreateWebRequest(endPoint);
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                var responseValue = string.Empty;
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return null;
                }
                using (var responseStream = response.GetResponseStream())
                {
                    using (var reader = new StreamReader(responseStream))
                    {
                        responseValue = reader.ReadToEnd();
                    }
                }
                return responseValue;
            }
        }

        public void RenderMap()
        {
            if (cmbDataKey.SelectedIndex != -1 && cmbShapeKey.SelectedIndex != -1 && cmbValue.SelectedIndex != -1)
            {
                string shapeKey = cmbShapeKey.SelectedItem.ToString();
                string dataKey = cmbDataKey.SelectedItem.ToString();
                string value = cmbValue.SelectedItem.ToString();
                string missingText = legendText0.Text.ToString();
                Opacity = Convert.ToByte(sliderOpacity.Value);

                List<SolidColorBrush> brushList = new List<SolidColorBrush>() { 
                    (SolidColorBrush)rctColor1.Fill, 
                    (SolidColorBrush)rctColor2.Fill, 
                    (SolidColorBrush)rctColor3.Fill, 
                    (SolidColorBrush)rctColor4.Fill, 
                    (SolidColorBrush)rctColor5.Fill, 
                    (SolidColorBrush)rctColor6.Fill, 
                    (SolidColorBrush)rctColor7.Fill, 
                    (SolidColorBrush)rctColor8.Fill, 
                    (SolidColorBrush)rctColor9.Fill, 
                    (SolidColorBrush)rctColor10.Fill,  
                    (SolidColorBrush)rctMissingColor.Fill};

                int classCount;

                if (!int.TryParse(cmbClasses.Text, out classCount))
                {
                    classCount = 4;
                }

                if (LayerProvider != null)
                {
                    LayerProvider.RangeStarts_FromControls = GetRangeValues_FromControls(classCount);
                    LayerProvider.ListLegendText = ListLegendText;
                    LayerProvider.Opacity = this.Opacity;

                    LayerProvider.SetShapeRangeValues(
                        _dashboardHelper,
                        cmbShapeKey.SelectedItem.ToString(),
                        cmbDataKey.SelectedItem.ToString(),
                        cmbValue.SelectedItem.ToString(),
                        brushList,
                        classCount,
                        missingText,
                        legTitle.Text,
                        showPolyLabels.IsChecked);
                }

            }
        }

        public List<double> GetRangeValues_FromControls(int RangeCount)
        {
            List<double> Range = new List<double>();

            ListLegendText.Add(legendText1.Name, legendText1.Text);
            ListLegendText.Add(legendText2.Name, legendText2.Text);
            ListLegendText.Add(legendText3.Name, legendText3.Text);
            ListLegendText.Add(legendText4.Name, legendText4.Text);
            ListLegendText.Add(legendText5.Name, legendText5.Text);
            ListLegendText.Add(legendText6.Name, legendText6.Text);
            ListLegendText.Add(legendText7.Name, legendText7.Text);
            ListLegendText.Add(legendText8.Name, legendText8.Text);
            ListLegendText.Add(legendText9.Name, legendText9.Text);
            ListLegendText.Add(legendText10.Name, legendText10.Text);

            double value = 0.0;

            if (RangeCount >= 2)
            {
                value = 0.0;
                double.TryParse(rampStart01.Text, out value);
                Range.Add(value);


                value = 0.0;
                double.TryParse(rampStart02.Text, out value);
                Range.Add(value);
            }

            if (RangeCount >= 3)
            {
                value = 0.0;
                double.TryParse(rampStart03.Text, out value);
                Range.Add(value);
            }

            if (RangeCount >= 4)
            {
                value = 0.0;
                double.TryParse(rampStart04.Text, out value);
                Range.Add(value);
            }
            
            if (RangeCount >= 5)
            {
                value = 0.0;
                double.TryParse(rampStart05.Text, out value);
                Range.Add(value);
            }
            
            if (RangeCount >= 6)
            {
                value = 0.0;
                double.TryParse(rampStart06.Text, out value);
                Range.Add(value);
            }

            if (RangeCount >= 7)
            {
                value = 0.0;
                double.TryParse(rampStart07.Text, out value);
                Range.Add(value);
            }
            
            if (RangeCount >= 8)
            {
                value = 0.0;
                double.TryParse(rampStart08.Text, out value);
                Range.Add(value);
            }
            
            if (RangeCount >= 9)
            {
                value = 0.0;
                double.TryParse(rampStart09.Text, out value);
                Range.Add(value);
            }
            
            if (RangeCount >= 10)
            {
                value = 0.0;
                double.TryParse(rampStart10.Text, out value);
                Range.Add(value);
            }

            return Range;
        }

        public StackPanel LegendStackPanel
        {
            get
            {
                return LayerProvider.LegendStackPanel;
            }
        }

        private void rctColor_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            System.Windows.Media.Color currentColor = ((SolidColorBrush)((System.Windows.Shapes.Rectangle)sender).Fill).Color;
            System.Drawing.Color initColor = System.Drawing.Color.FromArgb(255, currentColor.R, currentColor.G, currentColor.B);

            List<Int32> baseColors = new List<Int32>() 
            {
                -32640,
                -128,
                -8323200,
                -16711808,
                -8323073,
                -16744193,
                -32576,
                -32513,
            
                -65536,
                -256,
                -8323328,
                -16711872,
                -16711681,
                -16744256,
                -8355648,
                -65281,
                        
                -8372160,
                -32704,
                -16711936,
                -16744320,
                -16760704,
                -8355585,
                -8388544,
                -65408,
                        
                -8388608,
                -32768,
                -16744448,
                -16744384,
                -16776961,
                -16777056,
                -8388480,
                -8388353,
                        
                -12582912,
                -32768,
                -16744448,
                -16744384,
                -16776961,
                -16777056,
                -8388480,
                -8388353,
                        
                -12582912,
                -8372224,
                -16760832,
                -16760768,
                -16777088,
                -16777152,
                -12582848,
                -12582784,
                                    
                -16777216,
                -8355840,
                -8355776,
                -8355712,
                -12550016,
                -4144960,
                -12582848,
                -1
            };

            Int32 initColorString = initColor.ToArgb();

            if (baseColors.Contains(initColorString))
            {
                dialog.Color = initColor;
            }
            
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ((System.Windows.Shapes.Rectangle)sender).Fill = new SolidColorBrush(Color.FromRgb(dialog.Color.R, dialog.Color.G, dialog.Color.B));
                LayerProvider.UseCustomColors = true;

                Int32 colValue = dialog.Color.ToArgb();

                LayerProvider.CustomColorsDictionary.Add(((System.Windows.Shapes.Rectangle)sender).Name, Color.FromRgb(dialog.Color.R, dialog.Color.G, dialog.Color.B));

                if (((System.Windows.Shapes.Rectangle)sender).Tag is String && ((String)((System.Windows.Shapes.Rectangle)sender).Tag) == "Reset_Legend")
                {
                    Reset_Legend();
                }
            }
        }

        private void ClassCount_Changed(object sender, SelectionChangedEventArgs e)
        {
            Reset_Legend();
        }

        private void Reset_Legend()
        {
            if (cmbClasses.SelectedItem != null)
            {
                if (((ComboBoxItem)cmbClasses.SelectedItem).Content == null || LayerProvider == null)
                {
                    return;
                }
            }

            if (rctLowColor == null || rctHighColor == null)
            {
                return;
            }

            int stratCount;


            if (int.TryParse(cmbClasses.Text, out stratCount) == false)
            {
                stratCount = 4;
            }

            int classCount=0;
            if (cmbClasses.SelectedItem != null)
            {
                if (!int.TryParse(((ComboBoxItem)cmbClasses.SelectedItem).Content.ToString(), out classCount))
                {
                    classCount = 4;
                }
            }

            bool allVariablesSelected = (cmbShapeKey.SelectedItem != null && cmbDataKey.SelectedItem != null && cmbValue.SelectedItem != null);

            if (LayerProvider != null && allVariablesSelected )
            {
                if (quintilesOption.IsChecked == true)
                {
                    LayerProvider.RangesLoadedFromMapFile = false;

                    LayerProvider.ResetRangeValues(
                        cmbShapeKey.SelectedItem.ToString(),
                        cmbDataKey.SelectedItem.ToString(),
                        cmbValue.SelectedItem.ToString(),
                        classCount
                        );
                }
            }

            SolidColorBrush rampStart = (SolidColorBrush)rctLowColor.Fill;
            SolidColorBrush rampEnd = (SolidColorBrush)rctHighColor.Fill;

            if (cmbShapeKey.SelectedItem != null && cmbDataKey.SelectedItem != null && cmbValue.SelectedItem != null)
            {
                LayerProvider.UseCustomColors = false;
                SetRangeUISection();
            }

            if (string.IsNullOrEmpty(legTitle.Text))
            {
                if (LayerProvider.LegendText != null)
                {
                    legTitle.Text = LayerProvider.LegendText;
                }
                else if (_value != null)
                {
                    legTitle.Text = _value;
                }
            }

            PropertyChanged_EnableDisable();
        }

        public void SetVisibility(int stratCount, SolidColorBrush rampStart, SolidColorBrush rampEnd)
        {
            if (LayerProvider == null) return;
            
            bool isNewColorRamp = true;
            minMaxText.Text = DashboardSharedStrings.DASHBOARD_MAP_MIN + " " + _layerProvider._thematicItem.Min + "; ";
            minMaxText.Text += DashboardSharedStrings.DASHBOARD_MAP_MAX + " " + _layerProvider._thematicItem.Max + "; N = " + _layerProvider._thematicItem.RowCount + "" ;

            quintilesOption.IsChecked = LayerProvider.UseQuantiles;

            SolidColorBrush rampMissing = (SolidColorBrush)rctMissingColor.Fill;

            if (Equals(rampStart, _currentColor_rampStart) &&
                Equals(rampEnd, _currentColor_rampEnd) &&
                Equals(rampMissing, _currentColor_rampMissing) &&
                stratCount == _currentStratCount)
            {
                if (_initialRampCalc == false)
                {
                    isNewColorRamp = false;
                }
            }

            _currentStratCount = stratCount;
            _currentColor_rampStart = rampStart;
            _currentColor_rampMissing = rampMissing;
            _currentColor_rampEnd = rampEnd;

            int rd = rampStart.Color.R - rampEnd.Color.R;
            int gd = rampStart.Color.G - rampEnd.Color.G;
            int bd = rampStart.Color.B - rampEnd.Color.B;

            byte ri = (byte)(rd / (stratCount - 1));
            byte gi = (byte)(gd / (stratCount - 1));
            byte bi = (byte)(bd / (stratCount - 1));

            if (LayerProvider != null)
            {
                if (LayerProvider.UseCustomColors)
                {
                    rctMissingColor.Fill = new SolidColorBrush(LayerProvider.CustomColorsDictionary.GetWithKey(rctMissingColor.Name));
                    rctColor1.Fill = new SolidColorBrush(LayerProvider.CustomColorsDictionary.GetWithKey(rctColor1.Name));
                }
                else
                {
                    rctColor1.Fill = rampStart;
                    LayerProvider.CustomColorsDictionary.Add(rctColor1.Name, rampStart.Color);
                }

                if (LayerProvider != null)
                {
                    if(LayerProvider.ClassRangesDictionary.RangeDictionary.Count == 0)
                    {
                        LayerProvider.ClassRangesDictionary.SetRangesDictionary(LayerProvider.RangeValues);
                    }
                    
                    rampStart01.Text = LayerProvider.ClassRangesDictionary.GetAt("rampStart01");
                    rampEnd01.Text = LayerProvider.ClassRangesDictionary.GetAt("rampEnd01");
                }

                rctColor1.Visibility = System.Windows.Visibility.Visible;

                rampStart01.Visibility = System.Windows.Visibility.Hidden;
                centerText01.Text = "   X <";

                centerText01.Visibility = System.Windows.Visibility.Visible;
                rampEnd01.Visibility = System.Windows.Visibility.Visible;
                legendText1.Visibility = System.Windows.Visibility.Visible;

                Color color;
                int i = 2;
                int gradientControl = 1;

                List<UIControls> classControls = CreateUIControlsList();

                foreach (UIControls rowControls in classControls)
                {
                    if (LayerProvider.UseCustomColors)
                    {
                        color = LayerProvider.CustomColorsDictionary.GetWithKey(rowControls.rectangle.Name);
                    }
                    else
                    {
                        color = Color.FromArgb(255, 
                            (byte)(rampStart.Color.R - ri * gradientControl), 
                            (byte)(rampStart.Color.G - gi * gradientControl), 
                            (byte)(rampStart.Color.B - bi * gradientControl));

                        LayerProvider.CustomColorsDictionary.Add(rowControls.rectangle.Name, color);
                    }

                    gradientControl++;

                    rowControls.rectangle.Visibility = System.Windows.Visibility.Collapsed;
                    rowControls.rampStarts.Visibility = System.Windows.Visibility.Collapsed;
                    rowControls.centerTexts.Visibility = System.Windows.Visibility.Collapsed;
                    rowControls.rampEnds.Visibility = System.Windows.Visibility.Collapsed;
                    rowControls.legedTexts.Visibility = System.Windows.Visibility.Collapsed;

                    if (i < stratCount) // MIDDLE CLASSES
                    {
                        rowControls.rectangle.Visibility = System.Windows.Visibility.Visible;
                        rowControls.rampStarts.Visibility = System.Windows.Visibility.Visible;
                        rowControls.centerTexts.Visibility = System.Windows.Visibility.Visible;
                        rowControls.centerTexts.Text = "\u2264 X <";
                        rowControls.rampEnds.Visibility = System.Windows.Visibility.Visible;
                        rowControls.legedTexts.Visibility = System.Windows.Visibility.Visible;

                    }
                    else if (i == stratCount) // LAST CLASS
                    {
                        rowControls.rectangle.Visibility = System.Windows.Visibility.Visible;
                        rowControls.rampStarts.Visibility = System.Windows.Visibility.Visible;
                        rowControls.centerTexts.Visibility = System.Windows.Visibility.Visible;
                        rowControls.rampEnds.Visibility = System.Windows.Visibility.Visible;

                        rowControls.rampEnds.Visibility = System.Windows.Visibility.Hidden;
                        rowControls.centerTexts.Text = "\u2264 X   ";

                        rowControls.legedTexts.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        color = Color.FromArgb(Opacity, byte.MaxValue, byte.MaxValue, byte.MaxValue);
                        rowControls.rectangle.Visibility = System.Windows.Visibility.Collapsed;
                        rowControls.rampStarts.Visibility = System.Windows.Visibility.Collapsed;
                        rowControls.centerTexts.Visibility = System.Windows.Visibility.Collapsed;
                        rowControls.rampEnds.Visibility = System.Windows.Visibility.Collapsed;
                        rowControls.legedTexts.Visibility = System.Windows.Visibility.Collapsed;
                    }

                    i++;

                    if (isNewColorRamp) rowControls.rectangle.Fill = new SolidColorBrush(color);

                    if (LayerProvider != null )
                    {
                        rowControls.rampStarts.Text = LayerProvider.ClassRangesDictionary.GetAt(rowControls.rampStarts.Name);
                        rowControls.rampEnds.Text = LayerProvider.ClassRangesDictionary.GetAt(rowControls.rampEnds.Name);
                    }
                }

                EnableDisableClassRangeInput();
            }

            _initialRampCalc = false;
        }

        private void EnableDisableClassRangeInput()
        {
            foreach (UIElement element in stratGrid.Children)
            {
                if (element is System.Windows.Controls.TextBox)
                {
                    string elementName = ((System.Windows.Controls.TextBox)element).Name;

                    if (elementName.StartsWith("ramp"))
                    {
                        ((System.Windows.Controls.TextBox)element).IsEnabled = !(bool)quintilesOption.IsChecked;
                    }
                }
            }
        }

        private List<UIControls> CreateUIControlsList()
        {
            return new List<UIControls>()
            {
                new UIControls()
                {
                    centerTexts = centerText02,
                    legedTexts = legendText2,
                    rectangle = rctColor2,
                    rampEnds = rampEnd02,
                    rampStarts = rampStart02
                },
                new UIControls()
                {
                    centerTexts = centerText03,
                    legedTexts = legendText3,
                    rectangle = rctColor3,
                    rampEnds = rampEnd03,
                    rampStarts = rampStart03
                }, 
                new UIControls()
                {
                    centerTexts = centerText04,
                    legedTexts = legendText4,
                    rectangle = rctColor4,
                    rampEnds = rampEnd04,
                    rampStarts = rampStart04
                },
                new UIControls()
                {
                    centerTexts = centerText05,
                    legedTexts = legendText5,
                    rectangle = rctColor5,
                    rampEnds = rampEnd05,
                    rampStarts = rampStart05
                },
                new UIControls()
                {
                    centerTexts = centerText06,
                    legedTexts = legendText6,
                    rectangle = rctColor6,
                    rampEnds = rampEnd06,
                    rampStarts = rampStart06
                },
                new UIControls()
                {
                    centerTexts = centerText07,
                    legedTexts = legendText7,
                    rectangle = rctColor7,
                    rampEnds = rampEnd07,
                    rampStarts = rampStart07
                },
                new UIControls()
                {
                    centerTexts = centerText08,
                    legedTexts = legendText8,
                    rectangle = rctColor8,
                    rampEnds = rampEnd08,
                    rampStarts = rampStart08
                },
                new UIControls()
                {
                    centerTexts = centerText09,
                    legedTexts = legendText9,
                    rectangle = rctColor9,
                    rampEnds = rampEnd09,
                    rampStarts = rampStart09
                },
                new UIControls()
                {
                    centerTexts = centerText10,
                    legedTexts = legendText10,
                    rectangle = rctColor10,
                    rampEnds = rampEnd10 ,
                    rampStarts = rampStart10  
                },
            };
        }

        private void CheckBox_Quantiles_Click(object sender, RoutedEventArgs e)
        {
            OnQuintileOptionChanged();
        }

        public void OnQuintileOptionChanged()
        {
            if (LayerProvider != null)
            {
                LayerProvider.UseQuantiles = (bool)quintilesOption.IsChecked;
            }

            int widthQuantile = 0;
            int widthMinMax = 75;
            int widthCompare = 50;

            if (quintilesOption.IsChecked == true)
            {
                Reset_Legend();
            }

            quintileColumn.Width = new GridLength(widthQuantile, GridUnitType.Pixel);

            rampStartColumn.Width = new GridLength(widthMinMax, GridUnitType.Pixel);
            rampCompareColumn.Width = new GridLength(widthCompare, GridUnitType.Pixel);
            rampEndColumn.Width = new GridLength(widthMinMax, GridUnitType.Pixel);

            EnableDisableClassRangeInput();
        }

        private void CheckBox_showPolylabels_Click(object sender, RoutedEventArgs e)
        {
            OnShowPolylabelsChanged();
        }

        public void OnShowPolylabelsChanged()
        {
            if (LayerProvider != null)
            {
                LayerProvider.ShowPolyLabels = (bool)showPolyLabels.IsChecked;
            }

            //if (showPolyLabels.IsChecked == true)
            //{
            //    Reset_Legend();
            //}
            //PropertyChanged_EnableDisable();
        }


        public void cmbShapeKey_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbShapeKey.SelectedItem != null)
            {
                _shapeKey = cmbShapeKey.SelectedItem.ToString();
                cmbDataKey.IsEnabled = true;               
            }
            else
            { 
                _shapeKey = ""; 
            }

            PropertyChanged_EnableDisable();
        }

        private void cmbDataKey_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ReFillValueComboBoxes();

            if (cmbDataKey.SelectedItem != null)
            {
                _dataKey = cmbDataKey.SelectedItem.ToString();
                cmbValue.IsEnabled = true;
            }

            if (cmbValue.Items.Contains(_dataKey))
            {
                cmbValue.Items.Remove(_dataKey);
            }

            PropertyChanged_EnableDisable();
        }

        public void cmbValue_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbValue.SelectedItem != null)
            {
                _value = cmbValue.SelectedItem.ToString();
            }
            PropertyChanged_EnableDisable();
        }

        private void SetRangeUISection()
        {
            List<SolidColorBrush> brushList = new List<SolidColorBrush>() { 
                    (SolidColorBrush)rctColor1.Fill, 
                    (SolidColorBrush)rctColor2.Fill, 
                    (SolidColorBrush)rctColor3.Fill, 
                    (SolidColorBrush)rctColor4.Fill, 
                    (SolidColorBrush)rctColor5.Fill, 
                    (SolidColorBrush)rctColor6.Fill, 
                    (SolidColorBrush)rctColor7.Fill, 
                    (SolidColorBrush)rctColor8.Fill, 
                    (SolidColorBrush)rctColor9.Fill, 
                    (SolidColorBrush)rctColor10.Fill,  
                    (SolidColorBrush)rctMissingColor.Fill };

            int classCount=0;
            if (cmbClasses.SelectedItem!=null)
            if (!int.TryParse(((ComboBoxItem)cmbClasses.SelectedItem).Content.ToString(), out classCount))
            {
                classCount = 4;
            }

            if (radShapeFile.IsChecked == true && LayerProvider != null)
            {
            }
            else if (radMapServer.IsChecked == true && choroplethServerLayerProvider != null)
            {
                if (LayerProvider == null)
                {
                    LayerProvider = choroplethServerLayerProvider;
                }
            }
            else if (radKML.IsChecked == true && choroplethKmlLayerProvider != null)
            {
                if (LayerProvider == null)
                {
                    LayerProvider = choroplethKmlLayerProvider;
                }
            }

            if (quintilesOption.IsChecked == true)
            {
                LayerProvider.PopulateRangeValues();
            }

            if (string.IsNullOrEmpty(legTitle.Text))
            {
                legTitle.Text = LayerProvider.LegendText;
            }
            else
            {
                LayerProvider.LegendText = legTitle.Text;
            }
            
            if (LayerProvider.ShowPolyLabels == true)
            {
                showPolyLabels.IsChecked = true;
            }
            else
            {
                showPolyLabels.IsChecked = false;
            }

            try
            {
                for (int x = 1; x <= ChoroplethConstants.MAX_CLASS_DECRIPTION_COUNT; x++)
                {
                    System.Windows.Controls.TextBox t = FindName(ChoroplethConstants.legendTextControlPrefix + x) as System.Windows.Controls.TextBox;
                    if (string.IsNullOrEmpty(t.Text))
                    {
                        t.Text = LayerProvider.ListLegendText.GetWithKey(ChoroplethConstants.legendTextControlPrefix + x);
                    }
                    else
                    {
                        LayerProvider.ListLegendText.Add(ChoroplethConstants.legendTextControlPrefix + x, t.Text);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            SolidColorBrush rampStart = (SolidColorBrush)rctLowColor.Fill;
            SolidColorBrush rampEnd = (SolidColorBrush)rctHighColor.Fill;

            SetVisibility(classCount, rampStart, rampEnd);
        }

        private List<double> GetRangeValuesFromAttributeList(Dictionary<int, object> dictionary, int classCount)
        {
            List<double> RangeAttr = new List<double>();
            double value = 0.0;

            if (dictionary == null)
                return RangeAttr;

            if (classCount >= 2 && dictionary.Count > 1)
            {
                classAttributes class1 = (classAttributes)dictionary[1];
                double.TryParse(class1.rampStart, out value);
                RangeAttr.Add(value);

                class1 = (classAttributes)dictionary[2];
                double.TryParse(class1.rampStart, out value);
                RangeAttr.Add(value);
            }

            if (classCount >= 3 && dictionary.Count > 2)
            {
                classAttributes class1 = (classAttributes)dictionary[3];
                double.TryParse(class1.rampStart, out value);
                RangeAttr.Add(value);
            }
            if (classCount >= 4 && dictionary.Count > 3)
            {
                classAttributes class1 = (classAttributes)dictionary[4];
                double.TryParse(class1.rampStart, out value);
                RangeAttr.Add(value);
            }
            if (classCount >= 5 && dictionary.Count > 4)
            {
                classAttributes class1 = (classAttributes)dictionary[5];
                double.TryParse(class1.rampStart, out value);
                RangeAttr.Add(value);
            }
            if (classCount >= 6 && dictionary.Count > 5)
            {
                classAttributes class1 = (classAttributes)dictionary[6];
                double.TryParse(class1.rampStart, out value);
                RangeAttr.Add(value);
            }
            if (classCount >= 7 && dictionary.Count > 6)
            {
                classAttributes class1 = (classAttributes)dictionary[7];
                double.TryParse(class1.rampStart, out value);
                RangeAttr.Add(value);
            }
            if (classCount >= 8 && dictionary.Count > 7)
            {
                classAttributes class1 = (classAttributes)dictionary[8];
                double.TryParse(class1.rampStart, out value);
                RangeAttr.Add(value);
            }
            if (classCount >= 9 && dictionary.Count > 8)
            {
                classAttributes class1 = (classAttributes)dictionary[9];
                double.TryParse(class1.rampStart, out value);
                RangeAttr.Add(value);
            }

            return RangeAttr;
        }

        private void btnBrowseShapeFile_Click(object sender, RoutedEventArgs e)
        {
            LayerProvider = new Mapping.ChoroplethShapeLayerProvider(_myMap);
            object[] shapeFileProperties = LayerProvider.Load();

            if (shapeFileProperties != null)
            {
                layerAddednew.Add(LayerProvider.LayerId.ToString());
                
                if (choroplethShapeLayerProperties == null)
                {
                    ILayerProperties layerProperties = null;
                    layerProperties = new ChoroplethShapeLayerProperties(_myMap, this.DashboardHelper, this._mapControl);
                    layerProperties.MapGenerated += new EventHandler(this._mapControl.ILayerProperties_MapGenerated);
                    layerProperties.FilterRequested += new EventHandler(this._mapControl.ILayerProperties_FilterRequested);
                    layerProperties.EditRequested += new EventHandler(this._mapControl.ILayerProperties_EditRequested);
                    this.choroplethShapeLayerProperties = (ChoroplethShapeLayerProperties)layerProperties;
                    this._mapControl.grdLayerConfigContainer.Children.Add((UIElement)layerProperties);
                }

                choroplethShapeLayerProperties.Provider = LayerProvider as ChoroplethShapeLayerProvider;

                if (this.DashboardHelper != null)
                {
                    choroplethShapeLayerProperties.SetdashboardHelper(DashboardHelper);
                }

                if (shapeFileProperties.Length == 2)
                {
                    txtShapePath.Text = shapeFileProperties[0].ToString();
                    choroplethShapeLayerProperties.boundryFilePath = shapeFileProperties[0].ToString();
                    choroplethShapeLayerProperties.shapeAttributes = (IDictionary<string, object>)shapeFileProperties[1];
                    shapeAttributes = (IDictionary<string, object>)shapeFileProperties[1];

                    List<string> keyList = new List<string>();

                    if (shapeAttributes != null)
                    {
                        keyList = shapeAttributes.Keys.ToList();
                        
                        cmbShapeKey.Items.Clear();
                        choroplethShapeLayerProperties.cbxShapeKey.Items.Clear();

                        foreach (string key in keyList)
                        {
                            cmbShapeKey.Items.Add(key);
                            choroplethShapeLayerProperties.cbxShapeKey.Items.Add(key);
                        }
                    }
                }
            }
        }

        private void btnKMLFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "KML/KMZ Files (*.kml/*.kmz)|*.kml;*.kmz";
            dialog.Multiselect = false;

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtKMLpath.Text = dialog.FileName;
                KMLMapServerName = txtKMLpath.Text;
            }

            if (choroplethKmlLayerProvider == null)
            {
                choroplethKmlLayerProvider = new Mapping.ChoroplethKmlLayerProvider(_myMap);
                choroplethKmlLayerProvider.FeatureLoaded += new FeatureLoadedHandler(choroKMLprovider_FeatureLoaded);
            }

            LayerProvider = choroplethKmlLayerProvider;

            object[] kmlFileProperties = choroplethKmlLayerProvider.Load(KMLMapServerName);

            if (kmlFileProperties != null)
            {
                layerAddednew.Add(choroplethKmlLayerProvider.LayerId.ToString());
                
                if (choroplethKmlLayerProperties == null)
                {
                    ILayerProperties layerProperties = null;
                    layerProperties = new ChoroplethKmlLayerProperties(_myMap, this.DashboardHelper, this._mapControl);
                    layerProperties.MapGenerated += new EventHandler(this._mapControl.ILayerProperties_MapGenerated);
                    layerProperties.FilterRequested += new EventHandler(this._mapControl.ILayerProperties_FilterRequested);
                    layerProperties.EditRequested += new EventHandler(this._mapControl.ILayerProperties_EditRequested);
                    this.choroplethKmlLayerProperties = (ChoroplethKmlLayerProperties)layerProperties;
                    this._mapControl.grdLayerConfigContainer.Children.Add((UIElement)layerProperties);
                }
                
                choroplethKmlLayerProperties.boundryFilePath = KMLMapServerName;
                choroplethKmlLayerProperties.Provider = choroplethKmlLayerProvider;
                choroplethKmlLayerProperties.Provider.FeatureLoaded += new FeatureLoadedHandler(choroplethKmlLayerProperties.provider_FeatureLoaded);
                choroplethKmlLayerProperties.Provider.LayerId = choroplethKmlLayerProvider.LayerId;
                
                if (this.DashboardHelper != null)
                {
                    choroplethKmlLayerProperties.SetdashboardHelper(DashboardHelper);
                }
            }
        }

        public void MapFeatureSelectionChange()
        {
            if (cbxmapfeature.Items.Count > 0)
            {
                MapServerName = txtMapSeverpath.Text;
                
                if (cbxmapfeature.SelectedIndex > -1)
                {
                    int visibleLayer = ((SubObject)cbxmapfeature.SelectedItem).id;
                    MapVisibleLayer = visibleLayer;
                    
                    if (choroplethServerLayerProvider == null)
                    {
                        choroplethServerLayerProvider = (ChoroplethServerLayerProvider)LayerProvider;
                        choroplethServerLayerProvider.FeatureLoaded += new FeatureLoadedHandler(choroMapprovider_FeatureLoaded);
                    }
                    
                    object[] mapFileProperties = choroplethServerLayerProvider.Load(MapServerName + "/" + MapVisibleLayer);              
                    
                    if (mapFileProperties != null)
                    {
                        layerAddednew.Add(choroplethServerLayerProvider.LayerId.ToString());
                        
                        if (choroplethServerLayerProperties == null)
                        {
                            ILayerProperties layerProperties = null;
                            layerProperties = new ChoroplethServerLayerProperties(_myMap, this.DashboardHelper, this._mapControl);
                            ((ChoroplethServerLayerProperties)layerProperties).Provider = choroplethServerLayerProvider;
                            ((ChoroplethServerLayerProperties)layerProperties).Provider.LayerId = choroplethServerLayerProvider.LayerId;
                            ((ChoroplethServerLayerProperties)layerProperties).Provider.FeatureLoaded += new FeatureLoadedHandler(((ChoroplethServerLayerProperties)layerProperties).provider_FeatureLoaded);
                            layerProperties.MapGenerated += new EventHandler(this._mapControl.ILayerProperties_MapGenerated);
                            layerProperties.FilterRequested += new EventHandler(this._mapControl.ILayerProperties_FilterRequested);
                            layerProperties.EditRequested += new EventHandler(this._mapControl.ILayerProperties_EditRequested);
                            this.choroplethServerLayerProperties = (ChoroplethServerLayerProperties)layerProperties;                         
                            this._mapControl.grdLayerConfigContainer.Children.Add((UIElement)layerProperties);
                        }
                        
                        choroplethServerLayerProperties.boundryFilePath = MapServerName;
                        choroplethServerLayerProperties.Provider = choroplethServerLayerProvider;
                        choroplethServerLayerProperties.Provider.FeatureLoaded += new FeatureLoadedHandler(choroplethServerLayerProperties.provider_FeatureLoaded);
                        choroplethServerLayerProperties.Provider.LayerId = choroplethServerLayerProvider.LayerId;

                        if (this.DashboardHelper != null)
                        {
                            choroplethServerLayerProperties.SetdashboardHelper(DashboardHelper);
                        }
                    }
                }
                else
                {
                    MapVisibleLayer = -1;
                }
            }
        }

        private void Addfilters()
        {
            try
            {
                OnQuintileOptionChanged();

                this.datafilters = rowFilterControl.DataFilters;

                if (radShapeFile.IsChecked == true && this.choroplethShapeLayerProperties != null) 
                { 
                    choroplethShapeLayerProperties.datafilters = rowFilterControl.DataFilters; 
                }
                
                if (radMapServer.IsChecked == true && this.choroplethServerLayerProperties != null) 
                {
                    choroplethServerLayerProperties.datafilters = rowFilterControl.DataFilters;
                }

                if (radKML.IsChecked == true && choroplethKmlLayerProperties != null)
                {
                    choroplethKmlLayerProperties.datafilters = rowFilterControl.DataFilters;
                }

                this.datafilters = rowFilterControl.DataFilters;
                _dashboardHelper.SetDatafilters(this.datafilters);
            }
            catch (Exception)
            {
                throw new Exception();
            }
        }

        public void choroMapprovider_FeatureLoaded(string serverName, IDictionary<string, object> featureAttributes)
        {
            if (!string.IsNullOrEmpty(serverName))
            {
                shapeFilePath = serverName;
                if (featureAttributes != null)
                {
                    cmbShapeKey.Items.Clear();
                    cmbShapeKey.SelectedIndex = -1;
                    choroplethServerLayerProperties.cbxShapeKey.Items.Clear();
                    foreach (string key in featureAttributes.Keys)
                    {
                        if (key != "EpiInfoValCol")
                        {
                            cmbShapeKey.Items.Add(key);
                            choroplethServerLayerProperties.cbxShapeKey.Items.Add(key);
                        }
                    }
                    cmbShapeKey.Items.Refresh();
                    if (!string.IsNullOrEmpty(((EpiDashboard.Mapping.ChoroplethLayerProvider)(choroplethServerLayerProvider))._shapeKey))
                        cmbShapeKey.SelectedItem = ((EpiDashboard.Mapping.ChoroplethLayerProvider)(choroplethServerLayerProvider))._shapeKey;
                }
            }

            if (currentElement != null)
            {
                foreach (System.Xml.XmlElement child in currentElement.ChildNodes)
                {
                    if (child.Name.Equals("dataKey"))
                    {
                        cmbDataKey.SelectedItem = child.InnerText;
                    }
                    if (child.Name.Equals("shapeKey"))
                    {
                        cmbShapeKey.SelectedItem = child.InnerText;
                    }
                    if (child.Name.Equals("value"))
                    {
                        cmbValue.SelectedItem = child.InnerText;
                    }
                    if (child.Name.Equals("dotValue"))
                    {
                        // txtDotValue.Text = child.InnerText;
                    }
                    if (child.Name.Equals("dotColor"))
                    {
                        //rctDotColor.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(child.InnerText));
                    }
                }

                legTitle.Text = LayerProvider.LegendText;
                
                if (LayerProvider.ShowPolyLabels == true)
                {
                    showPolyLabels.IsChecked = true;
                }
                else
                {
                    showPolyLabels.IsChecked = false;
                }
            }
        }

        void choroKMLprovider_FeatureLoaded(string serverName, IDictionary<string, object> featureAttributes)
        {
            if (!string.IsNullOrEmpty(serverName))
            {
                shapeFilePath = serverName;
                if (featureAttributes != null)
                {
                    cmbShapeKey.Items.Clear();
                    choroplethKmlLayerProperties.cbxShapeKey.Items.Clear();
                    foreach (string key in featureAttributes.Keys)
                    {
                        cmbShapeKey.Items.Add(key);
                        choroplethKmlLayerProperties.cbxShapeKey.Items.Add(key);
                    }
                }
            }
            
            if (currentElement != null)
            {
                foreach (System.Xml.XmlElement child in currentElement.ChildNodes)
                {
                    if (child.Name.Equals("dataKey"))
                    {
                        cmbDataKey.SelectedItem = child.InnerText;
                    }
                    if (child.Name.Equals("shapeKey"))
                    {
                        cmbShapeKey.SelectedItem = child.InnerText;
                    }
                    if (child.Name.Equals("value"))
                    {
                        cmbValue.SelectedItem = child.InnerText;
                    }
                    if (child.Name.Equals("dotValue"))
                    {
                        //txtDotValue.Text = child.InnerText;
                    }
                    if (child.Name.Equals("dotColor"))
                    {
                        //rctDotColor.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(child.InnerText));
                    }
                }
                // RenderMap();
            }
        }


        private void radShapeFile_Checked(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtKMLpath.Text))
            {
                MessageBoxResult result = System.Windows.MessageBox.Show(DashboardSharedStrings.MAP_CHANGE_BOUNDARY_ALERT, DashboardSharedStrings.ALERT, MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    radShapeFile.IsChecked = false;
                    radKML.IsChecked = true;
                    return;
                }
                else
                    ClearonShapeFile();
            }
            else if (!string.IsNullOrEmpty(txtMapSeverpath.Text))
            {
                MessageBoxResult result = System.Windows.MessageBox.Show(DashboardSharedStrings.MAP_CHANGE_BOUNDARY_ALERT, DashboardSharedStrings.ALERT, MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    radShapeFile.IsChecked = false;
                    radMapServer.IsChecked = true;
                    return;
                }
                else
                    ClearonShapeFile();
            }
            else if (!string.IsNullOrEmpty(txtShapePath.Text))
            {
                return;
            }
            else
            {
                ClearonShapeFile();
            }
        }

        private void ClearonKML()
        {
            panelshape.IsEnabled = false;
            panelmap.IsEnabled = false;
            panelKml.IsEnabled = true;

            panelmapserver.IsEnabled = false;

            cmbShapeKey.Items.Clear();
            txtShapePath.Text = string.Empty;
            txtKMLpath.Text = string.Empty;
            cbxmapfeature.SelectedIndex = -1;
            txtMapSeverpath.Text = string.Empty;
            if (choroplethServerLayerProvider != null)
            {
                choroplethServerLayerProvider.FeatureLoaded -= new FeatureLoadedHandler(choroMapprovider_FeatureLoaded);
                choroplethServerLayerProvider = null;
            }
            if (LayerProvider != null)
            {
                LayerProvider.CloseLayer();
            }
        }

        public void ClearonShapeFile()
        {
            panelshape.IsEnabled = true;
            panelmap.IsEnabled = false;
            panelKml.IsEnabled = false;

            panelmapserver.IsEnabled = false;

            cmbShapeKey.Items.Clear();
            txtShapePath.Text = string.Empty;
            txtKMLpath.Text = string.Empty;
            cbxmapfeature.SelectedIndex = -1;
            txtMapSeverpath.Text = string.Empty;
            if (choroplethServerLayerProvider != null)
            {
                choroplethServerLayerProvider.FeatureLoaded -= new FeatureLoadedHandler(choroMapprovider_FeatureLoaded);
                choroplethServerLayerProvider = null;
            }
            if (choroplethKmlLayerProvider != null)
            {
                choroplethKmlLayerProvider.FeatureLoaded -= new FeatureLoadedHandler(choroKMLprovider_FeatureLoaded);
                choroplethKmlLayerProvider = null;
            }
        }

        public void ClearonMapServer()
        {
            panelshape.IsEnabled = false;
            panelmap.IsEnabled = true;
            panelKml.IsEnabled = false;

            panelmapserver.IsEnabled = true;

            cmbShapeKey.Items.Clear();
            txtShapePath.Text = string.Empty;
            txtKMLpath.Text = string.Empty;
            cbxmapfeature.SelectedIndex = -1;
            txtMapSeverpath.Text = string.Empty;
            
            if (choroplethKmlLayerProvider != null)
            {
                choroplethKmlLayerProvider.FeatureLoaded -= new FeatureLoadedHandler(choroKMLprovider_FeatureLoaded);
                choroplethKmlLayerProvider = null;
            }
            
            if (LayerProvider != null)
            {
                choroplethShapeLayerProperties.CloseLayer();
            }
        }


        private void radKML_Checked(object sender, RoutedEventArgs e)
        {

            if (!string.IsNullOrEmpty(txtShapePath.Text))
            {
                MessageBoxResult result = System.Windows.MessageBox.Show(DashboardSharedStrings.MAP_CHANGE_BOUNDARY_ALERT, DashboardSharedStrings.ALERT, MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    radKML.IsChecked = false;
                    radShapeFile.IsChecked = true;
                    return;
                }
                else
                    ClearonKML();
            }
            else if (!string.IsNullOrEmpty(txtMapSeverpath.Text))
            {
                MessageBoxResult result = System.Windows.MessageBox.Show(DashboardSharedStrings.MAP_CHANGE_BOUNDARY_ALERT, DashboardSharedStrings.ALERT, MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    radKML.IsChecked = false;
                    radMapServer.IsChecked = true;
                    return;
                }
                else
                    ClearonKML();
            }
            else if (!string.IsNullOrEmpty(txtKMLpath.Text))
            {
                return;
            }
            else
                ClearonKML();
        }

        private void radMapServer_Checked(object sender, RoutedEventArgs e)
        {

            if (!string.IsNullOrEmpty(txtShapePath.Text))
            {
                MessageBoxResult result = System.Windows.MessageBox.Show(DashboardSharedStrings.MAP_CHANGE_BOUNDARY_ALERT, DashboardSharedStrings.ALERT, MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    radMapServer.IsChecked = false;
                    radShapeFile.IsChecked = true;
                    return;
                }
                else
                {
                    ClearonMapServer();
                }
            }
            else if (!string.IsNullOrEmpty(txtKMLpath.Text))
            {
                MessageBoxResult result = System.Windows.MessageBox.Show(DashboardSharedStrings.MAP_CHANGE_BOUNDARY_ALERT, DashboardSharedStrings.ALERT, MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    radMapServer.IsChecked = false;
                    radKML.IsChecked = true;
                    return;
                }
                else
                {
                    ClearonMapServer();
                }
            }
            else if (!string.IsNullOrEmpty(txtMapSeverpath.Text) )
            {
                return;
            }
            else
            {
                ClearonMapServer();
            }
        }

        private void radconnectmapserver_Checked(object sender, RoutedEventArgs e)
        {
            panelmapconnect.IsEnabled = true;
            panelmapserver.IsEnabled = false;
            panelmapconnect.IsEnabled = true;
            txtMapSeverpath.Text = string.Empty;

            panelmapconnect.IsEnabled = false;
            panelmapserver.IsEnabled = true;
            panelmapconnect.IsEnabled = false;
        }

        public void cbxmapserver_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PropertyChanged_EnableDisable();
        }

        private void ResetShapeCombo()
        {
            if (radMapServer.IsChecked == true)
            {
                cmbShapeKey.Items.Clear();
                cmbShapeKey.SelectedIndex = -1;
                if (choroplethServerLayerProperties != null)
                {
                    choroplethServerLayerProperties.cbxShapeKey.Items.Clear();
                    choroplethServerLayerProperties.cbxShapeKey.SelectedIndex = -1;
                }
            }
        }

        private void btnMapserverlocate_Click(object sender, RoutedEventArgs e)
        {
            MapServerConnect();
        }

        public void MapServerConnect()
        {
            try
            {
                if (LayerProvider == null)
                {
                    LayerProvider = new Mapping.ChoroplethServerLayerProvider(_myMap);
                }
                
                string message = GetMessage(txtMapSeverpath.Text + "?f=json");
                System.Web.Script.Serialization.JavaScriptSerializer ser = new System.Web.Script.Serialization.JavaScriptSerializer();
                Rest rest = ser.Deserialize<Rest>(message);
                cbxmapfeature.ItemsSource = rest.layers;
            }
            catch
            {
                cbxmapfeature.DataContext = null;
                System.Windows.Forms.MessageBox.Show(DashboardSharedStrings.DASHBOARD_MAP_INVALID_MAP_SERVER);
            }
        }

        private void txtMapSeverpath_TextChanged(object sender, TextChangedEventArgs e)
        {
            btnMapserverlocate.IsEnabled = txtMapSeverpath.Text.Length > 0;
        }

        public void cbxmapfeature_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(sender is System.Windows.Controls.ComboBox)
            {
                if (((System.Windows.Controls.ComboBox)sender).SelectedIndex != -1)
                {
                    PropertyChanged_EnableDisable();
                }
            }

            MapFeatureSelectionChange();
        }

        private void Slider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender == null || txtOpacity == null)
            {
                return;
            }
            double value = ((Slider)sender).Value;
            double calculatedPercentage = (value - 100) / (255 - 100);
            txtOpacity.Text = string.Format("{0:N2}", calculatedPercentage * 100) + "%";

            brush.Opacity = calculatedPercentage;
        }

        private void rampValue_LostFocus(object sender, RoutedEventArgs e)
        {
            if ((sender is System.Windows.Controls.TextBox) == false)
            {
                return;
            }

            System.Windows.Controls.TextBox textBox = ((System.Windows.Controls.TextBox)sender);
            string newText = textBox.Text;
            float newValue = float.NaN;

            string name = textBox.Name;

            int classLevel = LayerProvider.ClassRangesDictionary.GetClassLevelWithKey(name);
            ClassLimitType limit = LayerProvider.ClassRangesDictionary.GetLimitTypeWithKey(name);

            if (float.TryParse(newText, out newValue) == false)
            {
                System.Windows.Controls.TextBox found = (System.Windows.Controls.TextBox)this.FindName(name);
                if (LayerProvider.ClassRangesDictionary.RangeDictionary.ContainsKey(name))
                {
                    found.Text = LayerProvider.ClassRangesDictionary.RangeDictionary[name];
                }
                return;
            }

            UpdateClassRangesDictionary_FromControls();

            if (IsMissingLimitValue())
            {
                return;
            }

            List<ClassLimits> limits = LayerProvider.ClassRangesDictionary.GetLimitValues();

            AdjustClassBreaks(limits, newValue, classLevel, limit);

            LayerProvider.ClassRangesDictionary.SetRangesDictionary(limits);

            foreach (KeyValuePair<string, string> kvp in LayerProvider.ClassRangesDictionary.RangeDictionary)
            {
                System.Windows.Controls.TextBox found = (System.Windows.Controls.TextBox)this.FindName(kvp.Key);
                found.Text = kvp.Value;
            }

            PropertyChanged_EnableDisable();
        }

        private void PropertyChanged_EnableDisable(object sender, TextChangedEventArgs e)
        {
            PropertyChanged_EnableDisable();
        }

        private void PropertyChanged_EnableDisable()
        {
            if (btnOK == null) return;

            if (!string.IsNullOrEmpty(txtProjectPath.Text) && (!string.IsNullOrEmpty(txtShapePath.Text) || (!string.IsNullOrEmpty(txtMapSeverpath.Text) || (!string.IsNullOrEmpty(txtKMLpath.Text)))))
            {
                if (tbtnVariables.IsEnabled == false)
                {
                    tbtnVariables.IsEnabled = true;
                    tbtnVariables_Checked(this, new RoutedEventArgs());
                }

                if (cmbShapeKey.SelectedIndex != -1 && cmbDataKey.SelectedIndex != -1 && cmbValue.SelectedIndex != -1)
                {
                    List<SolidColorBrush> brushList = new List<SolidColorBrush>()
                    {
                        (SolidColorBrush) rctColor1.Fill,
                        (SolidColorBrush) rctColor2.Fill,
                        (SolidColorBrush) rctColor3.Fill,
                        (SolidColorBrush) rctColor4.Fill,
                        (SolidColorBrush) rctColor5.Fill,
                        (SolidColorBrush) rctColor6.Fill,
                        (SolidColorBrush) rctColor7.Fill,
                        (SolidColorBrush) rctColor8.Fill,
                        (SolidColorBrush) rctColor9.Fill,
                        (SolidColorBrush) rctColor10.Fill,
                        (SolidColorBrush) rctMissingColor.Fill
                    };

                    int classCount=0;
                    if (cmbClasses.SelectedItem!=null)
                    if (!int.TryParse(((ComboBoxItem)cmbClasses.SelectedItem).Content.ToString(), out classCount))
                    {
                        classCount = 4;
                    }
                    
                    string validationMessage = string.Empty;

                    if (LayerProvider != null)
                    {
                        validationMessage = LayerProvider.PopulateRangeValues(_dashboardHelper,
                            cmbShapeKey.SelectedItem.ToString(),
                            cmbDataKey.SelectedItem.ToString(),
                            cmbValue.SelectedItem.ToString(),
                            brushList,
                            classCount,
                            legTitle.Text,
                            showPolyLabels.IsChecked);
                    }

                    validationText.Text = validationMessage;

                    if (validationText.Text != string.Empty)
                    {
                        tbtnDisplay.IsEnabled = false;
                        if(HasFilter() == false)
                        { 
                            tbtnFilters.IsEnabled = false;
                        }
                        btnOK.IsEnabled = false;
                    }
                    else
                    {
                        if (tbtnDisplay.IsEnabled == false)
                        {
                            tbtnDisplay.IsEnabled = true;
                            tbtnDisplay_Checked(this, new RoutedEventArgs());
                        }

                        if (IsMissingLimitValue() == false)
                        {
                            btnOK.IsEnabled = true;
                        }
                        else
                        {
                            btnOK.IsEnabled = false;
                        }
                        
                        tbtnDisplay.IsEnabled = true;
                        tbtnFilters.IsEnabled = true;
                        tbtnFilters.Visibility = Visibility.Visible;

                        tbtnDisplay.IsChecked = true;
                    }

                    return;
                }
                else
                {
                    tbtnDisplay.IsEnabled = false;
                    if (HasFilter() == false)
                    {
                        tbtnFilters.IsEnabled = false;
                    }
                    btnOK.IsEnabled = false;
                }
            }
            else
            {
                tbtnVariables.IsEnabled = false;
                tbtnDisplay.IsEnabled = false;
                if (HasFilter() == false)
                {
                    tbtnFilters.IsEnabled = false;
                }
                btnOK.IsEnabled = false;
            }
        }
                
        private void AdjustClassBreaks(List<ClassLimits> classBreaks, float newValue, int classLevel, ClassLimitType limitType = ClassLimitType.Start)
        {
            ReplaceClassLimit(classBreaks, newValue, classLevel, limitType);

            if (limitType == ClassLimitType.Start)
            {
                if (classBreaks[classLevel].End <= newValue)
                {
                    limitType = ClassLimitType.End;
                    AdjustClassBreaks(classBreaks, (float)(newValue + 0.01), classLevel, limitType);
                }

                classLevel -= 1;
                if (classLevel < 0) return;

                if (classBreaks[classLevel].End != newValue)
                {
                    limitType = ClassLimitType.End;
                    AdjustClassBreaks(classBreaks, newValue, classLevel, limitType);
                }
            }
            else
            {
                if (classBreaks[classLevel].Start >= newValue)
                {
                    limitType = ClassLimitType.Start;
                    AdjustClassBreaks(classBreaks, (float)(newValue - 0.01), classLevel, limitType);
                }

                classLevel += 1;
                if (classLevel == classBreaks.Count) return;

                if (classBreaks[classLevel].Start != newValue)
                {
                    limitType = ClassLimitType.Start;
                    AdjustClassBreaks(classBreaks, newValue, classLevel, limitType);
                }
            }
        }

        private void ReplaceClassLimit(List<ClassLimits> classBreaks, float newValue, int classLevel, ClassLimitType limitType = ClassLimitType.Start)
        {
            if (limitType == ClassLimitType.Start)
            {
                classBreaks[classLevel].Start = newValue;
            }
            else
            {
                classBreaks[classLevel].End = newValue;
            }
        }

        private bool IsMissingLimitValue()
        {
            foreach (UIElement element in stratGrid.Children)
            {
                if (element is System.Windows.Controls.TextBox)
                {
                    string elementName = ((System.Windows.Controls.TextBox)element).Name;
                    Visibility elementVisibility = ((System.Windows.Controls.TextBox)element).Visibility;
                    if (elementName.StartsWith("ramp") && elementVisibility == Visibility.Visible)
                    {
                        float rangeValue;
                        if(float.TryParse(((System.Windows.Controls.TextBox)element).Text, out rangeValue) == false)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}


