using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour {

	public int count = 100;
	public float dropTimeLength = 20.0f;
	private float nextDrop = 0.0f;
	private float dropIncriment;
	private int remaining;

	private bool playing = false;

	public GameObject critterFab;

	public GameObject[] critters;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		if (playing) {
			if (Time.time > nextDrop) {
				nextDrop = Time.time + dropIncriment;
				DropCritter();
			}
		}
	}

	void Reset() {
		int children = transform.childCount;
        for (int i = 0; i < children; i++) {
            GameObject.Destroy(transform.GetChild(i).gameObject);
        }

        Level.instance.AddCritterTotal(count);

		playing = false;
		remaining = count;
		critters = new GameObject[count];

		dropIncriment = dropTimeLength / count;

		for (int i = 0; i < count; i ++) {
			GameObject go = Instantiate(
				critterFab,
				transform.position,
				Quaternion.identity
			) as GameObject;

			go.transform.parent = transform;

			go.SetActive(false);

			critters[i] = go;
		}

	}

	void Activate() {
		playing = true;
	}

	void DropCritter() {
		if (remaining > 0) {
			critters[remaining -1].SetActive(true);
			Level.instance.SpawnCritter();

			remaining = remaining - 1;
		}
	}
}
