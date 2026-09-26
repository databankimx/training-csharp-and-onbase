/* Shared light/dark theme toggle logic - used by both the bar-chart player (player.js) and
   the Sieve of Eratosthenes grid (sieve.js). Expects a checkbox with id="theme-checkbox" in
   the page markup (see player.html / sieve.html). */
(function () {
  const THEME_STORAGE_KEY = 'algorithm-visualization-theme';

  function applyTheme(theme, checkbox) {
    document.documentElement.setAttribute('data-theme', theme);
    checkbox.checked = theme === 'dark';
  }

  function initTheme() {
    const checkbox = document.getElementById('theme-checkbox');
    if (!checkbox) return;

    const saved = localStorage.getItem(THEME_STORAGE_KEY);
    const preferred = window.matchMedia('(prefers-color-scheme: light)').matches ? 'light' : 'dark';
    applyTheme(saved || preferred, checkbox);

    checkbox.addEventListener('change', () => {
      const next = checkbox.checked ? 'dark' : 'light';
      applyTheme(next, checkbox);
      localStorage.setItem(THEME_STORAGE_KEY, next);
    });
  }

  initTheme();
})();
