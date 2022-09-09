using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Sandbox;

namespace FearfulCry.player;

partial class FearfulCryPlayer {
	protected virtual Vector3 LightOffset => Vector3.Forward * 20;
	private SpotLightEntity FirstPersonFlashlight;
	private SpotLightEntity WorldFlashlight;

	[Net, Predicted]
	private bool IsFlashlightOn { get; set; }

    /// <summary>
	/// CreateLight creates a SpotLightEntity that is used from the server
	/// and the client's view.
	/// </summary>
    private SpotLightEntity CreateLight()
	{
		var light = new SpotLightEntity{
			Enabled = false,
			DynamicShadows = true,
			Range = 1024,
			Falloff = 1.0f,
			LinearAttenuation = 0.0f,
			QuadraticAttenuation = 1.0f,
			Brightness = 2,
			Color = Color.White,
			InnerConeAngle = 20,
			OuterConeAngle = 40,
			FogStrength = 1.0f,
			Owner = this,
			LightCookie = Texture.Load("materials/effects/lightcookie.vtex"),
		};

		light.UseFogNoShadows();
		light.EnableLagCompensation = true;
		light.EnableViewmodelRendering = true;

		return light;
	}

    /// <summary>
	/// Check for any flashlight input and if so then toggle the flashlight.
	/// 
	/// If the flashlight is on then update the position/rotation of the light.
	/// </summary>
    private void FlashlightTick()
    {
		// If the flashlight button is pressed then toggle the flashlight.
		if ( Input.Pressed( InputButton.Flashlight ) )
		{
			IsFlashlightOn = !IsFlashlightOn;
			PlaySound( IsFlashlightOn ? "flashlight-on" : "flashlight-off" );

			if (IsServer) {
				if (!WorldFlashlight.IsValid()) {
					WorldFlashlight = CreateLight();
					WorldFlashlight.Transform = Transform;
					WorldFlashlight.EnableHideInFirstPerson = true;
				}
				WorldFlashlight.Enabled = IsFlashlightOn;
			}
			if (IsClient) {
				if (!FirstPersonFlashlight.IsValid()) {
					FirstPersonFlashlight = CreateLight();
					FirstPersonFlashlight.Transform = Transform;
					FirstPersonFlashlight.EnableViewmodelRendering = true;
				}
				FirstPersonFlashlight.Enabled = IsFlashlightOn;
			}
		}

		// Update the position and rotation of the flashlight.
        if (IsFlashlightOn) {
			UpdateLight(
                IsClient ? FirstPersonFlashlight : WorldFlashlight
            );
		}
    }

    private void UpdateLight(SpotLightEntity light) {
        light.SetParent( null );
        light.Rotation = EyeRotation;
        light.Position = EyePosition + EyeRotation.Forward * 20f;
        light.SetParent( this );
    }
}