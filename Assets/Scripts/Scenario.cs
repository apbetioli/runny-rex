using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scenario : MonoBehaviour {

	public float velocity = 5f;

	public GameObject groundPrefab;
	public float groundSize = 500f;
	public float groundCount = 2;
	private Queue<GameObject> groundPool = new Queue<GameObject>();

	public float spawnPositionX;
	public float destroyPositionX;

	public GameObject[] groundObstaclesPrefabs;
	public float minTimeBetweenObstacles = 1f;
	public float maxTimeBetweenObstacles = 4f;
	private float timeBetweenObstacles;
	private float sumTimeBetweenObstacles = 0f;
	private Queue<GameObject> obstacles = new Queue<GameObject> ();

	public GameObject[] cloudsPrefabs;
	public float cloudsVelocity = 40f;
	public float minTimeBetweenClouds = 1f;
	public float maxTimeBetweenClouds = 4f;
	public float minCloudsHeight = 30f;
	public float maxCloudsHeight = 50f;
	private float timeBetweenClouds;
	private float sumTimeBetweenClouds = 0f;
	private Queue<GameObject> clouds = new Queue<GameObject> ();

	public GameObject[] flyingObstaclesPrefabs;
	public float minFlyingObstaclesHeight = 40f;
	public float maxFlyingObstaclesHeight = 5f;

	void Start () {
		CreateGround ();
		timeBetweenObstacles = RandomTimeBetweenObstacles ();
	}
	
	void Update () {
		UpdateGroundPosition ();
		CreateGroundObstacles ();
		CreateClouds ();
	}

	private float RandomTimeBetweenObstacles() {
		return Random.Range (minTimeBetweenObstacles, maxTimeBetweenObstacles);
	}

	private float RandomTimeBetweenClouds() {
		return Random.Range (minTimeBetweenClouds, maxTimeBetweenClouds);
	}

	private void CreateGroundObstacles() {
		if (sumTimeBetweenObstacles < timeBetweenObstacles)
			sumTimeBetweenObstacles += Time.deltaTime;
		else {
			timeBetweenObstacles = RandomTimeBetweenObstacles();
			sumTimeBetweenObstacles = 0f;

			GameObject obstacle = SpawnObstacle ();
			obstacles.Enqueue (obstacle);
		}

		if(obstacles.Count > 0 && obstacles.Peek ().transform.position.x < destroyPositionX) {
			GameObject old = obstacles.Dequeue ();
			Destroy (old);
		}

		foreach(GameObject obj in obstacles) {
			obj.transform.position += Vector3.left * velocity * Time.deltaTime;
		}
	}

	private GameObject SpawnObstacle() {
		// TODO depois com a pontuação só aparecer os flying a partir de 500 pontos
		if (Random.Range (0f, 1f) < 0.7f)
			return SpawnGroundObstacle ();
		else
			return SpawnFlyingObstacle ();
	}

	private GameObject SpawnGroundObstacle() {
		int index = Random.Range (0, groundObstaclesPrefabs.Length);
		GameObject obstacle = Instantiate (groundObstaclesPrefabs [index]);
		obstacle.transform.parent = transform;
		obstacle.transform.position = new Vector3(spawnPositionX, 0 , 0);
		return obstacle;
	}

	private GameObject SpawnFlyingObstacle() {
		int index = Random.Range (0, flyingObstaclesPrefabs.Length);
		GameObject obstacle = Instantiate (flyingObstaclesPrefabs [index]);
		obstacle.transform.parent = transform;
		float obstacleY = Random.Range (minFlyingObstaclesHeight, maxFlyingObstaclesHeight);
		obstacle.transform.position = new Vector3(spawnPositionX, obstacleY, 0);
		return obstacle;
	}

	private void CreateClouds() {
		if (sumTimeBetweenClouds < timeBetweenClouds)
			sumTimeBetweenClouds += Time.deltaTime;
		else {
			timeBetweenClouds = RandomTimeBetweenClouds();
			sumTimeBetweenClouds = 0f;
			int index = Random.Range (0, cloudsPrefabs.Length);
			GameObject cloud = Instantiate (cloudsPrefabs [index]);
			cloud.transform.parent = transform;
			float spawnPositionY = Random.Range (minCloudsHeight, maxCloudsHeight);
			cloud.transform.position = new Vector3(spawnPositionX, spawnPositionY, 0);
			clouds.Enqueue (cloud);
		}

		if(clouds.Count > 0 && clouds.Peek ().transform.position.x < destroyPositionX) {
			GameObject old = clouds.Dequeue ();
			Destroy (old);
		}

		foreach(GameObject obj in clouds) {
			obj.transform.position += Vector3.left * cloudsVelocity * Time.deltaTime;
		}
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
			obj.transform.position += Vector3.left * velocity * Time.deltaTime;
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
