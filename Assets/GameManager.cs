using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour {
	public float minScale = 0.1f;
	public float maxScale = 0.7f;

	public float minTimeBetweenSpawns = 0.2f;
	public float maxTimeBetweenSpawns = 0.8f;

	public float baseSpeed = 0.6f;

	public float diffIncreaseInterval = 10.0f;
	public float diffCoeff = 1.05f;

	float minSpawnX, maxSpawnX, spawnY, bottomY;

	int points;

	TextureManager tm;

	GameObject baseCircle = null;

	public Text score;
	public Text timer;
	
	void Start () {
		points = 0;
		CalcSpawnRange();
		tm = FindObjectOfType<TextureManager>();
		if (score == null || timer == null) {
			Debug.LogError("UI fields not set in editor!");
		}
		StartCoroutine(DownloadAssetBundle());
	}

	IEnumerator DownloadAssetBundle() {
		// Wait for the Caching system to be ready
		while (!Caching.ready)
			yield return null;
		
		// Start the download
		string url = "file://" + Application.dataPath + "/Bundle.unity3d";
		using(WWW www = WWW.LoadFromCacheOrDownload (url, 0)){
			yield return www;
			if (www.error != null) Debug.LogError("WWW download:" + www.error);
			AssetBundle assetBundle = www.assetBundle;
			baseCircle = assetBundle.Load("Circle") as GameObject;
			Instantiate(assetBundle.Load("Background"));
			// Unload the AssetBundles compressed contents to conserve memory
			assetBundle.Unload(false);
			
		} // memory is freed from the web stream (www.Dispose() gets called implicitly)

		if (baseCircle != null) {
			StartCoroutine(IncreaseDifficulty());
			StartCoroutine(SpawnCircles());
		} else {
			Debug.LogError("Fatal error! Couldn't load assets");
		}
	}
	
	IEnumerator IncreaseDifficulty() {
		for (;;) {
			yield return new WaitForSeconds(diffIncreaseInterval);
			baseSpeed *= diffCoeff;
			minTimeBetweenSpawns /= diffCoeff;
			maxTimeBetweenSpawns /= diffCoeff;
			tm.SwitchTextureSet();
		}
	}

	IEnumerator SpawnCircles() {
		yield return null;
		for (;;) {
			float scale = Random.Range(minScale, maxScale);
			Vector3 spawnPos = new Vector3(Random.Range(minSpawnX, maxSpawnX), spawnY, 0);
			GameObject circleObj = Instantiate(baseCircle, spawnPos, Quaternion.identity) as GameObject;
			circleObj.GetComponent<Circle>().Init(tm.GetTexture((int)(scale*250)), scale, baseSpeed, bottomY);
			yield return new WaitForSeconds(Random.Range(minTimeBetweenSpawns, maxTimeBetweenSpawns));
		}
	}

	void CalcSpawnRange() {
		float offset = maxScale / 2;
		Vector3 worldPos = Camera.main.ScreenToWorldPoint (new Vector3 (0, Screen.height, -Camera.main.transform.position.z));
		minSpawnX = worldPos.x + offset;
		spawnY = worldPos.y + offset;
		worldPos = Camera.main.ScreenToWorldPoint (new Vector3 (Screen.width, Screen.height, -Camera.main.transform.position.z));
		maxSpawnX = worldPos.x - offset;
		worldPos = Camera.main.ScreenToWorldPoint (new Vector3 (0, 0, -Camera.main.transform.position.z));
		bottomY = worldPos.y - offset;
	}

	
	void Update () {
		int time = (int)Time.timeSinceLevelLoad;
		int sec =  time % 60;
		int min = time / 60;
		timer.text = min.ToString ("D2") + ":" + sec.ToString ("D2");
	}

	public void OnCircleDestroyed(float scale, bool clicked) {
		if (clicked) {
			points += (int)((maxScale - scale) * 100) + 10;
			score.text = "Points: " + points.ToString();
		}
	}
}
