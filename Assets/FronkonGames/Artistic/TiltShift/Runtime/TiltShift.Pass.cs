////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Martin Bustos @FronkonGames <fronkongames@gmail.com>. All rights reserved.
//
// THIS FILE CAN NOT BE HOSTED IN PUBLIC REPOSITORIES.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#if UNITY_6000_0_OR_NEWER
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
#endif

namespace FronkonGames.Artistic.TiltShift
{
  ///------------------------------------------------------------------------------------------------------------------
  /// <summary> Render Pass. </summary>
  /// <remarks> Only available for Universal Render Pipeline. </remarks>
  ///------------------------------------------------------------------------------------------------------------------
  public sealed partial class TiltShift
  {
    private sealed class RenderPass : ScriptableRenderPass, IDisposable
    {
      private readonly Settings settings;

#if UNITY_2022_3_OR_NEWER
      private RenderTargetIdentifier colorBuffer;
      private RenderTextureDescriptor renderTextureDescriptor;

      private RTHandle renderTextureHandle0;

      private const string CommandBufferName = Constants.Asset.AssemblyName;

      private readonly ProfilingSampler profilingSamples = new(Constants.Asset.AssemblyName);
      private ProfilingScope profilingScope;
#endif
      private readonly Material material;

      private static class ShaderIDs
      {
        internal static readonly int Intensity = Shader.PropertyToID("_Intensity");

        internal static readonly int Angle = Shader.PropertyToID("_Angle");
        internal static readonly int Aperture = Shader.PropertyToID("_Aperture");
        internal static readonly int Offset = Shader.PropertyToID("_Offset");
        internal static readonly int Blur = Shader.PropertyToID("_Blur");
        internal static readonly int BlurCurve = Shader.PropertyToID("_BlurCurve");
        internal static readonly int Distortion = Shader.PropertyToID("_Distortion");
        internal static readonly int DistortionScale = Shader.PropertyToID("_DistortionScale");
        internal static readonly int FocusedBrightness = Shader.PropertyToID("_FocusedBrightness");
        internal static readonly int FocusedContrast = Shader.PropertyToID("_FocusedContrast");
        internal static readonly int FocusedGamma = Shader.PropertyToID("_FocusedGamma");
        internal static readonly int FocusedHue = Shader.PropertyToID("_FocusedHue");
        internal static readonly int FocusedSaturation = Shader.PropertyToID("_FocusedSaturation");
        internal static readonly int UnfocusedBrightness = Shader.PropertyToID("_UnfocusedBrightness");
        internal static readonly int UnfocusedContrast = Shader.PropertyToID("_UnfocusedContrast");
        internal static readonly int UnfocusedGamma = Shader.PropertyToID("_UnfocusedGamma");
        internal static readonly int UnfocusedHue = Shader.PropertyToID("_UnfocusedHue");
        internal static readonly int UnfocusedSaturation = Shader.PropertyToID("_UnfocusedSaturation");

        internal static readonly int Brightness = Shader.PropertyToID("_Brightness");
        internal static readonly int Contrast = Shader.PropertyToID("_Contrast");
        internal static readonly int Gamma = Shader.PropertyToID("_Gamma");
        internal static readonly int Hue = Shader.PropertyToID("_Hue");
        internal static readonly int Saturation = Shader.PropertyToID("_Saturation");
      }

      private static class Keywords
      {
        internal static readonly string QualityFast = "QUALITY_FAST";
        internal static readonly string QualityNormal = "QUALITY_NORMAL";

        internal static readonly string DebugView = "DEBUG_VIEW";
      }

      /// <summary> Render pass constructor. </summary>
      public RenderPass(Settings settings)
      {
        this.settings = settings;

        string shaderPath = $"Shaders/{Constants.Asset.ShaderName}_URP";
        Shader shader = Resources.Load<Shader>(shaderPath);
        if (shader != null)
        {
          if (shader.isSupported == true)
          if (shader.isSupported == true)
          {
            material = new Material(shader);
#if UNITY_6000_0_OR_NEWER
            this.requiresIntermediateTexture = true;
#endif
          }
          else
            Log.Warning($"'{shaderPath}.shader' not supported");
        }
      }

      private void UpdateMaterial()
      {
        material.shaderKeywords = null;

        switch (settings.quality)
        {
          case Quality.Fast: material.EnableKeyword(Keywords.QualityFast); break;
          case Quality.Normal: material.EnableKeyword(Keywords.QualityNormal); break;
          case Quality.High: break;
        }

        material.SetFloat(ShaderIDs.Intensity, settings.intensity);

#if UNITY_EDITOR
        if (settings.debugView == true)
          material.EnableKeyword(Keywords.DebugView);
#endif
        material.SetFloat(ShaderIDs.Angle, Mathf.Deg2Rad * settings.angle);
        material.SetFloat(ShaderIDs.Aperture, settings.aperture);
        material.SetFloat(ShaderIDs.Offset, settings.offset);

        material.SetFloat(ShaderIDs.BlurCurve, settings.blurCurve);
        material.SetFloat(ShaderIDs.Blur, settings.blur * (float)settings.quality);
        material.SetFloat(ShaderIDs.Distortion, settings.distortion);
        material.SetFloat(ShaderIDs.DistortionScale, settings.distortionScale);

        material.SetFloat(ShaderIDs.FocusedBrightness, settings.focusedBrightness);
        material.SetFloat(ShaderIDs.FocusedContrast, settings.focusedContrast);
        material.SetFloat(ShaderIDs.FocusedGamma, 1.0f / settings.focusedGamma);
        material.SetFloat(ShaderIDs.FocusedHue, settings.focusedHue);
        material.SetFloat(ShaderIDs.FocusedSaturation, settings.focusedSaturation);

        material.SetFloat(ShaderIDs.UnfocusedBrightness, settings.unfocusedBrightness);
        material.SetFloat(ShaderIDs.UnfocusedContrast, settings.unfocusedContrast);
        material.SetFloat(ShaderIDs.UnfocusedGamma, 1.0f / settings.unfocusedGamma);
        material.SetFloat(ShaderIDs.UnfocusedHue, settings.unfocusedHue);
        material.SetFloat(ShaderIDs.UnfocusedSaturation, settings.unfocusedSaturation);

        material.SetFloat(ShaderIDs.Brightness, settings.brightness);
        material.SetFloat(ShaderIDs.Contrast, settings.contrast);
        material.SetFloat(ShaderIDs.Gamma, 1.0f / settings.gamma);
        material.SetFloat(ShaderIDs.Hue, settings.hue);
        material.SetFloat(ShaderIDs.Saturation, settings.saturation);
      }

#if UNITY_6000_0_OR_NEWER
      /// <inheritdoc/>
      public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
      {
        if (material == null || settings.intensity == 0.0f)
          return;

        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
        if (resourceData.isActiveTargetBackBuffer == false)
        {
          TextureHandle source = resourceData.activeColorTexture;
          TextureDesc destinationDesc = renderGraph.GetTextureDesc(source);
          destinationDesc.name = $"CameraColor-{Constants.Asset.AssemblyName}";
          destinationDesc.colorFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_SRGB;
          destinationDesc.clearBuffer = false;

          TextureHandle destination = renderGraph.CreateTexture(destinationDesc);

          UpdateMaterial();

          if (source.IsValid() == true && destination.IsValid() == true)
          {
            RenderGraphUtils.BlitMaterialParameters parametersPass0 = new(source, destination, material, 0);
            renderGraph.AddBlitPass(parametersPass0, $"{Constants.Asset.AssemblyName} Pass 0");

            RenderGraphUtils.BlitMaterialParameters parametersPass1 = new(destination, source, material, 1);
            renderGraph.AddBlitPass(parametersPass1, $"{Constants.Asset.AssemblyName} Pass 1");
          }
        }
      }
#elif UNITY_2022_3_OR_NEWER
      /// <inheritdoc/>
      public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
      {
        renderTextureDescriptor = renderingData.cameraData.cameraTargetDescriptor;
        renderTextureDescriptor.depthBufferBits = 0;

        colorBuffer = renderingData.cameraData.renderer.cameraColorTargetHandle;

        renderTextureDescriptor.colorFormat = RenderTextureFormat.ARGB32;
        RenderingUtils.ReAllocateIfNeeded(ref renderTextureHandle0, renderTextureDescriptor, settings.filterMode, TextureWrapMode.Clamp, false, 1, 0, $"_RTHandle0_{Constants.Asset.Name}");
      }

      /// <inheritdoc/>
      public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
      {
        if (material == null ||
            renderingData.postProcessingEnabled == false ||
            settings.intensity == 0.0f ||
            settings.affectSceneView == false && renderingData.cameraData.isSceneViewCamera == true)
          return;

        CommandBuffer cmd = CommandBufferPool.Get(CommandBufferName);

        if (settings.enableProfiling == true)
          profilingScope = new ProfilingScope(cmd, profilingSamples);

        UpdateMaterial();

        cmd.Blit(colorBuffer, renderTextureHandle0, material, 0);
        cmd.Blit(renderTextureHandle0, colorBuffer, material, 1);

        if (settings.enableProfiling == true)
          profilingScope.Dispose();

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
      }
#else
      #warning("Unity version not supported.");
#endif
      public void Dispose()
      {
#if UNITY_EDITOR
        if (UnityEditor.EditorApplication.isPlaying)
          UnityEngine.Object.Destroy(material);
        else
          UnityEngine.Object.DestroyImmediate(material);
#else
        UnityEngine.Object.Destroy(material);
#endif
      }
    }
  }
}
