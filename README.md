# Rogue Piñatas: VRmageddon bHaptics Integration

![Rogue Piñatas: VRmageddon header image](https://github.com/Dteyn/RoguePinatas_bHaptics/blob/main/assets/images/header.jpg)

# Description

This is a mod to add [bHaptics](https://bhaptics.com) support to [Rogue Piñatas: VRmageddon](https://roguepinatas.com/) by [Nerd Ninjas](https://nerdninjas.com/).

Support for TactSuit and TactSleeves / Tactosy arms is included.

The mod uses [BepInEx](https://github.com/BepInEx/BepInEx) version 5.4 (included in the mod .zip file).

>[!NOTE]
> This mod is only compatible with the [Steam version](https://store.steampowered.com/app/2700990/Rogue_Piatas_VRmageddon/). Meta Quest version is not compatible, but hopefully the developers add native support to the game!

# Installation

**Download** the [bHaptics Mod zip file](https://github.com/Dteyn/RoguePinatas_bHaptics/releases/download/v1.0.0/RoguePinatas_bHaptics-v1.0.0.zip) and **extract** into the game folder.

The default game folder location is: `C:\Program Files (x86)\Steam\steamapps\common\Piñata Apocalypse\`

After extracting, you should have these files under **\BepInEx\plugins**:

![Screenshot showing the BepInEx plugins folder](https://github.com/Dteyn/RoguePinatas_bHaptics/blob/main/assets/images/plugins-folder.jpg)

Next, start bHaptics Player, then power on your bHaptics suit and arms/sleeves and start the game. As the game loads, you should feel a 'heart beat' effect to know the suit is connected. That means you're good to go!

# Effects

This mod adds 18 effects to the game, 10 player effects and 8 weapon effects. Melee and ranged weapons have their own effects, and the JolliZapper and BoomBoxer have unique effects.

Effects are implemented for:

 - HeartBeat (Vest)
 - Player Damage (Vest)  
 - Explosions (Vest and Arms)
 - Heal (Vest)
 - Revive (Vest and Arms)
 - Death (Vest)
 - Candy Pickups (Vest)
 - Level Ups (Vest)
 - PartiBox Updgrades (Vest)
 - Melee Weapons (Vest and Arms)
 - Ranged Weapons (Vest and Arms)
 - JolliZapper (Vest and Arms)
 - BoomBoxer (Vest and Arms)

![Screenshot showing the complete list of effects](https://github.com/Dteyn/RoguePinatas_bHaptics/blob/main/assets/images/effects.jpg)

# Configuration

After the game has run once, a configuration file will be created in the game folder, at: **\BepInEx\config\RoguePinatas_bHaptics.cfg**

This file contains several options for configuring the intensity of most haptic effects. The possible intensity range values are `0.0` (off) to `1.0` (maximum).

You can also adjust or turn off the heartbeat effect which happens when player has less than 20% health.

Default config:

```cfg
[Effects]

## Heartbeat effect on low health (less than threshold defined below)
# Setting type: Boolean
# Default value: true
HeartBeatOnLowHealth = true

## Threshold of health (%), below which the heartbeat effect is started (if enabled above)
# Setting type: Single
# Default value: 0.2
LowHealthThreshold = 0.2

## Impact intensity when receiving damage of 1-10 HP
# Setting type: Single
# Default value: 0.6
ImpactIntensityRegDmg = 0.6

## Impact intensity when receiving damage of 10-20 HP
# Setting type: Single
# Default value: 0.8
ImpactIntensityMedDmg = 0.8

## Impact intensity when receiving damage of 20+ HP
# Setting type: Single
# Default value: 1
ImpactIntensityHighDmg = 1

## Candy pickups, small (regular candies)
# Setting type: Single
# Default value: 0.35
CandyPickupSmall = 0.35

## Candy pickups, larger candies
# Setting type: Single
# Default value: 0.7
CandyPickupLarge = 0.7

## Meta Candy pickups, small (regular meta candies)
# Setting type: Single
# Default value: 0.7
MetaCandyPickupSmall = 0.7

## Meta Candy pickups, larger candies
# Setting type: Single
# Default value: 1
MetaCandyPickupLarge = 1

## Level Up effect intensity
# Setting type: Single
# Default value: 0.75
LevelUpIntensity = 0.75

## PartiBox upgrade effect intensity
# Setting type: Single
# Default value: 1
PartiBoxIntensity = 1

[Weapons]

## Default intensity for melee weapons (except JolliZapper)
# Setting type: Single
# Default value: 1
WeaponMeleeIntensity = 1

## JolliZapper intensity when contacting a single enemy or object
# Setting type: Single
# Default value: 0.5
WeaponJolliZapperIntensity = 0.5

## JolliZapper intensity when contacting more than one enemy or object
# Setting type: Single
# Default value: 0.5
WeaponJolliZapperMultiIntensity = 0.5

## Default recoil intensity for most ranged weapons
# Setting type: Single
# Default value: 0.5
WeaponRecoilIntensity = 0.5

## PopRocket recoil intensity
# Setting type: Single
# Default value: 0.7
WeaponPopRocketIntensity = 0.7

## PopSweeper recoil intensity
# Setting type: Single
# Default value: 0.8
WeaponPopSweeperIntensity = 0.8

## Funderbuss recoil intensity
# Setting type: Single
# Default value: 0.75
WeaponFunderbussIntensity = 0.75

## FunderDrone recoil intensity
# Setting type: Single
# Default value: 0.8
WeaponFunderDroneIntensity = 0.8

## BoomBoxer shockwave intensity
# Setting type: Single
# Default value: 0.8
WeaponBoomBoxerIntensity = 0.8

## BoomBlaster shockwave intensity
# Setting type: Single
# Default value: 1
WeaponBoomBlasterIntensity = 1
```
