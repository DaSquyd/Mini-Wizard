# Mini Wizard

## Significant Mechanics
**Multi-Jump and Jump Forgiveness**
After the player has left the ground, they are allowed 2 additional jumps. We have implemented a system in which the player can still perfomr a grounded jump up to 0.1 seconds after falling off the side of a platform. We did this as it sometimes felt like the player deserved to get their single grounded jump but they technically weren't on the ground anymore. This way, the player has a much more reliable jump when performing tighter platforming tasks.

**Auto Enemy Targeter and Seeking Projectiles**
When the player shoots, we use a quaternion angle difference of all the enemies in a conecast in front of the player to determine which is closer to the center of the player's LoS. We weight this with the physical distance to the player to produce an ideal target. We are still fine-tuning this to make sure that the auto targeter selects an enemy intuitively. This can be demonstrated at the start of the test world with the red "enemies" to the right. The projectiles will seek the enemies they're targeted to. We chose to do this as we're not making a shooter, so the player shouldn't be required to have great accuracy. We instead focus on the selection of 

**Sword Movement Technique and Combos**
The sword (which is not yet implemented graphically) is the melee attack for the player. However, it also allows for some additional movement capabilities, especially in the air. The player is only allowed to perform one single attack combo in the air. A combo occurs when the player hits the melee button (or mouse 1) multiple times in a row within a given window. The first two attacks are slashes and the third and final attack is a jab. The player receives reduced gravity while in the attack animation as well as a forward boost, which allows them to cross gaps when used alongside jumps.

**Swapping Weapons**
The player can either be in the state of having a fire sword in one hand with iceballs in the other or fireballs in one hand and an ice sword in the other. Enemies have different weaknesses, so swapping between weapons is crucial. The fires sword shifts into the fireballs and vice versa, and the ice sword shifts into the iceballs and vice versa. Currently, we do not have the visuals in. However, the functionality does exist.
