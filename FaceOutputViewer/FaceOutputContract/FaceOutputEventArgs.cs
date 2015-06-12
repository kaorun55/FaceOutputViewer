using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceOutputContract
{
    public class FaceOutputEventArgs
    {
        FaceData[] faceData;

        public FaceOutputEventArgs( FaceData[] faceData )
        {
            this.faceData = faceData;
        }

        public FaceData[] FaceData
        {
            get
            {
                return faceData;
            }
        }
    }
}
