using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Ink;
using System.Windows.Input;
using System.Numerics;


namespace PathMatching
{
    public class Coefficient
    {
        public Complex negative;
        public Complex positive;
    }

    public class Statistics
    {

        public double xBar;

        public double yBar;

        public double mu11;
        public double mu20;
        public double mu02;

        public double M00;
        public double M10;
        public double M01;
        public double M11;
        public double M20;
        public double M02;

        public double orientation;

        public Statistics() { }
        public Statistics(Statistics other) {
            xBar = other.xBar;
            yBar = other.yBar;
            mu11 = other.mu11;
            mu20 = other.mu20;
            mu02 = other.mu02;
            M00 = other.M00;
            M10 = other.M10;
            M01 = other.M01;
            M11 = other.M11;
            M20 = other.M20;
            M02 = other.M02;  
            orientation = other.orientation;
        }
    }



    class ComplexStroke 
    {
        private Stroke _BaseStroke;

        public Stroke BaseStroke {
            get
            {
                return _BaseStroke;
            }
            set
            {
                _BaseStroke = value;
                UpdateComplexPoints();
            }
        }


        public List<Complex> ComplexPoints { get; set; }
        public List<Coefficient> DFTCoefficients { get; set; }
        public Statistics Stats { get; set; } 


        //private Stroke _reconstructedStroke { get; set; }

        public ComplexStroke(StylusPointCollection stylusPoints)
        {
            if (stylusPoints.Count > 0)
            {
                BaseStroke = new Stroke(stylusPoints);
                UpdateComplexPoints();
            }

        }

        public ComplexStroke(Stroke stroke)
        {
            BaseStroke = stroke.Clone();
            UpdateComplexPoints();
        }



        private void UpdateComplexPoints()
        {
            ComplexPoints = new List<Complex>();
            Stats = new Statistics()
            {
                M00 = 0.0,
                M01 = 0.0,
                M10 = 0.0,
                M11 = 0.0,
                M02 = 0.0, 
                M20 = 0.0

            };

            foreach (StylusPoint point in BaseStroke.StylusPoints)
            {
                Complex cPoint = new Complex(point.X, point.Y);
                ComplexPoints.Add(cPoint);

                Stats.M00 += 1;
                Stats.M10 += point.X;
                Stats.M01 += point.Y;
                Stats.M11 += point.X * point.Y;
                Stats.M20 += point.X * point.X;
                Stats.M02 += point.Y * point.Y;

            }
            Stats.xBar = Stats.M10 / Stats.M00;
            Stats.yBar = Stats.M01 / Stats.M00;

            Stats.mu11 = (Stats.M11 / Stats.M00) - (Stats.xBar * Stats.yBar);
            Stats.mu20 = (Stats.M20 / Stats.M00) - (Stats.xBar * Stats.xBar);
            Stats.mu02 = (Stats.M02 / Stats.M00) - (Stats.yBar * Stats.yBar);

            if((Stats.mu20 - Stats.mu02) != 0)
            {
                Stats.orientation = (1.0 / 2.0) * Math.Atan2(2 * Stats.mu11, Stats.mu20 - Stats.mu02);
            }
           
            DFTCoefficients = DFT(ComplexPoints);
           
        }


        public Stroke ReconstructedStroke(int num = -1)
        {
            if(DFTCoefficients != null)            {
                int coefCount = ((-1 < num) && (num < DFTCoefficients.Count)) ? num : DFTCoefficients.Count;
                return IDFT(DFTCoefficients, coefCount);
            }
            else
            {
                return null;
            }

        }

        public void ApplyComplex(Complex complex)
        {

            for(int k = 1; k< DFTCoefficients.Count; k++)
            {
                DFTCoefficients[k].positive *= complex;
                DFTCoefficients[k].negative *= complex;
            }
        }

        public static List<Coefficient> DFT(List<Complex> complices)
        {
            int N = complices.Count;
            List<Coefficient> coeffs = new List<Coefficient>();
            
            for (int k = 0; k < N; k++)
            {

                Complex pos = new Complex(0, 0);
                Complex neg = new Complex(0, 0);

                for (int m = 0; m<N; m++)
                {
                    Complex a = -1.0 * Complex.ImaginaryOne*2.0 * Math.PI * m * k * ((double)1 / (double)N);

                    pos = pos + complices[m] * Complex.Exp(a);
                    neg = neg + complices[m] * Complex.Exp(-a);
                }
                Coefficient coeff = new Coefficient
                {
                    positive = pos / N,
                    negative = neg / N
                };

                coeffs.Add(coeff);
            }
            return coeffs;
        }


        public static Stroke IDFT(List<Coefficient> coeffs, int M)
        {
            StylusPointCollection points = new StylusPointCollection(M);

            int N = coeffs.Count;
            for (int m = 0; m < N; m++)
            {

                Complex point = new Complex(0,0);
                for (int k = -N/2; k <= N/2; k++)
                {
                    if (Math.Abs(k)<=M/2 )
                    {
                        Complex coeff = k < 0 ? coeffs[-k].negative : coeffs[k].positive;

                        Complex a = Complex.ImaginaryOne * 2.0 * Math.PI * m * k * ((double)1 / (double)N);
                        point += coeff * Complex.Exp(a);
                    }

                    
                }

                points.Add(new StylusPoint(point.Real, point.Imaginary));
            }

            return new Stroke(points);
        }


    }
}
