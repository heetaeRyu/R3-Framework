using System.Threading;
using System.Collections.Generic;
using System.Text;

namespace Netmarble.Core
{
	public class ConcurrentQueue<T>
	{
		// A queue that is protected by Monitor.
		private readonly Queue<T> _inputQueue = new Queue<T>();

		/// <summary>
		/// 락 안 걸고 카운트 가져오기
		/// </summary>
		/// <value>The count unsafe.</value>
		public int CountUnsafe => _inputQueue.Count;

		public int Count
		{
			get
			{
				int count;
				Monitor.Enter(_inputQueue);
				try
				{
					// When the lock is obtained, add an element.
					count = _inputQueue.Count;
				}
				finally
				{
					// Ensure that the lock is released.
					Monitor.Exit(_inputQueue);
				}

				return count;
			}
		}

		// Lock the queue and add an element.
		public void Enqueue(T qValue)
		{
			// Request the lock, and block until it is obtained.
			Monitor.Enter(_inputQueue);
			try
			{
				// When the lock is obtained, add an element.
				_inputQueue.Enqueue(qValue);
			}
			finally
			{
				// Ensure that the lock is released.
				Monitor.Exit(_inputQueue);
			}
		}

		// Try to add an element to the queue: Add the element to the queue 
		// only if the lock is immediately available.
		public bool TryEnqueue(T qValue)
		{
			// Request the lock.
			if (Monitor.TryEnter(_inputQueue))
			{
				try
				{
					_inputQueue.Enqueue(qValue);
				}
				finally
				{
					// Ensure that the lock is released.
					Monitor.Exit(_inputQueue);
				}

				return true;
			}
			else
			{
				return false;
			}
		}

		// Try to add an element to the queue: Add the element to the queue 
		// only if the lock becomes available during the specified time
		// interval.
		public bool TryEnqueue(T qValue, int waitTime)
		{
			// Request the lock.
			if (Monitor.TryEnter(_inputQueue, waitTime))
			{
				try
				{
					_inputQueue.Enqueue(qValue);
				}
				finally
				{
					// Ensure that the lock is released.
					Monitor.Exit(_inputQueue);
				}

				return true;
			}
			else
			{
				return false;
			}
		}

		// Lock the queue and dequeue an element.
		public T Dequeue()
		{
			T retval;

			// Request the lock, and block until it is obtained.
			Monitor.Enter(_inputQueue);
			try
			{
				// When the lock is obtained, dequeue an element.
				retval = _inputQueue.Dequeue();
			}
			finally
			{
				// Ensure that the lock is released.
				Monitor.Exit(_inputQueue);
			}

			return retval;
		}

		// Try lock the queue and dequeue an element.
		public bool TryDequeue(out T result)
		{
			// Request the lock, and block until it is obtained.
			if (Monitor.TryEnter(_inputQueue))
			{
				try
				{
					if (_inputQueue.Count > 0)
					{
						// When the lock is obtained, dequeue an element.
						result = _inputQueue.Dequeue();
						return true;
					}
				}
				finally
				{
					// Ensure that the lock is released.
					Monitor.Exit(_inputQueue);
				}
			}

			result = default(T);
			return false;
		}

		//큐 날리기
		public void Clear()
		{
			Monitor.Enter(_inputQueue);
			try
			{
				_inputQueue.Clear();
			}
			finally
			{
				// Ensure that the lock is released.
				Monitor.Exit(_inputQueue);
			}
		}

		// Delete all elements that equal the given object.
		public int Remove(T qValue)
		{
			int removedCt = 0;

			// Wait until the lock is available and lock the queue.
			Monitor.Enter(_inputQueue);
			try
			{
				int counter = _inputQueue.Count;
				while (counter > 0)
					// Check each element.
				{
					T elem = _inputQueue.Dequeue();
					if (!elem.Equals(qValue))
					{
						_inputQueue.Enqueue(elem);
					}
					else
					{
						// Keep a count of items removed.
						removedCt += 1;
					}

					counter = counter - 1;
				}
			}
			finally
			{
				// Ensure that the lock is released.
				Monitor.Exit(_inputQueue);
			}

			return removedCt;
		}

		// Print all queue elements.
		public string PrintAllElements()
		{
			StringBuilder output = new StringBuilder();

			// Lock the queue.
			Monitor.Enter(_inputQueue);
			try
			{
				foreach (T elem in _inputQueue)
				{
					// Print the next element.
					output.AppendLine(elem.ToString());
				}
			}
			finally
			{
				// Ensure that the lock is released.
				Monitor.Exit(_inputQueue);
			}

			return output.ToString();
		}

		public Queue<T> GetQueueUnsafe()
		{
			return _inputQueue;
		}
	}
}