/* Counting Sort - step generator for the shared visualization player.
   Mirrors the C# CSharp.Supplemental.Algorithms.Sort project's CountingSort: counts
   occurrences of each value (no comparisons at all), then reconstructs the sorted array
   directly from those counts. The "compare" steps below are really just showing each
   original element being read during the counting pass - there's genuinely nothing being
   compared against anything else, which is the whole point of this algorithm. */
window.generateSteps = function () {
  const SIZE = 16;

  const array = Array.from({ length: SIZE }, (_, i) => i);
  for (let i = array.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1));
    [array[i], array[j]] = [array[j], array[i]];
  }

  const steps = [];
  const min = Math.min(...array);
  const max = Math.max(...array);
  const range = max - min + 1;
  const counts = new Array(range).fill(0);

  array.forEach((value, i) => {
    steps.push({ type: 'compare', indices: [i, i] }); // reading, not comparing to anything
    counts[value - min]++;
  });

  let index = 0;
  for (let v = 0; v < range; v++) {
    while (counts[v] > 0) {
      steps.push({ type: 'overwrite', index, value: v + min });
      index++;
      counts[v]--;
    }
  }
  steps.push({ type: 'sorted', indices: Array.from({ length: array.length }, (_, i) => i) });

  return {
    title: 'Counting Sort - O(n + k)',
    description:
      "Not comparison-based at all - counts how many times each value occurs, then reconstructs the sorted array directly from those counts. Only works when the value range is known and reasonably small relative to n.",
    array,
    steps
  };
};
