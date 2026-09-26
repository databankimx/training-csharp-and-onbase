/* Binary Search - step generator for the shared visualization player.
   Mirrors the C# CSharp.Supplemental.Algorithms.Search project's BinarySearch:
   repeatedly check the midpoint of the still-active range, discarding half each time. */
window.generateSteps = function () {
  const SIZE = 16;

  const array = Array.from({ length: SIZE }, (_, i) => i);
  for (let i = array.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1));
    [array[i], array[j]] = [array[j], array[i]];
  }
  array.sort((a, b) => a - b); // Binary search requires a sorted array

  const target = array[Math.floor(array.length * 0.7)];

  const steps = [];
  let low = 0;
  let high = array.length - 1;

  while (low <= high) {
    const activeIndices = [];
    for (let i = low; i <= high; i++) activeIndices.push(i);
    steps.push({ type: 'activeRange', indices: activeIndices });

    const mid = low + Math.floor((high - low) / 2);
    steps.push({ type: 'compare', indices: [mid, mid] });

    if (array[mid] === target) {
      steps.push({ type: 'found', index: mid });
      break;
    } else if (array[mid] < target) {
      low = mid + 1;
    } else {
      high = mid - 1;
    }
  }

  return {
    title: 'Binary Search - O(log n)',
    description: `Searching a sorted array for ${target} by repeatedly checking the midpoint of what's still in play, discarding half the remaining range each time.`,
    array,
    steps
  };
};
