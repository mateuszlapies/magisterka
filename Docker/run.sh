#!/bin/bash
npm start &
cd ..
dotnet run --project Application.csproj --launch-profile "Electron"