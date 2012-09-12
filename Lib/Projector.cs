using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using VVVV.Utils.VMath;
using System.Drawing;
using VVVV.Nodes.OpenCV;

namespace VVVV.Nodes.Mapping.Database
{
	[Serializable()]
	public class Projector : ISerializable
	{
		public Projector(int Width, int Height)
		{
			this.Width = Width;
			this.Height = Height;
		}

		public Projector(SerializationInfo info, StreamingContext ctxt)
		{
			this.FView = DeSerialiseMatrix(info, "View");
			this.FProjection = DeSerialiseMatrix(info, "Projection");
            this.ReprojectionError = (double)info.GetValue("Reprojection Error", typeof(double));
			this.Calibrated = (bool)info.GetValue("Calibrated", typeof(bool));
			this.Correspondences = (List<Correspondence>)info.GetValue("Correspondences", typeof(List<Correspondence>));
			this.Width = (int)info.GetValue("Width", typeof(int));
			this.Height = (int)info.GetValue("Height", typeof(int));
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

		public int Width {get; private set;}
		public int Height {get; private set;}

        Matrix4x4 FView;
        public Matrix4x4 View
        {
            get
            {
                if (!this.Calibrated)
                    return VVVV.Utils.VMath.VMath.IdentityMatrix;
                else
                    return FView;
            }
        }
        Matrix4x4 FProjection;
        public Matrix4x4 Projection
        {
            get
            {
                if (!this.Calibrated)
                    return VVVV.Utils.VMath.VMath.IdentityMatrix;
                else
                    return FProjection;
            }
        }

		public bool Calibrated;
		public double ReprojectionError { get; private set; }
		public List<Correspondence> Correspondences = new List<Correspondence>();

		public Intrinsics Intrinsics { get; private set; }

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			SerialiseMatrix(info, "View", this.View);
			SerialiseMatrix(info, "Projection", this.Projection);
			info.AddValue("Calibrated", this.Calibrated);
			info.AddValue("Reprojection Error", this.ReprojectionError);
			info.AddValue("Correspondences", this.Correspondences);
			info.AddValue("Width", this.Width);
			info.AddValue("Height", this.Height);
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
				this.FView = Calibration.View;
				this.FProjection = Calibration.Projection;
				this.Intrinsics = Calibration.Intrinsics;
				this.Calibrated = true;
			}
			catch (Exception e)
			{
                this.ReprojectionError = 0.0;
				this.Calibrated = false;
			}
		}

        public Dictionary<int, List<Correspondence>> CorrespondencesPerBoard
        {
            get
            {
                Dictionary<int, List<Correspondence>> CorrespondencesPerBoard = new Dictionary<int, List<Correspondence>>();

                foreach (var correspondence in this.Correspondences)
                {
                    if (!CorrespondencesPerBoard.ContainsKey(correspondence.BoardIndex))
                        CorrespondencesPerBoard.Add(correspondence.BoardIndex, new List<Correspondence>());

                    CorrespondencesPerBoard[correspondence.BoardIndex].Add(correspondence);
                }

                return CorrespondencesPerBoard;
            }
        }
	}
}
