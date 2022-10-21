using System.IO;
using System.Collections.Generic;
using UnityEngine;
using BepInEx;

namespace UMP_Plugin {
	[BepInDependency("pl.szikaka.receiver_2_modding_kit")]
	[BepInPlugin("pl.szikaka.UMP", "UMP Plugin", "1.0.3")]
	public class MainPlugin : BaseUnityPlugin {
		public static MainPlugin instance {
			get;
			private set;
		}

		public static readonly string folder_name = "UMP_Files";
		public static Dictionary<string, Attachments.ModularAttachment> attachments = new Dictionary<string, Attachments.ModularAttachment>();

		public static System.Collections.IEnumerator LoadAttachments() {
			List<AssetBundle> bundles = new List<AssetBundle>();

			foreach (FileInfo file in new DirectoryInfo(Path.Combine(Application.persistentDataPath, "Guns", folder_name, "Attachments")).GetFiles()) {
				if (file.Extension == "." + SystemInfo.operatingSystemFamily.ToString().ToLower()) {
					var ab_request = AssetBundle.LoadFromFileAsync(file.FullName);

					while (!ab_request.isDone) yield return null;

					if (ab_request.assetBundle != null) bundles.Add(ab_request.assetBundle);
				}	
			}

			foreach (var assetbundle in AssetBundle.GetAllLoadedAssetBundles()) {
				var assets_request = assetbundle.LoadAllAssetsAsync<Attachments.ModularAttachmentsList>();
				
				while (!assets_request.isDone) yield return null;

				if (assets_request.asset != null) {
					Debug.Log("Loaded attachement");

					foreach (var att in ((Attachments.ModularAttachmentsList) assets_request.asset).attachments) {
						attachments.Add(att.name, att);
					}
				}
			}

			GunConfig.UpdateAttachmentConfig();
		}

		private void Awake() {
			Logger.LogInfo("Plugin for UMP Gun is loaded!");

			instance = this;
		}
	}
}
