using PEPlugin.SDX;
using PEPExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomaticUVUnwrapping
{
    class UVUnwrapper
    {
        const int ABF_MAX_ITER = 15;

        /// <summary>
        /// ベクトルの成す角を返す
        /// </summary>
        /// <param name="v1">先端1</param>
        /// <param name="v2">対象点</param>
        /// <param name="v3">先端2</param>
        /// <returns>点v2における角度</returns>
        static float ABFVectorAngle(ABFVertex v1, ABFVertex v2, ABFVertex v3)
        {
            V3 d1, d2;
            d1 = (v1.PVertex.Position - v2.PVertex.Position);
            d1.Normalize();
            d2 = (v3.PVertex.Position - v2.PVertex.Position);
            d2.Normalize();

            var cos = d1.Dot(d2) / (d1.LengthSq() * d2.LengthSq());
            return (float)Math.Acos(cos);
        }

        void GetAngles(ABFFace f,out float a1,out float a2, out float a3)
        {
            var e1 = f.HEdge[0];
            var e2 = e1.Next;
            var e3 = e2.Next;

            var v1 = e1.Vertex;
            var v2 = e2.Vertex;
            var v3 = e3.Vertex;

            a1 = ABFVectorAngle(v3, v1, v2);
            a2 = ABFVectorAngle(v1, v2, v3);
            a3 = (float)Math.PI - a2 - a1;
        }

        static void ABFComputeSines(ABFSystem sys)
        {
            for (int i = 0; i < sys.AngleCount; i++)
            {
                sys.Sine[i] = (float)Math.Sin(sys.Alpha[i]);
                sys.Cosine[i] = (float)Math.Cos(sys.Alpha[i]);
            }
        }

        static float ABFComputeSinProduct(ABFSystem sys, ABFVertex v, int aid)
        {
            ABFHalfEdge e, e1, e2;
            float sin1, sin2;

            sin1 = sin2 = 1f;

            e = v.HEdge;

            do
            {
                e1 = e.Next;
                e2 = e.Next.Next;

                if (aid == e1.AngleID)
                {
                    /* we are computing a derivative for this angle,
                       so we use cos and drop the other part */
                    sin1 *= sys.Cosine[e1.AngleID];
                    sin2 = 0f;
                }
                else
                    sin1 *= sys.Sine[e1.AngleID];

                if (aid == e2.AngleID)
                {
                    /* see above */
                    sin1 = 0f;
                    sin2 *= sys.Cosine[e2.AngleID];
                }
                else
                    sin2 *= sys.Sine[e2.AngleID];

                e = e.Next.Next.Pair;
            } while (e != v.HEdge);

            return (sin1 - sin2);
        }

        static float ABFComputeGradAlpha(ABFSystem sys, ABFFace f, ABFHalfEdge e)
        {
            ABFVertex v = e.Vertex, v1 = e.Next.Vertex, v2 = e.Next.Next.Vertex;

            float deriv = (sys.Alpha[e.AngleID] - sys.Beta[e.AngleID]) * sys.Weight[e.AngleID];
            deriv += sys.LambdaTriangle[f.AngleID];

            if (!v.PVertex.IsBorder)
                deriv += sys.LambdaPlanar[v.AngleID];


            if (!v1.PVertex.IsBorder)
            {
                float product = ABFComputeSinProduct(sys, v1, e.AngleID);
                deriv += sys.LambdaLength[v1.AngleID] * product;
            }

            if (!v2.PVertex.IsBorder)
            {
                float product = ABFComputeSinProduct(sys, v2, e.AngleID);
                deriv += sys.LambdaLength[v2.AngleID] * product;
            }

            return deriv;
        }

        static float ABFComputeGradient(ABFSystem sys, ABFChart chart)
        {

            ABFHalfEdge e;
            float norm = 0f;

            for (int i = 0; i < chart.Faces.Count; i++)
            {

                ABFHalfEdge e1 = chart.Faces[i].HEdge[0], e2 = e1.Next, e3 = e2.Next;

                float gtriangle, galpha1, galpha2, galpha3;

                galpha1 = ABFComputeGradAlpha(sys, chart.Faces[i], e1);
                galpha2 = ABFComputeGradAlpha(sys, chart.Faces[i], e2);
                galpha3 = ABFComputeGradAlpha(sys, chart.Faces[i], e3);


                sys.BAlpha[e1.AngleID] = -galpha1;
                sys.BAlpha[e2.AngleID] = -galpha2;
                sys.BAlpha[e3.AngleID] = -galpha3;

                norm += galpha1 * galpha1 + galpha2 * galpha2 + galpha3 * galpha3;


                gtriangle = sys.Alpha[e1.AngleID] + sys.Alpha[e2.AngleID] + sys.Alpha[e3.AngleID] - (float)Math.PI;
                sys.BTriangle[chart.Faces[i].AngleID] = -gtriangle;

                norm += gtriangle * gtriangle;
            }

            for (int i = 0; i < chart.Vertices.Count; i++)
            {
                if (!chart.Vertices[i].PVertex.IsBorder)
                {
                    float gplanar = -2.0f * (float)Math.PI, glength;
                    e = chart.Vertices[i].HEdge;

                    do
                    {
                        gplanar += sys.Alpha[e.AngleID];
                        e = e.Next.Next.Pair;

                    } while (e != chart.Vertices[i].HEdge);

                    sys.BInterior[chart.Vertices[i].AngleID] = -gplanar;

                    norm += gplanar * gplanar;

                    glength = ABFComputeSinProduct(sys, chart.Vertices[i], -1);
                    sys.BInterior[sys.InteriorCount + chart.Vertices[i].AngleID] = -glength;

                    norm += glength * glength;
                }
            }

            return norm;
        }

        static bool ABFMatrixInvert(ABFSystem sys, ABFChart chart)
        {
            throw new NotImplementedException();
        }

        public void ABFSolve(ABFChart chart)
        {
            float limit = (chart.Faces.Count > 100) ? .1f : 0.001f;

            int nInterior = 0;
            int nFaces = 0;
            int nAngles = 0;

            // 内陸頂点数を集計しつつ、角IDを付与
            foreach (var vertex in chart.Vertices.Where(vertex => !vertex.PVertex.IsBorder))
            {
                vertex.AngleID = nInterior++;
            }

            // 面数・辺数を集計しつつ、面とそれを成す辺のAngleIDを付与
            foreach (ABFFace face in chart.Faces)
            {
                face.AngleID = nFaces++;
                face.HEdge[0].AngleID = nAngles++;
                face.HEdge[0].Next.AngleID = nAngles++;
                face.HEdge[0].Next.Next.AngleID = nAngles++;
            }

            ABFSystem sys = new ABFSystem(nAngles, nInterior, nFaces);

            // SystemのAlphaとWeightを初期化
            foreach (ABFFace face in chart.Faces)
            {
                float angle1, angle2, angle3;
                ABFHalfEdge edge1, edge2, edge3;
                edge1 = face.HEdge[0];
                edge2 = edge1.Next;
                edge3 = edge2.Next;
                GetAngles(face, out angle1, out angle2, out angle3);

                // angleをSystemの最大最小に丸める
                if (angle1 < sys.MinAngle)
                    angle1 = sys.MinAngle;
                else if (angle1 > sys.MaxAngle)
                    angle1 = sys.MaxAngle;

                if (angle2 < sys.MinAngle)
                    angle2 = sys.MinAngle;
                else if (angle2 > sys.MaxAngle)
                    angle2 = sys.MaxAngle;

                if (angle3 < sys.MinAngle)
                    angle3 = sys.MinAngle;
                else if (angle3 > sys.MaxAngle)
                    angle3 = sys.MaxAngle;

                // SystemのAlphaとWeightを初期化
                sys.Alpha[edge1.AngleID] = sys.Beta[edge1.AngleID] = angle1;
                sys.Alpha[edge2.AngleID] = sys.Beta[edge2.AngleID] = angle2;
                sys.Alpha[edge3.AngleID] = sys.Beta[edge3.AngleID] = angle3;

                sys.Weight[edge1.AngleID] = 2f / (angle1 * angle1);
                sys.Weight[edge2.AngleID] = 2f / (angle2 * angle2);
                sys.Weight[edge3.AngleID] = 2f / (angle3 * angle3);
            }

            //LSCM
            if (chart.LSCMMode)
            {
                chart.Alpha.Clear();
                chart.Alpha.AddRange(sys.Alpha);
                return;
            }

            //ABF++
            foreach (ABFVertex vertex in chart.Vertices)
            {
                if (!vertex.PVertex.IsBorder)
                {
                    float angleSum = 0.0f;
                    float scale;

                    ABFHalfEdge e = vertex.HEdge;
                    do
                    {
                        angleSum += sys.Beta[e.AngleID];
                        e = e.Next.Next.Pair;
                    } while (e != vertex.HEdge);

                    scale = (angleSum == 0.0f) ? 0.0f : 2.0f * (float)Math.PI / angleSum;

                    e = vertex.HEdge;
                    do
                    {
                        sys.Beta[e.AngleID] = sys.Alpha[e.AngleID] = sys.Beta[e.AngleID] * scale;
                        e = e.Next.Next.Pair;
                    } while (e != vertex.HEdge);
                }
            }

            if(sys.InteriorCount > 0)
            {
                ABFComputeSines(sys);

                // iteration
                float lastnorm = 1e10f;

                for (int i=0;i< ABF_MAX_ITER; i++)
                {
                    float norm = ABFComputeGradient(sys, chart);
                    lastnorm = norm;

                    if (norm < limit)
                        break;

                    if (!ABFMatrixInvert(sys, chart))
                    {
                        throw new FindInverseMatrixException("逆行列の計算に失敗しました。");
                    }

                    ABFComputeSines(sys);
                }
            }

            chart.Alpha.Clear();
            chart.Alpha.AddRange(sys.Alpha);
        }

        // pinning に必要だがpinningが必要かわからないので放置する
        //void solvParam(ABFChart chart)
        //{
        //    chart.Context = new NLContext();
        //    chart.Context.Variables = new NLVariable[2 * chart.Vertices.Count];
        //    chart.Context.Rows = 2 * (uint)chart.Faces.Count;
        //    chart.Context.LeastSquares = true;
        //}
    }
}
