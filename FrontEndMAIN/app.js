const { createApp } = Vue;

createApp({
    data() {
        return {
            apiUrl: "https://pishot-project-hqd6ffa0gvejbufu.canadacentral-01.azurewebsites.net/api",
            
            currentView: 'lobby',
            gameActive: false,
            
            profiles: [], 
            nbaData: [],
            
            newName: "", 
            newImageBase64: "",
            
            selectedP1: null, 
            selectedP2: null,
            
            statsProfileId: null, 
            nbaTwin: null,
            
            liveStats: { isTiebreak: false, p1: {}, p2: {} },
            winner: null, 
            timer: null
        };
    },
    computed: {
        top10() { return this.profiles.slice(0, 10); },
        statsProfile() { return this.profiles.find(p => p.id === this.statsProfileId); },
        
        p1Data() { return this.liveStats.p1 || { visualScore: 0, name: 'P1' }; },
        p2Data() { return this.liveStats.p2 || { visualScore: 0, name: 'P2' }; },
        isTiebreak() { return this.liveStats.isTiebreak; }
    },
    mounted() {
        this.loadProfiles(); 
        this.loadNbaData();
        // Run tick every 1 second
        this.timer = setInterval(this.tick, 1000);
    },
    methods: {
        async tick() {
            // 1. Check if Game is Active AND if there is a Winner
            await this.checkStatus();
            
            // 2. If Active, get scores
            if(this.gameActive) {
                await this.fetchLiveScores();
            }
        },

        async checkStatus() {
            try {
                // Add timestamp to prevent browser caching
                const r = await fetch(`${this.apiUrl}/game/current?t=${Date.now()}`);
                const d = await r.json();
                
                // --- DEBUGGING ---
                // Open Browser Console (F12) to see this if it fails
                // console.log("Game Status:", d); 

                const isActive = d.isActive || d.IsActive;
                
                if (isActive) {
                    this.gameActive = true;
                    
                    // FIX: Check BOTH casing styles (PascalCase vs camelCase)
                    const winId = d.currentWinnerId || d.CurrentWinnerId;
                    
                    if (winId) {
                        console.log("WINNER FOUND FROM API:", winId);
                        this.winner = {
                            id: winId,
                            name: d.winnerName || d.WinnerName,
                            profileImage: d.winnerImage || d.WinnerImage
                        };
                    }
                } else {
                    this.gameActive = false;
                    this.winner = null;
                }
            } catch(e){
                console.error("Status Check Error:", e);
            }
        },

        async fetchLiveScores() {
            try {
                const r = await fetch(`${this.apiUrl}/scores/live?t=${Date.now()}`);
                this.liveStats = await r.json();
            } catch(e){}
        },

        async finishGame() {
            if(this.winner) {
                console.log("Recording Result for Winner:", this.winner.id);
                await fetch(`${this.apiUrl}/game/finish`, {
                    method: 'POST', 
                    headers: {'Content-Type': 'application/json'},
                    body: JSON.stringify({winnerId: this.winner.id})
                });
            }
            this.gameActive = false; 
            this.winner = null; 
            
            // Switch back to lobby and reload stats
            this.currentView = 'lobby';
            setTimeout(this.loadProfiles, 1000);
        },

        // --- STANDARD METHODS (No changes needed below) ---
        async loadProfiles() {
            const r = await fetch(`${this.apiUrl}/profiles?t=${Date.now()}`);
            this.profiles = await r.json();
        },
        async loadNbaData() {
            try {
                const r = await fetch('nba_2025.json'); 
                this.nbaData = await r.json();
            } catch(e) {}
        },
        calculateNbaTwin() {
            if(!this.statsProfile || !this.nbaData.length) return;
            const uAcc = this.statsProfile.accuracy;
            this.nbaTwin = this.nbaData.reduce((prev, curr) => 
                (Math.abs(curr.accuracy - uAcc) < Math.abs(prev.accuracy - uAcc) ? curr : prev));
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
                method: 'POST', headers: {'Content-Type': 'application/json'},
                body: JSON.stringify({name: this.newName, profileImage: this.newImageBase64})
            });
            this.newName=""; this.newImageBase64=""; this.loadProfiles();
        },
        async startGame() {
            if(!this.selectedP1 || !this.selectedP2) return alert("Select 2 Players");
            await fetch(`${this.apiUrl}/game/start`, {
                method: 'POST', headers: {'Content-Type': 'application/json'},
                body: JSON.stringify({player1Id: parseInt(this.selectedP1), player2Id: parseInt(this.selectedP2)})
            });
            await this.checkStatus();
        },
        getProfile(id) { return this.profiles.find(p => p.id === id) || {accuracy:0}; },
        isLeader(data) { 
            if(!data.visualScore) return false;
            return data.visualScore >= this.p1Data.visualScore && data.visualScore >= this.p2Data.visualScore;
        }
    }
}).mount('#app');