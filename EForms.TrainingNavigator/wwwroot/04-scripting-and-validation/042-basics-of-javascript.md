# Basics of JavaScript

JavaScript is the scripting language that runs inside a web page - it's what lets a form react to what the user does, rather than just sitting there as static markup waiting to be submitted. Everything from a simple "you forgot to fill this in" validation message to a running total that updates as someone types is JavaScript.

## Where This Applies

Nothing about the language itself is specific to OnBase - the exact same JavaScript that works in a plain HTML page on any website works inside an OnBase e-form too. What *is* OnBase-specific is the environment the script runs in: which browser engine is actually rendering the form (which affects which JavaScript features are safe to use), and a few conventions OnBase expects (like where scripts and event handlers should live on the form). Those specifics are called out individually in each lesson where they're actually relevant, rather than lumped together here - the goal of this chapter is to build a solid general foundation in the language first.

## Why Start Here

The HTML Fundamentals chapter covered *structure* - what elements exist and how they're marked up. This chapter covers *behavior* - how to make a page actually do something in response to what a user (or the page itself, on load) does. The lessons that follow build in order, each one assuming you're comfortable with everything before it:

1. **Variables and Data Types** - storing and naming values to work with
2. **Operators** - combining, comparing, and transforming those values
3. **Control Flow** - making decisions (`if`/`else`, `switch`) based on a value
4. **Loops** - repeating an action across a range or a collection
5. **Functions** - packaging a piece of logic up so it can be reused and triggered by events
6. **Arrays and Objects** - the two structures almost everything else in JavaScript is built from
7. **Truthy/Falsy Values and Type Coercion** - a deeper, more nuanced look at how JavaScript compares and converts values between types, building on everything above

If you're already comfortable with JavaScript from other work, feel free to skim or skip ahead - but each lesson calls out anything that specifically matters for scripting an OnBase form, so it's worth at least scanning for those notes even if the general language content itself is familiar.
