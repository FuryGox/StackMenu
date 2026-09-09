using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StackMenu
{
    public enum StackSortOption
    {
        Name,
        Value,
        Type
    }

    // Reorders/splits a stack's Parent/Child chain and remembers the last sort option+direction per stack
    // (keyed by a stable representative card) so repeating the same option toggles asc/desc.
    public static class StackSorter
    {
        private static readonly Dictionary<GameCard, (StackSortOption Option, bool Descending)> lastSort = new Dictionary<GameCard, (StackSortOption, bool)>();

        public static void Sort(GameCard anyCardInStack, StackSortOption option)
        {
            List<GameCard> cards = anyCardInStack.GetAllCardsInStack();
            if (cards.Count < 2)
            {
                return;
            }

            GameCard key = GetStackKey(cards);
            bool descending = lastSort.TryGetValue(key, out var previous) && previous.Option == option && !previous.Descending;
            lastSort[key] = (option, descending);

            IEnumerable<GameCard> ordered = option switch
            {
                StackSortOption.Name => cards.OrderBy(c => c.CardData != null ? c.CardData.Name : c.name),
                StackSortOption.Value => cards.OrderBy(c => c.CardData != null ? c.CardData.GetValue() : 0),
                StackSortOption.Type => cards.OrderBy(c => c.CardData != null ? c.CardData.MyCardType.ToString() : string.Empty),
                _ => cards,
            };
            if (descending)
            {
                ordered = ordered.Reverse();
            }

            Relink(ordered.ToList());
        }

        public static void SplitAll(GameCard anyCardInStack)
        {
            List<GameCard> cards = anyCardInStack.GetAllCardsInStack();
            lastSort.Remove(GetStackKey(cards));

            foreach (GameCard card in cards)
            {
                card.RemoveFromStack();
            }

            for (int i = 0; i < cards.Count; i++)
            {
                float angle = i * Mathf.PI * 2f / Mathf.Max(cards.Count, 1);
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * 1.2f;
                cards[i].SendToPosition(cards[i].transform.position + offset);
            }
        }

        public static void SplitAllToEachStack(GameCard anyCardInStack)
        {
            List<GameCard> cards = anyCardInStack.GetAllCardsInStack();
            if (cards == null || cards.Count == 0) return;

            Vector3 centerPosition = anyCardInStack.transform.position;

            lastSort.Remove(GetStackKey(cards));

            var groupedCards = cards.GroupBy(card => card.CardData.Id).ToList();
            foreach (GameCard card in cards)
            {
                card.RemoveFromStack();
            }

            int groupCount = groupedCards.Count;

            for (int i = 0; i < groupCount; i++)
            {
                var group = groupedCards[i].ToList();

                Vector3 groupOffset = Vector3.zero;
                if (groupCount > 1)
                {
                    float angle = i * Mathf.PI * 2f / groupCount;
                    groupOffset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * 1.5f;
                }

                Vector3 targetPosition = centerPosition + groupOffset;

                GameCard baseCard = group[0];
                baseCard.SendToPosition(targetPosition);

                for (int j = 1; j < group.Count; j++)
                {
                    group[j].SetParent(group[j - 1]);
                }
            }
        }

        private static void Relink(List<GameCard> orderedCards)
        {
            foreach (GameCard card in orderedCards)
            {
                card.RemoveFromStack();
            }

            for (int i = 1; i < orderedCards.Count; i++)
            {
                orderedCards[i].SetParent(orderedCards[i - 1]);
            }
        }

        // The chain's root changes identity after every re-sort, so key by whichever card has the lowest
        // instance ID instead - that identity stays stable across re-sorts as long as the card set is unchanged.
        private static GameCard GetStackKey(List<GameCard> cards)
        {
            return cards.OrderBy(c => c.GetInstanceID()).First();
        }
    }
}
