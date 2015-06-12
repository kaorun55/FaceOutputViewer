using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using FaceOutputContract;

namespace RealSenseFaceOutput
{
    [Export( typeof( IFaceOutputContract ) )]
    public class RealSenseFaceOutput : IFaceOutputContract
    {
        FaceData[] faceData;

        public event FaceOutputEventHandler OnFaceOutput;

        public string Name
        {
            get
            {
                return "RealSense FaceOutput";
            }
        }

        public FaceData[] FaceData
        {
            get
            {
                return faceData;
            }
        }

        public BitmapSource ColorImage
        {
            get;
            private set;
        }

        public int ColorWidth
        {
            get;
            private set;
        }

        public int ColorHeight
        {
            get;
            private set;
        }


        public void Start()
        {
            Trace.Write( "RealSenseFaceOutput.Start()" );
        }

        public void Stop()
        {
        }
    }
}
