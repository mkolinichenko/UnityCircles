using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {
	public float minScale = 0.1f;
	public float maxScale = 0.7f;

	public float minTimeBetweenSpawns = 0.2f;
	public float maxTimeBetweenSpawns = 0.8f;

	public float baseSpeed = 0.6f;

	float minSpawnX, maxSpawnX, spawnY;

	TextureManager tm;

	public GameObject baseCircle;
	
	void Start () {
		CalcSpawnRange();
		tm = FindObjectOfType<TextureManager>();
	}

	void CalcSpawnRange() {
		float offset = maxScale / 2;
		Vector3 worldPos = Camera.main.ScreenToWorldPoint (new Vector3 (0, Screen.height, -Camera.main.transform.position.z));
		minSpawnX = worldPos.x + offset;
		spawnY = worldPos.y + offset;
		worldPos = Camera.main.ScreenToWorldPoint (new Vector3 (Screen.width, Screen.height, -Camera.main.transform.position.z));
		maxSpawnX = worldPos.x - offset;
	}

	float timer = 0.0f;
	void Update () {
		if (timer < 0) {
			timer = Random.Range(minTimeBetweenSpawns, maxTimeBetweenSpawns);
			//float scale = ((int)(Random.Range(minScale, maxScale) * 10)) / 10.0f;
			float scale = Random.Range(minScale, maxScale);
			Vector3 spawnPos = new Vector3(Random.Range(minSpawnX, maxSpawnX), spawnY, 0);
			GameObject circleObj = Instantiate(baseCircle, spawnPos, Quaternion.identity) as GameObject;
			Circle circle = circleObj.GetComponent<Circle>();
			circle.Init(tm.GetTexture((int)(scale*250)), scale, baseSpeed);
		} else {
			timer -= Time.deltaTime;
		}
	}
}
