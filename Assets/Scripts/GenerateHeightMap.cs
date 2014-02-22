using UnityEngine;
using System.Collections;
using System;

public class GenerateHeightMap : MonoBehaviour
{
	public Vector3 terrainSize = new Vector3(100, 30, 100);		// Terrain size in world
	public int mapSize = 129;									// Number of indices in map in either direction
	public int chunkCount = 3;
	private int width;
	private int length;
	private int height;
	private const int MIN_RESOLUTION = 33;
	private int resolutionScale = 1;							// Scale small resolutions to fit terrain minimum
	public GameObject[,] terrain;
	public Vector3[,] whereIsTerrain;
	public Material material;
	private float[][,] heightMaps;
	private int alphaTileSize;
	public MapShaperWrapper[] heightMapShapers = null;
	public GameObject player = null;

	// Use this for initialization
	void Start ()
	{
		/*TODO: Ensure that mapsize is a power of 2 + 1*/
		// scale map size to fit minimum resolution
		if (mapSize < MIN_RESOLUTION) {
			//resolutionScale = (MIN_RESOLUTION - 1) / (mapSize - 1);
			//mapSize = MIN_RESOLUTION;
		}

		terrain = new GameObject[chunkCount,chunkCount];
		whereIsTerrain = new Vector3[chunkCount,chunkCount];
		Material material;
		alphaTileSize = mapSize - 1;

		width = mapSize;
		length = mapSize;

		// start height map at zero
		int chunkCountSqr = chunkCount * chunkCount;
		heightMaps = new float[chunkCountSqr][,];
		for(int h = 0; h < chunkCountSqr; h++) {
			heightMaps[h] = new float[width, length];
			ResetHeightMap(heightMaps[h]);
		}

		// shape height map using attached functions
        for (int i = 0; i < heightMapShapers.Length; i++)
        {
			heightMapShapers[i].shaper.SeedRandom(heightMapShapers[i].seed);
            heightMapShapers[i].shaper.InitShaper();
        }

		// calculate where terrain is
		int x = - (chunkCount/2);
		int z = - (chunkCount/2);
		for (int i=0;i<chunkCount;i++) {
			for (int j=0;j<chunkCount;j++){
				whereIsTerrain[i,j] = new Vector3(x * terrainSize.x, 0, z * terrainSize.z);
				z++;
			}
			z = - (chunkCount/2);
			x++;
		}

		// put a few different textures on them
		// note: this will need some work
		/*SplatPrototype[] test = new SplatPrototype[3];
     	test[0] = new SplatPrototype(); 
		test[0].texture = (Texture2D)Resources.Load("GoodDirt",typeof(Texture2D));
		test[0].tileOffset = new Vector2(0, 0); 
		test[0].tileSize = new Vector2(128, 128);
		
		test[1] = new SplatPrototype(); 
		test[1].texture = (Texture2D)Resources.Load("Grassy",typeof(Texture2D));
		test[1].tileOffset = new Vector2(0, 0); 
		test[1].tileSize = new Vector2(128, 128);

		test[2] = new SplatPrototype(); 
		test[2].texture = (Texture2D)Resources.Load("snow",typeof(Texture2D));
		test[2].tileOffset = new Vector2(0, 0); 
		test[2].tileSize = new Vector2(128, 128);

    	tData.splatPrototypes = test;
		
    	float[, ,] alphamaps = new float[128, 128, test.Length];
     	float[, ,] singlePoint = new float[1, 1, test.Length];
		
    	// set the actual textures in each tile here.
		for (int i=0;i<ALPHA_TILE_SIZE;i++) {
			for (int j=0;j<ALPHA_TILE_SIZE;j++){
				
				if (heightMap[i,j*length/ALPHA_TILE_SIZE] > 0.5) {
					alphamaps[i,j,0] = 0;
					alphamaps[i,j,1] = 0;
					alphamaps[i,j,2] = 1;
					singlePoint = new float[1, 1, test.Length];
					singlePoint[0,0,0] = 0f;
					singlePoint[0,0,1] = 0f;
					singlePoint[0,0,2] = 1f;
					
				} else if (heightMap[i,j*length/ALPHA_TILE_SIZE] > 0.0) {
					alphamaps[i,j,0] = 0.0f;
					alphamaps[i,j,1] = 1.0f;
					alphamaps[i,j,2] = 0;
					singlePoint = new float[1, 1, test.Length];
					singlePoint[0,0,0] = 0f;
					singlePoint[0,0,1] = 1f;
					singlePoint[0,0,2] = 0f;

				} else {
					alphamaps[i,j,0] = 1.0f;
					alphamaps[i,j,1] = 0.0f;
					alphamaps[i,j,2] = 0;
					singlePoint = new float[1, 1, test.Length];
					singlePoint[0,0,0] = 1f;
					singlePoint[0,0,1] = 0f;
					singlePoint[0,0,2] = 0f;

				}

				// this is amazingly stupid, but alpha is only able to be at every point
				// and not altogether as far as I can tell.
				tData.SetAlphamaps(j, i, singlePoint);
			}
		}*/

		// set up the terrain chunks with the default tile
		for (int i=0; i<chunkCount; i++) {
			for (int j=0; j<chunkCount; j++) {
				CreateTerrain(i, j);
			}
		}

		if (player != null) {
			RaycastHit terrainHit;
			if (Physics.Raycast(player.transform.position, Vector3.down, out terrainHit, Mathf.Infinity, ~LayerMask.NameToLayer("Terrain"))) {
				Vector3 pos = player.transform.position;
				pos.y = terrainHit.point.y + player.GetComponent<CharacterController>().height;
				player.transform.position = pos;
			}
		}
	}
	
	public void ResetHeightMap(float[,] heightMap) {
	for (int i=0; i<width; i++) {
			for (int j=0;j<length; j++) {
				heightMap[i, j] = 0.5f;
			}
		}
	}
	
	public void ShiftTerrain(MapShift mapShift){
		GameObject[] oldTerrains = new GameObject[chunkCount];
		Vector3[] oldWheres = new Vector3[chunkCount];
		float[][,] oldMaps = new float[chunkCount][,];
		switch (mapShift) {
			case MapShift.LEFT:
				for (int i = 0; i < chunkCount; i++) {
					oldTerrains[i] = terrain[chunkCount - 1, i];
					oldWheres[i] = whereIsTerrain[chunkCount - 1, i];
					oldWheres[i].x -= terrainSize.x * chunkCount;
					oldMaps[i] = heightMaps[((chunkCount - 1) * chunkCount) + i];
				}
				for (int i = chunkCount - 2; i >= 0; i--) {
					for (int j = 0; j < chunkCount; j++) {
						terrain[i + 1, j] = terrain[i, j];
						whereIsTerrain[i + 1, j] = whereIsTerrain[i, j];
						heightMaps[((i + 1) * chunkCount) + j] = heightMaps[(i * chunkCount) + j];
					}
				}
				for (int i = 0; i < chunkCount; i++) {
					terrain[0, i] = oldTerrains[i];
					whereIsTerrain[0, i] = oldWheres[i];
					heightMaps[(0 * chunkCount) + i] = oldMaps[i];
					ResetHeightMap(heightMaps[(0 * chunkCount) + i]);
				}
				for (int i = 0; i < chunkCount; i++) {
					CreateTerrain(0, i);
				}
				for (int i = 0; i < chunkCount; i++) {
					//Terrain[] neighbors = {null, (i < chunkCount - 1) ? terrain[0, i + 1].GetComponent<Terrain>() : null, terrain[1, i].GetComponent<Terrain>(), (i > 0) ? terrain[0, i - 1].GetComponent<Terrain>() : null};
				//	terrain[0, i].GetComponent<Terrain>().SetNeighbors(neighbors[0], neighbors[1], neighbors[2], neighbors[3]);
					/*TODO: Stop unity from crashing the next time a boundary is crossed*/
				}
				for (int i = 0; i < chunkCount; i++) {
					terrain[0, i].GetComponent<Terrain>().Flush();
				}
				break;
			case MapShift.RIGHT:	
				for (int i = 0; i < chunkCount; i++) {
					oldTerrains[i] = terrain[0, i];
					oldWheres[i] = whereIsTerrain[0, i];
					oldWheres[i].x += terrainSize.x * chunkCount;
					oldMaps[i] = heightMaps[(0 * chunkCount) + i];
				}
				for (int i = 1; i < chunkCount; i++) {
					for (int j = 0; j < chunkCount; j++) {
						terrain[i - 1, j] = terrain[i, j];
						whereIsTerrain[i - 1, j] = whereIsTerrain[i, j];
						heightMaps[((i - 1) * chunkCount) + j] = heightMaps[(i * chunkCount) + j];
					}
				}
				for (int i = 0; i < chunkCount; i++) {
					terrain[chunkCount - 1, i] = oldTerrains[i];
					whereIsTerrain[chunkCount - 1, i] = oldWheres[i];
					heightMaps[((chunkCount - 1) * chunkCount) + i] = oldMaps[i];
					ResetHeightMap(heightMaps[((chunkCount - 1) * chunkCount) + i]);
				}
				for (int i = 0; i < chunkCount; i++) {
					CreateTerrain(chunkCount - 1, i);
				}
				break;
			case MapShift.DOWN:	
				for (int i = 0; i < chunkCount; i++) {
					oldTerrains[i] = terrain[i, chunkCount - 1];
					oldWheres[i] = whereIsTerrain[i, chunkCount - 1];
					oldWheres[i].z -= terrainSize.z * chunkCount;
					oldMaps[i] = heightMaps[(i * chunkCount) + chunkCount - 1];
				}
				for (int i = 0; i < chunkCount; i++) {
					for (int j = chunkCount - 2; j >= 0; j--) {
						terrain[i, j + 1] = terrain[i, j];
						whereIsTerrain[i, j + 1] = whereIsTerrain[i, j];
						heightMaps[(i * chunkCount) + j + 1] = heightMaps[(i * chunkCount) + j];
					}
				}
				for (int i = 0; i < chunkCount; i++) {
					terrain[i, 0] = oldTerrains[i];
					whereIsTerrain[i, 0] = oldWheres[i];
					heightMaps[(i * chunkCount) + 0] = oldMaps[i];
					ResetHeightMap(heightMaps[(i * chunkCount) + 0]);
				}
				for (int i = 0; i < chunkCount; i++) {
					CreateTerrain(i, 0);
				}
				break;
			case MapShift.UP:
				for (int i = 0; i < chunkCount; i++) {
					oldTerrains[i] = terrain[i, 0];
					oldWheres[i] = whereIsTerrain[i, 0];
					oldWheres[i].z += terrainSize.z * chunkCount;
					oldMaps[i] = heightMaps[(i * chunkCount) + 0];
				}
				for (int i = 0; i < chunkCount; i++) {
					for (int j = 1; j < chunkCount; j++) {
						terrain[i, j - 1] = terrain[i, j];
						whereIsTerrain[i, j - 1] = whereIsTerrain[i, j];
						heightMaps[(i * chunkCount) + j - 1] = heightMaps[(i * chunkCount) + j];
					}
				}
				for (int i = 0; i < chunkCount; i++) {
					terrain[i, chunkCount - 1] = oldTerrains[i];
					whereIsTerrain[i, chunkCount - 1] = oldWheres[i];
					heightMaps[(i * chunkCount) + chunkCount - 1] = oldMaps[i];
					ResetHeightMap(heightMaps[(i * chunkCount) + chunkCount - 1]);
				}
				for (int i = 0; i < chunkCount; i++) {
					CreateTerrain(i, chunkCount - 1);
				}
				break;
		}
	}
	
	public void CreateTerrain(int i, int j, bool createNewHeightMap = true) {
		// create initial terrain pieces and place them
		// assume character is always in the center tile
		int index = i * chunkCount + j;
		if (createNewHeightMap) {
			heightMaps[index] = ShapeHeightMap(heightMaps[index], new Vector2(width, length), index);
		}
		TerrainData tData = new TerrainData();
		tData.heightmapResolution = width;
		tData.alphamapResolution = alphaTileSize;
		tData.SetDetailResolution(width-1,16);
		tData.baseMapResolution = width;
		tData.SetHeights(0,0,heightMaps[index]);
		tData.size = new Vector3(terrainSize.x, terrainSize.y, terrainSize.z);
		
		if (terrain[i,j]) {
			GameObject.Destroy(terrain[i,j]);
		}
		terrain[i,j] = Terrain.CreateTerrainGameObject(tData);
		terrain[i,j].transform.position = whereIsTerrain[i,j];
		terrain[i,j].layer = LayerMask.NameToLayer("Terrain");		
	}

    public float[,] ShapeHeightMap(float[,] heightMap, Vector2 arraySize, int index)
    {
		int iIndex = index / chunkCount;
		int jIndex = index % chunkCount;

        for (int i = 0; i < heightMapShapers.Length; i++)
        {
			Vector3 startPoint = whereIsTerrain[iIndex, jIndex];
			heightMap = heightMapShapers[i].shaper.ShapeHeightMap(heightMap, arraySize, heightMapShapers[i].seed, startPoint.x, startPoint.z, heightMapShapers[i].scale, resolutionScale);
        }
		
		// Determine what chunks surround this one for averaging.
		float[][,] heightMapsToAvg = new float[4][,];
		int ijOppIndexToAvg = 0, jOppIndexToAvg = 1, iOppIndexToAvg = 2, curIndexToAvg = 3;
		int iOtherMove = -1, jOtherMove = -1;
		if (iIndex == 0) {
			curIndexToAvg -= 1;
			ijOppIndexToAvg += 1;
			iOppIndexToAvg += 1;
			jOppIndexToAvg -= 1;
			iOtherMove = 1;
		}
		if (jIndex == 0) {
			curIndexToAvg -= 2;
			ijOppIndexToAvg += 2;
			iOppIndexToAvg -= 2;
			jOppIndexToAvg += 2;
			jOtherMove = 1;
		}

		heightMapsToAvg[ijOppIndexToAvg] = heightMaps[(iIndex + iOtherMove) * chunkCount + jIndex + jOtherMove];
		heightMapsToAvg[jOppIndexToAvg] = heightMaps[iIndex * chunkCount + jIndex + jOtherMove];
		heightMapsToAvg[iOppIndexToAvg] = heightMaps[(iIndex + iOtherMove) * chunkCount + jIndex];
		heightMapsToAvg[curIndexToAvg] = heightMap;

		// Average height values at indices shared by chunks
		/*TODO destroying one terrain twice to create, make a function that updates a list of terrains*/
		if (heightMapsToAvg [0] != null && heightMapsToAvg [1] != null && heightMapsToAvg [2] != null && heightMapsToAvg [3] != null) {
			AverageHeightMaps (heightMapsToAvg);
			if (heightMapsToAvg [ijOppIndexToAvg] != null && terrain[iIndex + iOtherMove, jIndex + jOtherMove] != null) {
				terrain [iIndex + iOtherMove, jIndex + jOtherMove].GetComponent<Terrain>().terrainData.SetHeights(0, 0 , heightMapsToAvg[ijOppIndexToAvg]);
			}
			if (heightMapsToAvg [jOppIndexToAvg] != null && terrain[iIndex, jIndex + jOtherMove]) {
				terrain[iIndex, jIndex + jOtherMove].GetComponent<Terrain>().terrainData.SetHeights(0, 0 , heightMapsToAvg[jOppIndexToAvg]);
			}
			if (heightMapsToAvg [iOppIndexToAvg] != null && terrain[iIndex + iOtherMove, jIndex]) {
				terrain [iIndex + iOtherMove, jIndex].GetComponent<Terrain>().terrainData.SetHeights(0, 0 , heightMapsToAvg[iOppIndexToAvg]);
			}
			if (heightMapsToAvg [curIndexToAvg] != null && terrain [iIndex, jIndex]) {
				terrain [iIndex, jIndex].GetComponent<Terrain>().terrainData.SetHeights(0, 0 , heightMapsToAvg[curIndexToAvg]);
			}
		}
		
		return heightMap;
    }
	
	/*TODO: Seems can be fixed by moving the next closest vertices so that the derivate continues too (like bezier curve splicing)*/
	
	private void AverageHeightMaps(float[][,] heightMapsToAvg) {
		/* height map indices
		 * 0: left-down
		 * 1: left-up
		 * 2: right-down
		 * 3: right-up 
		 */

		// Edge averages
		int lastIndex = mapSize - 1;
		for (int k = 1; k < mapSize; k++) {
			// Down seam
			if (heightMapsToAvg[0] != null && heightMapsToAvg[2] != null) {
				heightMapsToAvg[0][lastIndex, lastIndex - k] = heightMapsToAvg[2][0, lastIndex - k]
				= (heightMapsToAvg[0][lastIndex, lastIndex - k] + heightMapsToAvg[2][0 , lastIndex - k]) / 2;
			}
			// Left seam
			if (heightMapsToAvg[0] != null && heightMapsToAvg[1] != null) {
				heightMapsToAvg[0][lastIndex - k, lastIndex] = heightMapsToAvg[1][lastIndex - k, 0]
				= (heightMapsToAvg[0][lastIndex - k, lastIndex] + heightMapsToAvg[1][lastIndex - k, 0]) / 2;
			}
			// Up seam
			if (heightMapsToAvg[1] != null && heightMapsToAvg[3] != null) {
				heightMapsToAvg[1][lastIndex, k] = heightMapsToAvg[3][0, k]
				= (heightMapsToAvg[1][lastIndex, k] + heightMapsToAvg[3][0, k]) / 2;
			}
			// Right seam
			if (heightMapsToAvg[2] != null && heightMapsToAvg[3] != null) {
				heightMapsToAvg[2][k, lastIndex] = heightMapsToAvg[3][k, 0]
				= (heightMapsToAvg[2][k, lastIndex] + heightMapsToAvg[3][k, 0]) / 2;
			}
		}

		// Corner average
		float cornerSum = 0;
		if (heightMapsToAvg[0] != null) { cornerSum += heightMapsToAvg[0][lastIndex, lastIndex]; }
		if (heightMapsToAvg[1] != null) { cornerSum += heightMapsToAvg[1][lastIndex, 0]; }
		if (heightMapsToAvg[2] != null)	{ cornerSum += heightMapsToAvg[2][0, lastIndex]; }
		if (heightMapsToAvg[3] != null) { cornerSum += heightMapsToAvg[3][0, 0]; }
		float cornerAvg = cornerSum / 4;
		if (heightMapsToAvg[0] != null) { heightMapsToAvg[0][lastIndex, lastIndex] = cornerAvg; }
		if (heightMapsToAvg[1] != null) { heightMapsToAvg[1][lastIndex, 0] = cornerAvg; }
		if (heightMapsToAvg[2] != null) { heightMapsToAvg[2][0, lastIndex] = cornerAvg; }
		if (heightMapsToAvg[3] != null) { heightMapsToAvg[3][0, 0] = cornerAvg; }
	}
	
	void Update() {
		int halfChunkCount = chunkCount / 2;
		Vector3 playerBoundsMin = new Vector3(whereIsTerrain[halfChunkCount, 0].x, 0, whereIsTerrain[0, halfChunkCount].z);
		Vector3 playerBoundsMax = new Vector3(whereIsTerrain[halfChunkCount, 0].x + terrainSize.x, 0, whereIsTerrain[0, halfChunkCount].z + terrainSize.z);
		if (player.transform.position.z < playerBoundsMin.z) {
			ShiftTerrain(MapShift.DOWN);
		}else if (player.transform.position.z > playerBoundsMax.z) {
			ShiftTerrain(MapShift.UP);
		}

		if (player.transform.position.x < playerBoundsMin.x) {
			ShiftTerrain(MapShift.LEFT);
		}else if (player.transform.position.x > playerBoundsMax.x) {
			ShiftTerrain(MapShift.RIGHT);
		}
		/*TODO make shifts in other directions work and average in new terrain, if the i or j are 0, reorder how the array to average is constructed*/
		/*TODO recreating a terrain should produce the same terrain every time (maybe just pass in x and z values to change seed and reset random every time). I might have fixed it*/
	}
	

} // end of class

[Serializable]
public class MapShaperWrapper {
	public MapShaper shaper = null;
	public int seed = 0;
	public float scale = 1;
}

public enum MapShift {
	LEFT = 0,
	RIGHT,
	DOWN,
	UP
}