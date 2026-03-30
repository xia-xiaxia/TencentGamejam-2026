using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public partial class BackBoard : MonoBehaviour, IStoryRuntime
{
    [Header("Data Source")]
    [SerializeField] private TextAsset storyJson;
    [SerializeField] private string storyJsonResourcePath = "Story/story";
    [SerializeField] private StoryDatabase storyDatabase;
    [SerializeField] private bool loadUnlockSaveOnStart = false;
    [SerializeField] private string startNodeId;
    [SerializeField] private string currentCharacterId;

    private readonly BackBoardStoryService storyService = new BackBoardStoryService();
    private readonly BackBoardCharacterService characterService = new BackBoardCharacterService();
    private readonly BackBoardValueService valueService = new BackBoardValueService();
    private readonly BackBoardStatusService statusService = new BackBoardStatusService();

    public static BackBoard Instance { get; private set; }

    /// <summary>
    /// 当前剧情节点 id。
    /// </summary>
    public string CurrentNodeId
    {
        get
        {
            return storyService.CurrentNodeId;
        }
    }

    /// <summary>
    /// 当前剧情节点数据。
    /// </summary>
    public StoryEventData CurrentNode
    {
        get
        {
            return storyService.CurrentNode;
        }
    }

    /// <summary>
    /// 当前是否处于游戏结束状态。
    /// </summary>
    public bool IsGameEnded { get; private set; }

    public event Action<StoryEventData> OnNodeChanged;
    public event Action<string> OnBlackboardValueChanged;
    public event Action<CharacterData> OnCurrentCharacterChanged;

    /// <summary>
    /// 初始化全局单例引用。
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            return;
        }

        if (Instance != this)
        {
            Debug.LogWarning("BackBoard 存在多个实例，这可能会导致数据混乱。", this);
        }
    }

    /// <summary>
    /// 启动时装载数据库并进入起始节点。
    /// </summary>
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

    /// <summary>
    /// 从配置数据重建剧情、角色与黑板状态。
    /// </summary>
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

        valueService.Clear();
        statusService.Clear();
        IsGameEnded = false;
        storyService.BuildEventMap(storyDatabase != null ? storyDatabase.events : null, this);
        if (loadUnlockSaveOnStart)
        {
            storyService.LoadUnlockProgress(this);
        }
        characterService.BuildCharacterMap(storyDatabase != null ? storyDatabase.characters : null, this);

        if (storyDatabase == null)
        {
            currentCharacterId = string.Empty;
            return;
        }

        currentCharacterId = characterService.ResolveCurrentCharacterId(currentCharacterId);

        if (!string.IsNullOrEmpty(currentCharacterId))
        {
            InitializeCurrentCharacterHealth();
        }
    }
}
