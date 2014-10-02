using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
public class CityRoads : MonoBehaviour {
	public float terrainSize;
	public float heightMapSize;
	public Texture2D roadTexture;
	public void drawCityRoads (Vector3 a, Vector3 b){
//		Vector2 aa = new Vector2 (a.x * heightMapSize/terrainSize, a.z * heightMapSize/terrainSize);
//		Vector2 bb = new Vector2 (b.x * heightMapSize/terrainSize, b.z * heightMapSize/terrainSize);
		List<Vector2> roadMesh = new List<Vector2> ();
		Vector2 aa = new Vector2 (a.x, a.z);
		Vector2 bb = new Vector2 (b.x, b.z);
		if (Mathf.Abs(aa.x-bb.x)<0.2f) {
			float leftx = aa.x - 5f;
			float rightx = aa.x + 5f;
			float ny = Mathf.Min (aa.y, bb.y)-5f;
			float maxy = Mathf.Max (aa.y, bb.y)+5f;
			while (ny<maxy) {
				roadMesh.Add (new Vector2 (leftx, ny));
				roadMesh.Add (new Vector2 (rightx, ny));
				ny += 0.3f;
			}
			roadMesh.Add (new Vector2 (leftx, maxy));
			roadMesh.Add (new Vector2 (rightx, maxy));
		}
		if (Mathf.Abs(bb.y - aa.y)<0.2f) {
			float lefty = aa.y - 5f;
			float righty = aa.y + 5f;
			float nx = Mathf.Min (aa.x, bb.x)-5f;
			float maxx = Mathf.Max (aa.x, bb.x)+5f;
			while (nx<maxx) {
				roadMesh.Add (new Vector2 (nx, righty));
				roadMesh.Add (new Vector2 (nx, lefty));
				//Debug.DrawLine(new Vector3(nx,1000f,righty),new Vector3(nx,1000f,lefty),Color.red,350.0f);
				nx += 0.3f;
			}
			roadMesh.Add (new Vector2 (maxx, righty));
			roadMesh.Add (new Vector2 (maxx, lefty));
		}
		PathGenerator2 pathGenerator = new PathGenerator2 ();
		pathGenerator.texture = roadTexture;
		pathGenerator.height = 0.5f;
		pathGenerator.generate (roadMesh);
	}
}
