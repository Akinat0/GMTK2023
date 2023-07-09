using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace URP.BlitPass
{
	public enum BufferType
	{
		CameraColor,
		Custom 
	}
	
	internal class ColorBlitRendererFeature : ScriptableRendererFeature
	{
		[SerializeField]
		private SettingsData _settings = new SettingsData();
		private ColorBlitPass _blitPass;

		public override void Create()
		{
			_blitPass = new ColorBlitPass(name);
		}

		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
		{
			if (_settings.blitMaterial == null)
			{
				Debug.LogWarningFormat("Missing Blit Material. {0} blit pass will not execute. Check for missing reference in the assigned renderer.", GetType().Name);
				return;
			}

			_blitPass.renderPassEvent = _settings.renderPassEvent;
			_blitPass.SettingsData = _settings;
			renderer.EnqueuePass(_blitPass);
		}
		
		[System.Serializable]
		public class SettingsData
		{
			public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;

			public Material blitMaterial = null;
			public int blitMaterialPassIndex = -1;
			public BufferType sourceType = BufferType.CameraColor;
			public BufferType destinationType = BufferType.CameraColor;
			public string sourceTextureId = "_SourceTexture";
			public string destinationTextureId = "_DestinationTexture";
		}
	}
}