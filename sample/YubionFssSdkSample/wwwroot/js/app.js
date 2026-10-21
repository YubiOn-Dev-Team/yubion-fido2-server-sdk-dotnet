/* global Vue, VueRouter, yubionFido2Client */

const { createApp, ref, reactive, computed, onMounted } = Vue;
const { createRouter, createWebHistory, useRoute, useRouter } = VueRouter;

// Helper to safely access client SDK functions (from yubion-fido2-server-sdk-client.min.js)
function getClientSdk() {
    const sdk = window.yubionFido2Client || window;
    if (!sdk || typeof sdk.convertRequestOptionsToBinary !== 'function') {
        throw new Error('YubiOn FSS client SDK (yubion-fido2-server-sdk-client.min.js) is not loaded properly.');
    }
    return sdk;
}

function convertCreationOptionsToBinary(opts) {
    if (opts && typeof opts === 'object' && opts.excludeCredentials == null) {
        delete opts.excludeCredentials;
    }
    return getClientSdk().convertCreationOptionsToBinary(opts);
}

function convertRequestOptionsToBinary(opts) {
    if (opts && typeof opts === 'object' && opts.allowCredentials == null) {
        delete opts.allowCredentials;
    }
    return getClientSdk().convertRequestOptionsToBinary(opts);
}

function convertPublicKeyCredentialToJsonable(cred) {
    return getClientSdk().convertPublicKeyCredentialToJsonable(cred);
}

// Shared state
const store = reactive({
    user: null
});

// Helper for API calls
async function apiCall(url, options = {}) {
    options.headers = {
        'Content-Type': 'application/json',
        ...(options.headers || {})
    };
    options.credentials = 'same-origin';

    const res = await fetch(url, options);
    const data = await res.json().catch(() => ({}));

    if (!res.ok) {
        throw new Error(data.message || `Request failed with status ${res.status}`);
    }
    return data;
}

// -------------------------------------------------------------
// Component: LoginView
// -------------------------------------------------------------
const LoginView = {
    template: `
        <div class="container">
            <h1>Login</h1>
            <form @submit.prevent="login">
                <p v-if="errorMessage" class="error">{{ errorMessage }}</p>
                <button type="submit" class="primary" :disabled="loading">
                    {{ loading ? 'Authenticating...' : 'Login with Passkey' }}
                </button>
            </form>
            <div class="links">
                <router-link to="/register">Create a new account</router-link>
            </div>
        </div>
    `,
    setup() {
        const router = useRouter();
        const loading = ref(false);
        const errorMessage = ref('');

        const login = async () => {
            loading.value = true;
            errorMessage.value = '';
            try {
                // 1. Get challenge from server
                const data = await apiCall('/api/login/start', { method: 'POST' });
                const requestOptions = data.requestOptions;

                // 2. Convert options and call WebAuthn API
                const binaryOptions = convertRequestOptionsToBinary(requestOptions);
                const credential = await navigator.credentials.get({ publicKey: binaryOptions });

                if (!credential) {
                    throw new Error('Credential retrieval failed or was canceled.');
                }

                // 3. Convert result to JSON and send to server
                const jsonableCredential = convertPublicKeyCredentialToJsonable(credential);
                const resp = {
                    requestResponse: {
                        attestationResponse: jsonableCredential
                    }
                };

                await apiCall('/api/login/finish', {
                    method: 'POST',
                    body: JSON.stringify(resp)
                });

                // Update session state and navigate
                const session = await apiCall('/api/session');
                store.user = session.user;
                router.push('/users');
            } catch (err) {
                console.error(err);
                errorMessage.value = err.message || 'Login failed. Please try again.';
            } finally {
                loading.value = false;
            }
        };

        return { loading, errorMessage, login };
    }
};

// -------------------------------------------------------------
// Component: RegisterView
// -------------------------------------------------------------
const RegisterView = {
    template: `
        <div class="container">
            <h1>Register</h1>
            <form @submit.prevent="register">
                <div class="form-group">
                    <label for="name">Name (UserID):</label>
                    <input id="name" v-model="name" type="text" required maxlength="64" placeholder="e.g. user01">
                </div>
                <div class="form-group">
                    <label for="displayName">Display Name:</label>
                    <input id="displayName" v-model="displayName" type="text" required maxlength="64" placeholder="e.g. Taro Yamada">
                </div>
                <p v-if="errorMessage" class="error">{{ errorMessage }}</p>
                <button type="submit" class="primary" :disabled="loading">
                    {{ loading ? 'Registering...' : 'Register and Add Passkey' }}
                </button>
            </form>
            <div class="links">
                <router-link to="/">Back to Login</router-link>
            </div>
        </div>
    `,
    setup() {
        const router = useRouter();
        const name = ref('');
        const displayName = ref('');
        const loading = ref(false);
        const errorMessage = ref('');

        const register = async () => {
            loading.value = true;
            errorMessage.value = '';
            try {
                // 1. Get challenge from server
                const data = await apiCall('/api/register/start', {
                    method: 'POST',
                    body: JSON.stringify({ name: name.value, displayName: displayName.value })
                });
                const creationOptions = data.creationOptions;

                // 2. Convert options and call WebAuthn API
                const binaryOptions = convertCreationOptionsToBinary(creationOptions);
                const credential = await navigator.credentials.create({ publicKey: binaryOptions });

                if (!credential) {
                    throw new Error('Credential creation failed or was canceled.');
                }

                // 3. Convert result to JSON and send to server
                const jsonableCredential = convertPublicKeyCredentialToJsonable(credential);
                const transports = credential.response.getTransports ? credential.response.getTransports() : [];
                const resp = {
                    createResponse: {
                        attestationResponse: jsonableCredential,
                        transports: transports
                    }
                };

                await apiCall('/api/register/finish', {
                    method: 'POST',
                    body: JSON.stringify(resp)
                });

                alert('Registration successful! Please log in.');
                router.push('/');
            } catch (err) {
                console.error(err);
                errorMessage.value = err.message || 'Registration failed. Please try again.';
            } finally {
                loading.value = false;
            }
        };

        return { name, displayName, loading, errorMessage, register };
    }
};

// -------------------------------------------------------------
// Component: UserListView
// -------------------------------------------------------------
const UserListView = {
    template: `
        <div class="container wide">
            <h1>User List</h1>
            <p v-if="errorMessage" class="error">{{ errorMessage }}</p>
            <p v-if="loading">Loading users...</p>
            <ul v-else-if="users.length > 0" class="item-list">
                <li v-for="user in users" :key="user.userId">
                    <router-link :to="'/users/' + user.userId">
                        {{ user.displayName || user.userName }} ({{ user.userName }})
                    </router-link>
                    <span>Creds: {{ user.credentialCount ?? 0 }}</span>
                </li>
            </ul>
            <p v-else>No users registered yet.</p>
        </div>
    `,
    setup() {
        const users = ref([]);
        const loading = ref(true);
        const errorMessage = ref('');

        const fetchUsers = async () => {
            loading.value = true;
            errorMessage.value = '';
            try {
                const response = await apiCall('/api/users');
                users.value = response.users || [];
            } catch (err) {
                console.error(err);
                errorMessage.value = err.message || 'Failed to fetch users.';
            } finally {
                loading.value = false;
            }
        };

        onMounted(fetchUsers);

        return { users, loading, errorMessage };
    }
};

// -------------------------------------------------------------
// Component: UserDetailView
// -------------------------------------------------------------
const UserDetailView = {
    template: `
        <div class="container wide" v-if="userData">
            <h1>User Detail</h1>
            <p><strong>Name (UserID):</strong> {{ userData.user?.userName }}</p>
            <p><strong>Display Name:</strong> {{ userData.user?.displayName }}</p>

            <h2>Credentials</h2>
            <p v-if="errorMessage" class="error">{{ errorMessage }}</p>
            <ul v-if="userData.credentials && userData.credentials.length > 0" class="item-list">
                <li v-for="cred in userData.credentials" :key="cred.credentialId">
                    <div>
                        <strong>{{ cred.credentialName || 'Passkey' }}</strong>
                        <div style="font-size: 0.85em; color: #666; word-break: break-all;">ID: {{ cred.credentialId }}</div>
                    </div>
                    <button v-if="isCurrentUser" @click="deleteCredential(cred.credentialId)" class="delete-btn">
                        Delete
                    </button>
                </li>
            </ul>
            <p v-else>No credentials found.</p>

            <div v-if="isCurrentUser" class="action-row">
                <button @click="addCredential" class="primary" :disabled="loading" style="width: auto;">
                    {{ loading ? 'Adding...' : 'Add New Credential' }}
                </button>
            </div>

            <div class="links">
                <router-link to="/users">Back to User List</router-link>
            </div>
        </div>
        <div v-else class="container">
            <p v-if="errorMessage" class="error">{{ errorMessage }}</p>
            <p v-else>Loading user details...</p>
        </div>
    `,
    setup() {
        const route = useRoute();
        const userData = ref(null);
        const loading = ref(false);
        const errorMessage = ref('');

        const userId = computed(() => route.params.userId);
        const isCurrentUser = computed(() => store.user && store.user.id === userId.value);

        const fetchUser = async () => {
            errorMessage.value = '';
            try {
                const data = await apiCall(`/api/users/${userId.value}`);
                userData.value = data;
            } catch (err) {
                console.error(err);
                errorMessage.value = err.message || 'Failed to fetch user details.';
            }
        };

        const addCredential = async () => {
            loading.value = true;
            errorMessage.value = '';
            try {
                // 1. Get challenge from server
                const data = await apiCall(`/api/users/${userId.value}/credentials/start`, { method: 'POST' });
                const creationOptions = data.creationOptions;

                // 2. Call WebAuthn
                const binaryOptions = convertCreationOptionsToBinary(creationOptions);
                const credential = await navigator.credentials.create({ publicKey: binaryOptions });

                if (!credential) {
                    throw new Error('Credential creation failed or was canceled.');
                }

                // 3. Send result to server
                const jsonableCredential = convertPublicKeyCredentialToJsonable(credential);
                const transports = credential.response.getTransports ? credential.response.getTransports() : [];
                const resp = {
                    createResponse: {
                        attestationResponse: jsonableCredential,
                        transports: transports
                    }
                };

                await apiCall(`/api/users/${userId.value}/credentials/finish`, {
                    method: 'POST',
                    body: JSON.stringify(resp)
                });

                alert('Credential added successfully!');
                await fetchUser();
            } catch (err) {
                console.error(err);
                errorMessage.value = err.message || 'Failed to add new credential.';
            } finally {
                loading.value = false;
            }
        };

        const deleteCredential = async (credentialId) => {
            if (userData.value?.credentials?.length === 1) {
                alert('You cannot delete the last credential.');
                return;
            }
            if (!confirm('Are you sure you want to delete this credential?')) {
                return;
            }

            errorMessage.value = '';
            try {
                await apiCall(`/api/users/${userId.value}/credentials/${credentialId}`, {
                    method: 'DELETE'
                });
                await fetchUser();
            } catch (err) {
                console.error(err);
                errorMessage.value = err.message || 'Failed to delete credential.';
            }
        };

        onMounted(fetchUser);

        return { userData, loading, errorMessage, isCurrentUser, addCredential, deleteCredential };
    }
};

// -------------------------------------------------------------
// Router Setup
// -------------------------------------------------------------
const routes = [
    { path: '/', component: LoginView },
    { path: '/register', component: RegisterView },
    { path: '/users', component: UserListView },
    { path: '/users/:userId', component: UserDetailView },
    { path: '/:pathMatch(.*)*', redirect: '/' }
];

const router = createRouter({
    history: createWebHistory(),
    routes
});

// Route guard
router.beforeEach(async (to, from, next) => {
    // If not checked yet, check session
    if (store.user === null) {
        try {
            const res = await apiCall('/api/session');
            if (res.loggedIn) {
                store.user = res.user;
            }
        } catch (e) {
            store.user = false;
        }
    }

    const publicPages = ['/', '/register'];
    const authRequired = !publicPages.includes(to.path);

    if (authRequired && !store.user) {
        return next('/');
    }
    next();
});

// -------------------------------------------------------------
// Main App Root Component
// -------------------------------------------------------------
const App = {
    template: `
        <div id="app-container">
            <header>
                <div class="brand">YubiOn FSS Demo (.NET)</div>
                <nav v-if="isLoggedIn">
                    <span>Welcome, {{ store.user?.displayName || store.user?.name }}</span>
                    <button @click="logout">Logout</button>
                </nav>
            </header>
            <main>
                <router-view></router-view>
            </main>
        </div>
    `,
    setup() {
        const router = useRouter();
        const isLoggedIn = computed(() => !!store.user);

        const logout = async () => {
            try {
                await apiCall('/api/logout', { method: 'POST' });
                store.user = null;
                router.push('/');
            } catch (err) {
                console.error('Logout error', err);
            }
        };

        return { store, isLoggedIn, logout };
    }
};

const app = createApp(App);
app.use(router);
app.mount('#app');
