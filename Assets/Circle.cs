using UnityEngine;
using System.Collections;

public class Circle : MonoBehaviour {

	float speed = 0.0f;
	float bottomY = 0.0f;

	public void Init(Texture2D tex, float scale, float baseSpeed, float bottom) {
		Rect texRect = new Rect (0, 0, tex.width, tex.height);
		Vector2 pivot = new Vector2 (0.5f, 0.5f);
		float texScaleToWorld = tex.width;
		GetComponent<SpriteRenderer>().sprite = Sprite.Create(tex, texRect, pivot, texScaleToWorld);

		bottomY = bottom;
		speed = baseSpeed + 0.4f - scale;
		transform.localScale = new Vector3(scale, scale, 1);
	}

	void Update () {
		transform.Translate (Vector3.down * Time.deltaTime * speed);
		if(transform.position.y < bottomY) {
			FindObjectOfType<GameManager>().OnCircleDestroyed(transform.localScale.x, false);
			Destroy(gameObject);
		}
	}

	void OnMouseDown() {
		FindObjectOfType<GameManager>().OnCircleDestroyed(transform.localScale.x, true);
		audio.Play();
		collider2D.enabled = false;
		Destroy(gameObject, audio.clip.length);
	}

	void OnDestroy() {
		Destroy(GetComponent<SpriteRenderer>().sprite);
	}
}
