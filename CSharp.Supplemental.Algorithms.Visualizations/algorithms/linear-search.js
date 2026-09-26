/* Linear Search - step generator for the shared visualization player.
   Mirrors the C# CSharp.Supplemental.Algorithms.Search project's LinearSearch:
   scan from the front, one element at a time, until the target is found. */
window.generateSteps = function () {
  const SIZE = 16;

  // Same "shuffle a perfect range" approach as the C# DataGenerator, sized
  // for what's actually readable as a bar chart rather than for timing.
  const array = Array.from({ length: SIZE }, (_, i) => i);
  for (let i = array.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1));
    [array[i], array[j]] = [array[j], array[i]];
  }

  // A target near, but not at, the end - shows a meaningful scan before
  // finding it, without picking the single worst or single best case.
  const targetIndex = array.length - 3;
  const target = array[targetIndex];

  const steps = [];
  for (let i = 0; i < array.length; i++) {
    steps.push({ type: 'compare', indices: [i, i] });
    if (array[i] === target) {
      steps.push({ type: 'found', index: i });
      break;
    }
  }

  return {
    title: 'Linear Search - O(n)',
    description: `Scanning for the value ${target}, one element at a time from the front, until it's found.`,
    array,
    steps
  };
};
