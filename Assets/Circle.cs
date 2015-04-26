using UnityEngine;
using System.Collections;

public class Circle : MonoBehaviour {

	float speed = 0.0f;
	float bottomY = 0.0f;

	//Инициализируем кружок данными из GameManager
	public void Init(Texture2D tex, float scale, float baseSpeed, float bottom) {
		//Создание спрайта из текстуры
		Rect texRect = new Rect (0, 0, tex.width, tex.height);
		Vector2 pivot = new Vector2 (0.5f, 0.5f);
		float texScaleToWorld = tex.width;
		GetComponent<SpriteRenderer>().sprite = Sprite.Create(tex, texRect, pivot, texScaleToWorld);

		bottomY = bottom; //Точка уничтожения кружка по оси Y в мировых координатах
		speed = baseSpeed - scale; //Скорость падения, убывает с увеличением размера
		//Для задания размера достаточно поменять transform, collider и текстура изменятся корректно
		transform.localScale = new Vector3(scale, scale, 1);
	}

	void Update () {
		//Кружок падает с постоянной скоростью вниз по оси Y
		transform.Translate (Vector3.down * Time.deltaTime * speed);
		//При достижении нижней границы кружок уничтожается, оповещается GameManager
		if(transform.position.y < bottomY) {
			FindObjectOfType<GameManager>().OnCircleDestroyed(transform.localScale.x, false);
			Destroy(gameObject);
		}
	}

	void OnMouseDown() {
		//При щелчке по кружку оповещаем GameManager, играем короткий звук
		FindObjectOfType<GameManager>().OnCircleDestroyed(transform.localScale.x, true);
		audio.Play();
		//Предотвращаются повторные нажатия, объект уничтожается по окончанию проигрывания
		collider2D.enabled = false;
		Destroy(gameObject, audio.clip.length);
	}

	void OnDestroy() {
		//Уничтожаем созданный Sprite
		Destroy(GetComponent<SpriteRenderer>().sprite);
	}
}
