using Godot;
using System;

public partial class interactive_object : Node3D
{
	// komunikacni objekt
	public MessageObject msgObject;
	[Export] public NodePath InteractiveObjectCommunicationWith = null;

	public enum EInteractiveLevel { Disable, OnlyUse, OnlyPhysic, UseAndPhysic}
	public enum EInteractivePhysicType { GrabItem, GrabJoint, GrabAction}

	[Export] public EInteractiveLevel InteractiveLevel = EInteractiveLevel.OnlyUse;
	[Export] public EInteractivePhysicType InteractivePhysicType = EInteractivePhysicType.GrabItem;

    private bool isPlayerInRange = false;

	public override void _Ready()
	{
		if(InteractiveObjectCommunicationWith == null)
		{
            msgObject = new MessageObject(this, GetParent());
        }
		else
            msgObject = new MessageObject(this, GetNode(InteractiveObjectCommunicationWith));
    }

	public void _on_interactive_object_area_3d_body_entered(Node3D body)
	{
		if (InteractiveLevel == EInteractiveLevel.Disable) return;

        if (body.IsClass("CharacterBody3D"))
		{
			GameMaster.GM.Log.WriteLog(this, LogSystem.ELogMsgType.INFO, "Player is entered to area");
			isPlayerInRange = true;
		}
	}

	public void _on_interactive_object_area_3d_body_exited(Node3D body)
	{
		if (InteractiveLevel == EInteractiveLevel.Disable) return;

        if (body.IsClass("CharacterBody3D"))
		{
            GameMaster.GM.Log.WriteLog(this, LogSystem.ELogMsgType.INFO, "Player is exited area");
			isPlayerInRange = false;
		}
	}

	public bool GetIsPlayerInRange()
	{
		return isPlayerInRange;
	}

	public void Use(FPSCharacter_Interaction player)
	{
		if (InteractiveLevel == EInteractiveLevel.Disable) return;
        if (InteractiveLevel == EInteractiveLevel.OnlyUse || InteractiveLevel == EInteractiveLevel.UseAndPhysic)
		{
            // nastavime node data jako playera a posleme zpravu o use_action dal
            msgObject.SetNodeData(player);
            msgObject.SendMessageToGDNow("msg_use_action");
        }
	}

	public string GetUseActionName()
	{
		if (InteractiveLevel == EInteractiveLevel.Disable) return "";

        string result = msgObject.LoadStringDataFromGDNow("msg_get_use_action_text");
		return result;
	}

	public string GetInteractiveObjectName()
	{
        if (InteractiveLevel == EInteractiveLevel.Disable) return "";

        string result = msgObject.LoadStringDataFromGDNow("msg_get_interactive_object_name");
		return result;
	}

    public void GrabActionStart(FPSCharacter_Interaction character)
    {
        if (InteractiveLevel == EInteractiveLevel.Disable) return;

        msgObject.SetNodeData(character);
        msgObject.SendMessageToGDNow("msg_grab_action_start");
    }

	public void GrabActionEnd(FPSCharacter_Interaction character)
	{
        if (InteractiveLevel == EInteractiveLevel.Disable) return;

        msgObject.SetNodeData(character);
        msgObject.SendMessageToGDNow("msg_grab_action_end");
    }

    public virtual void message_update()
	{
		string msg = msgObject.GetMessage();
		switch (msg)
		{
			default: break;
		}
	}
}
