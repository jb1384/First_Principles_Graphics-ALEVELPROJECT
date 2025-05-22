using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace First_Principles_Graphics
{
    class Angle
    {
        private int degrees;
        public Angle(int theta)
        {
            degrees = theta;
        }
        public int getDegrees()
        {
            return degrees;
        }
        public double getRadians()
        {
            return ((float)degrees * (Math.PI)) / 180;
            ;
        }
        public void Increment(int value)
        {
            degrees+= value;
        }
        public void Set(int value)
        {
            degrees = value;
        }
        public int getDegreesSimplified()
        {
            return degrees % 360;
        }
    }
}
