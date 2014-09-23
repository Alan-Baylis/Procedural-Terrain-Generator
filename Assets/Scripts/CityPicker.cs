using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum cityKind{VILLAGE, TOWN, METRO, DESERT, FIELD, NONE};

public class CityPicker : MonoBehaviour {
	

	const int villageSize = 4;
	const int townSize = 8;
	static float disp = 0.55f;
	static int[] pq = {-1,-1,-1,1,1,-1,1,1};

	public static float heightMapSize, terrainSize;
	public static float ratio;

//	public static


	public CityPicker(float heightMap, float terrain){
		heightMapSize = heightMap;
		terrainSize = terrain;
		ratio = terrainSize / heightMapSize;
	}


	public static Position findCenter(Position p1, Position p2){
		return new Position ((p1.x + p2.x) / 2, (p1.y + p2.y) / 2);
	}

	public static Position findCenterPlus(Position p1, Position p2){
		return new Position ((p1.x + p2.x + 1) / 2, (p1.y + p2.y + 1) / 2);
	}

	public static bool inList(Position p, List<Elem> l){
		foreach (Elem e in l) {
			if (e.p==p) return true;
		}
		return false;
	}

	public static bool checkShape(Heads h){
		Position p1 = findCenter (findCenter (h.c.north, h.c.south), findCenter (h.c.west, h.c.east));
		Position p2 = findCenterPlus (findCenterPlus (h.c.north, h.c.south), findCenterPlus (h.c.west, h.c.east));

		if (inList (p1, h.elems) || inList (p2, h.elems) || h.elems.Count>20)	return true;
		else return false;
	
	}
	
	public void setFlags(List<Heads> l,GameObject object1,GameObject object2,GameObject object3){

		foreach (Heads h in l) {
			if (checkShape(h)==true)
				setFlag (h,object1,object2,object3);
		}
	}

	public static bool checkMoisAndTemp(Heads h){
		return true;
	}

	public static void setFlag(Heads h,GameObject object1,GameObject object2,GameObject object3){
		
		if (checkMoisAndTemp (h))
				if (h.elems.Count <= villageSize)
						h.roadPoints = createVillage (h,object1);
				else if (h.elems.Count <= townSize)
						h.roadPoints = createTown (h,object1, object2, object3);
				else h.roadPoints = createMetro (h,object2, object3);
	}

	static void draw(Vector3[] v){
		
		Vector3[] ve = new Vector3[4];
		
		for (int i=0; i<v.Length/4; i++) {
			ve[0]=v[i*4];
			ve[1]=v[i*4+1];
			ve[2]=v[i*4+2];
			ve[3]=v[i*4+3];
			
			for (int j = 0;j<3;j++)
				Debug.DrawLine(ve[j], ve[j+1], Color.blue, 350.0f);
			Debug.DrawLine(ve[3], ve[0], Color.blue, 350f);
		}
	}

	public static Vector3[] createVillage(Heads h,GameObject object1){
		int numRows=3;

		Vector3[] rPoints = new Vector3[h.elems.Count*4];
		
		int m = 0;

		if (Mathf.Abs (h.c.west.x - h.c.east.x) < numRows * Field.tile)
						numRows = 2;

		foreach (Elem el in h.elems){

			GameObject c = (GameObject)Instantiate (object1, new Vector3 ((el.p.y-1)* Field.tile*ratio,Field.mat [el.p.x, el.p.y].height *512,(el.p.x-1)* Field.tile*ratio), Quaternion.identity);
			c.transform.position = new Vector3(c.transform.position.x, (float)(c.transform.position.y + disp*c.transform.localScale.y/2), c.transform.position.z);
			c.transform.eulerAngles = new Vector3(c.transform.rotation.x, Random.Range(0, 360), c.transform.rotation.z);
			if (el.height>=0.2)
			for (int i=0;i<4;i++){		
				int x = Random.Range (0,11);
				if (x<=5){
					int p,q;
					p=pq[i*2];
					q=pq[i*2+1];
					c = (GameObject)Instantiate (object1, new Vector3 ((el.p.y-1)* Field.tile*ratio-p*50,Field.mat [el.p.x, el.p.y].height *512,(el.p.x-1)* Field.tile*ratio-q*50), Quaternion.identity);
					c.transform.position = new Vector3(c.transform.position.x, (float)(c.transform.position.y + disp*c.transform.localScale.y/2-5), c.transform.position.z);
					c.transform.eulerAngles = new Vector3(c.transform.rotation.x, Random.Range(0, 360), c.transform.rotation.z);
				}
			}

			rPoints[m++] = new Vector3((el.p.y - 1) * Field.tile * ratio-Field.tile*ratio/2, el.height*1000, (el.p.x - 1) * Field.tile * ratio-Field.tile*ratio/2);
			rPoints[m++] = new Vector3((el.p.y - 1) * Field.tile * ratio-Field.tile*ratio/2, el.height*1000, (el.p.x - 1) * Field.tile * ratio+Field.tile*ratio/2);
			rPoints[m++] = new Vector3((el.p.y - 1) * Field.tile * ratio+Field.tile*ratio/2, el.height*1000, (el.p.x - 1) * Field.tile * ratio+Field.tile*ratio/2);
			rPoints[m++] = new Vector3((el.p.y - 1) * Field.tile * ratio+Field.tile*ratio/2, el.height*1000, (el.p.x - 1) * Field.tile * ratio-Field.tile*ratio/2);


		}

		draw (rPoints);

		return rPoints;
	}


	public static Vector3[] createTown (Heads h, GameObject object1, GameObject object2, GameObject object3)
	{
		Vector3[] rPoints = new Vector3[h.elems.Count*4];

		int i = 0;

		foreach (Elem el in h.elems) {

			int r = Random.Range (0, 11);
			GameObject c;
			if (r > 2) {
				for (int j=0;j<4;j++){
					r = Random.Range (0, 12);
					int p = pq[j*2];
					int q = pq[j*2+1];
					if (r<=10){
						if (r <= 2)
							c = (GameObject)Instantiate (object1, new Vector3 ((el.p.y - 1 - 0.25f*p) * Field.tile * ratio, Field.mat [el.p.x, el.p.y].height * 512, (el.p.x - 1 - 0.25f*q) * Field.tile * ratio), Quaternion.identity);
						else if (r <= 4)
							c = (GameObject)Instantiate (object3, new Vector3 ((el.p.y - 1 - 0.25f*p) * Field.tile * ratio, Field.mat [el.p.x, el.p.y].height * 512, (el.p.x - 1 - 0.25f*q) * Field.tile * ratio), Quaternion.identity);
						else
							c = (GameObject)Instantiate (object2, new Vector3 ((el.p.y - 1 - 0.25f*p) * Field.tile * ratio, Field.mat [el.p.x, el.p.y].height * 512, (el.p.x - 1 - 0.25f*q) * Field.tile * ratio), Quaternion.identity);

						c.transform.position = new Vector3 (c.transform.position.x, (float)(c.transform.position.y + disp * c.transform.localScale.y / 2-5), c.transform.position.z);
						c.transform.eulerAngles = new Vector3 (c.transform.rotation.x, Random.Range (0, 360), c.transform.rotation.z);
					}
				}

			}

			rPoints[i++] = new Vector3((el.p.y - 1) * Field.tile * ratio-Field.tile*ratio/2, el.height*1000, (el.p.x - 1) * Field.tile * ratio-Field.tile*ratio/2);
			rPoints[i++] = new Vector3((el.p.y - 1) * Field.tile * ratio-Field.tile*ratio/2, el.height*1000, (el.p.x - 1) * Field.tile * ratio+Field.tile*ratio/2);
			rPoints[i++] = new Vector3((el.p.y - 1) * Field.tile * ratio+Field.tile*ratio/2, el.height*1000, (el.p.x - 1) * Field.tile * ratio+Field.tile*ratio/2);
			rPoints[i++] = new Vector3((el.p.y - 1) * Field.tile * ratio+Field.tile*ratio/2, el.height*1000, (el.p.x - 1) * Field.tile * ratio-Field.tile*ratio/2);




		}

		draw (rPoints);

		return rPoints;

	}
	public static Vector3[] createMetro(Heads h,GameObject object2,GameObject object3){

		Vector3[] rPoints = new Vector3[h.elems.Count*4];
		
		int i = 0;

		foreach (Elem el in h.elems) {
						int r = Random.Range (0, 11);
						GameObject c; 
						if (r > 2) {
							for (int j=0;j<4;j++){
								r = Random.Range (0, 3);
								int p = pq[j*2];
								int q = pq[j*2+1];
								if (r<=2){
									if (r == 0)
											c = (GameObject)Instantiate (object2, new Vector3 ((el.p.y - 1 - 0.25f*p) * Field.tile * ratio, Field.mat [el.p.x, el.p.y].height * 512, (el.p.x - 1 - 0.25f*q) * Field.tile * ratio), Quaternion.identity);
									else
											c = (GameObject)Instantiate (object3, new Vector3 ((el.p.y - 1 - 0.25f*p) * Field.tile * ratio, Field.mat [el.p.x, el.p.y].height * 512, (el.p.x - 1 - 0.25f*q) * Field.tile * ratio), Quaternion.identity);

									c.transform.position = new Vector3 (c.transform.position.x, (float)(c.transform.position.y + disp * c.transform.localScale.y / 2-5), c.transform.position.z);
									c.transform.eulerAngles = new Vector3 (c.transform.rotation.x, Random.Range (0, 360), c.transform.rotation.z);
								}
							}
								

						}
		
			rPoints[i++] = new Vector3((el.p.y - 1) * Field.tile * ratio-Field.tile*ratio/2, el.height*1000, (el.p.x - 1) * Field.tile * ratio-Field.tile*ratio/2);
			rPoints[i++] = new Vector3((el.p.y - 1) * Field.tile * ratio-Field.tile*ratio/2, el.height*1000, (el.p.x - 1) * Field.tile * ratio+Field.tile*ratio/2);
			rPoints[i++] = new Vector3((el.p.y - 1) * Field.tile * ratio+Field.tile*ratio/2, el.height*1000, (el.p.x - 1) * Field.tile * ratio+Field.tile*ratio/2);
			rPoints[i++] = new Vector3((el.p.y - 1) * Field.tile * ratio+Field.tile*ratio/2, el.height*1000, (el.p.x - 1) * Field.tile * ratio-Field.tile*ratio/2);


		}

		draw (rPoints);
		return rPoints;
	}
}


