using UnityEngine;
using System;
using System.Collections;

public class Edge {
	public int index;
	public Center d0, d1;  // Delaunay edge
	public Corner v0, v1;  // Voronoi edge
	public Nullable<Vector2> midpoint;  // halfway between v0,v1
	public int river;  // volume of water, or 0

}
