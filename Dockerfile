FROM mcr.microsoft.com/dotnet/sdk:7.0
ADD ./ /root/src/
RUN ls /root/src
RUN dotnet dev-certs https -ep localhost.crt --format PEM
RUN cp localhost.crt /usr/local/share/ca-certificates
RUN update-ca-certificates
WORKDIR /root/src/
RUN dotnet build Magisterka.sln
WORKDIR Application
ENTRYPOINT dotnet run --project Application.csproj --launch-profile "Headless"