# Map Generator

## Overview

**Map Generator** implements the following algorithms and neural networks that enable map generation and classification:

1. Wave Function Collapse (WFC) algorithm: this is a procedural generation technique that allows complex patterns to be created from an input sample. It was used to generate maps to form a training data set for neural networks.
2. **Markov Chains of order n**: This is a mathematical modeling method based on the probability of moving to a future state considering n previous states, which allows meaningful content to be generated. Like the previous algorithm, this method was used to generate maps for the training set.
3. **Neural Network**: Using the maps generated with the previous methods (approximately 100 maps), a neural network model was trained to classify the maps according to easy, medium, or difficult difficulty, labels that the generated maps presented when the set was supplied.
4. **Generative Neural Network**: The **Wasserstein Generative Adversarial Network with Gradient Penalty** (WGAN-GP) generative neural network was implemented as a technique to generate maps with different levels of difficulty, using the same set from the previous neural network as training data.

This project was developed by Tomás Concha, Valentine Sierra and Joel Díaz, students of Video Game Development and Virtual Reality Engineering at the University of Talca, as part of Project 3 of the Procedural Content Generation module

## Technologies used

The Wave Function Collapse and Markov Chains of order n algorithms were developed in the Unity video game graphics engine, while the Neural Network and WGAN-GP were developed in the Google Colab cloud service.

## Instructions for use

To execute the first part of this project (WFC algorithms and Markov Chains of order n), follow these instructions:

1. Clone the repository locally on your computer.
2. Open the cloned project with Unity.
3. Run the program.

Once the program is open and running, follow these instructions to modify the parameters of the algorithms from the Inspector:

1. Use the following controls to generate and display maps:<ol type="a">
     <li>Wave Function Collapse.</li><ol type="i">
     <li>R to draw the map.</li>
     <li>G to generate a map.</li>
     <li>W to initialize and generate a map when the sample map is changed.</li>
     </ol>
     <li>Markov Chains of order n.</li><ol type="i">
     <li>R to draw the map.</li>
     <li>Space to generate a map.</li>
     </ol>
   </ol>
2. Maps generated at runtime will be displayed in the Assets/Generated Level.txt file.
3. If you want to change the sample map for the algorithms, modify or create the file in Assets/Sample Levels/Handmade Levels/.

To test the second part of the project, corresponding to neural networks, follow these instructions:

1. Select one of the links below to go to the project in Google Colab:<ol type="a">
     <li>Neural Network for classifying maps.</li>
     <li>WGAN-GP for generating maps.</li>
   </ol>
2. Once inside the project, select the “Run all” option in the taskbar.
3. If you wish, you can adjust parameters such as strides or padding to experiment with different configurations.
