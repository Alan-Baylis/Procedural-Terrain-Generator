using UnityEngine;
using System.Collections;

public class City : MonoBehaviour{


	public GameObject cube;
	bool t = false;



	public void arrange(GameObject cube){

		for (int i=0; i<Field.cc.Count; i++) {
						
						for (int j=0; j<Field.cc[i].elems.Count; j++) {
							if (j % 3 == 0) {
									GameObject c = (GameObject)Instantiate (cube, new Vector3 ((Field.cc [i].elems [j].p.y-1) * Field.tile*8, 
					      Field.mat [Field.cc [i].elems [j].p.x, Field.cc [i].elems [j].p.y].height +25, 
					                                                           (Field.cc [i].elems [j].p.x-1) * Field.tile*8), Quaternion.identity);
					//c.transform.localScale = c.transform.localScale + new Vector3 (Field.tile, Field.tile, Field.tile);
					c.transform.localScale=new Vector3(2, 2, 2);		
								}

			}
		}/*
		for (int i=0; i<Mathf.Sqrt(Field.mat.Length); i++) {

			for(int j=0; j<Mathf.Sqrt(Field.mat.Length); j++){
				GameObject c = (GameObject)Instantiate (cube, new Vector3 ((Field.mat[i,j].p.y-1)*Field.tile,Field.mat[i,j].height*500,(Field.mat[i,j].p.x-1)*Field.tile),
				                                        Quaternion.identity);
				c.transform.localScale = c.transform.localScale + new Vector3 (Field.tile, Field.tile, Field.tile);
			}
		}*/
	}

}
