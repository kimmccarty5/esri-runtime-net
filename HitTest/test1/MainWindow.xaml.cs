using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.UI.Controls;
using System.Diagnostics;

namespace test1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Boolean _allLayers;
        private double _pixelBuffer;

        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += MainWindow_Loaded;

            BtnAllLayers.Click += BtnAllLayers_Click;
            BtnFeatLayers.Click += BtnFeatLayers_Click;

            MyMapView.GeoViewTapped += MyMapView_GeoViewTapped;
        }

        private void BtnFeatLayers_Click(object sender, RoutedEventArgs e)
        {
            _allLayers = false;
        }

        private void BtnAllLayers_Click(object sender, RoutedEventArgs e)
        {
            _allLayers = true;

            
        }

        private async void MyMapView_GeoViewTapped(object sender, Esri.ArcGISRuntime.UI.Controls.GeoViewInputEventArgs e)
        {
            var point = e.Position;
            var watch = new Stopwatch();

            if (_allLayers == false)
            {
                //IReadOnlyList<IdentifyLayerResult> idLayersResult = await MyMapView.IdentifyLayersAsync(point, 20, false, 5);
                IReadOnlyList<Layer> layers = MyMapView.Map.AllLayers;
                
                watch.Start();
                
                int count = 0;
                foreach (var layer in layers)
                {
                    System.Threading.Thread.Sleep(500);
                    System.Diagnostics.Debug.WriteLine(layer.Name);

                    if (layer is FeatureLayer)
                    {
                        System.Diagnostics.Debug.WriteLine("Feature layer: " + layer.Name);

                        //Determine the feature layer that the tapPoint was on when clicked
                        var idLayerResults = await MyMapView.IdentifyLayerAsync(layer, point, 0, false);

                    }
                    count++;
                    
                }
                watch.Stop();
                MessageBox.Show("It took " + watch.Elapsed.Seconds.ToString("N2") + " seconds to go through " + count.ToString() + " layers.");
            }
            else
            {
                watch.Start();
                System.Threading.Thread.Sleep(1500);
                var idLayerResults = await MyMapView.IdentifyLayersAsync(point, 0, false);
                var x = idLayerResults.ToList();
                watch.Stop();
                MessageBox.Show("It took " + watch.Elapsed.Seconds.ToString() + " seconds to go through all layers.");

            }
                
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var map = new Map(BasemapType.DarkGrayCanvasVector, 39.8283, -98.5795, 5);
           
            //add map image layer
            var mapImageUri = new Uri("http://sampleserver5.arcgisonline.com/arcgis/rest/services/Elevation/WorldElevations/MapServer");
            ArcGISMapImageLayer imageLayer = new ArcGISMapImageLayer(mapImageUri);
            map.OperationalLayers.Add(imageLayer);

            //add map tiled layer
            var tileUri = new Uri("http://services.arcgisonline.com/arcgis/rest/services/World_Topo_Map/MapServer");
            ArcGISTiledLayer tiledLayer = new ArcGISTiledLayer(tileUri);
            map.OperationalLayers.Add(tiledLayer);

            //add vector tile layer
            var vectorUri = new Uri("http://www.arcgis.com/home/item.html?id=75f4dfdff19e445395653121a95a85db");
            ArcGISVectorTiledLayer vector = new ArcGISVectorTiledLayer(vectorUri);
            map.OperationalLayers.Add(vector);

            //add raster layer
            var rasterUri = new Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/NLCDLandCover2001/ImageServer");
            ImageServiceRaster rasterService = new ImageServiceRaster(rasterUri);// Create new image service raster from the Uri
            await rasterService.LoadAsync();// Load the image service raster
            RasterLayer raster = new RasterLayer(rasterService);// Create a new raster layer from the image service raster
            map.OperationalLayers.Add(raster);// Add the raster layer to the maps layer collection

            //Add feature layer us cities
            var featUri = new Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/USA/MapServer/0");
            FeatureLayer usCities = new FeatureLayer(featUri);
            map.OperationalLayers.Add(usCities);

            //add feature layer world cities
            var featUri1 = new Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/SampleWorldCities/MapServer/0");
            FeatureLayer worldCities = new FeatureLayer(featUri1);
            map.OperationalLayers.Add(worldCities);
            
            MyMapView.Map = map;




        }
    }
}
