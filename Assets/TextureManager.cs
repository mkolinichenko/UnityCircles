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
	
	List<Texture2D> activeSet = new List<Texture2D>();
	List<Texture2D> backupSet = new List<Texture2D>();

	public List<int> sizes;
	
	void Start () {
		if (sizes.Count == 0) sizes.Add (32);
		sizes.Sort();
		foreach (int s in sizes) {
			activeSet.Add(new Texture2D(s, s));
			backupSet.Add(new Texture2D(s, s));
			StartCoroutine(TextureAlgorithm.GetRandom().FillTexture(backupSet[backupSet.Count - 1], 1000000));
		}
		SwitchTextureSet();
	}

	void SwitchTextureSet() {
		List<Texture2D> tmp = activeSet;
		activeSet = backupSet;
		backupSet = tmp;
		foreach (Texture2D tex in backupSet) {
			StartCoroutine(TextureAlgorithm.GetRandom().FillTexture(tex, 32));
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

	void Update () {
	
	}
}
