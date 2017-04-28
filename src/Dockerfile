FROM node
WORKDIR /frontend
EXPOSE 80
COPY . .
RUN npm install -g serve
ENTRYPOINT serve -p 80 -s ./frontend/build
