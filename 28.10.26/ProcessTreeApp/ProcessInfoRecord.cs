namespace ProcessTreeApp
{
    internal sealed class ProcessInfoRecord
    {
        public int ProcessId { get; set; }

        public int ParentProcessId { get; set; }

        public string Name { get; set; }

        public string ExecutablePath { get; set; }

        public uint ThreadCount { get; set; }

        public string CreationDate { get; set; }

        public override string ToString()
        {
            return $"{Name} ({ProcessId})";
        }
    }
}
