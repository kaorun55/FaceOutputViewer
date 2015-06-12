using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace FaceOutputContract
{
    public class FaceOutputEventArgs
    {
        FaceData[] faceData;
        BitmapSource colorImage;

        public FaceOutputEventArgs( FaceData[] faceData, BitmapSource colorImage )
        {
            this.faceData = faceData;
            this.colorImage = colorImage;
        }

        public FaceData[] FaceData
        {
            get
            {
                return faceData;
            }
        }

        public BitmapSource ColroImage
        {
            get
            {
                return colorImage;
            }
        }
    }
}
