<template>
    <div class="signup-container">
        <div class="ui raised segment signup-box">
            <h2 class="ui header center aligned">Sign Up</h2>

            <form class="ui form" @submit.prevent="handleSubmit">
                <div class="field">
                    <label>Name</label>
                    <input type="text" placeholder="Your Name" v-model="name" required />
                </div>

                <div class="field">
                    <label>Email</label>
                    <input type="email" placeholder="Email" v-model="email" required />
                </div>

                <div class="field">
                    <label>Password</label>
                    <input type="password" placeholder="Password" v-model="password" required />
                </div>

                <button class="ui primary button" type="submit">Sign Up</button>
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
                    Already have an account?
                    <a href="/signin">Sign in here!</a>
                </p>
            </div>
        </div>
    </div>
</template>

<script>
    import { register } from "@/helpers/auth";
    import Swal from "sweetalert2";

    export default {
        name: 'SignupView',
        data() {
            return {
                name: '',
                email: '',
                password: ''
            };
        },
        methods: {
            async handleSubmit() {
                if (!this.name || !this.email || !this.password) {
                    Swal.fire({
                        icon: 'warning',
                        title: 'Oops!',
                        text: 'All fields are required!'
                    });
                    return;
                }

                try {
                    const payload = {
                        UserName: this.name, 
                        Email: this.email,
                        Password: this.password
                    };

                    const res = await register(payload);
                    console.log("Register response:", res);

                    Swal.fire({
                        icon: 'success',
                        title: 'Success!',
                        text: res.message || `Welcome, ${this.name}! Your account has been created.`,
                        timer: 2000,
                        showConfirmButton: false
                    });

                    
                    this.name = '';
                    this.email = '';
                    this.password = '';

                } catch (err) {
                    console.error("Register error:", err);

                    let message = "Signup failed!";
                    if (err.response) {
                        const status = err.response.status;
                        switch (status) {
                            case 400:
                                message = "Email already existed";
                                break;
                            default:
                                message = err.response.data?.message || message;
                        }
                    }

                    Swal.fire({
                        icon: 'error',
                        title: 'Error!',
                        text: message
                    });
                }
            }
        }
    };
</script>


<style scoped>
    .signup-container {
        display: flex;
        justify-content: center;
        align-items: center;
        min-height: 75vh;
        background-color: #f0f0f0;
    }

    .signup-box {
        width: 400px;
        padding: 30px;
        box-shadow: 0 4px 12px rgba(0,0,0,0.15);
        border-radius: 10px;
    }

    .ui.form .field {
        margin-bottom: 15px;
    }
</style>
