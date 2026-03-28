using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public partial class BackBoard : MonoBehaviour
{
    [Header("Data Source")]
    [SerializeField] private TextAsset storyJson;
    [SerializeField] private string storyJsonResourcePath = "Story/story";
    [SerializeField] private StoryDatabase storyDatabase;
    [SerializeField] private string startNodeId;
    [SerializeField] private string currentCharacterId;

    private readonly Dictionary<string, StoryEventData> eventMap = new Dictionary<string, StoryEventData>();
    private readonly Dictionary<string, CharacterData> characterMap = new Dictionary<string, CharacterData>();
    private readonly Dictionary<string, float> floatBoard = new Dictionary<string, float>();
    private readonly Dictionary<string, string> stringBoard = new Dictionary<string, string>();

    public string CurrentNodeId { get; private set; }
    public StoryEventData CurrentNode
    {
        get
        {
            if (string.IsNullOrEmpty(CurrentNodeId))
            {
                return null;
            }

            StoryEventData node;
            if (eventMap.TryGetValue(CurrentNodeId, out node))
            {
                return node;
            }

            return null;
        }
    }

    public event Action<StoryEventData> OnNodeChanged;
    public event Action<string> OnBlackboardValueChanged;
    public event Action<CharacterData> OnCurrentCharacterChanged;

    private void Start()
    {
        BuildDatabase();
        if (!TryGetStartNodeId(out var nodeId))
        {
            Debug.LogWarning("BackBoard 初始化失败：没有找到可用的开始节点。", this);
            return;
        }

        EnterNode(nodeId);
    }

    public void BuildDatabase()
    {
        if (storyJson == null && !string.IsNullOrEmpty(storyJsonResourcePath))
        {
            storyJson = Resources.Load<TextAsset>(storyJsonResourcePath);
        }

        if (storyJson != null)
        {
            storyDatabase = JsonUtility.FromJson<StoryDatabase>(storyJson.text);
        }

        eventMap.Clear();
        characterMap.Clear();
        if (storyDatabase == null)
        {
            return;
        }

        if (storyDatabase.events != null)
        {
            for (int i = 0; i < storyDatabase.events.Count; i++)
            {
                StoryEventData evt = storyDatabase.events[i];
                if (evt == null || string.IsNullOrEmpty(evt.id))
                {
                    continue;
                }

                if (eventMap.ContainsKey(evt.id))
                {
                    Debug.LogWarning("BackBoard 检测到重复节点 id: " + evt.id, this);
                    continue;
                }

                eventMap.Add(evt.id, evt);
            }
        }

        if (storyDatabase.characters == null)
        {
            return;
        }

        for (int i = 0; i < storyDatabase.characters.Count; i++)
        {
            CharacterData character = storyDatabase.characters[i];
            if (character == null || string.IsNullOrEmpty(character.id))
            {
                continue;
            }

            if (characterMap.ContainsKey(character.id))
            {
                Debug.LogWarning("BackBoard 检测到重复角色 id: " + character.id, this);
                continue;
            }

            if (character.bag == null)
            {
                character.bag = new List<ItemData>();
            }

            characterMap.Add(character.id, character);
        }

        if (string.IsNullOrEmpty(currentCharacterId) && storyDatabase.characters != null && storyDatabase.characters.Count > 0)
        {
            for (int i = 0; i < storyDatabase.characters.Count; i++)
            {
                CharacterData character = storyDatabase.characters[i];
                if (character != null && !string.IsNullOrEmpty(character.id))
                {
                    currentCharacterId = character.id;
                    break;
                }
            }
        }
    }
}
