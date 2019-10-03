using System;

namespace Sample_1
{
    internal struct Input
    {
        private static Random random = new Random(DateTime.Now.Millisecond);

        public Input(int index, string person) : this()
        {
            this.Index = index;
            this.Person = person;
            this.Delay = TimeSpan.FromMilliseconds(random.Next(2500));
        }

        public int Index { get; }
        public string Person { get; }
        public TimeSpan Delay { get; }
    }
}