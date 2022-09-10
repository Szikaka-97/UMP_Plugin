using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using Receiver2;
using RewiredConsts;
using Receiver2ModdingKit;
using R2CustomSounds;
using ImGuiNET;

namespace UMP_Plugin {
	public class UMPScript : ModGunScript {
		private float slide_forward_speed = -8;
		private float hammer_accel = -5000;

		private readonly float[] slide_push_hammer_curve = new float[] {
			0,
			0,
			0.2f,
			1
		};

		private float m_charging_handle_amount; // GunScript already has charging_handle_amount wtf?
		private float magazine_slide_lock_amount = 0.625f;
		private bool slide_locked_by_magazine;
		private RotateMover magazine_slide_catch = new RotateMover();
		private ModHelpEntry help_entry;

		[HideInInspector]
		[System.NonSerialized]
		public bool is_supressed;

		public bool attachment_debug;
		public Sprite help_entry_sprite;
		public static Attachments.ModularAttachmentPoint.AttachmentPointInfo[] attachment_point_infos;
		private List<Attachments.ModularAttachmentPoint> attachment_points;

		new private void FireBullet(ShellCasingScript round) {
			this.chamber_check_performed = false;
			if (!this.slot && this.rigid_body != null)
			{
				this.rigid_body.AddForceAtPosition(base.transform.forward * -100f, this.transform_muzzle_flash.position);
			}
			CartridgeSpec cartridge_spec = default(CartridgeSpec);
			cartridge_spec.SetFromPreset(round.cartridge_type);
			LocalAimHandler holdingPlayer = this.GetHoldingPlayer();
			if (holdingPlayer != null)
			{
				bool flinching = ConfigFiles.profile.enable_flinching;

				if(is_supressed) ConfigFiles.profile.enable_flinching = false;

				holdingPlayer.FiredGun(cartridge_spec);

				ConfigFiles.profile.enable_flinching = flinching;
			}
			if(!is_supressed) AudioManager.PlayOneShot3D(AudioManager.Instance().impulse_response, this.transform_muzzle_flash.position, 1f, 1f); // Copying the entire function to add one branch, yay!

			AudioManager.PlayOneShotAttached(this.sound_event_gunshot, this.transform_muzzle_flash.gameObject);
			AudioManager.PlayOneShotAttached(AudioManager.Instance().sound_event_firing_mechanics, this.transform_muzzle_flash.gameObject);

			AudioManager.Instance().LoudSound(this.transform_muzzle_flash.position, base.transform.forward);
			if (this.pooled_muzzle_flash.object_prefab != null)
			{
				GameObject gameObject = ObjectPool.Instantiate(this.pooled_muzzle_flash, this.transform_muzzle_flash.position, this.transform_muzzle_flash.rotation);
				if (this.flash_use_gun_position)
				{
					gameObject.transform.position = transform.position;
					gameObject.transform.rotation = transform.rotation;
					gameObject.transform.parent = transform;
				}
			}
			Vector3 direction = transform_bullet_fire.rotation * Vector3.forward;
			BulletTrajectory bulletTrajectory = BulletTrajectoryManager.PlanTrajectory(this.transform_bullet_fire.position, cartridge_spec, direction, this.right_hand_twist);
			if (ConfigFiles.global.display_trajectory_window && ConfigFiles.global.display_trajectory_window_show_debug)
			{
				bulletTrajectory.draw_path = BulletTrajectory.DrawType.Debug;
			}
			else if ((round.tracer || GunScript.force_tracers) && !is_supressed)
			{
				bulletTrajectory.draw_path = BulletTrajectory.DrawType.Tracer;
				bulletTrajectory.tracer_fuse = true;
			}
			bulletTrajectory.bullet_source = base.gameObject;
			bulletTrajectory.bullet_source_entity_type = holdingPlayer != null ? ReceiverEntityType.Player : ReceiverEntityType.UnheldGun;
			BulletTrajectoryManager.ExecuteTrajectory(bulletTrajectory);
			if (round != null) round.MakeSpent();
			Wolfire.ScreenEffect.Shake(3f, 0f, 0.15f);
			this.rotation_transfer_y += Random.Range(this.rotation_transfer_y_min, this.rotation_transfer_y_max);
			this.rotation_transfer_x += Random.Range(this.rotation_transfer_x_min, this.rotation_transfer_x_max);
			this.recoil_transfer_x -= Random.Range(this.recoil_transfer_x_min, this.recoil_transfer_x_max);
			this.recoil_transfer_y += Random.Range(this.recoil_transfer_y_min, this.recoil_transfer_y_max);
			this.add_head_recoil = true;
			if (this.CanMalfunction && this.malfunction == GunScript.Malfunction.None && (Random.Range(0f, 1f) < this.doubleFeedProbability || this.force_double_feed_failure))
			{
				if (this.force_double_feed_failure && this.force_just_one_failure)
				{
					this.force_double_feed_failure = false;
				}
				this.malfunction = GunScript.Malfunction.DoubleFeed;
				ReceiverEvents.TriggerEvent(ReceiverEventTypeInt.GunMalfunctioned, 2);
			}
			this.slide.vel += this.slideFireSpeed;
			ReceiverEvents.TriggerEvent(ReceiverEventTypeVoid.PlayerShotFired);
			this.last_time_fired = Time.time;
			this.last_frame_fired = Time.frameCount;
			this.dry_fired = false;
			_disconnector_needs_reset = true;
			if (this.shots_until_dirty > 0)
			{
				this.shots_until_dirty--;
			}
		}

		public void UpdateAttachments() {
			foreach (var point in attachment_points) {
				if (point.attachment != null && point.attachment.name != GunConfig.GetAttachmentOnPoint(point)) {
					point.attachment.DisableAttachment();
					DestroyImmediate(point.attachment.gameObject);
					point.attachment = null;
				}

				if (point.attachment == null && GunConfig.GetAttachmentOnPoint(point) != "Empty") {
					point.attachment = Instantiate(MainPlugin.attachments[GunConfig.GetAttachmentOnPoint(point)].gameObject).GetComponent<Attachments.ModularAttachment>();
					point.attachment.attachment_point = point;

					point.attachment.transform.parent = point.transform;
					
					point.attachment.transform.localScale = Vector3.one;
					point.attachment.transform.position = point.mount_point.position;
					point.attachment.transform.rotation = point.mount_point.rotation;

					point.attachment.EnableAttachment();

					if (point.attachment is Attachments.IColorableAttachment) {
						Attachments.IColorableAttachment colorable = point.attachment as Attachments.IColorableAttachment;

						if (((Vector4) GunConfig.GetAttachmentColorOnPoint(point)).sqrMagnitude == 0) GunConfig.SetAttachmentColorOnPoint(point, colorable.GetColor());
						else colorable.SetColor(GunConfig.GetAttachmentColorOnPoint(point));
					}
				}
			}
		}

		public override ModHelpEntry GetGunHelpEntry() {
			return help_entry = new ModHelpEntry("HK UMP") {
				info_sprite = this.help_entry_sprite,
				title = "H&K UMP",
				description = "Heckler & Koch UMP45\n"
							+ "Capacity: 25 + 1, .45 ACP (Automatic Colt Pistol)\n"
							+ "\n"
							+ "After making the iconic MP5 submachinegun, the Heckler & Koch company was put in a very precarious situation. While a tremendous success, the MP5 had its flaws, and they became only more apparent as years went on. Its sheet metal receiver was heavy, the complicated roller delayed blowback bolt was expensive to manufacture, and it was never designed with modularity in mind which started to become a large issue into the '80s.\n"
							+ "\n"
							+ "Following the trials for their G36 assault rifle, H&K decided to remake it's biggest submachinegun success, replacing a complicated mechanism with a simple blowback operation, encased in high-strength polymer and with mounting points for a new idea in firearms: a system of universal rails for adding tactical attachments. Thus, the UMP was born"
				};
		}

		public override LocaleTactics GetGunTactics() {
			return new LocaleTactics() {
				gun_internal_name = InternalName,
				title = "H&K UMP .45\"",
				text = "A modded submachinegun\n" +
					   "A .45 caliber submachinegun beloved by special forces operators around the world for it's simple operation, low mass and a high degree of modularity, allowing it to perform well in a variety of situations"
			};
		}

		public override void InitializeGun() {
			pooled_muzzle_flash = ((GunScript) ReceiverCoreScript.Instance().generic_prefabs.First(it => { return it is GunScript && ((GunScript) it).gun_model == GunModel.m1911; })).pooled_muzzle_flash;
			
			attachment_point_infos = (
				from point in GetComponentsInChildren<Attachments.ModularAttachmentPoint>(true) 
				select new Attachments.ModularAttachmentPoint.AttachmentPointInfo() { 
					name = point.name, 
					type = point.attachment_point_type
				}
			).ToArray();

			MainPlugin.instance.StartCoroutine(MainPlugin.LoadAttachments());
			MainPlugin.instance.StartCoroutine(Description.UMPDescriptionScript.LoadDescription());

			help_entry.AddSettingsButtonAction(delegate{
				Description.UMPDescriptionScript.Open();
			});

			GunConfig.CreateConfiguration();
		}

		public override void AwakeGun() {
			attachment_points = GetComponentsInChildren<Attachments.ModularAttachmentPoint>(true).ToList();

			hammer.amount = 1;

			magazine_slide_catch.transform = transform.Find("slide_catch");
			ApplyTransform("slide_catch", 0, magazine_slide_catch.transform);
			magazine_slide_catch.rotations[0] = magazine_slide_catch.transform.localRotation;
			ApplyTransform("slide_catch", 1, magazine_slide_catch.transform);
			magazine_slide_catch.rotations[1] = magazine_slide_catch.transform.localRotation;

			GunConfig.AddAttachmentEvent(this);

			UpdateAttachments();
		}

		public override void UpdateGun() {
			firing_modes[0].sound_event_path = sound_safety_on;
			firing_modes[1].sound_event_path = sound_safety_on;
			firing_modes[2].sound_event_path = sound_safety_on;

			// <hammer logic>
			hammer.asleep = true;
			hammer.accel = hammer_accel;

			if (slide.amount > 0) { // Bolt cocks the hammer when moving back 
				hammer.amount = Mathf.Max(hammer.amount, InterpCurve(slide_push_hammer_curve, slide.amount));
			}

			if (hammer.amount == 1) _hammer_state = 3; // Mark the hammer as dropping when cocked by the bolt

			if (trigger.amount == 0) _disconnector_needs_reset = false; // Release the disconnector when the trigger is released

			if (IsSafetyOn()) { // Safety blocks the trigger from moving
				trigger.amount = Mathf.Min(trigger.amount, 0.1f);

				trigger.UpdateDisplay();
			}

			if (_hammer_state != 3 && ((trigger.amount == 1 && !_disconnector_needs_reset && slide.amount == 0) || hammer.amount != _hammer_cocked_val)) { // Move hammer if it's cocked and is free to move
				hammer.asleep = false;
			}

			if (slide.amount == 0 && _hammer_state == 3) { // Simulate auto sear
				hammer.amount = Mathf.MoveTowards(hammer.amount, _hammer_cocked_val, Time.deltaTime * Time.timeScale * 50);
				if (hammer.amount == _hammer_cocked_val) _hammer_state = 2;
			}

			hammer.TimeStep(Time.deltaTime);

			if (hammer.amount == 0 && _hammer_state == 2) { // If hammer dropped and hammer was cocked then fire gun and decock hammer
				TryFireBullet(1, FireBullet);

				_disconnector_needs_reset = GetCurrentFireMode() == FireMode.SEMI;

				_hammer_state = 0;
			}
			// </hammer logic>

			if (slide.vel < 0) slide.vel = Mathf.Max(slide.vel, slide_forward_speed); // Slow down the slide moving forward, reducing fire rate

			if (player_input.GetButton(Action.Pull_Back_Slide) || player_input.GetButtonUp(Action.Pull_Back_Slide)) {
				slide.target_amount = Mathf.Min(slide.target_amount, 0.7f);
				m_charging_handle_amount = slide.amount;
			}
			else {
				m_charging_handle_amount = Mathf.Min(m_charging_handle_amount, slide.amount);
			}

			ApplyTransform("charging_handle", m_charging_handle_amount, slide_stop.transform);

			//ApplyTransform("magazine_catch_animation", magazine_catch.amount, magazine_catch.transform);
			//ApplyTransform(select_fire_animation_path, _select_fire.amount, select_fire_component);

			hammer.UpdateDisplay();

			// <handling the slide stop when magazine is empty>
			if (magazine_instance_in_gun != null && magazine_instance_in_gun.NumRounds() == 0) {
				if (slide.amount >= magazine_slide_lock_amount) magazine_slide_catch.amount = 1;
				else magazine_slide_catch.amount = 0;
			}
			else if (slide.amount > magazine_slide_lock_amount) magazine_slide_catch.amount = 0;
			if (player_input.GetButtonDown(Action.Slide_Lock)) magazine_slide_catch.amount = 0;

			if (magazine_slide_catch.amount == 1) {
				slide.target_amount = Mathf.Max(slide.target_amount, magazine_slide_lock_amount);
			}

			magazine_slide_catch.UpdateDisplay();
			// </handling the slide stop when magazine is empty>

			UpdateAnimatedComponents();
		}
	}
}

//Halo sight shader from https://vazgriz.com/158/reflex-sight-shader-in-unity3d/