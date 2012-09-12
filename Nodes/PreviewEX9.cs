#region usings
using System;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;

using SlimDX;
using SlimDX.Direct3D9;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.EX9;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Utils.SlimDX;
using System.Collections.Generic;

#endregion usings

//here you can change the vertex type
using VertexType = VVVV.Utils.SlimDX.ColoredVertex;

namespace VVVV.Nodes.Mapping.Database
{
	public struct BoardVertexBuffer
	{
		public BoardVertexBuffer(int Count, VertexBuffer VertexBuffer)
		{
			this.Count = Count;
			this.VertexBuffer = VertexBuffer;
		}

		public int Count;
		public VertexBuffer VertexBuffer;
	}

    //custom data per graphics device
    public class PreviewDeviceData : DeviceData
    {
        //vertex buffer for this device (per projector, per board)
        public List<List<BoardVertexBuffer>> VertexBufferPerProjectorPerLine { get; set; }

		public PreviewDeviceData(List<List<BoardVertexBuffer>> vb)
            : base()
        {
            VertexBufferPerProjectorPerLine = vb;
        }
    }

    #region PluginInfo
    [PluginInfo(Name = "Preview",
                Category = "Mapping",
                Version = "EX9",
                Help = "Render the dataset as a DirectX dataset",
                Tags = "")]
    #endregion PluginInfo
    public class PreviewEX9 : DXLayerOutPluginBase<PreviewDeviceData>, IPluginEvaluate
    {
        #region fields & pins
		[Input("Transform In")]
		IDiffSpread<Matrix> FInTransform;

        [Input("Database", IsSingle = true, Order = -1)]
        Pin<Database> FInDatabase;

        [Input("Projector Index")]
        IDiffSpread<int> FInProjectorIndex;

		[Output("Z")]
		ISpread<ISpread<double>> FOutZ;

        [Import]
        ILogger FLogger;

        //slice count
        int FSpreadCount;
        protected Database FDatabase;

        #endregion fields & pins

        // import host and hand it to base constructor
        // the two booleans set whether to create a render state and/or sampler state pin
        [ImportingConstructor]
        public PreviewEX9(IPluginHost host)
            : base(host, true, true)
        {
        }

        #region ReadsDatabase
        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            if (FDatabase != FInDatabase[0])
                this.SetDatabase(FInDatabase[0]);

            Evaluate2(SpreadMax);

            if (!FValid)
            {
                FValid = true;
                if (FDatabase != null)
                    UpdateOutput(SpreadMax);
            }
        }

        void SetDatabase(Database database)
        {
            if (FDatabase != null)
                FDatabase.Update -= FDatabase_Update;
            FDatabase = database;
            if (FDatabase != null)
            {
                FDatabase.Update += FDatabase_Update;
                this.Invalidate();
            }
        }

        bool FValid = true;
        void FDatabase_Update(object sender, EventArgs e)
        {
            this.Invalidate();
        }

        protected void Invalidate()
        {
            FValid = false;
        }
        #endregion

        //called when data for any output pin is requested
        public void Evaluate2(int SpreadMax)
        {
        }

        void UpdateOutput(int SpreadMax)
        {
            Reinitialize();

            //update device data, if vertex positions are changed but not reinitialise
            //Update();
            //we don't use that here because realtime speeds are not required
            //so we do everything in Reinitialise
        }

        #region device data handling

        //this method gets called, when Reinitialize() was called in evaluate,
        //or a graphics device asks for its data
        protected override PreviewDeviceData CreateDeviceData(Device device)
        {
            FLogger.Log(LogType.Message, "Creating resource...");

			var lines = new List<List<BoardVertexBuffer>>();

			int boardCount = 0;
			foreach (var projector in FDatabase.Projectors)
				boardCount += projector.Value.CorrespondencesPerBoard.Values.Count;

			FOutZ.SliceCount = boardCount;

			int slice0 = 0;

            foreach(var projector in FDatabase.Projectors)
            {
                var boards = projector.Value.CorrespondencesPerBoard;

				var linesPerProjector = new List<BoardVertexBuffer>();

                foreach (var board in boards)
                {
                    //create a vertex buffer with desired size
                    var vb = new VertexBuffer(device, board.Value.Count * Marshal.SizeOf(typeof(VertexType)), Usage.WriteOnly, VertexFormat.None, Pool.Managed);
                    var stream = vb.Lock(0, 0, LockFlags.None);

					FOutZ[slice0].SliceCount = board.Value.Count;

					int slice1 = 0;
					try
					{
						foreach (var point in board.Value)
						{
							var World = new Vector3((float)point.World.x, (float)point.World.y, (float)point.World.z);
							var Projection = new Color4((float)point.Projection.x / (float) projector.Value.Width, (float)point.Projection.y / (float) projector.Value.Height, 0.0f, 1.0f);

							stream.Write(new VertexType(World, Projection.ToArgb()));
							FOutZ[slice0][slice1] = point.World.z;

							slice1++;
						}
					}
					finally
					{
						vb.Unlock();
					}

					linesPerProjector.Add(new BoardVertexBuffer(board.Value.Count ,vb));
                }

				lines.Add(linesPerProjector);
				slice0++;
            }

            //return a new device data
            return new PreviewDeviceData(lines);
        }

        //this method gets called, when Update() was called in evaluate,
        //or a graphics device asks for its data, here you fill the resources with the actual data
        protected override void UpdateDeviceData(PreviewDeviceData deviceData)
        {
            //lock the vertexbuffer and get its data stream
            //var stream = deviceData.VB.Lock(0, 0, LockFlags.None);

			//here we don't do anything as it's all in reinit
        }

        //this is called by vvvv to delete the resources of a specific device data
        protected override void DestroyDeviceData(PreviewDeviceData deviceData, bool OnlyUnManaged)
        {
			foreach (var projector in deviceData.VertexBufferPerProjectorPerLine)
				foreach (var vb in projector)
					vb.VertexBuffer.Dispose();
			deviceData.VertexBufferPerProjectorPerLine.Clear();
        }

        #endregion device data handling

        //render into the vvvv renderer
        protected override void Render(Device device, PreviewDeviceData deviceData)
        {
            //enable simple sprite rendering
            device.SetRenderState(RenderState.PointSpriteEnable, true);
            device.SetRenderState(RenderState.PointScaleEnable, true);
            device.SetRenderState(RenderState.PointSize, 0.05f);

			//set vertex format
			device.VertexFormat = VertexType.Format;

			int slice = 0;
			foreach (var projector in deviceData.VertexBufferPerProjectorPerLine)
			{
				//set transform
				device.SetTransform(TransformState.World, FInTransform[slice]);

				//set render state from pin
				FRenderStatePin.SetSliceStates(slice);

				foreach (var board in projector)
				{
					//set vertex buffer
					device.SetStreamSource(0, board.VertexBuffer, 0, Marshal.SizeOf(typeof(VertexType)));

					//draw the geometry
					device.DrawPrimitives(PrimitiveType.LineStrip, 0, board.Count - 1);
				}

				slice++;
			}

        }
    }
}
