using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;

namespace FaceOutputContract
{
    public class FaceData
    {
        public bool IsFaceTracked
        {
            get;
            set;
        }

        public Point[] ColorSpacePoints
        {
            get;
            set;
        }

        public Point3D[] CameraSpacePoints
        {
            get;
            set;
        }
    }
}
