////////////////////////////////////////////////////////////////////////////////
//
//  Config (c) 2020 Tofunaut
//
//  Created by Nathaniel Ellingson for TofuUnity on 01/09/2020
//
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using Tofunaut.Core;
using Tofunaut.GridStrategy.Game;
using Tofunaut.UnityUtils;
using UnityEngine;

namespace Tofunaut.GridStrategy
{
    // --------------------------------------------------------------------------------------------
    public class Config
    {
        public const string LangKeyError = "#LANG#ERROR#";
        public const string StringIdError = "#ID#ERROR#";
        public static class LanguageKeys
        {
            public const string IdKey = "id";
            public const string English = "en";
        }

        private static readonly string DefaultConfigPath = Path.Combine(Application.streamingAssetsPath, "DefaultConfig.txt");

        private const string CardsKey = "Cards";
        private const string DecksKey = "Decks";
        private const string UnitsKey = "Units";
        private const string OpponentsKey = "Opponents";
        private const string SkillsKey = "Skills";
        private const string I18NKey = "i18n";

        private delegate bool ParseRawDataDelegate(object[] rawData);

        public int MaxPlayerEnergy { get; private set; }
        public int MaxHandSize { get; private set; }
        public int StartHandSize { get; private set; }

        private Dictionary<string, CardData> _idToCardData;
        private Dictionary<string, DeckData> _idToDeckData;
        private Dictionary<string, UnitData> _idToUnitData;
        private Dictionary<string, OpponentData> _idToOpponentData;
        private Dictionary<string, SkillData> _idToSkillData;
        private Dictionary<string, Dictionary<string, string>> _i18n;
        private string _languageKey;

        // --------------------------------------------------------------------------------------------
        public Config(string serializedData, bool overwriteDefaultConfig)
        {
            Dictionary<string, object[]> sheetData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object[]>>(serializedData);

            bool parsedWithErrors = false;

            // default to English, since... I speak english...
            _languageKey = LanguageKeys.English;

            // get global data
            if (sheetData.TryGetValue("Global", out object[] globalDatas))
            {
                if (globalDatas.Length <= 0)
                {
                    Debug.LogError("global datas array is empty");
                }
                else
                {
                    if (globalDatas.Length > 1)
                    {
                        Debug.LogWarning("global data array is longer than 1, other entries will be ignored");
                    }

                    parsedWithErrors |= !TryParseGlobals(Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(globalDatas[0].ToString()));
                }
            }

            parsedWithErrors |= !TryParseRawData(CardsKey, sheetData, BuildCardDatas);
            parsedWithErrors |= !TryParseRawData(DecksKey, sheetData, BuildDeckDatas);
            parsedWithErrors |= !TryParseRawData(UnitsKey, sheetData, BuildUnitDatas);
            parsedWithErrors |= !TryParseRawData(OpponentsKey, sheetData, BuildOpponentDatas);
            parsedWithErrors |= !TryParseRawData(SkillsKey, sheetData, BuildSkillDatas);
            parsedWithErrors |= !TryParseI18N(sheetData);

            if (parsedWithErrors)
            {
                Debug.Log("parsed config with errors, please review logs above");
            }
            else
            {
                Debug.Log("parsed config");

                if (overwriteDefaultConfig)
                {
                    // write all data to the DefaultConfigPath so we're always up to date
                    File.WriteAllLines(DefaultConfigPath, new[] { serializedData });
                }
            }
        }

        // --------------------------------------------------------------------------------------------
        public CardData GetCardData(string id)
        {
            if (_idToCardData.TryGetValue(id, out CardData cardData))
            {
                return cardData;
            }

            Debug.LogError($"no card for the id {id}");
            return new CardData();
        }

        // --------------------------------------------------------------------------------------------
        public DeckData GetDeckData(string id)
        {
            if (_idToDeckData.TryGetValue(id, out DeckData deckData))
            {
                return deckData;
            }

            Debug.LogError($"no deck for the id {id}");
            return new DeckData();
        }

        // --------------------------------------------------------------------------------------------
        public UnitData GetUnitData(string id)
        {
            if (_idToUnitData.TryGetValue(id, out UnitData unitData))
            {
                return unitData;
            }

            Debug.LogError($"no unit for the id {id}");
            return new UnitData();
        }

        // --------------------------------------------------------------------------------------------
        public OpponentData GetOpponentData(string id)
        {
            if (_idToOpponentData.TryGetValue(id, out OpponentData opponentData))
            {
                return opponentData;
            }

            Debug.LogError($"no opponent for the id {id}");
            return new OpponentData();
        }

        // --------------------------------------------------------------------------------------------
        public SkillData GetSkillData(string id)
        {
            if (_idToSkillData.TryGetValue(id, out SkillData skillData))
            {
                return skillData;
            }

            Debug.LogError($"no skill for the id {id}");
            return new SkillData();
        }

        // --------------------------------------------------------------------------------------------
        public void SetLanugage(string languageKey)
        {
            _languageKey = languageKey;
        }

        // --------------------------------------------------------------------------------------------
        public string Localize(string id)
        {
            if (_i18n.TryGetValue(id, out Dictionary<string, string> langDict))
            {
                if (langDict.TryGetValue(id, out string toReturn))
                {
                    return toReturn;
                }
                else
                {
                    return StringIdError;
                }
            }
            else
            {
                return LangKeyError;
            }
        }

        // --------------------------------------------------------------------------------------------
        public PlayerData GetPlayerDataFromOpponentId(string opponentId)
        {
            OpponentData opponentData = GetOpponentData(opponentId);

            return new PlayerData()
            {
                name = opponentData.id, // TODO: convert this using a string library for i18n
                deckData = GetDeckData(opponentData.deckId),
                heroData = GetUnitData(opponentData.heroId),
                headSpritePath = opponentData.headSpritePath,
                initialSource = opponentData.initialSource,
            };
        }

        // --------------------------------------------------------------------------------------------
        public List<string> GetCardIdsInDeck(string deckId)
        {
            return new List<string>(GetDeckData(deckId).cardIdToCount.Keys);
        }

        // --------------------------------------------------------------------------------------------
        public bool TryParseGlobals(Dictionary<string, object> globalData)
        {
            if (globalData.TryGetValue("maxplayerenergy", out object maxPlayerEnergyObj))
            {
                if (int.TryParse(maxPlayerEnergyObj.ToString(), out int maxPlayerEnergy))
                {
                    this.MaxPlayerEnergy = maxPlayerEnergy;
                }
                else
                {
                    Debug.LogError($"Could not parse {maxPlayerEnergyObj.ToString()} as int for MaxPlayerEnergy");
                    return false;
                }
            }
            else
            {
                Debug.LogError("the config data is missing a value for MaxPlayerEnergy");
                return false;
            }


            if (globalData.TryGetValue("maxhandsize", out object maxHandSizeObj))
            {
                if (int.TryParse(maxHandSizeObj.ToString(), out int maxHandSize))
                {
                    this.MaxHandSize = maxHandSize;
                }
                else
                {
                    Debug.LogError($"Could not parse {maxHandSizeObj.ToString()} as int for MaxHandSize");
                    return false;
                }
            }
            else
            {
                Debug.LogError("the config data is missing a value for MaxHandSize");
                return false;
            }


            if (globalData.TryGetValue("starthandsize", out object startHandSizeObj))
            {
                if (int.TryParse(startHandSizeObj.ToString(), out int startHandSize))
                {
                    this.StartHandSize = startHandSize;
                }
                else
                {
                    Debug.LogError($"Could not parse {startHandSizeObj.ToString()} as int for StartHandSize");
                    return false;
                }
            }
            else
            {
                Debug.LogError("the config data is missing a value for StartHandSize");
                return false;
            }

            return true;
        }

        // --------------------------------------------------------------------------------------------
        private bool TryParseRawData(string key, Dictionary<string, object[]> sheetData, ParseRawDataDelegate parseRawDataDelegate)
        {
            if (sheetData.TryGetValue(key, out object[] rawData))
            {
                return parseRawDataDelegate(rawData);
            }

            Debug.LogError($"the string {key} was not present in the sheeet data");
            return false;
        }

        // --------------------------------------------------------------------------------------------
        private bool BuildCardDatas(object[] rawCardDatas)
        {
            _idToCardData = new Dictionary<string, CardData>();
            bool hasErrors = false;

            for (int i = 0; i < rawCardDatas.Length; i++)
            {
                Dictionary<string, object> rawCardData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(rawCardDatas[i].ToString());
                CardData cardData = new CardData();

                // id
                if (rawCardData.TryGetValue("id", out object idObj))
                {
                    cardData.id = idObj.ToString();
                }
                else
                {
                    hasErrors |= true;
                    Debug.LogError($"card data index {i} is missing an id");
                }

                // energyreq
                if (rawCardData.TryGetValue("energyreq", out object energyReqObj))
                {
                    if (Int32.TryParse(energyReqObj.ToString(), out int energyRequired))
                    {
                        cardData.energyRequired = energyRequired;
                    }
                    else
                    {
                        hasErrors |= true;
                        Debug.LogError($"Could not parse {energyReqObj.ToString()} as int, index {i}");
                    }
                }
                else
                {
                    hasErrors |= true;
                    Debug.LogError($"card data index {i} is missing a value for solidarityreq");
                }

                // illustrationPrefab
                if (rawCardData.TryGetValue("illustrationprefab", out object illustrationPrefabObj))
                {
                    cardData.illustrationPrefabPath = illustrationPrefabObj.ToString();
                }

                // spawnUnitId
                if (rawCardData.TryGetValue("spawnunit", out object spawnUnitIdObj))
                {
                    cardData.spawnUnitId = spawnUnitIdObj.ToString();
                }

                // useSkillId
                if (rawCardData.TryGetValue("useskillid", out object useSkillIdObj))
                {
                    cardData.useSkillId = useSkillIdObj.ToString();
                }

                _idToCardData.Add(cardData.id, cardData);

            }

            return !hasErrors;
        }

        // --------------------------------------------------------------------------------------------
        private bool BuildDeckDatas(object[] rawDeckDatas)
        {
            _idToDeckData = new Dictionary<string, DeckData>();
            bool hasErrors = false;

            for (int i = 0; i < rawDeckDatas.Length; i++)
            {
                Dictionary<string, object> rawDeckData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(rawDeckDatas[i].ToString());

                // decks are different because there are many "deck datas" with the same id. All of them should go in the same deck.
                // this will error if id is not the first key, so TODO: make this more robust
                foreach (string key in rawDeckData.Keys)
                {
                    switch (key)
                    {
                        case "id":
                            string deckId = rawDeckData["id"].ToString();
                            if (!_idToDeckData.ContainsKey(deckId))
                            {
                                // create a new deck data if we don't have one for this ID yet
                                DeckData newDeckData = new DeckData()
                                {
                                    id = deckId,
                                    cardIdToCount = new Dictionary<string, int>(),
                                };
                                _idToDeckData.Add(deckId, newDeckData);
                            }
                            break;
                        case "card":
                            string deckIdForCard = rawDeckData["id"].ToString();
                            string cardId = rawDeckData["card"].ToString();
                            if (_idToDeckData.TryGetValue(deckIdForCard, out DeckData deckData))
                            {
                                int count = 0;
                                deckData.cardIdToCount.TryGetValue(cardId, out count);
                                deckData.cardIdToCount[cardId] = count;
                                _idToDeckData[deckIdForCard] = deckData;
                            }
                            break;
                        case "count":
                            string deckIdForCount = rawDeckData["id"].ToString();
                            string cardIdForCount = rawDeckData["card"].ToString();
                            if (_idToDeckData.TryGetValue(deckIdForCount, out DeckData deckDataForCount))
                            {
                                if (!Int32.TryParse(rawDeckData["count"].ToString(), out int countForCard))
                                {
                                    hasErrors = true;
                                    Debug.LogError($"could not parse string to int as count for card {cardIdForCount}: {rawDeckData["count"].ToString()}");
                                }
                                else
                                {
                                    deckDataForCount.cardIdToCount[cardIdForCount] = countForCard;
                                }

                                _idToDeckData[deckIdForCount] = deckDataForCount;
                            }
                            break;
                        default:
                            hasErrors = true;
                            Debug.LogError($"Unhandled column for deck data: {key}");
                            break;
                    }

                }
            }

            return !hasErrors;
        }

        // --------------------------------------------------------------------------------------------
        private bool BuildUnitDatas(object[] rawUnitsDatas)
        {
            _idToUnitData = new Dictionary<string, UnitData>();
            bool hasError = false;

            for (int i = 0; i < rawUnitsDatas.Length; i++)
            {
                Dictionary<string, object> rawUnitData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(rawUnitsDatas[i].ToString());
                UnitData unitData = new UnitData();

                // id
                if (rawUnitData.TryGetValue("id", out object idObj))
                {
                    unitData.id = idObj.ToString();
                }
                else
                {
                    Debug.LogError($"unit data index {i} is missing an id");
                    hasError = true;
                }

                // aspect
                if (rawUnitData.TryGetValue("aspect", out object aspectObj))
                {
                    if (Enum.TryParse(aspectObj.ToString(), out EAspect aspect))
                    {
                        unitData.aspect = aspect;
                    }
                    else
                    {
                        Debug.LogError($"Could not parse {aspectObj.ToString()} as EAspect");
                    }
                }
                else
                {
                    Debug.LogError($"skill data index {i} is missing an id");
                    hasError = true;
                }

                // prefabPath
                if (rawUnitData.TryGetValue("prefab", out object prefabPathObj))
                {
                    unitData.prefabPath = prefabPathObj.ToString();
                }
                else
                {
                    Debug.LogError($"unit data index {i} is missing a prefab");
                    hasError = true;
                }

                // health
                if (rawUnitData.TryGetValue("health", out object healthObj))
                {
                    if (int.TryParse(healthObj.ToString(), out int health))
                    {
                        unitData.health = health;
                    }
                    else
                    {
                        Debug.LogError($"Could not parse {healthObj.ToString()} as int, index {i}");
                        hasError = true;
                    }
                }
                else
                {
                    Debug.LogError($"unit data index {i} is missing a value for health");
                    hasError = true;
                }

                // moveSpeed
                if (rawUnitData.TryGetValue("moverange", out object moveRangeObj))
                {
                    if (Int32.TryParse(moveRangeObj.ToString(), out int moveRange))
                    {
                        unitData.moveRange = moveRange;
                    }
                    else
                    {
                        Debug.LogError($"Could not parse {moveRangeObj.ToString()} as int, index {i}");
                        hasError = true;
                    }
                }
                else
                {
                    Debug.LogError($"unit data index {i} is missing a value for moverange");
                    hasError = true;
                }

                // travelSpeed
                if (rawUnitData.TryGetValue("travelspeed", out object travelSpeedObj))
                {
                    if (float.TryParse(travelSpeedObj.ToString(), out float travelSpeed))
                    {
                        unitData.travelSpeed = travelSpeed;
                    }
                    else
                    {
                        Debug.LogError($"Could not parse {travelSpeedObj.ToString()} as float, index {i}");
                        hasError = true;
                    }
                }
                else
                {
                    Debug.LogError($"unit data index {i} is missing a value for travelspeed");
                    hasError = true;
                }

                // skillid
                if (rawUnitData.TryGetValue("skillid", out object skillIdObj))
                {
                    unitData.skillId = skillIdObj.ToString();
                }
                else
                {
                    Debug.LogError($"unit data index {i} is missing a value for skillid");
                    hasError = true;
                }

                _idToUnitData.Add(unitData.id, unitData);
            }

            return !hasError;
        }

        // --------------------------------------------------------------------------------------------
        private bool BuildOpponentDatas(object[] rawOpponentDatas)
        {
            _idToOpponentData = new Dictionary<string, OpponentData>();
            bool hasError = false;

            for (int i = 0; i < rawOpponentDatas.Length; i++)
            {
                Dictionary<string, object> rawOpponentData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(rawOpponentDatas[i].ToString());
                OpponentData opponentData = new OpponentData();

                // id
                if (rawOpponentData.TryGetValue("id", out object idObj))
                {
                    opponentData.id = idObj.ToString();
                }
                else
                {
                    Debug.LogError($"opponent data index {i} is missing an id");
                    hasError = true;

                    // break so we don't add it to the dictionary
                    break;
                }

                // hero
                if (rawOpponentData.TryGetValue("hero", out object heroObj))
                {
                    opponentData.heroId = heroObj.ToString();
                }
                else
                {
                    Debug.LogError($"opponent data index {i} is missing a hero");
                    hasError = true;
                }

                // deck
                if (rawOpponentData.TryGetValue("deck", out object deckObj))
                {
                    opponentData.deckId = deckObj.ToString();
                }
                else
                {
                    Debug.LogError($"opponent data index {i} is missing a deck");
                    hasError = true;
                }

                // head sprite path
                if (rawOpponentData.TryGetValue("headspritepath", out object headSpritePathObj))
                {
                    opponentData.headSpritePath = headSpritePathObj.ToString();
                }
                else
                {
                    Debug.LogError($"opponent data index {i} is missing a headSpritePath");
                    hasError = true;
                }

                // initial source
                if (rawOpponentData.TryGetValue("initialsource", out object initialSourceObj))
                {
                    if (int.TryParse(initialSourceObj.ToString(), out int initialSource))
                    {
                        opponentData.initialSource = initialSource;
                    }
                    else
                    {
                        Debug.LogError($"Could not parse {initialSourceObj.ToString()} as int for initialsource");
                    }
                }
                else
                {
                    Debug.LogError($"opponent data index {i} is missing a initialsource");
                    hasError = true;
                }


                _idToOpponentData.Add(opponentData.id, opponentData);
            }

            return !hasError;
        }

        private bool BuildSkillDatas(object[] rawSkillDatas)
        {
            _idToSkillData = new Dictionary<string, SkillData>();
            bool hasError = false;

            for (int i = 0; i < rawSkillDatas.Length; i++)
            {
                Dictionary<string, object> rawSkillData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(rawSkillDatas[i].ToString());
                SkillData skillData = new SkillData();

                // id
                if (rawSkillData.TryGetValue("id", out object idObj))
                {
                    skillData.id = idObj.ToString();
                }
                else
                {
                    Debug.LogError($"skill data index {i} is missing an id");
                    hasError = true;

                    // break so this isn't added to the dictionary
                    break;
                }

                // target
                if (rawSkillData.TryGetValue("target", out object targetObj))
                {
                    if (Enum.TryParse(targetObj.ToString(), out SkillData.ETarget target))
                    {
                        skillData.target = target;
                    }
                    else
                    {
                        Debug.LogError($"Could not parse {targetObj.ToString()} as ETarget for target");
                    }
                }
                else
                {
                    Debug.LogError($"skill data index {i} is missing a damage allies");
                    hasError = true;
                }

                // aspect
                if (rawSkillData.TryGetValue("aspect", out object aspectObj))
                {
                    if (Enum.TryParse(aspectObj.ToString(), out EAspect aspect))
                    {
                        skillData.aspect = aspect;
                    }
                    else
                    {
                        Debug.LogError($"Could not parse {aspectObj.ToString()} as EAspect");
                    }
                }
                else
                {
                    Debug.LogError($"skill data index {i} is missing an id");
                    hasError = true;
                }

                // range
                if (rawSkillData.TryGetValue("range", out object rangeObj))
                {
                    if (int.TryParse(rangeObj.ToString(), out int range))
                    {
                        skillData.range = range;
                    }
                    else
                    {
                        Debug.LogError($"Could not parse {rangeObj.ToString()} as int for range");
                    }
                }
                else
                {
                    Debug.LogError($"skill data index {i} is missing a damage dealt");
                    hasError = true;
                }

                // areaSize
                if (rawSkillData.TryGetValue("areasize", out object areaSizeObj))
                {
                    if (int.TryParse(areaSizeObj.ToString(), out int areaSize))
                    {
                        skillData.areaSize = areaSize;
                    }
                    else
                    {
                        Debug.LogError($"Could not parse {areaSizeObj.ToString()} as int for areaSize");
                    }
                }
                else
                {
                    Debug.LogError($"skill data index {i} is missing an area size");
                    hasError = true;
                }

                // area type
                if (rawSkillData.TryGetValue("areatype", out object areaTypeObj))
                {
                    // make sure first character is uppper case
                    char[] chars = areaTypeObj.ToString().ToCharArray();
                    chars[0] = chars[0].ToString().ToUpper().ToCharArray()[0];
                    string areaTypeObjString = new string(chars);
                    if (Enum.TryParse(areaTypeObjString, out SkillData.EAreaType areaType))
                    {
                        skillData.areaType = areaType;
                    }
                    else
                    {
                        Debug.LogError($"Could not parse {areaTypeObj.ToString()} as EAreaType");
                    }
                }
                else
                {
                    Debug.LogError($"skill data index {i} is missing an area type");
                    hasError = true;
                }

                // area offset
                if (rawSkillData.TryGetValue("areaoffset", out object areaOffsetObj))
                {
                    if (IntVector2.TryParse(areaOffsetObj.ToString(), out IntVector2 areaOffset))
                    {
                        skillData.areaOffset = areaOffset;
                    }
                    else
                    {
                        Debug.LogError($"Could not parse {areaOffsetObj.ToString()} as IntVector2 for area offset");
                    }
                }
                else
                {
                    Debug.LogError($"skill data index {i} is missing an area offset");
                    hasError = true;
                }

                // damage dealt
                if (rawSkillData.TryGetValue("damagedealt", out object damageDealtObj))
                {
                    if (int.TryParse(damageDealtObj.ToString(), out int damageDealt))
                    {
                        skillData.damageDealt = damageDealt;
                    }
                    else
                    {
                        Debug.LogError($"Could not parse {damageDealtObj.ToString()} as int for damage dealt");
                    }
                }
                else
                {
                    Debug.LogError($"skill data index {i} is missing a damage dealt");
                    hasError = true;
                }

                _idToSkillData.Add(skillData.id, skillData);
            }

            return !hasError;
        }

        // --------------------------------------------------------------------------------------------
        public bool TryParseI18N(Dictionary<string, object[]> sheetData)
        {
            bool hasError = false;
            _i18n = new Dictionary<string, Dictionary<string, string>>();
            if (sheetData.TryGetValue(I18NKey, out object[] i18nObjArray))
            {
                foreach (object i18nObj in i18nObjArray)
                {
                    try
                    {
                        Dictionary<string, string> i18nData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(i18nObj.ToString());

                        if (i18nData.TryGetValue(LanguageKeys.IdKey, out string id))
                        {
                            _i18n.Add(id, new Dictionary<string, string>());

                            if (i18nData.TryGetValue(LanguageKeys.English, out string enValue))
                            {
                                _i18n[id].Add(LanguageKeys.English, enValue);
                            }
                            else
                            {
                                Debug.LogError($"no value found in i18n dict for {id} in the language {LanguageKeys.English}");
                                hasError = true;
                            }
                        }
                        else
                        {
                            Debug.LogError($"no value found in i18n dict for {LanguageKeys.IdKey}");
                            hasError = true;
                        }
                    }
                    catch (Newtonsoft.Json.JsonException e)
                    {
                        Debug.LogError($"failed to parse i18n object: {e.Message}");
                        hasError = true;
                    }
                }
            }
            else
            {
                Debug.LogError($"the string {I18NKey} was not present in the sheeet data");
                hasError = true;
            }

            return !hasError;
        }

        // --------------------------------------------------------------------------------------------
        public static Config DefaultConfig()
        {
            if (!File.Exists(DefaultConfigPath))
            {
                Debug.LogError("no data found for the default config!");
                return null;
            }

            string serializedData = File.ReadAllText(DefaultConfigPath);
            return new Config(serializedData, false);
        }
    }

    // --------------------------------------------------------------------------------------------
    [Serializable]
    public struct CardData
    {
        public string id;
        public int energyRequired;
        public string illustrationPrefabPath;
        public string spawnUnitId;
        public string useSkillId;
    }

    // --------------------------------------------------------------------------------------------
    [Serializable]
    public struct DeckData
    {
        public string id;
        public Dictionary<string, int> cardIdToCount;
    }

    // --------------------------------------------------------------------------------------------
    [Serializable]
    public struct UnitData
    {
        public string id;
        public EAspect aspect;
        public string prefabPath;
        public int health;
        public int moveRange;
        public float travelSpeed;
        public string skillId;
    }

    // --------------------------------------------------------------------------------------------
    [Serializable]
    public struct SkillData
    {
        public enum EAreaType
        {
            None = 0,
            Single = 1,
            Line = 2,
            Cone = 3,
            Diamond = 4,
            Circle = 5,
            Square = 6,
        }

        public enum ETarget
        {
            None = 0,
            Self = 1,
            Ally = 2,
            Enemy = 3,
            Tile = 4,
        }

        public string id;
        public ETarget target;
        public int range;
        public EAspect aspect;
        public int areaSize;
        public EAreaType areaType;
        public IntVector2 areaOffset;
        public int damageDealt;
    }

    // --------------------------------------------------------------------------------------------
    /// <summary>
    /// Data for an AI opponent in a game.
    /// </summary>
    [Serializable]
    public struct OpponentData
    {
        public string id;
        public string heroId;
        public string deckId;
        public string headSpritePath;
        public int initialSource;
    }

    // --------------------------------------------------------------------------------------------
    /// <summary>
    /// Data for a modifier that can be applied
    /// </summary>
    [Serializable]
    public struct UnitModifierData
    {
        public string id;
        public int numTurnsActive;
        public float skillDamageAdditive;
        public float skillDamageMultiplier;
    }

    // --------------------------------------------------------------------------------------------
    /// <summary>
    /// Data for a player in a game.
    /// </summary>
    [Serializable]
    public struct PlayerData
    {
        public string name;
        public DeckData deckData;
        public UnitData heroData;
        public string headSpritePath;
        public int initialSource;

        // --------------------------------------------------------------------------------------------
        public void LoadAssets(AssetManager assetManager)
        {
            assetManager.Load<Sprite>(headSpritePath);
        }

        // --------------------------------------------------------------------------------------------
        public void ReleaseAssets(AssetManager assetManager)
        {
            assetManager.Release<Sprite>(headSpritePath);
        }
    }
}
