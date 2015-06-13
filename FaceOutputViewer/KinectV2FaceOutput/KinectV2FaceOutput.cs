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
using FaceOutputContract;
using Microsoft.Kinect;

namespace KinectV2FaceOutput
{
    [Export( typeof( IFaceOutputContract ) )]
    public class KinectV2FaceOutput : IFaceOutputContract
    {
        KinectSensor sensor;
        MultiSourceFrameReader multiReader;
        FrameDescription colorFrameDesc;

        ColorImageFormat colorFormat = ColorImageFormat.Bgra;

        KinectHdFace[] hdFace;
        Body[] bodies;

        // WPF
        WriteableBitmap colorBitmap;
        byte[] colorBuffer;
        int colorStride;
        Int32Rect colorRect;

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

        public BitmapSource ColorImage
        {
            get
            {
                return colorBitmap;
            }
        }

        public int ColorWidth
        {
            get;
            private set;
        }


        public uint VertexCount
        {
            get;
            private set;
        }


        public int ColorHeight
        {
            get;
            private set;
        }

        public void Start()
        {
            Trace.Write( "KinectV2FaceOutput.Start()" );

            sensor = KinectSensor.GetDefault();
            sensor.Open();

            // カラー画像の情報を作成する(BGRAフォーマット)
            colorFrameDesc = sensor.ColorFrameSource.CreateFrameDescription(
                                                    colorFormat );
            ColorWidth = colorFrameDesc.Width;
            ColorHeight = colorFrameDesc.Height;

            // カラー用のビットマップを作成する
            colorBitmap = new WriteableBitmap(
                                colorFrameDesc.Width, colorFrameDesc.Height,
                                96, 96, PixelFormats.Bgra32, null );
            colorStride = colorFrameDesc.Width * (int)colorFrameDesc.BytesPerPixel;
            colorRect = new Int32Rect( 0, 0,
                                colorFrameDesc.Width, colorFrameDesc.Height );
            colorBuffer = new byte[colorStride * colorFrameDesc.Height];

            // 顔情報を作成する
            bodies = new Body[sensor.BodyFrameSource.BodyCount];
            faceData = new FaceData[sensor.BodyFrameSource.BodyCount];

            hdFace = new KinectHdFace[sensor.BodyFrameSource.BodyCount];
            for ( int i = 0; i < sensor.BodyFrameSource.BodyCount; i++ ) {
                hdFace[i] = new KinectHdFace( sensor );
                faceData[i] = new FaceData();
            }

            // 
            multiReader = sensor.OpenMultiSourceFrameReader( FrameSourceTypes.Color | FrameSourceTypes.Body );
            multiReader.MultiSourceFrameArrived += multiReader_MultiSourceFrameArrived;

            // 顔の点の数
            VertexCount = KinectHdFace.VertexCount;
        }

        void multiReader_MultiSourceFrameArrived( object sender, MultiSourceFrameArrivedEventArgs e )
        {
            var multiFrame = e.FrameReference.AcquireFrame();
            if ( multiFrame ==null ) {
                return;
            }

            // カラーフレームを取得する
            using ( var colorFrame = multiFrame.ColorFrameReference.AcquireFrame() ) {
                if ( colorFrame != null ) {
                    // BGRAデータを取得する
                    colorFrame.CopyConvertedFrameDataToArray( colorBuffer, colorFormat );
                    colorBitmap.WritePixels( colorRect, colorBuffer, colorStride, 0 );
                }
            }

            // ボディ
            using ( var bodyFrame = multiFrame.BodyFrameReference.AcquireFrame() ) {
                if ( bodyFrame != null ) {
                    // ボディデータを取得する
                    bodyFrame.GetAndRefreshBodyData( bodies );

                    for ( int i = 0; i < bodies.Length; i++ ) {
                        hdFace[i].SetTrackingId( bodies[i].TrackingId );
                        hdFace[i].Update();

                        faceData[i].IsFaceTracked = hdFace[i].IsFaceTracked;
                        faceData[i].ColorSpacePoints = hdFace[i].ColorSpacePoints;
                        faceData[i].CameraSpacePoints = hdFace[i].CameraSpacePoints;
                    }
                }
            }

            UpdateFaceUotput();
        }

        private void UpdateFaceUotput()
        {
            if ( OnFaceOutput!=null ) {
                OnFaceOutput( this, new FaceOutputEventArgs( faceData, colorBitmap ) );
            }
        }

        public void Stop()
        {
            if ( multiReader != null ) {
                multiReader.MultiSourceFrameArrived -= multiReader_MultiSourceFrameArrived;
                multiReader.Dispose();
            }

            if ( sensor!=null ) {
                sensor.Close();
                sensor = null;
            }

            if ( colorBitmap != null ) {
                colorBitmap = null;
            }

            UpdateFaceUotput();
        }
    }
}
