using System.Collections.Generic;

public static partial class StoryCsvImporter
{
    private static List<EventRow> ReadEvents(string path)
    {
        List<Dictionary<string, string>> rows = ReadCsvAsMaps(path, new[] { "id", "Text", "image" });
        List<EventRow> result = new List<EventRow>();

        for (int i = 0; i < rows.Count; i++)
        {
            Dictionary<string, string> row = rows[i];
            EventRow item = new EventRow
            {
                Id = Require(row, "id", path),
                Text = Get(row, "Text"),
                Image = Get(row, "image")
            };

            result.Add(item);
        }

        return result;
    }

    private static List<OptionRow> ReadOptions(string path)
    {
        List<Dictionary<string, string>> rows = ReadCsvAsMaps(path, new[] { "optionId", "eventId", "Text", "nextNodeId" });
        List<OptionRow> result = new List<OptionRow>();

        for (int i = 0; i < rows.Count; i++)
        {
            Dictionary<string, string> row = rows[i];
            OptionRow item = new OptionRow
            {
                OptionId = Require(row, "optionId", path),
                EventId = Require(row, "eventId", path),
                Text = Get(row, "Text"),
                NextNodeId = Get(row, "nextNodeId"),
                RequiredItemId = Get(row, "requiredItemId"),
                RequiredUnlockedNodeId = Get(row, "requiredUnlockedNodeId"),
                ConsumeItemId = Get(row, "consumeItemId"),
                Order = ParseInt(Get(row, "order"), i)
            };

            result.Add(item);
        }

        return result;
    }

    private static List<EffectRow> ReadEffects(string path)
    {
        List<Dictionary<string, string>> rows = ReadCsvAsMaps(path, new[] { "ownerType", "ownerId", "targetKey", "type" });
        List<EffectRow> result = new List<EffectRow>();

        for (int i = 0; i < rows.Count; i++)
        {
            Dictionary<string, string> row = rows[i];
            EffectRow item = new EffectRow
            {
                OwnerType = Require(row, "ownerType", path),
                OwnerId = Require(row, "ownerId", path),
                TargetKey = Require(row, "targetKey", path),
                Type = Require(row, "type", path),
                FloatValue = ParseFloat(Get(row, "floatValue")),
                StringValue = Get(row, "stringValue")
            };

            result.Add(item);
        }

        return result;
    }

    private static List<CharacterRow> ReadCharacters(string path)
    {
        List<Dictionary<string, string>> rows = ReadCsvAsMaps(path, new[] { "id", "name", "description", "image", "health" });
        List<CharacterRow> result = new List<CharacterRow>();

        for (int i = 0; i < rows.Count; i++)
        {
            Dictionary<string, string> row = rows[i];
            CharacterRow item = new CharacterRow
            {
                Id = Require(row, "id", path),
                Name = Get(row, "name"),
                Description = Get(row, "description"),
                Image = Get(row, "image"),
                Health = ParseFloat(Get(row, "health"))
            };

            result.Add(item);
        }

        return result;
    }

    private static List<BagItemRow> ReadBagItems(string path)
    {
        List<Dictionary<string, string>> rows = ReadCsvAsMaps(path, new[] { "characterId", "itemId", "name", "description", "image" });
        List<BagItemRow> result = new List<BagItemRow>();

        for (int i = 0; i < rows.Count; i++)
        {
            Dictionary<string, string> row = rows[i];
            BagItemRow item = new BagItemRow
            {
                CharacterId = Require(row, "characterId", path),
                ItemId = Require(row, "itemId", path),
                Name = Get(row, "name"),
                Description = Get(row, "description"),
                Image = Get(row, "image")
            };

            result.Add(item);
        }

        return result;
    }
}
