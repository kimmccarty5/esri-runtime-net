using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using System;
using System.Windows;
using System.Windows.Media;
using Geometry = Esri.ArcGISRuntime.Geometry.Geometry;


namespace FindNearestBar
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        PictureMarkerSymbol m_pmsBar { get; set; }
        PictureMarkerSymbol m_pmsYAH { get; set; }


        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //Create map and add to view
            var map = new Map(BasemapType.NavigationVector, 51.5080, -0.1281, 14);
            MyMapView.Map = map;

            //create bar symbol
            m_pmsBar = new PictureMarkerSymbol(new Uri("http://static.arcgis.com/images/Symbols/PeoplePlaces/Bar.png"));
            m_pmsBar.Width = 25;
            m_pmsBar.Height = 25;


            //create You Are Here symbol
            m_pmsYAH = new PictureMarkerSymbol(new Uri("http://static.arcgis.com/images/Symbols/Animated/EnlargeRotatingBlueMarkerSymbol.png"))
            {
                Width = 25,
                Height = 25
            };

            //add bars as feature layer and render symbols
            var bars = new FeatureLayer(new Uri("http://hedwig.esri.com:6080/arcgis/rest/services/LondonBars1/MapServer/0"));
            bars.Renderer = new SimpleRenderer(m_pmsBar);
            MyMapView.Map.OperationalLayers.Add(bars);
            
            
            //wiring into the click event
            MyMapView.GeoViewTapped += MyMapView_GeoViewTapped;
        }

        //create point at click
        private void MyMapView_GeoViewTapped(object sender, Esri.ArcGISRuntime.UI.Controls.GeoViewInputEventArgs e)
        {
            MyOverlay.Graphics.Clear();
            //get clicked point
            var point = e.Location;
            var graphic = new Graphic(point, m_pmsYAH);

            //create 1 mile (radius) buffer around point
            var buffer = GeometryEngine.Buffer(point, 1609.34 / 4); 

            //create symbol for buffer
            var buffSym = new SimpleLineSymbol();
            buffSym.Color = Colors.Purple;
            buffSym.Style = SimpleLineSymbolStyle.Dash;
            buffSym.Width = 2;
            //create buffer graphic
            var buffer_graphic = new Graphic(buffer, buffSym);

            //add graphics to map
            MyOverlay.Graphics.Add(graphic);
            MyOverlay.Graphics.Add(buffer_graphic);

            //query nearest bars
            QueryFeatures(buffer, point);


        }

        private async void QueryFeatures(Geometry buffer, MapPoint location)
        {
            //create query
            var query = new QueryParameters();
            query.Geometry = buffer;
            query.MaxAllowableOffset = 0;
            System.Diagnostics.Debug.WriteLine(buffer.ToJson());

            var layer = MyMapView.Map.OperationalLayers[0] as FeatureLayer;
            var table = layer.FeatureTable;
            var results = await table.QueryFeaturesAsync(query);
            layer.ClearSelection();

            double shortDist = -1;
            Feature feature = null;

            //check for closest bars within buffer
            foreach (var item in results)
            {
                var calcDistance = GeometryEngine.Distance(location, item.Geometry);

                if (shortDist == -1)
                {
                    shortDist = calcDistance;
                    feature = item;
                    continue;
                }

                if (calcDistance < shortDist)
                {
                    shortDist = calcDistance;
                    feature = item;
                }
            }

            //exit if no closest bar
            if (feature == null)
                return;

            if (!(GeometryEngine.Intersects(buffer, feature.Geometry)))
                return;

            
            layer.SelectFeature(feature);
            layer.SelectionColor = Colors.DarkRed;


            var linePoints = new PolylineBuilder(SpatialReferences.WebMercator);
            linePoints.AddPoint(location);
            linePoints.AddPoint(feature.Geometry as MapPoint);
            var line = linePoints.ToGeometry();

            var x = (line.Extent.XMin + line.Extent.XMax) / 2;
            var y = (line.Extent.YMin + line.Extent.YMax) / 2;

            var lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Colors.Maroon, 2);
            var lineGraphic = new Graphic(line, lineSymbol);
            MyOverlay.Graphics.Add(lineGraphic);

           // double walk = decimal.Round(shortDist / 1609.34, 2);

            var textPoint = new MapPoint(x, y);
            var textSymbol = new TextSymbol(String.Format("{0:0.00}", shortDist / 1609.34) + " miles", Colors.Black, 15, Esri.ArcGISRuntime.Symbology.HorizontalAlignment.Center, Esri.ArcGISRuntime.Symbology.VerticalAlignment.Baseline);
            textSymbol.FontWeight = Esri.ArcGISRuntime.Symbology.FontWeight.Bold;
            textSymbol.BackgroundColor = Colors.Maroon;
            //textSymbol.Angle = Math.Atan2(line.Extent.YMax - line.Extent.YMin, line.Extent.XMax - line.Extent.XMin) * 180.0 / Math.PI;
            var textGraphic = new Graphic(textPoint, textSymbol);
            MyOverlay.Graphics.Add(textGraphic);
        }
    }
}
