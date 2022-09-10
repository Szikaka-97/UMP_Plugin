using UnityEngine;
using Receiver2;

namespace UMP_Plugin.Attachments {
	class FlashlightAttachment : ModularAttachment {
		public Light light;
		private bool toggle = false;

		public override void EnableAttachment() {
			
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

			light.gameObject.SetActive(toggle);
		}
	}
}
