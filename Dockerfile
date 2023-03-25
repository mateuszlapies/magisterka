FROM mcr.microsoft.com/dotnet/sdk:7.0
RUN apt update
RUN apt install nodejs npm -y
ADD ./ /root/src/
RUN ls /root/src
RUN dotnet dev-certs https -ep localhost.crt --format PEM
RUN cp localhost.crt /usr/local/share/ca-certificates
RUN update-ca-certificates
WORKDIR /root/src/
RUN chmod u+x Docker/run.sh
RUN mv Docker/run.sh Application/ClientApp/run.sh
RUN mv Docker/.env.development.local Application/ClientApp/.env.development.local
RUN dotnet build Magisterka.sln
WORKDIR Application/ClientApp/
ENTRYPOINT bash run.sh