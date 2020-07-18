using PEPlugin.Pmx;
using PEPlugin.SDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutomaticUVUnwrapping
{
    class ABFPVertex : IPXVertex, ICloneable
    {
        public int ID { get; set; } = -1;
        public bool IsBorder { get; set; } = false;

        // IPXVertex
        public V3 Position { get; set; }
        public V3 Normal { get; set; }
        public V2 UV { get; set; }
        public V4 UVA1 { get; set; }
        public V4 UVA2 { get; set; }
        public V4 UVA3 { get; set; }
        public V4 UVA4 { get; set; }
        public IPXBone Bone1 { get; set; }
        public IPXBone Bone2 { get; set; }
        public IPXBone Bone3 { get; set; }
        public IPXBone Bone4 { get; set; }
        public float Weight1 { get; set; }
        public float Weight2 { get; set; }
        public float Weight3 { get; set; }
        public float Weight4 { get; set; }
        public bool QDEF { get; set; }
        public bool SDEF { get; set; }
        public V3 SDEF_C { get; set; }
        public V3 SDEF_R0 { get; set; }
        public V3 SDEF_R1 { get; set; }
        public float EdgeScale { get; set; }

        public ABFPVertex()
        {
            ID = -1;
            IsBorder = false;

            Position = new V3();
            Normal = new V3();
            UV = new V2();
            UVA1 = new V4();
            UVA2 = new V4();
            UVA3 = new V4();
            UVA4 = new V4();

            SDEF_C = new V3();
            SDEF_R0 = new V3();
            SDEF_R1 = new V3();
        }

        public ABFPVertex(IPXVertex vertex)
        {
            ID = -1;
            IsBorder = false;

            Position = vertex.Position;
            Normal = vertex.Normal;
            UV = vertex.UV;
            UVA1 = vertex.UVA1;
            UVA2 = vertex.UVA2;
            UVA3 = vertex.UVA3;
            UVA4 = vertex.UVA4;
            Bone1 = vertex.Bone1;
            Bone2 = vertex.Bone2;
            Bone3 = vertex.Bone3;
            Bone4 = vertex.Bone4;
            Weight1 = vertex.Weight1;
            Weight2 = vertex.Weight2;
            Weight3 = vertex.Weight3;
            Weight4 = vertex.Weight4;
            QDEF = vertex.QDEF;
            SDEF = vertex.SDEF;
            SDEF_C = vertex.SDEF_C;
            SDEF_R0 = vertex.SDEF_R0;
            SDEF_R1 = vertex.SDEF_R1;
            EdgeScale = vertex.EdgeScale;
        }

        /// <summary>
        /// シャローコピー
        /// </summary>
        /// <returns><c>ABFPVertex</c>型</returns>
        public object Clone()
        {
            var clone = new ABFPVertex();

            clone.ID = ID;
            clone.IsBorder = IsBorder;
            clone.Position = Position;
            clone.Normal = Normal;
            clone.UV = UV;
            clone.UVA1 = UVA1;
            clone.UVA2 = UVA2;
            clone.UVA3 = UVA3;
            clone.UVA4 = UVA4;
            clone.Bone1 = Bone1;
            clone.Bone2 = Bone2;
            clone.Bone3 = Bone3;
            clone.Bone4 = Bone4;
            clone.Weight1 = Weight1;
            clone.Weight2 = Weight2;
            clone.Weight3 = Weight3;
            clone.Weight4 = Weight4;
            clone.QDEF = QDEF;
            clone.SDEF = SDEF;
            clone.SDEF_C = SDEF_C;
            clone.SDEF_R0 = SDEF_R0;
            clone.SDEF_R1 = SDEF_R1;
            clone.EdgeScale = EdgeScale;

            return clone;
        }
    }

    class ABFVertex
    {
        public int SID { get; set; }
        public int UVID { get; set; }
        public int AngleID { get; set; }
        public ABFHalfEdge HEdge { get; set; }

        public ABFPVertex PVertex { get; set; }
        public V2 UV { get => PVertex.UV; }

        public ABFVertex(IPXVertex vertex)
        {
            PVertex = new ABFPVertex(vertex);

        }
    }

    class ABFEdge
    {
        public int ID { get; set; }
        public int[] SID { get; } = new int[2];
        public ABFPVertex[] Vertex { get; } = new ABFPVertex[2];
        public bool IsBorder { get; set; }
    }

    class ABFFace
    {
        public ABFHalfEdge[] HEdge { get; } = new ABFHalfEdge[3];
        public ABFVertex[] Vertices { get; } = new ABFVertex[3];
        public bool IsBorder { get; set; }
        public int AngleID { get; set; }
        public int ChartID { get; set; }
        public int ChartFIndex { get; set; }
        public int TriID { get; set; }
    }

    class ABFHalfEdge
    {
        public int SID { set; get; }
        public ABFEdge PEdge { set; get; }
        public int AngleID { set; get; }
        public ABFVertex Vertex { set; get; }
        public ABFHalfEdge Next { set; get; }
        public ABFHalfEdge Pair { set; get; }
    }

    class ABFSystem
    {
        public int InteriorCount { get; set; }
        public int FaceCount { get; set; }
        public int AngleCount { get; set; }
        public List<float> Alpha { get; } = new List<float>();
        public List<float> Beta { get; } = new List<float>();
        public List<float> Sine { get; } = new List<float>();
        public List<float> Cosine { get; } = new List<float>();
        public List<float> Weight { get; } = new List<float>();

        public List<float> BAlpha { get; } = new List<float>();
        public List<float> BTriangle { get; } = new List<float>();
        public List<float> BInterior { get; } = new List<float>();

        public List<float> LambdaTriangle { get; } = new List<float>();
        public List<float> LambdaPlanar { get; } = new List<float>();
        public List<float> LambdaLength { get; } = new List<float>();

        public List<float[]> J2dt { get; } = new List<float[]>();
        public List<float> BStar { get; } = new List<float>();
        public List<float> DStar { get; } = new List<float>();

        public float MinAngle { get; set; }
        public float MaxAngle { get; set; }

        public ABFSystem(int angleCount, int interiorCount, int faceCount)
        {
            AngleCount = angleCount;
            InteriorCount = interiorCount;
            FaceCount = faceCount;

            for (int i = 0; i < AngleCount; i++)
            {
                Alpha.Add(0);
                Beta.Add(0);
                Sine.Add(0);
                Cosine.Add(0);
                Weight.Add(0);
                BAlpha.Add(0);

                J2dt.Add(new float[3]);
            }

            for (int i = 0; i < InteriorCount * 2; i++)
            {
                BInterior.Add(0);
            }

            for (int i = 0; i < FaceCount; i++)
            {
                LambdaTriangle.Add(0);
                BTriangle.Add(0);

                BStar.Add(0);
                DStar.Add(0);
            }

            for (int i = 0; i < InteriorCount; i++)
            {
                LambdaPlanar.Add(0);
                LambdaLength.Add(1);
            }

            MinAngle = 7.5f * (float)Math.PI / 180.0f;
            MaxAngle = (float)Math.PI - MinAngle;
        }
    }

    class ABFChart
    {
        public List<ABFVertex> Vertices { get; private set; }
        public List<ABFFace> Faces { get; } = new List<ABFFace>();
        public ABFVertex Pin1 { get; set; }
        public ABFVertex Pin2 { get; set; }
        public List<ABFHalfEdge> AllhEdges { get; } = new List<ABFHalfEdge>();
        public List<float> Alpha { get; } = new List<float>();
        public NLContext Context { get; set; }

        public bool LSCMMode { get; set; } = false;

        public ABFChart(IPXPmx pmx)
        {
            Vertices = pmx.Vertex.Select(v => new ABFVertex(v)).ToList();
        }
    }
}
