using System;
using UnityEngine;

namespace UMP_Plugin.Attachments {
	public class ModularAttachmentPoint : MonoBehaviour {
		[Flags]
		public enum Type {
			Barrel = 1,
			Bottom = 2,
			Side = 4,
			Top = 8
		}

		public class AttachmentPointInfo {
			public string name;
			public Type type;
		}

		public Type attachment_point_type;
		public Transform mount_point;

		public UMPScript gun_script;

		[HideInInspector]
		public ModularAttachment attachment;
	}
}
