using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

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

        void Start();
        void Stop();
    }
}
