<template>
    <header class="app-header">
        <div class="logo">
            <i class="linkify icon"></i>
            <span>Shortener URL</span>
        </div>

        <nav class="nav-links">
            <router-link to="/home" class="nav-item">
                <i class="home icon"></i> Home
            </router-link>
            <router-link to="/dashboard" class="nav-item">
                <i class="paperclip icon"></i>My URLs
            </router-link>
        </nav>

        <div class="user-section-btn">
            <div v-if="currentUser" class="dropdown">
                <button class="dropbtn">{{ currentUser.name }} <i class="dropdown icon"></i></button>
                <div class="dropdown-content">
                    <router-link to="/personal">Personal</router-link>
                    <a @click="logout">Logout</a>
                </div>
            </div>
            <router-link v-else to="/signin" class="signin-btn">
                <i class="user icon"></i> Account
            </router-link>
        </div>
    </header>
</template>


<script>
    export default {
        data() {
            return {
                currentUser: null
            };
        },
        mounted() {
            window.addEventListener("storage", this.loadUser);
            this.loadUser();
        },
        beforeUnmount() {
            window.removeEventListener("storage", this.loadUser);
        },
        methods: {
            loadUser() {
                const storedUser = localStorage.getItem('currentUser');

                // Kiểm tra xem storedUser có tồn tại và không phải "undefined"
                if (storedUser && storedUser !== "undefined") {
                    try {
                        this.currentUser = JSON.parse(storedUser);
                    } catch (err) {
                        console.error("Lỗi parse storedUser:", err);
                        this.currentUser = null;
                    }
                } else {
                    // Chưa đăng nhập → set currentUser = null
                    this.currentUser = null;
                }
            },
            logout() {
                localStorage.removeItem('currentUser');
                localStorage.removeItem('token');
                this.currentUser = null;
                window.dispatchEvent(new Event("storage"));
                this.$router.push('/home');
            }
        }
    };
</script>


<style scoped>
    .app-header {
        position: fixed;
        top: 0;
        width: 100%;
        height: 70px;
        display: flex;
        justify-content: space-between;
        align-items: center;
        padding: 0 2rem;
        backdrop-filter: blur(12px);
        -webkit-backdrop-filter: blur(12px);
        background: rgba(255, 255, 255, 0.15);
        border-bottom: 1px solid rgba(255,255,255,0.2);
        z-index: 1000;
        font-family: 'Poppins', sans-serif;
    }

    .logo {
        display: flex;
        align-items: center;
        font-weight: 600;
        font-size: 1.7rem;
        gap: 0.5rem;
        color: #fff;
    }

    .nav-links {
        display: flex; 
        gap: 1.5rem;
    }

    .nav-item {
        text-decoration: none;
        color: #fff;
        font-weight: 500;
        font-size: 1.3rem;
        transition: color 0.2s;
    }

        .nav-item:hover {
            color: #4f46e5;
        }

    .user-section-btn {
        padding: 0.6rem 1rem;
        border-radius: 6px;
        background-color: #4f46e5;
        color: #fff;
        font-weight: 500;
        text-decoration: none;
        transition: background 0.2s;
    }
        .user-section-btn:hover {
            background-color: #4338ca;
        }

        .dropdown {
            position: relative;
            display: inline-block;
        }

    .dropbtn {
        background: transparent;
        border: none;
        font-weight: 500;
        font-size: 1.1rem;
        cursor: pointer;
        color: #fff;
        display: flex;
        align-items: center;
        gap: 0.3rem;
    }

    .dropdown-content {
        display: none;
        position: absolute;
        right: 0;
        background-color: rgba(255,255,255,0.95);
        min-width: 120px;
        box-shadow: 0 4px 12px rgba(0,0,0,0.15);
        border-radius: 8px;
        overflow: hidden;
        z-index: 1000;
    }

        .dropdown-content a,
        .dropdown-content router-link {
            color: #111;
            padding: 0.5rem 1rem;
            text-decoration: none;
            display: block;
            transition: background 0.2s;
        }

            .dropdown-content a:hover,
            .dropdown-content router-link:hover {
                background-color: rgba(79,70,229,0.1);
            }

    .dropdown:hover .dropdown-content {
        display: block;
    }

    .signin-btn {
        padding: 0.6rem 1rem;
        border-radius: 6px;
        background-color: #4f46e5;
        color: #fff;
        font-weight: 500;
        text-decoration: none;
        transition: background 0.2s;
    }

        .signin-btn:hover {
            background-color: #4338ca;
        }
</style>