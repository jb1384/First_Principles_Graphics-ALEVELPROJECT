using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace First_Principles_Graphics
{
    public struct RenderableFace : IComparable<RenderableFace>
    {
        public Point[] Points;   //projected points
        public double ZDepth;    // The average Z value of face
        public Color FaceColor;  // the face color is calculated with light

        // This allows List.Sort() to work automatically
        public int CompareTo(RenderableFace other)
        {
            // Sort descending (furthest Z first)
            return other.ZDepth.CompareTo(this.ZDepth);
        }
    }
}
