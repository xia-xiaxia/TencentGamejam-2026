public static partial class StoryCsvImporter
{
    private class EventRow
    {
        public string Id;
        public string Text;
        public string Image;
    }

    private class OptionRow
    {
        public string OptionId;
        public string EventId;
        public string Text;
        public string NextNodeId;
        public string RequiredItemId;
        public string RequiredUnlockedNodeId;
        public string UnlockConditionText;
        public string ConsumeItemId;
        public int Order;
    }

    private class EffectRow
    {
        public string OwnerType;
        public string OwnerId;
        public string TargetKey;
        public string Type;
        public float FloatValue;
        public string StringValue;
    }

    private class CharacterRow
    {
        public string Id;
        public string Name;
        public string Description;
        public string Image;
        public float Health;
    }

    private class BagItemRow
    {
        public string CharacterId;
        public string ItemId;
        public string Name;
        public string Description;
        public string Image;
    }

    private class OptionLink
    {
        public int Order;
        public OptionData Option;
    }
}
