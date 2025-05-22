namespace First_Principles_Graphics
{
    class Coords3d
    {
        private double x;
        private double y;
        private double z;
        public Coords3d(double x_coord, double y_coord, double z_coord)
        {
            x = x_coord;
            y = y_coord;
            z = z_coord;
        }
        public double GetX()
        {
            return x;
        }
        public double GetY()
        {
            return y;
        }
        public double GetZ()
        {
            return z;
        }
        public void UpdateX(double newx)
        {
            x = newx;
        }
        public void UpdateY(double newy)
        {
            y = newy;
        }
        public void UpdateZ(double newz)
        {
            z = newz;
        }
    }
}