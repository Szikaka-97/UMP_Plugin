using UnityEngine;
using UnityEngine.UI;

namespace UMP_Plugin.Description {
	public class UMPColorSliderGroup : MonoBehaviour {
		public UMPDescriptionScript description;
		public Slider red_slider;
		public Slider green_slider;
		public Slider blue_slider;

		void Update() {
			red_slider.fillRect.GetComponent<Image>().color = new Color(red_slider.value, 0, 0);
			green_slider.fillRect.GetComponent<Image>().color = new Color(0, green_slider.value, 0);
			blue_slider.fillRect.GetComponent<Image>().color = new Color(0, 0, blue_slider.value);
		}

		public void SetSliders(Color color) {
			red_slider.SetValueWithoutNotify(color.r);
			green_slider.SetValueWithoutNotify(color.g);
			blue_slider.SetValueWithoutNotify(color.b);
		}
	}
}
