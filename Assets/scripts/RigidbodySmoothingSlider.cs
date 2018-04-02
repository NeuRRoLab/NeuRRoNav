using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RigidbodySmoothingSlider : MonoBehaviour {

	Slider slider;

	// Use this for initialization
	void Start () {
		slider = this.GetComponent<Slider>();
		UpdateValue();
	}

	public void UpdateValue() {
		TransformSmoother.maxSaves = Mathf.RoundToInt(slider.value);
	}

}
