# Restrainite

Restrainite is a [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader) mod for 
[Resonite](https://resonite.com/) that allows others to control restrictions of the local user. 


> With the current use of dynamic variables, it's not possible to restrict the access by others. Anyone in
> your game world can toggle it. Please keep that in mind and use the options in the extensive settings menu.
> There is currently no known way to restrict this based on user ids or similar, because of how the FrooxEngine and
> the network protocol works. (PRs welcome!) We might add an option to use cloud variables in the future.

## Documentation

Please check out the documentation at [restrainite.github.io](https://restrainite.github.io/) before using the mod.

## Why does this exist?

There are people who have various reasons for wanting certain features of the game disabled. A lot of these features 
can also be disabled through other means like Protoflux and could be seen as malicious, because then they could be 
applied to anyone. Those in-game items already exists. In NeosVR there also existed the NeosNoEscape mod, with similar 
objectives.

Some features can't be disabled with in-game code, so people could try to find in-game exploits to achieve their goal.
This presents a strong incentive to not report any security exploits they find. The primary motivation behind this mod 
is not to remove safety features, but give people a consenting choice to disable them.

If someone is using this mod maliciously, this a moderation issue. 

## Can I rely on this? What if a future Resonite update breaks Restrainite.

Breaking because of updates can always happen and is also very likely, if the game code changes that we patch.
We are reducing the impact of failures as much as possible by using defensive programming. In most cases only a single
feature should malfunction. We are a team of three dedicated developers to work on this, in case something happens.
Due to a team effort, the last breaking release was fixed within 4 hours of
[reporting](https://github.com/Restrainite/RestrainiteMod/issues/87). Our code is open source and we are always open to
bug reports, feature and pull requests. Talk to us on Resonite.

We plan to support this for the future, because we are long term active supporters of Resonite and use the mod
ourselves on a daily bases.

To prevent having too much old code over time, we might deprecate features and eventually remove them. Deprecated features
should not be used anymore and there will be a replacement available. Unless the code in Resonite changes so much,
that we can't provide that feature anymore.

## About the project
RestrainiteMod is © 2024-2025 by SnepDrone, Uruloke, Zenuru

Idea by Fuzy Sidwell

Tested by Azavit, Fuzy Sidwell, K3nzC0, Sylvan and others.

### License
RestrainiteMod is distributed under a [BSD 3-Clause License](https://github.com/Restrainite/RestrainiteMod?tab=BSD-3-Clause-1-ov-file).

### Contributing and Building from source
As an external contributor, when contributing to this project, please first discuss the change you wish to make via issue 
before making a pull request. 

Read more in our [GitHub repo](https://github.com/Restrainite/RestrainiteMod/blob/main/CONTRIBUTING.md).
