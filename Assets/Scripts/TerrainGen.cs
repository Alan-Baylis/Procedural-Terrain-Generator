
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Delaunay;
using Delaunay.Geo;

public class TerrainGen : MonoBehaviour {

	public Texture2D road;
	public Texture2D riverTexture;
	public Path[,] roadPaths;

	public int m_heightMapSize = 513; //Higher number will create more detailed height maps
	public int m_alphaMapSize = 1024; //This is the control map that controls how the splat textures will be blended
	
	PerlinNoise m_groundNoise, m_mountainNoise, m_treeNoise, m_detailNoise;
	
	public Terrain m_terrain;
	
	public Texture2D[] m_splats;
	public Texture2D m_detail0, m_detail1, m_detail2;
	List<SplatPrototype> m_splatPrototypes = new List<SplatPrototype> ();
	
	public GameObject[] m_trees;
	List<TreePrototype> m_treeProtoTypes = new List<TreePrototype> ();
	
	public int m_groundSeed = 0;
	public float m_groundFrq = 800.0f;
	
	public int m_mountainSeed = 1;
	public float  m_mountainFrq = 1200.0f;
	
	public int m_terrainSize=2400;
	public int m_terrainHeight = 512;
	
	public int m_islandSeed = 0;
	public float m_islandFrq = 800.0f;

	public int m_detailSeed = 3;
	public float  m_detailFrq = 100.0f;


	public int m_detailMapSize = 512; //Resolutions of detail (Grass) layers

	private float heightMinimum = 50000, heightMaximum = -50000 ;
	
	public float waterLimit;
	public float lowTexturePosition; // pozicije za texture
	public float mediumTexturePosition;
	public float highTexturePosition;

	public GameObject object1, object2, object3;
	
	//terrainData.detailPrototypes = m_detailProtoTypes;
	public int m_treeSeed = 2;
	public float  m_treeFrq = 400.0f;
	//Tree settings
	public int[] m_treeSpacing;  //spacing between trees
	public float m_treeDistance = 2000.0f; //The distance at which trees will no longer be drawn
	public float m_treeBillboardDistance = 400.0f; //The distance at which trees meshes will turn into tree billboards
	public float m_treeCrossFadeLength = 20.0f; //As trees turn to billboards there transform is rotated to match the meshes, a higher number will make this transition smoother
	public int m_treeMaximumFullLODCount = 400; //The maximum number of trees that will be drawn in a certain area. 

	//Detail settings
	public DetailRenderMode detailMode;
	public int m_detailObjectDistance = 400; //The distance at which details will no longer be drawn
	public float m_detailObjectDensity = 4.0f; //Creates more dense details within patch
	public int m_detailResolutionPerPatch = 32; //The size of detail patch. A higher number may reduce draw calls as details will be batch in larger patches
	public float m_wavingGrassStrength = 0.4f;
	public float m_wavingGrassAmount = 0.2f;
	public float m_wavingGrassSpeed = 0.4f;
	public Color m_wavingGrassTint = Color.white;
	public Color m_grassHealthyColor = Color.white;
	public Color m_grassDryColor = Color.white;

	DetailPrototype[] m_detailProtoTypes;
	public int numPoints;
	
	public GameObject waterTexture;
	
	public List<Vector2> points = new List<Vector2>();
	public List<Center> centers = new List<Center> ();
	public List<Edge> edges = new List<Edge>();
	public List<Corner> corners = new List<Corner>();
	
	private Voronoi voronoi;
	
	private PerlinNoise m_islandNoise;
	public List<Center> cityCenters = new List<Center>();

	
	// The Voronoi library generates multiple Point objects for
	// corners, and we need to canonicalize to one Corner object.
	// To make lookup fast, we keep an array of Points, bucketed by
	// x value, and then we only have to look at other Points in
	// nearby buckets. When we fail to find one, we'll create a new
	// Corner object.
	
	
	
	
	// Helper functions for the following for loop; ideally these
	// would be inlined
	
	private Dictionary<int,List<Corner>> _cornerMap = new Dictionary<int,List<Corner>>();
	
	Center[,] getCenter;
	float[,] htmap;

	
	// Use this for initialization
	void createTerrain () {

		TerrainData terrainData = new TerrainData();
		
		terrainData.heightmapResolution = m_heightMapSize;

		terrainData.size = new Vector3(m_terrainSize, m_terrainHeight, m_terrainSize);

		terrainData.alphamapResolution = m_alphaMapSize;


		//add random points
		
		for (int i=0; i< numPoints; i++) {
			
			points.Add(new Vector2(Random.Range(10,m_heightMapSize-10),Random.Range(10,m_heightMapSize-10)));
			
		}
		/*
		for (int i=0; i<20; i++)
			for (int j=0; j<20;j++)
				points.Add(new Vector2(m_terrainSize/20 * i, m_terrainSize/20*j));
				*/
		voronoi = new Voronoi (points, null, new Rect (0, 0, m_heightMapSize, m_heightMapSize));
		buildGraph (points, voronoi);
		
		
		
		
		
		m_groundNoise = new PerlinNoise(m_groundSeed);
		m_mountainNoise = new PerlinNoise(m_mountainSeed);
		m_treeNoise = new PerlinNoise(m_treeSeed);
		m_islandNoise = new PerlinNoise (m_islandSeed);
		
		
		htmap = new float[m_heightMapSize,m_heightMapSize];
		getCenter = new Center[m_heightMapSize,m_heightMapSize]; 
		
		m_terrain = new Terrain();
		
		
		for (int i= 0; i < m_splats.Length ; i++) {
			
			SplatPrototype splatPrototype = new SplatPrototype();
			splatPrototype.texture = m_splats[i];
			splatPrototype.tileSize = new Vector2 (2, 2);
			
			m_splatPrototypes.Add( splatPrototype);
			
		}
		
		for (int i= 0; i < m_trees.Length ; i++) {
			
			TreePrototype treePrototype = new TreePrototype();
			treePrototype.prefab = m_trees[i];
			
			
			m_treeProtoTypes.Add( treePrototype);
			
		}

		m_detailProtoTypes = new DetailPrototype[3];
		
		m_detailProtoTypes[0] = new DetailPrototype();
		m_detailProtoTypes[0].prototypeTexture = m_detail0;
		m_detailProtoTypes[0].renderMode = detailMode;
		m_detailProtoTypes[0].healthyColor = m_grassHealthyColor;
		m_detailProtoTypes[0].dryColor = m_grassDryColor;
		
		m_detailProtoTypes[1] = new DetailPrototype();
		m_detailProtoTypes[1].prototypeTexture = m_detail1;
		m_detailProtoTypes[1].renderMode = detailMode;
		m_detailProtoTypes[1].healthyColor = m_grassHealthyColor;
		m_detailProtoTypes[1].dryColor = m_grassDryColor;
		
		m_detailProtoTypes[2] = new DetailPrototype();
		m_detailProtoTypes[2].prototypeTexture = m_detail2;
		m_detailProtoTypes[2].renderMode = detailMode;
		m_detailProtoTypes[2].healthyColor = m_grassHealthyColor;
		m_detailProtoTypes[2].dryColor = m_grassDryColor;

		//terrainData.detailPrototypes = m_detailProtoTypes;

		
		terrainData.splatPrototypes = m_splatPrototypes.ToArray();
		terrainData.treePrototypes = m_treeProtoTypes.ToArray();
		terrainData.detailPrototypes = m_detailProtoTypes;
		m_terrain = Terrain.CreateTerrainGameObject(terrainData).GetComponent<Terrain>();

		FillHeights(htmap); // ovde je terren

		terrainData.SetHeights(0, 0, htmap);
		
		//float ratio = (float)(m_heightMapSize-1)/m_terrainSize;
		

		foreach (Corner p in corners) {


			Vector2 coords = new Vector2(p.point.x*m_terrainSize/m_heightMapSize,p.point.y*m_terrainSize/m_heightMapSize);

			p.elevation = Terrain.activeTerrain.SampleHeight(new Vector3(coords.x,0.0f,coords.y));
			p.elevation/= m_terrainHeight;

			
			p.water = p.elevation < waterLimit; //* (heightMaximum - heightMinimum) + heightMinimum;
			
		}

//		foreach (Corner p in corners) {
//
//
//
//			int elevX = (int) (p.point.x);
//			int elevY = (int) (p.point.y);
//			
//			if (elevX == m_heightMapSize) elevX--;
//			if (elevY == m_heightMapSize) elevY--;
//			
//			p.elevation = htmap [elevX, elevY] ;
//			
//			p.water = htmap[elevX, elevY] < waterLimit; //* (heightMaximum - heightMinimum) + heightMinimum;
//			
//		} 
		
		
		//create water
		
		int size = (int) (m_terrainSize /2 * 1.41);
		float waterLevel = m_terrainHeight * waterLimit;
		GameObject water = (GameObject)Instantiate (waterTexture, new Vector3 (m_terrainSize /2, waterLevel, m_terrainSize /2), Quaternion.identity);
		Vector3 v = water.transform.localScale;
		water.transform.localScale = v + new Vector3 (size, 0, size);
		
		

		assignOceanCoastAndLand ();
		
//		foreach (Corner q in corners) {
//			if (q.ocean || q.coast) {
//				q.elevation = 0f;
//			}
//		}		
		
		for (int i=0; i <m_heightMapSize; i++)
			for (int j=0; j<m_heightMapSize; j++)
				getCenter [i, j] = centers [0];

		
		fillCenters2 ();
		

		
		assignPolygonElevations (htmap);
		
		
		
		call ();
		
		
		//		foreach (Center center in centers)
		//						center.elevation = (center.elevation - heightMinimum) / (heightMaximum - heightMinimum);
		
		assignBiomes ();

		

		//FillAlphaMapByHeights (terrainData,htmap);
		FillAlphaMapByBiomes (terrainData); // postavlja texture
		
		

		m_terrain.transform.position = new Vector3(0, 0, 0); 
		
		
		
		//disable this for better frame rate
		m_terrain.castShadows = false;

		



	}


	private void fillCenters(){
		
		for (int i=0; i <m_heightMapSize; i++)
		for (int j=0; j<m_heightMapSize; j++) {
			
			Center minimumCenter = centers[0];
			float minimumCenterDist = Mathf.Pow(minimumCenter.point.x - i, 2) + Mathf.Pow(minimumCenter.point.y - j, 2);
			
			foreach(Center center in centers){
				
				float centDist = Mathf.Pow(center.point.x- i,2 ) + Mathf.Pow(center.point.y- j,2);
				
				if (centDist < minimumCenterDist) {
					
					minimumCenterDist = centDist;
					minimumCenter = center;
					
					
					
				}
				
			}
			
			getCenter[i,j] = minimumCenter;
			
		}
		
	}
	
	
	void FillTreeInstancesBiomes(Terrain terrain){
		
		Random.seed = 0;

		for(int x = 0; x < m_terrainSize; x ++) 
		{
			for (int z = 0; z < m_terrainSize; z ++) 
			{
				
				float ratio = (float)(m_heightMapSize-1)/m_terrainSize;
				
				Center.BiomeTypes biome = getCenter[(int)(x*ratio),(int)(z*ratio)].biome;
				
				//int space=0;
				int tree = 10;
				if ((int)biome == 4) {tree = 0;}
				if ((int)biome ==9) {tree = 1;}
				if ((int)biome == 10) { tree = 2;}
				if ((int)biome == 11 ){ tree =3; }
				if( (int) biome==13) { tree =4; }
				
				float unit = 1.0f / (m_terrainSize - 1);
				
				//float offsetX = Random.value * unit * m_treeSpacing;
				//float offsetZ = Random.value * unit * m_treeSpacing;
				
				float normX = x * unit;// + offsetX;
				float normZ = z * unit;// + offsetZ;
				
				// Get the steepness value at the normalized coordinate.
				float angle = terrain.terrainData.GetSteepness(normX, normZ);
				
				// Steepness is given as an angle, 0..90 degrees. Divide
				// by 90 to get an alpha blending value in the range 0..1.
				float frac = angle / 90.0f;
				
				float height = htmap[z*m_heightMapSize/m_terrainSize,x*m_heightMapSize/m_terrainSize];
				
				float fracHeight = (height-heightMinimum)/(heightMaximum-heightMinimum);
				
			if (tree<10)
				{

					if(frac < 0.5f && fracHeight> waterLimit+0.005f && Random.Range(0,m_treeSpacing[tree]) == 0) //make sure tree are not on steep slopes & in the sea
				{
					float worldPosX = x+(m_terrainSize-1);
					float worldPosZ = z+(m_terrainSize-1);
					
					float noise = m_treeNoise.FractalNoise2D(worldPosX, worldPosZ, 3, m_treeFrq, 1.0f);
					float ht = terrain.terrainData.GetInterpolatedHeight(normX, normZ);
	
					if( noise > 0 && ht < m_terrainHeight*0.4f )
					{
						
						TreeInstance temp = new TreeInstance();
						temp.position = new Vector3(normX,ht,normZ);
						temp.prototypeIndex = tree;
						temp.widthScale = 1;
						temp.heightScale = 1;
						temp.color = Color.white;
						temp.lightmapColor = Color.white;
//						if (tree==0)
//								Debug.DrawLine(new Vector3(x,0f,z), new Vector3(x,100f,z),Color.red,2600f);
						terrain.AddTreeInstance(temp);
					}
				}
				}
			}
		}
		
		
		terrain.treeBillboardDistance = m_treeBillboardDistance;
		terrain.treeCrossFadeLength = m_treeCrossFadeLength;
		terrain.treeMaximumFullLODCount = m_treeMaximumFullLODCount;
		
	}

	void FillHeights(float[,] htmap)
	{
		float ratio = (float)m_terrainSize/(float)m_heightMapSize;
		
		float mini = 1000000.0f, maxi = 0.0f;
		heightMaximum = -10000000.0f;
		heightMinimum = 10000000.0f;
		for(int x = 0; x < m_heightMapSize; x++)
		{
			for(int z = 0; z < m_heightMapSize; z++)
			{
				float worldPosX = (x+m_heightMapSize-1)*ratio;
				float worldPosZ = (z+m_heightMapSize-1)*ratio;
				
				float mountains = Mathf.Max(0.0f, m_mountainNoise.FractalNoise2D(worldPosX, worldPosZ, 8, m_mountainFrq, 0.8f));
				//float mountains = Mathf.Max(0.0f, (float)m_mountainNoise.PerlinNoise_2D(worldPosX, worldPosZ));
				
				if (mini > mountains) mini = mountains;
				if (maxi< mountains) maxi = mountains;
				
				float plain = m_groundNoise.FractalNoise2D(worldPosX, worldPosZ, 6, m_groundFrq, 0.1f) + 0.1f;
				//float plain = (float)m_groundNoise.PerlinNoise_2D(worldPosX, worldPosZ)+ 0.1f;
				
				
				float distX = 2*((float)x/m_heightMapSize - 0.5f);
				float distY = 2*((float)z/m_heightMapSize - 0.5f);
				float dist = Mathf.Pow (Mathf.Pow (distX, 2) + Mathf.Pow (distY, 2), 0.5f);
				//				float dist = Mathf.Min(x, z, m_heightMapSize - z, m_heightMapSize - x);
				//				dist = dist/m_heightMapSize;
				float dist2=Mathf.Min (dist,1.0f);
				//				dist/=Mathf.Pow(2.0f,0.5f);
				htmap[z,x] = plain+mountains*Mathf.Pow((1 - dist2),0.3f) - Mathf.Pow(dist,0.7f)/3;
				
				if (heightMinimum > htmap[z,x]) heightMinimum = htmap[z,x];
				if (heightMaximum < htmap[z,x]) heightMaximum = htmap[z,x];
			}
		}
		for (int x = 0; x < m_heightMapSize; x++)
		for (int z = 0; z < m_heightMapSize; z++) {
			htmap [x, z] = (htmap [x, z] - heightMinimum) / (heightMaximum - heightMinimum);
			
		}
		heightMinimum=0;
		heightMaximum=1;
		
		
	}
	
	void FillAlphaMapByBiomes (TerrainData terrainData)
	{
		float[,,] map  = new float[m_alphaMapSize, m_alphaMapSize, 15];
		
		Random.seed = 0;
		
		for(int x = 0; x < m_alphaMapSize; x++) 
		{
			for (int z = 0; z < m_alphaMapSize; z++) 
			{
				
				float ratio = (float)(m_heightMapSize-1)/m_alphaMapSize;
				
				Center.BiomeTypes biome = getCenter[(int)(x*ratio),(int)(z*ratio)].biome;
				
				for (int i=0; i< 15;i++)
					map[z,x,i]= 0;
				
				map[z,x,(int)biome]=1;
			}
		}
		
		
		terrainData.SetAlphamaps(0, 0, map); //pridruzi alfa mapu terenu
		
		
	}
	
	

	public void buildGraph(List<Vector2> points, Voronoi voronoi) {
		Center p;
		Corner q; 
		Vector2 point;
		Vector2 other;
		List<Delaunay.Edge> libedges= voronoi.Edges();
		Dictionary<System.Nullable<Vector2>,Center> centerLookup = new Dictionary<System.Nullable<Vector2>,Center>();
		
		// Build Center objects for each of the points, and a lookup map
		// to find those Center objects again as we build the graph
		foreach ( Vector2 ppp in points) {
			System.Nullable<Vector2> pp = (System.Nullable<Vector2>) ppp;
			p = new Center();
			p.index = centers.Count;
			p.point = (Vector2) pp;
			p.neighbors = new List<Center>();
			p.borders = new List<Edge>();
			p.corners = new List<Corner>();
			centers.Add(p);
			centerLookup[pp] = p;
		}
		foreach ( Center po in centers) {
			voronoi.Region(po.point);
		}
		
		
		
		foreach (Delaunay.Edge libedge in libedges) {
			LineSegment dedge = libedge.DelaunayLine();
			LineSegment vedge = libedge.VoronoiEdge();
			
			// Fill the graph data. Make an Edge object corresponding to
			// the edge from the voronoi library.
			Edge edge = new Edge();
			edge.index = edges.Count;
			edge.river = 0;
			edges.Add(edge);
			edge.midpoint = null;
			if (vedge.p0!= null && vedge.p1 != null)
				edge.midpoint = Vector2.Lerp( (Vector2) vedge.p0, (Vector2) vedge.p1, 0.5f);

			
			// Edges point to corners. Edges point to centers. 
			edge.v0 = makeCorner(vedge.p0);
			edge.v1 = makeCorner(vedge.p1);
			edge.d0 = centerLookup[dedge.p0];
			edge.d1 = centerLookup[dedge.p1];
			
			// Centers point to edges. Corners point to edges.
			if (edge.d0 != null) { edge.d0.borders.Add(edge); }
			if (edge.d1 != null) { edge.d1.borders.Add(edge); }
			if (edge.v0 != null) { edge.v0.protrudes.Add(edge); }
			if (edge.v1 != null) { edge.v1.protrudes.Add(edge); }
			
			// Centers point to centers.
			if (edge.d0 != null && edge.d1 != null) {
				addToCenterList(edge.d0.neighbors, edge.d1);
				addToCenterList(edge.d1.neighbors, edge.d0);
			}
			
			// Corners point to corners
			if (edge.v0 != null && edge.v1 != null) {
				addToCornerList(edge.v0.adjacent, edge.v1);
				addToCornerList(edge.v1.adjacent, edge.v0);
			}
			
			// Centers point to corners
			if (edge.d0 != null) {
				addToCornerList(edge.d0.corners, edge.v0);
				addToCornerList(edge.d0.corners, edge.v1);
			}
			if (edge.d1 != null) {
				addToCornerList(edge.d1.corners, edge.v0);
				addToCornerList(edge.d1.corners, edge.v1);
			}
			
			// Corners point to centers
			if (edge.v0 != null) {
				addToCenterList(edge.v0.touches, edge.d0);
				addToCenterList(edge.v0.touches, edge.d1);
			}
			if (edge.v1 != null) {
				addToCenterList(edge.v1.touches, edge.d0);
				addToCenterList(edge.v1.touches, edge.d1);
			}
		}
	}
	
	public Corner makeCorner(System.Nullable<Vector2> npoint) {
		Corner q;
		int bucket;
		if (npoint == null) return null;
		Vector2 point = (Vector2) npoint;
		for (bucket = (int)(point.x)-1; bucket <= (int)(point.x)+1; bucket++) {
			if (_cornerMap.ContainsKey(bucket))
			{
				foreach (Corner qq in _cornerMap[bucket]) {
					float dx = point.x - qq.point.x;
					float dy = point.y - qq.point.y;
					if (dx*dx + dy*dy < 1e-6) {
						return qq;
					}
				}
			}
		}
		bucket = (int)(point.x);
		
		if (! _cornerMap.ContainsKey(bucket)) _cornerMap[bucket] = new List<Corner>();
		q = new Corner();
		q.index = corners.Count;
		corners.Add(q);
		q.point = point;
		q.border = (point.x == 0 || point.x == m_heightMapSize
		            || point.y == 0 || point.y == m_heightMapSize);
		q.touches = new List<Center>();
		q.protrudes = new List<Edge>();
		q.adjacent = new List<Corner>();
		_cornerMap[bucket].Add(q);
		return q;
	}
	private void addToCornerList(List<Corner> v,Corner x) {
		if (x != null && v.IndexOf(x) < 0) { v.Add(x); }
	}
	private void addToCenterList(List<Center> v,Center x) {
		if (x != null && v.IndexOf(x) < 0) { v.Add(x); }
	}
	
	private void assignOceanCoastAndLand()
	{
		
		
		Queue<Center> queue = new Queue<Center> ();
		//Center p, r;
		//Corner q;
		int numWater;
		
		foreach (Center p in centers) {
			numWater = 0;
			foreach (Corner q in p.corners) {
				if (q.border) {
					p.border = true;
					p.ocean = true;
					q.water = true;
					queue.Enqueue(p);
				}
				if (q.water) {
					numWater += 1;
				}
			}
			p.water = (p.ocean || numWater >= p.corners.Count * 0.3f);
		}
		while (queue.Count > 0) {
			Center p = queue.Dequeue();
			foreach (Center r in p.neighbors) {
				if (r.water && !r.ocean) {
					r.ocean = true;
					queue.Enqueue(r);
				}
			}
		}
		
		foreach (Center p in centers) {
			int  numOcean = 0;
			int  numLand = 0;
			foreach (Center r in p.neighbors) {
				numOcean += (r.water)?1:0;
				numLand += (!r.water)?1:0;
			}
			p.coast = (numOcean > 0) && (numLand > 0);

		}
		
		
		foreach (Corner q in corners) {
			int numOcean = 0;
			int numLand = 0;
			foreach (Center p in q.touches) {
				numOcean += (p.ocean)?1:0;
				numLand += (!p.water)?1:0;
			}
			q.ocean = (numOcean == q.touches.Count);
			q.coast = (numOcean > 0) && (numLand > 0);
			q.water = q.border || ((numLand != q.touches.Count) && !q.coast);
		}
	}
	
	
	public void assignPolygonElevations(float[,] htmap){
		float sumElevation;
		//float ratio = (float)(m_heightMapSize-1)/m_terrainSize;
		foreach (Center p in centers) {
			sumElevation = 0f;
			foreach (Corner q in p.corners) {
				sumElevation += q.elevation;
			}
			p.elevation = sumElevation / p.corners.Count;
			
			//htmap[(int)(p.point.x),(int)(p.point.y)]=p.elevation;
		}
	}
	
	
	
	public List<Corner> landCorners(List<Corner> corners){
		List<Corner> locations = new List<Corner> ();
		foreach (Corner q in corners) {
			if (!q.ocean && !q.coast) {
				locations.Add(q);
			}
		}
		return locations;
	}
	
	public void calculateDownslopes() {
		
		
		foreach (Corner q in corners) {
			Corner r = q;
			foreach (Corner s in q.adjacent) {
				if (s.elevation < r.elevation) {
					r = s;
				}
			}
			q.downslope = r;
		}
	}
	
	public void calculateWatersheds() {
		//var q:Corner, r:Corner, i:int, changed:Boolean;
		bool changed;
		int i;
		// Initially the watershed pointer points downslope one step.      
		foreach ( Corner q in corners) {
			q.watershed = q;
			if (!q.ocean && !q.coast) {
				q.watershed = q.downslope;
			}
		}
		// Follow the downslope pointers to the coast. Limit to 100
		// iterations although most of the time with numPoints==2000 it
		// only takes 20 iterations because most points are not far from
		// a coast.  TODO: can run faster by looking at
		// p.watershed.watershed instead of p.downslope.watershed.
		for (i = 0; i < 10000; i++) {
			changed = false;
			foreach (Corner q in corners) {
				if (!q.ocean && !q.coast && !q.watershed.coast) {
					Corner r = q.downslope.watershed;
					if (!r.ocean) q.watershed = r;
					changed = true;
				}
			}
			if (!changed) break;
		}
		// How big is each watershed?
		foreach (Corner q in corners) {
			Corner r = q.watershed;
			r.watershed_size+=1;
		}
		
	}
	
	public void createRivers() {

		int k=0;
		for (int i = 0; i <corners.Count/2; i++) {
		//for (int i=0;i<8;i++){

			Corner q = corners[Random.Range(0, corners.Count-1)];
			if (q.ocean || q.elevation<waterLimit || q.elevation > 0.9f) continue;
			//if (q.ocean || q.elevation<0.3f || q.elevation > 0.9f) {i--;continue;}
			// Bias rivers to go west: if (q.downslope.x > q.x) continue;
			while (!q.coast ) {
				if (q == q.downslope) {
					break;
				}
				Edge edge = lookupEdgeFromCorner(q, q.downslope);
				edge.river = edge.river + 1;
				q.river+=1;
				q.downslope.river+= 1; 
				q = q.downslope;
			}
			//			k++;
			
		}
	}
	public Edge lookupEdgeFromCorner(Corner q, Corner s) {
		foreach (Edge edge in q.protrudes) {
			if (edge.v0 == s || edge.v1 == s) return edge;
		}
		return null;
	}
	
	public void assignCornerMoisture() {
		//Corner q, r;
		float	  newMoisture;
		Queue<Corner> queue=new Queue<Corner>();
		foreach (Corner q in corners) {
			if ((q.water || q.river > 0) && !q.ocean) {
				q.moisture = q.river > 0? Mathf.Min(3.0f, (0.2f * q.river)) : 1.0f;
				queue.Enqueue(q);
			} else {
				q.moisture = 0.0f;
			}
		}
		while (queue.Count > 0) {
			Corner q = queue.Dequeue();
			
			foreach (Corner r in q.adjacent) {
				newMoisture = q.moisture * 0.9f;
				if (newMoisture > r.moisture) {
					r.moisture = newMoisture;
					queue.Enqueue(r);
				}
			}
		}
		foreach (Corner q in corners) {
			if (q.ocean || q.coast) {
				q.moisture = 1.0f;
			}

		}
	}
	
	
	
	public void assignPolygonMoisture() {
		//Center p, q;
		float	 sumMoisture;
		foreach (Center p in centers) {
			sumMoisture = 0.0f;
			foreach (Corner q in p.corners) {
				if (q.moisture > 1.0f) q.moisture = 1.0f;
				sumMoisture += q.moisture;
			}
			p.moisture = sumMoisture / p.corners.Count;
		}
		
	}
	
	public void redistributeMoisture(List<Corner> locations) {
		
		
		locations.Sort(delegate(Corner x, Corner y)	{
			if( x.moisture < y.moisture) return -1;
			else if( x.moisture > y.moisture) return 1;
			else return 0;
		});
		for (int i = 0; i < locations.Count; i++) {
			locations[i].moisture = (float)i/(float)(locations.Count-1);
		}
		
	}
	
	public void call()
	{
		calculateDownslopes ();
		calculateWatersheds ();
		createRivers ();
		assignCornerMoisture ();
		redistributeMoisture (landCorners (corners));
		assignPolygonMoisture ();
	}
	
	
	

	
	
	public void assignBiomes() {
		foreach (Center p in centers) {
			p.biome = p.getBiome();
		}
	}
	
	public static List<Vector2> GetPoints(List<Vector2> points)
	{
		if (points.Count == 0)
			return new List<Vector2>();
		List<Vector2> pPoints = new List<Vector2>();
		float highestx = points[0].x;
		float highesty = points[0].y;
		float lowestx = points[0].x;
		float lowesty = points[0].y;
		for (int i = 0; i < points.Count; i++)
		{
			if (points[i].x > highestx)
				highestx = points[i].x;
			if (points[i].y > highesty)
				highesty = points[i].y;
			if (points[i].x < lowestx)
				lowestx = points[i].x;
			if (points[i].y < lowesty)
				lowesty = points[i].y;
		}
		for (int x = (int)lowestx  ; x < Mathf.CeilToInt(highestx); x++)
		{
			for (int y = (int)lowesty; y < Mathf.CeilToInt( highesty ); y++)
			{
				if (IsPointInPolygon( points, new Vector2(x, y)))
				{
					pPoints.Add(new Vector2(x,y));
				}
			}
		}
		return pPoints;
	}
	
	static private bool IsPointInPolygon( List<Vector2> polygon, Vector2 point)
	{
		float epsilon = 0.01f;
		bool isInside = false;
		for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
		{
			if (((polygon[i].y > point.y + epsilon) != (polygon[j].y > point.y + epsilon)) &&
			    (point.x +epsilon < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x))
			{
				isInside = !isInside;
			}
		}
		return isInside;
	}
	
	
	
	void fillCenters2(){
		foreach (Center center in centers) {
			
			foreach(Vector2 point in GetPoints(voronoi.Region(center.point))){
				
				getCenter[(int) point.x, (int) point.y] = center;
				
				
			}
			
		}
	}


	void FillDetailMap(Terrain terrain)
	{
		//each layer is drawn separately so if you have a lot of layers your draw calls will increase 
		int[,] detailMap0 = new int[m_detailMapSize,m_detailMapSize];
		int[,] detailMap1 = new int[m_detailMapSize,m_detailMapSize];
		int[,] detailMap2 = new int[m_detailMapSize,m_detailMapSize];
		
	//	float ratio = (float)m_terrainSize/(float)m_detailMapSize;
		
		//Random.seed = 0;
		
		for(int x = 0; x <m_detailMapSize; x ++) 
		{
			for (int z = 0; z <m_detailMapSize; z ++) 
			{

				detailMap0[z,x] = 0;
				detailMap1[z,x] = 0;
				detailMap2[z,x] = 0;

				float ratio = (float)m_terrainSize/(float)m_detailMapSize;
				float ratio1 = (float)(m_heightMapSize-1)/(float)m_detailMapSize;
				Center.BiomeTypes biome = getCenter[(int)(x*ratio1),(int)(z*ratio1)].biome;
				

				int det = 10;
				if ((int)biome == 6) {det= 0;}
				if ((int)biome ==12) {det = 1;}
				if (( int)biome==8) {det=2;}

				//float unit = 1.0f / (m_detailMapSize - 1);
				
				//float normX = x * unit;
				//float normZ = z * unit;
				
				// Get the steepness value at the normalized coordinate.
			//	float angle = terrain.terrainData.GetSteepness(normX, normZ);
				
				// Steepness is given as an angle, 0..90 degrees. Divide
				// by 90 to get an alpha blending value in the range 0..1.
				//float frac = angle / 90.0f;
				
				if(det<10 )

				{
					/*float worldPosX = (x+(m_detailMapSize-1))*ratio;
					float worldPosZ = (z+(m_detailMapSize-1))*ratio;
					
					float noise = m_detailNoise.FractalNoise2D(worldPosX, worldPosZ, 3, m_detailFrq, 1.0f);
					
					if(noise > 0.0f) 
					{*/
						float rnd = Random.value;
						//Randomly select what layer to use
						if(rnd < 0.01f)
							detailMap0[z,x] = 1;
						else if(rnd < 0.75f)
							detailMap1[z,x] = 1;
						else
							detailMap2[z,x] = 1;

				}
				
			}
		}
		
		terrain.terrainData.wavingGrassStrength = m_wavingGrassStrength;
		terrain.terrainData.wavingGrassAmount = m_wavingGrassAmount;
		terrain.terrainData.wavingGrassSpeed = m_wavingGrassSpeed;
		terrain.terrainData.wavingGrassTint = m_wavingGrassTint;
		terrain.detailObjectDensity = m_detailObjectDensity;
		terrain.detailObjectDistance = m_detailObjectDistance;
		terrain.terrainData.SetDetailResolution(m_detailMapSize, m_detailResolutionPerPatch);
		
		terrain.terrainData.SetDetailLayer(0,0,0,detailMap0);
		terrain.terrainData.SetDetailLayer(0,0,1,detailMap1);
		terrain.terrainData.SetDetailLayer(0,0,2,detailMap2);
		
	}

	private void generateRivers(){

		River river = new River ();
		river.riverTexture = riverTexture;
		river.terrainSize = m_terrainSize;
		river.heightMapSize = m_heightMapSize;
		river.waterlimit = waterLimit;
		river.drawRivers (corners);
		
	}

//	private void generateRoads(){
//
//		Roads roads = new Roads (this);
//		roads.createRoads (this);
//
//		
//	}

	private void generateRoads(){

		RoadGenerator roadGen = new RoadGenerator (this);
		roadGen.generate ();
		roadPaths = roadGen.paths;

		DrawRoads drawRaod = new DrawRoads (m_terrainSize, m_heightMapSize, road);
		for (int i =0; i< cityCenters.Count; i++)
						for (int j=i+1; j< cityCenters.Count; j++) {						
							if (roadPaths[i,j].length == -1 ) continue;
							if(roadPaths[i,j].points.Count<2) continue;

							bool skip = false;
							for (int h=0; h< cityCenters.Count; h++){
								if ( h==i || h==j) continue;

								if (roadPaths[i,j].length >= 0.7f*(roadPaths[i,h].length + roadPaths[h,j].length) ) skip =true;
							}

							if (skip) continue;

							drawRaod.createRoads(roadPaths[i,j].points);	
					
						}
	}

	private void createCities(){
		generateRoads ();
		Field.setCenters (getCenter);
		cityCenters = Field.start (m_terrain, m_heightMapSize, htmap,road, object1, object2, object3, waterLimit);

		foreach (GameObject house in GameObject.FindGameObjectsWithTag("house")) {
				//house.transform.position += new Vector3 (-terrainSizeX / 2, 0, -terrainSizeY/2);
			float x =house.transform.position.x;
			float z = house.transform.position.z;
			float y = Terrain.activeTerrain.SampleHeight(house.transform.position) ;
			y += house.transform.lossyScale.y/2 - 1.4f;
			house.transform.position = new Vector3 (x,y,z);
		}

	}
	void OnGUI(){
		switch(counter){
		case 0: GUI.Box (new Rect (Screen.width/2-200, 20, 400, 30),  "PRESS SPACE TO GENERATE TERRAIN AND TEXTURE"); break; //pravljenje terena i tekstura
		case 1:  GUI.Box (new Rect (Screen.width/2-200, 20, 400, 30), "PRESS SPACE TO GENERATE RIVERS");break;				 //reke
		case 2:  GUI.Box (new Rect (Screen.width/2-200, 20, 400, 30), "PRESS SPACE TO GENERATE VEGETATION");break;			 //vegetacija
		case 3:  GUI.Box (new Rect (Screen.width/2-200, 20, 400, 30), "PRESS SPACE TO GENERATE CITIES");break;	 //gradovi
		case 4:  GUI.Box (new Rect (Screen.width/2-200, 20, 400, 30), "PRESS SPACE TO GENERATE ROADS");break;	 //putevi
		}
	}
	int counter = 0;
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Space)) {
						switch (counter) {
						case 0:
								createTerrain ();
								break;    //pravljenje terena i tekstura
						case 1:
								generateRivers ();
								break;   //reke
						case 2:
								createVegetation ();
								break;  //vegetacija
						case 3:
								createCities ();
								break; 	//gradovi
						case 4:
								generateRoads ();
								break;      //putevi
						}
						++counter;
				}
		else if (Input.GetKeyDown (KeyCode.F1)) {
			switch (counter) {
			case 0:
				createTerrain ();
				generateRivers ();
				createVegetation ();
				createCities ();
				generateRoads ();
				counter = 5;
				break;
			case 1:
				generateRivers ();
				createVegetation ();
				createCities ();
				generateRoads ();
				counter = 5;
				break;
			case 2:
				createVegetation ();
				createCities ();
				generateRoads ();
				counter = 5;
				break;
			case 3:
				createCities ();
				generateRoads ();
				counter = 5;
				break;
			case 4:
				generateRoads ();
				counter = 5;
				break;
			}
		}
	}

	void createVegetation(){
		FillTreeInstancesBiomes (m_terrain);
		FillDetailMap (m_terrain); // vegetacija
	}


}


