using Godot;
using Godot.Collections;
using System;


public partial class inventory_menu : Control
{
    private InventorySystem inventorySystem = null;

    public enum EActiveTypeEffect {instant,anim}
	private bool _active = false;
    private bool active_nextFrame = false;

    private AnimationPlayer anim = null;
    private AudioStreamPlayer audio = null;

    [Export] public EActiveTypeEffect ActiveTypeEffect = EActiveTypeEffect.anim;
    [Export] public Array<AudioStream> InventoryOpenAudios;
    [Export] public Array<AudioStream> InventoryCloseAudios;

    // inventory slots
    private Array<InventorySlot> allInventorySlots;

    private InventoryItemPreview itemPreview = null;

    public override void _Ready()
	{
        anim = GetNode<AnimationPlayer>("AnimationPlayer");
        audio = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
        itemPreview = GetNode<InventoryItemPreview>("Panel/Panel_ItemPreview/SubViewportContainer");

        SetActiveInstant(false);

        allInventorySlots = new Array<InventorySlot>();
        LoadAllSlots();     // load slots from scene to array
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        if (!GetActive()) return;

        // dovoli tento update az dalsi frame (kvuli inputu)
        if (!active_nextFrame) { active_nextFrame = true; return; }
        
        // close this inventory
        if(Input.IsActionJustPressed("toggleInventory"))
            SetActive(false);
	}

    public void Init(InventorySystem newInventorySystem){inventorySystem= newInventorySystem;}

    public void SetActive(bool newActive)
    {
        switch (ActiveTypeEffect)
        {
            case EActiveTypeEffect.instant:
                SetActiveInstant(newActive);
                break;
            case EActiveTypeEffect.anim:
                SetActiveAnim(newActive);
                break;
            default:
                break;
        }
    }

	public void SetActiveInstant(bool newActive) 
	{ 
		_active = newActive;
        Visible = newActive;

        // ziskame interact charactera
        FPSCharacter_Interaction interChar = (FPSCharacter_Interaction)GameMaster.GM.GetFPSCharacter();
        if (interChar == null) return;

        // ostatni akce pri zmene
        if (_active)
        {
            // vyresetuje lean a zoom hrace
            interChar.GetObjectCamera().SetActualLean(ObjectCamera.ELeanType.Center);
            interChar.SetCameraZoom(false);

            // zakaze char_inputs a zobrazi mys
            interChar.SetInputEnable(false);
            interChar.SetMouseVisible(true);
        }
        else
        {
            // povoli char_inputs + captured mouse (uvnitr funkce SetInputEnable)
            interChar.SetInputEnable(true);
            active_nextFrame = false;
        }
    }

    public void SetActiveAnim(bool newActive)
    {
        _active = newActive;

        // ziskame interact charactera
        FPSCharacter_Interaction interChar = (FPSCharacter_Interaction)GameMaster.GM.GetFPSCharacter();
        if (interChar == null) return;

        // ostatni akce pri zmene
        if (_active)
        {
            Visible = newActive;
            // vyresetuje lean a zoom hrace
            interChar.GetObjectCamera().SetActualLean(ObjectCamera.ELeanType.Center);
            interChar.SetCameraZoom(false);

            // zakaze char_inputs a zobrazi mys
            interChar.SetInputEnable(false);
            interChar.SetMouseVisible(true);

            // audio
            UniversalFunctions.PlayRandomSound(audio, InventoryOpenAudios, 0, 1);

            // anim
            anim.Play("open_inventory");

            // camera chake
            interChar.GetObjectCamera().ShakeCameraTest(0.3f, 0.35f, 1.0f, 2.0f);

            // try update items (add)
            AddAllItemsToSlots();
        }
        else
        {
            // povoli char_inputs + captured mouse (uvnitr funkce SetInputEnable)
            interChar.SetInputEnable(true);
            active_nextFrame = false;

            // audio
            UniversalFunctions.PlayRandomSound(audio, InventoryCloseAudios, 0, 1);

            // anim
            anim.PlayBackwards("open_inventory");

            // camera shake
            interChar.GetObjectCamera().ShakeCameraTest(0.3f, 0.35f, 1.0f, 2.0f);
        }
    }

    public bool GetActive() { return _active; }

    public void _on_animation_player_animation_finished(string animName)
    {
        if (!_active)
        {
            Visible = false;
            GD.Print("anim dohrala");

            // try update items destroy
            DestroyAllUIItemsInSlots();
        }
    }

    public void SetTabsToCenter()
    {
        TabContainer tabs = GetNode<TabContainer>("TabContainer") as TabContainer;
        tabs.SetAnchorsPreset(LayoutPreset.Center);
    }

    /* ITEMS */

    public InventorySystem GetInventorySystem(){return inventorySystem;}

    public void LoadAllSlots()
    {
        foreach (var slotNode in GetNode("Panel/GridContainer").GetChildren())
        {
            InventorySlot slot = slotNode as InventorySlot;
            slot.Init(this);
            allInventorySlots.Add(slot);
        }
    }

    //  START
    public void AddAllItemsToSlots()
    {
        for (int i = 0; i < GetInventorySystem().GetAllInventoryItems().Count; i++)
        {
            GD.Print("TryAddItemToSlot");
            GetAllInventoryItemSlots()[i].SetItem(GetInventorySystem().GetAllInventoryItems()[i]);
            //GetAllInventoryItemSlots()[i].
        }
    }

    public Array<InventorySlot> GetAllInventoryItemSlots() { return allInventorySlots; }

    // END
    public void DestroyAllUIItemsInSlots()
    {
        foreach (InventorySlot slot in GetAllInventoryItemSlots())
        {
            if(slot.HasUIItem())
                slot.DestroyUIItem();
        }
    }

    public void FocusUIItem(InventorySlot pressedInventorySlot)
    {
        if(pressedInventorySlot.HasUIItem())
        {
            GetNode<Label>("Panel/Panel/Label").Text = pressedInventorySlot.GetInventoryItemData().itemName;

            GetNode<RichTextLabel>("Panel/Panel/RichTextLabel").Text =
                pressedInventorySlot.GetInventoryItemData().itemInfoText;

            testing_render_inventory_items.ApplyItemSubViewportSetting(
            GetNode<SubViewport>("Panel/Panel_ItemPreview/SubViewportContainer/SubViewport"),
            pressedInventorySlot.GetInventoryItemData().SettingsForPreview,
            pressedInventorySlot.GetInventoryItemData().itemMeshPreview);

            itemPreview.Activate(true);
        }
        else
        {
            GetNode<Label>("Panel/Panel/Label").Text = "";
            GetNode<RichTextLabel>("Panel/Panel/RichTextLabel").Text = "";
            itemPreview.Deactivate();
        }
    }
}
