using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using FaceOutputContract;

namespace RealSenseFaceOutput
{
    [Export( typeof( IFaceOutputContract ) )]
    public class RealSenseFaceOutput : IFaceOutputContract
    {
        PXCMSenseManager senceManager;
        PXCMFaceData rsFaceData;

        Rectangle[] rect;       //描画用の長方形を用意する
        const int DETECTION_MAXFACES = 2;    //顔を検出できる最大人数を設定

        const int COLOR_WIDTH = 1980;
        const int COLOR_HEIGHT = 1080;
        const int COLOR_FPS = 30;

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

        public BitmapSource ColorImage
        {
            get;
            private set;
        }

        public int ColorWidth
        {
            get
            {
                return COLOR_WIDTH;
            }
        }

        public int ColorHeight
        {
            get
            {
                return COLOR_HEIGHT;
            }
        }

        public uint VertexCount
        {
            get;
            private set;
        }


        public void Start()
        {
            Trace.Write( "RealSenseFaceOutput.Start()" );

            // SenseManagerを生成する
            senceManager = PXCMSenseManager.CreateInstance();
            if ( senceManager == null ) {
                throw new Exception( "SenseManagerの生成に失敗しました" );
            }

            // カラーストリームを有効にする
            pxcmStatus sts = senceManager.EnableStream( PXCMCapture.StreamType.STREAM_TYPE_COLOR, COLOR_WIDTH, COLOR_HEIGHT, COLOR_FPS );
            if ( sts < pxcmStatus.PXCM_STATUS_NO_ERROR ) {
                throw new Exception( "カラーストリームの取得に失敗しました" );
            }

            // 顔検出を有効にする
            sts = senceManager.EnableFace();
            if ( sts < pxcmStatus.PXCM_STATUS_NO_ERROR ) {
                throw new Exception( "顔検出の有効化に失敗しました" );
            }

            //顔検出器を生成する
            var faceModule = senceManager.QueryFace();

            //顔検出のプロパティを取得
            PXCMFaceConfiguration config = faceModule.CreateActiveConfiguration();
            config.SetTrackingMode( PXCMFaceConfiguration.TrackingModeType.FACE_MODE_COLOR_PLUS_DEPTH );
            config.ApplyChanges();

            // パイプラインを初期化する
            pxcmStatus ret = senceManager.Init();
            if ( ret < pxcmStatus.PXCM_STATUS_NO_ERROR ) {
                throw new Exception( "初期化に失敗しました" );
            }

            // デバイス情報の取得
            PXCMCapture.Device device = senceManager.QueryCaptureManager().QueryDevice();
            if ( device == null ) {
                throw new Exception( "deviceの生成に失敗しました" );
            }


            // ミラー表示にする
            device.SetMirrorMode( PXCMCapture.Device.MirrorMode.MIRROR_MODE_HORIZONTAL );

            PXCMCapture.DeviceInfo deviceInfo;
            device.QueryDeviceInfo( out deviceInfo );
            if ( deviceInfo.model == PXCMCapture.DeviceModel.DEVICE_MODEL_IVCAM ) {
                device.SetDepthConfidenceThreshold( 1 );
                device.SetIVCAMFilterOption( 6 );
                device.SetIVCAMMotionRangeTradeOff( 21 );
            }

            config.detection.isEnabled = true;
            config.detection.maxTrackedFaces = DETECTION_MAXFACES;
            //config.pose.isEnabled = true;
            config.landmarks.isEnabled = true;
            //config.QueryExpressions().Enable();
            //config.QueryExpressions().EnableAllExpressions();
            //config.QueryRecognition().Enable();
            //config.QueryExpressions().properties.maxTrackedFaces = 2;
            config.ApplyChanges();

            rsFaceData = faceModule.CreateOutput();

            CompositionTarget.Rendering += CompositionTarget_Rendering;

            VertexCount = 78;
        }

        public void Stop()
        {
            CompositionTarget.Rendering -= CompositionTarget_Rendering;

            if ( senceManager != null ) {
                senceManager.Dispose();
                senceManager = null;
            }
        }

        void CompositionTarget_Rendering( object sender, EventArgs e )
        {
            // フレームを取得する
            pxcmStatus ret = senceManager.AcquireFrame( false );
            if ( ret < pxcmStatus.PXCM_STATUS_NO_ERROR ) {
                return;
            }

            PXCMCapture.Sample sample = senceManager.QuerySample();
            UpdateColorImage( sample.color );

            //顔のデータを更新する
            UpdateFaceFrame();

            // フレームを解放する
            senceManager.ReleaseFrame();

            UpdateFaceOutput();
        }

        private void UpdateColorImage( PXCMImage colorFrame )
        {
            // データを取得する
            PXCMImage.ImageData data;
            pxcmStatus ret = colorFrame.AcquireAccess( PXCMImage.Access.ACCESS_READ, PXCMImage.PixelFormat.PIXEL_FORMAT_RGB24, out data );
            if ( ret < pxcmStatus.PXCM_STATUS_NO_ERROR ) {
                return;
            }

            // Bitmapに変換する
            var buffer = data.ToByteArray( 0, COLOR_WIDTH * COLOR_HEIGHT * 3 );
            ColorImage = BitmapSource.Create( COLOR_WIDTH, COLOR_HEIGHT, 96, 96, PixelFormats.Bgr24, null, buffer, COLOR_WIDTH * 3 );

            // データを解放する
            colorFrame.ReleaseAccess( data );
        }

        private void UpdateFaceFrame()
        {
            if ( senceManager == null ) {
                return;
            }

            //SenceManagerモジュールの顔のデータを更新する
            rsFaceData.Update();

            //検出した顔の数を取得する
            int numFaces = rsFaceData.QueryNumberOfDetectedFaces();

            faceData = new FaceData[numFaces];

            //それぞれの顔ごとに情報取得および描画処理を行う
            for ( int i = 0; i < numFaces; ++i ) {
                faceData[i] = new FaceOutputContract.FaceData();

                //顔の情報を取得する
                PXCMFaceData.Face face = rsFaceData.QueryFaceByIndex( i );

                // 顔の位置を取得:Depthで取得する
                var detection = face.QueryDetection();
                if ( detection == null ) {
                    continue;
                }

                PXCMRectI32 faceRect;
                detection.QueryBoundingRect( out faceRect );

                //追加：フェイスデータからランドマーク（特徴点群）についての情報を得る
                var landmarkData = face.QueryLandmarks();

                if ( landmarkData == null ) {
                    continue;
                }

                List<Point> points = new List<Point>();

                //ランドマークデータから何個の特徴点が認識できたかを調べる
                var numPoints = landmarkData.QueryNumPoints();
                //認識できた特徴点の数だけ、特徴点を格納するインスタンスを生成する
                var landmarkPoints = new PXCMFaceData.LandmarkPoint[numPoints];
                //ランドマークデータから、特徴点の位置を取得、表示
                if ( landmarkData.QueryPoints( out landmarkPoints ) ) {
                    for ( int j = 0; j < numPoints; j++ ) {
                        points.Add( new Point( landmarkPoints[j].image.x, landmarkPoints[j].image.y ) );
                    }
                }

                faceData[i].IsFaceTracked = true;
                faceData[i].ColorSpacePoints = points.ToArray();
            }
        }

        private void UpdateFaceOutput()
        {
            if ( OnFaceOutput!=null ) {
                OnFaceOutput( this, new FaceOutputEventArgs( faceData, ColorImage ) );
            }
        }
    }
}
