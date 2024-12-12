using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using AKBUtilities;

using ScottPlot;
using ScottPlot.Plottables;
using ScottPlot.WPF;

namespace Projekt1;
public partial class StatsPage : UserControl
{
    private LoadCellMeasurementsDataBase statsDB; 
    Crosshair                            MyCrosshair;
    double[]                             thisyd = new double[12];
    private ScottPlot.Box                box;
    private Timer                        timer;
    public StatsPage()
    {
        DataContext = App.DataModel;
        InitializeComponent();
        statsDB = new ();


        timer = new() {AutoReset = true, Enabled = true, Interval = 100};
        timer.Elapsed += Timer_Elapsed;
        timer.Start();

        MyCrosshair                             = boxcharts.Plot.Add.Crosshair(0, 0);
        MyCrosshair.VerticalLine.Color          = ScottPlot.Color.FromHex("#FFFFFF00");
        MyCrosshair.HorizontalLine.Color        = ScottPlot.Color.FromHex("#FFFFFF00");
        MyCrosshair.VerticalLine.LabelFontColor = ScottPlot.Color.FromHex("#FF6347FF");
        MyCrosshair.VerticalLine.LabelBackgroundColor = ScottPlot.Color.FromHex("#00000050");
        MyCrosshair.IsVisible                   = true;
        MyCrosshair.MarkerShape                 = MarkerShape.OpenCircle;
        MyCrosshair.MarkerSize                  = 2;
        MyCrosshair.VerticalLine.LabelFontSize  = 12;

        boxcharts.MouseMove += (_, e) =>
                               {
                                   try
                                   {
                                       Coordinates mouseLocation =
                                           boxcharts.Plot.GetCoordinates(new(e.GetPosition(this).X
                                                                           , e.GetPosition(this).Y));

                                       var plottables = boxcharts.Plot.GetPlottables<BoxPlot>().ToList();
                                       var boxes      = plottables[0].Boxes;
                                       var x          = (int)Math.Round(mouseLocation.X);

                                       if (!(x < 1 || x > 12))
                                       {
                                           MyCrosshair.VerticalLine.LabelText = @$"
LoadCell:{x}
------------
cpl     :{boxes[x - 1].BoxMin:0.000}
cpu     :{boxes[x - 1].BoxMax:0.000}
max     :{boxes[x - 1].WhiskerMax:0.000}
min     :{boxes[x - 1].WhiskerMin:0.000}
avg     :{boxes[x - 1].BoxMiddle:0.000}
yield   :{thisyd[x-1]:0.000}
";

                                           MyCrosshair.VerticalLine.LabelAlignment = Alignment.UpperLeft;
                                           MyCrosshair.VerticalLine.ManualLabelAlignment = Alignment.UpperLeft;
                                           MyCrosshair.VerticalLine.LabelOffsetY = -((float)mouseLocation.Y * 200);
                                           MyCrosshair.VerticalLine.LabelOffsetX = ((float)mouseLocation.X)-50;

                                       }

                                       Dispatcher.BeginInvoke(() =>
                                                              {
                                                                  MyCrosshair.Position = mouseLocation;
                                                                  boxcharts.Refresh();
                                                              });
                                   } finally {}
                               };
    }

    private void Timer_Elapsed(object sender, ElapsedEventArgs e)
    {
        timer.Stop();
        var rand = new double[12];

        try
        {
            for (var i = 0; i < rand.Length; i++)
            {
                rand[i] = (new Random().NextDouble() +1)* new Random().Next(1,3);
            }

            statsDB.insert_value(rand);
            LoadCellMeasurementsDataBase.mean_stdev_stats[] cpks    = null;
            LoadCellMeasurementsDataBase.iqr_stats[]        iqrStat = null;

            Parallel.Invoke( new Action[]
                             {
                                 ()=>statsDB.get_mean_stdev_analytics(1.2, 0.6, out cpks),
                                 ()=>statsDB.get_iqr_analytics(out iqrStat), 
                                 ()=>statsDB.get_yield_analytics(1.2, 0.6, DateTime.Now - TimeSpan.FromHours(1), DateTime.Now, out thisyd)
                             });
            
            Dispatcher.BeginInvoke(() =>
                                   {
                                       boxcharts.Plot.Remove(typeof(BoxPlot));
                                       boxcharts.Refresh();
                                   });

            Box[] boxes =
            [
                new Box()
              , new Box()
              , new Box()
              , new Box()
              , new Box()
              , new Box()
              , new Box()
              , new Box()
              , new Box()
              , new Box()
              , new Box()
              , new Box()
            ];

            for (var i = 0; i < cpks.Length; i++)
            {
                //boxes[i] = new()
                //           {
                //               Position   = i + 1
                //             , BoxMin     = cpks[i].mean - cpks[i].stddev
                //             , BoxMax     = cpks[i].mean + cpks[i].stddev
                //             , WhiskerMax = cpks[i].max
                //             , WhiskerMin = cpks[i].min
                //             , BoxMiddle  = cpks[i].mean
                //}; 
                boxes[i] = new()
                              {
                                  Position    = i + 1
                                 , BoxMin     = iqrStat[i].q1
                                 , BoxMax     = iqrStat[i].q3
                                 , WhiskerMax = cpks[i].max
                                 , WhiskerMin = cpks[i].min
                                 , BoxMiddle  = iqrStat[i].median
                              };
            }

            Dispatcher.BeginInvoke(() =>
                                   {
                                       BoxPlot bp = boxcharts.Plot.Add.Boxes(boxes);
                                       bp.FillColor          = ScottPlot.Color.FromHex("#006347FF");
                                       bp.Boxes.Find(x=> x.BoxMax == iqrStat.Select(x => x.q3).Max()).FillColor = ScottPlot.Color.FromHex("#FF000FFF");
                                       bp.Boxes.Find(x => x.BoxMin == iqrStat.Select(x => x.q1).Min()).FillColor = ScottPlot.Color.FromHex("#FF000FFF");
                                       boxcharts.Refresh();
                                   });
        } catch (Exception ex) {}
        
        timer.Start();
    }

    private void export_to_pdf(object sender, RoutedEventArgs e)
    {
        ((System.Windows.Controls.Label)sender).IsEnabled = false;
        var pdfview = new PDFReport(boxcharts.Plot);
        pdfview.ShowDialog();
        ((System.Windows.Controls.Label)sender).IsEnabled = true;
    }
}
