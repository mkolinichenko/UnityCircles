using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour {
	[Tooltip("Min scale of the circles")]
	public float minScale = 0.1f;
	[Tooltip("Max scale of the circles")]
	public float maxScale = 0.7f;

	[Tooltip("Min time between spawning two consecutive circles")]
	public float minTimeBetweenSpawns = 0.2f;
	[Tooltip("Max time between spawning two consecutive circles")]
	public float maxTimeBetweenSpawns = 0.8f;

	[Tooltip("Base speed of falling circles")]
	public float baseSpeed = 0.6f;

	[Tooltip("Interval in seconds between difficulty levels")]
	public float diffIncreaseInterval = 10.0f;
	[Tooltip("Multiplier for base speed and time between spawns")]
	public float diffCoeff = 1.05f;

	//Точки, привязанные к границам экрана
	float minSpawnX, maxSpawnX, spawnY, bottomY;

	//Заработанные очки
	int points;

	TextureManager tm;

	//Прототип для кружков
	GameObject baseCircle = null;

	[Tooltip("Score text field")]
	public Text score;
	[Tooltip("Timer text field")]
	public Text timer;

	//Вычисление границ экрана в мировых координатах
	void CalcSpawnRange() {
		float offset = maxScale / 2;//Кружки создаются и уничтожаются за границами видимости
		Vector3 worldPos = Camera.main.ScreenToWorldPoint (new Vector3 (0, Screen.height, -Camera.main.transform.position.z));
		minSpawnX = worldPos.x + offset;
		spawnY = worldPos.y + offset;
		worldPos = Camera.main.ScreenToWorldPoint (new Vector3 (Screen.width, Screen.height, -Camera.main.transform.position.z));
		maxSpawnX = worldPos.x - offset;
		worldPos = Camera.main.ScreenToWorldPoint (new Vector3 (0, 0, -Camera.main.transform.position.z));
		bottomY = worldPos.y - offset;
	}
	
	void Start () {
		points = 0;
		CalcSpawnRange();
		tm = FindObjectOfType<TextureManager>();
		if (score == null || timer == null) {
			Debug.LogError("UI fields not set in editor!");
		}
		StartCoroutine(DownloadAssetBundle()); //Начинаем с загрузки бандла
	}

	IEnumerator DownloadAssetBundle() {
		//Ждем готовности кэша
		while (!Caching.ready)
			yield return null;
		
		//Начинаем загрузку из папки dataPath
		string url = "file://" + Application.dataPath + "/Bundle.unity3d";
		using(WWW www = WWW.LoadFromCacheOrDownload (url, 0)){
			yield return www;
			if (www.error != null) Debug.LogError("WWW download:" + www.error);
			AssetBundle assetBundle = www.assetBundle;
			//Запоминаем прототип кружка
			baseCircle = assetBundle.Load("Circle") as GameObject;
			//Фон загружаем сразу
			Instantiate(assetBundle.Load("Background"));
			assetBundle.Unload(false);
			
		}

		if (baseCircle != null) {
			//Фактический запуск игры, если удалось загрузить бандл
			StartCoroutine(IncreaseDifficulty());
			StartCoroutine(SpawnCircles());
		} else {
			Debug.LogError("Fatal error! Couldn't load assets");
		}
	}

	//Повышаем сложность каждые diffIncreaseInterval секунд
	IEnumerator IncreaseDifficulty() {
		for (;;) {
			yield return new WaitForSeconds(diffIncreaseInterval);
			baseSpeed *= diffCoeff;
			minTimeBetweenSpawns /= diffCoeff;
			maxTimeBetweenSpawns /= diffCoeff;
			tm.SwitchTextureSet(); //Подменяем текстуры
		}
	}

	//Создаем кружки
	IEnumerator SpawnCircles() {
		yield return null;
		for (;;) {
			float scale = Random.Range(minScale, maxScale);
			Vector3 spawnPos = new Vector3(Random.Range(minSpawnX, maxSpawnX), spawnY, 0);
			GameObject circleObj = Instantiate(baseCircle, spawnPos, Quaternion.identity) as GameObject;
			//Формула для вычисления размера текстуры по размеру кружка
			Texture2D tex = tm.GetTexture((int)(scale*250));
			circleObj.GetComponent<Circle>().Init(tex, scale, baseSpeed, bottomY);
			yield return new WaitForSeconds(Random.Range(minTimeBetweenSpawns, maxTimeBetweenSpawns));
		}
	}


	
	void Update () {
		//Обновляем таймер
		int time = (int)Time.timeSinceLevelLoad;
		int sec =  time % 60;
		int min = time / 60;
		timer.text = min.ToString ("D2") + ":" + sec.ToString ("D2");
	}

	public void OnCircleDestroyed(float scale, bool clicked) {
		//Если кружок уничтожен кликом, то начисляем очки
		if (clicked) {
			points += (int)((maxScale - scale) * 100) + 10;
			score.text = "Points: " + points.ToString();
		}
	}
}
