/* Radix Sort - step generator for the shared visualization player.
   Mirrors the C# CSharp.Supplemental.Algorithms.Sort project's RadixSort: sorts by
   individual decimal digit, least significant first, using a stable counting pass for
   each digit position. */
window.generateSteps = function () {
  const SIZE = 16;

  const array = Array.from({ length: SIZE }, (_, i) => i);
  for (let i = array.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1));
    [array[i], array[j]] = [array[j], array[i]];
  }

  let working = array.slice();
  const steps = [];
  const max = Math.max(...working);

  for (let digitPlace = 1; Math.floor(max / digitPlace) > 0; digitPlace *= 10) {
    const counts = new Array(10).fill(0);

    working.forEach((value, i) => {
      steps.push({ type: 'compare', indices: [i, i] }); // reading this digit, not comparing
      counts[Math.floor(value / digitPlace) % 10]++;
    });

    for (let d = 1; d < 10; d++) counts[d] += counts[d - 1];

    const output = new Array(working.length);
    for (let i = working.length - 1; i >= 0; i--) {
      const digit = Math.floor(working[i] / digitPlace) % 10;
      counts[digit]--;
      output[counts[digit]] = working[i];
    }

    working = output;
    working.forEach((value, i) => steps.push({ type: 'overwrite', index: i, value }));
  }

  steps.push({ type: 'sorted', indices: Array.from({ length: working.length }, (_, i) => i) });

  return {
    title: 'Radix Sort - O(d · (n + k))',
    description:
      'Sorts by individual decimal digit, least significant first, using a stable counting pass for each digit position. For fixed-width integers, the digit count is a small constant, making this effectively O(n) in practice.',
    array,
    steps
  };
};
