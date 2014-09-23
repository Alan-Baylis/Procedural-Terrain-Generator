using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class River : MonoBehaviour {
	public Texture2D riverTexture;
	public float terrainSize;
	public float heightMapSize;
	public float waterlimit;

	public void drawRivers(List<Corner> Corner) {
		bool[] rivermade=new bool[Corner.Count];

		foreach (Corner q in Corner) {
			if (q.river > 0 && q.watershed.coast && !q.coast && !rivermade[q.index]) {
				int i = 0;
				foreach (Corner t in q.adjacent)
					if (t.river>0)
							i++;
				if (i == 1) {
					List<Spline> oneriver = new List<Spline> ();
					Corner nq = q.downslope;
					Spline spp = new Spline ();
					spp.point = new Vector2(2*q.point.x-nq.point.x, 2*q.point.y-nq.point.y);
					spp.volume =1;
					oneriver.Add (spp);
					nq=q;
					while (nq.elevation>=waterlimit-0.01f && !rivermade[nq.index]) {
						Spline spl = new Spline ();
						spl.point = nq.point;
						spl.volume = nq.river;
						oneriver.Add (spl);
						rivermade[nq.index]=true;
						nq = nq.downslope;
					}
					Spline sp = new Spline ();
					sp.point = nq.point;
					sp.volume = nq.river;
					oneriver.Add (sp);

//					Spline sp1 = new Spline ();
//					sp1.point = nq.downslope.point;
//					sp1.volume = nq.river;
//					oneriver.Add (sp1);
					if(!rivermade[nq.index]){
						rivermade[nq.index]=true;
						Spline spe = new Spline ();
						spe.point = new Vector2(2*nq.downslope.point.x-nq.point.x, 2*nq.downslope.point.y-nq.point.y);
						spe.volume =1;
						oneriver.Add (spe);
					} 
					else
					{					
						Spline sp1 = new Spline ();
						sp1.point = nq.downslope.point;
						sp1.volume = nq.river;
						oneriver.Add (sp1);
					}

					Generate(oneriver.ToArray(),0.2f);
//					if (oneriver.Count>=4)
//						break;
				}
			}
		}
	}
	
	public Vector2[] calculateEdgePoints(Vector2 a, Vector2 b, float dist)
	{
		float dx = b.x - a.x;
		float dy = b.y - a.y;
		float p = dist/ Mathf.Pow (dx * dx + dy * dy, 0.5f);
		Vector2[] Points = new Vector2[2];
		Points [0].x = a.x - dy * p;
		Points [0].y = a.y + dx * p;
		Points [1].x = a.x + dy * p;
		Points [1].y = a.y - dx * p;
		return Points;
	}
	public Vector2[] edgePointsOnCurve(Spline[] SplinePoints)
	{
		Vector2[] EdgePoints = new Vector2[SplinePoints.Length*2];	
		for (int i=1; i<SplinePoints.Length; i++) {
			Vector2[] p=calculateEdgePoints(SplinePoints[i-1].point,SplinePoints[i].point,SplinePoints[i-1].volume*0.01f);
			EdgePoints[2*i-2]=p[0];
			EdgePoints[2*i-1]=p[1];
		}
		Vector2[] pp=calculateEdgePoints(SplinePoints[SplinePoints.Length-1].point,SplinePoints[SplinePoints.Length-2].point,SplinePoints[SplinePoints.Length-1].volume*0.01f);
		EdgePoints[2*SplinePoints.Length-1]=pp[0];
		EdgePoints[2*SplinePoints.Length-2]=pp[1];
		return EdgePoints;
	}

	public Vector2 pointOnCurve(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
	{
		Vector2 ret = new Vector2();
		
		float t2 = t * t;
		float t3 = t2 * t;
		
		ret.x = 0.5f * ((2.0f * p1.x) +
		                (-p0.x + p2.x) * t +
		                (2.0f * p0.x - 5.0f * p1.x + 4 * p2.x - p3.x) * t2 +
		                (-p0.x + 3.0f * p1.x - 3.0f * p2.x + p3.x) * t3);
		
		ret.y = 0.5f * ((2.0f * p1.y) +
		                (-p0.y + p2.y) * t +
		                (2.0f * p0.y - 5.0f * p1.y + 4 * p2.y - p3.y) * t2 +
		                (-p0.y + 3.0f * p1.y - 3.0f * p2.y + p3.y) * t3);
		
		return ret;
	}

	public void Generate(Spline[] points, float dist)
	{
				if (points.Length >= 4) {
//				throw new ArgumentException("CatmullRomSpline requires at least 4 points", "points");
		
						List<Spline> splinePoints = new List<Spline> ();
						for (int i = 0; i < points.Length - 3; i++) {
								float dist2 = Mathf.Pow ((points [i + 1].point.x - points [i + 2].point.x), 2) + Mathf.Pow ((points [i + 1].point.y - points [i + 2].point.y), 2);
								dist2 = Mathf.Pow (dist2, 0.5f);
								int numPoints = Mathf.Max((int)(dist2 / dist),1);
								for (int j = 0; j< numPoints; j++) {
										Spline sp = new Spline ();
										sp.point = pointOnCurve (points [i].point, points [i + 1].point, points [i + 2].point, points [i + 3].point, (1f/numPoints) * j);
										sp.volume = ((points [i + 2].volume - points [i + 1].volume) * j / numPoints) + points [i + 1].volume;
										splinePoints.Add (sp);
								}
						}
						Spline spl = new Spline ();
						spl.point = points [points.Length - 2].point;
						spl.volume = points [points.Length - 2].volume;
						splinePoints.Add (spl);
						Vector2[] edgePoints = edgePointsOnCurve (splinePoints.ToArray ());
//						Vector2[]edgePoints=new Vector2[splinePoints.Count];
//			for (int i=0; i<splinePoints.Count;i++)
//			{
//				edgePoints[i]=splinePoints[i].point;
//			}
//			Vector2[]edgePoints=new Vector2[points.Length];
//			for (int i=0; i<points.Length;i++)
//			{
//				edgePoints[i]=points[i].point;
//			}

			for (int i=0; i<edgePoints.Length; i++){
				edgePoints[i].x  *= (float)terrainSize / heightMapSize;
				edgePoints[i].y *= (float)terrainSize / heightMapSize;
			}


			PathGenerator2 pathGenerator = new PathGenerator2();
			pathGenerator.texture = riverTexture;
			pathGenerator.height = 0.3f;
			pathGenerator.generate (new List<Vector2>(edgePoints));



//			for(int i=1;i<edgePoints.Length;i++)
//			{
//				
//				float beginY= edgePoints[i].x;// * terrainSize / heightMapSize;// - terrainSize/2;
//				float beginX = edgePoints[i].y;// * terrainSize / heightMapSize;//- terrainSize/2;
//				float endY = edgePoints[i-1].x;// * terrainSize / heightMapSize;//-terrainSize/2;
//				float endX = edgePoints[i-1].y;// * terrainSize / heightMapSize;//- terrainSize/2;
//				
//				Debug.DrawLine (new Vector3 (beginX, Terrain.activeTerrain.SampleHeight(new Vector3(beginX,0.0f,beginY)) + 10.0f, beginY), new Vector3 (endX, Terrain.activeTerrain.SampleHeight(new Vector3(endX, 0.0f, endY))+10.0f, endY), Color.red, 1000.0f);
//			}
				
				}

		}
//	public void bla()
//	{
//		foreach (Edge edge in p_graphVoronoi.edges)
//		if (edge.river != 0) {
//			
//			float beginX = edge.v0.point.x * terrainSizeX / heightMapSize - terrainSizeX/2;
//			float beginY = edge.v0.point.y * terrainSizeY / heightMapSize - terrainSizeY/2;
//			float endX = edge.v1.point.x * terrainSizeX / heightMapSize - terrainSizeX/2;
//			float endY = edge.v1.point.y * terrainSizeY / heightMapSize - terrainSizeY/2;
//			
//			Debug.DrawLine (new Vector3 (beginX, Terrain.activeTerrain.SampleHeight(new Vector3(beginX,0.0f,beginY)) + 10.0f, beginY), new Vector3 (endX, Terrain.activeTerrain.SampleHeight(new Vector3(endX, 0.0f, endY))+10.0f, endY), Color.red, 1000.0f);
//		}
//	}
	
}
