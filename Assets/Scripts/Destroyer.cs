using UnityEngine;
using System.Collections;

public class Destroyer : MonoBehaviour {

	void OnCollisionEnter (Collision col){

		if (col.collider.CompareTag ("ground") == false) {
			Destroy(this.gameObject);
		}

	}
}
