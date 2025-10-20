# PC(hemistry)G

## Overview

**PC(hemistry)G** is a Unity tool developed to improve the Chemistry TD video game using evolutionary algorithms and other procedural content generation techniques for puzzles. The tool features the implementation of:

1.  The  **Mu + Lambda Evolution Strategy**  algorithm. This generates routes for enemies to travel along, taking into account different parameters such as map size, route length, and percentage of map coverage, among others. To do this, different routes corresponding to generations are generated. In each generation, the best candidates are selected based on an evaluation with a fitness function. The best-rated routes are part of the new generation, as well as newly random generated maps. The same process is applied iteratively for a certain number of generations, and the best route is obtained according to the given parameters.
2.  The  **Backward from Goal State**  puzzle technique. This technique was implemented according to the context of the game. At the beginning, the enemies that will appear in the round are selected by the subsequent technique. Each enemy has a certain resistance to the turrets managed by the player. The turrets that cause the most damage to the enemies are determined, that is, the final and ideal state of the game in order to win, or solve it. Then, the compounds that make them up are broken down to obtain the elements with which they are synthesized. Finally, random elements are added to mislead the player. In this way, the initial state of the game is obtained by regressing from the goal state in which the player wins, making the puzzle solvable.
3.  The random local search technique  **Hill Climbing**. This technique was implemented to work in conjunction with the Backward from Goal State technique. It consists of defining the difficulty of the puzzle to be generated based on the parameters provided. The algorithm moves through the space of possible virus combinations, adjusting the amount of resistance and maximum damage that the turrets can produce, so that it depends more on the strategy adopted rather than the characteristics of the turrets.

This project was developed by Tomás Concha, Valentine Sierra and Joel Díaz, students of Video Game Development and Virtual Reality Engineering at the University of Talca, as part of Project 2 of the Procedural Content Generation module

## Instructions for use

To run the program, follow these instructions:

1. Download the .zip folder with the project.
2. Unzip the folder.
3. Open the “PCG.exe” file.

Once the program is open and running, follow these instructions to modify the parameters of the algorithms:

1. On the right side of the screen there is a panel with modifiable parameters, each of which allows you to change some aspect of the result of the execution of the algorithms. These parameters are:<ol type="a">
     <li>Virus Amount: This corresponds to the number of different viruses to spawn, which varies between 2 and 4.</li>
     <li>Resistance Amount: It is the sum of the resistance of viruses to spawning and corresponds to a float factor that varies between 1 and 3.</li>
     <li>Difficulty: It indicates the difficulty based on the maximum possible damage that can be done and how much this will be reflected in the puzzle; a factor between 0 and 1 for the actual maximum damage.</li>
     <li>Extension Weight: The weight for the evaluation function related to the length of the path that viruses will follow.</li>
     <li>Coverage Weight: The weight for the evaluation function related to the total percentage of the map covered by the path that the viruses will follow.</li>
     <li>Turns Weight: The weight for the evaluation function related to the number of turns that the path followed by the viruses will have.</li>
     <li>Turns Density: This variable corresponds to the desired density of turns on the route. The map turn evaluation function adapts to the desired turn density.</li>
     <li>Seed: Allows you to replicate a puzzle by controlling randomness.</li>
   </ol>
2. Once the desired configuration is ready, press the Generate button at the bottom of the panel.
3. With the mouse, you can press the buttons corresponding to the names of the compounds to spawn the turrets on the map, wherever you click on it, except the path the viruses follow. You can only create the same number of turrets as your Virus Amount.
4. Press the Start Round button to begin the round.
