<template>
    <div class="personal-container">
        <div class="profile-box">
            <h3 class="ui header">Profile Management</h3>
            <h2 class="ui dividing header">Update Information</h2>

            <div class="ui form">
                <div class="field">
                    <label>Name</label>
                    <input type="text" v-model="form.name" />
                </div>

                <div class="field">
                    <label>Email Address</label>
                    <input type="email" v-model="form.email" />
                </div>

                <button class="ui primary button" @click="updateAccount">
                    Update
                </button>
            </div>

            <div class="ui divider"></div>
            <button class="ui red basic button right floated" @click="deactivateAccount">
                Deactivate Account
            </button>
        </div>
    </div>
</template>

<script>
    import { updateAccount, deactivateAccount } from "@/helpers/auth";
    import Swal from "sweetalert2";

    export default {
        data() {
            return {
                currentUser: {},
                form: { name: "", email: "" },
            };
        },
        mounted() {
            const storedUser = localStorage.getItem("currentUser");
            if (storedUser) {
                this.currentUser = JSON.parse(storedUser);
                this.form.name = this.currentUser.name;
                this.form.email = this.currentUser.email;
            }
        },
        methods: {
            async updateAccount() {
                try {
                    const res = await updateAccount({
                        UserName: this.form.name,
                        Email: this.form.email
                    });

                    Swal.fire({
                        icon: 'success',
                        title: 'Updated!',
                        text: res.Message || "Account updated successfully!",
                        timer: 2000,
                        showConfirmButton: false
                    });

                  
                    this.currentUser.name = res.UserName || this.form.name;
                    this.currentUser.email = res.Email || this.form.email;
                    localStorage.setItem("currentUser", JSON.stringify(this.currentUser));
                    window.dispatchEvent(new Event("storage"));

                } catch (err) {
                    console.error(err);
                    Swal.fire({
                        icon: 'error',
                        title: 'Error!',
                        text: err.response?.data || "Update failed!",
                    });
                }
            },
            async deactivateAccount() {
                const result = await Swal.fire({
                    title: 'Are you sure?',
                    text: "Your account will be deactivated permanently!",
                    icon: 'warning',
                    showCancelButton: true,
                    confirmButtonColor: '#d33',
                    cancelButtonColor: '#3085d6',
                    confirmButtonText: 'Yes, deactivate!',
                    cancelButtonText: 'Cancel'
                });

                if (!result.isConfirmed) return;

                try {
                    const res = await deactivateAccount();

                    Swal.fire({
                        icon: 'success',
                        title: 'Deleted!',
                        text: res || "Account deactivated successfully.",
                        timer: 2000,
                        showConfirmButton: false
                    });

                    localStorage.removeItem("token");
                    localStorage.removeItem("currentUser");
                    window.dispatchEvent(new Event("storage"));
                    this.$router.push("/signin");

                } catch (err) {
                    console.error(err);
                    Swal.fire({
                        icon: 'error',
                        title: 'Error!',
                        text: err.response?.data || "Failed to deactivate account!",
                    });
                }
            },
        },
    };
</script>


<style scoped>
    .personal-container {
        min-height: 100vh;
        display: flex;
        justify-content: center;
        align-items: flex-start;
        padding: 100px 0;
        background: transparent;
        background-attachment: fixed;
    }

    .profile-box {
        width: 100%;
        max-width: 700px;
        background: rgba(255, 255, 255, 0.15);
        backdrop-filter: blur(12px);
        -webkit-backdrop-filter: blur(12px);
        border-radius: 14px;
        box-shadow: 0 8px 22px rgba(0, 0, 0, 0.08);
        padding: 2.5rem !important;
        border: 1px solid rgba(255, 255, 255, 0.2);
        color: #ffffff;
    }

        .profile-box .ui.header {
            color: white !important;
            font-size: 30px;
        }

        .profile-box .ui.dividing.header {
            color: white !important;
            font-size: 18px;
        }

        .profile-box .field label {
            color: #fff !important;
            font-size: 18px;
            font-weight: 500;
        }


    .ui.form input {
        background: rgba(255, 255, 255, 0.2) !important;
        color: #fff !important;
    }

        .ui.form input::placeholder {
            color: rgba(255, 255, 255, 0.7);
        }

    .field {
        color: #fff !important;
        font-size: 20px;
        margin-bottom: 1.5rem;
    }
</style>
