using System.Collections.Generic;
using BepInEx.Configuration;
using UnityEngine;
using System.Linq;

namespace UMP_Plugin {
	class GunConfig {
		public static string GetAttachmentOnPoint(Attachments.ModularAttachmentPoint point) {
			return m_attachments_points[point.name].Value;
		}

		public static void SetAttachmentOnPoint(Attachments.ModularAttachmentPoint point, string name) {
			if (name == "Empty") {
				m_attachments_points[point.name].Value = "Empty";
				return;
			}

			Attachments.ModularAttachment attachment = MainPlugin.attachments[name];

			if ((attachment.accepted_placement & point.attachment_point_type) == 0) {
				Debug.LogError("Invalid attachment point for attachment " + name);
			}

			m_attachments_points[point.name].Value = name;
		}

		public static Color GetAttachmentColorOnPoint(Attachments.ModularAttachmentPoint point) {
			return m_attachment_colors[point.name].Value;
		}

		public static void SetAttachmentColorOnPoint(Attachments.ModularAttachmentPoint point, Color color) {
			m_attachment_colors[point.name].Value = color;
		}

		static Dictionary<string, ConfigEntry<string>> m_attachments_points = new Dictionary<string, ConfigEntry<string>>();
		static Dictionary<string, ConfigEntry<Color>> m_attachment_colors = new Dictionary<string, ConfigEntry<Color>>();
		static List<UMPScript> m_guns = new List<UMPScript>();

		public static void CreateConfiguration() {
			MainPlugin.instance.Config.SettingChanged += AttachmentChange;
		}

		public static void UpdateAttachmentConfig() {
			foreach (var point in UMPScript.attachment_point_infos) {
				string[] attachment_names = new List<string> (
					from attachment in MainPlugin.attachments.Values
					where (attachment.accepted_placement & point.type) > 0
					select attachment.name
				) {"Empty"}.ToArray();

				m_attachments_points.Add(point.name,
					MainPlugin.instance.Config.Bind(
						new ConfigDefinition("Attachment settings", point.name),
						"",
						new ConfigDescription(
							"Empty", 
							new AcceptableValueList<string>(attachment_names)
						)
					)
				);

				m_attachment_colors.Add(point.name,
					MainPlugin.instance.Config.Bind(
						new ConfigDefinition("Attachment settings", point.name + " Color"),
						new Color(0, 0, 0, 0),
						new ConfigDescription(
							""
						)
					)
				);
			}
		}

		public static void AddAttachmentEvent(UMPScript gun) {
			m_guns.Add(gun);
		}

		private static void AttachmentChange(object sender, SettingChangedEventArgs event_args) {
			if (event_args.ChangedSetting.Definition.Section == "Attachment settings") {
				m_guns.RemoveAll(g => g == null);

				foreach (var gun in m_guns) {
					gun.UpdateAttachments();
				}

				if (Description.UMPDescriptionScript.instance != null && Description.UMPDescriptionScript.instance.isActiveAndEnabled) Description.UMPDescriptionScript.instance.UpdateAttachments();
			}
		}
	}
}