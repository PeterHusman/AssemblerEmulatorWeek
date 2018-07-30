using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace CAEmulator
{
    public class MemoryMap
    {
        private const ushort programSpaceStart = 0x8000;
        private ushort[] memory = new ushort[0x10000];

        public MemoryMap(ReadOnlySpan<byte> programData)
        {
            Span<ushort> memorySpan = memory.AsSpan();
            Span<ushort> programSpace = memorySpan.Slice(programSpaceStart);
            Span<byte> programSpaceAsBytes = MemoryMarshal.AsBytes(programSpace);
            programData.CopyTo(programSpaceAsBytes);
        }

        public Memory<ushort> GetMemoryMappedIO()
        {
            return memory.AsMemory().Slice(0, 128);
        }

        public ReadOnlySpan<byte> ProgramSpace => MemoryMarshal.AsBytes(memory.AsSpan().Slice(programSpaceStart));

        public Span<byte> GetInstruction(int instructionPointer)
        {
            return MemoryMarshal.AsBytes(memory.AsSpan().Slice(instructionPointer, 2));
        }

        public ref ushort this[int index]
        {
            get
            {
                return ref memory[index];
            }
        }

    }
}
