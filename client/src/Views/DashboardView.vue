<template>
    <div class="dashboard">
        <div class="dashboard-container">
            <h1 class="dashboard-header">
                <span class="title">Dashboard</span>
                <span class="sub-title">Manage your shortened URLs</span>
            </h1>

            <div class="urls-scroll-box">
                <UrlList :urls="urls"
                            @delete-url="handleDeleteUrl"
                            @edit-url="handleEditUrl" />
            </div>

        </div>
    </div>
</template>


<script>
    import UrlList from "../components/UrlList.vue";
    import { getAllLinks, updateLink, deleteLink } from "@/helpers/crud";
    import Swal from "sweetalert2";

    export default {
        name: "DashboardView",
        components: { UrlList },
        data() {
            return {
                urls: [],
            };
        },
        async mounted() {
            await this.loadUrls();
            window.addEventListener("storage", this.handleUserChange);
        },
        beforeUnmount() {
            window.removeEventListener("storage", this.handleUserChange);
        },
        methods: {
            async loadUrls() {
                const currentUser = localStorage.getItem("currentUser");

                if (!currentUser || currentUser === "undefined") {
                    this.urls = [];
                    Swal.fire({
                        icon: "warning",
                        title: "Oops!",
                        text: "You need to login to see your URL history!",
                    });
                    return;
                }

                try {
                    const fetchedUrls = await getAllLinks();
                    this.urls = fetchedUrls;
                } catch (err) {
                    console.error("Error loading URLs:", err);
                    this.urls = [];
                    Swal.fire({
                        icon: "warning",
                        title: "Oops!",
                        text: "Failed to load your URLs!",
                    });
                }
            },

            handleUserChange() {
                const user = localStorage.getItem("currentUser");
                if (!user || user === "undefined") {
                    this.urls = [];
                } else {
                    this.loadUrls();
                }
            },

            async handleDeleteUrl(id) {
                const result = await Swal.fire({
                    title: "Are you sure?",
                    text: "URL will be deleted forever!",
                    icon: "warning",
                    showCancelButton: true,
                    confirmButtonColor: "#d33",
                    cancelButtonColor: "#3085d6",
                    confirmButtonText: "Delete",
                    cancelButtonText: "Cancel",
                });

                if (!result.isConfirmed) return;

                try {
                    await deleteLink(id);
                    this.urls = this.urls.filter((u) => u.id !== id);

                    Swal.fire({
                        icon: "success",
                        title: "Deleted!",
                        text: "URL deleted successfully.",
                        timer: 1500,
                        showConfirmButton: false,
                    });
                } catch (err) {
                    console.error("Error deleting URL:", err);
                    Swal.fire({
                        icon: "error",
                        title: "Error!",
                        text: "Delete URL failed.",
                    });
                }
            },

            async handleEditUrl(data) {
                try {
                    const updated = await updateLink(data.id, data.shortenCode);

                    const idx = this.urls.findIndex((u) => u.id === data.id);
                    if (idx !== -1) {
                        this.urls[idx].shortenCode = updated.shortenCode;
                        this.urls[idx].shortenUrl = updated.shortenUrl;
                    }

                    Swal.fire({
                        icon: "success",
                        title: "Updated!",
                        text: "Short code updated successfully.",
                        timer: 1500,
                        showConfirmButton: false,
                    });
                } catch (err) {
                    console.error("Error updating URL:", err);

                    const message =
                        err.response?.data?.message ||
                        "Failed to update short code. Check your input.";

                    Swal.fire({
                        icon: "error",
                        title: "Error!",
                        text: message,
                    });
                }
            },
        },
    };
</script>

<style scoped>
    .dashboard {
        padding: 2.5rem 1rem;
        min-height: 80vh;
        background: transparent;
    }

    .dashboard-container {
        max-width: 900px;
        margin: 0 auto;
    }

    .dashboard-header {
        display: flex;
        flex-direction: column;
        align-items: flex-start;
        gap: 0.3rem;
        font-family: "Poppins", sans-serif;
        margin-bottom: 2rem;
        color: #fff;
    }

        .dashboard-header .title {
            font-size: 2rem;
            font-weight: 600;
            display: flex;
            align-items: center;
            gap: 0.5rem;
        }

        .dashboard-header .sub-title {
            font-size: 1rem;
            font-weight: 400;
            color: #ffd800;
        }

    .urls-scroll-box {
        max-height: 60vh;
        overflow-y: auto;
        padding-right: 8px;
        background: rgba(255, 255, 255, 0.07);
        backdrop-filter: blur(6px);
        border-radius: 14px;
        border: 1px solid rgba(255, 255, 255, 0.2);
        padding: 1rem;
    }

        .urls-scroll-box::-webkit-scrollbar {
            width: 6px;
        }

        .urls-scroll-box::-webkit-scrollbar-thumb {
            background: rgba(255, 255, 255, 0.3);
            border-radius: 10px;
        }

    .no-urls {
        text-align: center;
        color: #fff;
        font-style: italic;
        margin-top: 2rem;
    }
</style>