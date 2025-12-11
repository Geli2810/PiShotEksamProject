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

            viewingProfile: null,
            IsProcessingAction: false,

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

        p1Data() { return this.liveStats.p1 || { id: null, visualScore: 0, name: 'P1', attempts: 0 }; },
        p2Data() { return this.liveStats.p2 || { id: null, visualScore: 0, name: 'P2', attempts: 0 }; },
        isTiebreak() { return this.liveStats.isTiebreak; },

        // Visual helper: Indicates whose turn it is likely to be based on attempts
        isPlayer1Turn() {
            const p1Att = this.p1Data.attempts || 0;
            const p2Att = this.p2Data.attempts || 0;
            return p1Att === p2Att;
        }
    },
    mounted() {
        this.loadProfiles();
        this.loadNbaData();
        this.checkStatus(true);
        this.timer = setInterval(this.tick, 1000);
        this.tick();
    },
    beforeUnmount() {
        if (this.timer) {
            clearInterval(this.timer);
            this.timer = null;
        }
    },
    methods: {
        async tick() {
            // 1. If we are currently sending data, DO NOT fetch (prevents glitching)
            if (this.IsProcessingAction) {
                if (this.currentView) this.timer = setTimeout(this.tick, 1000);
                return;
            }

            // 2. Do the work
            await this.checkStatus();
            if (this.gameActive) await this.fetchLiveScores();

            // 3. Schedule the next tick only after work is done
            if (this.currentView) {
                this.timer = setTimeout(this.tick, 1000);
            }
        },

        // --- STATS & LEADERBOARD ---
        recalcStatsAndLeaderboard(profiles) {
            profiles.forEach(p => {
                const goals = p.goals ?? 0;
                const attempts = p.attempts ?? 0;
                const wins = p.wins ?? 0;
                const losses = p.losses ?? 0;
                const totalGames = wins + losses;

                p.goals = goals;
                p.attempts = attempts;
                p.wins = wins;
                p.losses = losses;
                p.accuracy = attempts > 0 ? Math.round((goals * 100.0) / attempts) : 0;
                p.winLossRatio = totalGames > 0 ? Math.round((wins * 100.0) / totalGames) : 0;
            });

            // Sort: Wins -> Accuracy -> Name
            profiles.sort((a, b) => {
                if (b.wins !== a.wins) return b.wins - a.wins;
                if (b.accuracy !== a.accuracy) return b.accuracy - a.accuracy;
                return a.name.localeCompare(b.name);
            });

            profiles.forEach((p, idx) => { p.rank = idx + 1; });
            return profiles;
        },

        // --- PROFILES MANAGEMENT ---
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

        openViewProfile(profile) {
            this.viewingProfile = profile;
        },

        openEditFromView() {
            const profileToEdit = this.viewingProfile;
            this.viewingProfile = null;
            this.openEditModal(profileToEdit);
        },

        async deleteProfileFromView() {
            if(!this.viewingProfile) return;
            await this.deleteProfile(this.viewingProfile.id);
            this.viewingProfile = null;
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

        // --- GAME LOGIC ---
        async checkStatus(logOnError = false) {
            try {
                // Fetch Game Status (Active? Winner Declared?)
                const res = await axios.get(`${this.apiUrl}/game/current`, {
                    params: { t: Date.now() }
                });
                const d = res.data;

                // Handle C# PascalCase vs camelCase
                const isActive = d.isActive ?? d.IsActive ?? false;

                if (isActive) {
                    this.gameActive = true;
                    const winId = d.currentWinnerId ?? d.CurrentWinnerId;
                    
                    if (winId) {
                        // WINNER FOUND! 
                        // We rely entirely on the API/Python script to tell us this.
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
                    this.liveStats = { isTiebreak: false, p1: {}, p2: {} };
                }
            } catch (e) {
                if (logOnError) {
                    console.error("Fejl i checkStatus:", e);
                }
            }
        },

        async fetchLiveScores() {
            if (this.IsProcessingAction) return;

            try {
                const res = await axios.get(`${this.apiUrl}/scores/live`, {
                    params: { t: Date.now() }
                });

                if (this.IsProcessingAction) return; 

                this.liveStats = res.data;

                // NY: AUTO "FIRST TO 5"
                const p1 = this.p1Data;
                const p2 = this.p2Data;

                // Kun i normal fase (ikke tiebreak) og hvis vi ikke allerede HAR en vinder
                if (p1.visualScore >= 5 && p2.visualScore < 5) {
                    if (p1.attempts === p2.attempts) {
                        await this.autoDeclareWinner('p1');
                    }
                }
                else if (p2.visualScore >= 5 && p1.visualScore < 5) {
                    if (p2.attempts === p1.attempts) {
                        await this.autoDeclareWinner('p2');
                    }
                }
            } catch (e) {
                console.error("Fejl i fetchLiveScores:", e);
            }
        },

        async finishGame() {
            try {
                // "RECORD RESULT" button triggers this
                if(this.winner) {
                     await axios.post(`${this.apiUrl}/game/finish`, {
                        winnerId: this.winner.id
                    });
                } else {
                     await axios.post(`${this.apiUrl}/game/stop`);
                }
            } catch (e) {
                console.error("Fejl i finishGame:", e);
                alert("Kunne ikke afslutte kampen");
                return;
            }

            this.gameActive = false;
            this.winner = null;
            this.liveStats = { isTiebreak: false, p1: {}, p2: {} };
            this.currentView = 'lobby';
            setTimeout(this.loadProfiles, 1000);
        },

        async loadProfiles() {
            try {
                const res = await axios.get(`${this.apiUrl}/profiles`, {
                    params: { t: Date.now() }
                });
                let profiles = Array.isArray(res.data) ? res.data : [];
                profiles = this.recalcStatsAndLeaderboard(profiles);
                this.profiles = profiles;

                if (this.statsProfileId && !this.profiles.find(p => p.id === this.statsProfileId)) {
                    this.statsProfileId = null;
                    this.nbaTwin = null;
                } else if (this.statsProfileId) {
                    this.calculateNbaTwin();
                }
            } catch (e) {
                console.error("Fejl i loadProfiles:", e);
            }
        },

        async loadNbaData() {
            try {
                const res = await axios.get('nba_2025.json');
                this.nbaData = res.data;
            } catch (e) {
                console.warn("Kunne ikke loade NBA-data", e);
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
                await axios.post(`${this.apiUrl}/game/start`, payload);
                await this.checkStatus(true);
            } catch (e) {
                console.error("Fejl i startGame:", e);
                alert("Kunne ikke starte spillet");
            }
        },

        getProfile(id) {
            return this.profiles.find(p => p.id === id) || { accuracy: 0, profileImage: "", name: "" };
        },

        isLeader(data) {
            if (data.visualScore == null) return false;
            return data.visualScore >= (this.p1Data.visualScore || 0) &&
                   data.visualScore >= (this.p2Data.visualScore || 0);
        },

        // --- MANUAL CONTROLS (Only for fallback/testing) ---
        async addAttempt(side) {
            if (!this.gameActive || this.winner) return;
            const player = side === 'p1' ? this.p1Data : this.p2Data;
            if (!player.id) return;

            this.IsProcessingAction = true;
            try {
                await axios.post(`${this.apiUrl}/scores/shot_attempt`, { profileId: player.id });
            } catch (e) {
                alert(e.response?.data?.msg || "Fejl ved attempt");
            } finally {
                setTimeout(async () => { await this.fetchLiveScores(); this.IsProcessingAction = false; }, 500);
            }
        },

        async addScore(side) {
            if (!this.gameActive || this.winner) return;
            const player = side === 'p1' ? this.p1Data : this.p2Data;
            if (!player.id) return;

            this.IsProcessingAction = true;
            try {
                await axios.post(`${this.apiUrl}/scores`, { profileId: player.id });
            } catch (e) {
                console.error(e);
            } finally {
                setTimeout(async () => { await this.fetchLiveScores(); this.IsProcessingAction = false; }, 500);
            }
        },

        async declareWinner(side) {
            if (!this.gameActive) return;
            const player = side === 'p1' ? this.p1Data : this.p2Data;
            if (!confirm(`Manual Override: Declare ${player.name} winner?`)) return;

            try {
                await axios.post(`${this.apiUrl}/game/declare_winner`, { winnerId: player.id });
                await this.checkStatus(true);
            } catch (e) {
                alert("Kunne ikke declare winner");
            }
        },

        async stopGame() {
            if (!confirm("Stoppe den aktuelle kamp uden at gemme resultat?")) return;
            try {
                await axios.post(`${this.apiUrl}/game/stop`);
                await this.checkStatus(true);
            } catch (e) {
                alert("Kunne ikke stoppe game");
            }
        }
    }
}).mount('#app');