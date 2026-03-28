using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public static partial class StoryCsvImporter
{
    private const string EventsFileName = "Events.csv";
    private const string OptionsFileName = "Options.csv";
    private const string EffectsFileName = "Effects.csv";
    private const string CharactersFileName = "Characters.csv";
    private const string BagItemsFileName = "BagItems.csv";

    private static StoryDatabase BuildDatabase(string sourceFolder)
    {
        string eventsPath = Path.Combine(sourceFolder, EventsFileName);
        string optionsPath = Path.Combine(sourceFolder, OptionsFileName);
        string effectsPath = Path.Combine(sourceFolder, EffectsFileName);
        string charactersPath = Path.Combine(sourceFolder, CharactersFileName);
        string bagItemsPath = Path.Combine(sourceFolder, BagItemsFileName);

        List<EventRow> eventRows = ReadEvents(eventsPath);
        List<OptionRow> optionRows = ReadOptions(optionsPath);
        List<EffectRow> effectRows = ReadEffects(effectsPath);
        List<CharacterRow> characterRows = ReadCharacters(charactersPath);
        List<BagItemRow> bagItemRows = ReadBagItems(bagItemsPath);

        StoryDatabase database = new StoryDatabase
        {
            events = new List<StoryEventData>(),
            characters = new List<CharacterData>()
        };

        Dictionary<string, StoryEventData> eventMap = new Dictionary<string, StoryEventData>(StringComparer.Ordinal);
        Dictionary<string, OptionData> optionMap = new Dictionary<string, OptionData>(StringComparer.Ordinal);
        Dictionary<string, CharacterData> characterMap = new Dictionary<string, CharacterData>(StringComparer.Ordinal);

        BuildEvents(eventRows, database, eventMap);
        BuildOptions(optionRows, eventMap, optionMap);
        BuildCharacters(characterRows, bagItemRows, database, characterMap);
        BuildEffects(effectRows, eventMap, optionMap);
        ValidateOptionNextNodes(optionRows, eventMap);

        return database;
    }

    private static void BuildEvents(List<EventRow> eventRows, StoryDatabase database, Dictionary<string, StoryEventData> eventMap)
    {
        for (int i = 0; i < eventRows.Count; i++)
        {
            EventRow row = eventRows[i];
            if (eventMap.ContainsKey(row.Id))
            {
                throw new Exception("Duplicate event id: " + row.Id);
            }

            StoryEventData evt = new StoryEventData
            {
                id = row.Id,
                Text = row.Text,
                image = row.Image,
                effectDatas = new List<EffectData>(),
                options = new List<OptionData>()
            };

            eventMap.Add(row.Id, evt);
            database.events.Add(evt);
        }
    }

    private static void BuildOptions(List<OptionRow> optionRows, Dictionary<string, StoryEventData> eventMap, Dictionary<string, OptionData> optionMap)
    {
        Dictionary<string, List<OptionLink>> eventOptions = new Dictionary<string, List<OptionLink>>(StringComparer.Ordinal);

        for (int i = 0; i < optionRows.Count; i++)
        {
            OptionRow row = optionRows[i];
            if (!eventMap.ContainsKey(row.EventId))
            {
                throw new Exception("Option references missing event: optionId=" + row.OptionId + ", eventId=" + row.EventId);
            }

            if (optionMap.ContainsKey(row.OptionId))
            {
                throw new Exception("Duplicate option id: " + row.OptionId);
            }

            OptionData option = new OptionData
            {
                Text = row.Text,
                nextNodeId = row.NextNodeId,
                effects = new List<EffectData>()
            };

            optionMap.Add(row.OptionId, option);

            List<OptionLink> list;
            if (!eventOptions.TryGetValue(row.EventId, out list))
            {
                list = new List<OptionLink>();
                eventOptions.Add(row.EventId, list);
            }

            list.Add(new OptionLink { Order = row.Order, Option = option });
        }

        foreach (KeyValuePair<string, List<OptionLink>> pair in eventOptions)
        {
            List<OptionLink> links = pair.Value;
            links.Sort((a, b) => a.Order.CompareTo(b.Order));
            StoryEventData evt = eventMap[pair.Key];
            for (int i = 0; i < links.Count; i++)
            {
                evt.options.Add(links[i].Option);
            }
        }
    }

    private static void BuildCharacters(List<CharacterRow> characterRows, List<BagItemRow> bagItemRows, StoryDatabase database, Dictionary<string, CharacterData> characterMap)
    {
        for (int i = 0; i < characterRows.Count; i++)
        {
            CharacterRow row = characterRows[i];
            if (characterMap.ContainsKey(row.Id))
            {
                throw new Exception("Duplicate character id: " + row.Id);
            }

            CharacterData character = new CharacterData
            {
                id = row.Id,
                name = row.Name,
                description = row.Description,
                image = row.Image,
                bag = new List<ItemData>()
            };

            characterMap.Add(row.Id, character);
            database.characters.Add(character);
        }

        for (int i = 0; i < bagItemRows.Count; i++)
        {
            BagItemRow row = bagItemRows[i];
            CharacterData character;
            if (!characterMap.TryGetValue(row.CharacterId, out character))
            {
                throw new Exception("BagItems references missing character: characterId=" + row.CharacterId + ", itemId=" + row.ItemId);
            }

            ItemData item = new ItemData
            {
                id = row.ItemId,
                name = row.Name,
                description = row.Description,
                image = row.Image,
                useOption = new List<OptionData>()
            };

            character.bag.Add(item);
        }
    }

    private static void BuildEffects(List<EffectRow> effectRows, Dictionary<string, StoryEventData> eventMap, Dictionary<string, OptionData> optionMap)
    {
        for (int i = 0; i < effectRows.Count; i++)
        {
            EffectRow row = effectRows[i];
            EffectData effect = new EffectData
            {
                targetKey = row.TargetKey,
                type = row.Type,
                floatValue = row.FloatValue,
                stringValue = row.StringValue
            };

            if (string.Equals(row.OwnerType, "Event", StringComparison.OrdinalIgnoreCase))
            {
                StoryEventData evt;
                if (!eventMap.TryGetValue(row.OwnerId, out evt))
                {
                    throw new Exception("Effect references missing event owner: ownerId=" + row.OwnerId);
                }

                evt.effectDatas.Add(effect);
            }
            else if (string.Equals(row.OwnerType, "Option", StringComparison.OrdinalIgnoreCase))
            {
                OptionData opt;
                if (!optionMap.TryGetValue(row.OwnerId, out opt))
                {
                    throw new Exception("Effect references missing option owner: ownerId=" + row.OwnerId);
                }

                opt.effects.Add(effect);
            }
            else
            {
                throw new Exception("Invalid ownerType in Effects.csv: " + row.OwnerType + " (use Event or Option)");
            }
        }
    }

    private static void ValidateOptionNextNodes(List<OptionRow> optionRows, Dictionary<string, StoryEventData> eventMap)
    {
        for (int i = 0; i < optionRows.Count; i++)
        {
            OptionRow row = optionRows[i];
            if (!string.IsNullOrEmpty(row.NextNodeId) && !eventMap.ContainsKey(row.NextNodeId))
            {
                throw new Exception("Option nextNodeId not found: optionId=" + row.OptionId + ", nextNodeId=" + row.NextNodeId);
            }
        }
    }

    private static void WriteJson(StoryDatabase database, string outputPath)
    {
        string directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string json = JsonUtility.ToJson(database, true);
        File.WriteAllText(outputPath, json, new UTF8Encoding(false));
    }
}
