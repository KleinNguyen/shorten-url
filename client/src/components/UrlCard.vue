<template>
    <div class="url-card">
        <div class="content">

            <div v-if="isEditing" class="edit-row">
                <div class="ui labeled input fluid">
                    <div class="ui label">{{ baseDomain }}</div>
                    <input type="text" v-model="editCode" />
                </div>
            </div>

            <div v-else class="header">
                <a :href="url.shortenUrl" target="_blank">{{ url.shortenUrl }}</a>
            </div>

            <div class="meta">Created: {{ new Date(url.dateTime).toLocaleString() }}</div>

            <div class="description">
                <p>
                    <strong>Original:</strong>
                    <a :href="url.originalUrl" target="_blank">{{ url.originalUrl }}</a>
                </p>
            </div>
        </div>

        <div class="extra content buttons-row">
            <div v-if="!isEditing" class="ui buttons small">
                <button class="ui teal button" @click="copyUrl">Copy</button>
                <button class="ui yellow button" @click="enableEdit">Edit</button>
                <button class="ui red button" @click="$emit('delete', url.id)">Delete</button>
            </div>

            <div v-else class="ui buttons small">
                <button class="ui button" @click="cancelEdit">Cancel</button>
                <button class="ui primary button" @click="saveEdit">Save</button>
            </div>
        </div>
    </div>
</template>

<script>
    import Swal from "sweetalert2";

    export default {
        name: "UrlCard",
        props: { url: Object },
        data() {
            return {
                isEditing: false,
                editCode: "",
            };
        },
        computed: {
            baseDomain() {
                const parts = this.url.shortenUrl.split("/");
                return parts.slice(0, -1).join("/") + "/";
            }
        },
        methods: {
            copyUrl() {
                navigator.clipboard.writeText(this.url.shortenUrl)
                    .then(() => {
                        Swal.fire({
                            icon: 'success',
                            title: 'Copied!',
                            text: 'URL copied to clipboard.',
                            timer: 1200,
                            showConfirmButton: false
                        });
                    })
                    .catch(err => {
                        console.error("Copy failed:", err);
                        Swal.fire({
                            icon: 'error',
                            title: 'Error!',
                            text: 'Failed to copy URL.',
                        });
                    });
            },
            enableEdit() {
                this.isEditing = true;
                this.editCode = this.url.shortenCode;
            },
            cancelEdit() {
                this.isEditing = false;
            },
            saveEdit() {
                if (!this.editCode.trim()) {
                    Swal.fire({
                        icon: 'warning',
                        title: 'Oops!',
                        text: 'Short code cannot be empty!',
                    });
                    return;
                }
                this.$emit("edit", {
                    id: this.url.id,
                    shortenCode: this.editCode.trim(),
                });
                this.isEditing = false;
            }
        }
    };
</script>


<style scoped>
    .url-card {
        margin-bottom: 1rem;
        background: rgba(255, 255, 255, 0.12);
        backdrop-filter: blur(10px);
        border-radius: 14px;
        border: 1px solid rgba(255, 255, 255, 0.2);
        padding: 1rem;
        color: #fff;
    }

    .header a,
    .description a {
        color: #ffd800;
    }

        .header a:hover,
        .description a:hover {
            color: #0015ff;
        }

    .edit-row {
        margin-bottom: .5rem;
    }

    .buttons-row {
        display: flex;
        justify-content: flex-end;
    }

    .header {
        margin-top: 1rem;
        font-size: 18px;
    }

    .meta {
        margin-bottom: 1rem;
    }

    .description {
        font-size: 16px;
    }
</style>
