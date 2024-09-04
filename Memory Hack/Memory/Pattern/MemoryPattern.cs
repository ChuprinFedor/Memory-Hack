using System;
using System.Collections.Generic;

namespace ExternalCheat.Memory.Pattern
{
    public class MemoryPattern
    {
        public readonly List<IMemoryPatternPart> PatternParts = new List<IMemoryPatternPart>();
        public int Size => PatternParts.Count;

        public MemoryPattern(string pattern, int type)
        {
            Parse(pattern, type);
        }

        private void Parse(string pattern, int type)
        {
            byte[] parts = null;
            
            if (type == 0)
            {
                parts = BitConverter.GetBytes(int.Parse(pattern));
            }
            else if (type == 1)
            {
                parts = BitConverter.GetBytes(float.Parse(pattern));
            }
            else if (type == 2)
            {
                parts = BitConverter.GetBytes(long.Parse(pattern));
            }
            else if (type == 3)
            {
                parts = new byte[pattern.Length];
                for (int i = 0; i < pattern.Length; i++)
                {
                    parts[i] = (byte)pattern[i];
                }
            }
            else if (type == 4)
            {
                parts = new byte[1];
                parts[0] = (byte)Convert.ToInt32(pattern);
            }
            
            PatternParts.Clear();

            foreach (var part in parts)
            {
                PatternParts.Add(new MatchMemoryPatternPart(part));
            }
        }
    }
}