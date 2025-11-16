# ================= Stage 1: Build Vue App =================
FROM node:18 AS build

# Thư mục làm việc
WORKDIR /app

# Copy package.json + package-lock.json để cài deps
COPY package*.json ./

# Cài dependencies
RUN npm install

# Copy toàn bộ source code
COPY . .

# ================= Build-time environment =================
# Biến này sẽ được embed vào JS bundle khi build
# Vue CLI chỉ nhận biến bắt đầu bằng VUE_APP_
ARG VUE_APP_IS_CONTAINER=true
ENV VUE_APP_IS_CONTAINER=$VUE_APP_IS_CONTAINER

# Build Vue app
RUN npm run build

# ================= Stage 2: Serve với Nginx =================
FROM nginx:alpine

# Copy folder dist từ stage build sang Nginx
COPY --from=build /app/dist /usr/share/nginx/html

# Copy file config Nginx nếu có tuỳ chỉnh routes
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Expose port 80
EXPOSE 80

# Chạy Nginx foreground
CMD ["nginx", "-g", "daemon off;"]
