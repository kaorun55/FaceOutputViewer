using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
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

            addins[addinIndex].Start();
        }

        public void Stop()
        {
            if ( addinIndex < 0 ) {
                return;
            }

            addins[addinIndex].Stop();
        }
    }
}
