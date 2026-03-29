using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 运行时调试窗口：显示当前剧情与状态，并可直接点击可见选项推进剧情。
/// </summary>
public class StoryRuntimeDebugWindow : MonoBehaviour
{
    [SerializeField] private bool visibleOnStart = true;
    [SerializeField] private KeyCode toggleKey = KeyCode.F3;
    [SerializeField] private Rect windowRect = new Rect(20f, 20f, 520f, 560f);

    private bool isVisible;
    private Vector2 scrollPos;

    private void Awake()
    {
        isVisible = visibleOnStart;
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            isVisible = !isVisible;
        }
    }

    private void OnGUI()
    {
        if (!isVisible)
        {
            return;
        }

        windowRect = GUI.Window(GetInstanceID(), windowRect, DrawWindow, "Story Runtime Debug");
    }

    private void DrawWindow(int windowId)
    {
        BackBoard board = BackBoard.Instance;
        if (board == null)
        {
            GUILayout.Label("BackBoard instance not ready.");
            GUI.DragWindow();
            return;
        }

        scrollPos = GUILayout.BeginScrollView(scrollPos);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Replay: Back To Start", GUILayout.Width(200f)))
        {
            board.ReplayFromStartNodeKeepUnlocked();
        }
        GUILayout.Label("(Unlocked nodes are kept)");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Restart: From Scratch", GUILayout.Width(200f)))
        {
            board.RestartFromScratch();
        }
        GUILayout.Label("(Reset player state + all unlocks)");
        GUILayout.EndHorizontal();

        GUILayout.Space(8f);

        CharacterData character = board.GetCurrentCharacter();
        string characterId = character != null ? character.id : "(none)";

        GUILayout.Label("Node: " + (board.CurrentNodeId ?? "(none)"));
        GUILayout.Label("Character: " + characterId);
        GUILayout.Label("Health: " + board.GetCurrentHealth().ToString("0.##") + " / " + board.GetCurrentMaxHealth().ToString("0.##"));

        GUILayout.Space(8f);
        GUILayout.Label("All Nodes:");
        DrawStringList(board.GetAllNodeIds(), "(empty)");

        GUILayout.Space(8f);
        GUILayout.Label("Played Nodes (Whole Game):");
        DrawStringList(board.GetPlayedNodeIds(), "(empty)");

        GUILayout.Space(8f);
        GUILayout.Label("Visited Nodes (Current Life):");
        DrawStringList(board.GetCurrentLifeNodeIds(), "(empty)");

        GUILayout.Space(8f);
        GUILayout.Label("Unlocked Options:");
        DrawStringList(board.GetUnlockedOptionKeys(), "(empty)");

        GUILayout.Space(8f);
        GUILayout.Label("Active Status:");
        DrawStringList(board.GetActiveStatusSummaries(), "(none)");

        GUILayout.Space(8f);
        GUILayout.Label("Curren bag items:");
        if (character != null && character.bag != null)        {
            for (int i = 0; i < character.bag.Count; i++)
            {
                ItemData item = character.bag[i];
                if (item != null)
                {
                    GUILayout.Label("- " + (string.IsNullOrEmpty(item.name) ? item.id : item.name));
                }
            }
        }
        else
        {
            GUILayout.Label("(none)");
        }

        GUILayout.Space(8f);
        GUILayout.Label("Visible Options:");
        List<OptionData> visibleOptions = board.GetVisibleOptions();
        if (visibleOptions.Count == 0)
        {
            GUILayout.Label("(none)");
        }
        else
        {
            for (int i = 0; i < visibleOptions.Count; i++)
            {
                OptionData option = visibleOptions[i];
                if (option == null)
                {
                    continue;
                }

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Select " + i, GUILayout.Width(80f)))
                {
                    board.ChooseOption(i);
                }

                string label = option.Text ?? "(no text)";
                string next = string.IsNullOrEmpty(option.nextNodeId) ? "(stay)" : option.nextNodeId;
                GUILayout.Label(label + "  ->  " + next);
                GUILayout.EndHorizontal();
            }
        }

        GUILayout.EndScrollView();
        GUI.DragWindow();
    }

    private static void DrawStringList(List<string> values, string emptyText)
    {
        if (values == null || values.Count == 0)
        {
            GUILayout.Label(emptyText);
            return;
        }

        for (int i = 0; i < values.Count; i++)
        {
            GUILayout.Label("- " + values[i]);
        }
    }
}
