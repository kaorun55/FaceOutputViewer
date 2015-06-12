﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FaceOutputContract;

namespace KinectV2FaceOutput
{
    [Export( typeof( IFaceOutputContract ) )]
    public class KinectV2FaceOutput : IFaceOutputContract
    {
        FaceData[] faceData;

        public event FaceOutputEventHandler OnFaceOutput;

        public string Name
        {
            get
            {
                return "Kinect V2 FaceOutput";
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
            Trace.Write( "KinectV2FaceOutput.Start()" );
        }

        public void Stop()
        {
        }
    }
}
