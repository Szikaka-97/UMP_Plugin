using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UMP_Plugin.Attachments {
	public interface IColorableAttachment {
		public void SetColor(Color color);
		public Color GetColor();
	}
}
