using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum cityKind{VILLAGE, TOWN, METRO, NONE};

public class CityPicker : MonoBehaviour {
	

	const int villageSize = 2;
	const int townSize = 5;


	/*public static GameObject[] objects;

	public static void create(){
		objects = new GameObject[3];
		for (int i=0; i<3; i++) {
			objects[i]=new GameObject();
		}
	}*/
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
	//public void setFlags(Heads l,GameObject g){
		foreach (Heads h in l) {
			//if (checkShape(h)==true)
			setFlag (h,object1,object2,object3);
		}
		//setFlag (l,g);
	}

	public static bool checkMoisAndTemp(Heads h){
		/*foreach (Elem el in h.elems) {
			if(el.temperature || el.moisture) return false;//to do
		}*/
		return true;
	}

	public static void setFlag(Heads h,GameObject object1,GameObject object2,GameObject object3){
		
		if (checkMoisAndTemp (h))
				if (h.elems.Count < villageSize)
						createVillage (h,object1);
				else if (h.elems.Count < townSize)
						createTown (h,object2);
				else createMetro (h,object3);

	}
	public static void createVillage(Heads h,GameObject object1){
		int numRows=3;
		if (Mathf.Abs (h.c.west.x - h.c.east.x) < numRows * Field.tile)
						numRows = 2;
		//Elem el = h.elems [0];
		foreach (Elem el in h.elems) {
			GameObject c = (GameObject)Instantiate (object1, new Vector3 ((el.p.y-1)* Field.tile*8,Field.mat [el.p.x, el.p.y].height*512,(el.p.x-1)* Field.tile*8), Quaternion.identity);
			c.transform.localScale = c.transform.localScale + new Vector3 (Field.tile, Field.tile, Field.tile);
//			c.transform.localScale=new Vector3(10,10,10);	
			//c.transform.localScale = new Vector3 (0.5f, 0.5f, 0.5f);
		}
		Debug.Log("Selo");
	}

	public static void createTown(Heads h,GameObject object2){
		//Elem el = h.elems [0];
		foreach (Elem el in h.elems) {
			GameObject c = (GameObject)Instantiate (object2, new Vector3 ((el.p.y-1)* Field.tile*8,Field.mat [el.p.x, el.p.y].height*512,(el.p.x-1)* Field.tile*8), Quaternion.identity);
		c.transform.localScale = c.transform.localScale + new Vector3 (Field.tile, Field.tile, Field.tile);
		//c.transform.localScale=new Vector3(10,10,10);	
		//c.transform.localScale = new Vector3 (0.5f, 0.5f, 0.5f);
			}
		
		Debug.Log("Town");
		
	}
	public static void createMetro(Heads h,GameObject object3){
		//Elem el = h.elems [0];
		foreach (Elem el in h.elems) {
			GameObject c = (GameObject)Instantiate (object3, new Vector3 ((el.p.y-1)* Field.tile*8,Field.mat [el.p.x, el.p.y].height*512,(el.p.x-1)* Field.tile*8), Quaternion.identity);
		c.transform.localScale = c.transform.localScale + new Vector3 (Field.tile, Field.tile, Field.tile);
			//c.transform.localScale=new Vector3(10,10,10);
		//c.transform.localScale = new Vector3 (0.5f, 0.5f, 0.5f);
			}
		Debug.Log("");		
	}
}


