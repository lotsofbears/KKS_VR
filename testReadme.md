## Controls overview
There are two controllers for each hand with identical functional without any tools or modes.  
There is no input customization or helping texts.  
Designed to be able to do any action with a **single hand**.  
The only means of movement are native in-game functions and *GripMove*, no more *Warp*.  
**No double clicks**, only *Short* and *LongPress* for *Trigger, Grip and Touchpad*.   
The sole function of *Menu* button is to toggle menu's visibility.  
* **Grip** is a **Grab** button. It grabs things to move them around.  
* **Trigger** is an **Action** button that performs actions where applicable or completes their wait period if one is already queued but not yet determined whether it's *Short* or *LongPress*.  
* **Touchpad** aka joystick is a **Generic** button. Never requires click in non-centered positions.

## Modules and their inputs

### GripMove
Grab the world and move around oneself.  
Available in **Any Scene** as the last priority action i.e. when no better actions are available.   
* **Grip** to start *GripMove*.  
* **Trigger** while in *GripMove* to manipulate **Yaw** while using controller as an axis.  
* **Touchpad** while in *GripMove* with pressed *Trigger* to manipulate **Rotation** of the camera.  
* **Touchpad** while in *GripMove* without pressed *Trigger* to become **Upright**. Registeres after *LongPress*.

Depending on the context may behave differently.  

### Impersonation aka PoV
Assume orientation of a character's head and loosely follow it.  
Available in **H Scene** outside of character interactions.  
* **Touchpad** to start, stop, change or reset *Impersonation*. Registers after *LongPress*.
* **Touchpad** while in *Impersonation* and in *GripMove* with pressed *Trigger* to enable remote following with current vector between the camera and a character's head as an offset.

Has settings for gender preferences and automatization.

### Assisted kiss/lick
Attach the camera to a partner's PoI to follow it.  
Available in **H Scene** when the camera is close to the said PoI. Outside of caress requires *GripMove*.  
* **Grip** while *Assisted* to start altered version of *GripMove* to acquire precise offsets on the fly. The long gap between camera and PoI will cause disengagement.
* **Trigger** while *Assisted* and not in *GripMove* to stop action and disengage.

Has plenty of settings for customization. 

### Controllers representation
Native in-game items serving as a representation of controllers.  
Available in **Any Scene** as the last priority action i.e. when no better actions are available.  
They won't go inside of things easily, preferring instead to stick to the surface.
* **Touchpad** with pressed *Trigger* to cycle available items.
* **HorizontalDirection** with pressed *Trigger* to cycle through item's animations.

### Grasp aka IK Manipulator
Alter currently playing animation on the fly.  
Available in **H, Talk and Text Scenes** when interacting with a character i.e. controller is in close proximity to it.  
* **Grip** to start *Grasp* i.e. hold relevant body parts and reposition them with the controller movements.
* **Trigger** while in *Grasp* to extend the amount of held body parts, up to the whole character. Registers with *ShortPress*.
* **Trigger** while in *Grasp* to extend the amount of held body parts temporarily. Registers with *LongPress*.
* **Trigger** while in *Grasp* and visual cue of held body part is green to attach body part.  
  Currently only to self/different character or controller. ~~Hand holding~~
* **Touchpad** while in *Grasp* to reset currently held body parts to default offsets.
* **Touchpad** while not in *Grasp* to reset relevant body part to the default offset. Registers as *LongPress*.
* **HorizontalDirection** while in *Grasp* and the main held body part is a hand to scroll through hand animations. Goes full circle then resets to default.
* **HorizontalDirection** while in *Grasp* and holding whole character to change *Yaw* of a character.
* **VerticalDirection** while in *Grasp* and holding whole character to move said character in direction of the camera.

### H Scene Interpreter
Provides VR-esque controls.  
Available in **H Scene**, relies heavily on [SensibleH](https://github.com/lotsofbears/KK_SensibleH), without it many functions will be unaviliable.  
Described horizonal directions assume the right controller, for the left controller the directions will be mirrored.
#### Generic
* **LeftDirection** to choose random animation of current category. Requires *LongPress*. 
* **RightDirection** to enter *PointMove*.
* **DirectionDown** on partner's body part to undress it.

#### PointMove
* **RightDirection** to choose one at random and exit *PointMove*. Requires *LongPress*.
* **VecrticalDirection** to scroll through available categories.
* **LeftDirection** to exit *PointMove*.

#### Caress
*AutoCaress* can be overtaken in any way by *Assisted kiss/lick*.  
* **Grip** on attached caress item while in *AutoCaress* to take manual control.
* **Trigger** on attached caress item to start *AutoCaress*. Requires *LongPress*.
* **Trigger** on attached caress item while in *AutoCaress* to stop it.
* **Trigger** while in manual control of a carress item to squize. Might not always work if *AutoCaress* still runs some other item.
* **DirectionDown** on attached caress item while not in *AutoCaress* to detach it.
* **DirectionDown** while not in *AutoCaress* to prompt a partner to initiate a kiss. Limited to caress positions. Requires *LongPress*.
* **HorizontalDirection** on attached caress item to toggle it's visibility.
* **HorizontalDirection** while an attached caress item is present to scroll through animations. Limited to caress positions.

#### Service, Intercourse
* **DirectionUp** to insert, start, finish, change speed. Requires *LongPress*.
* **DirectionUp** with pressed *Trigger* to opt for a no voice options. Requires *LongPress*.
* **DirectionUp** with pressed *Touchpad* to opt for anal if applicable. Can be used with pressed *Trigger*. Requires *LongPress*.
* **DirectionDown** to set condom, pullout, stop, change to outside during climax, change speed. Requires *LongPress*.
* **HorizontalDirection** to scroll through animations.
