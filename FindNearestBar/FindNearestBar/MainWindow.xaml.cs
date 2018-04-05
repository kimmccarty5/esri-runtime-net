using Esri.ArcGISRuntime.ArcGISServices;
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
            var map = new Map(BasemapType.NavigationVector, 51.505978, -0.111123, 15);
            MyMapView.Map = map;

            //create bar symbol
            m_pmsBar = new PictureMarkerSymbol(new Uri("http://static.arcgis.com/images/Symbols/PeoplePlaces/Bar.png"));
            m_pmsBar.Width = 25;
            m_pmsBar.Height = 25;


            //create You Are Here symbol
            m_pmsYAH = new PictureMarkerSymbol(new Uri("http://www.free-icons-download.net/images/yellow-star-icon-9290.png"))
            {
                Width = 35,
                Height = 35
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

            int mile = 4;
            //create 1 mile (radius) buffer around point
            var buffer = GeometryEngine.Buffer(point, 1609.34 / mile); 

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


            /* // ===================================================================
               //                       Single Closest Bar
               // -------------------------------------------------------------------

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

            //highlight closest bar
            layer.SelectFeature(feature);
            layer.SelectionColor = Colors.DarkRed;

            //draw line to closest bar
            var linePoints = new PolylineBuilder(SpatialReferences.WebMercator);
            linePoints.AddPoint(location);
            linePoints.AddPoint(feature.Geometry as MapPoint);
            var line = linePoints.ToGeometry();
            
            var lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.DashDotDot, Colors.Maroon, 2);
            var lineGraphic = new Graphic(line, lineSymbol);
            MyOverlay.Graphics.Add(lineGraphic);


            //create text symbol for distance at midpoint of line
            var x = (line.Extent.XMin + line.Extent.XMax) / 2; 
            var y = (line.Extent.YMin + line.Extent.YMax) / 2;
            var textPoint = new MapPoint(x, y);
            var text = String.Format("{0:0.00}", shortDist / 1609.34) + " miles";
            var textSymbol = new TextSymbol(text, Colors.Black, 15, Esri.ArcGISRuntime.Symbology.HorizontalAlignment.Center, Esri.ArcGISRuntime.Symbology.VerticalAlignment.Baseline);
            textSymbol.FontWeight = Esri.ArcGISRuntime.Symbology.FontWeight.Bold;
            //textSymbol.BackgroundColor = Colors.Maroon;
            //textSymbol.Angle = Math.Atan2(line.Extent.YMax - line.Extent.YMin, line.Extent.XMax - line.Extent.XMin) * 180.0 / Math.PI;
            var textGraphic = new Graphic(textPoint, textSymbol);
            MyOverlay.Graphics.Add(textGraphic);

            //===================================================================*/
            


            //==================================================================
            //                    Two Closest Bars
            //------------------------------------------------------------------

            double[] shortDist = { -1, -1 };
            Feature[] feature = new Feature[2];
            System.Diagnostics.Debug.WriteLine("#1: shortdist is at: " + shortDist[0].ToString() + " " + shortDist[1].ToString());

            //check for closest bars within buffer
            foreach (var item in results)
            {
                var calcDistance = GeometryEngine.Distance(location, item.Geometry);
                System.Diagnostics.Debug.WriteLine("feature distance: " + (calcDistance/1609.34).ToString() + " meters; shordist = " + shortDist[0].ToString() + " " + shortDist[1].ToString());
                if (shortDist[0] == -1)
                {
                    shortDist[0] = calcDistance;
                    System.Diagnostics.Debug.WriteLine("setting first");
                    feature[0] = item;
                    continue;
                }
                if(shortDist[1] == -1)
                {
                    System.Diagnostics.Debug.WriteLine("setting second");
                    shortDist[1] = calcDistance;
                    feature[1] = item;
                    continue;
                }

                if (calcDistance < shortDist[0])
                {
                    System.Diagnostics.Debug.WriteLine("less than first");
                    shortDist[1] = shortDist[0];
                    feature[1] = feature[0];
                    shortDist[0] = calcDistance;
                    feature[0] = item;
                }
                else if (calcDistance < shortDist[1])
                {
                    System.Diagnostics.Debug.WriteLine("less than second");
                    shortDist[1] = calcDistance;
                    feature[1] = item;
                }
            }

            //exit if no closest bar
            if (feature[0] == null || feature[1] == null)
                return;

            if (!(GeometryEngine.Intersects(buffer, feature[0].Geometry)))
                return;

            //highlight closest bar
            for (int i = 0; i < 2; i++)
            {
                layer.SelectFeature(feature[i]);
                layer.SelectionColor = Colors.DarkRed;

                //draw line to closest bar
                var linePoints = new PolylineBuilder(SpatialReferences.WebMercator);
                linePoints.AddPoint(location);
                linePoints.AddPoint(feature[i].Geometry as MapPoint);
                var line = linePoints.ToGeometry();

                var lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.DashDotDot, Colors.Maroon, 2);
                var lineGraphic = new Graphic(line, lineSymbol);
                MyOverlay.Graphics.Add(lineGraphic);
                
                //create text symbol for distance at midpoint of line
                var x = (line.Extent.XMin + line.Extent.XMax) / 2;
                var y = (line.Extent.YMin + line.Extent.YMax) / 2;
                var textPoint = new MapPoint(x, y);
                var text = String.Format("{0:0.00}", shortDist[i] / 1609.34) + " miles";
                var textSymbol = new TextSymbol(text, Colors.Black, 15, Esri.ArcGISRuntime.Symbology.HorizontalAlignment.Center, Esri.ArcGISRuntime.Symbology.VerticalAlignment.Baseline);
                textSymbol.FontWeight = Esri.ArcGISRuntime.Symbology.FontWeight.Bold;
                //textSymbol.BackgroundColor = Colors.Maroon;
                //textSymbol.Angle = Math.Atan2(line.Extent.YMax - line.Extent.YMin, line.Extent.XMax - line.Extent.XMin) * 180.0 / Math.PI;
                var textGraphic = new Graphic(textPoint, textSymbol);
                MyOverlay.Graphics.Add(textGraphic);
            }
            

            

            


            


        }
    }
}
