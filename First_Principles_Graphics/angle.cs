using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace First_Principles_Graphics
{
    class Angle
    {
        private double degrees;
        public Angle(int theta)
        {
            degrees = theta;
        }
        public double getDegrees()
        {
            return degrees;
        }
        public double getRadians()
        {
            return ((float)degrees * (Math.PI)) / 180;
            ;
        }
        public void Increment(double value)
        {
            degrees+= value;
        }
        public void Set(double value)
        {
            degrees = value;
        }
        public int getDegreesSimplified()
        {
            return (int)degrees % 360;
        }
    }
}
