using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioElement : MonoBehaviour {

	public float spawnPositionX;
	public float destroyPositionX;

	public GameObject[] prefabs;
	public float velocity = 40f;
	public float minTime = 1f;
	public float maxTime = 4f;
	public float minY = 30f;
	public float maxY = 50f;

	private float time = 0f;
	private float sumTime = 0f;
	private Queue<GameObject> pool = new Queue<GameObject> ();

	void Start () {
		time = RandomTime();
	}
	
	void Update () {
		CreateElements();
	}

	private void CreateElements() {
		if (sumTime < time)
			sumTime += Time.deltaTime;
		else {
			time = RandomTime();
			sumTime = 0f;

			GameObject obj = Spawn ();
			pool.Enqueue (obj);
		}

		if(pool.Count > 0 && pool.Peek ().transform.position.x < destroyPositionX) {
			GameObject old = pool.Dequeue ();
			Destroy (old);
		}

		foreach(GameObject obj in pool) {
			obj.transform.position += Vector3.left * velocity * Time.deltaTime;
		}
	}

	private GameObject Spawn() {
		int index = Random.Range (0, prefabs.Length);
		float spawnPositionY = Random.Range (minY, maxY);

		GameObject obj = Instantiate (prefabs [index]);
		obj.transform.parent = transform;
		obj.transform.position = new Vector3(spawnPositionX, spawnPositionY, 0);
		return obj;
	}

	private float RandomTime() {
		return Random.Range (minTime, maxTime);
	}


}
