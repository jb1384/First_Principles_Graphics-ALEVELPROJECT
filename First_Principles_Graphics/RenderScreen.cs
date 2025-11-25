using System;
using System.Diagnostics.Contracts;
using System.Threading.Channels;

namespace First_Principles_Graphics
{
    public partial class Screen : Form
    {
        const int xsize = 1280;
        const int ysize = 720;
        private ViewerSettings settings = new ViewerSettings();

        //hub variables
        private Font hudFont = new Font("Arial", 12);
        private Brush hudBrushColor = Brushes.Black;

        private Wireframe model;
        private Size size = new Size(xsize, ysize);
        private List<Point> points = new List<Point>();
        private List<int[]> edges = new List<int[]>();

        //variables for handling dynamic projection technique changing
        private string projectionMode = "Perspective";
        private int modeIndex = 0;
        private Func<double, List<Point>> projectionMethod;
        private double zCoefficient = 0;

        //variables for handling random spinning
        private System.Windows.Forms.Timer spinTimer;
        private System.Windows.Forms.Timer ChangeSpinAxisTimer;
        private Action<double> SpinAxisProc;
        private Random rnd = new Random();
        private bool RandomSpinMode = false;

        //variables for handling inertia
        private bool inertiaOn = false;
        private double velRotX, velRotY, velRotZ, velEnlarge;

        private System.Windows.Forms.Timer inertiaTimer;

        public Screen()
        {
            //set up size
            this.Size = size;
            this.ClientSize = size;
            this.KeyPreview = true;
            this.Load += new EventHandler(Screen_Load);
            this.Paint += new PaintEventHandler(Draw_Lines);
            this.KeyDown += new KeyEventHandler(DetectKeyDown);
            this.DoubleBuffered = true;

            //set up buttons
            Button reset = new Button();
            reset.Text = "Reset";
            reset.Size = new Size(150, 60);
            reset.Location = new Point(1120, 10);
            reset.Click += new EventHandler(this.ResetClick);
            this.Controls.Add(reset);

            Button toggleProjection = new Button();
            toggleProjection.Text = "Mode Toggle";
            toggleProjection.Size = new Size(150, 60);
            toggleProjection.Location = new Point(1120, 80);
            toggleProjection.Click += new EventHandler(this.ToggleProjection);
            this.Controls.Add(toggleProjection);

            Button SpinRandomly = new Button();
            SpinRandomly.Text = "Auto-Spin";
            SpinRandomly.Size = new Size(150, 60);
            SpinRandomly.Location = new Point(1120, 150);
            SpinRandomly.Click += new EventHandler(this.SpinRandomly);
            this.Controls.Add(SpinRandomly);

            Button ToggleInertia = new Button();
            ToggleInertia.Text = "Inertia On";
            ToggleInertia.Size = new Size(150, 60);
            ToggleInertia.Location = new Point(1120, 220);
            ToggleInertia.Click += new EventHandler(this.ToggleInertia);
            this.Controls.Add(ToggleInertia);

            //menu buttons
            Button ChangeModel = new Button();
            ChangeModel.Text = "Change Model";
            ChangeModel.Size = new Size(150, 60);
            ChangeModel.Location = new Point(1120, 650);
            ChangeModel.Click += new EventHandler(this.ChangeModelScreen);
            this.Controls.Add(ChangeModel);

            Button openSettings = new Button();
            openSettings.Text = "Settings";
            openSettings.Size = new Size(150, 60);
            openSettings.Location = new Point(1120, 580);
            openSettings.Click += new EventHandler(OpenSettingsClick);
            this.Controls.Add(openSettings);

        }
        private void ResetClick(object sender, EventArgs e)
        {
            model.getThetaX().Set(0);
            model.getThetaY().Set(0);
            model.getThetaZ().Set(0);
            model.SetXOffset(0);
            model.SetYOffset(0);
            model.SetFocalLength(500);
            model.UpdateScale(120);
            model.ResetVertices();

            RefreshModel();
            RefreshProjection();
            Redraw();
        }
        private void OpenSettingsClick(object sender, EventArgs e)
        {
            SettingsForm sf = new SettingsForm(settings);
            if (sf.ShowDialog() == DialogResult.OK)
            {
                // Refresh drawing using new settings
                ApplySettings();
                Redraw();
            }
        }
        private void ApplySettings()
        {
            this.BackColor = settings.BackgroundColor;
        }
        private void ToggleInertia(object sender, EventArgs e)
        {
            inertiaOn = !inertiaOn;
            ((Button)sender).Text = inertiaOn ? "Inertia Off" : "Inertia On";
            velRotX = velRotY = velRotZ = velEnlarge = 0;
            if (inertiaOn)
                inertiaTimer.Start();
            else
                inertiaTimer.Stop();

        }
        private void ApplyInertia(Object sender, EventArgs e)
        {

            bool changed = false;
            // X rotation velocity
            if (Math.Abs(velRotX) > 0.01)
            {
                model.getThetaX().Increment(velRotX);
                velRotX *= settings.inertiaFrictionFactor;
                changed = true;
            }

            // Y rotation
            if (Math.Abs(velRotY) > 0.01)
            {
                model.getThetaY().Increment(velRotY);
                velRotY *= settings.inertiaFrictionFactor;
                changed = true;
            }

            // Z rotation
            if (Math.Abs(velRotZ) > 0.01)
            {
                model.getThetaZ().Increment(velRotZ);
                velRotZ *= settings.inertiaFrictionFactor;
                changed = true;
            }
            if(Math.Abs(velEnlarge) > 0.01)
            {
                model.IncrementScale(velEnlarge);
                velEnlarge *= settings.inertiaFrictionEnlargement;
                changed = true;
            }

            if (changed)
            {
                RefreshModel();
                RefreshProjection();
                Redraw();
            }
        }

        private void ChangeModelScreen(object sender, EventArgs e)
        {

            LoadNewWireframeForm loader = new LoadNewWireframeForm();
            DialogResult result = loader.ShowDialog();

            if (result == DialogResult.OK && !string.IsNullOrEmpty(loader.path))
            {
                string filePath = loader.path;
                model = new Wireframe(filePath);

                SpinAxisProc = model.getThetaX().Increment;
                spinTimer.Stop();
                ChangeSpinAxisTimer.Stop();

                projectionMethod = model.ProjectVerticesPerspective;
                zCoefficient = 0;
                modeIndex = 0;
                projectionMode = "perspective";
                RefreshModel();
                RefreshProjection();
                Redraw();
            }
        }
        private void ToggleProjection(object sender, EventArgs e)
        {
            string[] modes = { "Perspective", "Orthographic", "Cabinet", "Cavalier" };
            modeIndex = (modeIndex + 1) % modes.Length;
            string newMode = modes[modeIndex];
            projectionMode = newMode;

            switch (newMode)
            {
                case ("Perspective"):
                    projectionMethod = model.ProjectVerticesPerspective;
                    break;
                case ("Orthographic"):
                    zCoefficient = 0;
                    projectionMethod = model.ProjectVerticesParallel;
                    break;
                case ("Cabinet"):
                    zCoefficient = 0.5;
                    break;
                case ("Cavalier"):
                    zCoefficient = 1;
                    break;
            }

            RefreshProjection();
            Redraw();
        }

        private void SpinRandomly(object sender, EventArgs e)
        {
            RandomSpinMode = !RandomSpinMode;
            ((Button)sender).Text = RandomSpinMode ? "Stop Spin" : "Auto-Spin";
            if(RandomSpinMode)
            {
                spinTimer.Start();
                ChangeSpinAxisTimer.Start();
            }
            else
            {
                spinTimer.Stop();
                ChangeSpinAxisTimer.Stop();
            }
        }
        private void ChangeSpinAxis(object sender, EventArgs e)
        {
            int next = rnd.Next(0, 3);
            switch(next)
            {
                case 0:
                    SpinAxisProc = model.getThetaX().Increment;
                    break;
                case 1:
                    SpinAxisProc = model.getThetaY().Increment;
                    break;
                case 2:
                    SpinAxisProc = model.getThetaZ().Increment;
                    break;
            }
            ChangeSpinAxisTimer.Interval = rnd.Next(3000, 3500);
        }
        private void PerformRandomSpin(object sender, EventArgs e)
        {
            SpinAxisProc(2);

            RefreshModel();
            RefreshProjection();
            Redraw();
        }

        private void Draw_Lines(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            using (Pen pen = new Pen(settings.LineColor, 1))
            {
                foreach (int[] edge in edges)
                {
                    Point A = points[edge[0]];
                    Point B = points[edge[1]];
                    g.DrawLine(pen, A, B);
                }

             //draw HUD
             g.DrawString($"Wireframe Loaded: {model.getName()}",
             hudFont,
             hudBrushColor,
             new PointF(10, 10));

             g.DrawString($"Projection Mode: {projectionMode}",
             hudFont,
             hudBrushColor,
             new PointF(10, 35));

             g.DrawString($"Scale: {(int)model.getScale()}",
             hudFont,
             hudBrushColor,
             new PointF(10, 60));

             g.DrawString($"X Angle: {model.getThetaX().getDegreesSimplified()}",
             hudFont,
             hudBrushColor,
             new PointF(10, 85));

             g.DrawString($"Y Angle: {model.getThetaY().getDegreesSimplified()}",
             hudFont,
             hudBrushColor,
             new PointF(10, 110));

             g.DrawString($"Z Angle: {model.getThetaZ().getDegreesSimplified()}",
             hudFont,
             hudBrushColor,
             new PointF(10, 135));

             g.DrawString($"X Offset: {model.getOffsetX()}",
             hudFont,
             hudBrushColor,
             new PointF(10, 160));

             g.DrawString($"Y Offset: {model.getOffsetY()}",
             hudFont,
             hudBrushColor,
             new PointF(10, 185));
            }
        }
        private void Redraw()
        {
            Invalidate();
        }
        private Wireframe LoadCube()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wireframefiles", "cube.txt");
            Wireframe cube = new Wireframe(path);
            return cube;
        }
        private void RefreshModel() //recalcuate 3d points
        {
            model.ResetVertices();
            model.ApplyScale();
            model.RotateX();
            model.RotateY();
            model.RotateZ();
            model.ApplyOffset();
        }
        private void RefreshProjection() //recalcuate all projected points
        {
            projectionMethod(zCoefficient); //whatever projection method is being used gets stored in this function
            model.FormatVerticesToScreenSize(xsize, ysize);
            points = model.GetProjectedVertices();
            edges = model.GetEdges();
        }
        private double Accelerate(double velocity, double amount, double limit)
        {
            velocity += amount * (1 - velocity / settings.maxSpeed);
            return Math.Max(Math.Min(velocity, limit), -limit);
        }
        private void ApplyMovement(ref double velocity, double accel, double limit, Action<double> increment)
        {
            if (inertiaOn)
            {
                velocity = Accelerate(velocity, accel, limit);
                increment(velocity);
            }
            else
            {
                increment(accel > 0 ? settings.rotateSpeed : -settings.rotateSpeed);
            }
        }

        private void DetectKeyDown(object sender, KeyEventArgs e)
        {
            double accelerationAmount = settings.accelerationAmount;
            double maxSpeed = settings.maxSpeed;
            double accelerationAmountEnlargement = settings.accelerationAmountEnlargement;
            double maxSpeedEnlargement = settings.maxSpeedEnlargement;
            switch(e.KeyCode)
            {
                //angles (note to self: ref keyword passes the velrotx by reference, meaning when its changed in function its changed in entire program.
                case Keys.W: ApplyMovement(ref velRotX, -accelerationAmount, maxSpeed, model.getThetaX().Increment); break;
                case Keys.S: ApplyMovement(ref velRotX, accelerationAmount, maxSpeed, model.getThetaX().Increment); break;
                case Keys.A: ApplyMovement(ref velRotY, accelerationAmount, maxSpeed, model.getThetaY().Increment); break;
                case Keys.D: ApplyMovement(ref velRotY, -accelerationAmount, maxSpeed, model.getThetaY().Increment); break;
                case Keys.Q: ApplyMovement(ref velRotZ, -accelerationAmount, maxSpeed, model.getThetaZ().Increment); break;
                case Keys.E: ApplyMovement(ref velRotZ, accelerationAmount, maxSpeed, model.getThetaZ().Increment); break;

                //scale
                case (Keys.N): ApplyMovement(ref velEnlarge, accelerationAmountEnlargement, maxSpeedEnlargement, model.IncrementScale); break;
                case (Keys.M): ApplyMovement(ref velEnlarge, -accelerationAmountEnlargement, maxSpeedEnlargement, model.IncrementScale); break;


                //focal length
                case (Keys.C): model.IncrementFocalLength(5); break;
                case (Keys.V): model.IncrementFocalLength(-5); break;

                //offset
                case (Keys.D1): model.IncrementYOffset(-1); break;
                case (Keys.D2): model.IncrementYOffset(1); break;
                case (Keys.D3): model.IncrementXOffset(1); break;
                case (Keys.D4): model.IncrementXOffset(-1); break;
                default: break;
            }
            RefreshModel();
            RefreshProjection();
            Redraw();
        }
        private void Screen_Load(object sender, EventArgs e)
        {
            // Code to run when the form loads
            spinTimer = new System.Windows.Forms.Timer();
            spinTimer.Interval = 20; 
            spinTimer.Tick += new EventHandler(PerformRandomSpin);

            ChangeSpinAxisTimer = new System.Windows.Forms.Timer();
            ChangeSpinAxisTimer.Interval = rnd.Next(3000, 3500);
            ChangeSpinAxisTimer.Tick += new EventHandler(ChangeSpinAxis);

            inertiaTimer = new System.Windows.Forms.Timer();
            inertiaTimer.Interval = 16; 
            inertiaTimer.Tick += new EventHandler(ApplyInertia);

            model = LoadCube();
            SpinAxisProc = model.getThetaX().Increment;
            projectionMethod = model.ProjectVerticesPerspective;
            RefreshModel();
            RefreshProjection();
            System.Diagnostics.Debug.WriteLine("LOADED");
            Redraw();
        }
    }
}