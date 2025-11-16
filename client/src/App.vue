<template>
    <div id="app" class="layout">
        <AppHeader />
        <main class="content">
            <router-view />
        </main>
        <Footer />
    </div>
</template>

<script>
    import AppHeader from './components/AppHeader.vue'
    import Footer from './components/Footer.vue'

    export default {
        components: {
            AppHeader,
            Footer
        }
    }
</script>

<style scoped>
    .layout {
        display: flex;
        flex-direction: column;
        height: 100vh;
        overflow: hidden;
    }

    .content {
        flex: 1;
        overflow-y: hidden;
        padding: 70px 0 80px 0;
        background-image: url('@/assets/bg.jpg');
        background-size: cover;
        background-position: center;
        background-attachment: fixed;

    }

    html, body {
        margin: 0;
        padding: 0;
        height: 100%;
    }

    body {
        -webkit-font-smoothing: antialiased;
        font-family: 'Segoe UI', sans-serif;
    }
</style>

