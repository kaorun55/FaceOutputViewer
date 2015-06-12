using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using FaceOutputContract;
using Livet;

namespace FaceOutputViewer.Models
{
    public class FaceOutputModel : NotificationObject
    {
        /*
         * NotificationObjectはプロパティ変更通知の仕組みを実装したオブジェクトです。
         */

        [ImportMany]
        private List<IFaceOutputContract> addins = new List<IFaceOutputContract>();
        IFaceOutputContract faceData;

        int addinIndex = -1;

        public List<IFaceOutputContract> Addins
        {
            get
            {
                return addins;
            }
        }

        public void LoadAddin()
        {
            var catalog = new DirectoryCatalog( "addins" );
            var container = new CompositionContainer( catalog );
            container.ComposeParts( this );

            foreach ( var addin in addins ) {
                addin.OnFaceOutput += addin_OnFaceOutput;
            }

        }

        void addin_OnFaceOutput( object sender, FaceOutputEventArgs e )
        {
            RaisePropertyChanged( "UpdateFaceData" );
        }

        public void SelectAddin(int index)
        {
            addinIndex = index;
        }

        public void Start()
        {
            if ( addinIndex < 0 ) {
                return;
            }

            faceData = addins[addinIndex];
            faceData.Start();
        }

        public void Stop()
        {
            if ( addinIndex < 0 ) {
                return;
            }

            faceData.Stop();
        }


        #region FaceData変更通知プロパティ
        public FaceData[] FaceData
        {
            get
            {
                return faceData.FaceData;
            }
        }
        #endregion

        public BitmapSource ColorImage
        {
            get
            {
                return faceData.ColorImage;
            }
        }

        public int ColorWidth
        {
            get
            {
                return faceData.ColorWidth;
            }
        }

        public int ColorHeight
        {
            get
            {
                return faceData.ColorHeight;
            }
        }
    }
}
