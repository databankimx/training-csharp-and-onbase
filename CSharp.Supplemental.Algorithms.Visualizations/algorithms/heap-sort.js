/* Heap Sort - step generator for the shared visualization player.
   Mirrors the C# CSharp.Supplemental.Algorithms.Sort project's HeapSort: builds a max-heap
   from the array, then repeatedly swaps the root (the largest remaining value) into its
   final position and re-heapifies what's left. */
window.generateSteps = function () {
  const SIZE = 16;

  const array = Array.from({ length: SIZE }, (_, i) => i);
  for (let i = array.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1));
    [array[i], array[j]] = [array[j], array[i]];
  }

  const working = array.slice();
  const steps = [];

  function heapify(n, i) {
    let largest = i;
    const left = 2 * i + 1;
    const right = 2 * i + 2;

    if (left < n) {
      steps.push({ type: 'compare', indices: [left, largest] });
      if (working[left] > working[largest]) largest = left;
    }
    if (right < n) {
      steps.push({ type: 'compare', indices: [right, largest] });
      if (working[right] > working[largest]) largest = right;
    }

    if (largest !== i) {
      [working[i], working[largest]] = [working[largest], working[i]];
      steps.push({ type: 'swap', indices: [i, largest] });
      heapify(n, largest);
    }
  }

  const n = working.length;
  for (let i = Math.floor(n / 2) - 1; i >= 0; i--) {
    heapify(n, i);
  }

  const sortedSoFar = [];
  for (let i = n - 1; i > 0; i--) {
    [working[0], working[i]] = [working[i], working[0]];
    steps.push({ type: 'swap', indices: [0, i] });
    sortedSoFar.unshift(i);
    steps.push({ type: 'sorted', indices: sortedSoFar.slice() });
    heapify(i, 0);
  }
  sortedSoFar.unshift(0);
  steps.push({ type: 'sorted', indices: sortedSoFar.slice() });

  return {
    title: 'Heap Sort - O(n log n), guaranteed, in-place',
    description:
      'Builds a max-heap (the largest value always at the root), then repeatedly swaps the root into its final position at the end of the array and re-heapifies what remains.',
    array,
    steps
  };
};
