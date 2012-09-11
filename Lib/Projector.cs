using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using VVVV.Utils.VMath;
using System.Drawing;

namespace VVVV.Nodes.Mapping.Database
{
	[Serializable()]
	public class Projector : ISerializable
	{
		public Projector() { }

		public Projector(SerializationInfo info, StreamingContext ctxt)
		{
			this.View = DeSerialiseMatrix(info, "View");
			this.Projection = DeSerialiseMatrix(info, "Projection");
			this.Calibrated = (bool)info.GetValue("Calibrated", typeof(bool));
			this.Correspondences = (List<Correspondence>)info.GetValue("Correspondences", typeof(List<Correspondence>));
		}

		static Matrix4x4 DeSerialiseMatrix(SerializationInfo info, string Name)
		{
			Matrix4x4 Matrix = new Matrix4x4();
			for (int i = 0; i < 16; i++)
			{
				Matrix[i] = (double)info.GetValue(Name + "_" + i.ToString(), typeof(double));
			}
			return Matrix;
		}

		static void SerialiseMatrix(SerializationInfo info, string Name, Matrix4x4 Matrix)
		{
			for (int i = 0; i < 16; i++)
			{
				info.AddValue(Name + "_" + i.ToString(), Matrix[i]);
			}
		}

		public Matrix4x4 View;
		public Matrix4x4 Projection;
		public bool Calibrated;
		public double ReprojectionError { get; private set; }
		public List<Correspondence> Correspondences = new List<Correspondence>();

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			SerialiseMatrix(info, "View", this.View);
			SerialiseMatrix(info, "Projection", this.Projection);
			info.AddValue("Calibrated", this.Calibrated);
			info.AddValue("Reprojection Error", this.ReprojectionError);
			info.AddValue("Correspondences", this.Correspondences);
		}

		public void Calibrate(Size Resolution)
		{
			CalibrationSet Calibration = new CalibrationSet();
			try
			{
				foreach (var Correspondence in this.Correspondences)
				{
					Calibration.Add(Correspondence.World, Correspondence.Projection);
				}
				this.ReprojectionError = Calibration.Calibrate(Resolution);
				this.View = Calibration.View;
				this.Projection = Calibration.Projection;
				this.Calibrated = true;
			}
			catch (Exception e)
			{
				this.View = new Matrix4x4();
				this.Projection = new Matrix4x4();
				this.Calibrated = false;
			}
		}
	}
}
