/* Sieve of Eratosthenes - grid visualization.
   Mirrors the C# CSharp.Supplemental.Algorithms.ReducingComplexity project's Best() method:
   assume everything is prime, then for each number found still unmarked, cross off every
   multiple of it starting from its square (anything smaller was already crossed off by a
   smaller prime).

   STEP FORMAT (specific to this grid, not the shared bar-chart step vocabulary):
     { type: 'current',   value: n }  // now considering n as a potential prime
     { type: 'prime',     value: n }  // n confirmed prime
     { type: 'composite', value: m }  // m crossed off as a multiple of the current prime
*/

(function () {
  const MAX = 100;

  const els = {
    stage: document.getElementById('sieve-stage'),
    statStep: document.getElementById('stat-step'),
    statOps: document.getElementById('stat-ops'),
    btnRestart: document.getElementById('btn-restart'),
    btnStepBack: document.getElementById('btn-step-back'),
    btnPlay: document.getElementById('btn-play'),
    btnStepForward: document.getElementById('btn-step-forward'),
    speed: document.getElementById('speed'),
    scrub: document.getElementById('scrub')
  };

  let steps = [];
  let currentStep = -1;
  let playing = false;
  let playTimer = null;

  function generateSteps() {
    const result = [];
    const isComposite = new Array(MAX + 1).fill(false);

    for (let n = 2; n <= MAX; n++) {
      if (isComposite[n]) continue;
      result.push({ type: 'current', value: n });
      result.push({ type: 'prime', value: n });
      for (let multiple = n * n; multiple <= MAX; multiple += n) {
        isComposite[multiple] = true;
        result.push({ type: 'composite', value: multiple });
      }
    }
    return result;
  }

  function renderGrid() {
    let html = '';
    for (let n = 1; n <= MAX; n++) {
      html += `<div class="cell" data-value="${n}">${n}</div>`;
    }
    els.stage.innerHTML = html;
  }

  function cellFor(value) {
    return els.stage.querySelector(`[data-value="${value}"]`);
  }

  // Recompute every cell's state at a given step by replaying from the start - same approach
  // player.js uses for array state, for the same reason (small dataset, simple and robust).
  function stateAtStep(index) {
    const primes = new Set();
    const composites = new Set();
    let current = null;

    for (let i = 0; i <= index; i++) {
      const step = steps[i];
      if (step.type === 'current') current = step.value;
      else if (step.type === 'prime') primes.add(step.value);
      else if (step.type === 'composite') composites.add(step.value);
    }

    return { primes, composites, current: index >= 0 ? current : null };
  }

  function applyState(index) {
    const { primes, composites, current } = stateAtStep(index);

    for (let n = 1; n <= MAX; n++) {
      const cell = cellFor(n);
      cell.classList.remove('state-current', 'state-marking', 'state-composite', 'state-prime');
      if (n === 1) continue; // 1 is neither prime nor composite - left unmarked throughout
      if (primes.has(n)) cell.classList.add('state-prime');
      else if (composites.has(n)) cell.classList.add('state-composite');
    }

    if (index >= 0 && current !== null) {
      const step = steps[index];
      if (step.type === 'composite') {
        const c = cellFor(step.value);
        if (c) c.classList.add('state-marking');
      }
      const currentCell = cellFor(current);
      if (currentCell && !currentCell.classList.contains('state-composite')) {
        currentCell.classList.add('state-current');
      }
    }
  }

  function operationCountAtStep(index) {
    // Matches the C# Best() method's own counting: one operation per number visited (whether
    // prime or already-composite), same as its outer "for (int n ...) count++" loop.
    let count = 0;
    for (let i = 0; i <= index; i++) {
      if (steps[i].type === 'current') count++;
    }
    return count;
  }

  function renderAtStep(index) {
    applyState(index);
    els.statStep.textContent = `${index + 1} / ${steps.length}`;
    els.statOps.textContent = operationCountAtStep(index).toLocaleString();
    els.scrub.value = index + 1;
  }

  function goToStep(index) {
    currentStep = Math.max(-1, Math.min(index, steps.length - 1));
    renderAtStep(currentStep);
    if (currentStep >= steps.length - 1) pause();
  }

  function stepForward() {
    goToStep(currentStep + 1);
  }

  function stepBack() {
    goToStep(currentStep - 1);
  }

  function restart() {
    pause();
    goToStep(-1);
  }

  function play() {
    if (currentStep >= steps.length - 1) goToStep(-1);
    playing = true;
    els.btnPlay.innerHTML = '&#10074;&#10074;';
    scheduleNext();
  }

  function pause() {
    playing = false;
    els.btnPlay.innerHTML = '&#9654;';
    if (playTimer) clearTimeout(playTimer);
  }

  function scheduleNext() {
    if (!playing) return;
    const speedValue = Number(els.speed.value);
    const delayMs = 420 - speedValue * 4; // faster default pace than the bar player - there
                                            // are many more steps here (up to ~130 for MAX=100)
    playTimer = setTimeout(() => {
      if (!playing) return;
      if (currentStep >= steps.length - 1) {
        pause();
        return;
      }
      stepForward();
      scheduleNext();
    }, Math.max(delayMs, 10));
  }

  function wireControls() {
    els.btnRestart.addEventListener('click', restart);
    els.btnStepBack.addEventListener('click', () => {
      pause();
      stepBack();
    });
    els.btnStepForward.addEventListener('click', () => {
      pause();
      stepForward();
    });
    els.btnPlay.addEventListener('click', () => (playing ? pause() : play()));
    els.scrub.addEventListener('input', () => {
      pause();
      goToStep(Number(els.scrub.value) - 1);
    });
  }

  function init() {
    renderGrid();
    steps = generateSteps();
    els.scrub.max = steps.length;
    wireControls();
    goToStep(-1);
  }

  init();
})();
