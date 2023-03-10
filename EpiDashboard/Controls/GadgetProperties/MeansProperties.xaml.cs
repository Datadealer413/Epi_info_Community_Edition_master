using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EpiDashboard;
using Epi;

namespace EpiDashboard.Controls.GadgetProperties
{
    /// <summary>
    /// Interaction logic for MeansProperties.xaml
    /// </summary>
    public partial class MeansProperties : GadgetPropertiesPanelBase
    {
        public MeansProperties(
            DashboardHelper dashboardHelper, 
            IGadget gadget,
            MeansParameters parameters
            )
        {
            InitializeComponent();
            this.DashboardHelper = dashboardHelper;
            this.Gadget = gadget;
            this.Parameters = parameters;

            List<string> fields = new List<string>();
            List<FieldInfo> items = new List<FieldInfo>();
            List<string> crosstabFields = new List<string>();
            List<string> pairIDFields = new List<string>();
            List<string> strataFields = new List<string>();

            crosstabFields.Add(string.Empty);
            pairIDFields.Add(string.Empty);
            items.Add(new FieldInfo()
            {
                Name = "",
                DataType = "",
                VariableCategory = VariableCategory.Field
            });
            //--
            ColumnDataType columnDataType = ColumnDataType.Boolean | ColumnDataType.Numeric | ColumnDataType.UserDefined;
            //--
            foreach (string fieldName in DashboardHelper.GetFieldsAsList())
            {
                items.Add(new FieldInfo()
                {
                    Name = fieldName,
                    DataType = DashboardHelper.GetColumnDbType(fieldName).ToString(),
                    VariableCategory = VariableCategory.Field
                });

                crosstabFields.Add(fieldName);
                pairIDFields.Add(fieldName);
                strataFields.Add(fieldName);
            }
            foreach (string fieldName in DashboardHelper.GetFieldsAsList(columnDataType))
            {
                fields.Add(fieldName);
            }
            //---ei-277
            if (DashboardHelper.IsUsingEpiProject)
            {
                if (fields.Contains("RecStatus")) fields.Remove("RecStatus");
                if (fields.Contains("FKEY")) fields.Remove("FKEY");
                if (fields.Contains("GlobalRecordId")) fields.Remove("GlobalRecordId");
                if (fields.Contains("FirstSaveTime")) fields.Remove("FirstSaveTime");
                if (fields.Contains("LastSaveTime")) fields.Remove("LastSaveTime");

                if (crosstabFields.Contains("RecStatus")) crosstabFields.Remove("RecStatus");
                if (crosstabFields.Contains("FKEY")) crosstabFields.Remove("FKEY");
                if (crosstabFields.Contains("GlobalRecordId")) crosstabFields.Remove("GlobalRecordId");
                if (crosstabFields.Contains("FirstSaveTime")) crosstabFields.Remove("FirstSaveTime");
                if (crosstabFields.Contains("LastSaveTime")) crosstabFields.Remove("LastSaveTime");
                if (crosstabFields.Contains("SYSTEMDATE")) crosstabFields.Remove("SYSTEMDATE");

                if (pairIDFields.Contains("RecStatus")) crosstabFields.Remove("RecStatus");
                if (pairIDFields.Contains("FKEY")) crosstabFields.Remove("FKEY");
                if (pairIDFields.Contains("GlobalRecordId")) crosstabFields.Remove("GlobalRecordId");
                if (pairIDFields.Contains("FirstSaveTime")) crosstabFields.Remove("FirstSaveTime");
                if (pairIDFields.Contains("LastSaveTime")) crosstabFields.Remove("LastSaveTime");
                if (pairIDFields.Contains("SYSTEMDATE")) crosstabFields.Remove("SYSTEMDATE");

                if (strataFields.Contains("RecStatus")) strataFields.Remove("RecStatus");
                if (strataFields.Contains("FKEY")) fields.Remove("FKEY");
                if (strataFields.Contains("GlobalRecordId")) strataFields.Remove("GlobalRecordId");
                if (strataFields.Contains("FirstSaveTime")) strataFields.Remove("FirstSaveTime");
                if (strataFields.Contains("LastSaveTime")) strataFields.Remove("LastSaveTime");
                if (strataFields.Contains("SYSTEMDATE")) strataFields.Remove("SYSTEMDATE");
            }
            //--

            fields.Sort();
            crosstabFields.Sort();
            pairIDFields.Sort();

            cbxField.ItemsSource = fields;
            cbxFieldWeight.ItemsSource = fields;
            cbxFieldCrosstab.ItemsSource = crosstabFields;
            cbxFieldPairID.ItemsSource = pairIDFields;
            lvFieldStrata.ItemsSource = strataFields;

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(cbxField.ItemsSource);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("VariableCategory");
            view.GroupDescriptions.Add(groupDescription);

            RowFilterControl = new RowFilterControl(this.DashboardHelper, Dialogs.FilterDialogMode.ConditionalMode, (gadget as MeansControl).DataFilters, true);
            RowFilterControl.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            panelFilters.Children.Add(RowFilterControl);



            #region Translation

            lblConfigExpandedTitleTxt.Text = DashboardSharedStrings.GADGET_CONFIG_TITLE_MEANS;

            tbtnVariables.Title = DashboardSharedStrings.GADGET_TABBUTTON_VARIABLES;
            tbtnVariables.Description = DashboardSharedStrings.GADGET_TABDESC_MEANS;
            tbtnDisplay.Title = DashboardSharedStrings.GADGET_TABBUTTON_DISPLAY;
            tbtnDisplay.Description = DashboardSharedStrings.GADGET_TABDESC_DISPLAY;

            tblockPanelVariablesTxt.Text = DashboardSharedStrings.GADGET_PANELHEADER_VARIABLES;
            tbtnFilters.Title = DashboardSharedStrings.GADGET_TABBUTTON_FILTERS;
            tblockPanelDisplayTxt.Text = DashboardSharedStrings.GADGET_PANELHEADER_DISPLAY;
            tbtnFilters.Description = DashboardSharedStrings.GADGET_TABDESC_FILTERS;

            tblockWeightTxt.Text = DashboardSharedStrings.GADGET_WEIGHT_VARIABLE;
            tblockTitleNDescSubheaderTxt.Text = DashboardSharedStrings.GADGET_PANELSUBHEADER_TITLENDESC;
            tblockTitleTxt.Text = DashboardSharedStrings.GADGET_GADET_TITLE;
            tblockDescTxt.Text = DashboardSharedStrings.GADGET_DESCRIPTION;
            tblockPrecision.Text = DashboardSharedStrings.GADGET_DECIMALS_TO_DISPLAY;
            tblockPanelDataFilterTxt.Text = DashboardSharedStrings.GADGET_PANELHEADER_DATA_FILTER;
            tblockAnyFilterGadgetOnlyTxt.Text = DashboardSharedStrings.GADGET_FILTER_GADGET_ONLY;
            tblockMeansOfTxt.Text = DashboardSharedStrings.GADGET_MEANS_VARIABLE;
            tblockCrossTabulateByTxt.Text = DashboardSharedStrings.GADGET_CROSSTAB_VARIABLE;
            tblockStratifyByTxt.Text = DashboardSharedStrings.GADGET_STRATA_VARIABLE;

            checkboxShowANOVATxt.Text = DashboardSharedStrings.GADGET_DISPLAY_ANOVA;
            checkboxShowObservationsTxt.Text = DashboardSharedStrings.GADGET_OBSERVATIONS;
            checkboxShowTotalTxt.Text = DashboardSharedStrings.GADGET_TOTAL;
            checkboxShowMeanTxt.Text = DashboardSharedStrings.GADGET_MEANS;
            checkboxShowVarianceTxt.Text = DashboardSharedStrings.GADGET_VARIANCE;
            checkboxShowStdDevTxt.Text = DashboardSharedStrings.GADGET_STANDARD_DEVIATION;
            checkboxShowMinTxt.Text = DashboardSharedStrings.GADGET_MINIMUM;
            checkboxShowQ1Txt.Text = DashboardSharedStrings.GADGET_SHOWQ1;
            checkboxShowMedianTxt.Text = DashboardSharedStrings.GADGET_MEDIAN;
            checkboxShowQ3Txt.Text = DashboardSharedStrings.GADGET_SHOWQ3;
            checkboxShowMaxTxt.Text = DashboardSharedStrings.GADGET_MAXIMUM;
            checkboxShowModeTxt.Text = DashboardSharedStrings.GADGET_MODE;
            tblockOutputColumnsTxt.Text = DashboardSharedStrings.GADGET_OUTPUT_COLUMNS_DISPLAY;
            btnOKTxt.Text = DashboardSharedStrings.BUTTON_OK;
            btnCancelTxt.Text = DashboardSharedStrings.BUTTON_CANCEL;

            #endregion // Translation
        }

        public bool HasSelectedFields
        {
            get
            {
                if (cbxField.SelectedIndex > -1)
                {
                    return true;
                }
                return false;
            }
        }

        private void lbxColumns_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void cbxFieldCrosstab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbxFieldCrosstab.SelectedIndex > 0)
            {
                System.Data.DataView mydv = this.DashboardHelper.DataSet.Tables[0].DefaultView;
                List<object> distinctCrosstabValues = new List<object>();
                for (int i = 0; i < mydv.Count; i++)
                {
                    System.Data.DataRow dr = mydv[i].Row;
                    object ob = dr[cbxFieldCrosstab.SelectedItem + ""];
                    if (!distinctCrosstabValues.Contains(ob))
                        distinctCrosstabValues.Add(ob);
                }
                if (distinctCrosstabValues.Count == 2)
                {
                    cbxFieldPairID.Visibility = Visibility.Visible;
                    tblockPairID.Visibility = Visibility.Visible;
                }
                else
                {
                    cbxFieldPairID.SelectedIndex = 0;
                    cbxFieldPairID.Visibility = Visibility.Collapsed;
                    tblockPairID.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                cbxFieldPairID.SelectedIndex = 0;
                cbxFieldPairID.Visibility = Visibility.Collapsed;
                tblockPairID.Visibility = Visibility.Collapsed;
            }
        }

        public MeansParameters Parameters { get; private set; }

        /// <summary>
        /// Used to generate the list of variables and options for the GadgetParameters object
        /// </summary> 
        protected override void CreateInputVariableList()
        {
            // Set data filters!
            this.DataFilters = RowFilterControl.DataFilters;

            Dictionary<string, string> inputVariableList = new Dictionary<string, string>();
            Parameters.ColumnNames = new List<string>();

            Parameters.SortVariables = new Dictionary<string, SortOrder>();

            Parameters.GadgetTitle = txtTitle.Text;
            Parameters.GadgetDescription = txtDesc.Text;

            List<string> listFields = new List<string>();

            Parameters.ShowCommentLegalLabels = checkboxListLabels.IsChecked.Value;
            Parameters.InputVariableList = inputVariableList;

            if (cbxField.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxField.SelectedItem.ToString()))
            {
                if (Parameters.ColumnNames.Count > 0)
                {
                    Parameters.ColumnNames[0] = cbxField.SelectedItem.ToString();
                }
                else
                {
                    Parameters.ColumnNames.Add(cbxField.SelectedItem.ToString());
                }
            }
            else
            {
                return;
            }

            if (cbxFieldWeight.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxFieldWeight.SelectedItem.ToString()))
            {
                Parameters.WeightVariableName = cbxFieldWeight.SelectedItem.ToString();
            }

            if (lvFieldStrata.SelectedItems.Count > 0)
            {
                Parameters.StrataVariableNames = new List<string>();
                foreach (String s in lvFieldStrata.SelectedItems)
                {
                    if (!string.IsNullOrEmpty(s))
                        Parameters.StrataVariableNames.Add(s);
                }
            }
            else
                Parameters.StrataVariableNames.Clear();

            if (cbxFieldCrosstab.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxFieldCrosstab.SelectedItem.ToString()))
            {
                Parameters.CrosstabVariableName = cbxFieldCrosstab.SelectedItem.ToString();
                Parameters.ColumnNames.Add(Parameters.CrosstabVariableName.ToString());
            }
            else
            {
                Parameters.CrosstabVariableName = String.Empty;
            }

            if (cbxFieldPairID.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxFieldPairID.SelectedItem.ToString()))
            {
                Parameters.PairIDVariableName = cbxFieldPairID.SelectedItem.ToString();
                Parameters.ColumnNames.Add(Parameters.PairIDVariableName.ToString());
            }
            else
            {
                Parameters.PairIDVariableName = String.Empty;
            }

            if (checkboxShowANOVA.IsChecked == true)
            {
                inputVariableList.Add("showanova", "true");
                Parameters.ShowANOVA = true;
            }
            else
            {
                inputVariableList.Add("showanova", "false");
                Parameters.ShowANOVA = false;
            }

            if (cbxFieldPrecision.SelectedIndex >= 0)
            {
                inputVariableList.Add("precision", cbxFieldPrecision.SelectedIndex.ToString());
                Parameters.Precision = cbxFieldPrecision.SelectedIndex.ToString();
            }
            Parameters.columnsToHide.Clear();
            if (!(bool)checkboxShowObservations.IsChecked) Parameters.columnsToHide.Add(1);
            if (!(bool)checkboxShowTotal.IsChecked) Parameters.columnsToHide.Add(2);
            if (!(bool)checkboxShowMean.IsChecked) Parameters.columnsToHide.Add(3);
            if (!(bool)checkboxShowVariance.IsChecked) Parameters.columnsToHide.Add(4);
            if (!(bool)checkboxShowStdDev.IsChecked) Parameters.columnsToHide.Add(5);
            if (!(bool)checkboxShowMin.IsChecked) Parameters.columnsToHide.Add(6);
            if (!(bool)checkboxShowQ1.IsChecked) Parameters.columnsToHide.Add(7);
            if (!(bool)checkboxShowMedian.IsChecked) Parameters.columnsToHide.Add(8);
            if (!(bool)checkboxShowQ3.IsChecked) Parameters.columnsToHide.Add(9);
            if (!(bool)checkboxShowMax.IsChecked) Parameters.columnsToHide.Add(10);
            if (!(bool)checkboxShowMode.IsChecked) Parameters.columnsToHide.Add(11);
            }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Dictionary<string, string> inputVariableList = Parameters.InputVariableList;

            //Just one column for Frequency, .ColumnNames should have only one item
            txtTitle.Text = Parameters.GadgetTitle;
            txtDesc.Text = Parameters.GadgetDescription;
            if (Parameters.ColumnNames.Count > 0)
            {
                cbxField.SelectedItem = Parameters.ColumnNames[0];
            }
            cbxFieldWeight.SelectedItem = Parameters.WeightVariableName;
            cbxFieldCrosstab.SelectedItem = Parameters.CrosstabVariableName;
            cbxFieldPairID.SelectedItem = Parameters.PairIDVariableName;
            checkboxShowANOVA.IsChecked = Parameters.ShowANOVA;
            checkboxListLabels.IsChecked = Parameters.ShowCommentLegalLabels;
            cbxFieldPrecision.SelectedIndex = Convert.ToInt32(Parameters.Precision);
            lvFieldStrata.MaxHeight = lvFieldStrata.MaxHeight + (Math.Max(0, System.Windows.SystemParameters.PrimaryScreenHeight - 768.0));
            scrollViewerDisplay.Height = scrollViewerDisplay.Height + (Math.Max(0, System.Windows.SystemParameters.PrimaryScreenHeight - 768.0));
            if (Parameters.StrataVariableNames.Count > 0)
            {
                foreach (string s in Parameters.StrataVariableNames)
                    lvFieldStrata.SelectedItems.Add(s.ToString());
            }

            checkboxShowObservations.IsChecked = true;
            checkboxShowTotal.IsChecked = true;
            checkboxShowMean.IsChecked = true;
            checkboxShowVariance.IsChecked = true;
            checkboxShowStdDev.IsChecked = true;
            checkboxShowMin.IsChecked = true;
            checkboxShowQ1.IsChecked = true;
            checkboxShowMedian.IsChecked = true;
            checkboxShowQ3.IsChecked = true;
            checkboxShowMax.IsChecked = true;
            checkboxShowMode.IsChecked = true;

            if (Parameters.columnsToHide.Count > 0)
            {
                foreach (int x in Parameters.columnsToHide)
                {
                    switch (x)
                    {
                        case 1:
                            checkboxShowObservations.IsChecked = false;
                            break;
                        case 2:
                            checkboxShowTotal.IsChecked = false;
                            break;
                        case 3:
                            checkboxShowMean.IsChecked = false;
                            break;
                        case 4:
                            checkboxShowVariance.IsChecked = false;
                            break;
                        case 5:
                            checkboxShowStdDev.IsChecked = false;
                            break;
                        case 6:
                            checkboxShowMin.IsChecked = false;
                            break;
                        case 7:
                            checkboxShowQ1.IsChecked = false;
                            break;
                        case 8:
                            checkboxShowMedian.IsChecked = false;
                            break;
                        case 9:
                            checkboxShowQ3.IsChecked = false;
                            break;
                        case 10:
                            checkboxShowMax.IsChecked = false;
                            break;
                        case 11:
                            checkboxShowMode.IsChecked = false;
                            break;
                    }
                }
            }
        }

        public class FieldInfo { public string Name { get; set; } public string DataType { get; set; } public VariableCategory VariableCategory { get; set; } }

        private void tbtnVariables_Checked(object sender, RoutedEventArgs e)
        {
            if (panelVariables == null) return;

            CheckButtonStates(sender as SettingsToggleButton);
            panelVariables.Visibility = System.Windows.Visibility.Visible;
            panelSorting.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplay.Visibility = System.Windows.Visibility.Collapsed;
            panelFilters.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tbtnSorting_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as SettingsToggleButton);
            panelVariables.Visibility = System.Windows.Visibility.Collapsed;
            panelSorting.Visibility = System.Windows.Visibility.Visible;
            panelDisplay.Visibility = System.Windows.Visibility.Collapsed;
            panelFilters.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tbtnDisplay_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as SettingsToggleButton);
            panelVariables.Visibility = System.Windows.Visibility.Collapsed;
            panelSorting.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplay.Visibility = System.Windows.Visibility.Visible;
            panelFilters.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tbtnFilters_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as SettingsToggleButton);
            panelVariables.Visibility = System.Windows.Visibility.Collapsed;
            panelSorting.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplay.Visibility = System.Windows.Visibility.Collapsed;
            panelFilters.Visibility = System.Windows.Visibility.Visible;
        }

        private void lbxAvailableVariables_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //if (lbxAvailableVariables.SelectedItems.Count == 1)
            //{
            //    string fieldName = lbxAvailableVariables.SelectedItem.ToString();

            //    lbxAvailableVariables.Items.Remove(fieldName);

            //    string method = " (ascending)";
            //    if (cmbSortMethod.SelectedIndex == 1)
            //    {
            //        method = " (descending)";
            //    }
            //    lbxSortOrder.Items.Add(fieldName + method);
            //}
        }

        private void lbxSortOrder_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //if (lbxSortOrder.SelectedItems.Count == 1)
            //{
            //    string fieldName = lbxSortOrder.SelectedItem.ToString();

            //    lbxSortOrder.Items.Remove(fieldName);

            //    string originalFieldName = fieldName.Replace(" (ascending)", String.Empty).Replace(" (descending)", String.Empty);

            //    lbxAvailableVariables.Items.Add(originalFieldName);

            //    List<string> items = new List<string>();

            //    foreach (string item in lbxAvailableVariables.Items)
            //    {
            //        items.Add(item);
            //    }

            //    items.Sort();

            //    lbxAvailableVariables.Items.Clear();

            //    foreach (string item in items)
            //    {
            //        lbxAvailableVariables.Items.Add(item);
            //    }
            //}
        }
    }
}
