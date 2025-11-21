
import api from "./api.js";

export const getAllLinks = async () => {
    const res = await api.get("/crud");
    return res.data;
};

export const updateLink = async (id, newCode) => {
    const res = await api.put(`/crud/${id}`, JSON.stringify(newCode), {
        headers: { "Content-Type": "application/json" } 
    });
    return res.data;
};

export const deleteLink = async (id) => {
    const res = await api.delete(`/crud/${id}`);
    return res.data;
};
