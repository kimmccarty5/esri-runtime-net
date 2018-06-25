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

            //System.Diagnostics.Debug.WriteLine(SpatialReferences.WebMercator.WkText);

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

            //var wkt = "PROJCS[\"WGS_1984_Web_Mercator_Auxiliary\",GEOGCS[\"GCS_WGS_1984\",DATUM[\"D_WGS_1984\",SPHEROID[\"WGS_1984\",6378137.0,298.257223563]],PRIMEM[\"Greenwich\",0.0],UNIT[\"Degree\",0.0174532925199433]],PROJECTION[\"Mercator_Auxiliary_Sphere\"],PARAMETER[\"False_Easting\",0.0],PARAMETER[\"False_Northing\",0.0],PARAMETER[\"Central_Meridian\",0.0],PARAMETER[\"Standard_Parallel_1\",0.0],PARAMETER[\"Auxiliary_Sphere_Type\",0.0],UNIT[\"Meter\",1.0]]";
            //MyMapView1.SpatialReference = new SpatialReference(wkt);

            //var map = new Map(SpatialReference.Create(wkt));
            //map.Basemap = Basemap.CreateImageryWithLabels();

            MyMapView1.Map = new Map(Basemap.CreateImageryWithLabels()) ; // map;
            MyMapView1.WrapAroundMode = WrapAroundMode.EnabledWhenSupported;
            createPolygon(graphicsOverlay1);
            
           

            MyMapView2.Map = new Map(Basemap.CreateTopographic());
            MyMapView2.WrapAroundMode = WrapAroundMode.EnabledWhenSupported;
            createPolygon(graphicsOverlay2);

        }
        public void createPolygon(GraphicsOverlay myGraphicsOverlay)
        {
            var rec = new PolygonBuilder(SpatialReferences.Wgs84);
            var lnSym = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Colors.Red, 2);
            rec.AddPoint(new MapPoint(xMin, yMin));
            rec.AddPoint(new MapPoint(xMin, yMax));
            rec.AddPoint(new MapPoint(xMax, yMax));
            rec.AddPoint(new MapPoint(xMax, yMin));
            myGraphicsOverlay.Graphics.Add(new Graphic(rec.ToGeometry(), lnSym));
            _geom = rec.ToGeometry();
            
        }
        public void ZoomToEnvelope(double xMin, double yMin, double xMax, double yMax, MapView MyMapView)
        {
            var p1 = new PolygonBuilder(SpatialReferences.Wgs84);
            var lnSym1 = new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, Colors.Yellow, 2);
            p1.AddPoint(xMin, yMin);
            p1.AddPoint(xMin, yMax);
            p1.AddPoint(180, yMax);
            p1.AddPoint(180, yMin);
            var rec = new Graphic(p1.ToGeometry(), lnSym1);

            graphicsOverlay2.Graphics.Add(rec);

            Envelope envelope = new Envelope(xMin + 1.6, yMin, 180.0, yMax, SpatialReferences.Wgs84);
            MyMapView.SetViewpointGeometryAsync(envelope);
        }

        public void ZoomToEnvelope(Geometry geometry, MapView MyMapView)
        {
            //MyMapView1.SetViewpointGeometryAsync(new Polygon((geometry as Polygon).Parts.Last().Points, geometry.SpatialReference));

            Envelope envelope = new Envelope(xMin, yMin, xMax, yMax, SpatialReferences.Wgs84);
            MyMapView.SetViewpointGeometryAsync(envelope);

        }

    }
}
