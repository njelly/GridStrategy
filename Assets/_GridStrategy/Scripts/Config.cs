using System;
using System.Collections.Generic;
using Tofunaut.GridStrategy.Game;
using UnityEngine;

namespace Tofunaut.GridStrategy
{
    // --------------------------------------------------------------------------------------------
    public class Config
    {
        private Dictionary<string, CardData> _idToCardData;
        private Dictionary<string, DeckData> _idToDeckData;
        private Dictionary<string, UnitData> _idToUnitData;

        // --------------------------------------------------------------------------------------------
        public Config(string serializedData)
        {
            Dictionary<string, object[]> sheetToData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object[]>>(serializedData);

            // Cards
            if (sheetToData.TryGetValue("Cards", out object[] rawCardDatas))
            {
                BuildCardDatas(rawCardDatas);
            }
            else
            {
                throw new Exception("the string Cards was not present in the config data");
            }

            // Decks
            if (sheetToData.TryGetValue("Decks", out object[] rawDeckDatas))
            {
                BuildDeckDatas(rawDeckDatas);
            }
            else
            {
                throw new Exception("the string Decks was not present in the config data");
            }

            // Units
            if (sheetToData.TryGetValue("Units", out object[] rawUnitsDatas))
            {
                BuildUnitDatas(rawUnitsDatas);
            }
            else
            {
                throw new Exception("the string Units was not present in the config data");
            }

            Debug.Log("parsed config");
        }

        // --------------------------------------------------------------------------------------------
        private void BuildCardDatas(object[] rawCardDatas)
        {
            _idToCardData = new Dictionary<string, CardData>();

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
                    Debug.LogError($"card data index {i} is missing an id");
                }

                // displayname
                if (rawCardData.TryGetValue("displayname", out object displayNameObj))
                {
                    cardData.displayName = displayNameObj.ToString();
                }
                else
                {
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
                        Debug.LogError($"Could not parse {typeObj.ToString()} as Card.Type, index {i}");
                    }
                }
                else
                {
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
                        Debug.LogError($"Could not parse {solidarityreqObject.ToString()} as int, index {i}");
                    }
                }
                else
                {
                    Debug.LogError($"card data index {i} is missing a value for solidarityreq");
                }

                _idToCardData.Add(cardData.id, cardData);
            }
        }

        // --------------------------------------------------------------------------------------------
        private void BuildDeckDatas(object[] rawDeckDatas)
        {
            _idToDeckData = new Dictionary<string, DeckData>();
            
            for(int i = 0; i < rawDeckDatas.Length; i++)
            {
                Dictionary<string, object> rawDeckData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(rawDeckDatas[i].ToString());

                // decks are different because there are many "deck datas" with the same id. All of them should go in the same deck.
                // this will error if id is not the first key, so TODO: make this more robust
                foreach(string key in rawDeckData.Keys)
                {
                    switch(key)
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
                            if(_idToDeckData.TryGetValue(deckIdForCard, out DeckData deckData))
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
                                    Debug.Log($"could not parse string to int as count for card {cardIdForCount}: {rawDeckData["count"].ToString()}");
                                }
                                else
                                {
                                    deckDataForCount.cardIdToCount[cardIdForCount] = countForCard;
                                }

                                _idToDeckData[deckIdForCount] = deckDataForCount;
                            }
                            break;
                        default:
                            Debug.LogError($"Unhandled column for deck data: {key}");
                            break;
                    }
                    
                }
            }
        }

        // --------------------------------------------------------------------------------------------
        private void BuildUnitDatas(object[] rawUnitsDatas)
        {
            _idToUnitData = new Dictionary<string, UnitData>(); 
            
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
                }

                // prefabPath
                if (rawUnitData.TryGetValue("prefab", out object prefabPathObj))
                {
                    unitData.prefabPath = prefabPathObj.ToString();
                }
                else
                {
                    Debug.LogError($"unit data index {i} is missing a prefab");
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
                    }
                }
                else
                {
                    Debug.LogError($"unit data index {i} is missing a value for health");
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
                    }
                }
                else
                {
                    Debug.LogError($"unit data index {i} is missing a value for move_speed");
                }

                _idToUnitData.Add(unitData.id, unitData);
            }
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
}