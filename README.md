# Spacestation Unity & FMOD

## GETTING STARTED
1. First, download the Unity project from [here](https://drive.google.com/file/d/1XNen4gFheBeZbtGHJ5yFNtuQo3f7jpkH/view?usp=drive_link) (FMOD project included).
2. Completely delete the `Assets` folder in the downloaded Unity project folder.
3. Next, clone this repository into the **same folder you just downloaded**.
4. Rename the cloned folder from `spacestation_unity_assets` to `Assets`.
5. Finally, relocate the repository (if you're using GitHub Desktop).

[See this video tutorial on the above.](https://drive.google.com/file/d/16hUuWjuza5-eHwe0eENf7sL2M5m9uQX3/view?usp=drive_link)

This repository tracks the FMOD project, scripts, and the Unity scene. It ignores the FMOD plugin, Google Resonance, FMOD soundbanks, and other Unity stuff (like textures/materials, etc.), so you can freely change them once Git is set up if you need to.

## Using Git
#### Adding changes
When you've made changes to a file, the process is:

1. *Stage* (AKA *add*) your changed file(s), which means "mark these files to be committed".
2. *Commit* your changes — think of a "commit" as a checkpoint. Making a commit saves a checkpoint on **your** computer. You can make as many commits as you want.
3. *Push* your commit(s), meaning "upload my checkpoint(s) to a GitHub branch".

#### Downloading changes
When someone else has made changes, you must *pull* their changes, meaning "download from a GitHub branch to my computer". We likely won't have to do this very often and can manage it in a meeting.

#### Branches
It is good practice to use Git branches. Each of us could have our own separate branch, and when we're satisfied with our changes, they can then be merged with the master (main) branch.

This way, we only have to worry about our own branch, and then we can merge them all less regularly (meaning less merging, which can be messy and a bit annoying).

Also, if two people are working on the same thing, they may work on the same branch to keep changes compact, for example.

Moving from one branch to another is called a *checkout*.

## TODO
- [ ] Sort out version control
- [ ] Get everyone on the repository and able to push changes
- [ ] Figure out which tasks to delegate
