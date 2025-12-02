const API_BASE_URL = 'https://pishot-project-hqd6ffa0gvejbufu.canadacentral-01.azurewebsites.net/api/Profile';

const app = Vue.createApp({
    data() {
        return {
            profiles: [],
            newProfile: {
                name: '',
                profileImagePath: ''
            },
            loading: false,
            message: '',
            messageType: '',
        };
    },
    mounted() {
        this.fetchProfiles();
    },
    methods: {
        // READ: Get all profiles (HTTP GET)
        async fetchProfiles() {
            this.loading = true;
            this.message = '';
            try {
                const response = await axios.get(API_BASE_URL);

                if (response.status === 204 || response.data === null) {
                    this.profiles = [];
                } else {
                    this.profiles = response.data;
                }
            } catch (error) {
                console.error("Error fetching profiles:", error);
                const errorMsg = error.response?.data?.title || error.response?.data?.message || error.message;
                this.setMessage(`Failed to fetch profiles: ${errorMsg}`, 'error');
            } finally {
                this.loading = false;
            }
        },

        // CREATE: Add a new profile (HTTP POST)
        async createProfile() {
            // **RETTELSE 1: Validering af tomme mellemrum**
            if (!this.newProfile.name || !this.newProfile.name.trim()) {
                this.setMessage("Profile Name is required and cannot be just whitespace.", 'error');
                return;
            }
            
            // **RETTELSE 2: Brug .trim() for at fjerne overflødige mellemrum i den data, der sendes**
            const trimmedName = this.newProfile.name.trim();

            // **RETTELSE 3: PascalCase i ProfileImagePath bevares**
            const payload = {
                Name: trimmedName,
                ProfileImagePath: this.newProfile.profileImagePath || '' 
            };

            try {
                console.log('Sending Payload:', payload);
                const response = await axios.post(API_BASE_URL, payload);

                // API returns Status 201 Created with the new Profile object
                const createdProfile = response.data;
                this.profiles.push(createdProfile); // Add to the local list
                
                // Nulstil formen
                this.newProfile.name = ''; 
                this.newProfile.profileImagePath = '';
                this.setMessage(`Profile '${createdProfile.name}' created successfully!`, 'success');
            } catch (error) {
                console.error("Error creating profile:", error);
                const errorData = error.response?.data || {};
                const errorMsg = errorData.title || errorData.message || error.message;
                this.setMessage(`Failed to create profile: ${errorMsg}`, 'error');
            }
        },

        // Delete a profile (HTTP DELETE)
        async deleteProfile(profileId) {
            if (!confirm(`Are you sure you want to delete profile ID ${profileId}?`)) {
                return;
            }

            try {
                const response = await axios.delete(`${API_BASE_URL}/${profileId}`);

                // API returns the deleted profile object
                const deletedProfile = response.data;

                // Remove thr profile from the local array
                this.profiles = this.profiles.filter(p => p.id !== profileId);
                this.setMessage(`Profile '${deletedProfile.name}' (ID: ${profileId}) deleted successfully.`, 'success');
            } catch (error) {
                console.error("Error deleting profile:", error);
                const errorMsg = error.response ? `Failed with status: ${error.response.status}` : error.message;
                this.setMessage(`Failed to delete profile: ${errorMsg}`, 'error');
            }
        },

        // UPDATE: Update a profile (HTTP PUT)
        async updateProfile(profileId) {
                const profile = this.profiles.find(p => p.id === profileId);
                if (!profile) {
                    this.setMessage(`Profile with ID ${profileId} not found.`, 'error');
                    return;
                }
                const newName = prompt("Enter new profile name:", profile.name);
                if (newName === null || !newName.trim()) {
                    this.setMessage("Profile name update cancelled or invalid.", 'error');
                    return;
                }
                const payload = {
                    Name: newName.trim(),
                    ProfileImagePath: profile.profileImagePath || ''
                };
                try {
                    const response = await axios.put(`${API_BASE_URL}/${profileId}`, payload);
                    const updatedProfile = response.data;
                    // Update local profile data
                    const index = this.profiles.findIndex(p => p.id === profileId);
                    if (index !== -1) {
                        this.profiles[index] = updatedProfile;
                    }
                    this.setMessage(`Profile '${updatedProfile.name}' updated successfully!`, 'success');
                } catch (error) {
                    console.error("Error updating profile:", error);
                    const errorData = error.response?.data || {};
                    const errorMsg = errorData.title || errorData.message || error.message;
                    this.setMessage(`Failed to update profile: ${errorMsg}`, 'error');
                }
            },

        // Helper function for showing temporary messages
        setMessage(msg, type) {
            this.message = msg;
            this.messageType = type;
            setTimeout(() => {
                this.message = '';
            }, 5000); // Clear message after 5 seconds
        }
    }
});
app.mount('#app');