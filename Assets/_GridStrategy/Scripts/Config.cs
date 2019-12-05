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

        // --------------------------------------------------------------------------------------------
        public Config(string serializedData)
        {

            Dictionary<string, object[]> sheetToData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object[]>>(serializedData);

            // Cards
            _idToCardData = new Dictionary<string, CardData>();
            if (sheetToData.TryGetValue("Cards", out object[] rawCardDatas))
            {
                BuildCardDatas(rawCardDatas);
            }
            else
            {
                throw new Exception("the string Cards was not present in the config data");
            }

            // Decks
            _idToDeckData = new Dictionary<string, DeckData>();
            if (sheetToData.TryGetValue("Decks", out object[] rawDeckDatas))
            {
                BuildDeckDatas(rawDeckDatas);
            }

            Debug.Log(_idToCardData.Keys.Count);
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
            for(int i = 0; i < rawDeckDatas.Length; i++)
            {
                Dictionary<string, object> rawDeckData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(rawDeckDatas[i].ToString());

                // decks are different because there are many "deck datas" with the same id. All of them should go in the same deck.


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
        public string displayName;
        public string prefabPath;
        public float health;
        public int moveSpeed;
    }
}