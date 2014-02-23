using UnityEngine;
using System.Collections;
using System;

public class GenerateVoronoi : MapShaper
{
	public enum VoronoiType {Linear = 0, Sine = 1, Tangent = 2};
	public int voronoiTypeInt = 0;
	public VoronoiType voronoiType = VoronoiType.Linear;
	public int voronoiCells = 16;
	public float voronoiFeatures = 1.0f;
	public float voronoiScale = 1.0f;
	public float voronoiBlend = 1.0f;

	//	voronoiPresets.Add(new voronoiPresetData("Scattered Peaks", VoronoiType.Linear, 16, 8, 0.5f, 1.0f));
	//	voronoiPresets.Add(new voronoiPresetData("Rolling Hills", VoronoiType.Sine, 8, 8, 0.0f, 1.0f));
	//	voronoiPresets.Add(new voronoiPresetData("Jagged Mountains", VoronoiType.Linear, 32, 32, 0.5f, 1.0f));
	public override float[,] ShapeHeightMap(MapShaperInfo info)
	{
		int Tx = (int)info.arraySize.x;
		int Ty = (int)info.arraySize.y;

		float[,] newHeightMap = new float[(int)info.arraySize.x, (int)info.arraySize.y];

		// Create Voronoi set...
		ArrayList voronoiSet = new ArrayList();
		int i;
		for (i = 0; i < voronoiCells; i++)
		{
			Peak newPeak = new Peak();
			int xCoord = (int)Mathf.Floor((float)random.NextDouble() * Tx);
			int yCoord = (int)Mathf.Floor((float)random.NextDouble() * Ty);
			float pointHeight = (float)random.NextDouble();
			if ((float)random.NextDouble() > voronoiFeatures)
			{
				pointHeight = 0.0f;
			}
			newPeak.peakPoint = new Vector2(xCoord, yCoord);
			newPeak.peakHeight = pointHeight;
			voronoiSet.Add(newPeak);
		}
		int Mx;
		int My;
		float highestScore = 0.0f;
		for (My = 0; My < Ty; My++)
		{
			for (Mx = 0; Mx < Tx; Mx++)
			{
				ArrayList peakDistances = new ArrayList();
				for (i = 0; i < voronoiCells; i++)
				{
					Peak peakI = (Peak)voronoiSet[i];
					Vector2 peakPoint = peakI.peakPoint;
					float distanceToPeak = Vector2.SqrMagnitude(peakPoint - new Vector2(Mx, My));
					PeakDistance newPeakDistance = new PeakDistance();
					newPeakDistance.id = i;
					newPeakDistance.dist = distanceToPeak;
					peakDistances.Add(newPeakDistance);
				}
				peakDistances.Sort();
				PeakDistance peakDistOne = (PeakDistance)peakDistances[0];
				PeakDistance peakDistTwo = (PeakDistance)peakDistances[1];
				int p1 = peakDistOne.id;
				float d1 = Mathf.Sqrt(peakDistOne.dist);
				float d2 = Mathf.Sqrt(peakDistTwo.dist);
				float scale = Mathf.Abs(d1 - d2) / ((Tx + Ty) / Mathf.Sqrt(voronoiCells));
				Peak peakOne = (Peak)voronoiSet[p1];
				float h1 = (float)peakOne.peakHeight;
				float hScore = h1 - Mathf.Abs(d1 / d2) * h1;
				float asRadians;
				switch (voronoiType)
				{
				case VoronoiType.Linear:
					// Nothing...
					break;
				case VoronoiType.Sine:
					asRadians = hScore * Mathf.PI - Mathf.PI / 2;
					hScore = 0.5f + Mathf.Sin(asRadians) / 2;
					break;
				case VoronoiType.Tangent:
					asRadians = hScore * Mathf.PI / 2;
					hScore = 0.5f + Mathf.Tan(asRadians) / 2;
					break;
				}
				hScore = (hScore * scale * voronoiScale) + (hScore * (1.0f - voronoiScale));
				if (hScore < 0.0f)
				{
					hScore = 0.0f;
				}
				else if (hScore > 1.0f)
				{
					hScore = 1.0f;
				}
				currentHeight = hScore;
				if (hScore > highestScore)
				{
					highestScore = hScore;
				}
				newHeightMap[Mx, My] = currentHeight;
			}
		}
		
		// Normalise...
		for (My = 0; My < Ty; My++)
		{
			for (Mx = 0; Mx < Tx; Mx++)
			{
				float normalisedHeight = newHeightMap[Mx, My] * (1.0f / highestScore);
				newHeightMap[Mx, My] = normalisedHeight;
			}
		}

		for (i = 0; i < info.arraySize.x; i++) {
			for (int j = 0; j < info.arraySize.y; j++) {
				info.heightMap[i, j] += (newHeightMap[i, j] - 0.5f) * info.mapToTerrainScale.y;
			}
		}
		
		return info.heightMap;
	}

	public struct Peak {
		public Vector2 peakPoint;
		public float peakHeight;
	}
	
	public class PeakDistance : IComparable {
		public int id;
		public float dist;
		
		public int CompareTo(object obj) {
			PeakDistance Compare = (PeakDistance) obj;
			int result = this.dist.CompareTo(Compare.dist);
			if (result == 0) {
				result = this.dist.CompareTo(Compare.dist);
			}
			return result;
		}
	}
}

