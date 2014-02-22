using UnityEngine;
using System.Collections;

public class GenerateDiamondSqaure : MapShaper
{
	public float roughness = 0.5f;

	public override float[,] ShapeHeightMap(float[,] heightMap, Vector2 arraySize, int seed, float x, float z, float desiredScale = 1, int resolutionScale = 1)
	{				
		SeedRandom(seed + (int)(x + z));

		int width = (int)arraySize.x;
		int length = (int)arraySize.y;

		float iterationScale = desiredScale;
		int iIndexJump = width - 1;
		int jIndexJump = length - 1;
		
		// Offset corner points.
		heightMap[0, 0] += (0.5f - (float)random.NextDouble()) * iterationScale;
		heightMap[width - 1, 0] += (0.5f - (float)random.NextDouble()) * iterationScale;
		heightMap[width - 1, length - 1] += (0.5f - (float)random.NextDouble()) * iterationScale;
		heightMap[0, length - 1] += (0.5f - (float)random.NextDouble()) * iterationScale;

		/*heightMap[0, 0] += (float)random.NextDouble() * iterationScale;
		heightMap[width - 1, 0] += (float)random.NextDouble() * iterationScale;
		heightMap[width - 1, length - 1] +=  (float)random.NextDouble() * iterationScale;
		heightMap[0, length - 1] += (float)random.NextDouble() * iterationScale;*/

		// Scale for first iteration.
		iterationScale *= roughness;
		float iterationScaleX2 = iterationScale * 2;
		int iIndexJumpX2 = iIndexJump;
		int jIndexJumpX2 = jIndexJump;
		iIndexJump /= 2;
		jIndexJump /= 2;

		while (iIndexJump > 0 && jIndexJump > 0) {
			// Add points to create squares.
			for (int i = iIndexJump; i < width; i += iIndexJumpX2) {
				for (int j = iIndexJump; j < length; j += jIndexJumpX2) {
					int preI = i - iIndexJump, postI = i + iIndexJump;
					int preJ = j - jIndexJump, postJ = j + jIndexJump;
					float sum = (heightMap[preI, preJ] + heightMap[postI, preJ] 
						+ heightMap[postI, postJ] + heightMap[preI, postJ]);
					heightMap[i, j] = sum / 4;
					heightMap[i,j] += (0.5f - (float)random.NextDouble()) * iterationScaleX2;
				}
			}

			// Add points to create diamonds.
			int jStart = 0;
			for (int i = 0; i < width; i += iIndexJump) {
				jStart = Mathf.Abs(jStart - jIndexJump);
				for (int j = jStart; j < length; j += jIndexJumpX2) {
					int preI = i - iIndexJump, postI = i + iIndexJump;
					int preJ = j - jIndexJump, postJ = j + jIndexJump;
					int neighborCount = 0;
					float neighbor1, neighbor2, neighbor3, neighbor4 = 0;
					float sum = 0;
					if (i > 0) {
						neighbor1 = heightMap[preI, j];
						sum += neighbor1;
						neighborCount++;
					}
					if (i < width - 1) {
						neighbor2 = heightMap[postI, j];
						sum += neighbor2;
						neighborCount++;
					}
					if (j > 0) {
						neighbor3 = heightMap[i, preJ];
						sum += neighbor3;
						neighborCount++;
					}
					if (j < length - 1) {
						neighbor4 = heightMap[i, postJ];
						sum += neighbor4;
						neighborCount++;
					}
					heightMap[i, j] = (sum / neighborCount);
					heightMap[i,j] += (0.5f - (float)random.NextDouble()) * iterationScaleX2;
				}
			}

			// Scale for next iteration.
			iterationScale *= roughness;
			iterationScaleX2 = iterationScale * 2;
			iIndexJumpX2 = iIndexJump;
			jIndexJumpX2 = jIndexJump;
			iIndexJump /= 2;
			jIndexJump /= 2;
		}
		
		return heightMap;
	}
}
		               

