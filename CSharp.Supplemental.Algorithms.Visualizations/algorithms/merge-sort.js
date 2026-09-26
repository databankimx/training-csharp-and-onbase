/* Merge Sort - step generator for the shared visualization player.
   Mirrors the C# CSharp.Supplemental.Algorithms.Sort project's MergeSort: recursively splits
   the array in half, sorts each half, then merges them back together. The recursion here
   works on the same underlying positions the whole time (rather than genuinely separate
   arrays, the way the C# version's Take()/Skip() does) so the visualization can show each
   merge writing its result back into the original index range it came from. */
window.generateSteps = function () {
  const SIZE = 16;

  const array = Array.from({ length: SIZE }, (_, i) => i);
  for (let i = array.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1));
    [array[i], array[j]] = [array[j], array[i]];
  }

  const working = array.slice();
  const steps = [];

  function merge(lo, mid, hi) {
    const left = working.slice(lo, mid + 1);
    const right = working.slice(mid + 1, hi + 1);
    let i = 0, j = 0, k = lo;

    while (i < left.length && j < right.length) {
      steps.push({ type: 'compare', indices: [lo + i, mid + 1 + j] });
      if (left[i] <= right[j]) {
        working[k] = left[i++];
      } else {
        working[k] = right[j++];
      }
      steps.push({ type: 'overwrite', index: k, value: working[k] });
      k++;
    }
    while (i < left.length) {
      working[k] = left[i++];
      steps.push({ type: 'overwrite', index: k, value: working[k] });
      k++;
    }
    while (j < right.length) {
      working[k] = right[j++];
      steps.push({ type: 'overwrite', index: k, value: working[k] });
      k++;
    }
  }

  function mergeSort(lo, hi) {
    if (lo >= hi) return;
    const mid = lo + Math.floor((hi - lo) / 2);
    mergeSort(lo, mid);
    mergeSort(mid + 1, hi);
    merge(lo, mid, hi);
  }

  mergeSort(0, working.length - 1);
  steps.push({ type: 'sorted', indices: Array.from({ length: working.length }, (_, i) => i) });

  return {
    title: 'Merge Sort - O(n log n), guaranteed',
    description:
      'Recursively splits the array in half, sorts each half independently, then merges the two sorted halves back together - watch each merge write its result back into the exact index range it came from.',
    array,
    steps
  };
};
