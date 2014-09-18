using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class Elem{

	public float temperature, moisture, height;
	public int available; //0 je validna vrednost
	public Position p; //koordinate u matrici


	public string toString(){
		return p.ToString();
	}
}

public class Coords{
	public Position north, west, east, south;
	public Coords(){
		north = new Position ();
		south = new Position ();
		east = new Position ();
		west = new Position ();
	}
}

public class Heads{
	public Coords c;
	public List<Elem> elems;

	public Heads(){
		c = new Coords();
		elems = new List<Elem> ();
	}
}

public class Position{
	public int x, y;

	public Position ()
	{

	}	
	public Position(int x, int y){
		this.x=x;
		this.y=y;
	}

	public static bool operator ==(Position x, Position y){
		if (x.x==y.x && x.y==y.y) return true;
		else return false;
	}

	public static bool operator !=(Position x, Position y){
		if (x.x==y.x && x.y==y.y) return false;
		else return true;
	}

	public static bool operator >(Position x, Position y){
		if (x.x>y.x) return true;
		else if (x.x<y.x) return false;
		else if (x.y>y.y) return true;
		else return false;
	}

	public static bool operator <(Position x, Position y){
		return !(x>y);
	}
		
	public string ToString(){
		return x+" "+y+"\n";
	}

	public int CompareTo(Position p){
		if (this>p) return 1;
		else if (this<p) return -1;
		else return 0;
	}

}


public class Field {

	public static int size, tile=16;
	public static Elem[,] mat;




	public static List<Heads> cc = new List<Heads> ();

	static float angleLimit = 20;

	public static void start(Terrain t, int tSize, float [,] htmap, GameObject object1,GameObject object2,GameObject object3,float waterLimit){

		float[,] newMat = new float[tSize, tSize];


		mat = new Elem[(tSize+tile-1)/tile+2,(tSize+tile-1)/tile+2];
		size = (tSize+tile-1)/tile+2;

//		newMat = t.terrainData.GetHeights (0, 0, tSize, tSize);
		newMat=htmap;
		shift (newMat);

		fill ();
		check (t,waterLimit);

		connectedComponents();


	/*	printH();
		printA();
		printB(newMat);*/

		//(new City ()).arrange (g);

		Debug.Log ("ovde");
		(new CityPicker ()).setFlags (cc,object1,object2,object3);

		Debug.Log (cc.Count);

	}
	

	static int count(){
		int k=0;
		for (int i=0;i<size; i++)
			for (int j=0;j<size;j++) 
				if (mat[i,j].available==-1) k++;
		return k;
	}

	static void check(Terrain t,float waterLimit){

		for (int i=1; i<size-1; i++)
						for (int j=1; j<size-1; j++) {
							if (mat[i,j].height<=waterLimit) mat[i,j].available=-1;
							else if (mat[i,j].available==1) continue;
							else{/*			
								if (Mathf.Atan(Mathf.Abs(mat[i,j].height-mat[i-1,j-1].height))>angleLimit) {
									mat[i,j].available=mat[i-1,j-1].available=1;
								}
								if (Mathf.Atan(Mathf.Abs(mat[i,j].height-mat[i-1,j].height))>angleLimit) {
									mat[i,j].available=mat[i-1,j].available=1;
								}
								if (Mathf.Atan(Mathf.Abs(mat[i,j].height-mat[i-1,j+1].height))>angleLimit) {
									mat[i,j].available=mat[i-1,j+1].available=1;
								}
								if (Mathf.Atan(Mathf.Abs(mat[i,j].height-mat[i,j-1].height))>angleLimit) {
									mat[i,j].available=mat[i,j-1].available=1;
								}
								if (Mathf.Atan(Mathf.Abs(mat[i,j].height-mat[i,j+1].height))>angleLimit) {
									mat[i,j].available=mat[i,j+1].available=1;
								}
								if (Mathf.Atan(Mathf.Abs(mat[i,j].height-mat[i+1,j-1].height))>angleLimit) {
									mat[i,j].available=mat[i+1,j-1].available=1;
								}	
								if (Mathf.Atan(Mathf.Abs(mat[i,j].height-mat[i+1,j].height))>angleLimit) {
									mat[i,j].available=mat[i+1,j].available=1;
								}
								if (Mathf.Atan(Mathf.Abs(mat[i,j].height-mat[i+1,j+1].height))>angleLimit) {
									mat[i,j].available=mat[i+1,j+1].available=1;
								}*/

								if ((mat[i,j].height-mat[i-1,j-1].height)*512>angleLimit) {
						//			Debug.Log ((mat[i,j].height-mat[i-1,j-1].height)*512);
									mat[i,j].available=mat[i-1,j-1].available=1;
								}
								if ((mat[i,j].height-mat[i-1,j].height)*512>angleLimit) {
									mat[i,j].available=mat[i-1,j].available=1;
								}
								if ((mat[i,j].height-mat[i-1,j+1].height)*512>angleLimit) {
									mat[i,j].available=mat[i-1,j+1].available=1;
								}
								if ((mat[i,j].height-mat[i,j-1].height)*512>angleLimit) {
									mat[i,j].available=mat[i,j-1].available=1;
								}
								if ((mat[i,j].height-mat[i,j+1].height)*512>angleLimit) {
									mat[i,j].available=mat[i,j+1].available=1;
								}
								if ((mat[i,j].height-mat[i+1,j-1].height)*512>angleLimit) {
									mat[i,j].available=mat[i+1,j-1].available=1;
								}	
								if ((mat[i,j].height-mat[i+1,j].height)*512>angleLimit) {
									mat[i,j].available=mat[i+1,j].available=1;
								}
								if ((mat[i,j].height-mat[i+1,j+1].height)*512>angleLimit) {
									mat[i,j].available=mat[i+1,j+1].available=1;
								}

							}
						}

	}

	static void fill(){
		for (int i=0; i<size; i++) {
						mat[0,i] = new Elem();
						mat[size-1, i] = new Elem();
						mat [0, i].height = -1;
						mat [size - 1, i].height = -1;
						mat [0, i].available = 1;
						mat [size - 1, i].available = 1;
						mat [0, i].p = new Position(0,i);
						mat [size - 1, i].p = new Position(size-1, i);


		}
		for (int i=0; i<size; i++) {
						mat[i,0] = new Elem();
						mat[i,size-1] = new Elem();
						mat [i, 0].height = -1;
						mat [i, size - 1].height = -1;
						mat [i, 0].available = 1;
						mat [i, size - 1].available = 1;
						mat[i,0].p = new Position(i,0);
						mat[i,size-1].p = new Position(i, size-1);
				}
	}

	static void shift(float[,] newMat){
		int size = (int)Mathf.Sqrt(newMat.Length);
		int size1 = (int)Mathf.Sqrt(mat.Length)-2;

		for (int i=0; i<size1; i++)
			for (int j=0; j<size1; j++) {
				mat [i+1, j+1] = new Elem();
				
				int k=0;
				int t=tile*i;
				while (k<tile && k+t<size){
					int n=0;
					int t1=tile*j;
					while (n<tile && n+t1<size){
						mat[i+1,j+1].height+=newMat[k+t, n+t1];
						n++;
					}
					k++;
				}
				mat [i+1,j+1].height/=tile*tile;
				mat [i+1, j+1].p = new Position(i+1, j+1);
		}
	}

	static void connectedComponents(){
		int i, j, k=1;
		int n=0;


		while (k<=mat.Length-4*size+4) {
			if (mat[i = (((k-1)/(size-2))+1), j = (((k-1)%(size-2))+1)].available==0){
				Heads h = new Heads();
				cc.Add (h);
				cc[n].elems = bfs(i, j, n);
				n++;
			}
			k++;
		}
	}

	static List<Elem> bfs(int i, int j, int n){

		Queue<Elem> q = new Queue<Elem> ();
		List<Elem> l = new List<Elem> ();
		Elem p;

		q.Enqueue (mat [i, j]);

		while (q.Count!=0) {

			p = q.Dequeue();
			i=p.p.x;
			j=p.p.y;

			if (cc[n].c.north.x == 0 && cc[n].c.north.y == 0)
			{
				cc[n].c.north = p.p;
				cc[n].c.south = p.p;
				cc[n].c.east = p.p;
				cc[n].c.west = p.p;
			}
			else
			{
				if ( p.p.x < cc[n].c.north.x) 
					cc[n].c.north = p.p;
				else if (p.p.x > cc[n].c.south.x)
					cc[n].c.south = p.p;
				if (p.p.y < cc[n].c.west.y)
					cc[n].c.west = p.p;
				else if (p.p.y > cc[n].c.east.y) 
					cc[n].c.east = p.p;
			}

			l.Add (p);

			if (mat[i-1,j].available==0) q.Enqueue(mat[i-1,j]);
			if (mat[i+1,j].available==0) q.Enqueue(mat[i+1,j]);
			if (mat[i,j-1].available==0) q.Enqueue(mat[i,j-1]);
			if (mat[i,j+1].available==0) q.Enqueue(mat[i,j+1]);

			p.available=2;

		}

		return l;
	}

	static void printH(){
		string s = "";
		for (int i=0; i<size; i++){
			s+="\n";
			for (int j=0; j<size; j++) 
				s+=(mat[i,j].height*512) +" ";
		}
		Debug.Log (s);

	}

	static void printA(){
		string s = "";
		for (int i=0; i<size; i++){
			s+="AAA\n";
			for (int j=0; j<size; j++) 
				s+=mat[i,j].available +" ";
		}
		Debug.Log (s);

	}
	static void printB(float [,] mat){
		string s = "";
		for (int i=0; i<size; i++){
			s+="BBB\n";
			for (int j=0; j<size; j++) 
				s+=mat[i,j] +" ";
		}
		Debug.Log (s);
		
	}


}





/*SORTIRANJE LISTE
		 * 
		 * List<Elem> l = new List<Elem>();
		
		for (int i=0;i<3;i++)
		for (int j=2;j>=0;j--){
			Elem e = new Elem();
			e.p = new Position(i,j);
			l.Add (e);
		}
		
		l.Sort(
			delegate(Elem e1, Elem e2){
				return e1.p.CompareTo(e2.p);
		}
		);
		
		Debug.Log (l[0].p.x);
		Debug.Log(l[0].p.y);*/
	

/*TEST POVEZANIH KOMPONENTI
		 * 
		 * 
		 * mat = new Elem[5,5];
		string s = "2222220112211122100222222";
		size=5;
		for (int i=0;i<size;i++)
			for (int j=0;j<size;j++){
				mat[i,j] = new Elem();
				mat[i,j].p = new Position(i,j);
				mat[i,j].available = s[i*5+j]-'0';
			}
		*/