using System;
using System.Diagnostics.Contracts;

namespace First_Principles_Graphics
{
    public partial class Screen : Form
    {
        const int xsize = 1280;
        const int ysize = 720;

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
            reset.Click += new EventHandler(this.Reset_Click);
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
            toggleProjection.Click += new EventHandler(this.toggleProjection);
            this.Controls.Add(toggleProjection);
        }
        private void Reset_Click(object sender, EventArgs e)
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

                projectionMethod = model.ProjectVerticesPerspective;
                zCoefficient = 0;
                modeIndex = 0;
                projectionMode = "perspective";

                RefreshModel();
                RefreshProjection();
                Redraw();
            }
        }
        private void toggleProjection(object sender, EventArgs e)
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
             new Font("Arial", 12),
             Brushes.Black,
             new PointF(10, 10));

             g.DrawString($"Projection Mode: {projectionMode}",
             new Font("Arial", 12),
             Brushes.Black,
             new PointF(10, 35));

             g.DrawString($"Scale: {model.getScale()}",
             new Font("Arial", 12),
             Brushes.Black,
             new PointF(10, 60));

             g.DrawString($"X Angle: {model.getThetaX().getDegreesSimplified()}",
             new Font("Arial", 12),
             Brushes.Black,
             new PointF(10, 85));

             g.DrawString($"Y Angle: {model.getThetaY().getDegreesSimplified()}",
             new Font("Arial", 12),
             Brushes.Black,
             new PointF(10, 110));

             g.DrawString($"Z Angle: {model.getThetaZ().getDegreesSimplified()}",
             new Font("Arial", 12),
             Brushes.Black,
             new PointF(10, 135));

             g.DrawString($"X Offset: {model.getOffsetX()}",
             new Font("Arial", 12),
             Brushes.Black,
             new PointF(10, 160));

             g.DrawString($"Y Offset: {model.getOffsetY()}",
             new Font("Arial", 12),
             Brushes.Black,
             new PointF(10, 185));

            }
        }
        private void Redraw()
        {
            Invalidate();
        }
        private Wireframe LoadCube()
        {
            Wireframe cube = new Wireframe(@"C:\Users\joshb\source\repos\First_Principles_Graphics\First_Principles_Graphics\wireframefiles\cube.txt");
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
            model = LoadCube();
            projectionMethod = model.ProjectVerticesPerspective;
            RefreshModel();
            RefreshProjection();
            System.Diagnostics.Debug.WriteLine("LOADED");
            Redraw();
        }
    }
}