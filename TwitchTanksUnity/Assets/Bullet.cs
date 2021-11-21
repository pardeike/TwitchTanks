using UnityEngine;

class Bullet : MonoBehaviour
{
	internal float distance;
	internal Vector3 destination;
	internal int idx;
	float time = 0f;
	Vector3 start;
	float ground;

	public void Start()
	{
		start = gameObject.transform.position;
		ground = start.y;
	}

	public void Update()
	{
		time += 1f;
		if (time > distance)
		{
			Destroy(gameObject);
			Game.GetInstance().Explode(gameObject.transform.position);
			return;
		}

		var f = Mathf.Clamp01(time / distance);
		var pos = start + (destination - start) * f;
		var h = Mathf.Sin(f * Mathf.PI);
		pos.y = ground + 10 * h;
		gameObject.transform.position = pos;
		gameObject.transform.localScale = Vector3.one * Mathf.Lerp(4, 20, h);
	}
}
