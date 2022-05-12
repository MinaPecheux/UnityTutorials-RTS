# [Unity/C#] Making a RTS game

**Mina Pêcheux • March 2021 - March 2022**

![thumbnail](imgs/thumbnail.jpg)

### 📕 Discover the series as an ebook and get bonus material <a rel="noopener follow" href="https://mpecheux.gumroad.com/l/rrylr"><strong>on Gumroad</strong></a>!

This repository contains the code for my on-going series of tutorials on how to make a real-time strategy (RTS) game in the well-known game engine Unity! Throughout this series of tutorials, I explore C# scripting for games, GUI building, event systems, behavior trees, sound optimization...

---
#### To run this project, make sure you play it from the **"Core" scene**! ;)

_&rarr; For more info, see <a href="https://mina-pecheux.medium.com/b3c14c46badd" rel="noopener follow">Tutorial #41</a>_


![demo](imgs/demo.gif)

You can find the list of all tutorials [on Medium](https://medium.com/c-sharp-progarmming/making-an-rts-game-in-unity-91a8a0720edc), or right below:

<ul class=""><li>Tutorial #1: <a rel="noopener follow" href="https://medium.com/codex/making-a-rts-game-1-placing-buildings-unity-c-c53c7180b630"><strong>Placing buildings</strong></a></li>

<li>Tutorial #2: <a rel="noopener follow" href="https://medium.com/codex/making-a-rts-game-2-adding-a-very-basic-ui-unity-c-8420480afda0"><strong>Adding a very basic UI</strong></a></li>

<li>Tutorial #3: <a rel="noopener follow" href="https://medium.com/codex/making-a-rts-game-3-setting-up-in-game-resources-unity-c-92355714b2d7"><strong>Setting up in-game resources</strong></a></li>

<li>Interlude #1: <a rel="noopener follow" href="https://medium.com/codex/rts-interlude-1-introducing-an-event-system-unity-c-14c121fb8ed"><strong>Introducing an event system</strong></a></li>

<li>Tutorial #4: <a rel="noopener follow" href="https://medium.com/codex/making-a-rts-game-4-selecting-units-unity-c-1c823b6823a5"><strong>Selecting units</strong></a></li>

<li>Tutorial #5: <a rel="noopener follow" href="https://medium.com/codex/making-a-rts-game-5-transforming-our-data-into-scriptable-objects-unity-c-d7d28a72a20"><strong>Transforming our data into Scriptable Objects</strong></a></li>

<li>Tutorial #6: <a rel="noopener follow" href="https://medium.com/codex/making-a-rts-game-6-improving-the-ui-unity-c-502a018980c4"><strong>Improving the UI</strong></a></li>

<li>Tutorial #7: <a rel="noopener follow" href="https://medium.com/codex/making-a-rts-game-7-polymorphism-take-2-unity-c-fe84d9d87844"><strong>Polymorphism, take 2!</strong></a></li>

<li>Tutorial #8: <a rel="noopener follow" href="https://medium.com/codex/making-a-rts-game-8-boosting-our-selection-feature-unity-c-8552bffd2f8b"><strong>Boosting our selection feature</strong></a></li>

<li>Tutorial #9: <a rel="noopener follow" href="https://medium.com/codex/making-a-rts-game-9-implementing-character-units-and-skills-unity-c-d89b1a3e57b7"><strong>Implementing character units and skills</strong></a></li>

<li>Tutorial #10: <a rel="noopener follow" href="https://medium.com/codex/making-a-rts-game-10-moving-the-camera-unity-c-5a2c2a6a9be2"><strong>Moving the camera</strong></a></li>

<li>Interlude #2: <a rel="noopener follow" href="https://medium.com/codex/rts-interlude-2-refactoring-the-event-system-unity-c-b52a2e3feae"><strong>Refactoring the event system</strong></a></li>

<li>Tutorial #11: <a rel="noopener follow" href="https://medium.com/codex/making-a-rts-game-11-adding-a-day-and-night-cycle-unity-c-ae0cc17a0350"><strong>Adding a day-and-night cycle</strong></a></li>

<li>Tutorial #12: <a rel="noopener follow" href="https://medium.com/codex/making-a-rts-game-12-moving-character-units-unity-c-9119ba4601d1"><strong>Moving Character Units</strong></a></li>

<li>Tutorial #13: <a rel="noopener follow" href="https://medium.com/codex/making-a-rts-game-13-adding-a-minimap-and-fog-of-war-1-3-unity-c-1a7e42bbf9cb"><strong>Adding a minimap and fog of war 1/3</strong></a></li>

<li>Tutorial #14: <a rel="noopener follow" href="https://medium.com/codex/making-a-rts-game-14-adding-a-minimap-and-fog-of-war-2-3-unity-c-bcb4e8da7593"><strong>Adding a minimap and fog of war 2/3</strong></a></li>

<li>Tutorial #15: <a href="https://mina-pecheux.medium.com/making-a-rts-game-15-adding-a-minimap-and-fog-of-war-3-3-unity-c-a89f3548275b" rel="noopener follow"><strong>Adding a minimap and fog of war 3/3</strong></a></li>

<li>Tutorial #16: <a href="https://mina-pecheux.medium.com/making-a-rts-game-16-introducing-a-sound-system-1-2-unity-c-c3153902161e" rel="noopener follow"><strong>Introducing a sound system 1/2</strong></a></li>

<li>Tutorial #17: <a href="https://mina-pecheux.medium.com/making-a-rts-game-17-introducing-a-sound-system-2-2-unity-c-bb72a51c56c1" rel="noopener follow"><strong>Introducing a sound system 2/2</strong></a></li>

<li>Tutorial #18: <a href="https://mina-pecheux.medium.com/making-a-rts-game-18-preparing-our-game-parameters-unity-c-96d3f598ecd5" rel="noopener follow"><strong>Preparing our game parameters</strong></a></li>

<li>Tutorial #19: <a href="https://mina-pecheux.medium.com/making-a-rts-game-19-displaying-our-in-game-settings-unity-c-f551e5a93032" rel="noopener follow"><strong>Displaying our in-game settings!</strong></a></li>

<li>Tutorial #20: <a href="https://mina-pecheux.medium.com/making-a-rts-game-20-saving-the-players-data-properly-unity-c-1c7f5af29329" rel="noopener follow"><strong>Saving the player’s data properly</strong></a></li>

<li>Tutorial #21: <a href="https://mina-pecheux.medium.com/making-a-rts-game-21-adding-players-and-unit-ownership-unity-c-43144a8375f" rel="noopener follow"><strong>Adding players and unit ownership</strong></a></li>

<li>Tutorial #22: <a href="https://mina-pecheux.medium.com/making-a-rts-game-22-producing-some-resources-with-our-buildings-unity-c-5dde5253f329" rel="noopener follow"><strong>Producing some resources with our buildings</strong></a></li>

<li>Tutorial #23: <a href="https://mina-pecheux.medium.com/making-a-rts-game-23-implementing-behaviour-trees-for-our-units-1-3-unity-c-1a61840058a6" rel="noopener follow"><strong>Implementing behaviour trees for our units 1/3</strong></a></li>

<li>Tutorial #24: <a href="https://mina-pecheux.medium.com/making-a-rts-game-24-implementing-behaviour-trees-for-our-units-2-3-unity-c-17f14cc3c580" rel="noopener follow"><strong>Implementing behaviour trees for our units 2/3</strong></a></li>

<li>Tutorial #25: <a href="https://mina-pecheux.medium.com/making-a-rts-game-25-implementing-behaviour-trees-for-our-units-3-3-unity-c-a132340bc71c" rel="noopener follow"><strong>Implementing behaviour trees for our units 3/3</strong></a></li>

<li>Tutorial #26: <a href="https://mina-pecheux.medium.com/making-a-rts-game-26-levelling-up-our-units-1-2-unity-c-22d3a25cc41" rel="noopener follow"><strong>Levelling up our units! 1/2</strong></a></li>

<li>Tutorial #27: <a href="https://mina-pecheux.medium.com/making-a-rts-game-27-levelling-up-our-units-2-2-unity-c-33e6959889b6" rel="noopener follow"><strong>Levelling up our units! 2/2</strong></a></li>

<li>Tutorial #28: <a href="https://mina-pecheux.medium.com/making-a-rts-game-28-adding-some-shortcuts-unity-c-c437b635ffca" rel="noopener follow"><strong>Adding some shortcuts</strong></a></li>

<li>Tutorial #29: <a href="https://mina-pecheux.medium.com/making-a-rts-game-29-improving-our-players-system-unity-c-328dfc9b818" rel="noopener follow"><strong>Improving our players system</strong></a></li>

<li>Tutorial #30: <a href="https://mina-pecheux.medium.com/making-a-rts-game-30-refactoring-our-save-load-system-with-binary-serialisation-1-2-unity-c-a388083cfbae" rel="noopener follow"><strong>Refactoring our save/load system with binary serialisation 1/2</strong></a></li>

<li>Tutorial #31: <a href="https://mina-pecheux.medium.com/making-a-rts-game-31-refactoring-our-save-load-system-with-binary-serialisation-2-2-unity-c-eb2c807c0fe6" rel="noopener follow"><strong>Refactoring our save/load system with binary serialisation 2/2</strong></a></li>

<li>Tutorial #32: <a href="https://mina-pecheux.medium.com/making-a-rts-game-32-creating-a-debug-console-unity-c-841f0fb97dda" rel="noopener follow"><strong>Creating a debug console</strong></a></li>

<li>Tutorial #33: <a href="https://mina-pecheux.medium.com/making-a-rts-game-33-using-unitys-terrain-tools-unity-c-2ed360459536" rel="noopener follow"><strong>Using Unity’s terrain tools</strong></a></li>

<li>Tutorial #34: <a href="https://mina-pecheux.medium.com/making-a-rts-game-34-improving-unit-navigation-adding-unit-formations-1-2-unity-c-234c1fcd8" rel="noopener follow"><strong>Improving unit navigation & adding unit formations 1/2</strong></a></li>

<li>Tutorial #35: <a href="https://mina-pecheux.medium.com/2757fabcbfc" rel="noopener follow"><strong>Improving unit navigation & adding unit formations 2/2</strong></a></li>

<li>Tutorial #36: <a href="https://mina-pecheux.medium.com/making-a-rts-game-36-using-workers-to-construct-buildings-1-3-unity-c-eda1a96b0c92" rel="noopener follow"><strong>Using workers to construct buildings 1/3</strong></a></li>

<li>Tutorial #37: <a href="https://mina-pecheux.medium.com/making-a-rts-game-37-using-workers-to-construct-buildings-2-3-unity-c-a95b6faf5f3" rel="noopener follow"><strong>Using workers to construct buildings 2/3</strong></a></li>

<li>Tutorial #38: <a href="https://mina-pecheux.medium.com/making-a-rts-game-38-using-workers-to-construct-buildings-3-3-unity-c-1469f08adb76" rel="noopener follow"><strong>Using workers to construct buildings 3/3</strong></a></li>

<li>Tutorial #39: <a href="https://mina-pecheux.medium.com/making-a-rts-game-39-boosting-our-game-scene-unity-c-bab128549317" rel="noopener follow"><strong>Boosting our game scene</strong></a></li>

<li>Tutorial #40: <a href="https://mina-pecheux.medium.com/making-a-rts-game-40-fixing-our-minimap-unity-c-dd3f46c8cfb4" rel="noopener follow"><strong>Fixing our minimap</strong></a></li>

<li>Tutorial #41: <a href="https://mina-pecheux.medium.com/b3c14c46badd" rel="noopener follow"><strong>Preparing for a main menu...</strong></a></li>

<li>Tutorial #42: <a href="https://mina-pecheux.medium.com/making-a-rts-game-42-designing-our-main-menu-1-2-unity-c-6ccf284c4db7" rel="noopener follow"><strong>Designing our main menu 1/2</strong></a></li>

<li>Tutorial #43: <a href="https://mina-pecheux.medium.com/making-a-rts-game-43-designing-our-main-menu-2-2-unity-c-44ddca297787" rel="noopener follow"><strong>Designing our main menu 2/2</strong></a></li>

<li>Tutorial #44: <a href="https://mina-pecheux.medium.com/making-a-rts-game-44-importing-models-animating-our-characters-1-2-unity-c-9f883a494f9b" rel="noopener follow"><strong>Importing models & animating our characters 1/2</strong></a></li>

<li>Tutorial #45: <a href="https://mina-pecheux.medium.com/making-a-rts-game-45-importing-models-animating-our-characters-2-2-unity-c-89f346f3fa92" rel="noopener follow"><strong>Importing models & animating our characters 2/2</strong></a></li>

<li>Tutorial #46: <a href="https://mina-pecheux.medium.com/making-a-rts-game-46-saving-our-game-scene-data-unity-c-79ef231ffa22" rel="noopener follow"><strong>Saving our game scene data...</strong></a></li>

<li>Tutorial #47: <a href="https://mina-pecheux.medium.com/making-a-rts-game-47-and-loading-back-our-game-scene-data-unity-c-f7399cac2b97" rel="noopener follow"><strong>... and loading back our game scene data!</strong></a></li>

<li>Interlude #3: <a href="https://mina-pecheux.medium.com/rts-interlude-3-showing-our-updated-minimaps-in-the-menu-unity-c-1ffd63fc9e2e" rel="noopener follow"><strong>Showing our updated minimaps in the menu</strong></a></li>

<li>Tutorial #48: <a href="https://mina-pecheux.medium.com/making-a-rts-game-48-various-fixes-improvements-and-clean-ups-unity-c-5ab3a587c44a" rel="noopener follow"><strong>Various fixes, improvements and clean-ups</strong></a></li>

<li>Tutorial #49: <a href="https://mina-pecheux.medium.com/making-a-rts-game-49-optimisation-tips-tricks-unity-c-d4d70001e58c" rel="noopener follow"><strong>Optimisation tips & tricks</strong></a></li>

<li>Interlude #4: <a href="https://mina-pecheux.medium.com/rts-interlude-4-improving-the-healthbars-unity-c-48ee8d663e09" rel="noopener follow"><strong>Improving the healthbars</strong></a></li>
  
<li>Tutorial #50: <a href="https://mina-pecheux.medium.com/making-a-rts-game-50-implementing-a-technology-tree-1-3-unity-c-1c516ba78712" rel="noopener follow"><strong>Implementing a technology tree 1/3</strong></a></li>

<li>Tutorial #51: <a href="https://mina-pecheux.medium.com/making-a-rts-game-51-implementing-a-technology-tree-2-3-unity-c-8f2e757ac5b" rel="noopener follow"><strong>Implementing a technology tree 2/3</strong></a></li>

<li>Tutorial #52: <a href="https://mina-pecheux.medium.com/making-a-rts-game-52-implementing-a-technology-tree-3-3-unity-c-c7038b979e77" rel="noopener follow"><strong>Implementing a technology tree 3/3</strong></a></li>

<li>Article #53: <a href="https://mina-pecheux.medium.com/making-a-rts-game-53-final-words-thanks-and-an-ebook-unity-c-aba38542b9b9" rel="noopener follow"><strong>Final words, thanks… and an ebook!</strong></a></li>

</ul>

## Known issues

### About installing / upgrading

The project was written in Unity 2020.3. This implies that, when loading it up with different recent versions of Unity, you'll need to "upgrade" the project. This usually goes without a hitch, but there is for now 2 bugs that have been spotted by the followers and I:

- **incorrect lighting**: upgrading to some Unity 2020+ might lead to some strange lighting of the scene (because of pipeline render defaults and lighting settings changes). To get a similar lighting to the original project, make sure that you remove all baked lighting data, that all your (directional) lights are set to "Realtime" and that you ignore the environment lighting.
  
  To do so, open the Window > Rendering > Lighting panel, go to the "Environment" tab, and set the "Intensity Multiplier" variable to 0 both in the Environment Lighting and Environment Reflections sections.

- **FOV shader bug** (thanks to @Oarcinae): upgrading to **Unity 2019.4** might lead to unexpected behaviour for the FOV shader (because of pipeline render defaults changes); to fix the issue, you can try modifying the `AlphaProjection.shader` and comment out the line: `#pragma exclude_renderers d3d11 gles` <i>(still an open issue #2, any help is welcome 🙂)</i>
