const { createApp } = Vue;

createApp({
    data() {
        return {
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
        this.checkStatus(true);          // første load: tjek status én gang
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
            try {
                await axios.post(`${this.apiUrl}/profiles`, {
                    name: this.newName,
                    profileImage: this.newImageBase64
                });
                this.newName = "";
                this.newImageBase64 = ""; 
                await this.loadProfiles();
            } catch (e) {
                console.error("Fejl ved createProfile:", e);
                alert("Kunne ikke oprette profil");
            }
        },

        async deleteProfile(id) {
            if (!confirm("Are you sure you want to delete this player?")) return;
            try {
                await axios.delete(`${this.apiUrl}/profiles/${id}`);
                await this.loadProfiles();
                
                if (this.selectedP1 === id) this.selectedP1 = null;
                if (this.selectedP2 === id) this.selectedP2 = null;
            } catch (e) {
                console.error("Fejl ved deleteProfile:", e);
                alert("Kunne ikke slette profil");
            }
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
            try {
                await axios.put(`${this.apiUrl}/profiles/${this.editingProfile.id}`, payload);
                this.editingProfile = null;
                this.editImageBase64 = null;
                await this.loadProfiles();
            } catch (e) {
                console.error("Fejl ved saveProfileUpdate:", e);
                alert("Kunne ikke opdatere profil");
            }
        },

        // --- GAME LOGIK ---
        async checkStatus(logOnError = false) {
            try {
                const res = await axios.get(`${this.apiUrl}/game/current`, {
                    params: { t: Date.now() }
                });
                const d = res.data;
                // console.log("GAME STATUS:", d);

                const isActive = d.isActive ?? d.IsActive ?? false;
                
                if (isActive) {
                    this.gameActive = true;
                    const winId = d.currentWinnerId ?? d.CurrentWinnerId;
                    if (winId) {
                        this.winner = {
                            id: winId,
                            name: d.winnerName || d.WinnerName,
                            profileImage: d.winnerImage || d.WinnerImage
                        };
                    } else {
                        this.winner = null;
                    }
                } else {
                    this.gameActive = false;
                    this.winner = null;
                }
            } catch (e) {
                if (logOnError) {
                    console.error("Fejl i checkStatus:", e);
                    alert("Fejl ved hentning af game status. Tjek API-log.");
                }
            }
        },

        async fetchLiveScores() {
            try {
                const res = await axios.get(`${this.apiUrl}/scores/live`, {
                    params: { t: Date.now() }
                });
                this.liveStats = res.data;
            } catch (e) {
                console.error("Fejl i fetchLiveScores:", e);
            }
        },

        async finishGame() {
            try {
                if (this.winner) {
                    await axios.post(`${this.apiUrl}/game/finish`, {
                        winnerId: this.winner.id
                    });
                }
            } catch (e) {
                console.error("Fejl i finishGame:", e);
                alert("Kunne ikke afslutte kampen");
                return;
            }

            this.gameActive = false; 
            this.winner = null; 
            this.currentView = 'lobby';
            setTimeout(this.loadProfiles, 1000);
        },

        async loadProfiles() {
            try {
                const res = await axios.get(`${this.apiUrl}/profiles`, {
                    params: { t: Date.now() }
                });
                this.profiles = res.data;
            } catch (e) {
                console.error("Fejl i loadProfiles:", e);
            }
        },

        async loadNbaData() {
            try { 
                const res = await axios.get('nba_2025.json');
                this.nbaData = res.data;
            } catch(e){
                console.warn("Kunne ikke loade NBA-data (ok i udvikling)", e);
            }
        },

        calculateNbaTwin() {
            if (!this.statsProfile || !this.nbaData.length) return;
            const uAcc = this.statsProfile.accuracy;
            this.nbaTwin = this.nbaData.reduce((prev, curr) => 
                (Math.abs(curr.accuracy - uAcc) < Math.abs(prev.accuracy - uAcc) ? curr : prev));
        },

        async startGame() {
            if (!this.selectedP1 || !this.selectedP2) {
                alert("Select 2 Players");
                return;
            }

            try {
                const payload = {
                    player1Id: Number(this.selectedP1),
                    player2Id: Number(this.selectedP2)
                };
                console.log("StartGame payload:", payload);

                const res = await axios.post(`${this.apiUrl}/game/start`, payload);
                console.log("Game started OK:", res.data);

                await this.checkStatus(true);
            } catch (e) {
                console.error("Fejl i startGame:", e);
                alert("Kunne ikke starte spillet");
            }
        },

        getProfile(id) { 
            return this.profiles.find(p => p.id === id) || {accuracy:0, profileImage:"", name:""}; 
        },

        isLeader(data) { 
            if (data.visualScore == null) return false;
            return data.visualScore >= (this.p1Data.visualScore || 0) &&
                   data.visualScore >= (this.p2Data.visualScore || 0);
        },

        // --- DEV TOOLS ---
        async devAddAttempt() {
            if (!this.devProfileId) return alert("Select a player");
            try {
                await axios.post(`${this.apiUrl}/scores/shot_attempt`, {
                    profileId: this.devProfileId
                });
                alert("Attempt sendt til API");
            } catch (e) {
                console.error("Fejl i devAddAttempt:", e);
                alert("Kunne ikke sende attempt");
            }
        },

        async devAddScore() {
            if (!this.devProfileId) return alert("Select a player");
            try {
                await axios.post(`${this.apiUrl}/scores`, {
                    profileId: this.devProfileId
                });
                alert("Score sendt til API");
            } catch (e) {
                console.error("Fejl i devAddScore:", e);
                alert("Kunne ikke sende score");
            }
        },

        async devDeclareWinner() {
            if (!this.devProfileId) return alert("Select a player som vinder");
            try {
                await axios.post(`${this.apiUrl}/game/declare_winner`, {
                    winnerId: this.devProfileId
                });
                alert("Winner declared");
            } catch (e) {
                console.error("Fejl i devDeclareWinner:", e);
                alert("Kunne ikke declare winner");
            }
        },

        async devFinishGame() {
            if (!this.devProfileId) return alert("Select a player som vinder");
            try {
                await axios.post(`${this.apiUrl}/game/finish`, {
                    winnerId: this.devProfileId
                });
                await this.checkStatus(true);
                alert("Game finish kaldt");
            } catch (e) {
                console.error("Fejl i devFinishGame:", e);
                alert("Kunne ikke finish game");
            }
        },

        async devStopGame() {
            try {
                await axios.post(`${this.apiUrl}/game/stop`);
                await this.checkStatus(true);
                alert("Game stoppet (stop endpoint)");
            } catch (e) {
                console.error("Fejl i devStopGame:", e);
                alert("Kunne ikke stoppe game");
            }
        }
    }
}).mount('#app');
