using System.Collections.Generic;
using UnityEngine;
public class BuildingDatas : MonoBehaviour
{
	[SerializeField] private BuildingsData[] buildingsDatas;

	public static BuildingDatas Instance { get; private set; }

	public BuildingsData[] Buildings => buildingsDatas;

	private void Awake()
	{
		if (Instance != null) {
			Destroy(gameObject);
			return;
		}
		Instance = this;
	}

}
