const { createApp } = Vue;

createApp({
    data() {
        return {
            // VERIFY THIS URL IS CORRECT
            apiUrl: "https://pishot-project-hqd6ffa0gvejbufu.canadacentral-01.azurewebsites.net/api",
            
            gameActive: false,
            profiles: [],
            
            // Lobby Selection
            newName: "",
            newImageBase64: "", 
            selectedP1: null,
            selectedP2: null,
            
            // Live Data
            liveStats: [],
            winner: null,
            timer: null,
            
            activeP1_Id: null,
            activeP2_Id: null
        };
    },
    computed: {
        p1Profile() { return this.getProfile(this.activeP1_Id); },
        p2Profile() { return this.getProfile(this.activeP2_Id); },
        
        p1Data() { return this.liveStats.find(p => p.id === this.activeP1_Id) || { score: 0 }; },
        p2Data() { return this.liveStats.find(p => p.id === this.activeP2_Id) || { score: 0 }; }
    },
    mounted() {
        this.loadProfiles();
        // Start the heartbeat (Every 1 second)
        this.timer = setInterval(this.tick, 1000);
    },
    methods: {
        async tick() {
            // Check status every second
            await this.checkStatus();

            if (this.gameActive) {
                await this.fetchLiveScores();
            }
        },

        async checkStatus() {
            try {
                // FIX 1: Add timestamp to prevent caching (?t=...)
                const r = await fetch(`${this.apiUrl}/game/current?t=${Date.now()}`);
                const d = await r.json();
                
                // Debugging: See exactly what the server sends
                // console.log("Server Status:", d); 

                // FIX 2: Check both casing styles (isActive or IsActive)
                const isLive = d.isActive || d.IsActive;

                if (isLive) {
                    this.activeP1_Id = d.p1_id || d.P1_id; // Check both casings
                    this.activeP2_Id = d.p2_id || d.P2_id;
                    this.gameActive = true;
                } else {
                    this.gameActive = false;
                }
            } catch (e) {
                console.error("Status Check Error:", e);
            }
        },

        async loadProfiles() {
            try {
                const r = await fetch(`${this.apiUrl}/profiles?t=${Date.now()}`);
                this.profiles = await r.json();
            } catch (e) { console.error(e); }
        },
        
        getProfile(id) {
            return this.profiles.find(p => p.id === id) || { name: "Loading...", profileImage: "" };
        },
        
        handleFileUpload(event) {
            const file = event.target.files[0];
            const reader = new FileReader();
            reader.onload = (e) => { this.newImageBase64 = e.target.result; };
            if(file) reader.readAsDataURL(file);
        },

        async createProfile() {
            if(!this.newName) return;
            await fetch(`${this.apiUrl}/profiles`, {
                method: 'POST',
                headers: {'Content-Type': 'application/json'},
                body: JSON.stringify({ 
                    name: this.newName,
                    profileImage: this.newImageBase64 
                })
            });
            this.newName = "";
            this.newImageBase64 = "";
            this.loadProfiles();
        },

        async startGame() {
            if(!this.selectedP1 || !this.selectedP2) return alert("Select 2 players");
            
            console.log(`Starting Game: P1=${this.selectedP1}, P2=${this.selectedP2}`);
            
            try {
                const response = await fetch(`${this.apiUrl}/game/start`, {
                    method: 'POST',
                    headers: {'Content-Type': 'application/json'},
                    // Ensure IDs are integers
                    body: JSON.stringify({ 
                        player1Id: parseInt(this.selectedP1), 
                        player2Id: parseInt(this.selectedP2) 
                    })
                });

                if(response.ok) {
                    console.log("Start Command Sent Successfully");
                    // We immediately force a status check to flip the screen
                    await this.checkStatus();
                } else {
                    alert("Server Error: Could not start game.");
                }
            } catch (e) {
                console.error("Start Game Error:", e);
                alert("Network Error: Could not start game.");
            }
        },

        async fetchLiveScores() {
            try {
                // Cache bust scores too
                const r = await fetch(`${this.apiUrl}/scores/live?t=${Date.now()}`);
                this.liveStats = await r.json();
                
                if(!this.winner) {
                    if(this.p1Data.score >= 5) this.winner = this.p1Profile;
                    if(this.p2Data.score >= 5) this.winner = this.p2Profile;
                }
            } catch (e) { console.error(e); }
        },

        async finishGame() {
            if(this.winner) {
                await fetch(`${this.apiUrl}/game/finish`, {
                    method: 'POST',
                    headers: {'Content-Type': 'application/json'},
                    body: JSON.stringify({ winnerId: this.winner.id })
                });
            }
            this.gameActive = false;
            this.winner = null;
            setTimeout(this.loadProfiles, 1000); 
        },
        
        isLeader(data) {
            if (!data.score) return false;
            return data.score >= this.p1Data.score && data.score >= this.p2Data.score;
        }
    }
}).mount('#app');