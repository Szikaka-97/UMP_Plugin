using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Receiver2;

namespace UMP_Plugin.Description {
	public class UMPDescriptionScript : MonoBehaviour {
		public static UMPDescriptionScript instance;

		public GameObject rotating_object;
		//public Camera view_camera;

		public TMP_Dropdown barrel_dropdown;
		public TMP_Dropdown bottom_rail_dropdown;
		public TMP_Dropdown left_rail_dropdown;
		public TMP_Dropdown right_rail_dropdown;
		public TMP_Dropdown top_rail_dropdown;

		public UMPColorSliderGroup barrel_sliders;
		public UMPColorSliderGroup bottom_rail_sliders;
		public UMPColorSliderGroup left_rail_sliders;
		public UMPColorSliderGroup right_rail_sliders;
		public UMPColorSliderGroup top_rail_sliders;

		private GraphicRaycaster raycaster;
		private Vector3 prev_mouse_position;
		private float rotation_velocity;
		private List<Attachments.ModularAttachmentPoint> attachment_points;

		public static System.Collections.IEnumerator LoadDescription() {
			foreach (AssetBundle ab in AssetBundle.GetAllLoadedAssetBundles()) {
				if (!ab.Contains("Assets/Guns/UMP/Description/UMP Description.prefab")) continue;

				var request = ab.LoadAssetAsync<GameObject>("Assets/Guns/UMP/Description/UMP Description.prefab");

				while (!request.isDone) yield return null;

				instance = Instantiate((GameObject) request.asset).GetComponent<UMPDescriptionScript>();
				instance.gameObject.SetActive(false);

				instance.transform.SetParent(GameObject.Find("ReceiverCore/Menus/Overlay Menu Canvas/Aspect Ratio Fitter/New Pause Menu/Backdrop1/Sub-Menu Layout Group/New Help Menu/New Description Component").transform);
				instance.transform.localScale = Vector3.one;
				instance.transform.localPosition = Vector3.zero;

				yield break;
			}
		}

		public static void Open() {
			instance.gameObject.SetActive(true);
			instance.transform.parent.Find("ScrollableContent Variant").gameObject.SetActive(false);
			instance.UpdateAttachments();

			instance.bottom_rail_sliders.SetSliders(GunConfig.GetAttachmentColorOnPoint(instance.attachment_points[0]));
			instance.left_rail_sliders.SetSliders(GunConfig.GetAttachmentColorOnPoint(instance.attachment_points[1]));
			instance.right_rail_sliders.SetSliders(GunConfig.GetAttachmentColorOnPoint(instance.attachment_points[2]));
			instance.top_rail_sliders.SetSliders(GunConfig.GetAttachmentColorOnPoint(instance.attachment_points[3]));
			instance.barrel_sliders.SetSliders(GunConfig.GetAttachmentColorOnPoint(instance.attachment_points[4]));
		}

		public static void Close() {
			instance.gameObject.SetActive(false);
			instance.transform.parent.Find("ScrollableContent Variant").gameObject.SetActive(true);
		}

		void Awake() {
			raycaster = GameObject.Find("ReceiverCore/Menus/Overlay Menu Canvas").GetComponent<GraphicRaycaster>();

			attachment_points = new List<Attachments.ModularAttachmentPoint>(rotating_object.GetComponentsInChildren<Attachments.ModularAttachmentPoint>());

			bottom_rail_dropdown.onValueChanged.AddListener( delegate(int value) { GunConfig.SetAttachmentOnPoint(attachment_points[0], bottom_rail_dropdown.options[value].text);});
			left_rail_dropdown.onValueChanged.AddListener( delegate(int value) { GunConfig.SetAttachmentOnPoint(attachment_points[1], left_rail_dropdown.options[value].text);});
			right_rail_dropdown.onValueChanged.AddListener( delegate(int value) { GunConfig.SetAttachmentOnPoint(attachment_points[2], right_rail_dropdown.options[value].text);});
			barrel_dropdown.onValueChanged.AddListener( delegate(int value) { GunConfig.SetAttachmentOnPoint(attachment_points[4], barrel_dropdown.options[value].text);});
			top_rail_dropdown.onValueChanged.AddListener( delegate(int value) { GunConfig.SetAttachmentOnPoint(attachment_points[3], top_rail_dropdown.options[value].text);});		
		}

		void Update() {
			if (Input.GetKeyDown(KeyCode.Escape)) Close();

			if (Input.GetMouseButtonDown(0)) {
				var event_data = new PointerEventData(GetComponent<EventSystem>());
				event_data.position = Input.mousePosition;
				List<RaycastResult> results = new List<RaycastResult>();

				raycaster.Raycast(event_data, results);

				if (results.Any(result => result.gameObject.GetComponent<SelectableButton>() != null)) {
					Close();

					return;
				}
			}
			
			if (Input.GetMouseButton(0)) {
				rotation_velocity = Vector3.Dot(Vector3.right, (prev_mouse_position - Input.mousePosition));
			}
			rotating_object.transform.localEulerAngles += Vector3.up * rotation_velocity;

			prev_mouse_position = Input.mousePosition;
			rotation_velocity *= 0.9f;

			UpdateLabelColors();
		}

		public void UpdateAttachments() {

			UpdateAttachmentLabels();

			foreach (var point in attachment_points) {
				if (point.attachment != null && point.attachment.name != GunConfig.GetAttachmentOnPoint(point)) {
					DestroyImmediate(point.attachment.gameObject);
					point.attachment = null;
				}

				if (point.attachment == null && GunConfig.GetAttachmentOnPoint(point) != "Empty") {
					point.attachment = Instantiate(MainPlugin.attachments[GunConfig.GetAttachmentOnPoint(point)].gameObject).GetComponent<Attachments.ModularAttachment>();
					point.attachment.enabled = false;
					point.attachment.attachment_point = point;

					point.attachment.transform.SetParent(point.transform);
					
					point.attachment.transform.localScale = Vector3.one;
					point.attachment.transform.position = point.mount_point.position;
					point.attachment.transform.rotation = point.mount_point.rotation;

					foreach (var child in point.attachment.GetComponentsInChildren<Transform>()) child.gameObject.layer = 8;
				}
			}
		}

		private void UpdateLabelColor(Attachments.ModularAttachmentPoint point, UMPColorSliderGroup sliders, TMP_Dropdown dropdown) {
			if (point.attachment != null && point.attachment is Attachments.IColorableAttachment) {
				sliders.gameObject.SetActive(true);

				dropdown.captionText.faceColor = GunConfig.GetAttachmentColorOnPoint(point);

				GunConfig.SetAttachmentColorOnPoint(point, new Color(sliders.red_slider.value, sliders.green_slider.value, sliders.blue_slider.value));
			}
			else {
				sliders.gameObject.SetActive(false);
				dropdown.captionText.faceColor = Color.gray;
			}
		}

		private void UpdateLabelColors() {
			UpdateLabelColor(attachment_points[0], bottom_rail_sliders, bottom_rail_dropdown);
			UpdateLabelColor(attachment_points[1], left_rail_sliders, left_rail_dropdown);
			UpdateLabelColor(attachment_points[2], right_rail_sliders, right_rail_dropdown);
			UpdateLabelColor(attachment_points[3], top_rail_sliders, top_rail_dropdown);
			UpdateLabelColor(attachment_points[4], barrel_sliders, barrel_dropdown);
		}

		public void UpdateAttachmentLabels() {
			if (barrel_dropdown != null) {
				barrel_dropdown.options.Clear();
				barrel_dropdown.options.Add(new TMP_Dropdown.OptionData("Empty"));
				barrel_dropdown.options.AddRange(
					from attachment in MainPlugin.attachments.Values
					where attachment.accepted_placement == Attachments.ModularAttachmentPoint.Type.Barrel
					select new TMP_Dropdown.OptionData(attachment.name)
				);
				for (int i = 0; i < barrel_dropdown.options.Count; i++) {
					if (barrel_dropdown.options[i].text == GunConfig.GetAttachmentOnPoint(attachment_points[4])) {
						barrel_dropdown.value = i;
						break;
					}
				}
			}
			if (bottom_rail_dropdown != null) {
				bottom_rail_dropdown.options.Clear();
				bottom_rail_dropdown.options.Add(new TMP_Dropdown.OptionData("Empty"));
				bottom_rail_dropdown.options.AddRange(
					from attachment in MainPlugin.attachments.Values
					where (attachment.accepted_placement & Attachments.ModularAttachmentPoint.Type.Bottom) > 0
					select new TMP_Dropdown.OptionData(attachment.name)
				);
				for (int i = 0; i < bottom_rail_dropdown.options.Count; i++) {
					if (bottom_rail_dropdown.options[i].text == GunConfig.GetAttachmentOnPoint(attachment_points[0])) {
						bottom_rail_dropdown.value = i;
						break;
					}
				}
			}
			if (right_rail_dropdown != null) {
				right_rail_dropdown.options.Clear();
				right_rail_dropdown.options.Add(new TMP_Dropdown.OptionData("Empty"));
				right_rail_dropdown.options.AddRange(
					from attachment in MainPlugin.attachments.Values
					where (attachment.accepted_placement & Attachments.ModularAttachmentPoint.Type.Side) > 0
					select new TMP_Dropdown.OptionData(attachment.name)
				);
				for (int i = 0; i < right_rail_dropdown.options.Count; i++) {
					if (right_rail_dropdown.options[i].text == GunConfig.GetAttachmentOnPoint(attachment_points[2])) {
						right_rail_dropdown.value = i;
						break;
					}
				}
			}
			if (left_rail_dropdown != null) {
				left_rail_dropdown.options.Clear();
				left_rail_dropdown.options.Add(new TMP_Dropdown.OptionData("Empty"));
				left_rail_dropdown.options.AddRange(
					from attachment in MainPlugin.attachments.Values
					where (attachment.accepted_placement & Attachments.ModularAttachmentPoint.Type.Side) > 0
					select new TMP_Dropdown.OptionData(attachment.name)
				);
				for (int i = 0; i < left_rail_dropdown.options.Count; i++) {
					if (left_rail_dropdown.options[i].text == GunConfig.GetAttachmentOnPoint(attachment_points[1])) {
						left_rail_dropdown.value = i;
						break;
					}
				}
			}
			if (top_rail_dropdown != null) { 
				top_rail_dropdown.options.Clear();
				top_rail_dropdown.options.Add(new TMP_Dropdown.OptionData("Empty"));
				top_rail_dropdown.options.AddRange(
					from attachment in MainPlugin.attachments.Values
					where (attachment.accepted_placement & Attachments.ModularAttachmentPoint.Type.Top) > 0
					select new TMP_Dropdown.OptionData(attachment.name)
				);
				for (int i = 0; i < top_rail_dropdown.options.Count; i++) {
					if (top_rail_dropdown.options[i].text == GunConfig.GetAttachmentOnPoint(attachment_points[3])) {
						top_rail_dropdown.value = i;
						break;
					}
				}
			}
		}
	}
}
