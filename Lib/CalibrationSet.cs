using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using VVVV.Nodes.OpenCV;
using VVVV.Utils.VMath;

namespace VVVV.Nodes.Mapping.Database
{
	class CalibrationSet
	{
		List<MCvPoint3D32f> FObjectPoints = new List<MCvPoint3D32f>();
		List<PointF> FImagePoints = new List<PointF>();

		public Matrix4x4 View
		{
			get
			{
				return FExtrinsics.Matrix * VMath.Scale(-1.0, 1.0, -1.0);
			}
		}

		public Matrix4x4 Projection
		{
			get
			{
				return FIntrinsics.Matrix * VMath.Scale(2.0 / (double)FIntrinsics.SensorSize.Width, -2.0 / (double)FIntrinsics.SensorSize.Height, 1.0) * VMath.Translate(-1.0, 1.0, 0.0) * VMath.Scale(1.0, -1.0, 1.0);
			}
		}

		public void Add(Vector3D World, Vector2D Projector)
		{
			FObjectPoints.Add(new MCvPoint3D32f((float)World.x, (float)World.y, (float)World.z));
			FImagePoints.Add(new PointF((float)Projector.x, (float)Projector.y));
		}

		Intrinsics FIntrinsics = null;
		Extrinsics FExtrinsics = null;

		public double Calibrate(Size resolution)
		{
			MCvPoint3D32f[][] objectPoints = new MCvPoint3D32f[1][];
			PointF[][] imagePoints = new PointF[1][];

			int count = this.FObjectPoints.Count;

			objectPoints[0] = new MCvPoint3D32f[count];
			imagePoints[0] = new PointF[count];

			for (int i = 0; i < count; i++)
			{
				objectPoints[0][i] = FObjectPoints[i];
				imagePoints[0][i] = FImagePoints[i];
			}

			IntrinsicCameraParameters intrinsicParam = new IntrinsicCameraParameters();
			ExtrinsicCameraParameters[] extrinsicParams;

			Matrix<double> mat = intrinsicParam.IntrinsicMatrix;
			mat[0, 0] = resolution.Width;
			mat[1, 1] = resolution.Height;
			mat[0, 2] = resolution.Width / 2.0d;
			mat[1, 2] = resolution.Height / 2.0d;
			mat[2, 2] = 1;

			CALIB_TYPE flags;
			flags = CALIB_TYPE.CV_CALIB_FIX_K1 | CALIB_TYPE.CV_CALIB_FIX_K2 | CALIB_TYPE.CV_CALIB_FIX_K3 | CALIB_TYPE.CV_CALIB_FIX_K4 | CALIB_TYPE.CV_CALIB_FIX_K5 | CALIB_TYPE.CV_CALIB_FIX_K6 | CALIB_TYPE.CV_CALIB_USE_INTRINSIC_GUESS | CALIB_TYPE.CV_CALIB_ZERO_TANGENT_DIST;

			double error = CameraCalibration.CalibrateCamera(objectPoints, imagePoints, resolution, intrinsicParam, flags, out extrinsicParams);
			this.FIntrinsics = new Intrinsics(intrinsicParam, resolution);
			this.FExtrinsics = new Extrinsics(extrinsicParams[0]);
			return error;
		}
	}
}
