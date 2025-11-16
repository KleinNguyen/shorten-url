import axios from "axios";

// runtime check host
const isHost = window.location.hostname === "localhost" || window.location.hostname === "127.0.0.1";

// baseURL logic
const apiBase = isHost
    ? "http://localhost:8888/gateway" // host browser → mapped port Ocelot
    : "http://gateway:8080/gateway";  // container → Docker network

console.log(`Running in ${isHost ? "host browser" : "Docker container"}`);
console.log("Axios baseURL:", apiBase);

// Tạo instance axios
const api = axios.create({
    baseURL: apiBase,
    headers: {
        'Content-Type': 'application/json',
    },
});

// Gắn token nếu có
const token = localStorage.getItem('token');
if (token) {
    api.defaults.headers.common['Authorization'] = `Bearer ${token}`;
}

export default api;
