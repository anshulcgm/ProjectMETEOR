#pragma warning disable 162
#pragma warning disable 429

using UnityEngine;
/** Binary heap implementation.
* Binary heaps are really fast for ordering nodes in a way that
* makes it possible to get the node with the lowest F score.
* Also known as a priority queue.
*
* This has actually been rewritten as a 4-ary heap
* for performance, but it's the same principle.
*
* \see http://en.wikipedia.org/wiki/Binary_heap
* \see https://en.wikipedia.org/wiki/D-ary_heap
*/
public class PriorityTree{
		/** Number of items in the tree */
		public int numberOfItems;

		/** The tree will grow by at least this factor every time it is expanded */
		public float growthFactor = 2;

		/**
		 * Number of children of each node in the tree.
		 * Different values have been tested and 4 has been empirically found to perform the best.
		 * \see https://en.wikipedia.org/wiki/D-ary_heap
		 */
		const int D = 4;

		/** Sort nodes by G score if there is a tie when comparing the F score.
		 * Disabling this will improve pathfinding performance with around 2.5%
		 * but may break ties between paths that have the same length in a less
		 * desirable manner (only relevant for grid graphs).
		 */
		const bool SortGScores = true;

		/** Internal backing array for the heap */
		private Node[] heap;

		/** True if the heap does not contain any elements */
		public bool isEmpty {
			get {
				return numberOfItems <= 0;
			}
		}
		/** Rounds up v so that it has remainder 1 when divided by D.
		 * I.e it is of the form n*D + 1 where n is any non-negative integer.
		 */
		static int RoundUpToNextMultipleMod1 (int v) {
			// I have a feeling there is a nicer way to do this
			return v + (4 - ((v-1) % D)) % D;
		}

		/** Create a new heap with the specified initial capacity */
		public PriorityTree (int capacity) {
			// Make sure the size has remainder 1 when divided by D
			// This allows us to always guarantee that indices used in the Remove method
			// will never throw out of bounds exceptions
			capacity = RoundUpToNextMultipleMod1(capacity);

			heap = new Node[capacity];
			numberOfItems = 0;
		}

		/** Removes all elements from the heap */
		public void Clear () {
			numberOfItems = 0;
		}

		internal Node GetNode (int i) {
			return heap[i];
		}

		internal void SetF (int i, double f) {
			heap[i].fVal = f;
		}

		/** Expands to a larger backing array when the current one is too small */
		void Expand () {
			int newSize = System.Math.Max(heap.Length+4, (int)System.Math.Round(heap.Length*growthFactor));

			// Make sure the size has remainder 1 when divided by D
			// This allows us to always guarantee that indices used in the Remove method
			// will never throw out of bounds exceptions
			newSize = RoundUpToNextMultipleMod1(newSize);

			if (newSize > 1<<18) {
				throw new System.Exception("Binary Heap Size really large (2^18). A heap size this large is probably the cause of pathfinding running in an infinite loop. " +
					"\nRemove this check (in BinaryHeap.cs) if you are sure that it is not caused by a bug");
			}

			var newHeap = new Node[newSize];

			for (int i = 0; i < heap.Length; i++) {
				newHeap[i] = heap[i];
			}
			heap = newHeap;
		}

		/** Adds a node to the heap */
		public void Add (Node node)
        {
            //throw an exception if you're adding a null node.
			if (node == null) throw new System.ArgumentNullException("node");

            //expand the heap 
			if (numberOfItems == heap.Length) {
				Expand();
			}

            //add the node and bubble it up.
            heap[numberOfItems] = node;
            node.index = numberOfItems;         
            Bubble(node);
			numberOfItems++;
		}

        //bubbles a node 'n'
        public void Bubble(Node n)
        {
            if(n.index == -1)
            {
                return;
            }
            Bubble(n.index);
        }

        //bubbles a node at an index i.
        private void Bubble(int bubbleIndex)
        {
            Node node = heap[bubbleIndex];
            double nodeF = node.fVal;
            double nodeG = node.gVal;

            while (bubbleIndex != 0)
            {
                // Parent node of the bubble node
                int parentIndex = (bubbleIndex - 1) / D;

                if (nodeF < heap[parentIndex].fVal || (SortGScores && nodeF == heap[parentIndex].fVal && nodeG > heap[parentIndex].gVal))
                {
                    // Swap the bubble node and parent node
                    // (we don't really need to store the bubble node until we know the final index though
                    // so we do that after the loop instead)
                    heap[bubbleIndex] = heap[parentIndex];
                    heap[bubbleIndex].index = bubbleIndex;
                    bubbleIndex = parentIndex;
                }
                else
                {
                    break;
                }
            }
            
            heap[bubbleIndex] = node;
            heap[bubbleIndex].index = bubbleIndex;
        }

		/** Returns the node with the lowest F score from the heap */
		public Node Remove () {
			numberOfItems--;
			Node returnItem = heap[0];
			Node swapItem = heap[numberOfItems];
			double swapItemG = swapItem.gVal;

            int swapIndex = 0;
            int parent = 0;

			while (true)
            {
				parent = swapIndex;
				double swapF = swapItem.fVal;
                double swapG = swapItem.gVal;

                int childStart = parent * D + 1;
                for(int i = 0; i < D && (childStart + i) < numberOfItems; i++)
                {
                    if (heap[childStart + i].fVal < swapF || (heap[childStart + i].fVal == swapF && heap[childStart + i].gVal > swapG))
                    {
                        swapF = heap[childStart + i].fVal;
                        swapG = heap[childStart + i].gVal;
                        swapIndex = childStart + i;
                    }
                }

				// If the parent's children are smaller or equal, swap them
				// (actually we are just pretenting we swapped them, we hold the swapData
				// in local variable and only assign it once we know the final index)
				if (parent != swapIndex) {
					heap[parent] = heap[swapIndex];
                    heap[parent].index = parent;
                } else {
					break;
				}
			}

			// Assign element to the final position
			heap[swapIndex] = swapItem;
            heap[swapIndex].index = swapIndex;

            returnItem.index = -1;
            return returnItem;
		}

		public void Validate () {
			for (int i = 1; i < numberOfItems; i++) {
				int parentIndex = (i-1)/D;
				if (heap[parentIndex].fVal > heap[i].fVal) {
					Debug.Log("Invalid state at " + i + ":" +  parentIndex + " ( " + heap[parentIndex].fVal + " > " + heap[i].fVal + " ) ");
				}
			}
		}

		/** Rebuilds the heap by trickeling down all items.
		 * Usually called after the hTarget on a path has been changed */
		public void Rebuild () {
			for (int i = 2; i < numberOfItems; i++) {
				int bubbleIndex = i;
				var node = heap[i];
				double nodeF = node.fVal;
				while (bubbleIndex != 1) {
					int parentIndex = bubbleIndex / D;

					if (nodeF < heap[parentIndex].fVal) {
						heap[bubbleIndex] = heap[parentIndex];
						heap[parentIndex] = node;
                        heap[bubbleIndex].index = bubbleIndex;
                        heap[parentIndex].index = parentIndex;
                        bubbleIndex = parentIndex;
					} else {
						break;
					}
				}
			}
		}
	}

