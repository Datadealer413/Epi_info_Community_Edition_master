using System;
using System.Collections.Generic;
using System.Data;
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
using ComponentArt.Win.DataVisualization.Charting;
using Epi.Fields;
using EpiDashboard;
using EpiDashboard.Gadgets.Charting;
using System.Collections;

namespace EpiDashboard.Controls.Charting
{
    /// <summary>
    /// Interaction logic for ParetoChart.xaml
    /// </summary>
    public partial class ParetoChart : ParetoChartBase
    {
        //public ColumnChartSettings ColumnChartSettings { get; set; }

        public ParetoChart(DashboardHelper dashboardHelper, ParetoChartParameters parameters, List<XYParetoChartData> dataList)
        {
            InitializeComponent();
            //this.Settings = settings;
            //this.ColumnChartSettings = settings;
            ParetoChartParameters = parameters;
            this.DashboardHelper = dashboardHelper;
            SetChartProperties();
            SetChartData(dataList);
            xyChart.Legend.BorderBrush = Brushes.Gray;
        }

        protected override void SetChartProperties()
        {
            xyChart.AnimationOnLoad = false;

            xyChart.Width = ParetoChartParameters.ChartWidth;
            xyChart.Height = ParetoChartParameters.ChartHeight;

            //xyChart.Palette = ColumnChartParameters.Palette;
            switch (ParetoChartParameters.Palette)
            {
                case 0:
                    xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("Atlantic");
                    break;
                case 1:
                    xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("Breeze");
                    break;
                case 2:
                    xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("ComponentArt");
                    break;
                case 3:
                    xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("Deep");
                    break;
                case 4:
                    xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("Earth");
                    break;
                case 5:
                    xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("Evergreen");
                    break;
                case 6:
                    xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("Heatwave");
                    break;
                case 7:
                    xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("Montreal");
                    break;
                case 8:
                    xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("Pastel");
                    break;
                case 9:
                    xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("Renaissance");
                    break;
                case 10:
                    xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("SharePoint");
                    break;
                case 11:
                    xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("Study");
                    break;
                default:
                case 12:
                    xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("VibrantA");
                    break;
                case 13:
                    xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("VibrantB");
                    break;
                case 14:
                    xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("VibrantC");
                    break;
            }
            if (ParetoChartParameters.PaletteColors.Count() == 12)
            {
                ComponentArt.Win.DataVisualization.Palette CorpColorPalette = new ComponentArt.Win.DataVisualization.Palette();
                CorpColorPalette.PaletteName = "CorpColorPalette";
                /////////////////////////////////////
                CorpColorPalette.ChartingDataPoints12 = new Object();
                var NewPalette12 = (IList)xyChart.Palette.ChartingDataPoints12;
                for (int j = 0; j < ParetoChartParameters.PaletteColors.Count(); j++)
                {
                    NewPalette12[j] = (Color)ColorConverter.ConvertFromString(ParetoChartParameters.PaletteColors[j].ToString());

                }
                CorpColorPalette.ChartingDataPoints12 = (Object)NewPalette12;
                xyChart.Palette.ChartingDataPoints12 = CorpColorPalette.ChartingDataPoints12;
                //////////////////////////////////
                CorpColorPalette.ChartingDataPoints11 = new Object();
                var NewPalette11 = (IList)xyChart.Palette.ChartingDataPoints11;
                for (int j = 0; j < ParetoChartParameters.PaletteColors.Count() - 1; j++)
                {
                    NewPalette11[j] = (Color)ColorConverter.ConvertFromString(ParetoChartParameters.PaletteColors[j].ToString());

                }
                CorpColorPalette.ChartingDataPoints11 = (Object)NewPalette11;
                xyChart.Palette.ChartingDataPoints11 = CorpColorPalette.ChartingDataPoints11;
                //////////////////////////////////
                CorpColorPalette.ChartingDataPoints10 = new Object();
                var NewPalette10 = (IList)xyChart.Palette.ChartingDataPoints10;
                for (int j = 0; j < ParetoChartParameters.PaletteColors.Count() - 2; j++)
                {
                    NewPalette10[j] = (Color)ColorConverter.ConvertFromString(ParetoChartParameters.PaletteColors[j].ToString());

                }
                CorpColorPalette.ChartingDataPoints10 = (Object)NewPalette10;
                xyChart.Palette.ChartingDataPoints10 = CorpColorPalette.ChartingDataPoints10;
                //////////////////////////////////
                CorpColorPalette.ChartingDataPoints9 = new Object();
                var NewPalette9 = (IList)xyChart.Palette.ChartingDataPoints9;
                for (int j = 0; j < ParetoChartParameters.PaletteColors.Count() - 3; j++)
                {
                    NewPalette9[j] = (Color)ColorConverter.ConvertFromString(ParetoChartParameters.PaletteColors[j].ToString());

                }
                CorpColorPalette.ChartingDataPoints9 = (Object)NewPalette9;
                xyChart.Palette.ChartingDataPoints9 = CorpColorPalette.ChartingDataPoints9;
                //////////////////////////////////
                CorpColorPalette.ChartingDataPoints8 = new Object();
                var NewPalette8 = (IList)xyChart.Palette.ChartingDataPoints8;
                for (int j = 0; j < ParetoChartParameters.PaletteColors.Count() - 4; j++)
                {
                    NewPalette8[j] = (Color)ColorConverter.ConvertFromString(ParetoChartParameters.PaletteColors[j].ToString());

                }
                CorpColorPalette.ChartingDataPoints8 = (Object)NewPalette8;
                xyChart.Palette.ChartingDataPoints8 = CorpColorPalette.ChartingDataPoints8;
                //////////////////////////////////
                CorpColorPalette.ChartingDataPoints7 = new Object();
                var NewPalette7 = (IList)xyChart.Palette.ChartingDataPoints7;
                for (int j = 0; j < ParetoChartParameters.PaletteColors.Count() - 5; j++)
                {
                    NewPalette7[j] = (Color)ColorConverter.ConvertFromString(ParetoChartParameters.PaletteColors[j].ToString());

                }
                CorpColorPalette.ChartingDataPoints7 = (Object)NewPalette7;
                xyChart.Palette.ChartingDataPoints7 = CorpColorPalette.ChartingDataPoints7;
                //////////////////////////////////
                CorpColorPalette.ChartingDataPoints6 = new Object();
                var NewPalette6 = (IList)xyChart.Palette.ChartingDataPoints6;
                for (int j = 0; j < ParetoChartParameters.PaletteColors.Count() - 6; j++)
                {
                    NewPalette6[j] = (Color)ColorConverter.ConvertFromString(ParetoChartParameters.PaletteColors[j].ToString());

                }
                CorpColorPalette.ChartingDataPoints6 = (Object)NewPalette6;
                xyChart.Palette.ChartingDataPoints6 = CorpColorPalette.ChartingDataPoints6;
                //////////////////////////////////
                CorpColorPalette.ChartingDataPoints5 = new Object();
                var NewPalette5 = (IList)xyChart.Palette.ChartingDataPoints5;
                for (int j = 0; j < ParetoChartParameters.PaletteColors.Count() - 7; j++)
                {
                    NewPalette5[j] = (Color)ColorConverter.ConvertFromString(ParetoChartParameters.PaletteColors[j].ToString());

                }
                CorpColorPalette.ChartingDataPoints5 = (Object)NewPalette5;
                xyChart.Palette.ChartingDataPoints5 = CorpColorPalette.ChartingDataPoints5;
                //////////////////////////////////
                CorpColorPalette.ChartingDataPoints4 = new Object();
                var NewPalette4 = (IList)xyChart.Palette.ChartingDataPoints4;
                for (int j = 0; j < ParetoChartParameters.PaletteColors.Count() - 8; j++)
                {
                    NewPalette4[j] = (Color)ColorConverter.ConvertFromString(ParetoChartParameters.PaletteColors[j].ToString());

                }
                CorpColorPalette.ChartingDataPoints4 = (Object)NewPalette4;
                xyChart.Palette.ChartingDataPoints4 = CorpColorPalette.ChartingDataPoints4;
                //////////////////////////////////
                CorpColorPalette.ChartingDataPoints3 = new Object();
                var NewPalette3 = (IList)xyChart.Palette.ChartingDataPoints3;
                for (int j = 0; j < ParetoChartParameters.PaletteColors.Count() - 9; j++)
                {
                    NewPalette3[j] = (Color)ColorConverter.ConvertFromString(ParetoChartParameters.PaletteColors[j].ToString());

                }
                CorpColorPalette.ChartingDataPoints3 = (Object)NewPalette3;
                xyChart.Palette.ChartingDataPoints3 = CorpColorPalette.ChartingDataPoints3;
                //////////////////////////////////
                CorpColorPalette.ChartingDataPoints2 = new Object();
                var NewPalette2 = (IList)xyChart.Palette.ChartingDataPoints2;
                for (int j = 0; j < ParetoChartParameters.PaletteColors.Count() - 10; j++)
                {
                    NewPalette2[j] = (Color)ColorConverter.ConvertFromString(ParetoChartParameters.PaletteColors[j].ToString());

                }
                CorpColorPalette.ChartingDataPoints2 = (Object)NewPalette2;
                xyChart.Palette.ChartingDataPoints2 = CorpColorPalette.ChartingDataPoints2;
                //////////////////////////////////
                CorpColorPalette.ChartingDataPoints1 = new Object();
                var NewPalette1 = (IList)xyChart.Palette.ChartingDataPoints1;
                for (int j = 0; j < ParetoChartParameters.PaletteColors.Count() - 11; j++)
                {
                    NewPalette1[j] = (Color)ColorConverter.ConvertFromString(ParetoChartParameters.PaletteColors[j].ToString());

                }
                CorpColorPalette.ChartingDataPoints1 = (Object)NewPalette1;
                xyChart.Palette.ChartingDataPoints1 = CorpColorPalette.ChartingDataPoints1;
            }
            xyChart.DefaultGridLinesVisible = ParetoChartParameters.ShowGridLines;
            xyChart.LegendDock = ParetoChartParameters.LegendDock;

            series0.BarRelativeBegin = double.NaN;
            series0.BarRelativeEnd = double.NaN;

            switch ((BarSpacing)ParetoChartParameters.BarSpace)
            {
                case BarSpacing.None:                
                    series0.RelativePointSpace = 0;
                    series0.BarRelativeBegin = 0;
                    series0.BarRelativeEnd = 0;
                    if (ParetoChartParameters.Composition == CompositionKind.SideBySide) ParetoChartParameters.Composition = CompositionKind.Stacked;
                    break;
                case BarSpacing.Small:
                    series0.RelativePointSpace = 0.1;
                    break;
                case BarSpacing.Medium:
                    series0.RelativePointSpace = 0.4;
                    break;
                case BarSpacing.Large:
                    series0.RelativePointSpace = 0.8;
                    break;
                case BarSpacing.Default:
                default:
                    series0.RelativePointSpace = 0.15;
                    break;
            }


            //if (!string.IsNullOrEmpty(ColumnChartSettings.YAxisFormattingString.Trim()))
            //{
            //    YAxisCoordinates.FormattingString = ColumnChartSettings.YAxisFormattingString;
            //}
            //if (!string.IsNullOrEmpty(ColumnChartSettings.Y2AxisFormattingString.Trim()))
            //{
            //    Y2AxisCoordinates.FormattingString = ColumnChartSettings.Y2AxisFormattingString;
            //}
            //xyChart.CompositionKind = ColumnChartSettings.Composition;
            //xyChart.Orientation = ColumnChartSettings.Orientation;
            //xAxisCoordinates.Angle = Settings.XAxisLabelRotation;

            //if (!string.IsNullOrEmpty(ParetoChartParameters.YAxisFormat.Trim()))
            //{
            //    YAxisCoordinates.FormattingString = ParetoChartParameters.YAxisFormat.Trim();
            //}
            //if (!string.IsNullOrEmpty(ParetoChartParameters.Y2AxisFormat.Trim()))
            //{
            //    Y2AxisCoordinates.FormattingString = ParetoChartParameters.Y2AxisFormat.Trim();
            //}
            xyChart.CompositionKind = ParetoChartParameters.Composition;
            xyChart.Orientation = ParetoChartParameters.Orientation;
            xAxisCoordinates.Angle = ParetoChartParameters.XAxisAngle;

            //switch (Settings.XAxisLabelType)
            //{
            //    case XAxisLabelType.Custom:
            //    case XAxisLabelType.FieldPrompt:
            //        tblockXAxisLabel.Text = Settings.XAxisLabel;
            //        tblockXAxisLabel.Text = Settings.XAxisLabel;        ???  WHY WAS THIS IN THERE TWICE ???
            //        break;
            //    case XAxisLabelType.None:
            //        tblockXAxisLabel.Text = string.Empty;
            //        break;
            //    default:
            //        tblockXAxisLabel.Text = Settings.XAxisLabel;
            //        break;
            //}

            switch (ParetoChartParameters.XAxisLabelType)
            {
                default:
                case 0:  //Automatic
                    if (!String.IsNullOrEmpty(ParetoChartParameters.XAxisLabel))
                    {
                        tblockXAxisLabel.Text = ParetoChartParameters.XAxisLabel;
                    }
                    else
                    {
                        tblockXAxisLabel.Text = ParetoChartParameters.ColumnNames[0];
                    }
                    break;
                case 1:  //Field Prompt
                    {
                        Field field = DashboardHelper.GetAssociatedField(ParetoChartParameters.ColumnNames[0]);
                        if (field != null)
                        {
                            RenderableField rField = field as RenderableField;
                            tblockXAxisLabel.Text = rField.PromptText;
                        }
                        else
                        {
                            tblockXAxisLabel.Text = ParetoChartParameters.ColumnNames[0];
                        }
                    }
                    break;
                case 2:  //None
                    tblockXAxisLabel.Text = string.Empty;
                    break;
                case 3:  //Custom
                    tblockXAxisLabel.Text = ParetoChartParameters.XAxisLabel;
                    break;
            }

            //x axis angle
            double angle = 0;
            //if (double.TryParse(txtXAxisAngle.Text, out angle))
            if (double.TryParse(ParetoChartParameters.XAxisAngle.ToString(), out angle))
            {
                xAxisCoordinates.Angle = angle;
            }

            if (!string.IsNullOrEmpty(tblockXAxisLabel.Text.Trim()))
            {
                tblockXAxisLabel.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                tblockXAxisLabel.Visibility = System.Windows.Visibility.Collapsed;
            }

            //YAxisLabel = Settings.YAxisLabel;
            //Y2AxisLabel = Settings.Y2AxisLabel;
            //Y2AxisLegendTitle = Settings.Y2AxisLegendTitle;

            //xyChart.UseDifferentBarColors = ColumnChartSettings.UseDiffColors;
            //xyChart.LegendVisible = Settings.ShowLegend;
            //xyChart.Legend.FontSize = Settings.LegendFontSize;

            YAxisLabel = ParetoChartParameters.YAxisLabel;
            tblockYAxisLabel.Text = ParetoChartParameters.YAxisLabel;

            tblockYAxisLabel.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            Size textSize = new Size(tblockYAxisLabel.DesiredSize.Width, tblockYAxisLabel.DesiredSize.Height);

            xyChart.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            Size chartSize = new Size(xyChart.DesiredSize.Width, xyChart.DesiredSize.Height);

            tblockYAxisLabel.Padding = new Thickness(((chartSize.Height - 144) / 2) - (textSize.Width / 2), 2, 0, 2);


            Y2AxisLabel = ParetoChartParameters.Y2AxisLabel;
            Y2AxisLegendTitle = ParetoChartParameters.Y2AxisLegendTitle;

            xyChart.UseDifferentBarColors = ParetoChartParameters.UseDiffColors;


            //Size textSize = new Size();
            //Size chartSize = new Size();

            //tblockChartTitle.Text = Settings.ChartTitle;
            //tblockSubTitle.Text = Settings.ChartSubTitle;

            tblockChartTitle.Text = ParetoChartParameters.ChartTitle;
            tblockSubTitle.Text = ParetoChartParameters.ChartSubTitle;
            tblockStrataTitle.Text = ParetoChartParameters.ChartStrataTitle;

            if (string.IsNullOrEmpty(tblockChartTitle.Text)) tblockChartTitle.Visibility = System.Windows.Visibility.Collapsed;
            else tblockChartTitle.Visibility = System.Windows.Visibility.Visible;

            if (string.IsNullOrEmpty(tblockSubTitle.Text)) tblockSubTitle.Visibility = System.Windows.Visibility.Collapsed;
            else tblockSubTitle.Visibility = System.Windows.Visibility.Visible;

            if (string.IsNullOrEmpty(tblockStrataTitle.Text)) tblockStrataTitle.Visibility = System.Windows.Visibility.Collapsed;
            else tblockStrataTitle.Visibility = System.Windows.Visibility.Visible;
            
            //yAxis.UseReferenceValue = Settings.UseRefValues;

            //series0.ShowPointAnnotations = ColumnChartSettings.ShowAnnotations;
            //series0.BarKind = ColumnChartSettings.BarKind;            

            //series1.ShowPointAnnotations = Settings.ShowAnnotationsY2;
            //series1.DashStyle = Settings.LineDashStyleY2;
            //series1.LineKind = Settings.LineKindY2;
            //series1.Thickness = Settings.Y2LineThickness;

            yAxis.UseReferenceValue = ParetoChartParameters.UseRefValues;

            series0.ShowPointAnnotations = ParetoChartParameters.ShowAnnotations;
            series0.BarKind = ParetoChartParameters.BarKind;

            series1.ShowPointAnnotations = ParetoChartParameters.Y2ShowAnnotations;
            series1.DashStyle = ParetoChartParameters.Y2LineDashStyle;
            series1.LineKind = ParetoChartParameters.Y2LineKind;
            series1.Thickness = ParetoChartParameters.Y2LineThickness;

            y2AxisCoordinates.FormattingString = "P0";

            xyChart.LegendVisible = ParetoChartParameters.ShowLegend;
            xyChart.Legend.FontSize = ParetoChartParameters.LegendFontSize;
            xyChart.Legend.BorderBrush = Brushes.Gray;

            if (ParetoChartParameters.ShowLegendBorder == true) 
            {
                xyChart.Legend.BorderThickness = new Thickness(1);
            }
            else 
            {
                xyChart.Legend.BorderThickness = new Thickness(0);
            }

            //EI-98
            YAxisCoordinates.FontSize = ParetoChartParameters.YAxisFontSize;
            xAxisCoordinates.FontSize = ParetoChartParameters.XAxisFontSize;

            tblockXAxisLabel.FontSize = ParetoChartParameters.XAxisLabelFontSize;
            tblockYAxisLabel.FontSize = ParetoChartParameters.YAxisLabelFontSize;
        }

        private void xyChart_DataStructureCreated(object sender, EventArgs e)
        {
            string sName = "";
                NumericCoordinates nc = new NumericCoordinates() { From = 0, To = 1, Step = 0.1 };
                y2AxisCoordinates.Coordinates = nc;

            SetLegendItems();
        }
    }
}
