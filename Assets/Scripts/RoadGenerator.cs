using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Path{

	public List<Vector2> points;
	public int length;

	public Path(){}

}


public class RoadGenerator {

	public Path[,] paths;
	public List<Vector2> mst;

	private List<Center> centers;

	private List<Center> cityCenters;

	private bool[] visited;
	private int[] centerDistances;
	private int[] lastCenter ;

	private float ratio;
	private float waterLevel;
	private float slopeLimit = 30.0f;
	private float heightDiffLimit = 0.1f;

	private TerrainGen terrainGen;

	public RoadGenerator(TerrainGen terrainGen){
		cityCenters = terrainGen.cityCenters;
		centers = terrainGen.centers;


		visited = new bool[centers.Count];
		centerDistances = new int[centers.Count];
		lastCenter = new int[centers.Count];

		ratio = (float)terrainGen.m_terrainSize / terrainGen.m_heightMapSize;
		waterLevel = terrainGen.waterLimit;

		this.terrainGen = terrainGen;
	}




	public void generate(){

		paths = new Path[cityCenters.Count, cityCenters.Count];

		for (int i=0; i < cityCenters.Count; i++) {

			int startCityIndex = cityCenters[i].index;

			for(int j=0;j<visited.Length;j++) visited[j] = false;

			Queue<Center> centersQueue = new Queue<Center>();
			centersQueue.Enqueue(cityCenters[i]);
			visited[startCityIndex] = true;
			centerDistances[startCityIndex] = 0;
			lastCenter[startCityIndex] = -1;

			while(centersQueue.Count != 0){

				Center currCenter = centersQueue.Dequeue();

				foreach(Center nextCenter in currCenter.neighbors){
					if ( visited[nextCenter.index]) continue;
					if ( Mathf.Abs(nextCenter.elevation - currCenter.elevation) > heightDiffLimit) continue;
					if (nextCenter.elevation <= waterLevel ) continue;
					if (terrainGen.m_terrain.terrainData.GetSteepness(nextCenter.point.x /terrainGen.m_heightMapSize, nextCenter.point.y/terrainGen.m_heightMapSize) > slopeLimit) continue;

					centersQueue.Enqueue(nextCenter);
					visited[nextCenter.index] = true;
					centerDistances[nextCenter.index] = centerDistances[currCenter.index] +1;
					lastCenter[nextCenter.index] = currCenter.index;

				}

			}

			for (int j=0; j < cityCenters.Count ; j++){

				paths[i,j] = new Path();

				int finalCityIndex = cityCenters[j].index;

				if (! visited[finalCityIndex]){
					paths[i,j].length = -1;
					continue;
				}

				paths[i,j].length = centerDistances[finalCityIndex];
				paths[i,j].points = new List<Vector2>();

				int currIndex = finalCityIndex;

				while(currIndex != -1){

					Vector2 newPoint = new Vector2(centers[currIndex].point.x * ratio, centers[currIndex].point.y *ratio);

					paths[i,j].points.Add(newPoint);
					currIndex = lastCenter[currIndex];
				}


			}

		}



		

	




	}




}
