using UnityEngine;

namespace UMP_Plugin.Attachments {
	public interface IColorableAttachment {
		public void SetColor(Color color);
		public Color GetColor();
	}
}
