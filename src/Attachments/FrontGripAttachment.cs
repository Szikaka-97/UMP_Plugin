namespace UMP_Plugin.Attachments {
	public class FrontGripAttachment : ModularAttachment {
		public float recoil_multiplier = 1;
		public float sway_multiplier = 1;

		public override void DisableAttachment() {
			attachment_point.gun_script.recoil_transfer_x_min *= recoil_multiplier;
			attachment_point.gun_script.recoil_transfer_x_max *= recoil_multiplier;
			attachment_point.gun_script.recoil_transfer_y_min *= recoil_multiplier;
			attachment_point.gun_script.recoil_transfer_y_min *= recoil_multiplier;
		}

		public override void EnableAttachment() {
			attachment_point.gun_script.recoil_transfer_x_min /= recoil_multiplier;
			attachment_point.gun_script.recoil_transfer_x_max /= recoil_multiplier;
			attachment_point.gun_script.recoil_transfer_y_min /= recoil_multiplier;
			attachment_point.gun_script.recoil_transfer_y_min /= recoil_multiplier;
		}
	}
}
