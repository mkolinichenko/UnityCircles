using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Базовый класс для алгоритмов генерации текстур
public class TextureAlgorithm {

	//Случайно выбирается один из явно указанных алгоритмов
	public static TextureAlgorithm GetRandom() {
		float val = Random.value;
		if (val < 0.25f) {
			return new SingleColor();
		} else if (val < 0.75f) {
			return new LinearGradient();
		} else {
			return new CircleGradient();
		}
	}

	//Основной метод для заполнения текстуры кружка
	public virtual IEnumerator FillTexture(Texture2D tex, int pixPerFrame) {
		int w = tex.width;
		int h = tex.height;
		int cx = w / 2;
		int cy = h / 2;
		int rsq = cx * cy;
		
		SetParameters();

		Color clear = new Color(1, 1, 1, 0);
		int pixDone = 0;
		//Явным образом рисуем кружок
		for (int x = 0; x < w; ++x) {
			int xsq = (x - cx) * (x - cx);
			for(int y = 0; y < h; ++y) {
				Color color = xsq + (y - cy) * (y - cy) < rsq ? GetColor(x, y, w, h) : clear;
				tex.SetPixel(x, y, color);
			}
			pixDone += h;
			//Заполняем max(h, pixPerFrame) пикселей за кадр
			if(pixDone >= pixPerFrame) {
				pixDone = 0;
				yield return null;
			}
		}
		
		tex.Apply ();
	}

	//Инициализация параметров
	public virtual void SetParameters() {

	}

	//Генерация цвета в определенной точке
	public virtual Color GetColor(int x, int y, int w, int h) {
		return Color.white;
	}
}

//Заполнение одним случайным цветом
public class SingleColor : TextureAlgorithm {
	Color mainColor;

	public override void SetParameters() {
		mainColor = new Color(Random.value, Random.value, Random.value);
	}

	public override Color GetColor(int x, int y, int w, int h) {
		return mainColor;
	}
}

//Заполнение градиентом между двумя случайными цветами
public class LinearGradient : TextureAlgorithm {
	Color firstColor, secondColor;
	bool hor;
	
	public override void SetParameters() {
		firstColor = new Color(Random.value, Random.value, Random.value);
		secondColor = new Color(Random.value, Random.value, Random.value);
		hor = Random.value > 0.5; // 50/50 вертикальный либо горизонтальный градиент
	}
	
	public override Color GetColor(int x, int y, int w, int h) {
		return Color.Lerp(firstColor, secondColor, hor ? (float)x/w : (float)y/h);
	}
}

//Заполнение градиентом по радиусу
public class CircleGradient : TextureAlgorithm {
	Color firstColor, secondColor;

	public override void SetParameters() {
		firstColor = new Color(Random.value, Random.value, Random.value);
		secondColor = new Color(Random.value, Random.value, Random.value);
	}
	
	public override Color GetColor(int x, int y, int w, int h) {
		float r = w / 2;
		float px = x - r;
		float py = y - r;
		return Color.Lerp(firstColor, secondColor, (px*px + py*py) / (r*r) );
	}
}


//Основной класс для управления текстурами
public class TextureManager : MonoBehaviour {

	//Сеты текстур разных размеров
	List<Texture2D> activeSet;
	List<Texture2D> backupSet;

	[Tooltip("Texture sizes")]
	public List<int> sizes;
	[Tooltip("Pixels of texture filled in one frame")]
	public int approxPixGenPerFrame = 32;
	int maxPixGenPerFrame = 1000000;

	[Tooltip("Delay between texture set switch and destruction of old set in seconds")]
	public float cleanUpDelay = 9.0f;
	
	void Start () {
		if (sizes.Count == 0) {
			Debug.LogWarning("Warning! No texture sizes specified, adding default");
			sizes.Add(32);
		}
		//Отсортируем размеры текстур по возрастанию
		//Сохраним этот порядок сортировки во всех сетах текстур
		sizes.Sort();

		//Создаём и полностью заполняем все текстуры в backupSet
		backupSet = new List<Texture2D>();
		foreach (int s in sizes) {
			backupSet.Add(new Texture2D(s, s));
			StartCoroutine(TextureAlgorithm.GetRandom().FillTexture(backupSet[backupSet.Count - 1], maxPixGenPerFrame));
		}

		//Замена activeSet на backupSet
		SwitchTextureSet();
	}

	public void SwitchTextureSet() {
		//Очищаем текущий набор
		if (activeSet != null) {
			foreach (Texture2D tex in activeSet) {
				Destroy (tex, cleanUpDelay);
			}

			activeSet.Clear ();
			activeSet = null;
		}

		//Меняем на ранее заготовленный backupSet
		activeSet = backupSet;

		//Запускаем постепенное заполнение backupSet
		backupSet = new List<Texture2D>();
		foreach (Texture2D tex in activeSet) {
			Texture2D newTex = new Texture2D(tex.width, tex.height);
			StartCoroutine(TextureAlgorithm.GetRandom().FillTexture(newTex, approxPixGenPerFrame));
			backupSet.Add(newTex);
		}
	}

	//Выбираем наименьшую текстуру размером не меньше prefSize
	public Texture2D GetTexture(int prefSize) {
		foreach (Texture2D tex in activeSet) {
			if(tex.width >= prefSize) {
				return tex;
			}
		}
		return activeSet[activeSet.Count - 1];
	}

}
