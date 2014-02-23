using UnityEngine;
using System.Collections;

public class GenerateDiamondSqaure : MapShaper
{
	public float roughness = 0.5f;

	public override float[,] ShapeHeightMap(MapShaperInfo info)
	{				
		SeedRandom(info.seed + (int)(info.x + info.z));

		int width = (int)info.arraySize.x;
		int length = (int)info.arraySize.y;

		float iterationScale = 1.0f;
		int iIndexJump = width - 1;
		int jIndexJump = length - 1;

		float[,] newHeightMap = new float[(int)info.arraySize.x, (int)info.arraySize.y];

		// Offset corner points.
		newHeightMap[0, 0] = (0.5f - (float)random.NextDouble()) * iterationScale;
		newHeightMap[width - 1, 0] = (0.5f - (float)random.NextDouble()) * iterationScale;
		newHeightMap[width - 1, length - 1] = (0.5f - (float)random.NextDouble()) * iterationScale;
		newHeightMap[0, length - 1] = (0.5f - (float)random.NextDouble()) * iterationScale;

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
					float sum = (newHeightMap[preI, preJ] + newHeightMap[postI, preJ] 
						+ newHeightMap[postI, postJ] + newHeightMap[preI, postJ]);
					newHeightMap[i, j] = sum / 4;
					newHeightMap[i, j] += (float)random.NextDouble() * iterationScale;
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
						neighbor1 = newHeightMap[preI, j];
						sum += neighbor1;
						neighborCount++;
					}
					if (i < width - 1) {
						neighbor2 = newHeightMap[postI, j];
						sum += neighbor2;
						neighborCount++;
					}
					if (j > 0) {
						neighbor3 = newHeightMap[i, preJ];
						sum += neighbor3;
						neighborCount++;
					}
					if (j < length - 1) {
						neighbor4 = newHeightMap[i, postJ];
						sum += neighbor4;
						neighborCount++;
					}
					newHeightMap[i, j] = (sum / neighborCount);
					newHeightMap[i, j] += (float)random.NextDouble() * iterationScale;
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

		for (int i = 0; i < info.arraySize.x; i++) {
			for (int j = 0; j < info.arraySize.y; j++) {
				info.heightMap[i, j] += (newHeightMap[i, j] - 0.5f) * info.mapToTerrainScale.y;
			}
		}

		return info.heightMap;
	}
}
		               

