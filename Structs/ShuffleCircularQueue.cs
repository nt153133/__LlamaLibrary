using System.Collections.Generic;
using System.Linq;
using LlamaLibrary.Extensions;

namespace LlamaLibrary.Structs
{
    public class ShuffleCircularQueue<T>
    {
        private readonly Queue<T> _queue;
        private readonly IEnumerable<T> _inputList;

        public ShuffleCircularQueue(IEnumerable<T> inputList)
        {
            _inputList = inputList;
            _queue = new Queue<T>(inputList.Count());
            RefillQueue();
        }

        public T Dequeue()
        {
            if (_queue.Count == 0)
            {
                RefillQueue();
            }

            return _queue.Dequeue();
        }

        private void RefillQueue()
        {
            foreach (var name in _inputList.Shuffle())
            {
                _queue.Enqueue(name);
            }
        }

        public bool Any => _inputList.Any();
    }
}