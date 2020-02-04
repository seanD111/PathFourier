using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace PathMatching
{
    class ComplexStrokeMatcher
    {
        public static double Wm = 0.9;
        public static double Wp = 0.1;


        private ComplexStroke stroke1 { get; set; }
        private ComplexStroke stroke2 { get; set; }

        private List<double> ratios { get; set; }
        private List<double> shifts { get; set; }

        public double stdR;
        public double meanR;
        public double stdShift;
        public double meanShift;

        public double Distance;




        public ComplexStrokeMatcher(ComplexStroke st1, ComplexStroke st2)
        {
            stroke1 = st1;
            stroke2 = st2;
        }

        public void CalculateMatch(int num=-1)
        {
            List<Coefficient> coeffs1 = stroke1.DFTCoefficients;
            List<Coefficient> coeffs2 = stroke2.DFTCoefficients;
            if ((coeffs1 != null) && (coeffs2 != null))
            {
                ratios = new List<double>();
                shifts = new List<double>();

                int Mc = new[] { coeffs1.Count, coeffs2.Count }.Min();
                if (num > 0)
                {
                    Mc = Math.Min(num, Mc);
                }
                
                
                for(int k = -Mc/2; k <= Mc/2; k++)
                {
                    if (k != 0)
                    {
                        Complex coeff1 = k < 0 ? coeffs1[-k].negative : coeffs1[k].positive;
                        Complex coeff2 = k < 0 ? coeffs2[-k].negative : coeffs2[k].positive;

                        double ratio = coeff2.Magnitude / coeff1.Magnitude;
                        ratios.Add(ratio);

                        double shift = (coeff1.Phase - coeff2.Phase + stroke1.Stats.orientation -stroke2.Stats.orientation )/(double)k;
                        shifts.Add(shift);
                    }                    
                }

                meanR = ratios.Average();
                stdR = Math.Sqrt(ratios.Average(v => Math.Pow(v - meanR, 2)));

                meanShift = shifts.Average();
                stdShift = Math.Sqrt(shifts.Average(v => Math.Pow(v - meanShift, 2)));

                Distance = Wm * stdR + Wp * stdShift;
            }


        }



    }
}
