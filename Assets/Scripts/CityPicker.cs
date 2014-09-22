using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum cityKind{VILLAGE, TOWN, METRO, DESERT, FIELD, NONE};

public class CityPicker : MonoBehaviour {
	

	const int villageSize = 4;
	const int townSize = 8;
	static float disp = 0.55f;
	static int[] pq = {-1,-1,-1,1,1,-1,1,1};


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

		if (inList (p1, h.elems) || inList (p2, h.elems))	return true;
		else return false;
	
	}
	
	public void setFlags(List<Heads> l,GameObject object1,GameObject object2,GameObject object3){
	
		foreach (Heads h in l) {
			if (checkShape(h)==true)
				setFlag (h,object1,object2,object3);
		}
		//setFlag (l,g);
	}

	public static bool checkMoisAndTemp(Heads h){
		return true;
	}

	public static void setFlag(Heads h,GameObject object1,GameObject object2,GameObject object3){
		
		if (checkMoisAndTemp (h))
				if (h.elems.Count <= villageSize)
						createVillage (h,object1);
				else if (h.elems.Count <= townSize)
						createTown (h,object1, object2, object3);
				else createMetro (h,object2, object3);
	}


	public static void createVillage(Heads h,GameObject object1){
		int numRows=3;

		if (Mathf.Abs (h.c.west.x - h.c.east.x) < numRows * Field.tile)
						numRows = 2;

		foreach (Elem el in h.elems){

			GameObject c = (GameObject)Instantiate (object1, new Vector3 ((el.p.y-1)* Field.tile*8,Field.mat [el.p.x, el.p.y].height *512,(el.p.x-1)* Field.tile*8), Quaternion.identity);
			c.transform.position = new Vector3(c.transform.position.x, (float)(c.transform.position.y + disp*c.transform.localScale.y/2), c.transform.position.z);
			c.transform.eulerAngles = new Vector3(c.transform.rotation.x, Random.Range(0, 360), c.transform.rotation.z);
			if (el.height>=0.2)
			for (int i=0;i<4;i++){		
				int x = Random.Range (0,11);
				if (x<=5){
					int p,q;
					p=pq[i*2];
					q=pq[i*2+1];
					c = (GameObject)Instantiate (object1, new Vector3 ((el.p.y-1)* Field.tile*8-p*50,Field.mat [el.p.x, el.p.y].height *512,(el.p.x-1)* Field.tile*8-q*50), Quaternion.identity);
					c.transform.position = new Vector3(c.transform.position.x, (float)(c.transform.position.y + disp*c.transform.localScale.y/2-5), c.transform.position.z);
					c.transform.eulerAngles = new Vector3(c.transform.rotation.x, Random.Range(0, 360), c.transform.rotation.z);
				}
			}

		}
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

	public static void createTown (Heads h, GameObject object1, GameObject object2, GameObject object3)
	{
		Vector3[] rPoints = new Vector3[h.elems.Count*4];

		int i = 0;

		foreach (Elem el in h.elems) {

				int r = Random.Range (0, 11);
				GameObject c;
				if (r > 2) {
							r = Random.Range (0, 11);
							if (r <= 2)
									c = (GameObject)Instantiate (object1, new Vector3 ((el.p.y - 1) * Field.tile * 8, Field.mat [el.p.x, el.p.y].height * 512, (el.p.x - 1) * Field.tile * 8), Quaternion.identity);
							else if (r <= 4)
									c = (GameObject)Instantiate (object3, new Vector3 ((el.p.y - 1) * Field.tile * 8, Field.mat [el.p.x, el.p.y].height * 512, (el.p.x - 1) * Field.tile * 8), Quaternion.identity);
							else
									c = (GameObject)Instantiate (object2, new Vector3 ((el.p.y - 1) * Field.tile * 8, Field.mat [el.p.x, el.p.y].height * 512, (el.p.x - 1) * Field.tile * 8), Quaternion.identity);

							c.transform.position = new Vector3 (c.transform.position.x, (float)(c.transform.position.y + disp * c.transform.localScale.y / 2-5), c.transform.position.z);
							c.transform.eulerAngles = new Vector3 (c.transform.rotation.x, Random.Range (0, 360), c.transform.rotation.z);

				rPoints[i++] = new Vector3((el.p.y - 1) * Field.tile * 8-Field.tile*4, el.height*1000, (el.p.x - 1) * Field.tile * 8-Field.tile*4);
				rPoints[i++] = new Vector3((el.p.y - 1) * Field.tile * 8-Field.tile*4, el.height*1000, (el.p.x - 1) * Field.tile * 8+Field.tile*4);
				rPoints[i++] = new Vector3((el.p.y - 1) * Field.tile * 8+Field.tile*4, el.height*1000, (el.p.x - 1) * Field.tile * 8+Field.tile*4);
				rPoints[i++] = new Vector3((el.p.y - 1) * Field.tile * 8+Field.tile*4, el.height*1000, (el.p.x - 1) * Field.tile * 8-Field.tile*4);



				/*rPoints[i++] = new Vector3(el.p.y*Field.tile*8, el.height*1000, el.p.x*Field.tile*8);
				rPoints[i++] = new Vector3(el.p.y*Field.tile*8, el.height*1000, (el.p.x-1)*Field.tile*8);
				rPoints[i++] = new Vector3((el.p.y-1)*Field.tile*8, el.height*1000, (el.p.x-1)*Field.tile*8);
				rPoints[i++] = new Vector3((el.p.y-1)*Field.tile*8, el.height*1000, el.p.x*Field.tile*8);*/	
			}



		}


		draw (rPoints);

	}
	public static void createMetro(Heads h,GameObject object2,GameObject object3){

		Vector3[] rPoints = new Vector3[h.elems.Count*4];
		
		int i = 0;

		foreach (Elem el in h.elems) {
						int r = Random.Range (0, 11);
						GameObject c; 
						if (r > 2) {
								r = Random.Range (0, 2);
								if (r == 0)
										c = (GameObject)Instantiate (object2, new Vector3 ((el.p.y - 1) * Field.tile * 8, Field.mat [el.p.x, el.p.y].height * 512, (el.p.x - 1) * Field.tile * 8), Quaternion.identity);
								else
										c = (GameObject)Instantiate (object3, new Vector3 ((el.p.y - 1) * Field.tile * 8, Field.mat [el.p.x, el.p.y].height * 512, (el.p.x - 1) * Field.tile * 8), Quaternion.identity);

								c.transform.position = new Vector3 (c.transform.position.x, (float)(c.transform.position.y + disp * c.transform.localScale.y / 2-5), c.transform.position.z);
								c.transform.eulerAngles = new Vector3 (c.transform.rotation.x, Random.Range (0, 360), c.transform.rotation.z);

				rPoints[i++] = new Vector3((el.p.y - 1) * Field.tile * 8-Field.tile*4, el.height*1000, (el.p.x - 1) * Field.tile * 8-Field.tile*4);
				rPoints[i++] = new Vector3((el.p.y - 1) * Field.tile * 8-Field.tile*4, el.height*1000, (el.p.x - 1) * Field.tile * 8+Field.tile*4);
				rPoints[i++] = new Vector3((el.p.y - 1) * Field.tile * 8+Field.tile*4, el.height*1000, (el.p.x - 1) * Field.tile * 8+Field.tile*4);
				rPoints[i++] = new Vector3((el.p.y - 1) * Field.tile * 8+Field.tile*4, el.height*1000, (el.p.x - 1) * Field.tile * 8-Field.tile*4);
				

						}
		


		}

		draw (rPoints);
	}
}


