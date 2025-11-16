
import api from "./api.js";

// Lấy toàn bộ link
export const getAllLinks = async () => {
    const res = await api.get("/crud");
    return res.data;
};

// Cập nhật link
export const updateLink = async (id, newCode) => {
    const res = await api.put(`/crud/${id}`, JSON.stringify(newCode), {
        headers: { "Content-Type": "application/json" } // quan trọng
    });
    return res.data;
};

// Xoá link
export const deleteLink = async (id) => {
    const res = await api.delete(`/crud/${id}`);
    return res.data;
};
