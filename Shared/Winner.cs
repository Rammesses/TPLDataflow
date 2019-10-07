using System;

namespace Shared
{
    public class Winner
    {
        private static Random random = new Random(DateTime.Now.Millisecond);

        public Winner(int index, string person)
        {
            this.Index = index;
            this.Person = person;
            this.Delay = TimeSpan.FromMilliseconds(random.Next(5000));
        }

        public int Index { get; }
        public string Person { get; }
        public TimeSpan Delay { get; }
    }
}