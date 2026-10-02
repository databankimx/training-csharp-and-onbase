# Supplemental: Algorithm Visualizations

## What This Is

Browser-based, step-through visualizations for the algorithms in the Search, Sort, and Reducing Complexity projects. No C# source -- this project is a set of HTML/CSS/JavaScript files that open directly in a browser.

---

## How to Use

After running any algorithm demo in Search, Sort, or Reducing Complexity, press `V` at the "press any key to return" prompt. The project opens the appropriate visualization in the default browser using `Process.Start`.

Alternatively, open `player.html` or `sieve.html` directly from this directory.

---

## What's Included

`player.html` -- a step-through player for the sorting and searching algorithms. Each step highlights the current comparison or swap and shows the operation count. Use the step/play controls to move through the algorithm at whatever pace makes sense.

`sieve.html` -- a dedicated visualization for the Sieve of Eratosthenes. Shows the grid of numbers being marked composite as each prime's multiples are crossed off.

The `algorithms/` subfolder contains the algorithm-specific step data consumed by the player.

---

## Relationship to the Code Projects

The visualizations match the algorithm implementations in the code projects step for step. Run the code first, read the operation count and elapsed time in the console, then open the visualization to see the same sequence of comparisons and swaps animated. The two views are complementary -- the console output shows the quantitative result; the visualization shows the qualitative behavior.
