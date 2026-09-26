/* Shell Sort - step generator for the shared visualization player.
   Mirrors the C# CSharp.Supplemental.Algorithms.Sort project's ShellSort: a generalized
   Insertion Sort comparing elements a shrinking "gap" apart (powers of two here) before a
   final gap-1 pass. Watch how far-apart elements jump into place in large strides early on,
   rather than one small shift at a time the way plain Insertion Sort does. */
window.generateSteps = function () {
  const SIZE = 16;

  const array = Array.from({ length: SIZE }, (_, i) => i);
  for (let i = array.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1));
    [array[i], array[j]] = [array[j], array[i]];
  }

  const working = array.slice();
  const steps = [];

  const n = working.length;
  let k = Math.floor(Math.log2(n));
  let interval = Math.floor(Math.pow(2, k - 1));

  while (interval > 0) {
    for (let i = interval; i < n; i++) {
      const temp = working[i];
      let j = i;

      while (j >= interval) {
        steps.push({ type: 'compare', indices: [j - interval, j] });
        if (working[j - interval] <= temp) break;
        working[j] = working[j - interval];
        steps.push({ type: 'overwrite', index: j, value: working[j] });
        j -= interval;
      }

      working[j] = temp;
      steps.push({ type: 'overwrite', index: j, value: temp });
    }

    k--;
    interval = Math.floor(Math.pow(2, k - 1));
  }

  steps.push({ type: 'sorted', indices: Array.from({ length: n }, (_, i) => i) });

  return {
    title: 'Shell Sort - O(n²) worst case, faster in practice',
    description:
      'A generalized Insertion Sort: compares elements a shrinking gap apart (here, powers of two) before a final adjacent-elements pass - out-of-order elements can jump into place in large strides instead of one small shift at a time.',
    array,
    steps
  };
};
