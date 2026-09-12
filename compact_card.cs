using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace StackMenu
{
    public class CompactCard : CardData
    {
        [ExtraData("stackmenu_compacted_data")]
        public string CompactedDataString = "";

        [ExtraData("stackmenu_compacted_icon_id")]
        public string IconCardId = "";

        [ExtraData("stackmenu_compacted_value")]
        public int SavedValue = 0;

        [ExtraData("stackmenu_compacted_cities_value")]
        public int SavedCitiesValue = 0;

        public List<string> CompactedData
        {
            get
            {
                if (string.IsNullOrEmpty(CompactedDataString))
                {
                    return new List<string>();
                }
                try
                {
                    return JsonConvert.DeserializeObject<List<string>>(CompactedDataString) ?? new List<string>();
                }
                catch
                {
                    return new List<string>();
                }
            }
            set
            {
                CompactedDataString = value != null ? JsonConvert.SerializeObject(value) : "";
            }
        }

        public void SetCompactedData(List<string> data)
        {
            CompactedData = data;
        }

        public void UnpackCard()
        {
            CompactedData = new List<string>();
        }

        public void AddCompactedData(string data)
        {
            var list = CompactedData;
            list.Add(data);
            CompactedData = list;
        }

        public void UpdateVisuals()
        {
            if (SavedValue > 0)
            {
                Value = SavedValue;
            }
            if (SavedCitiesValue > 0)
            {
                CitiesValue = SavedCitiesValue;
            }

            if (!string.IsNullOrEmpty(IconCardId))
            {
                CardData? prefab = WorldManager.instance?.GetCardPrefab(IconCardId, showError: false);
                if (prefab != null && prefab.Icon != null)
                {
                    Icon = prefab.Icon;
                }
            }
            else if (CompactedData.Count > 0)
            {
                string firstId = CompactedData[0];
                CardData? prefab = WorldManager.instance?.GetCardPrefab(firstId, showError: false);
                if (prefab != null && prefab.Icon != null)
                {
                    IconCardId = firstId;
                    Icon = prefab.Icon;
                }
            }

            if (CompactedData.Count > 0)
            {
                descriptionOverride = string.Join(" + ", CompactedData);
            }

            if (MyGameCard != null)
            {
                MyGameCard.UpdateIcon();
                CardMenu.UpdateBadge(MyGameCard);
            }
        }

        protected override bool CanHaveCard(CardData otherCard)
        {
            if (otherCard.MyCardType == CardType.Fish || otherCard.MyCardType == CardType.Humans || otherCard.MyCardType == CardType.Mobs)
            {
                return false;
            }
            return true;
        }

        public new bool CanHaveCardOnTop(CardData otherCard, bool isPrefab = false)
        {
            if (otherCard.MyCardType == CardType.Fish || otherCard.MyCardType == CardType.Humans || otherCard.MyCardType == CardType.Mobs)
            {
                return false;
            }
            return true;
        }

        public override void UpdateCard()
        {
            base.UpdateCard();

            if ((SavedValue > 0 && Value != SavedValue) ||
                (SavedCitiesValue > 0 && CitiesValue != SavedCitiesValue) ||
                (!string.IsNullOrEmpty(IconCardId) && Icon == null))
            {
                UpdateVisuals();
            }

            var otherCompactCard = ChildrenMatchingPredicate(childCard => childCard.Id == "stackmenu_compactcard");
            if (otherCompactCard != null && otherCompactCard.Count > 0)
            {
                foreach (var data in otherCompactCard.Select(card => (card as CompactCard)))
                {
                    if (data != null)
                    {
                        foreach (var item in data.CompactedData)
                        {
                            AddCompactedData(item);
                        }
                        SavedValue += data.SavedValue > 0 ? data.SavedValue : data.Value;
                        SavedCitiesValue += data.SavedCitiesValue > 0 ? data.SavedCitiesValue : data.CitiesValue;
                        data.MyGameCard.DestroyCard();
                    }
                }
                UpdateVisuals();
            }

            var otherCards = ChildrenMatchingPredicate(childCard => childCard.Id != "stackmenu_compactcard");
            if (otherCards != null && otherCards.Count > 0)
            {
                foreach (var data in otherCards)
                {
                    AddCompactedData(data.Id);
                }
                SavedValue += otherCards.Sum(card => card.Value);
                SavedCitiesValue += otherCards.Sum(card => card.CitiesValue);

                foreach (var data in otherCards)
                {
                    data.MyGameCard.DestroyCard();
                }
            }
        }
    }
}