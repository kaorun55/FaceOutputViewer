using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Data;

using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.EventListeners;
using Livet.Messaging.Windows;

using FaceOutputViewer.Models;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Collections.ObjectModel;

namespace FaceOutputViewer.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        /* コマンド、プロパティの定義にはそれぞれ 
         * 
         *  lvcom   : ViewModelCommand
         *  lvcomn  : ViewModelCommand(CanExecute無)
         *  llcom   : ListenerCommand(パラメータ有のコマンド)
         *  llcomn  : ListenerCommand(パラメータ有のコマンド・CanExecute無)
         *  lprop   : 変更通知プロパティ(.NET4.5ではlpropn)
         *  
         * を使用してください。
         * 
         * Modelが十分にリッチであるならコマンドにこだわる必要はありません。
         * View側のコードビハインドを使用しないMVVMパターンの実装を行う場合でも、ViewModelにメソッドを定義し、
         * LivetCallMethodActionなどから直接メソッドを呼び出してください。
         * 
         * ViewModelのコマンドを呼び出せるLivetのすべてのビヘイビア・トリガー・アクションは
         * 同様に直接ViewModelのメソッドを呼び出し可能です。
         */

        /* ViewModelからViewを操作したい場合は、View側のコードビハインド無で処理を行いたい場合は
         * Messengerプロパティからメッセージ(各種InteractionMessage)を発信する事を検討してください。
         */

        /* Modelからの変更通知などの各種イベントを受け取る場合は、PropertyChangedEventListenerや
         * CollectionChangedEventListenerを使うと便利です。各種ListenerはViewModelに定義されている
         * CompositeDisposableプロパティ(LivetCompositeDisposable型)に格納しておく事でイベント解放を容易に行えます。
         * 
         * ReactiveExtensionsなどを併用する場合は、ReactiveExtensionsのCompositeDisposableを
         * ViewModelのCompositeDisposableプロパティに格納しておくのを推奨します。
         * 
         * LivetのWindowテンプレートではViewのウィンドウが閉じる際にDataContextDisposeActionが動作するようになっており、
         * ViewModelのDisposeが呼ばれCompositeDisposableプロパティに格納されたすべてのIDisposable型のインスタンスが解放されます。
         * 
         * ViewModelを使いまわしたい時などは、ViewからDataContextDisposeActionを取り除くか、発動のタイミングをずらす事で対応可能です。
         */

        /* UIDispatcherを操作する場合は、DispatcherHelperのメソッドを操作してください。
         * UIDispatcher自体はApp.xaml.csでインスタンスを確保してあります。
         * 
         * LivetのViewModelではプロパティ変更通知(RaisePropertyChanged)やDispatcherCollectionを使ったコレクション変更通知は
         * 自動的にUIDispatcher上での通知に変換されます。変更通知に際してUIDispatcherを操作する必要はありません。
         */

        FaceOutputModel faceModel = new FaceOutputModel();
        int CurrentPosition  = -1;

        public void Initialize()
        {
            CompositeDisposable.Add( new PropertyChangedEventListener( faceModel )
            {
                {"UpdateFaceData", (s, e) => UpdateFaceData()},
            } );
        }

        List<Ellipse> facePoints;

        private void UpdateFaceData()
        {
            var faceData = faceModel.FaceData.FirstOrDefault( f => f.IsFaceTracked );
            if ( faceData != null ) {
                UpdateEllipse( faceData );
            }

            // 表示
            RaisePropertyChanged( "CanvasFace" );
            RaisePropertyChanged( "ColorImage" );
        }

        private void CreateEllipse()
        {
            facePoints = new List<Ellipse>();
            for ( int i = 0; i <faceModel.VertexCount; i++ ) {
                facePoints.Add( MakeEllipse( new Point( 0, 0 ), 1, Brushes.Green, 0.5f ) );
            }

            _CanvasFace = new ObservableCollection<FrameworkElement>( facePoints );
        }

        private void UpdateEllipse( FaceOutputContract.FaceData faceData )
        {
            float scale = 980 / (float)faceModel.ColorWidth;

            for ( int i = 0; i <faceModel.VertexCount; i++ ) {
                var ellipse = facePoints[i];
                var point = faceData.ColorSpacePoints[i];

                // カラーを縮小表示しているため合わせる
                point.X *= scale;
                point.Y *= scale;

                // カラー座標系で円を配置する
                Canvas.SetLeft( ellipse, point.X - (ellipse.Width / 2) );
                Canvas.SetTop( ellipse, point.Y - (ellipse.Height / 2) );
            }
        }

        private Ellipse MakeEllipse( Point point, int R, Brush color, float scale )
        {
            var ellipse = new Ellipse()
            {
                Width = R * 2,
                Height =  R * 2,
                Fill = color,
            };

            // カラーを縮小表示しているため合わせる
            point.X *= scale;
            point.Y *= scale;

            // Depth座標系で円を配置する
            Canvas.SetLeft( ellipse, point.X - R );
            Canvas.SetTop( ellipse, point.Y - R );

            return ellipse;
        }


        #region UpdateAddinCommand
        private ViewModelCommand _UpdateAddinCommand;

        public ViewModelCommand UpdateAddinCommand
        {
            get
            {
                if ( _UpdateAddinCommand == null ) {
                    _UpdateAddinCommand = new ViewModelCommand( UpdateAddin );
                }
                return _UpdateAddinCommand;
            }
        }

        public void UpdateAddin()
        {
            faceModel.LoadAddin();
            AddinList = new ListCollectionView( faceModel.Addins.Select( a => a.Name ).ToArray() );
            AddinList.CurrentChanged += AddinList_CurrentChanged;

            faceModel.SelectAddin( AddinList.CurrentPosition );
        }

        void AddinList_CurrentChanged( object sender, EventArgs e )
        {
            var lv = sender as ICollectionView;
            CurrentPosition = lv.CurrentPosition;
            faceModel.SelectAddin( lv.CurrentPosition );

            if ( lv.CurrentPosition < 0 ) {
                System.Diagnostics.Trace.WriteLine( "選択無し" );
                return;
            }

            var name = lv.CurrentItem as string;
            System.Diagnostics.Trace.WriteLine( string.Format( "CurrentChanged:位置={0},名前={1}", lv.CurrentPosition, name ) );
        }
        #endregion


        #region AddinList変更通知プロパティ
        private ListCollectionView _AddinList;

        public ListCollectionView AddinList
        {
            get
            {
                return _AddinList;
            }
            set
            { 
                if ( _AddinList == value )
                    return;
                _AddinList = value;
                RaisePropertyChanged( "AddinList" );
            }
        }
        #endregion


        #region StartCommand
        private ViewModelCommand _StartCommand;

        public ViewModelCommand StartCommand
        {
            get
            {
                if ( _StartCommand == null ) {
                    _StartCommand = new ViewModelCommand( Start );
                }
                return _StartCommand;
            }
        }

        public void Start()
        {
            if ( AddinList ==null ) {
                return;
            }

            faceModel.Start();
            CreateEllipse();
        }
        #endregion


        #region StopCommand
        private ViewModelCommand _StopCommand;

        public ViewModelCommand StopCommand
        {
            get
            {
                if ( _StopCommand == null ) {
                    _StopCommand = new ViewModelCommand( Stop );
                }
                return _StopCommand;
            }
        }

        public void Stop()
        {
            if ( AddinList ==null ) {
                return;
            }

            faceModel.Stop();
        }
        #endregion


        #region ColorImage変更通知プロパティ
        public BitmapSource ColorImage
        {
            get
            {
                return faceModel.ColorImage;
            }
        }
        #endregion


        #region CanvasFace変更通知プロパティ
        private ObservableCollection<FrameworkElement> _CanvasFace = new ObservableCollection<FrameworkElement>();

        public ObservableCollection<FrameworkElement> CanvasFace
        {
            get
            {
                return _CanvasFace;
            }
            set
            {
                if ( _CanvasFace == value )
                    return;
                _CanvasFace = value;
                RaisePropertyChanged( "CanvasFace" );
            }
        }
        #endregion
    }
}
