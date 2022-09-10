using UnityEngine;
using Receiver2;

namespace UMP_Plugin.Attachments {
	public class LaserAttachment : ModularAttachment, IColorableAttachment {
		public Transform laser_origin;

		private bool toggle = false;
		public GameObject light_beam;
		
		public override void EnableAttachment() {
			light_beam.SetActive(false);
		}

		public override void DisableAttachment() {

		}
		
		void Update() {
			if (attachment_point.gun_script.player_input.GetButtonDown(RewiredConsts.Action.Hammer)) {
				AudioManager.PlayOneShotAttached("event:/flashlight/switch_on", gameObject);
				attachment_point.gun_script.recoil_transfer_x += 5;
				attachment_point.gun_script.recoil_transfer_y += 5;
				toggle = !toggle;
			}

			if (toggle && Physics.Raycast(laser_origin.position, laser_origin.forward, out RaycastHit hit, 100, -1 ^ 0x40000000, QueryTriggerInteraction.Ignore)) {
				light_beam.SetActive(true);
				light_beam.transform.position = hit.point - (laser_origin.forward * 0.1f);
				light_beam.transform.rotation = laser_origin.rotation;
			}
			else {
				light_beam.SetActive(false);
			}
		}

		public Color GetColor() {
			return light_beam.GetComponent<Light>().color;
		}

		public void SetColor(Color color) {
			light_beam.GetComponent<Light>().color = color;
		}
	}
}
