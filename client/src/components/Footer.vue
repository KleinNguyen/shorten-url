<template>
    <footer class="app-footer">
        <div class="footer-container">
            <div class="footer-left">
                <p>
                    © 2025 <strong>MyURL</strong>. From Team <span class="highlight">Team 2</span> with 💙
                </p>
            </div>

            <div class="footer-right">
                <a href="#">Terms</a>
                <a href="#">Privacy</a>
                <a href="#">Contact</a>
            </div>
        </div>
    </footer>
</template>


<script>
    export default {
        name: "AppFooter",
    };
</script>

<style>
    .app-footer {
        width: 100%;
        background: rgba(27,28,29,0.95);
        backdrop-filter: blur(8px);
        -webkit-backdrop-filter: blur(8px);
        color: #f1f1f1;
        padding: 1.8rem 2rem;
        box-shadow: 0 -4px 12px rgba(0,0,0,0.25);
        font-family: 'Poppins', sans-serif;
        display: flex;
        justify-content: center;
    }

    .footer-container {
        width: 100%;
        max-width: 1200px;
        display: flex;
        flex-wrap: wrap;
        justify-content: space-between;
        align-items: center;
        gap: 1rem;
    }

    .footer-left p {
        margin: 0;
        font-size: 0.95rem;
        color: #f1f1f1;
    }

    .highlight {
        color: #00b5ad;
        font-weight: 600;
    }

    .footer-right {
        display: flex;
        gap: 1rem;
        flex-wrap: wrap;
    }

        .footer-right a {
            color: #f1f1f1;
            text-decoration: none;
            font-weight: 500;
            transition: color 0.2s, transform 0.2s;
        }

            .footer-right a:hover {
                color: #00b5ad;
                transform: translateY(-2px);
            }

</style>

