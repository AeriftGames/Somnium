using Godot;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;
using static all_material_surfaces;

public partial class CCharacterJumpLandEffectComponent : CBaseComponent
{

    [ExportGroupAttribute("Shake Settings")]
    [Export] public float JumpShakeStrenght = 1.0f;
    [Export] public float JumpShakeFade = 9.0f;
    [Export] public float LandShakeStrenght = 1.0f;
    [Export] public float LandShakeFade = 9.0f;
    [ExportGroupAttribute("Audio Settings")]
    [Export] public Godot.Collections.Array<AudioStream> JumpingSounds;
    [Export] public float JumpingVolumeDB = -5f;
    [Export] public float JumpingAudioPitch = 1.0f;
    [Export] public float JumpingAudioPitchOffset = 0.2f;

    AnimationPlayer PlayerAnim;
    AudioStreamPlayer PlayerAudio;

    public all_material_surfaces AllMaterialSurfaces = null;

    private Node3D CameraJump = null;

    //
    private float lastYPosFallingStart = 0.0f;
    private float lastYPosFallingEnd = 0.0f;

    public override void PostInit(FpsCharacterBase newCharacterBase)
    {
        base.PostInit(newCharacterBase);

        // nacteni vsech dat material surfaces
        AllMaterialSurfaces =
            (all_material_surfaces)GD.Load("res://player/material_surface/all_material_surfaces.tres");

        PlayerAnim = GetNode<AnimationPlayer>("AnimationPlayer_JumpLand");
        PlayerAudio = GetNode<AudioStreamPlayer>("AudioStreamPlayer_JumpLand");

        CameraJump = ourCharacterBase.GetCharacterLookComponent().GetCameraJump();

    }

    public void Update(double delta)
    {

    }

    public void ApplyEffectJump()
    {
        PlayerAnim.Play("Jump");

        RandomNumberGenerator a = new RandomNumberGenerator();
        a.Randomize();
        float PitchScale = a.RandfRange(JumpingAudioPitch - (JumpingAudioPitchOffset / 2),
                JumpingAudioPitch + (JumpingAudioPitchOffset / 2));

        // play sounds
        UniversalFunctions.PlayRandomSound(PlayerAudio, JumpingSounds, JumpingVolumeDB, PitchScale);

        // Pokud mame komponentu pro Shake - provedeme jej
        FPSCharacterMoveAnim FPSMoveAnim = ourCharacterBase as FPSCharacterMoveAnim;
        if(FPSMoveAnim != null)
        { FPSMoveAnim.GetCharacterCameraShakeComponent().ApplyUserParamShake(JumpShakeStrenght,JumpShakeFade);}
    }

    public async void ApplyEffectLand()
    {
        // calculate amount
        CalculateAmountLanding();

        // Anim
        //PlayerAnim.Play("CameraLandMedium");

        await ToSignal(GetTree(), "physics_frame");

        // Detect materal surface name and play specific audio set of footsteps
        EMaterialSurface materialSurface = AllMaterialSurfaces.GetMaterialSurfaceFromGroup(
            UniversalFunctions.DetectSurfaceMaterialOfFloor(ourCharacterBase,
            ourCharacterBase.GlobalPosition+(Vector3.Up*0.3f)));

        if (materialSurface != EMaterialSurface.None)
        {
            // Play random sound
            UniversalFunctions.PlayRandomSound(
                PlayerAudio,
                AllMaterialSurfaces.GetAudioArray(
                    materialSurface, all_material_surfaces.EMaterialSurfaceAudio.Landing),
                AllMaterialSurfaces.GetMaterialSurfaceAudioVolumeDB(
                    materialSurface, all_material_surfaces.EMaterialSurfaceAudio.Landing) - 8,
                AllMaterialSurfaces.GetMaterialSurfaceAudioPitch(
                    materialSurface, all_material_surfaces.EMaterialSurfaceAudio.Landing) - 0.1f);
        };

        // Pokud mame komponentu pro Shake - provedeme jej
        FPSCharacterMoveAnim FPSMoveAnim = ourCharacterBase as FPSCharacterMoveAnim;
        if (FPSMoveAnim != null)
        { FPSMoveAnim.GetCharacterCameraShakeComponent().ApplyUserParamShake(LandShakeStrenght, LandShakeFade); }
    }

    public void SetStartFallingNow() { lastYPosFallingStart = ourCharacterBase.GlobalPosition.Y; }
    public void CalculateAmountLanding()
    {
        // save actual y pos of character
        lastYPosFallingEnd = ourCharacterBase.GlobalPosition.Y;
        // execute landing effect event with param of fall height
        float heightfall = lastYPosFallingStart - lastYPosFallingEnd;

        GD.Print(heightfall);

        if (heightfall < 0.15)
        {
            // very mini
        }
        else if (heightfall <= 0.3f)
        {
            // mini land
            PlayerAnim.Play("CameraLandSmall");
        }
        else if (heightfall <= 1.2f)
        {
            // small land
            PlayerAnim.Play("CameraLandMedium");
        }
        else if (heightfall <= 2.5f)
        {
            // medium land
            PlayerAnim.Play("CameraLandMedium_2");
        }
        else if (heightfall <= 4.0f)
        {
            // high
            PlayerAnim.Play("CameraLandMedium_3");
        }
        else if (heightfall <= 6.0f)
        {
            // extreme
            PlayerAnim.Play("CameraLandMedium_4");
        }
        else
        {
            // death?
            PlayerAnim.Play("CameraLandDeath");
        }
    }
}