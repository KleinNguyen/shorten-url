import api from "./api.js";

export const createShortUrl = async (data) => {
    const res = await api.post("/shorten", data);
    return res.data;
};