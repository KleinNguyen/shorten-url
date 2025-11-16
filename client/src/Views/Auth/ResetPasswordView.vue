<template>
    <div class="reset-container">
        <div class="ui raised padded segment reset-box">
            <h2 class="ui header center aligned">Reset Password</h2>

            <form class="ui form" @submit.prevent="handleReset">
                <div class="field">
                    <label>Email</label>
                    <input type="email" v-model="email" readonly />
                </div>

                <div class="field">
                    <label>New Password</label>
                    <input type="password" v-model="password" placeholder="Enter new password" required />
                </div>

                <div class="field">
                    <label>Confirm Password</label>
                    <input type="password" v-model="confirmPassword" placeholder="Confirm new password" required />
                </div>

                <button class="ui teal button fluid" type="submit">Reset Password</button>
            </form>

            <div v-if="successMessage" class="ui positive message" style="margin-top: 20px;">
                <p>{{ successMessage }}</p>
            </div>
            <div v-if="errorMessage" class="ui negative message" style="margin-top: 20px;">
                <p>{{ errorMessage }}</p>
            </div>
        </div>
    </div>
</template>

<script>
    import { resetPassword, verifyToken } from "@/helpers/auth";
    import Swal from "sweetalert2";

    export default {
        name: "ResetPasswordView",
        data() {
            return {
                email: "",
                token: "",
                password: "",
                confirmPassword: ""
            };
        },
        mounted() {
            this.email = this.$route.query.email || "";
            this.token = this.$route.query.token || "";

            if (this.token) {
                verifyToken(this.token)
                    .then(res => {
                        if (!res.success) {
                            Swal.fire({
                                icon: 'error',
                                title: 'Invalid Token',
                                text: res.message || 'Token is not valid.'
                            });
                        }
                    })
                    .catch(err => {
                        Swal.fire({
                            icon: 'error',
                            title: 'Error',
                            text: err.response?.data?.message || "Invalid token"
                        });
                    });
            }
        },
        methods: {
            async handleReset() {
                if (!this.email || !this.token) {
                    Swal.fire({
                        icon: 'error',
                        title: 'Invalid Link',
                        text: 'Invalid reset link.'
                    });
                    return;
                }

                if (this.password !== this.confirmPassword) {
                    Swal.fire({
                        icon: 'warning',
                        title: 'Oops!',
                        text: 'Passwords do not match!'
                    });
                    return;
                }

                if (this.password.length < 6) {
                    Swal.fire({
                        icon: 'warning',
                        title: 'Oops!',
                        text: 'Password must be at least 6 characters.'
                    });
                    return;
                }

                try {
                    await resetPassword({
                        email: this.email,
                        token: this.token,
                        newPassword: this.password,
                        confirmPassword: this.confirmPassword
                    });

                    Swal.fire({
                        icon: 'success',
                        title: 'Success!',
                        text: 'Password reset successfully!',
                        timer: 2000,
                        showConfirmButton: false
                    });

                    setTimeout(() => {
                        this.$router.push("/signin");
                    }, 1500);

                } catch (err) {
                    Swal.fire({
                        icon: 'error',
                        title: 'Error!',
                        text: err.response?.data || "Failed to reset password."
                    });
                }
            }
        }
    };
</script>


<style scoped>
    .reset-container {
        display: flex;
        justify-content: center;
        align-items: center;
        min-height: 75vh;
        background-color: #f0f0f0;
    }

    .reset-box {
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
