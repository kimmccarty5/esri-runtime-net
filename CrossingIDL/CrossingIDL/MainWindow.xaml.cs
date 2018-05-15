using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System.Windows;
using System.Windows.Media;
using Geometry = Esri.ArcGISRuntime.Geometry.Geometry;
using Esri.ArcGISRuntime.ArcGISServices;


namespace CrossingIDL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    

    public partial class MainWindow : Window
    {
        
        
        
        // Fiji extent
        double xMin = 174.8;
        double yMin = -21.4;
        double xMax = -178.4+360;
        double yMax = -14.7;
        Geometry _geom;

        public MainWindow()
        {
            
            InitializeComponent();

            this.Loaded += MainWindow_Loaded;

            Btn1.Click += Btn1_Click;
            Btn2.Click += Btn2_Click;

        }

        private void Btn2_Click(object sender, RoutedEventArgs e)
        {
            ZoomToEnvelope(xMin, yMin, xMax, yMax, MyMapView2);
        }

        private void Btn1_Click(object sender, RoutedEventArgs e)
        {
            ZoomToEnvelope(_geom, MyMapView1);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            
            
            MyMapView1.Map = new Map(Basemap.CreateTopographic());
            MyMapView1.WrapAroundMode = WrapAroundMode.EnabledWhenSupported;
            createPolygon(graphicsOverlay1);
            
           

            MyMapView2.Map = new Map(Basemap.CreateTerrainWithLabels());
            MyMapView2.WrapAroundMode = WrapAroundMode.EnabledWhenSupported;
            createPolygon(graphicsOverlay2);

        }
        public void createPolygon(GraphicsOverlay myGraphicsOverlay)
        {
            var rec = new PolygonBuilder(SpatialReferences.Wgs84);
            var lnSym = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Colors.Black, 2);
            rec.AddPoint(new MapPoint(xMin, yMin));
            rec.AddPoint(new MapPoint(xMin, yMax));
            rec.AddPoint(new MapPoint(xMax, yMax));
            rec.AddPoint(new MapPoint(xMax, yMin));
            myGraphicsOverlay.Graphics.Add(new Graphic(rec.ToGeometry(), lnSym));
            _geom = rec.ToGeometry();
            
        }
        public void ZoomToEnvelope(double xMin, double yMin, double xMax, double yMax, MapView MyMapView)
        {
            Envelope envelope = new Envelope(xMin+2, yMin, 180, yMax, SpatialReferences.Wgs84);
            MyMapView.SetViewpointGeometryAsync(envelope);

        }

        public void ZoomToEnvelope(Geometry geometry, MapView MyMapView)
        {
            //MyMapView1.SetViewpointGeometryAsync(new Polygon((geometry as Polygon).Parts.Last().Points, geometry.SpatialReference));
            var p1 = new PolygonBuilder(SpatialReferences.Wgs84);
            var lnSym1 = new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, Colors.Red, 2);
            p1.AddPoint(xMin, yMin);
            p1.AddPoint(xMin, yMax);
            p1.AddPoint(180, yMax);
            p1.AddPoint(180, yMin);
            var rec = new Graphic(p1.ToGeometry(), lnSym1);
            
            graphicsOverlay1.Graphics.Add(rec);

            Envelope envelope = new Envelope(xMin+1.6, yMin, 180.0, yMax, SpatialReferences.Wgs84);
            MyMapView.SetViewpointGeometryAsync(envelope);
            
        }

    }
}
