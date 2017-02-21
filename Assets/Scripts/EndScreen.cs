using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class EndScreen : MonoBehaviour {

	// Use this for initialization
	void Start () {
        StartCoroutine(EndScreenTimer());
	}
	
    public IEnumerator EndScreenTimer()
    {
        yield return new WaitForSeconds(3);

        SceneManager.LoadScene("Scenes/Main");

        StopAllCoroutines();
    }


	
}
