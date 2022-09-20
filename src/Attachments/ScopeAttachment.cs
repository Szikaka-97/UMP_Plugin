using UnityEngine;
using Receiver2;

namespace UMP_Plugin.Attachments {
	public class ScopeAttachment : SightAttachment {
		public Camera camera;
		[Range(0, 1)]
		public float min_camera_dot = 0;

		public override void EnableAttachment() {
			base.EnableAttachment();
		}

		public override void DisableAttachment() {
			base.DisableAttachment();
		}

		void Update() {
			camera.gameObject.SetActive(Vector3.Dot(camera.transform.forward, Vector3.Normalize(camera.transform.position - LocalAimHandler.player_instance.main_camera.transform.position)) > min_camera_dot);
		}
	}
}
