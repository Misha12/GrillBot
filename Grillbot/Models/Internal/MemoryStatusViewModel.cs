using System;

namespace Grillbot.Models.Internal
{
    public class MemoryStatusViewModel
    {
        public long TotalAllocatedBytes { get; set; }
        public long TotalManagedMemory { get; set; }
        public GCMemoryInfo MemoryInfo { get; set; }

        public MemoryStatusViewModel()
        {
            TotalAllocatedBytes = GC.GetTotalAllocatedBytes();
            TotalManagedMemory = GC.GetTotalMemory(false);
            MemoryInfo = GC.GetGCMemoryInfo();
        }
    }
}
