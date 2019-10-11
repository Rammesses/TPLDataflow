using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Shared;

namespace Sample_2
{
    public class SummarizerBlock : IPropagatorBlock<int, Tuple<int, int>>,
                                     IReceivableSourceBlock<Tuple<int, int>>
    {
        private ConcurrentDictionary<int, int> counts = new ConcurrentDictionary<int, int>();

        private readonly ITargetBlock<int> target;
        private readonly IReceivableSourceBlock<Tuple<int, int>> source;

        public SummarizerBlock()
        {
            var outputBufferBlock = new BufferBlock<Tuple<int, int>>();

            // The inputActionBlock (target) receives data and adds them to the summary.
            var inputActionBlock = new ActionBlock<int>(item =>
            {
                UpdateSummary(item);
            });


            // When the inputActionBlock is set to the completed state, propagate out the
            // summary data and set the outputBufferBlock (source) to the completed state.
            inputActionBlock.Completion.ContinueWith(delegate
            {
                var years = counts.Keys.ToArray();
                Array.Sort(years);

                foreach (var year in years)
                {
                    outputBufferBlock.Post(new Tuple<int, int>(year, counts[year]));
                }

                outputBufferBlock.Complete();
            });

            // wire our implementation blocks up
            target = inputActionBlock;
            source = outputBufferBlock;
        }

        public void UpdateSummary(int input)
        {
            var year = input;
            int lastCountForYear = 0;
            if (counts.ContainsKey(year))
            {
                lastCountForYear = counts[year];
            }

            var newCount = lastCountForYear + 1;

            counts.AddOrUpdate(year, newCount, (a, b) => newCount);
        }

        #region IReceivableSourceBlock<TOutput> members

        // Attempts to synchronously receive an item from the source.
        public bool TryReceive(Predicate<Tuple<int, int>> filter, out Tuple<int, int> item)
        {
            return source.TryReceive(filter, out item);
        }

        // Attempts to remove all available elements from the source into a new 
        // array that is returned.
        public bool TryReceiveAll(out IList<Tuple<int, int>> items)
        {
            return source.TryReceiveAll(out items);
        }

        #endregion

        #region ISourceBlock<TOutput> members

        // Links this dataflow block to the provided target.
        public IDisposable LinkTo(ITargetBlock<Tuple<int, int>> target, DataflowLinkOptions linkOptions)
        {
            return source.LinkTo(target, linkOptions);
        }

        // Called by a target to reserve a message previously offered by a source 
        // but not yet consumed by this target.
        bool ISourceBlock<Tuple<int, int>>.ReserveMessage(DataflowMessageHeader messageHeader,
           ITargetBlock<Tuple<int, int>> target)
        {
            return source.ReserveMessage(messageHeader, target);
        }

        // Called by a target to consume a previously offered message from a source.
        Tuple<int, int> ISourceBlock<Tuple<int, int>>.ConsumeMessage(DataflowMessageHeader messageHeader,
           ITargetBlock<Tuple<int, int>> target, out bool messageConsumed)
        {
            return source.ConsumeMessage(messageHeader,
               target, out messageConsumed);
        }

        // Called by a target to release a previously reserved message from a source.
        void ISourceBlock<Tuple<int, int>>.ReleaseReservation(DataflowMessageHeader messageHeader,
           ITargetBlock<Tuple<int, int>> target)
        {
            source.ReleaseReservation(messageHeader, target);
        }

        #endregion

        #region ITargetBlock<TInput> members

        // Asynchronously passes a message to the target block, giving the target the 
        // opportunity to consume the message.
        DataflowMessageStatus ITargetBlock<int>.OfferMessage(DataflowMessageHeader messageHeader,
           int messageValue, ISourceBlock<int> source, bool consumeToAccept)
        {
            return target.OfferMessage(messageHeader,
               messageValue, source, consumeToAccept);
        }

        #endregion

        #region IDataflowBlock members

        // Gets a Task that represents the completion of this dataflow block.
        public Task Completion { get { return source.Completion; } }

        // Signals to this target block that it should not accept any more messages, 
        // nor consume postponed messages. 
        public void Complete()
        {
            target.Complete();
        }

        public void Fault(Exception error)
        {
            target.Fault(error);
        }

        #endregion
    }
}
