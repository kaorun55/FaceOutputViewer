using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public void Start()
        {
            Trace.Write( "RealSenseFaceOutput.Start()" );
        }

        public void Stop()
        {
        }
    }
}
