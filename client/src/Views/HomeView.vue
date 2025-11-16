<template>
    <div class="home-container">
        <div class="shorten-box">
            <h2 class="ui header center aligned">Shorten Your URL</h2>

            <label for="original-url" class="mr-4">Shorten a long URL:</label>
            <div class="ui action input fluid mb-3">
                <input v-model="originalUrl" type="text" placeholder="Enter your long link here..." />
                <button class="ui teal button" @click="shortenUrl">
                    <i class="linkify icon"></i> Shorten
                </button>
            </div>

            <label for="custom-alias" class="mr-4">Customize your link:</label>
            <div class="ui fluid input mb-3">
                <input v-model="customAlias" type="text" placeholder="Enter alias (optional)" />
            </div>

            <div v-if="shortenedUrl" class="ui segment result-box">
                <h5 class="ui header">Your shortened URL:</h5>
                <a :href="shortenedUrl" target="_blank" rel="noopener noreferrer" class="ui teal text">
                    {{ shortenedUrl }}
                </a>
            </div>

            <label class="mb-4">ToolBox:</label>
            <div class="ui four buttons mt-3">
                <button class="ui olive button" @click="copyUrl">
                    <i class="copy outline icon"></i> Copy
                </button>
                <button class="ui green button" @click="resetForm">
                    <i class="redo icon"></i> Shorten another
                </button>
            </div>
        </div>
    </div>
</template>

<script>
    import { createShortUrl } from "@/helpers/shorten";
    import Swal from "sweetalert2";

    export default {
        name: "HomeView",
        data() {
            return {
                originalUrl: "",
                shortenedUrl: "",
                customAlias: "",
            };
        },
        methods: {
            async shortenUrl() {
                if (!this.originalUrl.trim()) {
                    Swal.fire({
                        icon: 'warning',
                        title: 'Arlert!',
                        text: 'Please enter a URL first!',
                    });
                    return;
                }

                try {
                    const payload = {
                        OriginalUrl: this.originalUrl.trim(),
                        ShortenCode: this.customAlias.trim() || null,
                    };

                    const res = await createShortUrl(payload);
                    this.shortenedUrl = res.link || "";

                    Swal.fire({
                        icon: 'success',
                        title: 'Success!',
                        text: 'Your URL has been shortened!',
                        timer: 1500,
                        showConfirmButton: false,
                    });
                } catch (err) {
                    const errorMessage = err.response?.data?.message || err.message || "Failed to shorten URL";
                    Swal.fire({
                        icon: 'error',
                        title: 'Oops!',
                        text: errorMessage,
                    });
                }
            },
            copyUrl() {
                if (this.shortenedUrl) {
                    navigator.clipboard.writeText(this.shortenedUrl);
                    Swal.fire({
                        icon: 'success',
                        title: 'Copied!',
                        text: 'Copied to clipboard!',
                        timer: 1200,
                        showConfirmButton: false,
                    });
                }
            },
            resetForm() {
                this.originalUrl = "";
                this.shortenedUrl = "";
                this.customAlias = "";
            },
        },
    };
</script>


<style scoped>
    .home-container {
        min-height: 100vh;
        display: flex;
        justify-content: center;
        align-items: flex-start;
        padding: 100px 0;
        background: transparent;
        background-attachment: fixed;
    }

    .ui.header.center.aligned {
        color: white !important;
        font-size: 35px;
    }

    .shorten-box {
        width: 100%;
        max-width: 650px;
        background: rgba(255, 255, 255, 0.15);
        backdrop-filter: blur(12px);
        -webkit-backdrop-filter: blur(12px);
        border-radius: 14px;
        box-shadow: 0 8px 22px rgba(0, 0, 0, 0.08);
        padding: 2.5rem !important;
        border: 1px solid rgba(255, 255, 255, 0.2);
        color: #ffffff;
    }

    .result-box {
        text-align: center;
        background: rgba(255, 255, 255, 0.2);
        border-radius: 8px;
        padding: 1rem;
        border: 1px solid rgba(255, 255, 255, 0.3);
        color: #000;
    }

    .mt-3 {
        margin-top: 1rem;
    }

    .mb-3 {
        margin-bottom: 1rem;
    }

    .mr-4 {
        font-size: 16px;
    }

    .mb-4 {
        margin-bottom: 1.5rem;
        font-size: 16px;
    }
</style>
