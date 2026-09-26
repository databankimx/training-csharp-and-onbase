/* Selection Sort - step generator for the shared visualization player.
   Mirrors the C# CSharp.Supplemental.Algorithms.Sort project's SelectionSort:
   repeatedly find the smallest remaining element and swap it into place. */
window.generateSteps = function () {
  const SIZE = 16;

  const array = Array.from({ length: SIZE }, (_, i) => i);
  for (let i = array.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1));
    [array[i], array[j]] = [array[j], array[i]];
  }

  const working = array.slice();
  const steps = [];
  const sortedSoFar = [];

  for (let i = 0; i < working.length; i++) {
    let minIndex = i;
    for (let j = i + 1; j < working.length; j++) {
      steps.push({ type: 'compare', indices: [j, minIndex] });
      if (working[j] < working[minIndex]) minIndex = j;
    }
    if (minIndex !== i) {
      [working[minIndex], working[i]] = [working[i], working[minIndex]];
      steps.push({ type: 'swap', indices: [minIndex, i] });
    }
    sortedSoFar.push(i);
    steps.push({ type: 'sorted', indices: sortedSoFar.slice() });
  }

  return {
    title: 'Selection Sort - O(n²)',
    description:
      'Each pass scans the entire unsorted remainder for the smallest value, then swaps it into place - always a full scan, regardless of how sorted the array already is.',
    array,
    steps
  };
};
