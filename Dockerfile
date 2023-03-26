FROM mcr.microsoft.com/dotnet/sdk
ENV PATH="$PATH:/root/.dotnet/tools"
RUN wget https://packages.microsoft.com/config/debian/11/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb
RUN rm packages-microsoft-prod.deb
RUN apt update
RUN apt dist-upgrade -y
RUN apt install nodejs npm dotnet-runtime-6.0 -y
RUN dotnet tool install --global dotnet-certificate-tool
RUN dotnet dev-certs https -ep cert.pfx
RUN certificate-tool add --file cert.pfx
ADD ./ /root/src/
WORKDIR /root/src/
RUN mv Docker/.env.development.local Application/ClientApp/.env.development.local
RUN dotnet build Magisterka.sln
WORKDIR Application
ENTRYPOINT dotnet run --project Application.csproj --launch-profile "Application"