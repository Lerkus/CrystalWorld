using UnityEngine;
using System.Collections;

public class Follow : MonoBehaviour {

    [HideInInspector]
    public GameObject ObjectToFollow;

    public void Initialize(GameObject objectToFollow)
    {
        ObjectToFollow = objectToFollow;
    }

	void Update () {
        transform.position = ObjectToFollow.transform.position;
	}
}
