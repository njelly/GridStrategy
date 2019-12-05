using System;
using System.Collections.Generic;
using Tofunaut.Core;
using Tofunaut.GridStrategy.Game;
using UnityEngine;

namespace Tofunaut.GridStrategy
{
    public class Config
    {
        private Dictionary<string, CardData> _idToCardData;

        public Config(string serializedData)
        {
            _idToCardData = new Dictionary<string, CardData>();

            Dictionary<string, object[]> sheetToData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object[]>>(serializedData);
            if (sheetToData.TryGetValue("Cards", out object[] cardsData))
            {
                for(int i = 0; i < cardsData.Length; i++)
                {
                    Dictionary<string, object> cardData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(cardsData[i].ToString());
                    foreach(string key in cardData.Keys)
                    {
                        Debug.Log($"{key}: {cardData[key].ToString()}");
                    }
                }
            }
            else
            {
                throw new Exception("the string Cards was not present in the config data");
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