using Godot;
using System;

public partial class dead_cam_body : RigidBody3D
{
    public bool isActivate = false;
    public float lerpSpeed = 100.0f;

    public override void _Ready()
    {
        base._Ready();
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        // lerping camera
        if(isActivate)
        {
            // lerp pos of camera
            GameMaster.GM.GetFPSCharacter().GetFPSCharacterCamera().GlobalPosition = 
                GameMaster.GM.GetFPSCharacter().GetFPSCharacterCamera().
                GlobalPosition.Lerp(GlobalPosition, lerpSpeed * (float)delta);
        }
    }

    public void ActivateDeadCam()
    {
        Camera3D characterCamera = GameMaster.GM.GetFPSCharacter().GetFPSCharacterCamera();
        Vector3 oldGlobalPosition = characterCamera.GlobalPosition;
        Vector3 oldDirectionPoint = GameMaster.GM.GetFPSCharacter().GlobalPosition + 
            (characterCamera.GlobalTransform.Basis.Z * -100.0f);

        // nastavi aktualni pozici na misto kde ma/mela byt kamera
        GlobalPosition = oldGlobalPosition;

        // vypne lerping kamery k characteru
        GameMaster.GM.GetFPSCharacter().GetObjectCamera().SetLerpToCharacterEnable(false);

        // zrusi stary child kamery s hracem
       characterCamera.GetParent().RemoveChild(characterCamera);

        // kamera se nove prida jako child do levelu na puvodni globalni pozici a pohled
        GameMaster.GM.LevelLoader.GetActualLevelScene().AddChild(characterCamera);
        characterCamera.GlobalPosition = oldGlobalPosition;
        characterCamera.LookAtFromPosition(characterCamera.GlobalPosition,oldDirectionPoint);

        // Physics Process logika zapnuta
        isActivate = true;
    }
}
