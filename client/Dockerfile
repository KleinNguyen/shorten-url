FROM node:18 AS build

WORKDIR /app

COPY client/package*.json ./

RUN npm install

COPY client/ .

ARG VUE_APP_IS_CONTAINER=true

ENV VUE_APP_IS_CONTAINER=$VUE_APP_IS_CONTAINER

RUN npm run build

FROM nginx:alpine

COPY --from=build /app/dist /usr/share/nginx/html

COPY client/nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
