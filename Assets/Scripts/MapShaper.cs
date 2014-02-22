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
	public abstract float[,] ShapeHeightMap(float[,] heightMap, Vector2 arraySize, int seed, float x, float z, float desiredScale = 1, int resolutionScale = 1);

	protected bool ShouldUpdateHeight(int resolutionScale, int i, int j) {
		if (resolutionScale == 1 || i % resolutionScale == 0 && j % resolutionScale == 0) 
		{
			return true;
		}
		return false;
	}

	/*TODO: This should go in GenerateHeightMap and only be done at the end of generation*/
	/*TODO: Make this actually work*/
	protected float[,] InterpolateIndices(float[,] heightMap, Vector2 arraySize, int resolutionScale) {
		int width = (int)arraySize.x;
		int length = (int)arraySize.x;
		int indexJump = resolutionScale / 2;
		while (indexJump > 0) {
			for (int i = indexJump; i < width; i += indexJump) {
				for (int j = indexJump; j < length; j += indexJump) {
					if (i % (indexJump * 2) != 0 || j % (indexJump * 2) != 0) {
						int neighborCount = 0;
						float neighbor1, neighbor2, neighbor3, neighbor4 = 0;
						float sum = 0;
						if (i > indexJump) {
							neighbor1 = heightMap[i - indexJump,j];
							sum += neighbor1;
							neighborCount++;
						}
						if (i < width - 1 - indexJump) {
							neighbor2 = heightMap[i + indexJump,j];
							sum += neighbor2;
							neighborCount++;
						}
						if (j > indexJump) {
							neighbor3 = heightMap[i,j - indexJump];
							sum += neighbor3;
							neighborCount++;
						}
						if (j < length - 1 - indexJump) {
							neighbor4 = heightMap[i,j + indexJump];
							sum += neighbor4;
							neighborCount++;
						}
						heightMap[i,j] = sum / neighborCount;
					}
				}
			}
			//first and last column
			for (int i = indexJump; i < width; i += indexJump) {
				int j = 0;
				int neighborCount = 0;
				float neighbor1, neighbor2, neighbor3, neighbor4 = 0;
				float sum = 0;
				if (i > indexJump) {
					neighbor1 = heightMap[i - indexJump,j];
					sum += neighbor1;
					neighborCount++;
				}
				if (i < width - 1 - indexJump) {
					neighbor2 = heightMap[i + indexJump,j];
					sum += neighbor2;
					neighborCount++;
				}
				heightMap[i,j] = sum / neighborCount;

				j = length - 1;
				neighborCount = 0;
				neighbor1 = neighbor2 = neighbor3 = neighbor4 = 0;
				sum = 0;
				if (i > indexJump) {
					neighbor1 = heightMap[i - indexJump,j];
					sum += neighbor1;
					neighborCount++;
				}
				if (i < width - 1 - indexJump) {
					neighbor2 = heightMap[i + indexJump,j];
					sum += neighbor2;
					neighborCount++;
				}
				heightMap[i,j] = sum / neighborCount;
			}
			indexJump /= 2;
		}
		return heightMap;
	}
}
