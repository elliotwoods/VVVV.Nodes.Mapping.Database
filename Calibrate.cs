#region usings
using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;

using VVVV.Nodes.OpenCV;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;
using Emgu.CV.CvEnum;
#endregion usings

namespace VVVV.Nodes.Mapping.Database
{
	#region PluginInfo
	[PluginInfo(Name = "Calibrate", Category = "Mapping", Version = "Correspondences", Tags = "", AutoEvaluate = true)]
	#endregion PluginInfo
	public class CalibrateNode : IPluginEvaluate
	{
		class CalibrationSet
		{
			List<MCvPoint3D32f> FObjectPoints = new List<MCvPoint3D32f>();
			List<PointF> FImagePoints = new List<PointF>();

			public Matrix4x4 View
			{
				get
				{
					return FExtrinsics.Matrix;
				}
			}

			public Matrix4x4 Projection
			{
				get
				{
					return FIntrinsics.Matrix * VMath.Scale(2.0 / (double) FIntrinsics.SensorSize.Width, -2.0 / (double) FIntrinsics.SensorSize.Height, 1.0) * VMath.Translate(-1.0, 1.0, 0.0) * VMath.Scale(1.0, -1.0, 1.0);
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

		#region fields & pins
		[Input("Database", IsSingle = true)]
		Pin<Database> FInDatabase;

		[Input("Width", DefaultValue = 1024, IsSingle=true)]
		ISpread<int> FInWidth;

		[Input("Height", DefaultValue = 768, IsSingle = true)]
		ISpread<int> FInHeight;

		[Input("Solve", IsBang = true, IsSingle = true)]
		ISpread<bool> FInSolve;

		[Output("Projector Index")]
		ISpread<int> FOutProjectorIndex;

		[Output("View Matrix")]
		ISpread<Matrix4x4> FOutViewMatrix;

		[Output("Projection Matrix")]
		ISpread<Matrix4x4> FOutProjectionMatrix;

		[Output("Reprojection Error", DimensionNames=new string[]{"px"})]
		ISpread<double> FOutReprojectionError;

		[Output("Status")]
		[Import()]
		ILogger FLogger;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if (FInSolve[0])
			{
				if (FInDatabase[0] != null)
				{
					Dictionary<int, CalibrationSet> set = new Dictionary<int, CalibrationSet>();

					foreach (var point in FInDatabase[0].Correspondences)
					{
						if (!set.ContainsKey(point.ProjectorIndex))
						{
							set.Add(point.ProjectorIndex, new CalibrationSet());
						}
						set[point.ProjectorIndex].Add(point.World, point.Projection);
					}

					FOutProjectorIndex.SliceCount = 0;
					FOutViewMatrix.SliceCount = 0;
					FOutProjectionMatrix.SliceCount = 0;
					FOutReprojectionError.SliceCount = 0;

					foreach (var projector in set)
					{
						FOutProjectorIndex.Add(projector.Key);
						FOutReprojectionError.Add(projector.Value.Calibrate(new Size(FInWidth[0], FInHeight[0])));
						FOutViewMatrix.Add(projector.Value.View);
						FOutProjectionMatrix.Add(projector.Value.Projection);
					}
				}
			}
		}
	}
}
