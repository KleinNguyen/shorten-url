<template>
    <div class="url-list-container">
        <h3 class="ui header">Your Shortened URLs</h3>

        <div v-if="urls.length === 0" class="ui message">
            You haven't created any links yet.
        </div>

        <div v-else class="ui divided items">
            <UrlCard v-for="url in urls"
                     :key="url.id"
                     :url="url"
                     @delete="deleteUrl"
                     @edit="editUrl" />
        </div>
    </div>
</template>

<script>
    import UrlCard from "./UrlCard.vue";

    export default {
        name: "UrlList",
        components: { UrlCard },
        props: { urls: Array },
        emits: ["delete-url", "edit-url"],
        methods: {
            editUrl(data) { this.$emit("edit-url", data); },
            deleteUrl(id) { this.$emit("delete-url", id); },
        },
    };
</script>

<style scoped>
    .url-list-container {
        padding: 2rem;
        border-radius: 14px;
        background: rgba(255, 255, 255, 0.15);
        backdrop-filter: blur(12px);
        -webkit-backdrop-filter: blur(12px);
        border: 1px solid rgba(255, 255, 255, 0.2);
        box-shadow: 0 8px 22px rgba(0, 0, 0, 0.08);
        color: #ffffff;
    }

        .url-list-container .ui.header {
            color: #fff !important;
            margin-bottom: 1rem;
        }
</style>
