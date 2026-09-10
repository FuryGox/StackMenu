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
        public List<string> CompactedData { get; set; } = new List<string>();

        public void SetCompactedData(List<string> data)
        {
            CompactedData = data;
        }

        public void UnpackCard()
        {
            CompactedData.Clear();
        }
    }
}