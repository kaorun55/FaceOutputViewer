using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;
using Microsoft.Kinect;
using Microsoft.Kinect.Face;

namespace KinectV2FaceOutput
{
    public class KinectHdFace
    {
        KinectSensor sensor;

        HighDefinitionFaceFrameSource hdFaceSource;
        HighDefinitionFaceFrameReader hdFaceReader;

        bool produce = false;
        FaceModelBuilder faceModelBuilder;
        FaceAlignment faceAlignment;
        FaceModel faceModel;

        Dictionary<FaceShapeDeformations, float> deformations = new Dictionary<FaceShapeDeformations, float>();

        public static uint VertexCount = FaceModel.VertexCount;

        public FaceModelBuilderCollectionStatus CollectionStatus = FaceModelBuilderCollectionStatus.MoreFramesNeeded;

        public bool IsFaceTracked = false;


        public Point3D[] CameraSpacePoints
        {
            get;
            private set;
        }

        public Point[] ColorSpacePoints
        {
            get;
            private set;
        }

        public int[] Triangles
        {
            get;
            set;
        }

        public ulong TrackingId
        {
            get
            {
                return hdFaceSource.TrackingId;
            }
        }

        public KinectHdFace( KinectSensor sensor )
        {
            this.sensor = sensor;

            hdFaceSource = new HighDefinitionFaceFrameSource( sensor );
            hdFaceReader = hdFaceSource.OpenReader();

            // イベントではなく、同期で呼び出す
            //hdFaceReader.FrameArrived += hdFaceReader_FrameArrived;
            faceModelBuilder = hdFaceSource.OpenModelBuilder( FaceModelBuilderAttributes.None );

            faceAlignment = new FaceAlignment();
            faceModel = new FaceModel( 1.0f, deformations );

            Triangles = new int[FaceModel.TriangleCount];
        }

        public void SetTrackingId( ulong trackingId )
        {
            hdFaceSource.TrackingId = trackingId;
        }

        public void Update()
        {
            if ( TrackingId  == 0 ) {
                return;
            }

            using ( var frame = hdFaceReader.AcquireLatestFrame() ) {
                if ( frame == null ) {
                    return;
                }

                IsFaceTracked = frame.IsFaceTracked;

                if ( !frame.IsFaceTracked ) {
                    return;
                }

                frame.GetAndRefreshFaceAlignmentResult( faceAlignment );

                BuildFaceModel();

                var cameraPoints = faceModel.CalculateVerticesForAlignment( faceAlignment );

                if ( CameraSpacePoints == null ) {
                    CameraSpacePoints = new Point3D[cameraPoints.Count];
                }

                if ( ColorSpacePoints == null ) {
                    ColorSpacePoints = new Point[cameraPoints.Count];
                }


                var colorPoints = new ColorSpacePoint[ColorSpacePoints.Length];
                sensor.CoordinateMapper.MapCameraPointsToColorSpace( cameraPoints.ToArray(), colorPoints );

                for ( int i = 0; i < cameraPoints.Count; i++ ) {
                    CameraSpacePoints[i] = new Point3D()
                    {
                        X = cameraPoints[i].X,
                        Y = cameraPoints[i].Y,
                        Z = cameraPoints[i].Z,
                    };

                    ColorSpacePoints[i] = new Point()
                    {
                        X = colorPoints[i].X,
                        Y = colorPoints[i].Y,
                    };
                }
            }
        }

        void hdFaceReader_FrameArrived( object sender, HighDefinitionFaceFrameArrivedEventArgs e )
        {
            Update();
        }

        private void BuildFaceModel()
        {
            //Trace.WriteLine( faceModelBuilder.CollectionStatus.ToString() );

            if ( produce ) {
                return;
            }

            CollectionStatus = faceModelBuilder.CollectionStatus;
            if ( CollectionStatus == FaceModelBuilderCollectionStatus.Complete ) {
                Trace.WriteLine( "CollectFaceData Start." );
                faceModelBuilder.BeginFaceDataCollection();
                produce = true;
            }
        }

        void faceModelBuilder_CollectionCompleted( object sender, FaceModelBuilderCollectionCompletedEventArgs e )
        {
            Trace.WriteLine( "CollectFaceData Success." );
            faceModel = e.ModelData.ProduceFaceModel();
        }
    }
}
