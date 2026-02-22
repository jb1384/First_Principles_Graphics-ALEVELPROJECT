using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.LinkLabel;

namespace First_Principles_Graphics
{
    class Wireframe
    {
        //3d vertices
        private List<Coords3d> vertices = new List<Coords3d>();
        private List<Coords3d> unmodifiedVertices = new List<Coords3d>();

        //2d vertices
        private List<Point> projectedVertices = new List<Point>();

        private List<int[]> edges = new List<int[]>();

        private string name;
        private Angle thetaX = new Angle(0);
        private Angle thetaY = new Angle(0);
        private Angle thetaZ = new Angle(0);
        private int focalLength = 500;
        private double scale = 100;
        private Coords3d offset = new Coords3d(0,0,0); //represents offset vector from center of screen
        private string path_; 
        //faces
        private bool hasFaceInfo;
        private List<int[]> faces;


        public Wireframe(string path)
        {
            
            path_ = path;
            hasFaceInfo = false;
            try
            {
                LoadFileFromPath(path_);
            }
            catch(Exception e)
            {
                MessageBox.Show($"Error Loading Wireframe File: {e}");
                string fallBackPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wireframefiles","Platonic", "Cube.txt");
                LoadFileFromPath(fallBackPath);
            }

        }
        private void LoadFileFromPath(string path)
        {
            string[] lines;
            lines = File.ReadAllLines(path);
            name = lines[0];

            //load vertices and edges into respective lists
            int lineNum = 1;
            string line = lines[lineNum];
            string[] s;
            faces = new List<int[]>();

            //load vertices
            Coords3d coords;
            while (line != "edge")
            {
                s = line.Split(',');
                coords = new Coords3d(float.Parse(s[0]), float.Parse(s[1]), float.Parse(s[2]));
                vertices.Add(coords);
                unmodifiedVertices.Add(coords);
                lineNum++;
                line = lines[lineNum];
            }

            //load edges
            int[] edge;

            while (line != "face" && lineNum < lines.Length - 1)
            {
                lineNum++;
                line = lines[lineNum].Trim();

                if (line == "face")
                    break;

                s = line.Split(',');
                edge = new int[2] { Convert.ToInt16(s[0]), Convert.ToInt16(s[1]) };
                edges.Add(edge);
            }

            // get face info is there is any.
            if (lineNum >= lines.Length || lines[lineNum] != "face")
            {
                hasFaceInfo = false;
                return;
            }
            lineNum++;
            line = lines[lineNum];
            hasFaceInfo = true;
            int[] face;
            while (lineNum < lines.Length)
            {
                line = lines[lineNum].Trim();
                s = line.Split(',');
                face = new int[s.Length];
                int i = 0;
                foreach (string v in s)
                {
                    face[i] = Convert.ToInt16(v);
                    i++;
                }
                faces.Add(face);
                lineNum++;
            }
        }
        public void FormatVerticesToScreenSize(int sizex, int sizey) //this procedure centres the vertices
        {
            int xcentre = ((sizex) / 2);
            int ycentre = ((sizey) / 2);

            List<Point> newprojectedVertices = new List<Point>();
            foreach(Point p in projectedVertices)
            {
                float currentX = p.X;
                float currentY = p.Y;
                int newX = Convert.ToInt16((currentX) + xcentre);
                int newY = Convert.ToInt16((currentY) + ycentre);

                newprojectedVertices.Add(new Point(newX, newY));
            }
            projectedVertices = newprojectedVertices;
        }

        public List<Point> ProjectVerticesPerspective(double DummyParameter)
        {
            projectedVertices.Clear();
            int u;
            int v; 

            foreach(Coords3d vertex in vertices)
            {
                double x = vertex.GetX();
                double y = vertex.GetY();
                double z = vertex.GetZ();

                if (z + focalLength == 0) //prevent crashes
                {
                    z -= 0.1;
                }

                //Offset Perspective Projection
                u = Convert.ToInt16((focalLength * x) / (focalLength + z));
                v = Convert.ToInt16((focalLength * y) / (focalLength + z));
                Point p = new Point(u, v);
                projectedVertices.Add(p);
            }
            return projectedVertices;
        }
        public List<Point> ProjectVerticesParallel(double zCoefficient = 0)
        {
            projectedVertices.Clear();
            int u;
            int v;
            double angle = Math.PI / 4;

            foreach (Coords3d vertex in vertices)
            {
                double x = vertex.GetX();
                double y = vertex.GetY();
                double z = vertex.GetZ();

                u = Convert.ToInt16(x + z * zCoefficient * Math.Cos(angle));
                v = Convert.ToInt16(y + z * zCoefficient * Math.Sin(angle));
                Point p = new Point(u, v);
                projectedVertices.Add(p);
            }
            return projectedVertices;
        }
        public void ApplyScale()
        {
            double x;
            double y;
            double z;

            int i = 0;
            foreach(Coords3d vertex in unmodifiedVertices)
            {
                x = vertex.GetX();
                y = vertex.GetY();
                z = vertex.GetZ();

                vertices[i].UpdateX(x * scale);
                vertices[i].UpdateY(y * scale);
                vertices[i].UpdateZ(z * scale);

                i++;
            }
        }
        public void RotateX()
        {
            double newY;
            double newZ;
            
            foreach(Coords3d vertex in vertices)
            {
                newY = (vertex.GetY() * Math.Cos(thetaX.getRadians())) - (vertex.GetZ() * Math.Sin(thetaX.getRadians()));
                newZ = (vertex.GetY() * Math.Sin(thetaX.getRadians())) + (vertex.GetZ() * Math.Cos(thetaX.getRadians()));

                vertex.UpdateY(newY);
                vertex.UpdateZ(newZ);
            }
        }
        public void RotateY()
        {
            double newX;
            double newZ;

            foreach(Coords3d vertex in vertices)
            {
                newX = (vertex.GetX() * Math.Cos(thetaY.getRadians()) + (vertex.GetZ() * Math.Sin(thetaY.getRadians())));
                newZ = (vertex.GetZ() * Math.Cos(thetaY.getRadians()) - (vertex.GetX() * Math.Sin(thetaY.getRadians())));

                vertex.UpdateX(newX);
                vertex.UpdateZ(newZ);
            }
        }
        public void RotateZ()
        {
            double newX;
            double newY;

            foreach(Coords3d vertex in vertices)
            {
                newX = ((vertex.GetX() * Math.Cos(thetaZ.getRadians()) - (vertex.GetY() * Math.Sin(thetaZ.getRadians()))));
                newY = ((vertex.GetX() * Math.Sin(thetaZ.getRadians()) + (vertex.GetY() * Math.Cos(thetaZ.getRadians()))));

                vertex.UpdateX(newX);
                vertex.UpdateY(newY);
            }
        }
        public void ApplyOffset()
        {
            double x;
            double y;
            double offsetX = offset.GetX();
            double offsetY = offset.GetY();
            foreach(Coords3d vertex in vertices)
            {
                x = vertex.GetX();
                y = vertex.GetY();

                vertex.UpdateX(x + offsetX);
                vertex.UpdateY(y + offsetY);
            }
        }
        public void ResetVertices()
        {
            vertices.Clear();
            foreach (Coords3d v in unmodifiedVertices)
            {
                vertices.Add(new Coords3d(v.GetX(), v.GetY(), v.GetZ()));
            }
        }
        //setters
        public void IncrementFocalLength(int l)
        {
            focalLength += l;
        }
        public void SetFocalLength(int l)
        {
            focalLength = l;
        }
        public void UpdateScale(double s)
        {
            scale = s;
        }
        public void IncrementScale(double factor)
        {
            scale += factor;
        }
        public void SetXOffset(double x)
        {
            offset.UpdateX(x);
        }
        public void SetYOffset(double y)
        {
            offset.UpdateY(y);
        }
        public void IncrementXOffset(double X)
        {
            double xOffset = offset.GetX();
            offset.UpdateX(xOffset + X);
        }
        public void IncrementYOffset(double Y)
        {
            double yOffset = offset.GetY();
            offset.UpdateY(yOffset + Y);
        }

        private double[] GetVector(Coords3d p1, Coords3d p2)
        {
            double x = p2.GetX() - p1.GetX();
            double y = p2.GetY() - p1.GetY();
            double z = p2.GetZ() - p1.GetZ();
            double[] vectorBetweenPoints = { x, y, z};

            return vectorBetweenPoints;
        }

        private double[] CrossProduct(double[] v1, double[] v2)
        {
            double x = v1[1] * v2[2] - v1[2] * v2[1];
            double y = v1[2] * v2[0] - v1[0] * v2[2];
            double z = v1[0] * v2[1] - v1[1] * v2[0];
            double[] crossProduct = { x, y, z };

            return crossProduct;
        }

        private double DotProduct(double[] v1, double[] v2)
        {
            return (v1[0] * v2[0]) + (v1[1] * v2[1]) + (v1[2] * v2[2]);
        }

        public List<int[]> GetVisibleEdges(bool isPerspective)
        {
            //if no face data exists, return all edges. should not be needed.
            if (!hasFaceInfo || faces.Count == 0) return edges;

            List<int[]> visibleEdges = new List<int[]>();

            //use a hashset to prevent drawing the same shared edge twice
            HashSet<string> addedEdges = new HashSet<string>();
            bool isVisible;
            Coords3d p0;
            Coords3d p1;
            Coords3d p2;
            foreach (int[] face in faces)
            {
                if (face.Length < 3) //handles lines which are not part of a faces
                {
                    isVisible = true;
                    int start = face[0];
                    int end = face[1];
                    visibleEdges.Add(new int[] { start, end });
                    continue;
                }

                //only 3 vertices are required to define the plane. 
                p0 = vertices[face[0]];
                p1 = vertices[face[1]];
                p2 = vertices[face[2]];

                //find the plane by finding 2 vectors on it. 
                double[] vA = GetVector(p0, p1);
                double[] vB = GetVector(p0, p2);

                double[] normal = CrossProduct(vA, vB);

                //calculate view vector. if perspective, this is vector from camera to point. if parallel, this is (0, 0, 1) 
                isVisible = false;

                if (isPerspective)
                {
                    //vector from camera to a point on the face (p0)
                    double[] viewVector = new double[] {p0.GetX(), p0.GetY(), p0.GetZ() + focalLength};

                    //if dot product > 0, the face is pointing towards the camera
                    if (DotProduct(normal, viewVector) < 0)
                    {
                        isVisible = true;
                    }
                }
                else
                {
                    //orthographic, check the Z component of the normal
                    //if Z is negative, it points towards the screen
                    if (normal[2] < 0)
                    {
                        isVisible = true;
                    }
                }

                if (isVisible)  //add all edges of this face to the visible list
                {
                    for (int i = 0; i < face.Length; i++)
                    {
                        int start = face[i];
                        int end = face[(i + 1) % face.Length];

                        //key used to prevent duplicates being added
                        string key = start < end ? $"{start}-{end}" : $"{end}-{start}";

                        if (!addedEdges.Contains(key))
                        {
                            visibleEdges.Add(new int[] { start, end });
                            addedEdges.Add(key);
                        }
                    }
                }
            }
            return visibleEdges;
        }
        


        //getters
        public List<Point> GetProjectedVertices()
        {
            return projectedVertices;
        }
        public List<int[]> GetEdges()
        {
            return edges;
        }
        public Angle getThetaX()
        {
            return thetaX;
        }
        public Angle getThetaY()
        {
            return thetaY;
        }
        public Angle getThetaZ()
        {
            return thetaZ;
        }
        public double getScale()
        {
            return scale;
        }
        public string getName()
        {
            return name;
        }
        public double getOffsetX()
        {
            return offset.GetX();
        }
        public double getOffsetY()
        {
            return offset.GetY();
        }
        public int getFocalLength()
        {
            return focalLength;
        }
        public string getPath()
        {
            return path_;
        }
        public bool getHasFaceInfo()
        {
            return hasFaceInfo;
        }
    }
}
