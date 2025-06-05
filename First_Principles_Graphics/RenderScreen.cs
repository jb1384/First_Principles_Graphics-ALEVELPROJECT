using System;
using System.Diagnostics.Contracts;

namespace First_Principles_Graphics
{
    public partial class Screen : Form
    {
        const int xsize = 1280;
        const int ysize = 720;

        //hub variables
        private Font hudFont = new Font("Arial", 12);
        private Brush hudBrushColor = Brushes.Black;

        private Wireframe model;
        private Size size = new Size(xsize, ysize);
        private List<Point> points = new List<Point>();
        private List<int[]> edges = new List<int[]>();
        private int rotateSpeed = 5;

        //variables for handling dynamic projection technique changing
        private string projectionMode = "Perspective";
        private int modeIndex = 0;
        private Func<double, List<Point>> projectionMethod;
        private double zCoefficient = 0;

        //variables for handling random spinning
        private System.Windows.Forms.Timer spinTimer;
        private System.Windows.Forms.Timer ChangeSpinAxisTimer;
        private Action<int> SpinAxisProc;
        private Random rnd = new Random();
        private bool RandomSpinMode = false;

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

            Button ChangeModel = new Button();
            ChangeModel.Text = "Change Model";
            ChangeModel.Size = new Size(150, 60);
            ChangeModel.Location = new Point(1120, 80);
            ChangeModel.Click += new EventHandler(this.ChangeModelScreen);
            this.Controls.Add(ChangeModel);

            Button toggleProjection = new Button();
            toggleProjection.Text = "Mode Toggle";
            toggleProjection.Size = new Size(150, 60);
            toggleProjection.Location = new Point(1120, 150);
            toggleProjection.Click += new EventHandler(this.ToggleProjection);
            this.Controls.Add(toggleProjection);

            Button SpinRandomly = new Button();
            SpinRandomly.Text = "Auto-Spin";
            SpinRandomly.Size = new Size(150, 60);
            SpinRandomly.Location = new Point(1120, 220);
            SpinRandomly.Click += new EventHandler(this.SpinRandomly);
            this.Controls.Add(SpinRandomly);
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
            using (Pen pen = new Pen(Color.Black, 1))
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

             g.DrawString($"Scale: {model.getScale()}",
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
        private void DetectKeyDown(object sender, KeyEventArgs e)
        {
            switch(e.KeyCode)
            {
                //angles
                case (Keys.W):
                    model.getThetaX().Increment(-rotateSpeed);
                    break;
                case (Keys.S):
                    model.getThetaX().Increment(rotateSpeed);
                    break;
                case (Keys.D):
                    model.getThetaY().Increment(-rotateSpeed);
                    break;
                case (Keys.A):
                    model.getThetaY().Increment(rotateSpeed);
                    break;
                case (Keys.Q):
                    model.getThetaZ().Increment(-rotateSpeed);
                    break;
                case (Keys.E):
                    model.getThetaZ().Increment(rotateSpeed);
                    break;

                //focal length
                case (Keys.C):
                    model.IncrementFocalLength(5);
                    break;
                case (Keys.V):
                    model.IncrementFocalLength(-5);
                    break;

                //scale
                case (Keys.N):
                    model.IncrementScale(1);
                    break;
                case (Keys.M):
                    model.IncrementScale(-1);
                    break;

                //offset
                case (Keys.D1):
                    model.IncrementYOffset(-1);
                    break;
                case (Keys.D2):
                    model.IncrementYOffset(1);
                    break;
                case (Keys.D3):
                    model.IncrementXOffset(1);
                    break;
                case (Keys.D4):
                    model.IncrementXOffset(-1);
                    break;
                default:
                    break;
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