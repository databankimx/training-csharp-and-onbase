/* ================================================================
   Algorithm Visualization - shared player engine

   Loads one algorithm's step-generator script (based on the ?algo=
   query parameter), then plays back the steps it produces against
   a bar-chart rendering of the array. The algorithm scripts know
   nothing about rendering; this file knows nothing about any
   specific algorithm. One engine, any number of algorithms.

   STEP FORMAT (produced by algorithms/<name>.js's generateSteps()):
     { type: 'compare',      indices: [i, j] }
     { type: 'swap',         indices: [i, j] }   // also swaps array values
     { type: 'overwrite',    index: i, value: v } // sets array[i] = v directly
     { type: 'activeRange',  indices: [i, ...] }  // still-in-play range (e.g. binary search)
     { type: 'sorted',       indices: [i, ...] }  // permanently marks as finalized
     { type: 'found',        index: i }
     { type: 'notFound' }

   Every step except 'sorted' counts as one operation toward the
   running operation counter shown in the header - matching the
   same "count what's actually significant" idea the C# demos'
   EfficiencyReport is built on.
   ================================================================ */

(function () {
  const els = {
    title: document.getElementById('algo-title'),
    description: document.getElementById('algo-description'),
    statStep: document.getElementById('stat-step'),
    statOps: document.getElementById('stat-ops'),
    stage: document.getElementById('stage'),
    legend: document.getElementById('legend'),
    btnRestart: document.getElementById('btn-restart'),
    btnStepBack: document.getElementById('btn-step-back'),
    btnPlay: document.getElementById('btn-play'),
    btnStepForward: document.getElementById('btn-step-forward'),
    speed: document.getElementById('speed'),
    scrub: document.getElementById('scrub')
  };

  let originalArray = [];
  let steps = [];
  let currentStep = -1; // -1 means "before the first step"
  let playing = false;
  let playTimer = null;
  let sortedIndices = new Set();

  const LEGEND_ITEMS = [
    { state: 'idle', label: 'Untouched', color: 'var(--bar-idle)' },
    { state: 'compare', label: 'Comparing', color: 'var(--bar-compare)' },
    { state: 'swap', label: 'Swapping', color: 'var(--bar-swap)' },
    { state: 'active-range', label: 'Active range', color: 'var(--bar-active-range)' },
    { state: 'sorted', label: 'Sorted / final', color: 'var(--bar-sorted)' },
    { state: 'found', label: 'Found', color: 'var(--bar-found)' }
  ];

  function renderLegend() {
    els.legend.innerHTML = LEGEND_ITEMS.map(
      (item) =>
        `<div class="legend-item"><span class="legend-swatch" style="background:${item.color}"></span>${item.label}</div>`
    ).join('');
  }

  // Recompute the array's values at a given step index by replaying every
  // swap/overwrite from the start. Simple and robust rather than fragile
  // incremental undo - these arrays are small enough that replaying is fast.
  function arrayStateAtStep(index) {
    const arr = originalArray.slice();
    for (let i = 0; i <= index; i++) {
      const step = steps[i];
      if (step.type === 'swap') {
        const [a, b] = step.indices;
        [arr[a], arr[b]] = [arr[b], arr[a]];
      } else if (step.type === 'overwrite') {
        arr[step.index] = step.value;
      }
    }
    return arr;
  }

  function sortedIndicesAtStep(index) {
    const result = new Set();
    for (let i = 0; i <= index; i++) {
      const step = steps[i];
      if (step.type === 'sorted') {
        step.indices.forEach((idx) => result.add(idx));
      }
    }
    return result;
  }

  function operationCountAtStep(index) {
    let count = 0;
    for (let i = 0; i <= index; i++) {
      if (steps[i].type !== 'sorted') count++;
    }
    return count;
  }

  function renderBars(values) {
    const max = Math.max(...values, 1);
    els.stage.innerHTML = values
      .map((v, i) => {
        const heightPct = Math.max((v / max) * 100, 4);
        return `<div class="bar" data-index="${i}" style="height:${heightPct}%">
                  <span class="bar-value">${v}</span>
                </div>`;
      })
      .join('');
  }

  function applyStateClasses(index) {
    const bars = els.stage.querySelectorAll('.bar');
    bars.forEach((bar) => {
      bar.classList.remove('state-compare', 'state-swap', 'state-active-range', 'state-sorted', 'state-found');
    });

    sortedIndicesAtStep(index).forEach((i) => {
      if (bars[i]) bars[i].classList.add('state-sorted');
    });

    if (index < 0) return;
    const step = steps[index];
    if (!step) return;

    switch (step.type) {
      case 'compare':
        step.indices.forEach((i) => bars[i] && bars[i].classList.add('state-compare'));
        break;
      case 'swap':
        step.indices.forEach((i) => bars[i] && bars[i].classList.add('state-swap'));
        break;
      case 'overwrite':
        if (bars[step.index]) bars[step.index].classList.add('state-swap');
        break;
      case 'activeRange':
        step.indices.forEach((i) => bars[i] && bars[i].classList.add('state-active-range'));
        break;
      case 'found':
        if (bars[step.index]) bars[step.index].classList.add('state-found');
        break;
    }
  }

  function renderAtStep(index) {
    const values = arrayStateAtStep(Math.max(index, -1));
    renderBars(values);
    applyStateClasses(index);

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
    const speedValue = Number(els.speed.value); // 1 (slow) to 100 (fast)
    const delayMs = 620 - speedValue * 6; // roughly 620ms down to ~20ms
    playTimer = setTimeout(() => {
      if (!playing) return;
      if (currentStep >= steps.length - 1) {
        pause();
        return;
      }
      stepForward();
      scheduleNext();
    }, Math.max(delayMs, 15));
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

  function loadAlgorithmScript(name) {
    return new Promise((resolve, reject) => {
      const script = document.createElement('script');
      script.src = `algorithms/${name}.js`;
      script.onload = () => resolve();
      script.onerror = () => reject(new Error(`Could not load algorithm script: ${name}.js`));
      document.head.appendChild(script);
    });
  }

  async function init() {
    const algo = window.CURRENT_ALGORITHM;

    if (!algo) {
      els.title.textContent = 'No algorithm specified';
      els.description.textContent = 'Opened directly, outside the console app - run a demo and press \'V\' to open a specific algorithm\'s visualization.';
      return;
    }

    try {
      await loadAlgorithmScript(algo);
      const result = window.generateSteps();

      originalArray = result.array;
      steps = result.steps;
      els.title.textContent = result.title;
      els.description.textContent = result.description || '';
      els.scrub.max = steps.length;

      renderLegend();
      wireControls();
      goToStep(-1);
    } catch (err) {
      els.title.textContent = 'Failed to load visualization';
      els.description.textContent = String(err);
    }
  }

  init();
})();
