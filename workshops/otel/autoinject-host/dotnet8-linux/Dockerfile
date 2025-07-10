FROM mcr.microsoft.com/dotnet/aspnet:8.0

RUN mkdir /App
COPY bin/Debug/net8.0/ /App
COPY entrypoint.sh /App

# Set the entrypoint to the entrypoint.sh script
RUN chmod +x /App/entrypoint.sh
ENTRYPOINT ["/bin/sh", "-c", "/App/entrypoint.sh"]