using UnityEngine;
using System.Collections;
using System;

public abstract class MapShaper : MonoBehaviour
{
	protected System.Random random;
	protected float currentHeight = 0;
	
	public void SeedRandom(int seed)
	{
		random = new System.Random(seed);
	}
	public virtual void InitShaper(){}
	public abstract float[,] ShapeHeightMap(MapShaperInfo info);
}

public class MapShaperInfo {
	public MapShaperInfo(float[,] heightMap, Vector2 arraySize, int seed, float x, float z, Vector3 mapToTerrainScale) {
		this.heightMap = heightMap;
		this.arraySize = arraySize;
		this.seed = seed;
		this.x = x;
		this.z = z;
		this.mapToTerrainScale = mapToTerrainScale;
	}
	public float[,] heightMap;
	public Vector2 arraySize;
	public int seed;
	public float x;
	public float z;
	public Vector3 mapToTerrainScale;
}