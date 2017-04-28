FROM microsoft/dotnet
WORKDIR /app
ENV ASPNETCORE_URLS http://+:50547
ENV ASPNETCORE_ENVIRONMENT Production
EXPOSE 50547
COPY . .
RUN dotnet restore ./FacebookPostsScraper.sln
WORKDIR ./FacebookPostsScraper
ENTRYPOINT ["dotnet", "run"]
