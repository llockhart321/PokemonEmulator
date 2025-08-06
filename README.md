🎯 Overview
Covenant Critters is a polished, physics-inspired 2D RPG that reinvents the monster-collection genre through a systems-driven approach to combat, state management, and environmental simulation. Built in Unity as an educational and portfolio-level project, it showcases advanced gameplay mechanics, modular code architecture, and efficient resource management.

Ideal for employers seeking candidates with strengths in game systems, turn-based logic, physics-style simulations, and clean Unity/C# architecture.

🌟 Project Highlights
🧠 Systems-Oriented Battle Engine: Turn-based combat modeled with state machines and stat-driven decision logic

🧭 Scene Persistence & Save Systems: Seamless transitions with persistent world states and creature data

🏗️ Modular Unity Architecture: Clean MVC-style codebase designed for scalability and maintainability

🧪 Simulated Combat Physics: Attack interactions designed with balance, type effectiveness, and damage modeling

🎨 20+ Unique Creatures: Each with distinct types, moves, and interaction rules — enabling emergent strategy

🚀 Features
🕹️ Gameplay Systems
System	Description
Turn-Based Combat	Tactical battles using multi-type moves, buffs/debuffs, and turn-based logic
Creature Simulation	Stat-driven creatures with behavior patterns and performance tradeoffs
Trainer AI	Rule-based enemy trainers with varying difficulty logic
Wild Encounter System	Simulated environment interactions triggering probabilistic encounters
Team & Inventory Management	Real-time roster switching and stateful inventory tracking

⚙️ Core Mechanics
Typed Movesets: Water, Fire, Grass, and Normal moves with strategic synergies

Battle State Machines: Finite state logic for combat flow (Action → Select → Resolve)

Persistent World State: Save/load system for player position, party data, and environmental flags

Dynamic UI: Fully responsive and animated HUD for combat and inventory

🎮 Physics & Simulation Relevance
This project models several simulation-style systems highly relevant to jobs involving physics:

Concept	Application
Finite State Machines	Used to drive deterministic combat logic
Stat Interactions	Simulate health, attack/defense, and type effectiveness
Event Systems	Model environmental triggers and conditional outcomes
Performance Optimization	Implemented object pooling, efficient scene transitions, and light-weight memory use

🧪 Sample System Logic
Battle State Machine
csharp
Copy
Edit
enum BattleState { 
    Start, 
    ActionSelection, 
    MoveSelection, 
    PerformMove, 
    EnemyMove, 
    Busy, 
    BattleOver 
}
Creature Stats Model
csharp
Copy
Edit
[CreateAssetMenu(fileName = "NewPokemon", menuName = "Pokemon/Create New Pokemon")]
public class Pokemon : ScriptableObject
{
    public string pokeName;
    public float baseHP, baseAttack, baseDefense;
    public string typeClass;
    public List<Attack> attacks;
}
📁 Architecture Snapshot
bash
Copy
Edit
Covenant_Critters/
├── Scripts/
│   ├── BattleManager.cs       # Turn-based combat state machine
│   ├── GameManager.cs         # Scene/state controller
│   ├── Pokemon.cs             # Creature simulation data
│   └── PlayerMovement.cs      # Character physics and navigation
├── Scenes/
│   ├── StartMenuScene.unity
│   ├── BattleScene.unity
│   └── InventoryScene.unity
├── Pokes/                     # ScriptableObject creature configs
├── Sprites/                   # UI and game art
├── Animations/                # Sprite and creature animations
🧰 Tech Stack
Engine: Unity 2022.3 LTS

Language: C# with UnityEngine API

IDE: Visual Studio 2022 / VS Code

Version Control: Git

🔬 Development Practices
Area	Description
Modularity	Clear separation of systems for combat, UI, state, and inventory
Code Standards	PascalCase naming, XML doc comments, safe null checks
Testing	Manual testing + planned unit coverage for logic-heavy systems
Scalability	Easily extensible type system and state handlers

📷 Screenshots
Coming soon: Battle UI, creature detail views, exploration scenes.

🧑‍💻 Ideal For Job Applications In:
Physics-Based Game Systems

Simulation Programming

Gameplay Engineering

AI/Turn-Based Strategy Systems

Unity Game Development

📄 License & Credits
Created as part of an educational portfolio. All art assets and fonts sourced from royalty-free repositories or created by the dev team.
