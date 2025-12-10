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

        // NY: default-object har også attempts
        p1Data() { return this.liveStats.p1 || { id: null, visualScore: 0, name: 'P1', attempts: 0 }; },
        p2Data() { return this.liveStats.p2 || { id: null, visualScore: 0, name: 'P2', attempts: 0 }; },
        isTiebreak() { return this.liveStats.isTiebreak; }
    },
    mounted() {
        this.loadProfiles();
        this.loadNbaData();
        this.checkStatus(true);          // første load: tjek status én gang
        this.timer = setInterval(this.tick, 1000);
    },
    beforeUnmount() {
        if (this.timer) {
            clearInterval(this.timer);
            this.timer = null;
        }
    },
    methods: {
        async tick() {
            await this.checkStatus();
            if (this.gameActive) await this.fetchLiveScores();
        },

        // --- HJÆLPER: beregn stats + leaderboard i frontend ---
        recalcStatsAndLeaderboard(profiles) {
            // 1) Beregn accuracy og win% for hver profil
            profiles.forEach(p => {
                const goals = p.goals ?? 0;
                const attempts = p.attempts ?? 0;
                const wins = p.wins ?? 0;
                const losses = p.losses ?? 0;

                const accuracy =
                    attempts > 0 ? Math.round((goals * 100.0) / attempts) : 0;

                const totalGames = wins + losses;
                const winLossRatio =
                    totalGames > 0 ? Math.round((wins * 100.0) / totalGames) : 0;

                p.goals = goals;
                p.attempts = attempts;
                p.wins = wins;
                p.losses = losses;
                p.accuracy = accuracy;
                p.winLossRatio = winLossRatio;
            });

            // 2) Sorter (flest wins -> accuracy -> navn)
            profiles.sort((a, b) => {
                if (b.wins !== a.wins) return b.wins - a.wins;
                if (b.accuracy !== a.accuracy) return b.accuracy - a.accuracy;
                return a.name.localeCompare(b.name);
            });

            // 3) Sæt rank efter sortering
            profiles.forEach((p, idx) => {
                p.rank = idx + 1;
            });

            return profiles;
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
                    this.liveStats = { isTiebreak: false, p1: {}, p2: {} };
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

                // NY: AUTO "FIRST TO 5"
                const p1 = this.p1Data;
                const p2 = this.p2Data;

                // Kun i normal fase (ikke tiebreak) og hvis vi ikke allerede HAR en vinder
                if (this.gameActive && !this.isTiebreak && !this.winner) {
                    if (p1.visualScore >= 5 && p2.visualScore < 5) {
                        await this.autoDeclareWinner('p1');
                    } else if (p2.visualScore >= 5 && p1.visualScore < 5) {
                        await this.autoDeclareWinner('p2');
                    }
                }
            } catch (e) {
                console.error("Fejl i fetchLiveScores:", e);
            }
        },

        // NY: bruges kun internt til auto-stop, ingen confirm/alert
        async autoDeclareWinner(side) {
            const player = side === 'p1' ? this.p1Data : this.p2Data;
            if (!player.id) return;

            try {
                await axios.post(`${this.apiUrl}/game/declare_winner`, {
                    winnerId: player.id
                });
                await this.checkStatus(true);
            } catch (e) {
                console.error("Fejl i autoDeclareWinner:", e);
            }
        },

        // RECORD RESULT → stop game
        async finishGame() {
            try {
                await axios.post(`${this.apiUrl}/game/stop`);
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

                // rå data fra API
                let profiles = Array.isArray(res.data) ? res.data : [];

                // flyt stats-logik til frontend
                profiles = this.recalcStatsAndLeaderboard(profiles);

                this.profiles = profiles;

                // hvis valgt stats-profil ikke længere findes, reset
                if (this.statsProfileId && !this.profiles.find(p => p.id === this.statsProfileId)) {
                    this.statsProfileId = null;
                    this.nbaTwin = null;
                } else if (this.statsProfileId) {
                    // opdater NBA twin hvis en profil allerede er valgt
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
            return this.profiles.find(p => p.id === id) || { accuracy: 0, profileImage: "", name: "" };
        },

        isLeader(data) {
            if (data.visualScore == null) return false;
            return data.visualScore >= (this.p1Data.visualScore || 0) &&
                   data.visualScore >= (this.p2Data.visualScore || 0);
        },

        // --- LIVE GAME CONTROL PANEL (bruges i game-viewet) ---
        async addAttempt(side) {
            if (!this.gameActive) return alert("Der er ingen aktiv kamp");
            if (this.winner) return alert("Kampen er allerede afgjort. Tryk 'RECORD RESULT' eller start en ny kamp.");

            const player = side === 'p1' ? this.p1Data : this.p2Data;
            if (!player.id) return alert("Spiller er ikke loaded endnu");

            try {
                await axios.post(`${this.apiUrl}/scores/shot_attempt`, {
                    profileId: player.id
                });
                // liveScore opdateres automatisk via tick -> fetchLiveScores
            } catch (e) {
                console.error("Fejl i addAttempt:", e);
                const msg = e.response?.data?.msg || "Kunne ikke sende attempt";
                alert(msg);
            }
        },

        async addScore(side) {
            if (!this.gameActive) return alert("Der er ingen aktiv kamp");
            if (this.winner) return alert("Kampen er allerede afgjort. Tryk 'RECORD RESULT' eller start en ny kamp.");

            const player = side === 'p1' ? this.p1Data : this.p2Data;
            if (!player.id) return alert("Spiller er ikke loaded endnu");

            try {
                await axios.post(`${this.apiUrl}/scores`, {
                    profileId: player.id
                });
                // liveScore opdateres automatisk via tick -> fetchLiveScores
            } catch (e) {
                console.error("Fejl i addScore:", e);
                const msg = e.response?.data?.msg || "Kunne ikke sende score";
                alert(msg);
            }
        },

        async declareWinner(side) {
            if (!this.gameActive) return alert("Der er ingen aktiv kamp");
            const player = side === 'p1' ? this.p1Data : this.p2Data;
            if (!player.id) return alert("Spiller er ikke loaded endnu");

            if (!confirm(`Erklær ${player.name} som vinder?`)) return;

            try {
                await axios.post(`${this.apiUrl}/game/declare_winner`, {
                    winnerId: player.id
                });
                await this.checkStatus(true);
                alert(`${player.name} er erklæret som vinder`);
            } catch (e) {
                console.error("Fejl i declareWinner:", e);
                alert("Kunne ikke declare winner");
            }
        },

        async stopGame() {
            if (!confirm("Stoppe den aktuelle kamp uden at gemme resultat?")) return;
            try {
                await axios.post(`${this.apiUrl}/game/stop`);
                await this.checkStatus(true);
                this.gameActive = false;
                this.winner = null;
                this.liveStats = { isTiebreak: false, p1: {}, p2: {} };
                this.currentView = 'lobby';
                alert("Game stoppet");
            } catch (e) {
                console.error("Fejl i stopGame:", e);
                alert("Kunne ikke stoppe game");
            }
        },

        // --- GAMLE DEV TOOLS (kan stadig bruges fra dev-tab) ---
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
