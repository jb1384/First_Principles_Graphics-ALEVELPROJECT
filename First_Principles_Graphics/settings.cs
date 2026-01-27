namespace First_Principles_Graphics
{
    public class ViewerSettings
    {
        public Color LineColor { get; set; } = Color.Black;
        public Color BackgroundColor { get; set; } = Color.White;
        public int rotateSpeed { get; set; } = 5;
        public int offsetSpeed { get; set; } = 1;
        public double inertiaFrictionFactor { get; set; } = 0.97;
        public double accelerationAmount { get; set; } = 0.6;
        public double maxSpeed { get; set; } = 50;
        public double inertiaFrictionEnlargement { get; set; } = 0.92;
        public double accelerationAmountEnlargement { get; set; } = 0.6;
        public double maxSpeedEnlargement { get; set; } = 20;
        public double inertiaFrictionOffset { get; set; } = 0.92;
        public double accelerationAmountOffset { get; set; } = 0.6;
        public double maxSpeedOffset { get; set; } = 100;
    }
}
