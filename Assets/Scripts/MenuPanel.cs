using UnityEngine;
using System.Collections;

public class MenuPanel : MonoBehaviour {

	public bool visible = false;

	// Use this for initialization
	void Start () {
		if(visible) gameObject.GetComponent<Animator>().SetBool("IsVisible", true);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void ShowPanel() {
		gameObject.GetComponent<Animator>().SetBool("IsVisible", true);
	}

	public void HidePanel() {
		gameObject.GetComponent<Animator>().SetBool("IsVisible", false);
	}
}
