<template>
    <div class="signin-container">
        <div class="ui raised padded segment signin-box">
            <h2 class="ui header center aligned">Sign In</h2>

            <form class="ui form" @submit.prevent="handleSubmit">
                <div class="field">
                    <label>Email</label>
                    <input type="email" placeholder="Enter your email" v-model="email" required />
                </div>

                <div class="field">
                    <label>Password</label>
                    <input type="password" placeholder="Enter your password" v-model="password" required />
                </div>

                <button class="ui primary button" type="submit">Sign In</button>
                <p>
                    <a href="/forgot-password">Forgot Password?</a>
                </p>
            </form>

            <div v-if="successMessage" class="ui positive message" style="margin-top: 20px;">
                <p>{{ successMessage }}</p>
            </div>

            <div v-if="errorMessage" class="ui negative message" style="margin-top: 20px;">
                <p>{{ errorMessage }}</p>
            </div>

            <div class="ui divider"></div>
            <div class="center aligned">
                <p>
                    Don't have an account?
                    <a href="/signup">Sign up here!</a>
                </p>
            </div>
        </div>
    </div>
</template>

<script>
    import { login, getCurrentUser } from "@/helpers/auth"; 
    import api from "@/helpers/api"; 
    import Swal from "sweetalert2";

    export default {
        name: "SigninView",
        data() {
            return {
                email: "",
                password: ""
            };
        },
        methods: {
            async handleSubmit() {
                if (!this.email || !this.password) {
                    Swal.fire({
                        icon: 'warning',
                        title: 'Oops!',
                        text: 'Both email and password are required!'
                    });
                    return;
                }

                try {
                    const payload = {
                        email: this.email,
                        password: this.password,
                    };

                    const res = await login(payload);
                    console.log("Login response:", res);

                    if (res.token) {

                        localStorage.setItem("token", res.token);
                        api.defaults.headers.common["Authorization"] = `Bearer ${res.token}`;

                        let user;

                        if (res.user) {
                            user = {
                                id: res.user.id,
                                name: res.user.userName,
                                email: res.user.email,
                            };
                        } else {
                            user = { email: this.email, name: this.email };
                        }

                        localStorage.setItem("currentUser", JSON.stringify(user));
                        window.dispatchEvent(new Event("storage"));

                        Swal.fire({
                            icon: 'success',
                            title: 'Welcome!',
                            text: `Welcome back, ${user.name || user.email}!`,
                            timer: 1800,
                            showConfirmButton: false
                        });

                        this.$router.push("/");

                    } else {
                        Swal.fire({
                            icon: 'error',
                            title: 'Login Failed',
                            text: "Invalid server response"
                        });
                    }

                } catch (err) {
                    console.error("Login error:", err);

                    let message = "Login failed!";
                    if (err.response) {
                        const status = err.response.status;
                        switch (status) {
                            case 401:
                                message = "Password is incorrect";
                                break;
                            case 404:
                                message = "Email is not existed";
                                break;
                            case 500:
                                message = "Server error. Please try again later.";
                                break;
                            default:
                                message = err.response.data?.message || message;
                        }
                    }

                    Swal.fire({
                        icon: 'error',
                        title: 'Login Failed',
                        text: message
                    });
                }
            },
        },
    };
</script>



<style scoped>
    .signin-container {
        display: flex;
        justify-content: center;
        align-items: center;
        min-height: 75vh;
        background-color: #f0f0f0;
    }

    .signin-box {
        width: 400px;
        padding: 30px;
        border-radius: 10px;
        box-shadow: 0 6px 15px rgba(0,0,0,0.1);
        background-color: #fff;
    }

    .ui.form .field {
        margin-bottom: 15px;
    }
</style>
