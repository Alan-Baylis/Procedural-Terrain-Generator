using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
public class Roads {
	// The road array marks the edges that are roads.  The mark is 1,
	// 2, or 3, corresponding to the three contour levels. Note that
	// these are sparse arrays, only filled in where there are roads.
	public int[] road; // edge index -> int contour level
	public List<Edge>[] roadConnection;// center index -> array of Edges with roads
	public Texture2D roadTexture;
	//public terrainGen map;
	public Roads(terrainGen map) {
	//	this.map = map;
		road = new int[map.edges.Count]; 
		roadConnection = new List<Edge>[map.centers.Count]; 
		roadTexture = map.road;
	}
	public float terrainSize;
	public float heightMapSize;
	
	// We want to mark different elevation zones so that we can draw
	// island-circling roads that divide the areas.
	public void createRoads(terrainGen map) {
				// Oceans and coastal polygons are the lowest contour zone
				// (1). Anything connected to contour level K, if it's below
				// elevation threshold K, or if it's water, gets contour level
				// K.  (2) Anything not assigned a contour level, and connected
				// to contour level K, gets contour level K+1.
		terrainSize = map.m_terrainSize;
		heightMapSize = map.m_heightMapSize;


				List<Center> queue = new List<Center> ();
				//var p:Center, q:Corner, r:Center, edge:Edge, newLevel:int;
				float[] elevationThresholds = {0.0f, 0.15f, 0.28f, 0.45f, 0.65f};
				elevationThresholds [1] = map.waterLimit + 0.02f;
				int[] cornerContour = new int[map.corners.Count];  // corner index -> int contour level
				int[] centerContour = new int[map.centers.Count]; // center index -> int contour level
				bool[] roadMade = new bool[map.centers.Count];
				foreach (Center p in map.centers) {
						if (p.coast || p.ocean) {
								centerContour [p.index] = 1;
								queue.Add (p);
						}
				}

				while (queue.Count>0) {
						Center p = queue [0];
						queue.RemoveAt (0);

						foreach (Center r in p.neighbors) {
								int newLevel = centerContour [p.index] != 0 ? centerContour [p.index] : 0;
								while (r.elevation > elevationThresholds[newLevel] && !r.water && newLevel<4) {
										// NOTE: extend the contour line past bodies of
										// water so that roads don't terminate inside lakes.
										newLevel += 1;
								}
								if ((centerContour [r.index] != 0 && newLevel < centerContour [r.index]) || centerContour [r.index] == 0) {
										centerContour [r.index] = newLevel;
										queue.Add (r);
								}
						}
						//	queue.RemoveAt(0);
				}
		
				// A corner's contour level is the MIN of its polygons
				foreach (Center p in map.centers) {
						foreach (Corner q in p.corners) {
								cornerContour [q.index] = Mathf.Min (cornerContour [q.index] != 0 ? cornerContour [q.index] : 999,
				                                   centerContour [p.index] != 0 ? centerContour [p.index] : 999);
						}
				}
		int a = 100, b = -100; bool boolean = true;
				// Roads go between polygons that have different contour levels
				foreach (Center p in map.centers) {
						//Debug.Log ("indeks centra" + p.index+ "indexi edgeva");
						foreach (Edge edge in p.borders) {
								if (edge.v0 != null && edge.v1 != null
										&& cornerContour [edge.v0.index] != cornerContour [edge.v1.index]) {


										road [edge.index] = Mathf.Min (cornerContour [edge.v0.index],
					                             cornerContour [edge.v1.index]);
										if (roadConnection [p.index] == null) {
												roadConnection [p.index] = new List<Edge> ();
										}










										//OVDE SE MENJAAAAA
										Heads h;
										if ((h = inTown (Field.cc, edge.d0)) !=null){
											edge.d0.point = change(h.roadPoints, edge.d0);
										}
										if ((h = inTown (Field.cc, edge.d1)) !=null){
											edge.d1.point = change (h.roadPoints, edge.d1);
										}
										roadConnection[p.index].Add(edge);





										//Debug.Log(" "+edge.index);
										if (road [edge.index] < a)
												a = road [edge.index];
										if (road [edge.index] > b)
												b = road [edge.index];
//										float beginY = edge.d0.point.x * map.m_terrainSize / map.m_heightMapSize;
//										float beginX = edge.d0.point.y * map.m_terrainSize / map.m_heightMapSize;
//										float endY = edge.d1.point.x * map.m_terrainSize / map.m_heightMapSize;
//										float endX = edge.d1.point.y * map.m_terrainSize / map.m_heightMapSize;
//										Debug.DrawLine (new Vector3 (beginX, Terrain.activeTerrain.SampleHeight (new Vector3 (beginX, 0, beginY)) + 10.0f, beginY), new Vector3 (endX, Terrain.activeTerrain.SampleHeight (new Vector3 (endX, 0, endY)) + 10.0f, endY), Color.red, 350.0f);
								}
								//Debug.Log ("indeks centra" + p.index);

						}
				}
				
				foreach (Center p in map.centers) {
						if (roadConnection [p.index] != null && roadConnection [p.index].Count == 2 && !roadMade [p.index]) {
								List<Vector2> oneRoad = new List<Vector2> ();
								Edge nEdge = roadConnection [p.index] [0];
								
								Center nq, q = p;
								if (nEdge.d0 == p)
										nq = nEdge.d1;
								else
										nq = nEdge.d0;
								oneRoad.Add (p.point);
								oneRoad.Add (nq.point);
								int hr = road [nEdge.index];
								while (nq!=p && nq!=q && !roadMade[nq.index]) {
										q = nq;
										foreach (Edge ed in roadConnection[q.index]) {
												if (hr == road [ed.index] && nEdge != ed) {
														if (q == ed.d0)
																nq = ed.d1;
														else
																nq = ed.d0;
														oneRoad.Add (nq.point);
														roadMade [q.index] = true;
														nEdge = ed;
														break;
												}
										}
								}
								roadMade [p.index] = true;
								if (nq == p) {
										oneRoad.Add (oneRoad [1]);
										oneRoad.Add(oneRoad[2]);
										//dodaj sporedni putic i iscrtaj
								} else {
										Vector2 endp = oneRoad [oneRoad.Count - 2];
										oneRoad.Add (new Vector2 (2 * q.point.x - endp.x, 2 * q.point.y - endp.y));
										nEdge = roadConnection [p.index] [1];
										if (nEdge.d0 == p)
												nq = nEdge.d1;
										else
												nq = nEdge.d0;
										q=p;
										oneRoad.Insert (0, nq.point);
										while (nq!=q && !roadMade[nq.index]) {
												q = nq;
												foreach (Edge ed in roadConnection[q.index]) {
														if (hr == road [ed.index] && nEdge != ed) {
																if (q == ed.d0)
																		nq = ed.d1;
																else
																		nq = ed.d0;
																oneRoad.Insert (0, nq.point);
																roadMade [q.index] = true;
																nEdge = ed;
																break;
														}
												}
										}
										endp = oneRoad [1];
										oneRoad.Insert (0, new Vector2 (2 * q.point.x - endp.x, 2 * q.point.y - endp.y));
										//dodaj putic i iscrtaj
										//Debug.Log (oneRoad.Count);

								}
								Generate(oneRoad.ToArray(),5-hr,0.3f);
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
		public Vector2[] edgePointsOnCurve(Vector2[] SplinePoints, int volume)
		{
			Vector2[] EdgePoints = new Vector2[SplinePoints.Length*2];	
			for (int i=1; i<SplinePoints.Length; i++) {
				Vector2[] p=calculateEdgePoints(SplinePoints[i-1],SplinePoints[i],volume*0.1f);
				EdgePoints[2*i-2]=p[0];
				EdgePoints[2*i-1]=p[1];
			}
			Vector2[] pp=calculateEdgePoints(SplinePoints[SplinePoints.Length-1],SplinePoints[SplinePoints.Length-2],volume*0.1f);
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
		
	public void Generate(Vector2[] points,int volume, float dist)
	{
		if (points.Length >= 4) {
			//				throw new ArgumentException("CatmullRomSpline requires at least 4 points", "points");
			
			List<Vector2> splinePoints = new List<Vector2> ();
			for (int i = 0; i < points.Length - 3; i++) {
				float dist2 = Mathf.Pow ((points [i + 1].x - points [i + 2].x), 2) + Mathf.Pow ((points [i + 1].y - points [i + 2].y), 2);
				dist2 = Mathf.Pow (dist2, 0.5f);
				int numPoints = Mathf.Max ((int)(dist2 / dist), 1);
				for (int j = 0; j< numPoints; j++) {
					splinePoints.Add (pointOnCurve (points [i], points [i + 1], points [i + 2], points [i + 3], (1f / numPoints) * j));
					
				}
			}
			splinePoints.Add (points [points.Length - 2]);
			
			Vector2[] edgePoints = edgePointsOnCurve (splinePoints.ToArray (), volume);
			
			for (int i=0; i<edgePoints.Length; i++){
				edgePoints[i].x  *= (float)terrainSize / heightMapSize;
				edgePoints[i].y *= (float)terrainSize / heightMapSize;
			}
			
			PathGenerator2 pathGenerator = new PathGenerator2();
			pathGenerator.texture = roadTexture;
			pathGenerator.height = 0.5f;
			pathGenerator.generate (new List<Vector2>(edgePoints));
			
			//						for (int ii=1; ii<edgePoints.Length; ii++) {
			//								float beginY = edgePoints [ii].x * terrainSize / heightMapSize;// - terrainSize/2;
			//								float beginX = edgePoints [ii].y * terrainSize / heightMapSize;//- terrainSize/2;
			//								float endY = edgePoints [ii - 1].x * terrainSize / heightMapSize;//-terrainSize/2;
			//								float endX = edgePoints [ii - 1].y * terrainSize / heightMapSize;
			//								Debug.DrawLine (new Vector3 (beginX, Terrain.activeTerrain.SampleHeight (new Vector3 (beginX, 0.0f, beginY)) + 100.0f, beginY), new Vector3 (endX, Terrain.activeTerrain.SampleHeight (new Vector3 (endX, 0.0f, endY)) + 100.0f, endY), Color.yellow, 1000.0f);
			//						}
			
		}
		
		
	}
	//kordinate za proveru puteva
	

	public bool inElem(Elem e, Center c){

		float limit = Field.tile * CityPicker.ratio;

		for (int i=0;i<limit;i++){
			for (int j=0;j<limit;j++){

				float x = e.p.y*limit - (i + limit/2);
				float y = e.p.x*limit - (j + limit/2);

				if (c.point.y*CityPicker.ratio==x && c.point.x*CityPicker.ratio==y)
					return true;
			}
		}

		return false;
	}

	public Heads inTown(List<Heads> l, Center c){


		foreach (Heads h in l){
			foreach(Elem e in h.elems){
				if (inElem(e,c)){
					return h;
				}
			}
		}
		return null;
	}

	public Vector2 change(Vector3[] roadPoints, Center c){
		float min = 0;
		Vector2 v = Vector2.zero;

		if (roadPoints != null)
			for (int i=0;i<roadPoints.Length;i++){
				float r = Mathf.Sqrt(Mathf.Pow (c.point.y * CityPicker.ratio - roadPoints[i].x , 2) + Mathf.Pow (c.point.x * CityPicker.ratio - roadPoints[i].z, 2));	
				if (min==0 || r<min){
					min=r;
					v = new Vector2(roadPoints[i].z/CityPicker.ratio, roadPoints[i].x/CityPicker.ratio);
				}
			}
		return v;

	}

}


	