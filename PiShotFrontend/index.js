const API_BASE_URL = '';

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
                const response = await fetch(API_BASE_URL);

                if (response.status === 204) {
                    this.profiles = [];
                } else if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                } else {
                    this.profiles = await response.json();
                }
            } catch (error) {
                console.error("Error fetching profiles:", error);
                this.setMessage(`Failed to fetch profiles: ${error.message}`, 'error');
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
                const response = await fetch(API_BASE_URL, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify(this.newProfile),
                });

                if (!response.ok) {
                    const errorData = await response.json().catch(() => ({}));
                    throw new Error(errorData.title || errorData.message || `Failed with status: ${response.status}`);
                }

                // API returns Status 201 Created with the new Profile object
                const createdProfile = await response.json();
                this.profiles.push(createdProfile); // Add to the local list
                this.newProfile.Name = ''; // Clear form
                this.newProfile.ProfileImagePath = '';
                this.setMessage(`Profile '${createdProfile.name}' created successfully!`, 'success');
            } catch (error) {
                console.error("Error creating profile:", error);
                this.setMessage(`Failed to create profile: ${error.message}`, 'error');
            }
        },

        // Delete a profile (HTTP DELETE)
        async deleteProfile(profileId) {
            if (!confirm(`Are you sure you want to delete profile ID ${profileId}?`)) {
                return;
            }

            try {
                const response = await fetch(`${API_BASE_URL}/${profileId}`, {
                    method: 'DELETE',
                });

                if (!response.ok) {
                    throw new Error(`Failed to delete profile with status: ${response.status}`);
                }

                // API returns the deleted profile object
                const deletedProfile = await response.json();

                // Remove thr profile from the local array
                this.profiles = this.profiles.filter(p => p.id !== profileId);
                this.setMessage(`Profile '${deletedProfile.name}' (ID: ${profileId}) deleted successfully.`, 'success');
            } catch (error) {
                console.error("Error deleting profile:", error);
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