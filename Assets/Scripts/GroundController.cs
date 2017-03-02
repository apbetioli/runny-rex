using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundController : MonoBehaviour {

	public float speedRatio = 1f;

	public GameObject groundPrefab;
	public float groundSize = 500f;
	public float groundCount = 2;
	private Queue<GameObject> groundPool = new Queue<GameObject>();

	void Start () {
		CreateGround ();
	}
	
	void Update () {
		UpdateGroundPosition ();
	}

	private void CreateGround() {
		float x = 0;
		for (int i = 0; i < groundCount; i++) {
			GameObject ground = Instantiate (groundPrefab);
			ground.transform.parent = transform;
			ground.transform.position = new Vector3 (x, 0, 0);
			groundPool.Enqueue (ground);
			x += groundSize;
		}
	}

	private void UpdateGroundPosition() {
		foreach(GameObject obj in groundPool) {
			float speed = speedRatio * GameManager.Instance.speed;
			obj.transform.position += Vector3.left * speed * Time.deltaTime;
		}

		if (groundPool.Peek ().transform.position.x < -groundSize) {
			GameObject old = groundPool.Dequeue ();
			Vector3 position = old.transform.position;
			position.x = groundSize * (groundCount -1);
			old.transform.position = position;
			groundPool.Enqueue (old);
		}
	}
}
