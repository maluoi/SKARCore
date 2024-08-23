// Largely from Nick Rewkowski's work at Nakamir here:
// https://github.com/Nakamir-Code/SKARCore/blob/develop/Examples/StereoKitTest/StereoKitTest_ARCore/Activities/ARCoreDemoActivity.cs

using System;

#if ANDROID
using Android.Opengl;
using Google.AR.Core;
using Google.AR.Core.Exceptions;
#endif

namespace StereoKit.Framework
{
	public class ARCore : IStepper
	{
		public bool Enabled => true;

		#if ANDROID
		Session arCoreSession;
		int     arCoreTexId;

		public bool Initialize()
		{
			int[] textures = new int[1];
			GLES20.GlGenTextures(1, textures, 0);
			arCoreTexId = textures[0];
			GLES20.GlBindTexture  (GLES11Ext.GlTextureExternalOes, arCoreTexId);
			GLES20.GlTexParameteri(GLES11Ext.GlTextureExternalOes, GLES20.GlTextureWrapS,     GLES20.GlClampToEdge);
			GLES20.GlTexParameteri(GLES11Ext.GlTextureExternalOes, GLES20.GlTextureWrapT,     GLES20.GlClampToEdge);
			GLES20.GlTexParameteri(GLES11Ext.GlTextureExternalOes, GLES20.GlTextureMinFilter, GLES20.GlNearest);
			GLES20.GlTexParameteri(GLES11Ext.GlTextureExternalOes, GLES20.GlTextureMagFilter, GLES20.GlNearest);
			Tex arTexture = new Tex();
			arTexture.SetNativeSurface(arCoreTexId, TexType.ImageNomips);

			try { arCoreSession = new Session(Android.App.Application.Context); }
			catch (UnavailableArcoreNotInstalledException) { Log.Warn("Please install ARCore"); return false; }
			catch (UnavailableApkTooOldException         ) { Log.Warn("Please update ARCore (Google Play Services for AR--search on Google). The app requires at least 1.29."); return false; }
			catch (UnavailableSdkTooOldException         ) { Log.Warn("Please update this app"); return false; }
			catch (Java.Lang.Exception e                 ) { Log.Warn("This device does not support AR" + e); return false; };

			Config config = new Config(arCoreSession);
			if (!arCoreSession.IsSupported(config))
				return false;
			arCoreSession.Configure(config);
			arCoreSession.SetCameraTextureName(arCoreTexId);
			arCoreSession.SetDisplayGeometry(0, 480, 640);
			arCoreSession.Resume();

			Frame  arCoreFrame  = arCoreSession.Update();
			Camera arCoreCamera = arCoreFrame.Camera;
			float  focalLength  = arCoreCamera.ImageIntrinsics.GetFocalLength()[0];
			int[]  size         = arCoreCamera.ImageIntrinsics.GetImageDimensions();

			float fovW = (float)(2 * System.Math.Atan(size[0] / (focalLength * 2.0f))) * Units.rad2deg;
			float fovH = (float)(2 * System.Math.Atan(size[1] / (focalLength * 2.0f))) * Units.rad2deg;
			float fovD = MathF.Sqrt(MathF.Pow((float)fovW, 2) + MathF.Pow((float)fovH, 2));
			Renderer.SetFOV(fovW);

			Material arMaterial = new Material("skyUnlitExternal.hlsl");
			arMaterial.DepthWrite  = false;
			arMaterial.DepthTest   = DepthTest.LessOrEq;
			arMaterial.QueueOffset = 100;
			arMaterial[MatParamName.DiffuseTex] = arTexture;
			Renderer.SkyMaterial = arMaterial;

			return true;
		}

		public void Step()
		{
			if (arCoreSession == null) return;

			Frame arCoreFrame;
			try { arCoreFrame = arCoreSession.Update(); }
			catch
			{
				Log.Warn("arCoreSession.Update failed");
				arCoreSession = null;
				return;
			}

			Camera cam = arCoreFrame.Camera;
			// This is critical for some reason
			GLES20.GlBindTexture(GLES11Ext.GlTextureExternalOes, arCoreTexId);
			Matrix rotation = Matrix.R(new Quat(cam.Pose.Qy(), -cam.Pose.Qx(), cam.Pose.Qz(), cam.Pose.Qw())) * Matrix.R(0, 0, 90.0f);
			Matrix newPose  = rotation * Matrix.T(cam.Pose.Tx(), cam.Pose.Ty(), cam.Pose.Tz());

			Renderer.CameraRoot  = newPose;

			cam.Dispose();
		}

		public void Shutdown()
		{
			if (arCoreSession != null)
			{
				arCoreSession.Pause();
				arCoreSession.Close();
			}
		}
		#else
		public bool Initialize() => false;
		public void Shutdown  () { }
		public void Step      () { }
		#endif
	}
}
