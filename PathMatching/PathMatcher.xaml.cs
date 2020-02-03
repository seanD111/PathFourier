using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.ComponentModel;

namespace PathMatching
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class PathMatcher : Window
    {
        public PathMatcher()
        {
            InitializeComponent();
          
        }

        private void strokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            if (sender == inkIn1){
                int sum = 0;
                 foreach (Stroke stroke in inkIn1.Strokes)
                {
                    sum += stroke.StylusPoints.Count;
                }
                coefficientSlider1.Maximum = sum;
            }
            else if (sender == inkIn2)
            {
                int sum = 0;
                foreach (Stroke stroke in inkIn2.Strokes)
                {
                    sum += stroke.StylusPoints.Count;
                }
                coefficientSlider2.Maximum = sum;
            }     
        }

        private void clearIn1(object sender, RoutedEventArgs e)
        {
            inkIn1.Strokes = new StrokeCollection();
            inkReconstruction1.Strokes = new StrokeCollection();

        }

        private void clearIn2(object sender, RoutedEventArgs e)
        {
            inkIn2.Strokes = new StrokeCollection();
            inkReconstruction2.Strokes = new StrokeCollection();
        }

        private void strokeCollected(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void cSlide1(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            StylusPointCollection allPoints = new StylusPointCollection();

            foreach (Stroke stroke in inkIn1.Strokes)
            {
                allPoints.Add(stroke.StylusPoints);
            }
            
            ComplexStroke cStroke = new ComplexStroke(allPoints);
            

            inkReconstruction1.Strokes = new StrokeCollection();
            inkReconstruction1.Strokes.Add(cStroke.ReconstructedStroke((int)coefficientSlider1.Value));

        }

        private void cSlide2(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            StylusPointCollection allPoints = new StylusPointCollection();

            foreach (Stroke stroke in inkIn2.Strokes)
            {
                allPoints.Add(stroke.StylusPoints);
            }

            ComplexStroke cStroke = new ComplexStroke(allPoints);


            inkReconstruction2.Strokes = new StrokeCollection();
            inkReconstruction2.Strokes.Add(cStroke.ReconstructedStroke((int)coefficientSlider2.Value));

        }
    }


    public class MyData : INotifyPropertyChanged
    {
        private int coefficientCount;
        private int takeMinimum;

        public MyData() { }

        public int TakeMinimum
        {
            get { return takeMinimum; }
            set
            {
                takeMinimum = value;
                OnPropertyChanged("TakeMinimum");
            }
        }

        public int CoefficientCount
        {
            get { return coefficientCount; }
            set
            {
                coefficientCount = value;
                OnPropertyChanged("CoefficientCount");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}
