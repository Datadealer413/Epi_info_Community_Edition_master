using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Media;
using Epi;



namespace EpiDashboard.Mapping
{

    public class ChoroplethLayerPropertiesUserControlBase : UserControl
    {
        public ESRI.ArcGIS.Client.Map myMap;
        public DashboardHelper dashboardHelper;
        public string boundryFilePath;
        public IMapControl mapControl;
        public bool partitionSetUsingQuantiles;

        private Dictionary<int, object> _classAttribList;
        public Dictionary<int, object> ClassAttributeList
        {
            get { return _classAttribList; }
            set { _classAttribList = value; }
        }

        public bool BoundryFilePathExists(string path)
        {
            if (System.IO.File.Exists(path))
            {
                return true;
            }
            else
            {
                Epi.Windows.MsgBox.ShowInformation("The boundry file does not exist with the given path.");
                return false;
            }
        }



        public void CreateFromXml(System.Xml.XmlElement element, ChoroplethLayerProvider provider)
        {
            foreach (System.Xml.XmlElement child in element.ChildNodes)
            {
                if (child.Name.Equals("classTitles"))
                {
                    foreach (System.Xml.XmlElement classTitle in child)
                    {
                        provider.ListLegendText.Add(classTitle.Name, classTitle.InnerText);
                    }
                }

                if (child.Name.Equals("partitionUsingQuantiles"))
                {
                    bool asQuintiles = false;
                    bool.TryParse(child.InnerText, out asQuintiles);
                    provider.UseQuantiles = asQuintiles;
                    partitionSetUsingQuantiles = asQuintiles;
                }

                if (child.Name.Equals("classes"))
                {
                    int legacyClassCountIndex = 2;
                    int.TryParse(child.InnerText, out legacyClassCountIndex);
                    provider._classCount = legacyClassCountIndex + 3;
                }

                if (child.Name.Equals("resolution"))
                {
                    string resolution = child.InnerText;
                    provider._resolution = double.Parse(resolution);
                }

                if (child.Name.Equals("centerPoint_X"))
                {
                    string centerPoint_X = child.InnerText;
                    provider._centerPoint_X = double.Parse(centerPoint_X);
                }

                if (child.Name.Equals("centerPoint_Y"))
                {
                    string centerPoint_Y = child.InnerText;
                    provider._centerPoint_Y = double.Parse(centerPoint_Y);
                }

                if (child.Name.Equals("spatialRef"))
                {
                    string spatialRef = child.InnerText;
                    provider._spatialRef = int.Parse(spatialRef);
                }

                if (child.Name.Equals("envMinX"))
                {
                    string envMinX= child.InnerText;
                    provider._envMinX = double.Parse(envMinX);
                    // Check to see if map7 file has new extent info, if not--it is an old map
                    bool newMapFile = true;
                    provider.NewMap7File = newMapFile;
                }

                if (child.Name.Equals("envMinY"))
                {
                    string envMinY = child.InnerText;
                    provider._envMinY = double.Parse(envMinY);
                }

                if (child.Name.Equals("envMaxX"))
                {
                    string envMaxX = child.InnerText;
                    provider._envMaxX = double.Parse(envMaxX);
                }

                if (child.Name.Equals("envMaxY"))
                {
                    string envMaxY = child.InnerText;
                    provider._envMaxY = double.Parse(envMaxY);
                }

                if (child.Name.Equals("opacity"))
                {
                    byte opacity = 115;
                    byte.TryParse(child.InnerText, out opacity);
                    provider.Opacity = opacity;
                }

                if (child.Name.Equals("customColors"))
                {
                    provider.UseCustomColors = true;

                    foreach (System.Xml.XmlElement color in child)
                    {
                        string rgb = color.InnerText;
                        string[] co = rgb.Split(',');
                        Color newColor = Color.FromRgb(byte.Parse(co[0]), byte.Parse(co[1]), byte.Parse(co[2]));
                        provider.CustomColorsDictionary.Add(color.Name, newColor);
                    }
                }

                if (child.Name.Equals("classRanges"))
                {
                    provider.ClassRangesDictionary.RangeDictionary = new Dictionary<string, string>();

                    foreach (System.Xml.XmlElement classRangElement in child)
                    {
                        provider.ClassRangesDictionary.Add(classRangElement.Name, classRangElement.InnerText);
                    }

                    List<double> rangeStartsFromMapFile = new List<double>();

                    try
                    {
                        AddRangeStarts(provider, rangeStartsFromMapFile, "rampStart01");
                        AddRangeStarts(provider, rangeStartsFromMapFile, "rampStart02");
                        AddRangeStarts(provider, rangeStartsFromMapFile, "rampStart03");
                        AddRangeStarts(provider, rangeStartsFromMapFile, "rampStart04");
                        AddRangeStarts(provider, rangeStartsFromMapFile, "rampStart05");
                        AddRangeStarts(provider, rangeStartsFromMapFile, "rampStart06");
                        AddRangeStarts(provider, rangeStartsFromMapFile, "rampStart07");
                        AddRangeStarts(provider, rangeStartsFromMapFile, "rampStart08");
                        AddRangeStarts(provider, rangeStartsFromMapFile, "rampStart09");
                        AddRangeStarts(provider, rangeStartsFromMapFile, "rampStart10");
                    }
                    catch { }

                    provider.RangeStartsFromMapFile = rangeStartsFromMapFile;
                    provider.RangesLoadedFromMapFile = true;
                }

                if (child.Name.Equals("legTitle"))
                {
                    provider.LegendText = child.InnerText;
                }

                if (child.Name.Equals("showPolyLabels"))
                {
                    bool show = false;
                    bool.TryParse(child.InnerText, out show);
                    provider.ShowPolyLabels = show;
                }
            }
        }

        private static void AddRangeStarts(ChoroplethLayerProvider provider, List<double> rangeStartsFromMapFile, string rampKey)
        {
            if (provider.ClassRangesDictionary.RangeDictionary.ContainsKey(rampKey))
            {
                string double_String = provider.ClassRangesDictionary.RangeDictionary[rampKey];
                if (string.IsNullOrEmpty(double_String) == false)
                {
                    rangeStartsFromMapFile.Add(Convert.ToDouble(double_String));
                }
            }
        }

        public static XmlNode Serialize(System.Xml.XmlDocument doc, ChoroplethLayerProvider provider, string dataKey, string shapeKey, string value, string legacyClassCountIndex, SolidColorBrush highColor, SolidColorBrush lowColor, SolidColorBrush missingColor, string uniqueXmlString, DashboardHelper dashboardHelper, XmlAttribute type, double opacity)
        {
            Epi.ApplicationIdentity appId = new Epi.ApplicationIdentity(typeof(Configuration).Assembly);

            try
            {   
                string classTitles = "<classTitles>" + Environment.NewLine;
                long classTitleCount = 0;
                string classTitleTagName = "";

                foreach (KeyValuePair<string, string> entry in provider.ListLegendText.Dict)
                {
                    classTitleTagName = entry.Key;
                    classTitles += string.Format("<{1}>{0}</{1}>", entry.Value, classTitleTagName) + Environment.NewLine;
                    classTitleCount++;
                }

                classTitles += "</classTitles>" + Environment.NewLine;

                string customColors = "";

                if (provider.CustomColorsDictionary != null)
                {
                    customColors = "<customColors>" + Environment.NewLine;

                    foreach (KeyValuePair<string, Color> keyValuePair in provider.CustomColorsDictionary.Dict)
                    {
                        string customColor = "<" + keyValuePair.Key + ">";
                        string color = keyValuePair.Value.R + "," + keyValuePair.Value.G + "," + keyValuePair.Value.B;
                        customColor += color + "</" + keyValuePair.Key + ">" + Environment.NewLine;
                        customColors += customColor;
                    }

                    customColors += "</customColors>" + Environment.NewLine;
                }

                string useCustomColorsTag = "<useCustomColors>" + provider.UseCustomColors + "</useCustomColors>" + Environment.NewLine;
                string asQuintileTag = "<partitionUsingQuantiles>" + provider.UseQuantiles + "</partitionUsingQuantiles>" + Environment.NewLine;
                string classRanges = "";


                string resolution = "<resolution>" + provider.ArcGIS_Map.Resolution.ToString() + "</resolution>" + Environment.NewLine;
                string centerPoint_X = "<centerPoint_X>" + provider.ArcGIS_Map.Extent.GetCenter().X.ToString() + "</centerPoint_X>" + Environment.NewLine;
                string centerPoint_Y = "<centerPoint_Y>" + provider.ArcGIS_Map.Extent.GetCenter().Y.ToString() + "</centerPoint_Y>" + Environment.NewLine;
                string spatialRef = "<spatialRef>" + provider.ArcGIS_Map.Extent.SpatialReference.WKID.ToString() + "</spatialRef>" + Environment.NewLine;
                //string spatialRef = "<spatialRef>" + provider.ArcGIS_Map.SpatialReference.WKID.ToString() + "</spatialRef>" + Environment.NewLine;
                string envMinX = "<envMinX>" + provider.ArcGIS_Map.Extent.XMin + "</envMinX>" + Environment.NewLine;
                string envMinY = "<envMinY>" + provider.ArcGIS_Map.Extent.YMin + "</envMinY>" + Environment.NewLine;
                string envMaxX = "<envMaxX>" + provider.ArcGIS_Map.Extent.XMax + "</envMaxX>" + Environment.NewLine;
                string envMaxY = "<envMaxY>" + provider.ArcGIS_Map.Extent.YMax + "</envMaxY>" + Environment.NewLine;

                if (provider.UseQuantiles == false)
                {
                    if (provider.ClassRangesDictionary != null)
                    {
                        classRanges = "<classRanges>" + Environment.NewLine;

                        foreach (KeyValuePair<string, string> keyValuePair in provider.ClassRangesDictionary.RangeDictionary)
                        {
                            string rangeName = "<" + keyValuePair.Key + ">";
                            string rangeValue = keyValuePair.Value;
                            rangeName += rangeValue + "</" + keyValuePair.Key + ">" + Environment.NewLine;
                            classRanges += rangeName;
                        }

                        classRanges += "</classRanges>" + Environment.NewLine;
                    }
                }

                string xmlString = uniqueXmlString +
                                   "<highColor>" + highColor.Color.ToString() + "</highColor>" + Environment.NewLine +
                                   "<legTitle>" + provider.LegendText + "</legTitle>" + Environment.NewLine +
                                   "<showPolyLabels>" + provider.ShowPolyLabels + "</showPolyLabels>" + Environment.NewLine +
                                   classTitles +
                                   classRanges +
                                   useCustomColorsTag +
                                   resolution +
                                   centerPoint_X +
                                   centerPoint_Y +
                                   spatialRef +
                                   envMinX +
                                   envMinY +
                                   envMaxX +
                                   envMaxY +
                                   asQuintileTag +
                                   customColors +
                                   "<lowColor>" + lowColor.Color.ToString() + "</lowColor>" + Environment.NewLine +
                                   "<missingColor>" + missingColor.Color.ToString() + "</missingColor>" + Environment.NewLine +
                                   "<classes>" + legacyClassCountIndex + "</classes>" + Environment.NewLine +
                                   "<dataKey>" + dataKey + "</dataKey>" + Environment.NewLine +
                                   "<shapeKey>" + shapeKey + "</shapeKey>" + Environment.NewLine +
                                   "<value>" + value + "</value>" + Environment.NewLine +
                                   "<opacity>" + opacity + "</opacity>" + Environment.NewLine +
                                   "<versionTag>" + appId.Version.ToString() + "</versionTag>" + Environment.NewLine;

                doc.PreserveWhitespace = true;

                XmlElement element = doc.CreateElement("dataLayer");
                element.InnerXml = xmlString;
                element.AppendChild(dashboardHelper.Serialize(doc));

                element.Attributes.Append(type);

                return element;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        protected void ChoroplethProperties_RenderMap(ChoroplethLayerProvider provider)
        {
            dynamic layerPropertiesControl = this;

            EpiDashboard.Controls.ChoroplethProperties choroplethprop = new Controls.ChoroplethProperties(mapControl as StandaloneMapControl, myMap);

            if (provider is ChoroplethShapeLayerProvider)
            {
                choroplethprop.txtShapePath.Text = boundryFilePath;
            }
            else if (provider is ChoroplethKmlLayerProvider)
            {
                choroplethprop.txtKMLpath.Text = boundryFilePath;
            }
            else if (provider is ChoroplethServerLayerProvider)
            {
                choroplethprop.txtMapSeverpath.Text = boundryFilePath;
            }
            choroplethprop.LayerProvider = provider;
            choroplethprop.txtProjectPath.Text = dashboardHelper.Database.DataSource;
            choroplethprop.SetDashboardHelper(dashboardHelper);
            choroplethprop.cmbClasses.Text = layerPropertiesControl.cbxClasses.Text;

            foreach (string str in layerPropertiesControl.cbxShapeKey.Items)
            {
                choroplethprop.cmbShapeKey.Items.Add(str);
            }

            foreach (string str in layerPropertiesControl.cbxDataKey.Items)
            {
                choroplethprop.cmbDataKey.Items.Add(str);
            }

            foreach (string str in layerPropertiesControl.cbxValue.Items)
            {
                choroplethprop.cmbValue.Items.Add(str);
            }

            choroplethprop.cmbShapeKey.SelectedItem = layerPropertiesControl.cbxShapeKey.Text;
            choroplethprop.cmbDataKey.SelectedItem = layerPropertiesControl.cbxDataKey.Text;

            choroplethprop.cmbValue.SelectionChanged -= new System.Windows.Controls.SelectionChangedEventHandler(choroplethprop.cmbValue_SelectionChanged);
            choroplethprop.cmbValue.SelectedItem = layerPropertiesControl.cbxValue.Text;
            choroplethprop.cmbValue.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(choroplethprop.cmbValue_SelectionChanged);

            choroplethprop.rctHighColor.Fill = layerPropertiesControl.rctHighColor.Fill;
            choroplethprop.rctLowColor.Fill = layerPropertiesControl.rctLowColor.Fill;
            choroplethprop.rctMissingColor.Fill = layerPropertiesControl.rctMissingColor.Fill;

            if (provider is ChoroplethShapeLayerProvider)
            {
                choroplethprop.radShapeFile.IsChecked = true;
                choroplethprop.choroplethShapeLayerProvider = (ChoroplethShapeLayerProvider)provider;
            }
            else if (provider is ChoroplethKmlLayerProvider)
            {
                choroplethprop.radKML.IsChecked = true;
                choroplethprop.choroplethKmlLayerProvider = (ChoroplethKmlLayerProvider)provider;
            }
            else if (provider is ChoroplethServerLayerProvider)
            {
                choroplethprop.radMapServer.IsChecked = true;
                choroplethprop.choroplethServerLayerProvider = (ChoroplethServerLayerProvider)provider;
            }

            choroplethprop.legTitle.Text = provider.LegendText;
            choroplethprop.showPolyLabels.IsChecked = provider.ShowPolyLabels;
            choroplethprop.ListLegendText = provider.ListLegendText;

            choroplethprop.SetProperties();

            if (provider._resolution != 0)

            choroplethprop.RenderMap();

                {
                if (provider._mapIsNew)
                {
                        // Define extent from map7 file
                        ESRI.ArcGIS.Client.Geometry.Envelope myExtent = new ESRI.ArcGIS.Client.Geometry.Envelope(provider._envMinX, provider._envMinY, provider._envMaxX, provider._envMaxY);

                        // Zoom to location
                        provider.ArcGIS_Map.ZoomTo(myExtent);
                 }
            }
        }

    }
}
