import axios from "axios";

const hostname = window.location.hostname;

const isLocal =
    hostname === "localhost" || hostname === "127.0.0.1";

const isRender = hostname.includes("onrender.com");

let apiBase;

// LOCAL DEVELOPMENT
if (isLocal) {
    apiBase = "http://localhost:8888/gateway";
}
// RENDER PRODUCTION
else if (isRender) {
    apiBase = import.meta.env.VITE_API_URL;
}
// DOCKER COMPOSE
else {
    apiBase = "http://gateway:8080/gateway";
}

console.log("Environment:",
    isLocal ? "Local" :
    isRender ? "Render" : "Docker"
);

console.log("Axios baseURL:", apiBase);

const api = axios.create({
    baseURL: apiBase,
    headers: { "Content-Type": "application/json" }
});

const token = localStorage.getItem("token");
if (token) {
    api.defaults.headers.common["Authorization"] = `Bearer ${token}`;
}

console.log("RENDER ENV:", import.meta.env);
console.log("VITE_API_URL:", import.meta.env.VITE_API_URL);


export default api;
