const { createApp } = Vue;

createApp({
    data() {
        return {
            // Tilpas hvis jeres base er anderledes
            apiUrl: "https://pishot-project-hqd6ffa0gvejbufu.canadacentral-01.azurewebsites.net/api",
            
            currentView: 'lobby', // lobby | profiles | stats | dev
            gameActive: false,
            
            profiles: [], 
            nbaData: [],
            
            // Create New vars
            newName: "", 
            newImageBase64: "",
            
            // Edit vars
            editingProfile: null,
            editImageBase64: null,

            // Match vars
            selectedP1: null, 
            selectedP2: null,
            
            // Stats vars
            statsProfileId: null, 
            nbaTwin: null,
            
            liveStats: { isTiebreak: false, p1: {}, p2: {} },
            winner: null, 
            timer: null,

            // Dev tools
            devProfileId: null
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
        this.timer = setInterval(this.tick, 1000);
    },
    methods: {
        async tick() {
            await this.checkStatus();
            if (this.gameActive) await this.fetchLiveScores();
        },

        // --- PROFILES MANAGEMENT (Create, Edit, Delete) ---
        handleFileUpload(event) {
            const file = event.target.files[0];
            const reader = new FileReader();
            reader.onload = (e) => { this.newImageBase64 = e.target.result; };
            if (file) reader.readAsDataURL(file);
        },
        async createProfile() {
            if (!this.newName) return;
            await fetch(`${this.apiUrl}/profiles`, {
                method: 'POST',
                headers: {'Content-Type': 'application/json'},
                body: JSON.stringify({name: this.newName, profileImage: this.newImageBase64})
            });
            this.newName = "";
            this.newImageBase64 = ""; 
            await this.loadProfiles();
        },
        async deleteProfile(id) {
            if (!confirm("Are you sure you want to delete this player?")) return;
            
            await fetch(`${this.apiUrl}/profiles/${id}`, {
                method: 'DELETE'
            });
            await this.loadProfiles();
            
            if (this.selectedP1 === id) this.selectedP1 = null;
            if (this.selectedP2 === id) this.selectedP2 = null;
        },
        openEditModal(profile) {
            this.editingProfile = { ...profile }; 
            this.editImageBase64 = null;
        },
        handleEditFileUpload(event) {
            const file = event.target.files[0];
            const reader = new FileReader();
            reader.onload = (e) => { this.editImageBase64 = e.target.result; };
            if (file) reader.readAsDataURL(file);
        },
        async saveProfileUpdate() {
            if (!this.editingProfile) return;
            
            const payload = {
                name: this.editingProfile.name,
                profileImage: this.editImageBase64 || this.editingProfile.profileImage
            };

            await fetch(`${this.apiUrl}/profiles/${this.editingProfile.id}`, {
                method: 'PUT',
                headers: {'Content-Type': 'application/json'},
                body: JSON.stringify(payload)
            });

            this.editingProfile = null;
            this.editImageBase64 = null;
            await this.loadProfiles();
        },

        // --- GAME LOGIK ---
        async checkStatus() {
            try {
                const r = await fetch(`${this.apiUrl}/game/current?t=${Date.now()}`);
                const d = await r.json();
                const isActive = d.isActive || d.IsActive;
                
                if (isActive) {
                    this.gameActive = true;
                    const winId = d.currentWinnerId || d.CurrentWinnerId;
                    if (winId) {
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
            } catch(e) {
                // Ignorer fejl i polling
            }
        },
        async fetchLiveScores() {
            try {
                const r = await fetch(`${this.apiUrl}/scores/live?t=${Date.now()}`);
                this.liveStats = await r.json();
            } catch(e){}
        },
        async finishGame() {
            if (this.winner) {
                await fetch(`${this.apiUrl}/game/finish`, {
                    method: 'POST',
                    headers: {'Content-Type': 'application/json'},
                    body: JSON.stringify({winnerId: this.winner.id})
                });
            }
            this.gameActive = false; 
            this.winner = null; 
            this.currentView = 'lobby';
            setTimeout(this.loadProfiles, 1000);
        },
        async loadProfiles() {
            const r = await fetch(`${this.apiUrl}/profiles?t=${Date.now()}`);
            this.profiles = await r.json();
        },
        async loadNbaData() {
            try { 
                const r = await fetch('nba_2025.json'); 
                this.nbaData = await r.json(); 
            } catch(e){}
        },
        calculateNbaTwin() {
            if (!this.statsProfile || !this.nbaData.length) return;
            const uAcc = this.statsProfile.accuracy;
            this.nbaTwin = this.nbaData.reduce((prev, curr) => 
                (Math.abs(curr.accuracy - uAcc) < Math.abs(prev.accuracy - uAcc) ? curr : prev));
        },
        async startGame() {
            if (!this.selectedP1 || !this.selectedP2) return alert("Select 2 Players");
            await fetch(`${this.apiUrl}/game/start`, {
                method: 'POST',
                headers: {'Content-Type': 'application/json'},
                body: JSON.stringify({
                    player1Id: parseInt(this.selectedP1),
                    player2Id: parseInt(this.selectedP2)
                })
            });
            await this.checkStatus();
        },
        getProfile(id) { 
            return this.profiles.find(p => p.id === id) || {accuracy:0, profileImage:"", name:""}; 
        },
        isLeader(data) { 
            if (data.visualScore == null) return false;
            return data.visualScore >= (this.p1Data.visualScore || 0) &&
                   data.visualScore >= (this.p2Data.visualScore || 0);
        },

        // --- DEV TOOLS: kalder kun eksisterende endpoints ---
        async devAddAttempt() {
            if (!this.devProfileId) return alert("Select a player");
            await fetch(`${this.apiUrl}/scores/shot_attempt`, {
                method: 'POST',
                headers: {'Content-Type': 'application/json'},
                body: JSON.stringify({profileId: this.devProfileId})
            });
            alert("Attempt sendt til API");
        },
        async devAddScore() {
            if (!this.devProfileId) return alert("Select a player");
            await fetch(`${this.apiUrl}/scores`, {
                method: 'POST',
                headers: {'Content-Type': 'application/json'},
                body: JSON.stringify({profileId: this.devProfileId})
            });
            alert("Score sendt til API");
        },
        async devDeclareWinner() {
            if (!this.devProfileId) return alert("Select a player som vinder");
            await fetch(`${this.apiUrl}/game/declare_winner`, {
                method: 'POST',
                headers: {'Content-Type': 'application/json'},
                body: JSON.stringify({winnerId: this.devProfileId})
            });
            alert("Winner declared");
        },
        async devFinishGame() {
            if (!this.devProfileId) return alert("Select a player som vinder");
            await fetch(`${this.apiUrl}/game/finish`, {
                method: 'POST',
                headers: {'Content-Type': 'application/json'},
                body: JSON.stringify({winnerId: this.devProfileId})
            });
            await this.checkStatus();
            alert("Game finish kaldt");
        },
        async devStopGame() {
            await fetch(`${this.apiUrl}/game/stop`, {
                method: 'POST'
            });
            await this.checkStatus();
            alert("Game stoppet (stop endpoint)");
        }
    }
}).mount('#app');
