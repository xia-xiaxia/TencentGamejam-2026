using UnityEngine;

public partial class BackBoard
{
    public bool EnterNode(string nodeId)
    {
        if (string.IsNullOrEmpty(nodeId))
        {
            Debug.LogWarning("EnterNode 失败：nodeId 为空。", this);
            return false;
        }

        StoryEventData node;
        if (!eventMap.TryGetValue(nodeId, out node))
        {
            Debug.LogWarning("EnterNode 失败：未找到节点 " + nodeId, this);
            return false;
        }

        CurrentNodeId = nodeId;
        ApplyEffects(node.effectDatas);

        if (OnNodeChanged != null)
        {
            OnNodeChanged(node);
        }

        return true;
    }

    public bool ChooseOption(int optionIndex)
    {
        StoryEventData node = CurrentNode;
        if (node == null)
        {
            Debug.LogWarning("ChooseOption 失败：当前没有节点。", this);
            return false;
        }

        if (node.options == null || optionIndex < 0 || optionIndex >= node.options.Count)
        {
            Debug.LogWarning("ChooseOption 失败：选项索引越界 " + optionIndex, this);
            return false;
        }

        OptionData option = node.options[optionIndex];
        if (option == null)
        {
            Debug.LogWarning("ChooseOption 失败：选项数据为空。", this);
            return false;
        }

        ApplyEffects(option.effects);

        if (string.IsNullOrEmpty(option.nextNodeId))
        {
            return true;
        }

        return EnterNode(option.nextNodeId);
    }

    public Sprite GetCurrentNodeSprite()
    {
        StoryEventData node = CurrentNode;
        if (node == null || string.IsNullOrEmpty(node.image))
        {
            return null;
        }

        return Resources.Load<Sprite>(node.image);
    }

    private bool TryGetStartNodeId(out string nodeId)
    {
        if (!string.IsNullOrEmpty(startNodeId))
        {
            nodeId = startNodeId;
            return true;
        }

        if (storyDatabase != null && storyDatabase.events != null && storyDatabase.events.Count > 0)
        {
            StoryEventData firstNode = storyDatabase.events[0];
            if (firstNode != null && !string.IsNullOrEmpty(firstNode.id))
            {
                nodeId = firstNode.id;
                return true;
            }
        }

        nodeId = null;
        return false;
    }
}
