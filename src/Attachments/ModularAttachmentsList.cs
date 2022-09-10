using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UMP_Plugin.Attachments {
	[CreateAssetMenu(fileName = "UMP Attachments list", menuName = "Receiver 2 Modding/UMP/UMP Attachments list")]
	public class ModularAttachmentsList : ScriptableObject {
		public List<ModularAttachment> attachments = new List<ModularAttachment>();
	}
}
