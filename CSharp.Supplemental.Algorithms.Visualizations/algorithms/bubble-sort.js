/* Bubble Sort - step generator for the shared visualization player.
   Mirrors the C# CSharp.Supplemental.Algorithms.Sort project's BubbleSort:
   repeated passes swapping adjacent out-of-order elements, with the same
   early-exit-on-no-swaps optimization, and each pass "bubbling" the
   largest remaining element into its final sorted position. */
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

  for (let i = working.length - 1; i >= 0; i--) {
    let swapped = false;
    for (let j = 0; j < i; j++) {
      steps.push({ type: 'compare', indices: [j, j + 1] });
      if (working[j] > working[j + 1]) {
        [working[j], working[j + 1]] = [working[j + 1], working[j]];
        steps.push({ type: 'swap', indices: [j, j + 1] });
        swapped = true;
      }
    }
    sortedSoFar.push(i);
    steps.push({ type: 'sorted', indices: sortedSoFar.slice() });
    if (!swapped) {
      // Zero swaps in this pass means everything from i-1 down to 0 is already in order too -
      // mark the whole remaining prefix as sorted, not just the position that triggered this.
      for (let k = i - 1; k >= 0; k--) sortedSoFar.push(k);
      steps.push({ type: 'sorted', indices: sortedSoFar.slice() });
      break;
    }
  }

  return {
    title: 'Bubble Sort - O(n²)',
    description:
      'Each pass compares adjacent elements and swaps them if out of order, "bubbling" the largest remaining value to its final position.',
    array,
    steps
  };
};
