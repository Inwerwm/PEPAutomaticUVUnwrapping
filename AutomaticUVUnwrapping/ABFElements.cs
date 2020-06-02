using PEPlugin.Pmx;
using PEPlugin.SDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomaticUVUnwrapping
{
    class ABFPVertex
    {
        public int ID { get; private set; }
        public IPXVertex Vertex { get; private set; }
        public bool IsBorder { get; set; }
    }

    class ABFVertex
    {
        public int SID { get; set; }
        public int UVID { get; set; }
        public int AngleID { get; set; }
        public ABFhEdge HEdge { get; set; }

        public V2 UV { get; private set; }
        public ABFPVertex PVertex { get; set; }
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
        public ABFhEdge[] HEdge { get; } = new ABFhEdge[3];
        public ABFVertex[] Vertices { get; } = new ABFVertex[3];
        public bool IsBorder{get;set;}
        public int AngleID{get;set;}
        public int ChartID{get;set;}
        public int ChartFIndex{get;set;}
        public int TriID{get;set;}
    }

    class ABFhEdge
    {
        public int SID{set;get;}
        public ABFEdge PEdge{set;get;}
        public int AngleID{set;get;}
        public ABFVertex Vertices{set;get;}
        public ABFhEdge Next{set;get;}
        public ABFhEdge Pair { set; get; }
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
    }

    class ABFChart
    {
        public List<ABFVertex> Vertices { get; } = new List<ABFVertex>();
        public List<List<ABFFace>> Faces { get; } = new List<List<ABFFace>>();
        public int VertexCount { get; set; }
        public int EdgeCount { get; set; }
        public int FaceCount { get; set; }
        public ABFVertex Pin1 { get; set; }
        public ABFVertex Pin2 { get; set; }
        public List<ABFhEdge> AllhEdges { get; } = new List<ABFhEdge>();
        public List<float> Alpha { get; } = new List<float>();
    }
}
