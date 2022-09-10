using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UMP_Plugin.Attachments {
	public class SightAttachment : ModularAttachment, IColorableAttachment {
		
		public Transform ads_transform;
		public Renderer sight_renderer;
		private Pose original_ads;
		
		public override void EnableAttachment() {
			Transform pose_aim_down_sights = attachment_point.gun_script.transform.Find("pose_aim_down_sights");

			original_ads = new Pose(pose_aim_down_sights.localPosition, pose_aim_down_sights.localRotation);

			pose_aim_down_sights.localPosition = ads_transform.localPosition;
			pose_aim_down_sights.localRotation = ads_transform.localRotation;
		}

		public override void DisableAttachment() {
			Transform pose_aim_down_sights = attachment_point.gun_script.transform.Find("pose_aim_down_sights");

			pose_aim_down_sights.localPosition = original_ads.position;
			pose_aim_down_sights.localRotation = original_ads.rotation;
		}

		public virtual Color GetColor() {
			return sight_renderer.sharedMaterial.color;
		}

		public virtual void SetColor(Color color) {
			sight_renderer.sharedMaterial.color = color;
		}
	}
}
