import { useEffect, useState } from "react";

export default function BattleshipGame() {
    const GRID_SIZE = 10;
    const EMPTY = "⬜";
    const HIT = "🔥";
    const MISS = "❌";
    const SUNK = "💀";
    const SHIP = "🚢";

    const [grid, setGrid] = useState(
        Array(GRID_SIZE)
            .fill(null)
            .map(() => Array(GRID_SIZE).fill(EMPTY))
    );

    const [shipsLoaded, setShipsLoaded] = useState(false);
    const [destroyersSunk, setDestroyersSunk] = useState(0); // Track if ships are placed
    const [count, setCount] = useState(0); //  Track if ships are placed

    // Fetch Initial Ship Positions (Only When Button is Clicked)
    const fetchShips = async () => {
        try {
            const resetResponse = await fetch(
                "https://localhost:7080/api/Game/reset",
                {
                    method: "POST",
                }
            );

            if (!resetResponse.ok) throw new Error("Failed to reset game");

            console.log("✅ Game reset successful!");

            const response = await fetch(
                "https://localhost:7080/api/Game/ships"
            );
            if (!response.ok) throw new Error("Failed to fetch ships");
            const data = await response.json();

            console.log("🚢 Initial Ship Positions:", data);

            // Create a fresh copy of the grid
            const newGrid = grid.map((row) => [...row]);

            // Show ships before game starts
            data.forEach((ship) => {
                if (ship.originalPositions.length === 5) {
                    ship.originalPositions.forEach(([row, col]) => {
                        newGrid[row][col] = SHIP;
                    });
                }
            });
            // console.log("newgrid2", newGrid);

            setGrid(newGrid);
            setShipsLoaded(true); // Mark ships as loaded
        } catch (error) {
            console.error("❌ Error fetching ships:", error);
        }
    };

    // Handle Cell Clicks (Firing Shots)
    const handleCellClick = async (row, col) => {
        setCount((prev) => prev + 1);
        if (!shipsLoaded) {
            alert("⚠️ Ships are not loaded yet! Click 'Start Game' first.");
            return;
        }
        if (destroyersSunk === 2) {
            alert("🚫 Game is over! No more shots allowed.");
            return;
        }

        if (grid[row][col] !== EMPTY && grid[row][col] !== SHIP) {
            alert("⚠️ Already fired at this position:", row, col);
            return;
        }

        if (grid[row][col] === SHIP) {
            alert("🚫 You cannot fire at your own ship!");
            return;
        }

        const position = `${String.fromCharCode(65 + col)}${row + 1}`;
        console.log("🔄 Sending request to API:", position);

        try {
            const response = await fireShot(position);
            console.log("res", response);

            setGrid((prevGrid) => {
                const newGrid = prevGrid.map((row) => [...row]); // Ensure a fresh copy of grid

                if (response.sunk) {
                    response.sunkPositions.forEach(([sunkRow, sunkCol]) => {
                        newGrid[sunkRow][sunkCol] = SUNK;
                    });

                    if (response.sunkPositions.length === 4) {
                        setDestroyersSunk((prev) => prev + 1);
                    }
                } else if (response.hit) {
                    newGrid[row][col] = HIT;
                } else {
                    newGrid[row][col] = MISS;
                }

                return newGrid; // Ensures processes the update correctly
            });
        } catch (error) {
            console.error("❌ Error firing shot:", error);
        }
    };

    useEffect(() => {
        fetchShips();
    }, []);

    // useEffect(() => {
    //   console.log("max tries", count);
    //   if (count === 5) {
    //     setDestroyersSunk(2);
    //     alert("maximum tries reached");
    //   }
    // }, [count]);

    return (
        <div
            style={{
                display: "grid",
                placeItems: "center",
                height: "100px",
                textAlign: "center",
            }}
        >
            <h2>Battleship Game</h2>
            <p>
                Legend: 🚢 = Our Battleship, 💀 = Sunk Ship, 🔥 = Hit, ❌ = Miss
            </p>
            <div
                style={{
                    display: "inline-block",
                    border: "2px solid black",
                    padding: "10px",
                }}
            >
                {grid.map((row, rowIndex) => (
                    <div key={rowIndex}>
                        {row.map((cell, colIndex) => (
                            <button
                                key={colIndex}
                                onClick={() =>
                                    handleCellClick(rowIndex, colIndex)
                                }
                                style={{
                                    width: "40px",
                                    height: "40px",
                                    fontSize: "20px",
                                    margin: "2px",
                                    backgroundColor:
                                        cell === EMPTY
                                            ? "white"
                                            : "transparent",
                                }}
                            >
                                {cell}
                            </button>
                        ))}
                    </div>
                ))}
            </div>
            {destroyersSunk === 2 && (
                <div
                    style={{
                        marginTop: "20px",
                        fontSize: "24px",
                        fontWeight: "bold",
                        color: "green",
                    }}
                >
                    💥 BOOM! YOU SANK THEM ALL! 🎯
                    <button
                        onClick={() => window.location.reload()}
                        style={{
                            display: "block",
                            margin: "20px auto",
                            padding: "10px 20px",
                            fontSize: "18px",
                            fontWeight: "bold",
                            color: "white",
                            backgroundColor: "green",
                            border: "none",
                            borderRadius: "5px",
                            cursor: "pointer",
                        }}
                    >
                        🔄 Restart Game
                    </button>
                </div>
            )}
        </div>
    );
}

//  API Call Function (Connects to Backend)
const fireShot = async (position) => {
    try {
        console.log(
            "Sending request to API with:",
            JSON.stringify({ position })
        );

        const response = await fetch("https://localhost:7080/api/Game/fire", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ position }),
        });

        if (!response.ok) {
            throw new Error(
                `Failed to get response from API: ${response.status}`
            );
        }

        const data = await response.json();
        console.log("🔥 API Response:", data);

        return {
            hit: data.message.includes("Hit"),
            alreadyFired: data.message.includes("Already fired"),
            sunk: data.message.includes("Sunk"),
            sunkShipName: data.sunkShipName || "",
            sunkPositions: data.sunkPositions || [],
        };
    } catch (error) {
        console.error("❌ Error:", error);
        return { hit: false, sunk: false, sunkPositions: [] }; // Default to "Miss"
    }
};
