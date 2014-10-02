using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
public class VectorSet{
	
	public Vector2 a{ get; set; }
	public Vector2 b{ get; set; }
	public VectorSet(Vector2 aa, Vector2 bb)
	{
		a = aa;
		b = bb;
	}
}
//public class Spline : MonoBehaviour {
//	
//	public Vector2 point;
//	public float volume;
//	
//}
public class DrawRoads : MonoBehaviour {
	public float terrainSize;
	public float heightMapSize;
	public Texture2D roadTexture;
	public List<VectorSet> used;
	public Shader pathShader;
	Elem towncenter;
	public DrawRoads(float ts, float hms, Texture2D text) {
		terrainSize = ts;
		heightMapSize = hms;
		roadTexture = text;
		used=new List<VectorSet>();
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
	public void createRoads(List<Vector2> roadp) {
		List<Vector2> oneRoad = new List<Vector2> ();
		Elem h,p;

		for (int i=1; i<roadp.Count; i++) {

			if ((h = inTown (Field.cc, roadp [i - 1])) != null) {
				if ((p = inTown (Field.cc, roadp [i])) == null)
				{
					oneRoad=new List<Vector2>();
					oneRoad.Add (change(roadp [i],h));
				}

			}
			else
			{
				if ((p = inTown (Field.cc, roadp[i])) != null){
					oneRoad.Add(roadp[i-1]);
					oneRoad.Add (change(roadp [i-1],p));
					if (oneRoad.Count>1){
						Vector2 start = new Vector2 (2 * oneRoad[0].x - oneRoad [1].x, 2 * oneRoad [0].y - oneRoad [1].y);
						Vector2 end = new Vector2 (2 * oneRoad[oneRoad.Count - 1].x - oneRoad [oneRoad.Count - 2].x, 2 * oneRoad [oneRoad.Count - 1].y - oneRoad [oneRoad.Count - 2].y);
						oneRoad.Add (end);
						oneRoad.Insert (0, start);
						generate(oneRoad);
						oneRoad=new List<Vector2>();
					}

				}
				else
					oneRoad.Add(roadp[i-1]);
			}
		}
		if (oneRoad.Count >0) {
			if ((h = inTown (Field.cc, roadp[roadp.Count-1])) != null)
				oneRoad.Add (change(roadp [roadp.Count-2],h));
			else
				oneRoad.Add(roadp[roadp.Count-1]);

			Vector2 start = new Vector2 (2 * oneRoad [0].x - oneRoad [1].x, 2 * oneRoad [0].y - oneRoad [1].y);
			Vector2 end = new Vector2 (2 * oneRoad [oneRoad.Count - 1].x - oneRoad [oneRoad.Count - 2].x, 2 * oneRoad [oneRoad.Count - 1].y - oneRoad [oneRoad.Count - 2].y);
			oneRoad.Add (end);
			oneRoad.Insert (0, start);
			generate (oneRoad);
		}
	}
	public bool inList(List<VectorSet> llist, Vector2 aa, Vector2 bb)
	{

		for (int i=0; i<llist.Count;i++) {
			if (aa == llist[i].a && bb == llist[i].b)
								return true;
						if (aa == llist[i].b && bb == llist[i].a)
								return true;
				}
				return false;
		}
	public void generate(List<Vector2> road)
	{
		List<Vector2> oneRoad = new List<Vector2> ();
		oneRoad.Add (road [0]);
		oneRoad.Add (road [1]);
		for (int i=2; i<road.Count; i++) {
						oneRoad.Add (road [i]);
			if (inList(used, road[i], road[i-1])) {
								generate2 (oneRoad.ToArray(), 50, 5f);
								oneRoad = new List<Vector2> ();
								oneRoad.Add (road [i - 1]);
								oneRoad.Add (road [i]);
						}
			else {
				used.Add (new VectorSet (road [i - 1], road [i]));
						}
				}
		generate2 (oneRoad.ToArray(), 50, 5f);
	}
	public void generate2(Vector2[] points,int volume, float dist)
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
			
			PathGenerator2 pathGenerator = new PathGenerator2();
			pathGenerator.pathShader = pathShader;
			pathGenerator.texture = roadTexture;
			pathGenerator.height = 0.5f;
			pathGenerator.generate (new List<Vector2>(edgePoints));
		}

	}
	public Elem inElem(Elem e, Vector2 c){
		
		float limit = Field.tile*CityPicker.ratio;
		float xx = (e.p.y - 1) * limit;
		float yy = (e.p.x - 1) * limit;
		if (c.x >= xx - limit / 2f && c.x <= xx + limit / 2f && c.y >= yy - limit / 2f && c.y <= yy + limit / 2f)
						return e;
		return null;
	}
	
	public Elem inTown(List<Heads> l, Vector2 c){

		foreach (Heads h in l){
			if(h.elems.Count>CityPicker.villageSize)
			foreach(Elem e in h.elems){
				Elem p=inElem(e,c);
				if (p!=null){
					return p;
				}
			}
		}
		return null;
	}
	
	public Vector2 change(Vector2 c, Elem e){
		float limit = Field.tile*CityPicker.ratio;
		float xx = (e.p.y - 1) * limit;
		float yy = (e.p.x - 1) * limit;
		Vector2 v = new Vector2 (c.x,c.y);
		if (v.x < xx - limit / 2f)
						v.x = xx - limit / 2f;
		else
			if (v.x > xx + limit / 2f)
				v.x = xx + limit / 2f;
		if (v.y < yy- limit / 2f)
			v.y = yy - limit / 2f;
		else
			if (v.y > yy + limit / 2f)
				v.y = yy + limit / 2f;
		return v;
		
	}
		
}
	
	
