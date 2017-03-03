using UnityEngine.SceneManagement;
using UnityEngine;

public class Start : MonoBehaviour {

	public void Awake()
    {
        if (!GameManager.Playing)
            SceneManager.LoadScene("Run", LoadSceneMode.Additive);
    }
    public void Restart()
    {
        GameManager.Restart();
    }
}
