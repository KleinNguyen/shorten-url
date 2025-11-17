import api from "./api.js";


//register
export const register = async (data) => {
    const res = await api.post("/auth/register", data);
    return res.data
}

// login
export const login = async (data) => {
    const res = await api.post("/auth/login", data);
    const token = res.data.token;
    localStorage.setItem("token", token);
    api.defaults.headers.common["Authorization"] = `Bearer ${token}`;


    return res.data;
};

// send email
export const sendEmail = async (data) => {
    const res = await api.post("/auth/send-email", data);
    return res.data;
};

// verify
export const verifyCode = async (data) => {
    const res = await api.post("/auth/verify-code", data);
    return res.data;
};

export const verifyToken = async (token) => {
    const res = await api.get(`/auth/verify-token?token=${token}`);
    return res.data;
};

// Đặt lại mật khẩu
export const resetPassword = async (data) => {
    const res = await api.post("/auth/reset-password", data);
    return res.data;
};

export const updateAccount = async (data) => {
    const currentUser = JSON.parse(localStorage.getItem("currentUser"));
    if (!currentUser) throw new Error("User not logged in");

    const res = await api.put(`/auth/${ currentUser.id }`, data);
    return res.data;
};

// Deactivate account
export const deactivateAccount = async () => {
    const currentUser = JSON.parse(localStorage.getItem("currentUser"));
    if (!currentUser) throw new Error("User not logged in");

    const res = await api.delete(`/auth/${currentUser.id}`);
    return res.data;
};