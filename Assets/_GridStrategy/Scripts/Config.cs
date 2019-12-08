using System;
using System.Collections.Generic;
using Tofunaut.GridStrategy.Game;
using UnityEngine;

namespace Tofunaut.GridStrategy
{
    // --------------------------------------------------------------------------------------------
    public class Config
    {
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
        public Config(string serializedData)
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
            }
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
                if (rawUnitData.TryGetValue("movespeed", out object moveSpeedObj))
                {
                    if (Int32.TryParse(moveSpeedObj.ToString(), out int moveSpeed))
                    {
                        unitData.moveSpeed = moveSpeed;
                    }
                    else
                    {
                        Debug.LogError($"Could not parse {moveSpeedObj.ToString()} as int, index {i}");
                        hasErrors = true;
                    }
                }
                else
                {
                    Debug.LogError($"unit data index {i} is missing a value for move_speed");
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

                _idToOpponentData.Add(opponentData.id, opponentData);
            }

            return !hasError;
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
        public int moveSpeed;
    }

    // --------------------------------------------------------------------------------------------
    [Serializable]
    public struct OpponentData
    {
        public string id;
        public string heroId;
        public string deckId;
    }
}
