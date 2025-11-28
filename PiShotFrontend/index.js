const { createApp } = Vue;

createApp({
    data() {
        return {
            // --- CONFIGURATION ---
            // REPLACE THIS with your actual Azure API URL
            apiUrl: "https://pishot-project-hqd6ffa0gvejbufu.canadacentral-01.azurewebsites.net/api/scores/live",
            
            // --- STATE ---
            players: [],      // Stores [{name: "Alice", score: 2}, ...]
            loading: true,
            winner: null,
            connectionStatus: "Connecting...",
            pollingTimer: null,
            winningScore: 5   // Must match your Python Logic
        };
    },
    mounted() {
        // Run immediately when page loads
        this.fetchScores();
        
        // Then run every 1 second (1000ms)
        this.pollingTimer = setInterval(this.fetchScores, 1000);
    },
    beforeUnmount() {
        // Clean up timer if page closes (good practice)
        clearInterval(this.pollingTimer);
    },
    methods: {
        async fetchScores() {
            try {
                // 1. Fetch data from C# API
                const response = await fetch(this.apiUrl);
                
                if (!response.ok) {
                    throw new Error(`Server Error: ${response.status}`);
                }

                // 2. Parse JSON
                // API returns object like: { "Player 1": 3, "Player 2": 5 }
                const data = await response.json();

                // 3. Convert Object to Array for Vue List
                this.players = Object.entries(data).map(([key, value]) => {
                    return { name: key, score: value };
                });

                // 4. Sort alphabetically to keep names from jumping around
                this.players.sort((a, b) => a.name.localeCompare(b.name));

                // 5. Update Status
                this.connectionStatus = "Connected";
                this.loading = false;

                // 6. Check for Winner
                this.checkWinner();

            } catch (error) {
                console.error("Scoreboard Error:", error);
                this.connectionStatus = "Error - Retrying...";
            }
        },

        isLeader(player) {
            // Logic: Is this player's score the highest (and > 0)?
            if (player.score === 0) return false;
            
            // Find the highest score in the array
            const maxScore = Math.max(...this.players.map(p => p.score));
            return player.score === maxScore;
        },

        checkWinner() {
            // Find a player with score >= winningScore
            const winnerObj = this.players.find(p => p.score >= this.winningScore);
            
            if (winnerObj) {
                this.winner = winnerObj.name;
            } else {
                this.winner = null;
            }
        }
    }
}).mount('#app');