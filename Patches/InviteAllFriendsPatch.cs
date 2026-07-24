using TMPro;
using UnityEngine;

namespace AmongUsRevamped;

[HarmonyPatch(typeof(FriendsListUI), nameof(FriendsListUI.Open))]
public static class InviteAllFriendsButtonPatch
{
    public static PassiveButton inviteAllButton;
    private static TextMeshPro inviteAllLabel;

    public static void Postfix(FriendsListUI __instance) => Ensure(__instance);

    public static void Ensure(FriendsListUI __instance)
    {
        if (__instance == null || inviteAllButton != null) return;

        var template = __instance.ViewRequestsButton;
        if (template == null)
        {
            Logger.Warn("No template button in the friends panel, invite all button not created", "InviteAllFriends");
            return;
        }

        // The platform friends button occupies the slot right of the "Among Us Friends" pill and is
        // hidden on platforms without a friends list, so borrow its transform to sit beside the pill
        var slot = __instance.PlatformFriendsButton != null ? __instance.PlatformFriendsButton.transform : null;
        var parent = slot != null ? slot.parent : template.transform.parent;

        var clone = Object.Instantiate(template.gameObject, parent);
        clone.name = "InviteAllFriendsButton";

        if (slot != null)
        {
            clone.transform.localPosition = slot.localPosition;
            clone.transform.localScale = slot.localScale;

            if (__instance.PlatformFriendsButton.activeSelf)
                clone.transform.localPosition += new Vector3(1.9f, 0f, 0f);

            var slotRenderer = slot.GetComponent<SpriteRenderer>();
            if (slotRenderer != null)
            {
                var renderers = clone.GetComponentsInChildren<SpriteRenderer>(true);
                for (int i = 0; i < renderers.Length; i++)
                {
                    if (renderers[i] != null) renderers[i].size = slotRenderer.size;
                }
            }
        }
        else
        {
            clone.transform.localPosition = template.transform.localPosition + new Vector3(0f, -1.1f, -0.1f);
            clone.transform.localScale = template.transform.localScale * 0.85f;
        }

        inviteAllButton = clone.GetComponent<PassiveButton>();
        if (inviteAllButton == null)
        {
            Logger.Warn("Template button has no PassiveButton, invite all button not created", "InviteAllFriends");
            Object.Destroy(clone);
            return;
        }

        inviteAllButton.selected = false;
        if (inviteAllButton.selectedSprites != null) inviteAllButton.selectedSprites.SetActive(false);
        if (inviteAllButton.selectedInactiveSprites != null) inviteAllButton.selectedInactiveSprites.SetActive(false);
        if (inviteAllButton.onClickSprites != null) inviteAllButton.onClickSprites.SetActive(false);
        if (inviteAllButton.disabledSprites != null) inviteAllButton.disabledSprites.SetActive(false);

        // The template's label is a sibling rather than a child, so the clone comes out blank
        inviteAllLabel = clone.GetComponentInChildren<TextMeshPro>();
        if (inviteAllLabel == null && __instance.ViewRequestsText != null)
        {
            inviteAllLabel = Object.Instantiate(__instance.ViewRequestsText, clone.transform);
            inviteAllLabel.name = "InviteAllFriendsText";
            inviteAllLabel.transform.localPosition = new Vector3(0f, 0f, -0.1f);
            inviteAllLabel.transform.localScale = Vector3.one * 0.75f;
            inviteAllLabel.alignment = TextAlignmentOptions.Center;
        }

        if (inviteAllLabel != null)
        {
            inviteAllLabel.DestroyTranslator();
            inviteAllLabel.text = Translator.Get("inviteAllFriends");
        }

        inviteAllButton.OnClick = new();
        inviteAllButton.OnClick.AddListener((UnityEngine.Events.UnityAction)(() =>
        {
            if (InviteAllFriends.Ready)
            {
                InviteAllFriends.SendAll();
                return;
            }

            if (InviteAllFriends.Available)
                Logger.SendInGame(Translator.Get("inviteAllCooldown", Mathf.CeilToInt(InviteAllFriends.RemainingCooldown)));
            else
                Logger.SendInGame(Translator.Get("inviteAllNoLobby"));
        }));

        clone.SetActive(false);

        Logger.Info($"Created invite all button under {template.name} at {clone.transform.localPosition}", "InviteAllFriends");

        // Real coordinates of the surrounding widgets, so the placement can be corrected without guessing
        LogAnchor("ViewRequestsButton", __instance.ViewRequestsButton?.transform);
        LogAnchor("AddFriendArea", __instance.AddFriendArea?.transform);
        LogAnchor("InactiveAllFriends", __instance.InactiveAllFriends?.transform);
        LogAnchor("PlatformFriendsButton", __instance.PlatformFriendsButton?.transform);
        LogAnchor("FriendArea", __instance.FriendArea?.transform);
    }

    private static void LogAnchor(string label, Transform target)
    {
        if (target == null)
        {
            Logger.Info($"anchor {label}: null", "InviteAllFriends");
            return;
        }

        var renderer = target.GetComponent<SpriteRenderer>();
        string bounds = renderer != null ? $" size={renderer.size} bounds={renderer.bounds.size}" : "";
        Logger.Info($"anchor {label}: local={target.localPosition} world={target.position} parent={target.parent?.name}{bounds}", "InviteAllFriends");
    }
}

[HarmonyPatch(typeof(FriendsListUI), nameof(FriendsListUI.Update))]
public static class InviteAllFriendsButtonUpdatePatch
{
    private static string lastState;

    public static void Postfix(FriendsListUI __instance)
    {
        if (__instance == null) return;

        InviteAllFriendsButtonPatch.Ensure(__instance);

        var button = InviteAllFriendsButtonPatch.inviteAllButton;

        bool optionOn = Main.InviteAllButton.Value;
        bool open = __instance.IsOpen;
        bool rightTab = __instance.CurrentTab == FriendsListUI.FriendsListTab.AmongUsFriends;
        bool available = InviteAllFriends.Available;

        string state = $"button={button != null} option={optionOn} open={open} tab={__instance.CurrentTab} available={available} " +
                       $"(lobby={InviteAllFriends.DebugIsLobby} online={InviteAllFriends.DebugIsOnline} manager={FriendsListManager.InstanceExists} guest={InviteAllFriends.DebugIsGuest})";

        if (state != lastState)
        {
            lastState = state;
            Logger.Info(state, "InviteAllFriends");
        }

        if (button == null) return;

        bool show = optionOn && open && rightTab && available;
        if (button.gameObject.activeSelf != show) button.gameObject.SetActive(show);
    }
}

[HarmonyPatch(typeof(FriendsListUI), nameof(FriendsListUI.OnDisable))]
public static class InviteAllFriendsCleanupPatch
{
    public static void Postfix()
    {
        // The panel is rebuilt per scene, so drop the stale reference and let Open recreate it
        InviteAllFriendsButtonPatch.inviteAllButton = null;
    }
}
