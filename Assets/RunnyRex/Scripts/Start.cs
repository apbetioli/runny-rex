using UnityEngine;

public class Start : MonoBehaviour
{

    void Update()
    {
        if (Input.GetButton("Jump"))
            Restart();
    }

    public void Restart()
    {
        GameManager.Restart();
    }

	public void ShowLeaderboard() 
	{
		GameManager.Instance.ShowLeaderboard ();
	}

	public void ShowAchievements()
	{
		GameManager.Instance.ShowAchievements ();
	}
}
