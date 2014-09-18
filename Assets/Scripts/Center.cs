using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Center {
	
	public enum BiomeTypes {
		OCEAN=0, MARSH, ICE, LAKE ,
		BEACH, SNOW ,TUNDRA ,BARE,
		SCORCHED ,TAIGA, SHRUBLAND ,TEMPERATE_RAIN_FOREST,
		GRASSLAND, TROPICAL_RAIN_FOREST, SUBTROPICAL_DESERT
	};
	
	public int index;
	
	public Vector2 point;  // location
	public bool water;  // lake or ocean
	public bool ocean;  // ocean
	public bool coast;  // land polygon touching an ocean
	public bool border;  // at the edge of the map
	public BiomeTypes biome;  // biome type (see article)
	public float elevation;  // 0.0-1.0
	public float moisture;  // 0.0-1.0
	
	public List<Center> neighbors;
	public List<Edge> borders;
	public List<Corner> corners;
	
	public BiomeTypes getBiome() {
		BiomeTypes b;
		if (this.ocean) {
			return BiomeTypes.OCEAN;
		} else if (this.water) {
			if (this.elevation < 0.3f) return BiomeTypes.MARSH;
			if (this.elevation > 0.85f) return BiomeTypes.ICE;
			return BiomeTypes.LAKE;
		} else if (this.coast) {
			return BiomeTypes.BEACH;
		} else if (this.elevation > 0.85f) return BiomeTypes.ICE;
		 else if (this.elevation > 0.7f) {
			if (this.moisture > 0.5f) return BiomeTypes.SNOW;
			else if (this.moisture > 0.4f) return BiomeTypes.TUNDRA;
			else if (this.moisture > 0.2f) return BiomeTypes.BARE;
			else return BiomeTypes.SCORCHED;
		} else if (this.elevation > 0.68f) {
			if (this.moisture > 0.76f) return BiomeTypes.TAIGA;
			else  return BiomeTypes.SHRUBLAND;
			//else return BiomeTypes.SUBTROPICAL_DESERT;
		} else if (this.elevation > 0.5f) {
			if (this.moisture > 0.50f) return BiomeTypes.TEMPERATE_RAIN_FOREST;
			else  return BiomeTypes.GRASSLAND;}
			//else return BiomeTypes.SUBTROPICAL_DESERT;
		else {
			if (this.moisture > 0.33f) return BiomeTypes.TROPICAL_RAIN_FOREST;
			else if (this.moisture > 0.16f) return BiomeTypes.GRASSLAND;
			else return BiomeTypes.SUBTROPICAL_DESERT;
		}
	 }
}