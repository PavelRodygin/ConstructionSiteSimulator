using System.Collections.Generic;
using System.Linq;
using CodeBase.Core.Systems.PopupHub.Popups;

namespace CodeBase.Systems.PopupHub
{
    public class PopupsPriorityQueue
    {
        //Represents a collection of key/value pairs that are sorted on the key
        private readonly SortedDictionary<int, Queue<BasePopup>> _dictionary = new();
        private int? _minPriority = null;
        
        /// <summary>
        /// Adds a popup to the priority queue, organizing it by its priority level.
        /// If the given priority does not exist, a new queue is created for it.
        /// Updates the minimum priority to ensure efficient retrieval of the highest-priority popup.
        /// </summary>
        /// <param name="popup">The popup to enqueue, with an associated priority.</param>
        public void Enqueue(BasePopup popup)
        {
            // 1. Determine Priority:
            // Extracts the priority from the popup object.
            var priority = (int)popup.Priority;

            // 2. Add to the Queue:
            // If no queue exists for the given priority, it creates a new one (Queue<BasePopup>).
            // Adds the popup to the corresponding queue.
            if (!_dictionary.ContainsKey(priority))
                _dictionary[priority] = new Queue<BasePopup>();

            _dictionary[priority].Enqueue(popup);
            
            if (_minPriority == null || priority < _minPriority)
                _minPriority = priority;
        }

        /// <summary>
        /// Attempts to dequeue the  lowest priority popup from the queue.
        /// If the queue is empty, the method returns false and sets the output parameter to null.
        /// Updates the priority tracking and removes empty queues as necessary.
        /// </summary>
        /// <param name="popup">The output parameter that will contain the dequeued popup, or null if no popups are available.</param>
        /// <returns>
        /// True if a popup was successfully dequeued; false if the queue is empty.
        /// </returns>
        public bool TryDequeue(out BasePopup popup)
        {
            //  1. Check for Available Items:
            // If _minPriority is null, there are no popups in the queue. Returns false and sets popup to null.
            if (_minPriority == null)
            {
                popup = null;
                return false;
            }

            //  2. Dequeue the PopupHub:
            // Retrieves the queue associated with the current _minPriority.
            //     Dequeues the first popup in that queue.
            var queue = _dictionary[_minPriority.Value];
            popup = queue.Dequeue();

            if (queue.Count == 0)
            {
                _dictionary.Remove(_minPriority.Value);
                _minPriority = _dictionary.Count > 0 ? _dictionary.Keys.Min() : null;
            }

            return true;
        }
    }
}