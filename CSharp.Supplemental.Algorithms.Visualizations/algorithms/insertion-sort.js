/* Insertion Sort - step generator for the shared visualization player.
   Mirrors the C# CSharp.Supplemental.Algorithms.Sort project's InsertionSort:
   builds the sorted portion one element at a time, shifting larger already-sorted
   elements over to make room rather than swapping pairwise. */
window.generateSteps = function () {
  const SIZE = 16;

  const array = Array.from({ length: SIZE }, (_, i) => i);
  for (let i = array.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1));
    [array[i], array[j]] = [array[j], array[i]];
  }

  const working = array.slice();
  const steps = [];

  for (let i = 1; i < working.length; i++) {
    const key = working[i];
    let j = i - 1;

    while (j >= 0) {
      steps.push({ type: 'compare', indices: [j, j + 1] });
      if (working[j] <= key) break;
      working[j + 1] = working[j];
      steps.push({ type: 'overwrite', index: j + 1, value: working[j] });
      j--;
    }

    working[j + 1] = key;
    steps.push({ type: 'overwrite', index: j + 1, value: key });

    const sortedSoFar = [];
    for (let k = 0; k <= i; k++) sortedSoFar.push(k);
    steps.push({ type: 'sorted', indices: sortedSoFar });
  }

  return {
    title: 'Insertion Sort - O(n²), O(n) best case',
    description:
      'Grows a sorted region from the front, shifting larger already-sorted elements over one at a time to make room for each new value - adaptive on nearly-sorted input.',
    array,
    steps
  };
};
