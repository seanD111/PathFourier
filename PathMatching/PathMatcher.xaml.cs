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
using System.Numerics;

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
            inkReconstruction1.Children.Clear();
            inkReconstruction1.Strokes = new StrokeCollection();

        }

        private void clearIn2(object sender, RoutedEventArgs e)
        {
            inkIn2.Strokes = new StrokeCollection();
            inkReconstruction2.Children.Clear();
            inkReconstruction2.Strokes = new StrokeCollection();
        }

        private void strokeCollected(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        //private void cSlide1(object sender, RoutedPropertyChangedEventArgs<double> e)
        //{
        //    StylusPointCollection allPoints = new StylusPointCollection();

        //    foreach (Stroke stroke in inkIn1.Strokes)
        //    {
        //        allPoints.Add(stroke.StylusPoints);
        //    }

        //    ComplexStroke cStroke = new ComplexStroke(allPoints);


        //    inkReconstruction1.Strokes = new StrokeCollection();
        //    inkReconstruction1.Strokes.Add(cStroke.ReconstructedStroke((int)coefficientSlider1.Value));

        //}

        private void cSlide1(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            StylusPointCollection allPoints = new StylusPointCollection();

            foreach (Stroke stroke in inkIn1.Strokes)
            {
                allPoints.Add(stroke.StylusPoints);
            }

            ComplexStroke cStroke = new ComplexStroke(allPoints);

            inkReconstruction1.Children.Clear();

            Stroke reconstruct = cStroke.ReconstructedStroke((int)coefficientSlider1.Value);
            DrawStrokePoints(reconstruct, inkReconstruction1, Brushes.Red);
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

            inkReconstruction2.Children.Clear();

            Stroke reconstruct = cStroke.ReconstructedStroke((int)coefficientSlider2.Value);
            DrawStrokePoints(reconstruct, inkReconstruction2, Brushes.Blue);

            inkReconstruction2.Strokes = new StrokeCollection();
            inkReconstruction2.Strokes.Add(cStroke.ReconstructedStroke((int)coefficientSlider2.Value));

        }

        private void matchClick(object sender, RoutedEventArgs e)
        {
            StylusPointCollection allPoints1 = new StylusPointCollection();
            StylusPointCollection allPoints2 = new StylusPointCollection();
            foreach (Stroke stroke in inkIn1.Strokes)
            {
                allPoints1.Add(stroke.StylusPoints);
            }            
            foreach (Stroke stroke in inkIn2.Strokes)
            {
                allPoints2.Add(stroke.StylusPoints);
            }

            ComplexStroke cStroke1 = new ComplexStroke(allPoints1);
            ComplexStroke cStroke2 = new ComplexStroke(allPoints2);

            ComplexStrokeMatcher matcher = new ComplexStrokeMatcher(cStroke1, cStroke2);

            int parsedValue;
            if (!int.TryParse(matchCoefficientTextbox.Text, out parsedValue))
            {
                MessageBox.Show("This is a number only field");
                return;
            }
            if(cStroke1.DFTCoefficients!=null && cStroke2.DFTCoefficients != null)
            {
                matcher.CalculateMatch(parsedValue);
                matchDistanceLabel.Content = matcher.Distance;


                cStroke2.DFTCoefficients[0] = cStroke1.DFTCoefficients[0];

                for (int k = -cStroke2.DFTCoefficients.Count / 2; k <= cStroke2.DFTCoefficients.Count / 2; k++)
                //for (int k = -parsedValue / 2; k < parsedValue/2; k++)
                {
                    double magnitude = 1 / matcher.meanR;
                    //double shift = (cStroke2.Stats.orientation - cStroke1.Stats.orientation + (2.0 * Math.PI * matcher.meanShift * (double)k)/(double)cStroke2.DFTCoefficients.Count);
                    double shift = -(matcher.meanShift * k + cStroke2.Stats.orientation - cStroke1.Stats.orientation);
                    //double shift = -matcher.meanShift;
                    Complex transform = Complex.FromPolarCoordinates(magnitude, shift);
                    int ind = Math.Abs(k);
                    if (k < 0)
                    {
                        cStroke2.DFTCoefficients[ind].negative *= transform;
                    }
                    else if (k > 0)
                    {
                        cStroke2.DFTCoefficients[ind].positive *= transform;
                    }



                }

                inkMatched.Children.Clear();

                DrawStrokePoints(cStroke1.ReconstructedStroke(), inkMatched, Brushes.Red);
                DrawStrokePoints(cStroke2.ReconstructedStroke(), inkMatched, Brushes.Blue);
            }
        }


        private void DrawStrokePoints(Stroke stk, InkCanvas outCanvas, Brush color)
        {
            for (int i = 0; i < stk.StylusPoints.Count; i++)
            {
                StylusPoint point = stk.StylusPoints[i];
                Ellipse circle = new Ellipse()
                {
                    Width = 5,
                    Height = 5,
                    Stroke = color,
                    StrokeThickness = 1,

                };
                InkCanvas.SetLeft(circle, point.X - circle.Width / 2);
                InkCanvas.SetTop(circle, point.Y - circle.Height / 2);

                outCanvas.Children.Add(circle);

            }
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
