## Game Design Document: Block Brawlers (Working Title) - Revised

**Document Version:*### 6. Gameplay Mechanics

#### 6.1. Character & Combat.2
**Date:** June 9, 2025
**Platform:** PC (Windows/macOS/Linux) via MonoGame
**Development Focus:** Code-first, AI-assisted development in Visual Studio Code. Minimal reliance on visual editors.

---

### 1. Game Overview

"Block Brawlers" is a multiplayer online battleground (MOB) game with a **class-based progression system that emphasizes flexible spell selection and team synergy**. Players customize blocky, pixel-art characters, choose a **base class (Brawler, Ranger, or Spellcaster)**, and then select from a curated pool of **~30 class-specific spells** to equip 8 active abilities for battle. The core experience is driven by player choice, tactical execution, and cooperative gameplay within large-scale team battles.

### 2. Core Game Loop

1.  **Launch Game:** Player launches the MonoGame application.
2.  **Main Menu:** Accesses Character Creation, Game Modes, Settings, and Quit.
3.  **Character Creation:** Design a pixelated character (body shape, color, simple textures), **then choose a base class (Brawler, Ranger, or Spellcaster)**.
4.  **Spell Selection:** From a pool of approximately 30 spells specific to the chosen base class, select 8 active spells for the action bar.
5.  **Queue for Game Mode:** Select a desired multiplayer battleground (5v5, 16v16, 32v32) or a training/practice area.
6.  **Multiplayer Battleground:** Engage in objective-based PvP combat, earning experience and currency.
7.  **Post-Game:** View results, earn rewards, return to main menu or re-queue.
8.  **Progression:** Use earned currency to acquire new gear (minor stat improvements, aesthetic). All class spells are available from the start; there's no unlocking of additional spells within your chosen class.

### 3. Visuals & Art Style

-   **Aesthetic:** Low-poly, pixelated, "boxy" art style reminiscent of early 3D games or Minecraft.
-   **Characters/Mobs:** Composed entirely of textured blocks/cuboids. No traditional skeletal animation; movement will be achieved through rigid body rotations and translations of block components.
-   **Environments:** Maps constructed from large, textured blocks, creating clear pathways, chokepoints, and objectives.
-   **Textures:** Low-resolution, pixel-art textures applied to block surfaces to define details and materials.
-   **Spells:** Visual effects for spells will be particle-based and programmatic, matching the pixelated aesthetic.
-   **User Interface (UI):** Clean, functional, pixel-art inspired UI elements (buttons, health bars, spell icons) rendered directly via code.

---

### 4. Development Environment Setup

**Prerequisites for developers getting started:**

#### 4.1. Required Tools
- **Visual Studio Code** - Primary development environment
- **.NET 8.0 SDK** - For building C# projects
- **MonoGame** - Game development framework
- **Azure PlayFab LocalMultiplayerAgent** - Required for local server testing

#### 4.2. LocalMultiplayerAgent Setup
The LocalMultiplayerAgent is essential for testing your dedicated server locally before deploying to Azure PlayFab.

1. **Download LocalMultiplayerAgent:**
   - Go to the [PlayFab LocalMultiplayerAgent releases](https://github.com/PlayFab/LocalMultiplayerAgent/releases)
   - Download the latest release for Windows
   - Extract to `C:\Main\Tools\LocalMultiplayerAgent` (or update the path in `deploy-local-server.bat`)

2. **Verify Installation:**
   - The extracted folder should contain `LocalMultiplayerAgent.exe`
   - You should see sample configuration files like `MultiplayerSettingsLinuxContainersOnWindowsSample.json`

3. **First-time Setup:**
   - Run the `deploy-local-server.bat` script from the project root
   - This will build your server, create the necessary configuration files, and deploy to the LocalMultiplayerAgent
   - The LocalMultiplayerAgent will automatically start after successful deployment

#### 4.3. PlayFab Account Setup
- Create a free developer account at [PlayFab.com](https://playfab.com)
- Create a new "Title" for Block Brawlers in the PlayFab Game Manager dashboard
- Note your Title ID for configuration

#### 4.4. Development Workflow
1. Make changes to your game code
2. Run `deploy-local-server.bat` to build and deploy the server locally
3. Test multiplayer functionality using the LocalMultiplayerAgent
4. Debug by attaching to the `project-axiom-server.exe` process as needed

---

### 5. Technical Requirements & Architecture Philosophy

-   **Engine:** MonoGame (C#).
-   **Development Environment:** Visual Studio Code.
-   **Backend Platform:** **Azure PlayFab**. The backend will be built on PlayFab's managed services. This includes **PlayFab Multiplayer Servers** for hosting dedicated server instances, and PlayFab's services for player data, matchmaking, and live operations.
-   **Authentication:** For the MVP, a basic **PlayFab email/password** system will be implemented for testing and initial play. The long-term goal, post-MVP and upon integration with game storefronts, is to use **Steam Authentication** via the Steamworks SDK.
-   **Core Principle:** All game logic, UI, entity definitions, and game mode rules will be implemented in C# code. 3D assets will be defined programmatically (e.g., cube meshes generated by code) or loaded as simple, text-based data (e.g., OBJ files with associated texture paths). Textures will be simple 2D image files.
-   **Networking:** Client-server architecture. The `project-axiom-server.exe` will be a custom dedicated server executable managed and hosted by Azure PlayFab. Players connect as clients.
-   **Physics:** Integration of a suitable C# physics library (e.g., JoltPhysics.NET, BEPUphysics v2) for character collision, spell projectiles, and environmental interactions.
-   **AI (Bots):** Implement sophisticated bot AI to fill game modes, assist with player base establishment, and provide testing capabilities. Bots should demonstrate pathfinding, target prioritization, spell usage, and objective awareness.
-   **Data Serialization:** Game data (character saves, spell definitions, gear properties, map layouts) will be stored and loaded using structured formats (e.g., JSON, XML, or custom binary formats) to facilitate AI understanding and manipulation.

---

### 6. Gameplay Mechanics

#### 5.1. Character & Combat

- **Character Model:** Player character composed of basic block primitives (head, torso, arms, legs). Movement handled by translating and rotating these blocks.
- **First-Person Perspective:** Primary gameplay view is first-person. Third-person view (optional, lower priority) could be added later for character admiration.
- **Movement:** Standard WASD movement, space for jump, mouse for camera look. Sprinting mechanic.
- **Targeting:**
    - **Click-to-target:** Clicking on an enemy within a certain range highlights and locks onto them for single-target spells.
    - **Reticle-based (Skillshot):** Many spells will be skillshots, requiring direct aiming with a mouse reticle.
    - **Area-of-Effect (AoE):** Spells with a defined radius around the player or a targeted location.
- **Spell Casting:**
    - **Action Bar:** 8 customizable spell slots bound to hotkeys (1-8).
    - **Resource System:** Spells consume a class-appropriate resource (e.g., Mana for Spellcasters, Frenzy/Stamina for Brawlers, Energy for Rangers). Regeneration over time.
    - **Cooldowns:** Each spell has an individual cooldown.
- **Health System:** Player has a health pool. Damage reduces health. Reaching 0 health results in death.
- **Death & Respawn:** Players respawn after a short delay at a designated spawn point in battlegrounds.
- **Damage Types:** Spells can inflict different damage types (e.g., Arcane, Fire, Frost, Physical). Resistance/vulnerability system (lower priority).

#### 6.2. Base Classes & Spells

Players choose one of three base classes, each with its own thematic focus, armor type, primary resource, and a unique pool of approximately **30 spells** from which to select their 8 active abilities.

- **1. Spellcaster:**
    
    - **Theme:** Focuses on magical abilities, ranged combat, and elemental manipulation.
    - **Armor:** Light Armor (lowest base defense).
    - **Resource:** Mana (regenerates slowly, often replenished by specific spells or passive effects).
    - **Spell Examples:** Fireball, Ice Shard, Arcane Missile, Healing Word, Shield Bubble, Teleport, Chain Lightning, Polymorph, Mass Dispel, Blizzard, Drain Life, Summon Imp.
    - **Gameplay Role:** High burst damage, crowd control, healing, utility. Squishy but powerful.
    - **Inspiration:** Warlock, Mage, Priest.
- **2. Ranger:**
    
    - **Theme:** Focuses on agility, precision, stealth, and nature-based abilities.
    - **Armor:** Medium Armor (balanced defense).
    - **Resource:** Energy (regenerates quickly, often used for rapid, low-cooldown abilities).
    - **Spell Examples:** Multi-shot, Explosive Arrow, Stealth, Dash, Bear Trap, Healing Touch (minor self/ally heal), Poison Dart, Vine Snare, Beast Companion (temporary summon), Fan of Knives, Blind.
    - **Gameplay Role:** Sustained damage, mobility, debuffs, traps, opportunistic strikes.
    - **Inspiration:** Hunter, Rogue, Thief, Druid.
- **3. Brawler:**
    
    - **Theme:** Focuses on melee combat, durability, physical prowess, and direct confrontation.
    - **Armor:** Heavy Armor (highest base defense).
    - **Resource:** Frenzy/Stamina (generated by dealing/taking damage, or slowly over time; used for powerful offensive/defensive abilities).
    - **Spell Examples:** Charge, Slam, Taunt, Shield Wall (damage reduction), Cleave, Hamstring (slow), Ground Pound (AoE stun), Intercept, Battle Cry (team buff), Execute, Impenetrable Skin.
    - **Gameplay Role:** Tanking, front-line damage, disruptive crowd control, area denial.
    - **Inspiration:** Warrior, Death Knight, Paladin.
- **Spell Definition:** Each spell will be defined by a structured C# class or data object (e.g., `SpellData` containing name, description, resource cost, cooldown, range, damage type, cast time, and references to associated visual effects and sound effects). This is a prime area for AI assistance in defining and balancing.
    

#### 6.3. Gear & Progression

- **Gear Slots:** Head, Chest, Legs, Hands, Feet, Weapon, Trinket (2).
- **Stat Increments:** Gear provides minor statistical improvements (e.g., +Health, +Resource Pool, +Spell Power, +Cooldown Reduction). Each class benefits more from certain stats.
- **Power Ceiling:** Earned gear should provide a maximum of **10%** statistical advantage over base/starter gear. This ensures skill remains paramount.
- **Acquisition:** Earned through playing game modes (rewards for winning, performance).
- **Cash Store (Future):** Cosmetic items only. No pay-to-win elements. Aesthetic gear from the cash store should _never_ be statistically better than earned gear. This is a low-priority feature for initial development.

#### 6.4. Multiplayer & Game Modes

- **Lobby/Queue System:** Players queue for desired game modes.
- **Matchmaking:** Simple matchmaking based on queue time and game mode selection. (No complex ELO/MMR initially).
- **Dedicated Servers:** Game logic resides on dedicated servers, with clients sending input and receiving game state.
- **Game Modes:**
    - **Training Grounds:**
        - Single-player instance with static training dummies.
        - No time limit.
        - Purpose: Test spells, character movement, and UI.
        - High priority for initial development.
    - **Ranked 5v5 (Capture the Point):**
        - Two teams of 5 players.
        - Small, focused map with 3-5 capture points.
        - Objective: Capture and hold points to accrue score. First to X score or highest score at time limit wins.
    - **Unranked 16v16 (Arathi Basin Style):**
        - Two teams of 16 players.
        - Larger map with 5-7 capture points (e.g., Farm, Stables, Sawmill, Gold Mine, Blacksmith).
        - Objective: Control points to generate resources/score. First to X resources wins.
    - **Unranked 32v32 (Alterac Valley Style - Stretch Goal):**
        - Two teams of 32 players.
        - Massive map with multiple objectives (capture points, destroying enemy structures, defeating mini-bosses).
        - Objective: Kill the enemy faction's main boss (e.g., a powerful Golem). Requires coordinated attacks on sub-objectives to weaken boss defenses. This is a significant stretch goal and might be simplified or deferred.
- **Bots:** Crucial for initial development and player base scaffolding. Bots should fill empty player slots, allowing for testing of larger game modes. Their AI behavior will be a core development task.

---

### 7. Initial Development Iteration Plan (MVP Focus)

**Phase 1: Local Prototyping (Client-Side Only)**

1.  **Project Setup:** Basic MonoGame project structure in Visual Studio Code.
2.  **Main Menu:** Implement a functional Main Menu with placeholder buttons.
3.  **Basic 3D Rendering:** Display a single white cube (player placeholder) in a 3D space with a simple camera.
4.  **Player Movement:** Implement WASD movement and mouse-look for the cube.
5.  **Character Creation (Placeholder):** A simple UI to select a base class (Brawler, Ranger, or Spellcaster).
6.  **Basic Environment Creation:** Create a simple flat ground plane with boundary walls for a training area.
7.  **Static Training Dummy Placement:** Manually place 2-3 static training dummy cubes at fixed positions.
8.  **Player Resource Bar (Class-Specific):** Display a simple UI bar representing the primary resource for the chosen class.
9.  **Click-to-Target System:** Implement functionality to click on a training dummy and visually indicate it is targeted.
10. **Placeholder Spell Bar UI:** Create a static UI element with 8 empty slots.
11. **Rudimentary Health System & Display:** Give the player and dummies a health value and display simple health bars.
12. **Implement ONE Basic Spell/Attack (Brawler - "Slam"):** Implement local-only logic for using a melee ability on a targeted dummy, including resource cost and cooldown.
13. **Basic "Death" State for Dummies:** When a dummy's health reaches zero, make it disappear.

**Phase 2: Backend Integration & Server-Authoritative Logic (PlayFab)**

14. **PlayFab Setup:** Create a free developer account and a new "Title" for Block Brawlers in the PlayFab Game Manager dashboard.
15. **SDK & Project Scaffolding:**
    * Integrate the PlayFab C# SDK into the client project.
    * Create two new projects: `project-axiom-server` (a Console Application) and `project-axiom-shared` (a Class Library).
    * Move shared data structures (Character, SpellData) into the `project-axiom-shared` project.
16. **MVP Authentication:**
    * In the client, create a basic UI for player registration and login.
    * Implement calls to `PlayFabClientAPI.RegisterPlayFabUser` and `LoginWithEmailAddress`.
    * **Note:** This is a placeholder system for MVP. No password reset or advanced features are needed. PlayFab handles the security automatically.
17. **Basic Player Data:**  - Upon successful login, store and retrieve the player's chosen class using PlayFab's Player Data system (`UpdateUserData`, `GetUserData`). This replaces local save files for character data.
    * Implemented character saving to PlayFab when creating a new character
    * Added `CharacterSelectionState` that automatically loads saved character data
    * Shows character selection screen if a character exists, otherwise prompts for character creation
    * Added character deletion functionality for starting over
    * Updated game flow: Main Menu → Character Selection → Character Creation (if needed) → Training Grounds
18. **Prepare the Dedicated Server:** Integrate the PlayFab Game Server SDK into the `project-axiom-server` project. This allows the server executable to communicate its health and status to the PlayFab agent.
19. **Local Server Testing:**
    * Download and run the **`LocalMultiplayerAgent`** tool provided by Microsoft.
    * Launch your `project-axiom-server.exe` locally.
    * The agent will simulate the PlayFab environment, allowing for rapid local testing and debugging of the dedicated server.
20. **Server Onboarding in PlayFab:**
    * Package the `project-axiom-server` build into a .zip file.
    * In the PlayFab dashboard, create a new Multiplayer Server "Build" and upload the zip file.
    * Configure the build to run on a specific VM size (e.g., a small Dv2) and define network ports.
21. **Basic Server Allocation:** ✅ **COMPLETE**
    * In the client, when the player wants to enter the "Training Grounds," call `PlayFabClientAPI.RequestMultiplayerServer`.
    * This will request PlayFab to find an available server from the fleet you configured.
    * On success, PlayFab returns the IP address and port of the allocated server. The client then connects to this endpoint.
22. **Authoritative Movement:** ✅ **COMPLETE** - Implement the server-authoritative movement as described in the original steps 26-28. The client sends inputs, the server processes them and holds the true position, and the client reconciles its predicted state with the server's authoritative state.
    * **Client-side prediction:** `ClientMovementSystem` handles local movement prediction
    * **Server authority:** `ServerMovementSystem` processes inputs and maintains authoritative positions
    * **Client reconciliation:** Client receives server updates and reconciles with prediction
    * **Network protocol:** JSON-based message system for input/position updates
    * **Anti-cheat:** Basic movement validation on server side
23. **Authoritative Targeting & Spells:** Re-implement the "Slam" ability to be fully server-authoritative as described in the original steps 30-31. The client sends a *request* to cast a spell, and the server validates and executes it.
24. **Server-Side Player State:** Move all critical player state (health, resource, cooldowns, position) to the server. The client's UI should only reflect the state sent by the server.
25. **Implement Remaining Spells:** Implement the basic attacks for Ranger ("Multi-shot") and Spellcaster ("Magic Missile") following the same server-authoritative pattern.
26. **Server Logging & Persistence:** Implement basic server-side logging. For the MVP, player state like health/position does not need to persist between sessions, but equipped spells (stored in PlayFab Player Data) should.
27. **Basic Player Respawn (Server-Side):** Move the death/respawn logic to the server. When a player's health reaches zero, the server manages their respawn timer and new position.

### 8. AI/LLM Integration Strategy

- **Code Generation:** Leverage LLMs (e.g., GitHub Copilot, dedicated prompts) to generate C# classes for:
    - `SpellData` structures, including boilerplate for various effects.
    - **Base `Character` class, with derived `Brawler`, `Ranger`, and `Spellcaster` classes implementing class-specific resources and abilities.**
    - `Mob` base classes with health, movement, and targeting logic.
    - UI element rendering and interaction.
    - Game state management.
    - Network serialization/deserialization.
    - Bot AI behaviors (pathfinding, decision trees).
- **Code Refinement & Optimization:** Ask LLMs to analyze existing C# code for performance bottlenecks, clarity, and adherence to best practices.
- **Debugging & Error Resolution:** Provide error messages and ask LLMs for potential causes and fixes.
- **Concept Expansion:** Prompt LLMs for ideas on new spell mechanics **within each class's theme**, game mode variations, or subtle gear stats once core systems are in place.
- **Documentation:** Generate comments, summaries, and internal documentation for complex code sections.
- **Testing Scenarios:** Generate test cases for spell interactions, physics, and bot behavior.
