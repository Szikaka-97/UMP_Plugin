using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UMP_Plugin.Attachments {
	public abstract class ModularAttachment : MonoBehaviour {
		public ModularAttachmentPoint.Type accepted_placement;

		[HideInInspector]
		public ModularAttachmentPoint attachment_point;

		public abstract void EnableAttachment();
		public abstract void DisableAttachment();
	}
}
