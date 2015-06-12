using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace FaceOutputContract
{
    public interface IFaceOutputContract
    {
        event FaceOutputEventHandler OnFaceOutput;

        string Name
        {
            get;
        }

        FaceData[] FaceData
        {
            get;
        }

        BitmapSource ColorImage
        {
            get;
        }

        int ColorWidth
        {
            get;
        }

        int ColorHeight
        {
            get;
        }

        void Start();
        void Stop();
    }
}
