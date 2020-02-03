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

        //private Stroke _reconstructedStroke { get; set; }

        public ComplexStroke(StylusPointCollection stylusPoints)
        {
            BaseStroke = new Stroke(stylusPoints);
            UpdateComplexPoints();
        }

        public ComplexStroke(Stroke stroke)
        {
            BaseStroke = stroke.Clone();
            UpdateComplexPoints();
        }



        private void UpdateComplexPoints()
        {
            ComplexPoints = new List<Complex>();
            foreach (StylusPoint point in BaseStroke.StylusPoints)
            {
                Complex cPoint = new Complex(point.X, point.Y);
                ComplexPoints.Add(cPoint);
            }
            DFTCoefficients = DFT(ComplexPoints);
           
        }


        public Stroke ReconstructedStroke(int num = -1)
        {
            int coefCount = (-1 < num && num < DFTCoefficients.Count) ? num : DFTCoefficients.Count;
            return IDFT(DFTCoefficients, coefCount);
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
                for (int k = -M/2; k <= M/2; k++)
                {
                    Complex coeff = k < 0 ? coeffs[-k].negative : coeffs[k].positive;

                    Complex a = Complex.ImaginaryOne * 2.0 * Math.PI * m * k * ((double)1 / (double)N);
                    point += coeff * Complex.Exp(a);
                    
                }

                points.Add(new StylusPoint(point.Real, point.Imaginary));
            }

            return new Stroke(points);
        }


    }
}
