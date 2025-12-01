const API_BASE_URL = 'https://pishot-project-hqd6ffa0gvejbufu.canadacentral-01.azurewebsites.net/api/Profile';

const app = Vue.createApp({
    data() {
        return {
            profiles: [],
            newProfile: {
                Name: '',
                ProfileImagePath: ''
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
                this.setMessage(`Failed to fetch profiles: ${errorMsg}`, 'error');
            } finally {
                this.loading = false;
            }
        },

        // CREATE: Add a new profile (HTTP POST)
        async createProfile() {
            if (!this.newProfile.Name) {
                this.setMessage("Profile Name is required.", 'error');
                return;
            }

            try {
                const response = await axios.post(API_BASE_URL, this.newProfile);

                // API returns Status 201 Created with the new Profile object
                const createdProfile = response.json();
                this.profiles.push(createdProfile); // Add to the local list
                this.newProfile.Name = ''; // Clear form
                this.newProfile.ProfileImagePath = '';
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
                const deletedProfile = response.json();

                // Remove thr profile from the local array
                this.profiles = this.profiles.filter(p => p.id !== profileId);
                this.setMessage(`Profile '${deletedProfile.name}' (ID: ${profileId}) deleted successfully.`, 'success');
            } catch (error) {
                console.error("Error deleting profile:", error);
                const errorMsg = error.response ? `Failed with status: ${error.response.status}` : error.message;
                this.setMessage(`Failed to delete profile: ${error.message}`, 'error');
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