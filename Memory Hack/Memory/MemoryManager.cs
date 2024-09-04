using System;
using System.Collections.Generic;
using System.Linq;
using ExternalCheat.Memory.Pattern;

namespace ExternalCheat.Memory
{
    public class MemoryManager
    {
        private readonly IntPtr _processHandle;

        public MemoryManager(IntPtr handle)
        {
            _processHandle = handle;
        }

        public List<List<IntPtr>> PatternScan(MemoryPattern pattern)
        {
            var regions = QueryMemoryRegions();

            List<List<IntPtr>> addrs = new List<List<IntPtr>>();

            foreach (var memoryRegion in regions)
            {
                var addr = ScanForPatternInRegion(memoryRegion, pattern);

                if (addr.Count == 0)
                {
                    continue;
                }
                else
                {
                    addrs.Add(addr);
                }
            }

            return addrs;
        }

        public byte[] ReadMemory(IntPtr addr, int size)
        {
            var buff = new byte[size];
            return WinAPI.ReadProcessMemory(_processHandle, addr, buff, size, out _) ? buff : buff;
        }

        public int ReadInt32(IntPtr addr)
        {
            return BitConverter.ToInt32(ReadMemory(addr, 4), 0);
        }

        public float ReadFloat(IntPtr addr)
        {
            return BitConverter.ToSingle(ReadMemory(addr, 4), 0);
        }

        public long ReadInt64(IntPtr addr)
        {
            return BitConverter.ToInt64(ReadMemory(addr, 8), 0);
        }

        public string ReadString(IntPtr addr, int sl)
        {
            byte[] s0 = ReadMemory(addr, sl);
            string s1 = "";
            for (int i = 0; i < sl; i++)
            {
                s1 += (char)s0[i];
            }
            return s1;
        }

        public byte ReadByte(IntPtr addr)
        {
            return ReadMemory(addr, 1)[0];
        }

        public void WriteInt32(IntPtr addr, int value)
        {
            var b = BitConverter.GetBytes(value);
            WinAPI.WriteProcessMemory(_processHandle, addr, b, b.Length, out _);
        }

        public void WriteFloat(IntPtr addr, float value)
        {
            var b = BitConverter.GetBytes(value);
            WinAPI.WriteProcessMemory(_processHandle, addr, b, b.Length, out _);
        }

        public void WriteInt64(IntPtr addr, long value)
        {
            var b = BitConverter.GetBytes(value);
            WinAPI.WriteProcessMemory(_processHandle, addr, b, b.Length, out _);
        }

        public void WriteString(IntPtr addr, string value)
        {
            var b = new byte[value.Length];
            for (int i = 0; i < value.Length; i++)
            {
                b[i] = (byte)value[i];
            }
            WinAPI.WriteProcessMemory(_processHandle, addr, b, b.Length, out _);
        }

        public void WriteByte(IntPtr addr, byte value)
        {
            var b = new byte[] { value };
            WinAPI.WriteProcessMemory(_processHandle, addr, b, b.Length, out _);
        }

        public List<IntPtr> ScanForPatternInRegion(MemoryRegion region, MemoryPattern pattern)
        {
            List<IntPtr> addrs = new List<IntPtr>();

            var endAddr = (int)region.RegionSize - pattern.Size;
            var wholeMemory = ReadMemory(region.BaseAddress, (int)region.RegionSize);

            try
            {
                for (var addr = 0; addr < endAddr; addr++)
                {
                    var buff = new byte[pattern.Size];
                    Array.Copy(wholeMemory, addr, buff, 0, buff.Length);

                    var found = true;

                    for (var i = 0; i < pattern.Size; i++)
                    {
                        if (!pattern.PatternParts[i].Matches(buff[i]))
                        {
                            found = false;
                            break;
                        }
                    }

                    if (!found)
                    {
                        continue;
                    }

                    addrs.Add(region.BaseAddress + addr);
                }
            }
            catch { }

            return addrs;
        }

        /*public IntPtr ScanForPatternInRegion(MemoryRegion region, MemoryPattern pattern)
        {
            var endAddr = (int) region.RegionSize - pattern.Size;
            var wholeMemory = ReadMemory(region.BaseAddress, (int) region.RegionSize);

            for (var addr = 0; addr < endAddr; addr++)
            {
                var b = wholeMemory.Skip(addr).Take(pattern.Size).ToArray();

                if (!pattern.PatternParts.First().Matches(b.First()))
                {
                    continue;
                }

                if (!pattern.PatternParts.Last().Matches(b.Last()))
                {
                    continue;
                }

                var found = true;

                for (var i = 1; i < pattern.Size - 1; i++)
                {
                    if (!pattern.PatternParts[i].Matches(b[i]))
                    {
                        found = false;
                        break;
                    }
                }

                if (!found)
                {
                    continue;
                }

                return region.BaseAddress + addr;
            }

            return IntPtr.Zero;
        }*/

        public List<MemoryRegion> QueryMemoryRegions() {
            long curr = 0;
            var regions = new List<MemoryRegion>();

            while (true) {
                try {
                    var memDump = WinAPI.VirtualQueryEx(_processHandle, (IntPtr) curr, out var memInfo, 48);
                    
                    if (memDump == 0) break;

                    if ((memInfo.State & 0x1000) != 0 && (memInfo.Protect & 0x100) == 0)
                    {
                        regions.Add(new MemoryRegion
                        {
                            BaseAddress = memInfo.BaseAddress,
                            RegionSize = memInfo.RegionSize,
                            Protect = memInfo.Protect
                        });
                    }

                    curr = (long) memInfo.BaseAddress + (long) memInfo.RegionSize;
                } catch {
                    break;
                }
            }

            return regions;
        }
    }
}