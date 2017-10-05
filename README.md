# Custom Collision based 3D Controller
This project might be migrated into a full game project

Currently just developing a base for future games

Goals:
- [x] Collision Detection
- [x] Collision Resolution
  * Still needs fixing up but is functional
- [x] State Machine
  * Handles changes in the Agents logic
  * Uses interfaces to cleanly and dynamical to switch between states
  * Currently planned states
    - [x] Idle
    - [x] Walk
    - [x] Run
    - [x] Jump
      * Has slight control in the air
    - [x] Fall
      * Has no control in the air
    - [x] Roll
      * Souls-Like style
    - [x] Lock On
      * Wind Waker/Souls-Like targeting
      * Strafing around the target
    - [ ] Combat
      * Different weapons, spells and abilities
- [ ] Camera Collision
  * To avoid clipping inside models
      
