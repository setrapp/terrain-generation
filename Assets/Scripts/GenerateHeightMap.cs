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
	public float startingHeight = 0.5f;
	public GameObject[,] terrain;
	public Vector3[,] whereIsTerrain;
	public Material material;
	private float[][,] heightMaps;
	private int alphaTileSize;
	public bool ignoreShapers = false;
	public MapShaperWrapper[] heightMapShapers = null;
	public GameObject player = null;
	private SplatPrototype[] terrainSplats;
	public HeightTexture[] heightTextures;
	private Vector2 distBetweenIndices;
	public GenerateClouds cloudGen;

	// Use this for initialization
	void Start ()
	{
		distBetweenIndices.x = terrainSize.x / mapSize;
		distBetweenIndices.y = terrainSize.z / mapSize;

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

		terrainSplats = new SplatPrototype[heightTextures.Length];
		for (int i = 0; i < terrainSplats.Length; i++) {
			terrainSplats[i] = new SplatPrototype(); 
			terrainSplats[i].texture = heightTextures[i].texture;
			terrainSplats[i].tileOffset = new Vector2(0, 0); 
			terrainSplats[i].tileSize = new Vector2(64, 64);//terrainSize.x, terrainSize.z);
		}

		// set up the terrain chunks with the default tile
		for (int i=0; i<chunkCount; i++) {
			for (int j=0; j<chunkCount; j++) {
				CreateTerrain(i, j);
			}
		}
		TextreAllTerrain();

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
				heightMap[i, j] = startingHeight;
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

		TextreAllTerrain();
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
		tData.splatPrototypes = terrainSplats;
		
		if (terrain[i,j]) {
			GameObject.Destroy(terrain[i,j]);
		}
		terrain[i,j] = Terrain.CreateTerrainGameObject(tData);
		terrain[i,j].transform.position = whereIsTerrain[i,j];
		terrain[i,j].layer = LayerMask.NameToLayer("Terrain");
		terrain [i, j].GetComponent<Terrain>().heightmapPixelError = 0;
	}

	private void TextreAllTerrain() {
		for (int i = 0; i < chunkCount; i++) {
			for (int j = 0; j < chunkCount; j++) {
				TextureTerrain(i, j);
			}
		}
	}

	private void TextureTerrain(int i, int j) {
		float[,] heightMap = heightMaps[i * chunkCount + j];
		TerrainData tData = terrain[i, j].GetComponent<Terrain>().terrainData;
		float[,,] alphas = new float[tData.alphamapWidth, tData.alphamapHeight, terrainSplats.Length];
		for (int k = 0; k < tData.alphamapWidth; k++)
		{
			for (int l = 0; l < tData.alphamapHeight; l++) {
				int overlapCount = 0;
				for (int m = 0; m < heightTextures.Length; m++)
				{
					alphas[k, l, m] = 0;
				}
				for (int m = 0; m < heightTextures.Length; m++)
				{
					if (heightMap[k, l] >= heightTextures[m].minHeight && heightMap[k, l] <= heightTextures[m].maxHeight){
						alphas[k, l, m] = 1.0f;
						overlapCount++;
					}
				}
				for (int m = 0; m < heightTextures.Length; m++)
				{
					alphas[k, l, m] /= overlapCount;
				}
			}
		}
		tData.SetAlphamaps(0, 0, alphas);
	}

    public float[,] ShapeHeightMap(float[,] heightMap, Vector2 arraySize, int index)
    {
		if (ignoreShapers) {
			return heightMap;
		}

		int iIndex = index / chunkCount;
		int jIndex = index % chunkCount;

        for (int i = 0; i < heightMapShapers.Length; i++)
        {
			Vector3 startPoint = whereIsTerrain[iIndex, jIndex];
			//Debug.Log("----- " + iIndex + " " + jIndex + " -----");
			heightMap = heightMapShapers[i].shaper.ShapeHeightMap(new MapShaperInfo(heightMap, arraySize, heightMapShapers[i].seed, startPoint.x, startPoint.z, new Vector3(distBetweenIndices.x, heightMapShapers[i].scale, distBetweenIndices.y)));
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

[Serializable]
public class HeightTexture {
	public Texture2D texture;
	public float minHeight;
	public float maxHeight;
}

public enum MapShift {
	LEFT = 0,
	RIGHT,
	DOWN,
	UP
}