# 🚢 Battleship Game

A simple **Battleship game** built using **React (Frontend)** and **.NET 8 API (Backend)**.  
The goal is to **fire shots, sink enemy ships, and win** by sinking both **hidden Destroyers**.  

- **BattleshipAPI** → Backend API that handles game logic and stores ship positions.  
- **BattleshipConsole** → Console version of the game (alternative to the web version).  
- **ReactApp** → Frontend UI where players fire shots on a 10x10 grid. 

🎯 How to Play
Click on the grid (React) or enter coordinates (Console) to fire shots.
Battleship (size 5) is visible, but Destroyers (size 4) are hidden.
The game will give feedback:
🔥 Hit → A ship was hit!
❌ Miss → You missed!
💀 Sunk → A ship has been destroyed!
Win Condition: The game ends when both Destroyers are sunk.
Click "Restart Game" to play again (React) or type "RESET" in the console version.

🛑 How to Stop the Game
Backend → Press Ctrl + C
Frontend → Press Ctrl + C
Console Game → Close the terminal or press Ctrl + C
