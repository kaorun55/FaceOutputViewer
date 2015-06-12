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
    }
}
