FROM node
WORKDIR /frontend
EXPOSE 80
COPY . .
RUN yarn global add serve
ENTRYPOINT serve -p 80 -s ./build
