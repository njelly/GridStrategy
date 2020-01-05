using System;
using System.Collections.Generic;
using System.IO;
using Tofunaut.GridStrategy.Game;
using Tofunaut.UnityUtils;
using UnityEngine;

namespace Tofunaut.GridStrategy
{
    // --------------------------------------------------------------------------------------------
    public class Config
    {
        private static readonly string DefaultConfigPath = Path.Combine(Application.streamingAssetsPath, "DefaultConfig.txt");

        private const string CardsKey = "Cards";
        private const string DecksKey = "Decks";
        private const string UnitsKey = "Units";
        private const string OpponentsKey = "Opponents";

        private delegate bool ParseRawDataDelegate(object[] rawData);

        private Dictionary<string, CardData> _idToCardData;
        private Dictionary<string, DeckData> _idToDeckData;
        private Dictionary<string, UnitData> _idToUnitData;
        private Dictionary<string, OpponentData> _idToOpponentData;

        // --------------------------------------------------------------------------------------------
        public Config(string serializedData, bool overwriteDefaultConfig)
        {
            Dictionary<string, object[]> sheetData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object[]>>(serializedData);

            bool parsedWithErrors = false;
            parsedWithErrors |= !TryParseRawData(CardsKey, sheetData, BuildCardDatas);
            parsedWithErrors |= !TryParseRawData(DecksKey, sheetData, BuildDeckDatas);
            parsedWithErrors |= !TryParseRawData(UnitsKey, sheetData, BuildUnitDatas);
            parsedWithErrors |= !TryParseRawData(OpponentsKey, sheetData, BuildOpponentDatas);

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
        public PlayerData GetPlayerDataFromOpponentId(string opponentId)
        {
            OpponentData opponentData = GetOpponentData(opponentId);

            return new PlayerData()
            {
                name = opponentData.id, // TODO: convert this using a string library for i18n
                deckData = GetDeckData(opponentData.deckId),
                heroData = GetUnitData(opponentData.heroId),
                headSpritePath = opponentData.headSpritePath,
            };
        }

        // --------------------------------------------------------------------------------------------
        public List<string> GetCardIdsInDeck(string deckId)
        {
            return new List<string>(GetDeckData(deckId).cardIdToCount.Keys);
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

                // displayname
                if (rawCardData.TryGetValue("displayname", out object displayNameObj))
                {
                    cardData.displayName = displayNameObj.ToString();
                }
                else
                {
                    hasErrors |= true;
                    Debug.LogError($"card data index {i} is missing a display name");
                }

                // type
                if (rawCardData.TryGetValue("type", out object typeObj))
                {
                    if (Enum.TryParse(typeObj.ToString(), true, out Card.Type cardType))
                    {
                        cardData.type = cardType;
                    }
                    else
                    {
                        hasErrors |= true;
                        Debug.LogError($"Could not parse {typeObj.ToString()} as Card.Type, index {i}");
                    }
                }
                else
                {
                    hasErrors |= true;
                    Debug.LogError($"card data index {i} is missing a card type");
                }

                // solidarityreq
                if (rawCardData.TryGetValue("solidarityreq", out object solidarityreqObject))
                {
                    if (Int32.TryParse(solidarityreqObject.ToString(), out int solidarityRequired))
                    {
                        cardData.solidarityRequired = solidarityRequired;
                    }
                    else
                    {
                        hasErrors |= true;
                        Debug.LogError($"Could not parse {solidarityreqObject.ToString()} as int, index {i}");
                    }
                }
                else
                {
                    hasErrors |= true;
                    Debug.LogError($"card data index {i} is missing a value for solidarityreq");
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
            bool hasErrors = false;

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
                    hasErrors = true;
                }

                // prefabPath
                if (rawUnitData.TryGetValue("prefab", out object prefabPathObj))
                {
                    unitData.prefabPath = prefabPathObj.ToString();
                }
                else
                {
                    Debug.LogError($"unit data index {i} is missing a prefab");
                    hasErrors = true;
                }

                // health
                if (rawUnitData.TryGetValue("health", out object healthObj))
                {
                    if (Int32.TryParse(healthObj.ToString(), out int health))
                    {
                        unitData.health = health;
                    }
                    else
                    {
                        Debug.LogError($"Could not parse {healthObj.ToString()} as int, index {i}");
                        hasErrors = true;
                    }
                }
                else
                {
                    Debug.LogError($"unit data index {i} is missing a value for health");
                    hasErrors = true;
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
                        hasErrors = true;
                    }
                }
                else
                {
                    Debug.LogError($"unit data index {i} is missing a value for moverange");
                    hasErrors = true;
                }

                _idToUnitData.Add(unitData.id, unitData);
            }

            return !hasErrors;
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

                _idToOpponentData.Add(opponentData.id, opponentData);
            }

            return !hasError;
        }

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
        public string displayName;
        public Card.Type type;
        public int solidarityRequired;
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
        public string prefabPath;
        public float health;
        public int moveRange;
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
