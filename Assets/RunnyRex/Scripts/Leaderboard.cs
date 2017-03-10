using UnityEngine;
using UnityEngine.SocialPlatforms;
using System.Collections;
using GooglePlayGames;

/*
 * Controls the interaction with Google play leaderboard
 */
public class Leaderboard : MonoBehaviour {

	void Start() {
		Activate();
		Authenticate ();
	}

	public void Activate() {
		#if UNITY_ANDROID
		PlayGamesPlatform.Activate();
		Debug.Log("Play Game Platform Activated");
		#endif
	}

	public void Authenticate() {
		#if UNITY_ANDROID
		Social.localUser.Authenticate((auth,msg) => {
			Debug.Log(msg);	

			if (auth) {
				Debug.Log ("Authentication successful");
				Debug.Log ("Username: " + Social.localUser.userName + "\nUser ID: " + Social.localUser.id + "\nIsUnderage: " + Social.localUser.underage);
			}
			else {
				Debug.LogWarning ("Authentication failed");
			}
		});	
		#endif
	}

	public void ReportHighScore() {
		#if UNITY_ANDROID
		if (Social.localUser.authenticated) {
			Social.ReportScore(Storage.Highscore, GPGSIds.leaderboard_long_run, success => {
				if(success)
					Debug.Log("Report score ok");
				else
					Debug.LogWarning("Report score failed");
			});
		}
		else {
			Debug.LogWarning("Unable to report score. Not Authenticated.");
		}
		#endif
	}

	public void ShowLeaderboard () {
		#if UNITY_ANDROID
		Social.localUser.Authenticate ((auth,msg) => {
			Debug.Log(msg);

			if (auth) {
				Debug.Log("Showing leaderboard");
				//Social.ShowLeaderboardUI();
				PlayGamesPlatform.Instance.ShowLeaderboardUI(GPGSIds.leaderboard_long_run);

			} else {
				Debug.LogWarning("Authentication failed");
			}
		});
		#endif
	}

	public void ShowAchievements () {
		#if UNITY_ANDROID
		Social.localUser.Authenticate ((auth,msg) => {
			Debug.Log(msg);

			if (auth) {
				Debug.Log("Showing achievements");
				PlayGamesPlatform.Instance.ShowAchievementsUI();

			} else {
				Debug.LogWarning("Authentication failed");
			}
		});
		#endif
	}

	public void UnlockAchievement(string id) {
		#if UNITY_ANDROID
		Social.localUser.Authenticate ((auth,msg) => {
			Debug.Log(msg);

			if (auth) {
				Social.ReportProgress (id, 100.0f, success => {
					if(!success)
						Debug.LogWarning("Error unlocking achievement");
				});
			} else {
				Debug.LogWarning("Authentication failed");
			}
		});
		#endif
	}
}
