using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace CallOuts
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        SimpleMarkerSymbol _sms;
        private static GraphicsOverlay _overlay;

        public MainWindow()
        {
            string licenseKey = "runtimelite,1000,rud549870135,16-oct-2018,1JPJD4SZ8LLF663PR217";
            Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.SetLicense(licenseKey);
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Create the map
            var map = new Map(BasemapType.NavigationVector, 37.7749, -122.2294, 10);


            // Create a layer
            var cities = new FeatureLayer(new Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/USA/MapServer/0"));

            
            // Cache data from layer to improve performance
            (cities.FeatureTable as ServiceFeatureTable).FeatureRequestMode = FeatureRequestMode.ManualCache;
            (cities.FeatureTable as ServiceFeatureTable).LoadStatusChanged += async (s, ev) =>
            {
                if (ev.Status != Esri.ArcGISRuntime.LoadStatus.Loaded)
                    return;

                var fields = new[] { "*" };
                await (cities.FeatureTable as ServiceFeatureTable).PopulateFromServiceAsync(new QueryParameters { WhereClause = "st='CA'" }, true, fields);
            };
            

            // Create renderer for the layer
            _sms = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Colors.DarkTurquoise, 12);
            _sms.Outline = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Colors.Black, 1);
            cities.Renderer = new SimpleRenderer(_sms);

            // Filter the data to CA
            cities.DefinitionExpression = "st = 'CA'";

            // Add the layer to the map
            map.OperationalLayers.Add(cities);

            // Add an overlay to the view
            _overlay = new GraphicsOverlay();
            _overlay.Renderer = new SimpleRenderer(new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Diamond, Colors.Red, 5));
            MyMapView.GraphicsOverlays.Add(_overlay);


            // Add the map to the view
            MyMapView.Map = map;

            // Wire into the event
            MyMapView.MouseMove += MyMapView_MouseMove; //GeoViewTapped
        }

        private async void MyMapView_MouseMove(object sender, MouseEventArgs e)// GeoViewInputEventArgs e)
        {
            // Get the screen point
            var point = e.GetPosition(MyMapView); //e.Position;

            // Add a graphic to show screen point location
            //var mapPoint = MyMapView.ScreenToLocation(point);
            //MyMapView.GraphicsOverlays[0].Graphics.Add(new Graphic(mapPoint));

            // Use hit test to find feature
            var results = await MyMapView.IdentifyLayerAsync(MyMapView.Map.OperationalLayers[0], point, 0, false);

            // Show feature in callout
            if (results != null && results.GeoElements?.Count > 0)
            {
                var feature = results.GeoElements.FirstOrDefault();
                MyMapView.ShowCalloutForGeoElement(feature, point, new CalloutDefinition("City: " + feature.Attributes["AREANAME"]));

            }
            else
                MyMapView.DismissCallout();

        }


    }
}
