using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TextureAlgorithm {
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

	public virtual IEnumerator FillTexture(Texture2D tex, int pixPerFrame) {
		int w = tex.width;
		int h = tex.height;
		int cx = w / 2;
		int cy = h / 2;
		int rsq = cx * cy;
		
		SetParameters();

		Color clear = new Color(1, 1, 1, 0);
		int pixDone = 0;
		for (int x = 0; x < w; ++x) {
			int xsq = (x - cx) * (x - cx);
			for(int y = 0; y < h; ++y) {
				Color color = xsq + (y - cy) * (y - cy) < rsq ? GetColor(x, y, w, h) : clear;
				tex.SetPixel(x, y, color);
			}
			pixDone += h;
			if(pixDone >= pixPerFrame) {
				pixDone = 0;
				yield return null;
			}
		}
		
		tex.Apply ();
	}

	public virtual void SetParameters() {

	}

	public virtual Color GetColor(int x, int y, int w, int h) {
		return Color.white;
	}
}

public class SingleColor : TextureAlgorithm {
	Color mainColor;

	public override void SetParameters() {
		mainColor = new Color(Random.value, Random.value, Random.value);
	}

	public override Color GetColor(int x, int y, int w, int h) {
		return mainColor;
	}
}

public class LinearGradient : TextureAlgorithm {
	Color firstColor, secondColor;
	bool hor;
	
	public override void SetParameters() {
		firstColor = new Color(Random.value, Random.value, Random.value);
		secondColor = new Color(Random.value, Random.value, Random.value);
		hor = Random.value > 0.5;
	}
	
	public override Color GetColor(int x, int y, int w, int h) {
		return Color.Lerp(firstColor, secondColor, hor ? (float)x/w : (float)y/h);
	}
}

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

public class TextureManager : MonoBehaviour {
	
	List<Texture2D> activeSet;
	List<Texture2D> backupSet;

	public List<int> sizes;
	public int approxPixGenPerFrame = 32;
	int maxPixGenPerFrame = 1000000;

	public float cleanUpDelay = 9.0f;
	
	void Start () {
		if (sizes.Count == 0) {
			Debug.LogWarning("Warning! No texture sizes specified,");
			sizes.Add(32);
		}
		sizes.Sort();

		backupSet = new List<Texture2D>();
		foreach (int s in sizes) {
			backupSet.Add(new Texture2D(s, s));
			StartCoroutine(TextureAlgorithm.GetRandom().FillTexture(backupSet[backupSet.Count - 1], maxPixGenPerFrame));
		}

		SwitchTextureSet();
	}

	public void SwitchTextureSet() {

		if (activeSet != null) {
			foreach (Texture2D tex in activeSet) {
				Destroy (tex, cleanUpDelay);
			}

			activeSet.Clear ();
			activeSet = null;
		}

		activeSet = backupSet;

		backupSet = new List<Texture2D>();
		foreach (Texture2D tex in activeSet) {
			Texture2D newTex = new Texture2D(tex.width, tex.height);
			StartCoroutine(TextureAlgorithm.GetRandom().FillTexture(newTex, approxPixGenPerFrame));
			backupSet.Add(newTex);
		}
	}

	public Texture2D GetTexture(int prefSize) {
		foreach (Texture2D tex in activeSet) {
			if(tex.width >= prefSize) {
				return tex;
			}
		}
		return activeSet[activeSet.Count - 1];
	}

}
