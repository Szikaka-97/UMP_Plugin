using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using Receiver2;

namespace UMP_Plugin.Attachments {
	public class GizmoAttachment : SightAttachment, IColorableAttachment {
		public LaserAttachment laser;

		public TextMeshPro ammo_count;
		public TextMeshPro ammo_max;
		public SpriteRenderer bullet_indicator;
		public SpriteRenderer magazine_indicator;
		public SpriteRenderer malfunction_indicator;
		public SpriteRenderer danger_indicator;

		public override void EnableAttachment() {
			base.EnableAttachment();

			laser.gameObject.SetActive(true);
			laser.attachment_point = this.attachment_point;
			laser.EnableAttachment();
		}

		public override void DisableAttachment() {
			base.DisableAttachment();

			laser.DisableAttachment();
		}

		public void Update() {
			GunScript gun = attachment_point.gun_script;

			if (gun.magazine_instance_in_gun != null) {
				ammo_count.text = gun.magazine_instance_in_gun.NumRounds().ToString();
				ammo_max.text = gun.magazine_instance_in_gun.kMaxRounds.ToString();
				magazine_indicator.color = new Color(0.2f, 0.5f, 1);
			}
			else {
				ammo_count.text = "0";
				ammo_max.text = "0";
				magazine_indicator.color = Color.black;
			}

			if (gun.round_in_chamber != null) bullet_indicator.color = Color.green;
			else bullet_indicator.color = Color.red;

			if (gun.malfunction == GunScript.Malfunction.None) malfunction_indicator.color = new Color(1, 0.6f, 0, 0);
			else malfunction_indicator.color = new Color(1, 0.6f, 0, 1);

			danger_indicator.color = new Color(1, 0, 0, AudioManager.Instance().GetComponent<MusicScript>().Danger * ((Mathf.Sin(Time.time * 3) / 2f) + 1.5f));
		}

		public override void SetColor(Color color) {
			laser.SetColor(color);
		}

		public override Color GetColor() {
			return laser.GetColor();
		}
	}
}
