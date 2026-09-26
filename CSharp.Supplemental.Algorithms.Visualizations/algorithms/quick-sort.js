/* Quick Sort - step generator for the shared visualization player.
   Mirrors the C# CSharp.Supplemental.Algorithms.Sort project's QuickSort: always picks the
   last element of the current partition as pivot, then partitions around it recursively. */
window.generateSteps = function () {
  const SIZE = 16;

  const array = Array.from({ length: SIZE }, (_, i) => i);
  for (let i = array.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1));
    [array[i], array[j]] = [array[j], array[i]];
  }

  const working = array.slice();
  const steps = [];
  const finalized = new Set();

  function partition(low, high) {
    const pivot = working[high];
    let i = low - 1;

    for (let j = low; j < high; j++) {
      steps.push({ type: 'compare', indices: [j, high] });
      if (working[j] <= pivot) {
        i++;
        if (i !== j) {
          [working[i], working[j]] = [working[j], working[i]];
          steps.push({ type: 'swap', indices: [i, j] });
        }
      }
    }

    if (i + 1 !== high) {
      [working[i + 1], working[high]] = [working[high], working[i + 1]];
      steps.push({ type: 'swap', indices: [i + 1, high] });
    }

    finalized.add(i + 1);
    steps.push({ type: 'sorted', indices: Array.from(finalized) });
    return i + 1;
  }

  function quickSort(low, high) {
    if (low < high) {
      const pivotIndex = partition(low, high);
      quickSort(low, pivotIndex - 1);
      quickSort(pivotIndex + 1, high);
    } else if (low === high) {
      finalized.add(low);
      steps.push({ type: 'sorted', indices: Array.from(finalized) });
    }
  }

  quickSort(0, working.length - 1);

  return {
    title: 'Quick Sort - O(n²) worst case, O(n log n) average',
    description:
      'Always picks the last element of the current partition as pivot, then partitions everything smaller to its left and larger to its right, recursively - a known worst-case trap on already-sorted input, but typically the fastest of the O(n log n)-average sorts on random data.',
    array,
    steps
  };
};
