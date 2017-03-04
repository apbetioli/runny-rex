using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioElementController : MonoBehaviour {

	public ScenarioElement[] prefabs;
	public Transform spawnPoint;
	public Transform destroyPoint;
	public float minTime = 1f;
	public float maxTime = 4f;
	public float speedRatio = 1f;

	private float time = 0f;
	private float sumTime = 0f;
	private Queue<GameObject> pool = new Queue<GameObject> ();

	void Start () {
		time = RandomTime();
	}
	
	void Update () {
		if(!GameManager.Playing)
            return;
			
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

		if(pool.Count > 0 && pool.Peek ().transform.position.x < destroyPoint.position.x) {
			GameObject old = pool.Dequeue ();
			Destroy (old);
		}

		foreach(GameObject obj in pool) {
			float speed = speedRatio * GameManager.Instance.speed;
			obj.transform.position += Vector3.left * speed * Time.deltaTime;
		}
	}

	private GameObject Spawn() {
		int index = Random.Range (0, prefabs.Length);
		ScenarioElement element = prefabs [index];

		if (GameManager.Instance.Score < element.minScore)
			return Spawn (); 

		float spawnPositionY = Random.Range (element.minY, element.maxY);

		GameObject obj = Instantiate (element.gameObject);
		obj.transform.parent = transform;
		obj.transform.position = new Vector3(spawnPoint.position.x, spawnPositionY, 0);
		return obj;
	}

	private float RandomTime() {
		return Random.Range (minTime, maxTime);
	}

}
