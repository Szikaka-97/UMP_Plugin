using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Receiver2;


namespace UMP_Plugin.Attachments {
	public class BarrelAttachment : ModularAttachment {
		public Transform point_bullet_fire;
		public Transform point_muzzleflash;
		public PooledGameObject muzzle_flash;
		public bool is_supressor;

		private Pose point_bullet_fire_original;
		private Pose point_muzzleflash_original;
		private PooledGameObject muzzle_flash_original;

		public override void EnableAttachment() {
			Transform bullet_fire_original = attachment_point.gun_script.transform.Find("point_bullet_fire");
			Transform muzzleflash_original = attachment_point.gun_script.transform.Find("point_muzzleflash");

			point_bullet_fire_original = new Pose(bullet_fire_original.localPosition, bullet_fire_original.localRotation);
			point_muzzleflash_original = new Pose(muzzleflash_original.localPosition, muzzleflash_original.localRotation);

			((UMPScript) attachment_point.gun_script).is_supressed = is_supressor;
			if (muzzle_flash.object_prefab != null) {
				muzzle_flash_original = attachment_point.gun_script.pooled_muzzle_flash;
				attachment_point.gun_script.pooled_muzzle_flash = muzzle_flash;
			}

			bullet_fire_original.SetPositionAndRotation(point_bullet_fire.position, point_bullet_fire.rotation);
			muzzleflash_original.SetPositionAndRotation(point_muzzleflash.position, point_muzzleflash.rotation);
		}

		public override void DisableAttachment() {
			Transform bullet_fire_original = attachment_point.gun_script.transform.Find("point_bullet_fire");
			Transform muzzleflash_original = attachment_point.gun_script.transform.Find("point_muzzleflash");

			((UMPScript) attachment_point.gun_script).is_supressed = false;

			if (muzzle_flash_original != null) {
				attachment_point.gun_script.pooled_muzzle_flash = muzzle_flash_original;
			}

			bullet_fire_original.localPosition = point_bullet_fire_original.position;
			bullet_fire_original.localRotation = point_bullet_fire_original.rotation;
			muzzleflash_original.localPosition = point_muzzleflash_original.position;
			muzzleflash_original.localRotation = point_muzzleflash_original.rotation;
		}
	}
}
