using System.Collections.Generic;
using UnityEngine;

namespace Abu.Tools
{
    public static class Utility
    {
        public static int GetRandomWeightedIndex(IReadOnlyList<int> weights)
        {
            if(weights == null || weights.Count == 0) return -1;
 
            int total = 0;
            
            for(int i = 0; i < weights.Count; i++)
                if(weights[i] >= 0) total += weights[i];
            
            float r = Random.value * total;
            float s = 0f;
 
            for(int i = 0; i < weights.Count; i++)
            {
                if (weights[i] <= 0f) continue;
     
                s += weights[i];
                if (s >= r) return i;
            }
 
            return -1;
        }
    }
}