using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace URP.BlitPass
{
	internal class ColorBlitPass : ScriptableRenderPass
	{
		public FilterMode FilterMode { get; set; }
        public ColorBlitRendererFeature.SettingsData SettingsData;
        
        private RenderTargetIdentifier _source;
        private RenderTargetIdentifier _destination;
        private readonly int _temporaryRTId = Shader.PropertyToID("_TempRT");
        
        private int _sourceId;
        private int _destinationId;
        private bool _isSourceAndDestinationSameTarget;
        
        private readonly string _profilerTag;
        private readonly int _intensityPropertyID = Shader.PropertyToID("_Intensity");
        
        public ColorBlitPass(string tag)
        {
            _profilerTag = tag;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
	        RenderTextureDescriptor blitTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
	        blitTargetDescriptor.depthBufferBits = 0;

	        _isSourceAndDestinationSameTarget = SettingsData.sourceType == SettingsData.destinationType &&
	                                            (SettingsData.sourceType == BufferType.CameraColor || SettingsData.sourceTextureId == SettingsData.destinationTextureId);

	        var renderer = renderingData.cameraData.renderer;

	        if(SettingsData.sourceType == BufferType.CameraColor)
	        {
		        _sourceId = -1;
		        _source = renderer.cameraColorTargetHandle;
	        }
	        else
	        {
		        _sourceId = Shader.PropertyToID(SettingsData.sourceTextureId);
		        cmd.GetTemporaryRT(_sourceId, blitTargetDescriptor, FilterMode);
		        _source = new RenderTargetIdentifier(_sourceId);
	        }

	        if(_isSourceAndDestinationSameTarget)
	        {
		        _destinationId = _temporaryRTId;
		        cmd.GetTemporaryRT(_destinationId, blitTargetDescriptor, FilterMode);
		        _destination = new RenderTargetIdentifier(_destinationId);
	        }
	        else if(SettingsData.destinationType == BufferType.CameraColor)
	        {
		        _destinationId = -1;
		        _destination = renderer.cameraColorTargetHandle;
	        }
	        else
	        {
		        _destinationId = Shader.PropertyToID(SettingsData.destinationTextureId);
		        cmd.GetTemporaryRT(_destinationId, blitTargetDescriptor, FilterMode);
		        _destination = new RenderTargetIdentifier(_destinationId);
	        }
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get(_profilerTag);

            if (_isSourceAndDestinationSameTarget)
            {
	            Blit(cmd, _source, _destination, SettingsData.blitMaterial, SettingsData.blitMaterialPassIndex);
                Blit(cmd, _destination, _source);
            }
            else
            {
                Blit(cmd, _source, _destination, SettingsData.blitMaterial, SettingsData.blitMaterialPassIndex);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            if (_destinationId != -1)
                cmd.ReleaseTemporaryRT(_destinationId);

            if (_source == _destination && _sourceId != -1)
                cmd.ReleaseTemporaryRT(_sourceId);
        }
	}
}