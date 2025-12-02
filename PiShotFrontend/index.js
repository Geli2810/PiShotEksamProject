// index.js (Modified)
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
            
            hardcodedImages: [
            'https://images2.minutemediacdn.com/image/upload/c_crop,x_0,y_455,w_4261,h_2396/c_fill,w_1440,ar_16:9,f_auto,q_auto,g_auto/images%2FGettyImages%2Fmmsport%2F169%2F01k7fgbtnxw9888f4nw2.jpg',
            'https://core.opendorse.com/profile/691167/20241016145855_smu2.jpeg',
            'https://cdn.britannica.com/10/258410-050-88518061/NAB-basketball-Kawhi-Leonard-LA-Clippers-free-throw-against-Dallas-Mavericks-2023.jpg?w=300',
            'https://upload.wikimedia.org/wikipedia/commons/thumb/7/7a/LeBron_James_%2851959977144%29_%28cropped2%29.jpg/960px-LeBron_James_%2851959977144%29_%28cropped2%29.jpg'
        ],
            selectedImage: '', // Tracks the currently selected hardcoded image
            profileToEdit: null,
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
            if (!this.newProfile.name) {
                this.setMessage("Profile Name is required.", 'error');
                return;
            }

            // --- 2. Logic to determine the final image path ---
            const finalImagePath = this.newProfile.profileImagePath.trim()
                ? this.newProfile.profileImagePath.trim()
                : this.selectedImage;

            const payload = {
                Name: this.newProfile.name,
                profileImagePath: finalImagePath || '' // Use the determined path
            };
            
            try {
                // ... (rest of createProfile method) ...
                console.log('Sending Payload:', payload);
                const response = await axios.post(API_BASE_URL, payload);

                const createdProfile = response.data;
                this.profiles.push(createdProfile);
                this.newProfile.name = ''; 
                this.newProfile.profileImagePath = '';
                this.selectedImage = ''; // Clear selection after creation
                this.setMessage(`Profile '${createdProfile.name}' created successfully!`, 'success');
            } catch (error) {
                // ... (error handling) ...
                console.error("Error creating profile:", error);
                const errorData = error.response?.data || {};
                const errorMsg = errorData.title || errorData.message || error.message;
                this.setMessage(`Failed to create profile: ${errorMsg}`, 'error');
            }
        },

        // Delete a profile (HTTP DELETE)
    async deleteProfile(profileId) {
      if (!confirm(`Are you sure you want to delete profile ${profile.Name}?`)) {
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

    isEditing(profileId) {
        return this.profileToEdit && this.profileToEdit.id === profileId;
    },

    startUpdate(profile) {
        this.profileToEdit = {
            ...profile,
            profileImagePath: profile.profileImagePath || ''
        };
    },

    cancelUpdate() {
        this.profileToEdit = null;
        this.setMessage("Profile update cancelled.", 'error');
    },

    async saveUpdate(profileId) {
            if (!this.profileToEdit || this.profileToEdit.id !== profileId) {
                this.setMessage("Error: Profile mismatch during save.", 'error');
                return;
            }

            const newName = this.profileToEdit.name.trim();

            if (!newName) {
                this.setMessage("Profile name cannot be empty.", 'error');
                return;
            }

            const payload = {
                Name: newName,
                // Use the image path selected in the editing controls
                ProfileImagePath: this.profileToEdit.profileImagePath || '' 
            };
            
            try {
                const response = await axios.put(`${API_BASE_URL}/${profileId}`, payload);
                const updatedProfile = response.data;
                
                // Update local profile data
                const index = this.profiles.findIndex(p => p.id === profileId);
                if (index !== -1) {
                    this.profiles[index] = updatedProfile;
                }

                this.profileToEdit = null; // Clear the editing state
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