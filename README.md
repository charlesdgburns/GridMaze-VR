# GridMaze in Unity [under development]

The GridMaze is an experimental apparatus designed by [Thomas Akam](https://www.psy.ox.ac.uk/people/thomas-akam), allowing for goal-directed behaviours in a flexible environment.
Here we replicate the experimental setup and tasks in Unity, mostly for fun (and thanks to @MaxKirkby5's wonderful CAD). We can now simulate a mouse's point of view or even compete in collecting rewards in a goal-directed navigation task.

![GIF of mouse POV](/Screenshots/intro_gif.gif)

The apparatus consists of a 7x7 grid of towers, each armed with a light cue, a sound cue, and a reward port. These towers can be connected by acrylic bridges to create mazes with interesting structure.

In a simple goal-directed navigation task, a single tower is lit which gives a cue that a reward is available at the tower. After poking in the reward port, the light cue turns off and after a random time interval (4-6s), another tower is randomly selected and cued. This usually repeats for 30 minutes, during which mice can collect upwards of 150 rewards.


## Cloning and testing in Unity Editor

If you want to muck around in Unity Editor, maybe testing a new task, you can do so by simply cloning this git repository:

```
cd <your-desired-path> 
git clone https://github.com/charlesdgburns/GridMaze-VR.git
```

Then in your Unity Hub, simply open the new folder `<your-desired-path>/GridMaze-VR` when you "add project from disk".

![Unity Hub -> add -> add project from disk](/Screenshots/add_project_from_disk.png)

This has been tested on Editor versions 6.000.0.42f1 and 6.000.2.13f1 (recommended).

Note that writing tasks in unity is not at all as nice as in [pyControl](https://pycontrol.readthedocs.io/en/latest/), but Claude can help translate taskfiles or write new tasks in C# (best example is found in `/Assets/Scripts/GameStateManager.cs`). 

The BigMaze scene is pretty plug-and-play if you want to have an early go. Will be web-playable soon enough.