using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour {

	public int count = 100;
	public float dropTimeLength = 20.0f;
	private float nextDrop = 0.0f;

	public GameObject critterFab;

	public GameObject[] critters;

	// Use this for initialization
	void Start () {
		critters = new GameObject[count];
		for (int i = 0; i < count; i ++) {
			GameObject go = Instantiate(
				critterFab,
				transform.position,
				transform.rotation
			) as GameObject;

			go.active = false;

			critters[i] = go;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (Time.time > nextDrop) {
			nextDrop = Time.time + (dropTimeLength / count);
			DropCritter();
		}
	}

	void DropCritter() {
		if (count > 0) {
			critters[count -1].active = true;

			count = count -1;
		}
	}
}
